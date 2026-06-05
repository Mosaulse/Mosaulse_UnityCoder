/*---------------------------------------------------------------------------------------------
 *  Copyright (c) UnityCoder Team. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;
using IOPath = System.IO.Path;

namespace UnityCoder.Editor.Integration
{
	/// <summary>
	/// 文件工具类
	/// </summary>
	internal static class FileUtility
	{
		private static string _packagePath;

		/// <summary>
		/// 获取包的绝对路径
		/// </summary>
		/// <returns>包路径</returns>
		public static string PackagePath
		{
			get
			{
				if (string.IsNullOrEmpty(_packagePath))
				{
					var assembly = Assembly.GetExecutingAssembly();
					var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(assembly);
					if (packageInfo != null)
					{
						_packagePath = packageInfo.resolvedPath;
					}
				}
				return _packagePath;
			}
		}

		/// <summary>
		/// 获取相对于Assets目录的完整路径
		/// </summary>
		/// <param name="assetPath">相对于Assets的路径</param>
		/// <returns>完整路径</returns>
		public static string GetAssetFullPath(string assetPath)
		{
			return IOPath.Combine(Application.dataPath, assetPath);
		}

		/// <summary>
		/// 获取包内资源的完整路径
		/// </summary>
		/// <param name="paths">路径片段</param>
		/// <returns>完整路径</returns>
		public static string GetPackageAssetFullPath(params string[] paths)
		{
			if (string.IsNullOrEmpty(PackagePath))
				return null;

			var fullPath = PackagePath;
			foreach (var path in paths)
			{
				fullPath = IOPath.Combine(fullPath, path);
			}

			return File.Exists(fullPath) ? fullPath : null;
		}

		/// <summary>
		/// 获取绝对路径
		/// </summary>
		/// <param name="path">相对或绝对路径</param>
		/// <returns>绝对路径</returns>
		public static string GetAbsolutePath(string path)
		{
			if (string.IsNullOrEmpty(path))
				return string.Empty;

			if (IOPath.IsPathRooted(path))
				return path;

			return IOPath.GetFullPath(IOPath.Combine(Environment.CurrentDirectory, path));
		}

		/// <summary>
		/// 规范化路径分隔符为Unix风格
		/// </summary>
		/// <param name="path">路径</param>
		/// <returns>规范化后的路径</returns>
		public static string NormalizeWindowsToUnix(this string path)
		{
			return path?.Replace('\\', '/');
		}

		/// <summary>
		/// 规范化路径分隔符
		/// </summary>
		/// <param name="path">路径</param>
		/// <returns>规范化后的路径</returns>
		public static string NormalizePathSeparators(this string path)
		{
			return path?.Replace('\\', IOPath.DirectorySeparatorChar)
					  .Replace('/', IOPath.DirectorySeparatorChar);
		}

		/// <summary>
		/// Unix路径分隔符
		/// </summary>
		public static readonly char UnixSeparator = '/';

		/// <summary>
		/// 安全地写入文件内容，处理文件被占用的情况
		/// </summary>
		/// <param name="filePath">文件路径</param>
		/// <param name="content">文件内容</param>
		/// <param name="encoding">编码格式</param>
		/// <param name="maxRetries">最大重试次数</param>
		/// <param name="retryDelayMs">重试延迟毫秒数</param>
		/// <returns>是否成功写入</returns>
		public static bool SafeWriteAllText(string filePath, string content, Encoding encoding, int maxRetries = 3, int retryDelayMs = 100)
		{
			for (int attempt = 0; attempt <= maxRetries; attempt++)
			{
				try
				{
					// 检查文件是否被占用
					if (IsFileLocked(filePath))
					{
						if (attempt < maxRetries)
						{
							// Debug.LogWarning($"文件 {filePath} 被占用，等待 {retryDelayMs}ms 后重试... (尝试 {attempt + 1}/{maxRetries + 1})");
							System.Threading.Thread.Sleep(retryDelayMs);
							continue;
						}
						else
						{
							// Debug.LogError($"文件 {filePath} 持续被占用，无法写入");
							return false;
						}
					}

					// 写入文件
					File.WriteAllText(filePath, content, encoding);
					return true;
				}
				catch (UnauthorizedAccessException ex)
				{
					if (attempt < maxRetries)
					{
						// Debug.LogWarning($"无权限访问文件 {filePath}: {ex.Message}，等待 {retryDelayMs}ms 后重试...");
						System.Threading.Thread.Sleep(retryDelayMs);
					}
					else
					{
						// Debug.LogError($"无权限访问文件 {filePath}: {ex.Message}");
						return false;
					}
				}
				catch (IOException ex)
				{
					if (attempt < maxRetries)
					{
						// Debug.LogWarning($"IO异常写入文件 {filePath}: {ex.Message}，等待 {retryDelayMs}ms 后重试...");
						System.Threading.Thread.Sleep(retryDelayMs);
					}
					else
					{
						// Debug.LogError($"IO异常写入文件 {filePath}: {ex.Message}");
						return false;
					}
				}
				catch (Exception ex)
				{
					// Debug.LogError($"写入文件 {filePath} 时发生未知错误: {ex.Message}");
					return false;
				}
			}

			return false;
		}

		/// <summary>
		/// 检查文件是否被锁定/占用
		/// </summary>
		/// <param name="filePath">文件路径</param>
		/// <returns>是否被锁定</returns>
		public static bool IsFileLocked(string filePath)
		{
			if (!File.Exists(filePath))
				return false;

			try
			{
				using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
				{
					stream.Close();
				}
				return false;
			}
			catch (IOException)
			{
				// 文件被占用
				return true;
			}
			catch (UnauthorizedAccessException)
			{
				// 没有权限访问文件，可能被占用
				return true;
			}
			catch
			{
				// 其他异常，假设文件被占用
				return true;
			}
		}
	}
}

