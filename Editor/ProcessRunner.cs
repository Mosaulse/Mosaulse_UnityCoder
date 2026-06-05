/*---------------------------------------------------------------------------------------------
 *  Copyright (c) UnityCoder Team. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Diagnostics;
using System.Text;

namespace UnityCoder.Editor.Integration
{
    /// <summary>
    /// 进程运行结果
    /// </summary>
    internal class ProcessRunnerResult
    {
        public bool Success { get; set; }
        public string Output { get; set; }
        public string Error { get; set; }
    }

    /// <summary>
    /// 进程运行器
    /// </summary>
    internal static class ProcessRunner
    {
        /// <summary>
        /// 启动进程并等待退出
        /// </summary>
        /// <param name="fileName">可执行文件名</param>
        /// <param name="arguments">参数</param>
        /// <returns>运行结果</returns>
        public static ProcessRunnerResult StartAndWaitForExit(string fileName, string arguments = "")
        {
            try
            {
                var startInfo = ProcessStartInfoFor(fileName, arguments);
                return StartAndWaitForExit(startInfo);
            }
            catch (Exception ex)
            {
                return new ProcessRunnerResult
                {
                    Success = false,
                    Error = ex.Message,
                    Output = string.Empty
                };
            }
        }

        /// <summary>
        /// 启动进程并等待退出（带输出回调）
        /// </summary>
        /// <param name="startInfo">进程启动信息</param>
        /// <param name="onOutputReceived">输出接收回调</param>
        /// <returns>运行结果</returns>
        public static ProcessRunnerResult StartAndWaitForExit(ProcessStartInfo startInfo, Action<string> onOutputReceived = null)
        {
            var output = new StringBuilder();
            var error = new StringBuilder();

            try
            {
                using (var process = new Process())
                {
                    process.StartInfo = startInfo;
                    process.EnableRaisingEvents = true;

                    if (startInfo.RedirectStandardOutput)
                    {
                        process.OutputDataReceived += (sender, args) =>
                        {
                            if (!string.IsNullOrEmpty(args.Data))
                            {
                                output.AppendLine(args.Data);
                                onOutputReceived?.Invoke(args.Data);
                            }
                        };
                    }

                    if (startInfo.RedirectStandardError)
                    {
                        process.ErrorDataReceived += (sender, args) =>
                        {
                            if (!string.IsNullOrEmpty(args.Data))
                            {
                                error.AppendLine(args.Data);
                            }
                        };
                    }

                    process.Start();

                    if (startInfo.RedirectStandardOutput)
                        process.BeginOutputReadLine();

                    if (startInfo.RedirectStandardError)
                        process.BeginErrorReadLine();

                    process.WaitForExit();

                    return new ProcessRunnerResult
                    {
                        Success = process.ExitCode == 0,
                        Output = output.ToString(),
                        Error = error.ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                return new ProcessRunnerResult
                {
                    Success = false,
                    Error = ex.Message,
                    Output = output.ToString()
                };
            }
        }

        /// <summary>
        /// 启动进程（不等待）
        /// </summary>
        /// <param name="startInfo">进程启动信息</param>
        public static void Start(ProcessStartInfo startInfo)
        {
            try
            {
                using (var process = new Process())
                {
                    process.StartInfo = startInfo;
                    process.Start();
                }
            }
            catch (Exception ex)
            {
                // UnityEngine.Debug.LogError($"启动进程失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 创建进程启动信息
        /// </summary>
        /// <param name="fileName">可执行文件名</param>
        /// <param name="arguments">参数</param>
        /// <param name="redirect">是否重定向输出</param>
        /// <param name="shell">是否使用shell执行</param>
        /// <returns>进程启动信息</returns>
        public static ProcessStartInfo ProcessStartInfoFor(string fileName, string arguments, bool redirect = true, bool shell = false)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = shell,
                RedirectStandardOutput = redirect && !shell,
                RedirectStandardError = redirect && !shell,
                CreateNoWindow = true
            };

            // 只有在重定向输出时才设置编码
            if (redirect && !shell)
            {
                startInfo.StandardOutputEncoding = Encoding.UTF8;
                startInfo.StandardErrorEncoding = Encoding.UTF8;
            }

            return startInfo;
        }
    }
}

