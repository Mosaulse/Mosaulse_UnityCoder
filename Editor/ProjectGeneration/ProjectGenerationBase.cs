/*---------------------------------------------------------------------------------------------
 *  Copyright (c) UnityCoder Team. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor.Compilation;
using UnityEngine;
using Unity.CodeEditor;

namespace UnityCoder.Editor.Integration
{
    /// <summary>
    /// 项目生成基类
    /// </summary>
    internal abstract class ProjectGenerationBase : IGenerator
    {
        protected const string k_WindowsNewline = "\r\n";

        protected readonly string m_ProjectDirectory;
        protected readonly IAssemblyNameProvider m_AssemblyNameProvider;
        protected readonly IFileIO m_FileIOProvider;
        protected readonly IGUIDGenerator m_GUIDProvider;

        public string ProjectDirectory => m_ProjectDirectory;
        public IAssemblyNameProvider AssemblyNameProvider => m_AssemblyNameProvider;

        protected ProjectGenerationBase(
            string projectDirectory,
            IAssemblyNameProvider assemblyNameProvider,
            IFileIO fileIoProvider,
            IGUIDGenerator guidProvider)
        {
            m_ProjectDirectory = projectDirectory ?? "";
            m_AssemblyNameProvider = assemblyNameProvider;
            m_FileIOProvider = fileIoProvider;
            m_GUIDProvider = guidProvider;
        }

        public abstract GeneratorStyle Style { get; }

        public virtual void Sync()
        {
            // 获取所有程序集
            var assemblies = CompilationPipeline.GetAssemblies();
            GenerateSolutionFile(assemblies);
            GenerateProjectFiles(assemblies);

            // 创建额外的编辑器配置文件
            CreateEditorExtraFiles();
        }

        public virtual bool SyncIfNeeded(IEnumerable<string> affectedFiles, IEnumerable<string> reimportedFiles)
        {
            // 简化实现，总是重新生成
            Sync();

            // 同样创建额外配置文件
            CreateEditorExtraFiles();

            // 总是返回 true，表示已同步
            return true;
        }

        public virtual bool HasSolutionBeenGenerated()
        {
            return File.Exists(SolutionFile());
        }

        public virtual string SolutionFile()
        {
            return SolutionFileImpl();
        }

        protected virtual string SolutionFileImpl()
        {
            return Path.Combine(m_ProjectDirectory, $"{Path.GetFileName(m_ProjectDirectory)}.sln");
        }

        public virtual bool IsSupportedFile(string path)
        {
            if (string.IsNullOrEmpty(path))
                return true;

            var extension = Path.GetExtension(path)?.ToLower();
            var supportedExtensions = new[] { ".cs", ".uxml", ".uss", ".shader", ".compute", ".cginc", ".hlsl", ".glslinc", ".template", ".txt", ".xml", ".fnt", ".asmdef" };

            return Array.IndexOf(supportedExtensions, extension) >= 0;
        }

        protected virtual void GenerateSolutionFile(Assembly[] assemblies)
        {
            try
            {
                var solutionPath = SolutionFile();
                var content = SolutionText(assemblies);

                // 确保目录存在
                var directory = Path.GetDirectoryName(solutionPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    // Debug.Log($"创建目录: {directory}");
                }

                // 写入解决方案文件
                m_FileIOProvider.WriteAllText(solutionPath, content);
                // Debug.Log($"已生成解决方案文件: {solutionPath}");
            }
            catch (Exception ex)
            {
                // Debug.LogError($"生成解决方案文件失败: {ex.Message}");
                // Debug.LogException(ex);
            }
        }

        protected virtual void GenerateProjectFiles(Assembly[] assemblies)
        {
            try
            {
                foreach (var assembly in assemblies)
                {
                    var projectPath = Path.Combine(m_ProjectDirectory, $"{assembly.name}.csproj");
                    var content = GenerateProjectContent(assembly);

                    // 确保目录存在
                    var directory = Path.GetDirectoryName(projectPath);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                        // Debug.Log($"创建目录: {directory}");
                    }

                    // 写入项目文件
                    m_FileIOProvider.WriteAllText(projectPath, content);
                    // Debug.Log($"已生成项目文件: {projectPath}");
                }
            }
            catch (Exception ex)
            {
                // Debug.LogError($"生成项目文件失败: {ex.Message}");
                // Debug.LogException(ex);
            }
        }

        protected virtual string GenerateProjectContent(Assembly assembly)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<Project Sdk=\"Microsoft.NET.Sdk\">");
            sb.AppendLine("  <PropertyGroup>");
            sb.AppendLine("    <TargetFramework>netstandard2.1</TargetFramework>");
            sb.AppendLine("    <LangVersion>latest</LangVersion>");
            sb.AppendLine("    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>");
            sb.AppendLine("    <AssemblyName>" + assembly.name + "</AssemblyName>");
            sb.AppendLine("  </PropertyGroup>");

            // 添加引用
            if (assembly.assemblyReferences?.Length > 0)
            {
                sb.AppendLine("  <ItemGroup>");
                foreach (var reference in assembly.assemblyReferences)
                {
                    sb.AppendLine($"    <ProjectReference Include=\"{reference.name}.csproj\" />");
                }
                sb.AppendLine("  </ItemGroup>");
            }

            sb.AppendLine("</Project>");
            return sb.ToString();
        }

        /// <summary>
        /// 创建编辑器额外配置文件
        /// </summary>
        protected virtual void CreateEditorExtraFiles()
        {
            try
            {
                // 获取当前编辑器安装路径
                var currentEditorPath = CodeEditor.CurrentEditorInstallation;
                // Debug.Log($"当前编辑器路径: {currentEditorPath}");

                // 简化实现，移除对IVisualStudioInstallation的依赖
                // Debug.Log($"编辑器路径: {currentEditorPath}");
                // Debug.Log("跳过编辑器额外配置文件创建");
            }
            catch (Exception ex)
            {
                // Debug.LogWarning($"创建编辑器额外配置文件时出错: {ex.Message}");
                // Debug.LogException(ex);
            }
        }

        protected virtual string SolutionText(IEnumerable<Assembly> assemblies, Solution previousSolution = null)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Microsoft Visual Studio Solution File, Format Version 12.00");
            sb.AppendLine("# Visual Studio Version 17");
            sb.AppendLine("VisualStudioVersion = 17.0.31903.59");
            sb.AppendLine("MinimumVisualStudioVersion = 10.0.40219.1");

            foreach (var assembly in assemblies)
            {
                var projectGuid = m_GUIDProvider.ProjectGuid(m_ProjectDirectory, assembly.name);
                var projectPath = $"{assembly.name}.csproj";

                sb.AppendLine($"Project(\"{{{m_GUIDProvider.SolutionGuid(m_ProjectDirectory, ScriptingLanguage.CSharp)}}}\") = \"{assembly.name}\", \"{projectPath}\", \"{{{projectGuid}}}\"");
                sb.AppendLine("EndProject");
            }

            sb.AppendLine("Global");
            sb.AppendLine("\tGlobalSection(SolutionConfigurationPlatforms) = preSolution");
            sb.AppendLine("\t\tDebug|Any CPU = Debug|Any CPU");
            sb.AppendLine("\t\tRelease|Any CPU = Release|Any CPU");
            sb.AppendLine("\tEndGlobalSection");
            sb.AppendLine("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution");

            foreach (var assembly in assemblies)
            {
                var projectGuid = m_GUIDProvider.ProjectGuid(m_ProjectDirectory, assembly.name);
                sb.AppendLine($"\t\t{{{projectGuid}}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU");
                sb.AppendLine($"\t\t{{{projectGuid}}}.Debug|Any CPU.Build.0 = Debug|Any CPU");
                sb.AppendLine($"\t\t{{{projectGuid}}}.Release|Any CPU.ActiveCfg = Release|Any CPU");
                sb.AppendLine($"\t\t{{{projectGuid}}}.Release|Any CPU.Build.0 = Release|Any CPU");
            }

            sb.AppendLine("\tEndGlobalSection");
            sb.AppendLine("EndGlobal");

            return sb.ToString();
        }

        protected virtual IEnumerable<SolutionProjectEntry> GetSolutionProjects(IEnumerable<Assembly> assemblies, Solution previousSolution)
        {
            var projects = new List<SolutionProjectEntry>();

            foreach (var assembly in assemblies)
            {
                var projectPath = $"{assembly.name}.csproj";
                projects.Add(new SolutionProjectEntry
                {
                    ProjectFactoryGuid = "FAE04EC0-301F-11D3-BF4B-00C04F79EFBC",
                    Name = assembly.name,
                    FileName = projectPath,
                    ProjectGuid = m_GUIDProvider.ProjectGuid(m_ProjectDirectory, assembly.name),
                    Metadata = ""
                });
            }

            return projects;
        }

        protected virtual string GetProjectExtension()
        {
            return ".csproj";
        }

        protected virtual void GetProjectHeaderProperties(ProjectProperties properties, StringBuilder headerBuilder)
        {
            // 基础实现，子类可以重写
        }

        protected virtual void GetProjectHeaderVstuFlavoring(ProjectProperties properties, StringBuilder headerBuilder, bool isPlayerProject)
        {
            // 基础实现，子类可以重写
        }

        protected virtual void GetProjectHeaderAnalyzers(ProjectProperties properties, StringBuilder headerBuilder)
        {
            if (properties.Analyzers?.Any() == true)
            {
                headerBuilder.Append("  <ItemGroup>").Append(k_WindowsNewline);
                foreach (var analyzer in properties.Analyzers)
                {
                    headerBuilder.Append("    <Analyzer Include=\"" + analyzer + "\" />").Append(k_WindowsNewline);
                }
                headerBuilder.Append("  </ItemGroup>").Append(k_WindowsNewline);
            }
        }
    }


}
