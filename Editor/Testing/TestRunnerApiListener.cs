using System;
using UnityEditor;
using UnityEngine;

namespace UnityCoder.Editor.Integration.Testing
{
	[InitializeOnLoad]
	internal class TestRunnerApiListener
	{

		static TestRunnerApiListener()
		{
			// 简化实现，避免依赖具体的测试框架类型
		}

		public static void RetrieveTestList(string mode)
		{
			// 简化实现
		}

		private static void RetrieveTestList(object mode)
		{
			// 简化实现
		}

		public static void ExecuteTests(string command)
		{
			// 简化实现
		}

		private static void ExecuteTests(object filter)
		{
			// 简化实现
		}
	}
}
