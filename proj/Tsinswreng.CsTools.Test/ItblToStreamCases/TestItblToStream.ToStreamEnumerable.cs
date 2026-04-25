using System.Text;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsTools.Test.ItblToStreamCases;

/// <summary>
/// Cases for <see cref="Tsinswreng.CsTools.ItblToStreamOld{T}.ToStream(IEnumerable{T}, CancellationToken)"/>.
/// </summary>
public partial class TestItblToStream {
	/// <summary>
	/// Register ToStream(Enumerable) test cases.
	/// </summary>
	/// <param name="Node">Target node.</param>
	public void RegisterToStreamFromEnumerable(ITestNode Node) {
		var register = Node.MkTestFnRegister(
			typeof(TestItblToStream),
			[typeof(Tsinswreng.CsTools.ItblToStreamOld<string>)],
			[nameof(Tsinswreng.CsTools.ItblToStreamOld<string>.ToStream)],
			"ToStream(Enumerable):"
		);
		var r = register.Register;

		r("concatenates bytes in source order", async _ => {
			var sut = new Tsinswreng.CsTools.ItblToStreamOld<string>(Encoding.UTF8.GetBytes);
			var input = new[] { "ab", "中", "!" };
			using var stream = sut.ToStream(input, CancellationToken.None);
			var actual = ReadAllText(stream);
			AssertEqual("ab中!", actual, "ToStreamEnumerable/concatenate");
			return null;
		});
	}
}

