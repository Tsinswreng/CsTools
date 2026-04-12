using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsTools.Test.GZipLinesCases;

/// <summary>
/// Cases for <see cref="Tsinswreng.CsTools.GZipLinesUtf8.ToLines(Stream, CancellationToken)"/>.
/// </summary>
public partial class TestGZipLinesUtf8 {
	/// <summary>
	/// Register ToLines test cases.
	/// </summary>
	/// <param name="Node">Target node.</param>
	public void RegisterToLines(ITestNode Node) {
		var register = Node.MkTestFnRegister(
			typeof(TestGZipLinesUtf8),
			[typeof(Tsinswreng.CsTools.GZipLinesUtf8)],
			[nameof(Tsinswreng.CsTools.GZipLinesUtf8.ToLines)],
			"ToLines:"
		);
		var r = register.Register;

		r("decompresses back to original lines", async _ => {
			var source = new[] { "a", "中", "c" };
			using var compressed = Tsinswreng.CsTools.GZipLinesUtf8.ToStream(source, CancellationToken.None);
			var actual = new List<string>();
			await foreach(var line in Tsinswreng.CsTools.GZipLinesUtf8.ToLines(compressed, CancellationToken.None)) {
				actual.Add(line);
			}
			AssertSequenceEqual(source, actual, "ToLines/decompresses-back");
			return null;
		});
	}
}

