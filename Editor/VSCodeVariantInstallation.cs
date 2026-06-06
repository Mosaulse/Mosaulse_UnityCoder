/*---------------------------------------------------------------------------------------------
 *  Copyright (c) UnityCoder Team. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SimpleJSON;
using UnityEngine;
using IOPath = System.IO.Path;
using UnityDebug = UnityEngine.Debug; // 明确别名避免冲突

namespace UnityCoder.Editor.Integration
{
    /// <summary>
    /// VSCode及其变体编辑器安装类
    /// 支持Visual Studio Code以及Cursor、Antigravity、Trae、Kiro、Qoder等变体
    /// </summary>
    internal class VSCodeVariantInstallation : CodeEditorInstallation
    {
        private static readonly IGenerator _generator = GeneratorFactory.GetInstance(GeneratorStyle.SDK);

        // EditorType 枚举已不再需要，因为现在使用 VSCodeVariantConfig 来管理编辑器类型

        /// <summary>
        /// 编辑器配置信息
        /// </summary>
        public VSCodeVariantConfig Config { get; set; }

        /// <summary>
        /// 是否为预发布版本
        /// </summary>
        public bool IsPrerelease { get; set; }

        public override bool SupportsAnalyzers => true;

        public override Version LatestLanguageVersionSupported => new Version(13, 0);

        public override IGenerator ProjectGenerator => _generator;

        // IsCandidateForDiscovery 方法已移动到 VSCodeVariantRegistry 类中，不再需要

        /// <summary>
        /// 尝试发现编辑器安装
        /// </summary>
        /// <param name="editorPath">编辑器路径</param>
        /// <param name="installation">输出的安装对象</param>
        /// <returns>是否发现成功</returns>
        public static bool TryDiscoverInstallation(string editorPath, out ICodeEditorInstallation installation)
        {
            installation = null;

            if (string.IsNullOrEmpty(editorPath))
                return false;

            // 初始化注册表
            VSCodeVariantRegistry.InitializeDefaultVariants();

            // 使用注册表检查是否为候选路径
            if (!VSCodeVariantRegistry.IsCandidateForDiscovery(editorPath))
                return false;

            // 检测编辑器变体
            var variantConfig = VSCodeVariantRegistry.DetectVariantByPath(editorPath);
            if (variantConfig == null)
            {
                // UnityDebug.Log($"未识别的编辑器变体: {editorPath}");
                return false;
            }

            Version version = null;
            var isPrerelease = false;

            try
            {
                var manifestBase = GetRealPath(editorPath);

#if UNITY_EDITOR_WIN
                // on Windows, editorPath is a file, resources as subdirectory
                manifestBase = IOPath.GetDirectoryName(manifestBase);
#elif UNITY_EDITOR_OSX
                // on Mac, editorPath is a directory
                manifestBase = IOPath.Combine(manifestBase, "Contents");
#else
                // on Linux, editorPath is a file, in a bin sub-directory
                var parent = Directory.GetParent(manifestBase);
                // but we can link to [vscode]/code or [vscode]/bin/code
                manifestBase = parent?.Name == "bin" ? parent.Parent?.FullName : parent?.FullName;
#endif

                if (manifestBase == null)
                    return false;

                // 尝试直接路径
                var manifestFullPath = IOPath.Combine(manifestBase, "resources", "app", "package.json");

                // 如果直接路径不存在，尝试查找包含resources目录的子目录
                if (!File.Exists(manifestFullPath))
                {
                    var resourcesDirs = Directory.GetDirectories(manifestBase, "resources", SearchOption.AllDirectories);
                    foreach (var resourcesDir in resourcesDirs)
                    {
                        var candidatePath = IOPath.Combine(resourcesDir, "app", "package.json");
                        if (File.Exists(candidatePath))
                        {
                            manifestFullPath = candidatePath;
                            break;
                        }
                    }
                }

                if (File.Exists(manifestFullPath))
                {
                    var manifest = JsonUtility.FromJson<VSCodeManifest>(File.ReadAllText(manifestFullPath));

                    // 使用自定义版本检测器或默认检测器
                    if (variantConfig.CustomVersionDetector != null)
                    {
                        version = variantConfig.CustomVersionDetector(manifest.version);
                    }
                    else
                    {
                        Version.TryParse(manifest.version.Split('-').First(), out version);
                    }

                    isPrerelease = manifest.version.ToLower().Contains("insider") ||
                                 manifest.version.ToLower().Contains("preview") ||
                                 variantConfig.PrereleaseKeywords.Any(kw =>
                                     manifest.version.ToLower().Contains(kw));
                }
            }
            catch (Exception ex)
            {
                // UnityDebug.Log($"无法获取编辑器版本信息: {ex.Message}");
                // 不因版本检测失败而阻止发现
            }

            // 检查路径中是否包含预发布标识
            var lowerPath = editorPath.ToLower();
            isPrerelease = isPrerelease || variantConfig.PrereleaseKeywords.Any(kw => lowerPath.Contains(kw));

            installation = new VSCodeVariantInstallation()
            {
                Config = variantConfig,
                IsPrerelease = isPrerelease,
                Name = $"{variantConfig.DisplayName}{(version != null ? $" [{version.ToString(3)}]" : string.Empty)}{(isPrerelease ? " (Preview)" : "")}",
                Path = editorPath,
                Version = version ?? new Version()
            };

            return true;
        }

        /// <summary>
        /// 获取所有已安装的VSCode变体
        /// </summary>
        /// <returns>编辑器安装集合</returns>
        public static IEnumerable<ICodeEditorInstallation> GetVSCodeInstallations()
        {
            var candidates = new List<string>();

#if UNITY_EDITOR_WIN
            candidates.AddRange(GetWindowsInstallationPaths());
#elif UNITY_EDITOR_OSX
            candidates.AddRange(GetMacInstallationPaths());
#elif UNITY_EDITOR_LINUX
            candidates.AddRange(GetLinuxInstallationPaths());
#endif

            foreach (var candidate in candidates.Distinct())
            {
                if (TryDiscoverInstallation(candidate, out var installation))
                    yield return installation;
            }
        }

#if UNITY_EDITOR_WIN
        /// <summary>
        /// 获取Windows平台的安装路径
        /// </summary>
        /// <returns>候选路径列表</returns>
        private static IEnumerable<string> GetWindowsInstallationPaths()
        {
            var localAppPath = IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs");
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

            // 初始化注册表以获取所有变体
            VSCodeVariantRegistry.InitializeDefaultVariants();

            foreach (var variant in VSCodeVariantRegistry.GetAllVariants())
            {
                // 标准安装路径
                yield return IOPath.Combine(localAppPath, $"Microsoft {variant.DisplayName}", variant.ExecutablePattern.Replace(".*\\.exe$", ".exe"));
                yield return IOPath.Combine(localAppPath, $"Microsoft {variant.DisplayName} Insiders", variant.ExecutablePattern.Replace(".*\\.exe$", " - Insiders.exe"));
                
                yield return IOPath.Combine(programFiles, $"Microsoft {variant.DisplayName}", variant.ExecutablePattern.Replace(".*\\.exe$", ".exe"));
                yield return IOPath.Combine(programFiles, $"Microsoft {variant.DisplayName} Insiders", variant.ExecutablePattern.Replace(".*\\.exe$", " - Insiders.exe"));
                
                if (programFilesX86 != programFiles)
                {
                    yield return IOPath.Combine(programFilesX86, $"Microsoft {variant.DisplayName}", variant.ExecutablePattern.Replace(".*\\.exe$", ".exe"));
                    yield return IOPath.Combine(programFilesX86, $"Microsoft {variant.DisplayName} Insiders", variant.ExecutablePattern.Replace(".*\\.exe$", " - Insiders.exe"));
                }

                // 独立安装路径
                yield return IOPath.Combine(localAppPath, variant.DisplayName, variant.ExecutablePattern.Replace(".*\\.exe$", ".exe"));
                yield return IOPath.Combine(programFiles, variant.DisplayName, variant.ExecutablePattern.Replace(".*\\.exe$", ".exe"));
                if (programFilesX86 != programFiles)
                {
                    yield return IOPath.Combine(programFilesX86, variant.DisplayName, variant.ExecutablePattern.Replace(".*\\.exe$", ".exe"));
                }
            }
        }
#endif

#if UNITY_EDITOR_OSX
        /// <summary>
        /// 获取Mac平台的安装路径
        /// </summary>
        /// <returns>候选路径列表</returns>
        private static IEnumerable<string> GetMacInstallationPaths()
        {
            var appPath = "/Applications";
            
            // 初始化注册表以获取所有变体
            VSCodeVariantRegistry.InitializeDefaultVariants();

            foreach (var variant in VSCodeVariantRegistry.GetAllVariants())
            {
                // 标准应用
                yield return IOPath.Combine(appPath, $"{variant.DisplayName}.app");
                yield return IOPath.Combine(appPath, $"{variant.DisplayName} - Insiders.app");
            }
        }
#endif

#if UNITY_EDITOR_LINUX
        /// <summary>
        /// 获取Linux平台的安装路径
        /// </summary>
        /// <returns>候选路径列表</returns>
        private static IEnumerable<string> GetLinuxInstallationPaths()
        {
            // 初始化注册表以获取所有变体
            VSCodeVariantRegistry.InitializeDefaultVariants();

            foreach (var variant in VSCodeVariantRegistry.GetAllVariants())
            {
                // 标准位置
                var baseName = variant.Id.Replace("vscode", "visual-studio-code").Replace("traecn", "trae");
                yield return $"/usr/bin/{variant.Id}";
                yield return $"/usr/local/bin/{variant.Id}";
                yield return $"/opt/{baseName}/bin/{variant.Id}";
            }
        }
#endif

        /// <summary>
        /// VSCode package.json清单结构
        /// </summary>
        [Serializable]
        internal class VSCodeManifest
        {
            public string name;
            public string version;
        }

        /// <summary>
        /// 获取真实路径（处理符号链接）
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns>真实路径</returns>
        internal static string GetRealPath(string path)
        {
#if UNITY_EDITOR_LINUX
            try
            {
                byte[] buf = new byte[512];
                int ret = readlink(path, buf, buf.Length);
                if (ret == -1) return path;
                char[] cbuf = new char[512];
                int chars = System.Text.Encoding.Default.GetChars(buf, 0, ret, cbuf, 0);
                return new String(cbuf, 0, chars);
            }
            catch
            {
                return path;
            }
#else
            return path;
#endif
        }

#if UNITY_EDITOR_LINUX
        [System.Runtime.InteropServices.DllImport("libc")]
        private static extern int readlink(string path, byte[] buffer, int buflen);
#endif

        /// <summary>
        /// 初始化方法
        /// </summary>
        public static void Initialize()
        {
            // 初始化编辑器变体注册表
            VSCodeVariantRegistry.InitializeDefaultVariants();
        }

        public override string[] GetAnalyzers()
        {
            var vstuPath = GetExtensionPath();
            if (string.IsNullOrEmpty(vstuPath))
                return Array.Empty<string>();

            return GetAnalyzers(vstuPath);
        }

        private string GetExtensionPath()
        {
            var vscode = IsPrerelease ? ".vscode-insiders" : ".vscode";
            var extensionsPath = IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), vscode, "extensions");
            if (!Directory.Exists(extensionsPath))
                return null;

            return Directory
                .EnumerateDirectories(extensionsPath, $"{MicrosoftUnityExtensionId}*")
                .OrderByDescending(n => n)
                .FirstOrDefault();
        }

        private const string MicrosoftUnityExtensionId = "visualstudiotoolsforunity.vstuc";

        public override void CreateExtraFiles(string projectDirectory)
        {
            try
            {
                // UnityEngine.Debug.Log($"开始为项目目录 '{projectDirectory}' 创建VSCode配置文件...");

                var vscodeDirectory = IOPath.Combine(projectDirectory.NormalizePathSeparators(), ".vscode");
                // UnityEngine.Debug.Log($"VSCode配置目录: {vscodeDirectory}");

                Directory.CreateDirectory(vscodeDirectory);
                // UnityDebug.Log("✓ VSCode配置目录创建成功");

                CreateRecommendedExtensionsFile(vscodeDirectory);
                CreateSettingsFile(vscodeDirectory);
                CreateLaunchFile(vscodeDirectory);

                // UnityDebug.Log("✓ 所有VSCode配置文件创建完成");
            }
            catch (IOException ex)
            {
                // UnityDebug.LogWarning($"创建VSCode配置文件时出现IO异常: {ex.Message}");
                // UnityDebug.LogException(ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                // UnityDebug.LogWarning($"创建VSCode配置文件时权限不足: {ex.Message}");
                // UnityDebug.LogException(ex);
            }
            catch (Exception ex)
            {
                // UnityDebug.LogWarning($"创建VSCode配置文件时出错: {ex.Message}");
                // UnityDebug.LogException(ex);
            }
        }

        public override bool Open(string path, int line, int column, string solution)
        {
            var application = Path;

            line = Math.Max(1, line);
            column = Math.Max(0, column);

            var directory = IOPath.GetDirectoryName(solution);
            var workspace = TryFindWorkspace(directory);

            var target = workspace ?? directory;

            ProcessRunner.Start(string.IsNullOrEmpty(path)
                ? ProcessStartInfoFor(application, $"\"{target}\"")
                : ProcessStartInfoFor(application, $"\"{target}\" -g \"{path}\":{line}:{column}"));

            return true;
        }

        private static string TryFindWorkspace(string directory)
        {
            var files = Directory.GetFiles(directory, "*.code-workspace", SearchOption.TopDirectoryOnly);
            if (files.Length == 0 || files.Length > 1)
                return null;

            return files[0];
        }

        private static ProcessStartInfo ProcessStartInfoFor(string application, string arguments)
        {
#if UNITY_EDITOR_OSX
            // wrap with built-in OSX open feature
            arguments = $"-n \"{application}\" --args {arguments}";
            application = "open";
            return ProcessRunner.ProcessStartInfoFor(application, arguments, redirect: false, shell: true);
#else
            return ProcessRunner.ProcessStartInfoFor(application, arguments, redirect: false);
#endif
        }

        #region VSCode配置文件创建

        private const string DefaultLaunchFileContent = @"{
    ""version"": ""0.2.0"",
    ""configurations"": [
        {
            ""name"": ""Attach to Unity"",
            ""type"": ""vstuc"",
            ""request"": ""attach""
        }
    ]
}";

        private static void CreateLaunchFile(string vscodeDirectory)
        {
            var launchFile = IOPath.Combine(vscodeDirectory, "launch.json");

            if (File.Exists(launchFile))
            {
                return;
            }

            File.WriteAllText(launchFile, DefaultLaunchFileContent);
        }


        private void CreateSettingsFile(string vscodeDirectory)
        {
            var settingsFile = IOPath.Combine(vscodeDirectory, "settings.json");

            if (File.Exists(settingsFile))
            {
                return;
            }
            const string excludes = @"    ""files.exclude"": {
        ""**/.DS_Store"": true,
        ""**/.git"": true,
        ""**/.vs"": true,
        ""**/.gitmodules"": true,
        ""**/.vsconfig"": true,
        ""**/*.booproj"": true,
        ""**/*.pidb"": true,
        ""**/*.suo"": true,
        ""**/*.user"": true,
        ""**/*.userprefs"": true,
        ""**/*.unityproj"": true,
        ""**/*.dll"": true,
        ""**/*.exe"": true,
        ""**/*.pdf"": true,
        ""**/*.mid"": true,
        ""**/*.midi"": true,
        ""**/*.wav"": true,
        ""**/*.gif"": true,
        ""**/*.ico"": true,
        ""**/*.jpg"": true,
        ""**/*.jpeg"": true,
        ""**/*.png"": true,
        ""**/*.psd"": true,
        ""**/*.tga"": true,
        ""**/*.tif"": true,
        ""**/*.tiff"": true,
        ""**/*.3ds"": true,
        ""**/*.3DS"": true,
        ""**/*.fbx"": true,
        ""**/*.FBX"": true,
        ""**/*.lxo"": true,
        ""**/*.LXO"": true,
        ""**/*.ma"": true,
        ""**/*.MA"": true,
        ""**/*.obj"": true,
        ""**/*.OBJ"": true,
        ""**/*.meta"": true,
        ""build/"": true,
        ""Build/"": true,
        ""Library/"": true,
        ""library/"": true,
        ""obj/"": true,
        ""Obj/"": true,
        ""Logs/"": true,
        ""logs/"": true,
        ""UserSettings/"": true,
        ""temp/"": true,
        ""Temp/"": true
    }";

            var content = @"{
