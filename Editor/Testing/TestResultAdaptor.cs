using System;

namespace UnityCoder.Editor.Integration.Testing
{
	[Serializable]
	internal enum TestStatusAdaptor
	{
		Passed,
		Failed,
		Skipped,
		Inconclusive
	}

	[Serializable]
	internal class TestResultAdaptorContainer
	{
		public TestResultAdaptor[] TestResultAdaptors;
	}

	[Serializable]
	internal class TestResultAdaptor
	{
		public string Name;
		public string FullName;

		public int PassCount;
		public int FailCount;
		public int InconclusiveCount;
		public int SkipCount;

		public string ResultState;
		public string StackTrace;

		public TestStatusAdaptor TestStatus;

		public int Parent;

		public TestResultAdaptor(object testResultAdaptor, int parent)
		{
			// 简化实现，避免依赖具体的测试框架类型
			Name = "test_name";
			FullName = "test_full_name";
			PassCount = 0;
			FailCount = 0;
			InconclusiveCount = 0;
			SkipCount = 0;
			ResultState = "Inconclusive";
			StackTrace = string.Empty;
			TestStatus = TestStatusAdaptor.Inconclusive;
			Parent = parent;
		}
	}
}
