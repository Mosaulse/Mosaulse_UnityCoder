using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityCoder.Editor.Integration.Testing
{
	internal class TestRunnerCallbacks
	{
		private string Serialize<TContainer, TSource, TAdaptor>(
			TSource source,
			Func<TSource, int, TAdaptor> createAdaptor,
			Func<TSource, IEnumerable<TSource>> children,
			Func<TAdaptor[], TContainer> container)
		{
			var adaptors = new List<TAdaptor>();

			void AddAdaptor(TSource item, int parentIndex)
			{
				var index = adaptors.Count;
				adaptors.Add(createAdaptor(item, parentIndex));
				foreach (var child in children(item))
					AddAdaptor(child, index);
			}

			AddAdaptor(source, -1);

			return JsonUtility.ToJson(container(adaptors.ToArray()));
		}

		private string Serialize(object testAdaptor)
		{
			// 简化实现
			return "{}";
		}

		public void RunFinished(object testResultAdaptor)
		{
			// 简化实现
		}

		public void RunStarted(object testAdaptor)
		{
			// 简化实现
		}

		public void TestFinished(object testResultAdaptor)
		{
			// 简化实现
		}

		public void TestStarted(object testAdaptor)
		{
			// 简化实现
		}

		private static string TestModeName(object testMode)
		{
			// 简化实现
			return "EditMode";
		}

		internal void TestListRetrieved(object testMode, object testAdaptor)
		{
			// 简化实现
		}
	}
}
