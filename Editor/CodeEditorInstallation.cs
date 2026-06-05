/*---------------------------------------------------------------------------------------------
 *  Copyright (c) UnityCoder Team. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.IO;
using Unity.CodeEditor;
using IOPath = System.IO.Path;

namespace UnityCoder.Editor.Integration
{
    /// <summary>
    /// 代码编辑器安装接口
    /// </summary>
    internal interface ICodeEditorInstallation
    {
        /// <summary>
        /// 编辑器名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 编辑器可执行文件路径
        /// </summary>
        string Path { get; }

        /// <summary>
        /// 编辑器版本
        /// </summary>
        Version Version { get; }

        /// <summary>
        /// 是否支持分析器
        /// </summary>
        bool SupportsAnalyzers { get; }

        /// <summary>
        /// 支持的最新C#语言版本
        /// </summary>
        Version LatestLanguageVersionSupported { get; }

        /// <summary>
        /// 获取分析器DLL路径数组
        /// </summary>
        /// <returns>分析器DLL路径数组</returns>
        string[] GetAnalyzers();

        /// <summary>
        /// 转换为Unity CodeEditor安装对象
        /// </summary>
        /// <returns>CodeEditor安装对象</returns>
        CodeEditor.Installation ToCodeEditorInstallation();

        /// <summary>
        /// 打开文件或项目
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="line">行号</param>
        /// <param name="column">列号</param>
        /// <param name="solutionPath">解决方案路径</param>
        /// <returns>是否成功打开</returns>
        bool Open(string path, int line, int column, string solutionPath);

        /// <summary>
        /// 获取项目生成器
        /// </summary>
        IGenerator ProjectGenerator { get; }

        /// <summary>
        /// 创建额外的配置文件
        /// </summary>
        /// <param name="projectDirectory">项目目录</param>
        void CreateExtraFiles(string projectDirectory);
    }

    /// <summary>
    /// 代码编辑器安装基类
    /// </summary>
    internal abstract class CodeEditorInstallation : ICodeEditorInstallation
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public Version Version { get; set; }

        public abstract bool SupportsAnalyzers { get; }
        public abstract Version LatestLanguageVersionSupported { get; }
        public abstract string[] GetAnalyzers();
        public abstract IGenerator ProjectGenerator { get; }
        public abstract void CreateExtraFiles(string projectDirectory);
        public abstract bool Open(string path, int line, int column, string solutionPath);

        /// <summary>
        /// 获取支持的语言版本
        /// </summary>
        /// <param name="versions">版本映射表</param>
        /// <returns>支持的语言版本</returns>
        protected Version GetLatestLanguageVersionSupported(VersionPair[] versions)
        {
            if (versions != null)
            {
                foreach (var entry in versions)
                {
                    if (Version >= entry.IdeVersion)
                        return entry.LanguageVersion;
                }
            }

            // 默认返回7.0
            return new Version(7, 0);
        }

        /// <summary>
        /// 获取分析器DLL文件
        /// </summary>
        /// <param name="path">分析器目录路径</param>
        /// <returns>分析器DLL文件路径数组</returns>
        protected static string[] GetAnalyzers(string path)
        {
            var analyzersDirectory = FileUtility.GetAbsolutePath(IOPath.Combine(path, "Analyzers"));

            if (Directory.Exists(analyzersDirectory))
                return Directory.GetFiles(analyzersDirectory, "*Analyzers.dll", SearchOption.AllDirectories);

            return Array.Empty<string>();
        }

        /// <summary>
        /// 转换为Unity CodeEditor安装对象
        /// </summary>
        /// <returns>CodeEditor安装对象</returns>
        public CodeEditor.Installation ToCodeEditorInstallation()
        {
            return new CodeEditor.Installation() { Name = Name, Path = Path };
        }
    }
}

