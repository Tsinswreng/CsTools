using System.Text;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsTools.Test.ItblToStreamCases;

/// <summary>
/// Cases for <see cref="Tsinswreng.CsTools.ItblToStream{T}.ToStream(IEnumerable{T})"/>.
/// </summary>
public partial class TestItblToStream {
	/// <summary>
	/// Register ToStream(Enumerable) test cases.
	/// </summary>
	/// <param name="Node">Target node.</param>
	public void RegisterToStreamFromEnumerable(ITestNode Node) {
		var register = Node.MkTestFnRegister(
			typeof(TestItblToStream),
			[typeof(Tsinswreng.CsTools.ItblToStream<string>)],
			[nameof(Tsinswreng.CsTools.ItblToStream<string>.ToStream)],
			"ToStream(Enumerable):"
		);
		var r = register.Register;

		r("concatenates bytes in source order", async _ => {
			var sut = new Tsinswreng.CsTools.ItblToStream<string>(Encoding.UTF8.GetBytes);
			var input = new[] { "ab", "中", "!" };
			using var stream = sut.ToStream(input);
			var actual = ReadAllText(stream);
			AssertEqual("ab中!", actual, "ToStreamEnumerable/concatenate");
			return null;
		});
	}
}
