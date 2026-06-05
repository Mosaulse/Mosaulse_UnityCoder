/*---------------------------------------------------------------------------------------------
 *  Copyright (c) UnityCoder Team. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityDebug = UnityEngine.Debug; // 明确别名避免冲突

namespace UnityCoder.Editor.Integration
{
    /// <summary>
    /// VSCode编辑器变体配置信息
    /// </summary>
    public class VSCodeVariantConfig
    {
        /// <summary>
        /// 编辑器类型ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 路径匹配关键字（用于自动发现）
        /// </summary>
        public string[] PathKeywords { get; set; }

        /// <summary>
        /// 可执行文件名模式
        /// </summary>
        public string ExecutablePattern { get; set; }

        /// <summary>
        /// 是否为预发布版本的关键字
        /// </summary>
        public string[] PrereleaseKeywords { get; set; } = new string[] { "insider", "preview" };

        /// <summary>
        /// 自定义版本检测逻辑（可选）
        /// </summary>
        public Func<string, Version> CustomVersionDetector { get; set; }
    }

    /// <summary>
    /// VSCode变体编辑器注册管理器
    /// </summary>
    public static class VSCodeVariantRegistry
    {
        private static readonly Dictionary<string, VSCodeVariantConfig> _registeredVariants = new Dictionary<string, VSCodeVariantConfig>();
        private static bool _isInitialized = false;

        /// <summary>
        /// 初始化默认的编辑器变体
        /// </summary>
        public static void InitializeDefaultVariants()
        {
            if (_isInitialized) return;

            // 注册标准Visual Studio Code
            RegisterVariant(new VSCodeVariantConfig
            {
                Id = "vscode",
                DisplayName = "Visual Studio Code",
                PathKeywords = new[] { "code" },
                ExecutablePattern = @"code.*\.exe$"
            });

            // 注册Cursor
            RegisterVariant(new VSCodeVariantConfig
            {
                Id = "cursor",
                DisplayName = "Cursor",
                PathKeywords = new[] { "cursor" },
                ExecutablePattern = @"cursor.*\.exe$"
            });

            // 注册Antigravity
            RegisterVariant(new VSCodeVariantConfig
            {
                Id = "antigravity",
                DisplayName = "Antigravity",
                PathKeywords = new[] { "antigravity" },
                ExecutablePattern = @"antigravity.*\.exe$"
            });

            // 注册Trae
            RegisterVariant(new VSCodeVariantConfig
            {
                Id = "trae",
                DisplayName = "Trae",
                PathKeywords = new[] { "trae" },
                ExecutablePattern = @"trae.*\.exe$"
            });

            // 注册Trae CN
            RegisterVariant(new VSCodeVariantConfig
            {
                Id = "traecn",
                DisplayName = "Trae CN",
                PathKeywords = new[] { "trae cn" },
                ExecutablePattern = @"trae.*\.exe$"
            });

            // 注册Kiro
            RegisterVariant(new VSCodeVariantConfig
            {
                Id = "kiro",
                DisplayName = "Kiro",
                PathKeywords = new[] { "kiro" },
                ExecutablePattern = @"kiro.*\.exe$"
            });

            // 注册Qoder
            RegisterVariant(new VSCodeVariantConfig
            {
                Id = "qoder",
                DisplayName = "Qoder",
                PathKeywords = new[] { "qoder" },
                ExecutablePattern = @"qoder.*\.exe$"
            });

            // 注册Lingma
            RegisterVariant(new VSCodeVariantConfig
            {
                Id = "lingma",
                DisplayName = "Lingma",
                PathKeywords = new[] { "lingma" },
                ExecutablePattern = @"lingma.*\.exe$"
            });

            // 注册Windsurf
            RegisterVariant(new VSCodeVariantConfig
            {
                Id = "windsurf",
                DisplayName = "Windsurf",
                PathKeywords = new[] { "windsurf" },
                ExecutablePattern = @"windsurf.*\.exe$",
            });

            _isInitialized = true;
        }

        /// <summary>
        /// 注册新的编辑器变体
        /// </summary>
        /// <param name="config">编辑器配置</param>
        public static void RegisterVariant(VSCodeVariantConfig config)
        {
            if (string.IsNullOrEmpty(config.Id))
                throw new ArgumentException("编辑器ID不能为空");

            if (string.IsNullOrEmpty(config.DisplayName))
                throw new ArgumentException("显示名称不能为空");

            if (config.PathKeywords == null || config.PathKeywords.Length == 0)
                throw new ArgumentException("必须提供至少一个路径关键字");

            _registeredVariants[config.Id.ToLower()] = config;

        }

        /// <summary>
        /// 获取所有已注册的编辑器变体
        /// </summary>
        /// <returns>编辑器配置列表</returns>
        public static IEnumerable<VSCodeVariantConfig> GetAllVariants()
        {
            return _registeredVariants.Values;
        }

        /// <summary>
        /// 根据路径检测编辑器类型
        /// </summary>
        /// <param name="path">编辑器路径</param>
        /// <returns>匹配的编辑器配置，如果未找到则返回null</returns>
        public static VSCodeVariantConfig DetectVariantByPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;

            var lowerPath = path.ToLower();

            // 按优先级排序：更具体的匹配优先
            var sortedVariants = _registeredVariants.Values
                .OrderByDescending(v => v.PathKeywords.Max(kw =>
                    lowerPath.Contains(kw) ? kw.Length : 0));

            foreach (var variant in sortedVariants)
            {
                if (variant.PathKeywords.Any(keyword => lowerPath.Contains(keyword.ToLower())))
                {
                    return variant;
                }
            }

            return null;
        }

        /// <summary>
        /// 检查是否为候选发现路径
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns>是否为候选路径</returns>
        public static bool IsCandidateForDiscovery(string path)
        {
#if UNITY_EDITOR_OSX
            if (!Directory.Exists(path)) return false;
            return _registeredVariants.Values.Any(v => 
                Regex.IsMatch(path, $@".*({string.Join("|", v.PathKeywords)}).*\.app$", RegexOptions.IgnoreCase));
#elif UNITY_EDITOR_WIN
            if (!File.Exists(path)) return false;
            return _registeredVariants.Values.Any(v => 
                Regex.IsMatch(path, $@".*({string.Join("|", v.PathKeywords)}).*\.exe$", RegexOptions.IgnoreCase));
#else
            if (!File.Exists(path)) return false;
            return _registeredVariants.Values.Any(v =>
                Regex.IsMatch(path, $@".*({string.Join("|", v.PathKeywords)})$", RegexOptions.IgnoreCase));
#endif
        }
    }
}
