using Tsinswreng.CsTools.Test.GZipLinesCases;
using Tsinswreng.CsTools.Test.ItblToStreamCases;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsTools.Test;

/// Test manager for this csproj.
/// It collects and registers all tester classes.
public class CsToolsTestMgr : DiEtTestMgr {
	public static CsToolsTestMgr Inst = new();

	/// Register all testers into root test node.
	/// <param name="Test">Optional input node (root node is used in practice).</param>
	/// <returns>Root node after tester registration.</returns>
	public override ITestNode RegisterTestsInto(ITestNode? Test) {
		Test = this.TestNode;
		this.RegisterTester<TestGZipLinesUtf8>();
		this.RegisterTester<TestItblToStream>();
		return Test;
	}
}

