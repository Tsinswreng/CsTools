using System.Text;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsTools.Test.ItblToStreamCases;

/// <summary>
/// Main tester for <see cref="Tsinswreng.CsTools.ItblToStream{T}"/>.
/// Each partial file below covers one overload.
/// </summary>
public partial class TestItblToStream : ITester {
	/// <summary>
	/// Register all method-level test groups.
	/// </summary>
	/// <param name="Node">Optional node to register into.</param>
	/// <returns>The node after registration.</returns>
	public ITestNode RegisterTestsInto(ITestNode? Node) {
		Node ??= new TestNode();
		Node.Ordered = false;
		Node.IsParallelRecursive = false;
		RegisterToStreamFromEnumerable(Node);
		RegisterToStreamFromAsyncEnumerable(Node);
		return Node;
	}

	/// <summary>
	/// Decode all stream bytes as UTF-8 text for readable assertion.
	/// </summary>
	/// <param name="Input">Result stream from testee.</param>
	/// <returns>Decoded UTF-8 text.</returns>
	private static string ReadAllText(Stream Input) {
		using var ms = new MemoryStream();
		Input.CopyTo(ms);
		return Encoding.UTF8.GetString(ms.ToArray());
	}

	/// <summary>
	/// Assertion helper with case label.
	/// </summary>
	/// <param name="Expected">Expected text.</param>
	/// <param name="Actual">Actual text.</param>
	/// <param name="CaseName">Readable case name.</param>
	private static void AssertEqual(string Expected, string Actual, string CaseName) {
		if(!string.Equals(Expected, Actual, StringComparison.Ordinal)) {
			throw new Exception($"Case '{CaseName}' failed. Expected: {Expected}; Actual: {Actual}.");
		}
	}
}

