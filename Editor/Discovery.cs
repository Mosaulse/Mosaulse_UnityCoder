/*--------------------------------------------------------------------------------------------- 
 *  Copyright (c) UnityCoder Team. All rights reserved. 
 *  Licensed under the MIT License. See License.txt in the project root for license information. 
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using System.IO;

namespace UnityCoder.Editor.Integration
{
	internal static class Discovery
	{
		public static IEnumerable<IVisualStudioInstallation> GetVisualStudioInstallations()
		{
#if UNITY_EDITOR_WIN
			foreach (var installation in VisualStudioForWindowsInstallation.GetVisualStudioInstallations())
				yield return installation;
#endif

			// 使用新的VSCode变体注册系统
			foreach (var installation in GetVSCodeVariantInstallations())
			{
				var vsInstallation = installation as IVisualStudioInstallation;
				if (vsInstallation != null)
					yield return vsInstallation;
			}
		}

		public static bool TryDiscoverInstallation(string editorPath, out IVisualStudioInstallation installation)
		{
			try
			{
#if UNITY_EDITOR_WIN
				if (VisualStudioForWindowsInstallation.TryDiscoverInstallation(editorPath, out installation))
					return true;
#endif
				// 使用新的VSCode变体发现机制
				if (TryDiscoverVSCodeVariantInstallation(editorPath, out var codeInstallation))
				{
					installation = codeInstallation as IVisualStudioInstallation;
					return installation != null;
				}
			}
			catch (IOException)
			{
				installation = null;
			}

			return false;
		}

		public static void Initialize()
		{
#if UNITY_EDITOR_WIN
			VisualStudioForWindowsInstallation.Initialize();
#endif
			// 初始化VSCode变体注册系统
			VSCodeVariantRegistry.InitializeDefaultVariants();
		}

		// UnityCoder 插件需要的方法
		public static IEnumerable<ICodeEditorInstallation> GetSupportedInstallations()
		{
			// 返回所有已注册的VSCode变体安装
			return GetVSCodeVariantInstallations();
		}

		// UnityCoder 插件需要的方法
		public static bool TryDiscoverInstallation(string editorPath, out ICodeEditorInstallation installation)
		{
			// 使用新的VSCode变体发现机制
			return TryDiscoverVSCodeVariantInstallation(editorPath, out installation);
		}

		/// <summary>
		/// 获取所有VSCode变体安装
		/// </summary>
		/// <returns>安装列表</returns>
		private static IEnumerable<ICodeEditorInstallation> GetVSCodeVariantInstallations()
		{
			// 这里可以实现自动扫描系统中已安装的VSCode变体
			// 当前简化实现，后续可以扩展
			yield break;
		}

		/// <summary>
		/// 尝试发现VSCode变体安装
		/// </summary>
		/// <param name="editorPath">编辑器路径</param>
		/// <param name="installation">输出的安装对象</param>
		/// <returns>是否发现成功</returns>
		private static bool TryDiscoverVSCodeVariantInstallation(string editorPath, out ICodeEditorInstallation installation)
		{
			return VSCodeVariantInstallation.TryDiscoverInstallation(editorPath, out installation);
		}
	}
}