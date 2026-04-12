using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsTools.Test.GZipLinesCases;

/// <summary>
/// Cases for <see cref="Tsinswreng.CsTools.GZipLinesUtf8.ToStream(IEnumerable{string}, CancellationToken)"/>.
/// </summary>
public partial class TestGZipLinesUtf8 {
	/// <summary>
	/// Register ToStream(Enumerable) test cases.
	/// </summary>
	/// <param name="Node">Target node.</param>
	public void RegisterToStreamFromEnumerable(ITestNode Node) {
		var register = Node.MkTestFnRegister(
			typeof(TestGZipLinesUtf8),
			[typeof(Tsinswreng.CsTools.GZipLinesUtf8)],
			[nameof(Tsinswreng.CsTools.GZipLinesUtf8.ToStream)],
			"ToStream(Enumerable):"
		);
		var r = register.Register;

		r("gzip content equals newline-joined input", async _ => {
			var lines = new[] { "alpha", "中", "omega" };
			using var stream = Tsinswreng.CsTools.GZipLinesUtf8.ToStream(lines, CancellationToken.None);
			var actual = DecompressToString(stream);
			AssertEqual("alpha\n中\nomega", actual, "ToStreamEnumerable/join-with-lf");
			return null;
		});
	}
}