" + excludes + @",
    ""files.associations"": {
        ""*.asset"": ""yaml"",
        ""*.meta"": ""yaml"",
        ""*.prefab"": ""yaml"",
        ""*.unity"": ""yaml""
    },
    ""explorer.fileNesting.enabled"": true,
    ""explorer.fileNesting.patterns"": {
        ""*.sln"": ""*.csproj"",
        ""*.slnx"": ""*.csproj""
    },
    ""dotnet.defaultSolution"": """ + IOPath.GetFileName(ProjectGenerator.SolutionFile()) + @"""
}";


            File.WriteAllText(settingsFile, content);
            // UnityEngine.Debug.Log("✓ settings.json文件创建成功");
        }

        private const string RecommendedExtensionsContent = @"{
    ""recommendations"": [
        ""Zlorn.vstuc"",
        ""dotnetdev-kr-custom.csharp"",
        ""visualstudiotoolsforunity.vstuc"",
        ""ms-dotnettools.csdevkit"",
        ""ms-dotnettools.csharp"",
        ""ms-dotnettools.vscode-dotnet-runtime""
    ]
}";

        private static void CreateRecommendedExtensionsFile(string vscodeDirectory)
        {
            var extensionFile = IOPath.Combine(vscodeDirectory, "extensions.json");

            if (File.Exists(extensionFile))
            {
                return;
            }

            File.WriteAllText(extensionFile, RecommendedExtensionsContent);
        }

        private static void WriteAllTextFromJObject(string file, JSONNode node)
        {
            using (var fs = File.Open(file, FileMode.Create))
            using (var sw = new StreamWriter(fs))
            {
                sw.Write(node.ToString(4)); // 4 spaces indent
            }
        }

        #endregion
    }
}
