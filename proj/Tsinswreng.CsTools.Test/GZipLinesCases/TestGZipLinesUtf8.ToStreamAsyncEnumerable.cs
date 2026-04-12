using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsTools.Test.GZipLinesCases;

/// <summary>
/// Cases for <see cref="Tsinswreng.CsTools.GZipLinesUtf8.ToStream(IAsyncEnumerable{string}, CancellationToken)"/>.
/// </summary>
public partial class TestGZipLinesUtf8 {
	/// <summary>
	/// Register ToStream(AsyncEnumerable) test cases.
	/// </summary>
	/// <param name="Node">Target node.</param>
	public void RegisterToStreamFromAsyncEnumerable(ITestNode Node) {
		var register = Node.MkTestFnRegister(
			typeof(TestGZipLinesUtf8),
			[typeof(Tsinswreng.CsTools.GZipLinesUtf8)],
			[nameof(Tsinswreng.CsTools.GZipLinesUtf8.ToStream)],
			"ToStream(AsyncEnumerable):"
		);
		var r = register.Register;

		r("gzip content equals newline-joined async input", async _ => {
			using var stream = Tsinswreng.CsTools.GZipLinesUtf8.ToStream(GetLines(), CancellationToken.None);
			var actual = DecompressToString(stream);
			AssertEqual("l1\n第二行\nl3", actual, "ToStreamAsyncEnumerable/join-with-lf");
			return null;
		});
	}

	/// <summary>
	/// Async source enumerable for compression test.
	/// </summary>
	/// <returns>Three lines yielded asynchronously.</returns>
	private static async IAsyncEnumerable<string> GetLines() {
		yield return "l1";
		await Task.Yield();
		yield return "第二行";
		await Task.Yield();
		yield return "l3";
	}
}

