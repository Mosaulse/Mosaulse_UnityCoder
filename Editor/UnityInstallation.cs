/*---------------------------------------------------------------------------------------------
 *  Copyright (c) UnityCoder Team. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using UnityEngine;

namespace UnityCoder.Editor.Integration
{
    /// <summary>
    /// Unity安装检测辅助类
    /// </summary>
    internal static class UnityInstallation
    {
        /// <summary>
        /// 检查是否为主Unity编辑器进程
        /// </summary>
        public static bool IsMainUnityEditorProcess
        {
            get
            {
                try
                {
                    // 检查是否在编辑器环境中运行
#if UNITY_EDITOR
                    return !Application.isBatchMode && !Application.isConsolePlatform;
#else
                    return false;
#endif
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 获取Unity版本信息
        /// </summary>
        public static string UnityVersion
        {
            get
            {
#if UNITY_EDITOR
                return Application.unityVersion;
#else
                return "Unknown";
#endif
            }
        }

        /// <summary>
        /// 检查Unity版本是否满足最低要求
        /// </summary>
        /// <param name="minimumVersion">最低版本要求</param>
        /// <returns>是否满足要求</returns>
        public static bool IsUnityVersionSupported(string minimumVersion)
        {
            try
            {
                var currentVersion = new Version(UnityVersion.Split('f')[0]);
                var minVersion = new Version(minimumVersion);
                return currentVersion >= minVersion;
            }
            catch (Exception)
            {
                // 如果版本解析失败，默认返回true
                return true;
            }
        }
    }
}

