using System.IO.Compression;
using System.Text;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsTools.Test.GZipLinesCases;

/// <summary>
/// Main tester for <see cref="Tsinswreng.CsTools.GZipLinesUtf8"/>.
/// Each partial file below registers one target method's cases.
/// </summary>
public partial class TestGZipLinesUtf8 : ITester {
	/// <summary>
	/// Register all method-level test groups.
	/// </summary>
	/// <param name="Node">Optional node to register into.</param>
	/// <returns>The node after registration.</returns>
	public ITestNode RegisterTestsInto(ITestNode? Node) {
		Node ??= new TestNode();
		Node.Ordered = false;
		Node.IsParallelRecursive = false;
		RegisterToLines(Node);
		RegisterToStreamFromEnumerable(Node);
		RegisterToStreamFromAsyncEnumerable(Node);
		return Node;
	}

	/// <summary>
	/// Decompress gzip stream and read all text as UTF-8.
	/// </summary>
	/// <param name="Compressed">Compressed stream from testee.</param>
	/// <returns>Decompressed UTF-8 text.</returns>
	private static string DecompressToString(Stream Compressed) {
		Compressed.Position = 0;
		using var gzip = new GZipStream(Compressed, CompressionMode.Decompress, leaveOpen: true);
		using var reader = new StreamReader(gzip, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
		return reader.ReadToEnd();
	}

	/// <summary>
	/// Assertion helper with case label.
	/// </summary>
	/// <typeparam name="T">Compared value type.</typeparam>
	/// <param name="Expected">Expected value.</param>
	/// <param name="Actual">Actual value.</param>
	/// <param name="CaseName">Readable case name.</param>
	private static void AssertEqual<T>(T Expected, T Actual, string CaseName) {
		if(!EqualityComparer<T>.Default.Equals(Expected, Actual)) {
			throw new Exception($"Case '{CaseName}' failed. Expected: {Expected}; Actual: {Actual}.");
		}
	}

	/// <summary>
	/// Assert sequence equality in order.
	/// </summary>
	/// <typeparam name="T">Element type.</typeparam>
	/// <param name="Expected">Expected sequence.</param>
	/// <param name="Actual">Actual sequence.</param>
	/// <param name="CaseName">Readable case name.</param>
	private static void AssertSequenceEqual<T>(IReadOnlyList<T> Expected, IReadOnlyList<T> Actual, string CaseName) {
		if(Expected.Count != Actual.Count) {
			throw new Exception($"Case '{CaseName}' failed. Sequence length mismatch: expected {Expected.Count}, actual {Actual.Count}.");
		}
		for(var i = 0; i < Expected.Count; i++) {
			if(!EqualityComparer<T>.Default.Equals(Expected[i], Actual[i])) {
				throw new Exception($"Case '{CaseName}' failed at index {i}. Expected: {Expected[i]}; Actual: {Actual[i]}.");
			}
		}
	}
}

