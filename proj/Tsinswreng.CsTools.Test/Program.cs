using Microsoft.Extensions.DependencyInjection;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsTools.Test;

/// <summary>
/// This is the executable entrypoint of the test assembly.
/// It initializes DI and runs the CsTreeTest node tree.
/// </summary>
public static class Program {
	/// <summary>
	/// Launch all tests registered under <see cref="CsToolsTestMgr"/>.
	/// </summary>
	/// <param name="Args">Command-line arguments (currently unused).</param>
	public static async Task Main(string[] Args) {
		_ = Args;
		IServiceCollection svcColct = new ServiceCollection();
		var mgr = CsToolsTestMgr.Inst;
		_ = mgr.InitSvc(svcColct, sc => sc.BuildServiceProvider());
		ITestExecutor executor = new TreeTestExecutor();
		await executor.RunEtPrint(mgr.TestNode);
	}
}

