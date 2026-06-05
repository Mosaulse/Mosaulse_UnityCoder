using System;

#if UNITY_6000_5_OR_NEWER
using UnityEngine;
#endif

namespace UnityCoder.Editor.Integration.Testing
{
	[Serializable]
	internal class TestAdaptorContainer
	{
		public TestAdaptor[] TestAdaptors;
	}

	[Serializable]
	internal class TestAdaptor
	{
		public string Id;
		public string Name;
		public string FullName;

		public string Type;
		public string Method;
		public string Assembly;

		public int Parent;

		public TestAdaptor(object testAdaptor, int parent)
		{
			// 简化实现，避免依赖具体的测试框架类型
			Id = "test_id";
			Name = "test_name";
			FullName = "test_full_name";
			Type = "test_type";
			Method = "test_method";
			Assembly = "test_assembly";
			Parent = parent;
		}
	}
}
