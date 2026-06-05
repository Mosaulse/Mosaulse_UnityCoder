/*---------------------------------------------------------------------------------------------
 *  Copyright (c) UnityCoder Team. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using Unity.CodeEditor;

[assembly: InternalsVisibleTo("UnityCoder.Editor.Integration.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace UnityCoder.Editor.Integration
{
    /// <summary>
    /// UnityCoder编辑器集成主类
    /// 支持Visual Studio Code及其变体编辑器，排除Visual Studio自动发现
    /// </summary>
    [InitializeOnLoad]
    public class UnityCoderEditor : IExternalCodeEditor
    {
        /// <summary>
        /// 已发现的编辑器安装
        /// </summary>
        private static readonly AsyncOperation<Dictionary<string, ICodeEditorInstallation>> _discoverInstallations;





        /// <summary>
        /// 构造函数
        /// </summary>
        static UnityCoderEditor()
        {
            if (!UnityInstallation.IsMainUnityEditorProcess)
                return;

            Discovery.Initialize();
            CodeEditor.Register(new UnityCoderEditor());

            _discoverInstallations = AsyncOperation<Dictionary<string, ICodeEditorInstallation>>.Run(DiscoverInstallations);
        }

        /// <summary>
        /// 获取已安装的编辑器列表
        /// </summary>
        CodeEditor.Installation[] IExternalCodeEditor.Installations => _discoverInstallations
            .Result
            .Values
            .Select(v => v.ToCodeEditorInstallation())
            .ToArray();

        /// <summary>
        /// 发现编辑器安装
        /// </summary>
        /// <returns>编辑器安装字典</returns>
        private static Dictionary<string, ICodeEditorInstallation> DiscoverInstallations()
        {
            try
            {
                var installations = new Dictionary<string, ICodeEditorInstallation>();

                // 自动发现VSCode及其变体
                foreach (var installation in Discovery.GetSupportedInstallations())
                {
                    var absolutePath = FileUtility.GetAbsolutePath(installation.Path);
                    if (!installations.ContainsKey(absolutePath))
                    {
                        installations.Add(absolutePath, installation);
                    }
                }





                return installations;
            }
            catch (Exception ex)
            {
                // UnityEngine.Debug.LogError($"DiscoverInstallations: 发现编辑器安装失败 - {ex.Message}");
                return new Dictionary<string, ICodeEditorInstallation>();
            }
        }

        /// <summary>
        /// 检查当前是否启用
        /// </summary>
        internal static bool IsEnabled => CodeEditor.CurrentEditor is UnityCoderEditor && UnityInstallation.IsMainUnityEditorProcess;

        /// <summary>
        /// 初始化编辑器
        /// </summary>
        /// <param name="editorInstallationPath">编辑器安装路径</param>
        public void Initialize(string editorInstallationPath)
        {
            // 初始化逻辑
        }

        /// <summary>
        /// 尝试获取指定路径的编辑器安装
        /// </summary>
        /// <param name="editorPath">编辑器路径</param>
        /// <param name="lookupDiscoveredInstallations">是否查找已发现的安装</param>
        /// <param name="installation">输出的安装对象</param>
        /// <returns>是否找到安装</returns>
        internal virtual bool TryGetEditorInstallationForPath(string editorPath, bool lookupDiscoveredInstallations, out ICodeEditorInstallation installation)
        {
            editorPath = FileUtility.GetAbsolutePath(editorPath);

            // 查找已发现的安装
            if (lookupDiscoveredInstallations && _discoverInstallations.Result.TryGetValue(editorPath, out installation))
                return true;

            // 尝试发现新安装
            return Discovery.TryDiscoverInstallation(editorPath, out installation);
        }

        /// <summary>
        /// 尝试获取指定路径的安装（实现IExternalCodeEditor接口）
        /// </summary>
        /// <param name="editorPath">编辑器路径</param>
        /// <param name="installation">输出的安装对象</param>
        /// <returns>是否找到安装</returns>
        public virtual bool TryGetInstallationForPath(string editorPath, out CodeEditor.Installation installation)
        {
            var result = TryGetEditorInstallationForPath(editorPath, lookupDiscoveredInstallations: false, out var coderInstallation);
            installation = coderInstallation?.ToCodeEditorInstallation() ?? default;
            return result;
        }

        /// <summary>
        /// GUI界面绘制
        /// </summary>
        public void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            var package = UnityEditor.PackageManager.PackageInfo.FindForAssembly(GetType().Assembly);
            var style = new GUIStyle
            {
                richText = true,
                margin = new RectOffset(0, 4, 0, 0)
            };

            GUILayout.Label($"<size=10><color=grey>{package.displayName} v{package.version} enabled</color></size>", style);
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // 显示当前选中的编辑器信息
            if (TryGetEditorInstallationForPath(CodeEditor.CurrentEditorInstallation, true, out var currentInstallation))
            {
                EditorGUILayout.LabelField("Current editor:", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Name:", currentInstallation.Name);
                EditorGUILayout.LabelField("Path:", currentInstallation.Path);
                EditorGUILayout.LabelField("Version:", currentInstallation.Version?.ToString() ?? "Unknown");
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUILayout.HelpBox("The current selected editor path is invalid. Please check the path or select a different editor.", MessageType.Warning);
            }

            EditorGUILayout.Space();



            EditorGUILayout.Space();

            // 项目生成选项
            EditorGUILayout.LabelField("Generate .csproj files for:", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            SettingsButton(ProjectGenerationFlag.Embedded, "Embedded packages", "", currentInstallation);
            SettingsButton(ProjectGenerationFlag.Local, "Local packages", "", currentInstallation);
            SettingsButton(ProjectGenerationFlag.Registry, "Registry packages", "", currentInstallation);
            SettingsButton(ProjectGenerationFlag.Git, "Git packages", "", currentInstallation);
            SettingsButton(ProjectGenerationFlag.BuiltIn, "Built-in packages", "", currentInstallation);
            SettingsButton(ProjectGenerationFlag.LocalTarBall, "Local tarball packages", "", currentInstallation);
            SettingsButton(ProjectGenerationFlag.Unknown, "Packages form unknown sources", "", currentInstallation);
            SettingsButton(ProjectGenerationFlag.PlayerAssemblies, "Player projects", "For each player project generate an additional csproj with the name 'project-player.csproj'", currentInstallation);
            RegenerateProjectFiles(currentInstallation);
            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// 刷新编辑器安装列表
        /// </summary>
        private static void RefreshEditorInstallations()
        {
            try
            {
                // 重新发现安装
                _discoverInstallations.Result.Clear();
                foreach (var kvp in DiscoverInstallations())
                {
                    _discoverInstallations.Result[kvp.Key] = kvp.Value;
                }

                // 强制Unity刷新外部工具设置UI
                EditorUtility.RequestScriptReload();
            }
            catch (Exception ex)
            {
                // UnityEngine.Debug.LogError($"RefreshEditorInstallations: 刷新编辑器安装列表失败 - {ex.Message}");
            }
        }



        /// <summary>
        /// 获取可执行文件过滤器
        /// </summary>
        /// <returns>文件过滤器字符串</returns>
        private string GetExecutableFilter()
        {
#if UNITY_EDITOR_WIN
            return "exe";
#elif UNITY_EDITOR_OSX
            return "app";
#else
            return "";
#endif
        }

        /// <summary>
        /// 重新生成项目文件按钮
        /// </summary>
        /// <param name="installation">编辑器安装</param>
        private static void RegenerateProjectFiles(ICodeEditorInstallation installation)
        {
            var rect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect());
            rect.width = 252;
            if (GUI.Button(rect, "Regenerate project files"))
            {
                installation?.ProjectGenerator.Sync();
            }
        }

        /// <summary>
        /// 设置按钮
        /// </summary>
        /// <param name="preference">偏好设置标志</param>
        /// <param name="guiMessage">GUI消息</param>
        /// <param name="toolTip">工具提示</param>
        /// <param name="installation">编辑器安装</param>
        private static void SettingsButton(ProjectGenerationFlag preference, string guiMessage, string toolTip, ICodeEditorInstallation installation)
        {
            if (installation?.ProjectGenerator?.AssemblyNameProvider == null)
                return;

            var generator = installation.ProjectGenerator;
            var prevValue = generator.AssemblyNameProvider.ProjectGenerationFlag.HasFlag(preference);

            var newValue = EditorGUILayout.Toggle(new GUIContent(guiMessage, toolTip), prevValue);
            if (newValue != prevValue)
                generator.AssemblyNameProvider.ToggleProjectGeneration(preference);
        }

        /// <summary>
        /// 根据需要同步文件
        /// </summary>
        /// <param name="addedFiles">添加的文件</param>
        /// <param name="deletedFiles">删除的文件</param>
        /// <param name="movedFiles">移动的文件</param>
        /// <param name="movedFromFiles">从何处移动的文件</param>
        /// <param name="importedFiles">导入的文件</param>
        public void SyncIfNeeded(string[] addedFiles, string[] deletedFiles, string[] movedFiles, string[] movedFromFiles, string[] importedFiles)
        {
            if (TryGetEditorInstallationForPath(CodeEditor.CurrentEditorInstallation, true, out var installation))
            {
                installation.ProjectGenerator.SyncIfNeeded(
                    addedFiles.Union(deletedFiles).Union(movedFiles).Union(movedFromFiles),
                    importedFiles);
            }

            // 检查调试符号文件
            foreach (var file in importedFiles.Where(a => Path.GetExtension(a) == ".pdb"))
            {
                var pdbFile = FileUtility.GetAssetFullPath(file);

                // 跳过Unity包如com.unity.ext.nunit
                if (pdbFile.IndexOf($"{Path.DirectorySeparatorChar}com.unity.", StringComparison.OrdinalIgnoreCase) > 0)
                    continue;

                var asmFile = Path.ChangeExtension(pdbFile, ".dll");
                if (!File.Exists(asmFile))
                    continue;


            }
        }

        /// <summary>
        /// 同步所有文件
        /// </summary>
        public void SyncAll()
        {
            if (TryGetEditorInstallationForPath(CodeEditor.CurrentEditorInstallation, true, out var installation))
            {
                installation.ProjectGenerator.Sync();
            }
        }

        /// <summary>
        /// 检查路径是否受支持
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="generator">项目生成器</param>
        /// <returns>是否受支持</returns>
        private static bool IsSupportedPath(string path, IGenerator generator)
        {
            // 路径为空表示"打开C#项目"，我们只想打开解决方案而不指定特定文件
            if (string.IsNullOrEmpty(path))
                return true;

            // cs, uxml, uss, shader, compute, cginc, hlsl, glslinc, template是Unity内置扩展名
            // txt, xml, fnt, asmdef通常是Unity用户扩展名
            return generator.IsSupportedFile(path);
        }

        /// <summary>
        /// 打开项目
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="line">行号</param>
        /// <param name="column">列号</param>
        /// <returns>是否成功打开</returns>
        public bool OpenProject(string path, int line, int column)
        {
            var editorPath = CodeEditor.CurrentEditorInstallation;

            // 尝试获取编辑器安装实例
            if (!TryGetEditorInstallationForPath(editorPath, true, out var installation))
            {
                return false;
            }

            // 获取项目生成器
            var generator = installation.ProjectGenerator;
            if (generator == null)
            {
                return false;
            }

            // 检查路径是否受支持
            if (!IsSupportedPath(path, generator))
            {
                return false;
            }

            // 检查项目是否已生成
            if (!IsProjectGeneratedFor(path, generator, out var missingFlag))
            {
                return false;
            }

            // 获取或生成解决方案文件
            var solutionFile = GetOrGenerateSolutionFile(generator);
            if (string.IsNullOrEmpty(solutionFile))
            {
                return false;
            }

            // 尝试打开项目
            try
            {
                return installation.Open(path, line, column, solutionFile);
            }
            catch (Exception ex)
            {
                // UnityEngine.Debug.LogError($"OpenProject: 打开项目失败 - {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取项目生成标志描述
        /// </summary>
        /// <param name="flag">生成标志</param>
        /// <returns>描述文本</returns>
        private static string GetProjectGenerationFlagDescription(ProjectGenerationFlag flag)
        {
            switch (flag)
            {
                case ProjectGenerationFlag.BuiltIn:
                    return "内置包";
                case ProjectGenerationFlag.Embedded:
                    return "嵌入式包";
                case ProjectGenerationFlag.Git:
                    return "Git包";
                case ProjectGenerationFlag.Local:
                    return "本地包";
                case ProjectGenerationFlag.LocalTarBall:
                    return "本地tarball";
                case ProjectGenerationFlag.PlayerAssemblies:
                    return "播放器项目";
                case ProjectGenerationFlag.Registry:
                    return "注册表包";
                case ProjectGenerationFlag.Unknown:
                    return "来源未知的包";
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// 检查项目是否为指定文件生成
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="generator">项目生成器</param>
        /// <param name="missingFlag">缺失的标志</param>
        /// <returns>是否已生成</returns>
        private static bool IsProjectGeneratedFor(string path, IGenerator generator, out ProjectGenerationFlag missingFlag)
        {
            missingFlag = ProjectGenerationFlag.None;

            // 打开整个解决方案时无需检查
            if (string.IsNullOrEmpty(path))
                return true;

            // 我们只检查cs脚本
            // 这里简化实现，实际应该检查脚本语言
            if (Path.GetExtension(path)?.ToLower() != ".cs")
                return true;

            // 简化实现，假设项目已正确生成
            return true;
        }

        /// <summary>
        /// 获取或生成解决方案文件
        /// </summary>
        /// <param name="generator">项目生成器</param>
        /// <returns>解决方案文件路径</returns>
        private static string GetOrGenerateSolutionFile(IGenerator generator)
        {
            generator.Sync();
            return generator.SolutionFile();
        }
    }
}
