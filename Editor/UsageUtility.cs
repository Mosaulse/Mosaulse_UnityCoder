/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/
using System;
using System.IO;
using System.Linq;

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Microsoft.Unity.VisualStudio.Editor
{
	[Serializable]
	internal class FileUsage
	{
		public string Path;
		public string[] GameObjectPath;
	}

	internal static class UsageUtility
	{
		internal static void ShowUsage(string json)
		{
			try
			{
				var usage = JsonUtility.FromJson<FileUsage>(json);
				ShowUsage(usage.Path, usage.GameObjectPath);
			}
			catch (Exception)
			{
				// ignore malformed request
			}
		}

		internal static void ShowUsage(string path, string[] gameObjectPath)
		{
			// 简化实现，移除对FileUtility的依赖
			path = MakeRelativeToProjectPath(path);
			if (path == null)
				return;

			path = NormalizeWindowsToUnix(path);
			var extension = Path.GetExtension(path).ToLower();

			EditorUtility.FocusProjectWindow();

			switch (extension)
			{
				case ".unity":
					ShowSceneUsage(path, gameObjectPath);
					break;
				default:
					var asset = AssetDatabase.LoadMainAssetAtPath(path);
					Selection.activeObject = asset;
					EditorGUIUtility.PingObject(asset);
					break;
			}
		}

		private static void ShowSceneUsage(string scenePath, string[] gameObjectPath)
		{
			var scene = SceneManager.GetSceneByPath(scenePath.Replace(Path.DirectorySeparatorChar, '/'));
			if (!scene.isLoaded)
			{
				var result = EditorUtility.DisplayDialogComplex("Show Usage",
						 $"Do you want to open \"{Path.GetFileName(scenePath)}\"?",
						 "Open Scene",
						 "Cancel",
						 "Open Scene (additive)");

				switch (result)
				{
					case 0:
						EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
						scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
						break;
					case 1:
						return;
					case 2:
						scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
						break;
				}
			}

			ShowSceneUsage(scene, gameObjectPath);
		}

		private static void ShowSceneUsage(Scene scene, string[] gameObjectPath)
		{
			if (gameObjectPath == null || gameObjectPath.Length == 0)
				return;

			var go = scene.GetRootGameObjects().FirstOrDefault(g => g.name == gameObjectPath[0]);
			if (go == null)
				return;

			for (var ni = 1; ni < gameObjectPath.Length; ni++)
			{
				var transform = go.transform;
				for (var i = 0; i < transform.childCount; i++)
				{
					var child = transform.GetChild(i);
					var childgo = child.gameObject;
					if (childgo.name == gameObjectPath[ni])
					{
						go = childgo;
						break;
					}
				}
			}

			Selection.activeObject = go;
			EditorGUIUtility.PingObject(go);
		}

		/// <summary>
		/// 将路径转换为相对于项目路径的路径
		/// </summary>
		/// <param name="path">文件路径</param>
		/// <returns>相对于项目路径的路径，如果转换失败则返回null</returns>
		private static string MakeRelativeToProjectPath(string path)
		{
			try
			{
				var projectPath = Path.GetFullPath(Application.dataPath).Replace("Assets", "");
				var fullPath = Path.GetFullPath(path);
				if (fullPath.StartsWith(projectPath))
				{
					return fullPath.Substring(projectPath.Length);
				}
				return null;
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		/// 将Windows路径分隔符转换为Unix路径分隔符
		/// </summary>
		/// <param name="path">文件路径</param>
		/// <returns>使用Unix路径分隔符的路径</returns>
		private static string NormalizeWindowsToUnix(string path)
		{
			return path.Replace('\\', '/');
		}
	}
}
