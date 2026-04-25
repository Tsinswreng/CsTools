using System.Text;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsTools.Test.ItblToStreamCases;

/// <summary>
/// Cases for <see cref="Tsinswreng.CsTools.ItblToStream{T}.ToStream(IAsyncEnumerable{T}, CancellationToken)"/>.
/// </summary>
public partial class TestItblToStream {
	/// <summary>
	/// Register ToStream(AsyncEnumerable) test cases.
	/// </summary>
	/// <param name="Node">Target node.</param>
	public void RegisterToStreamFromAsyncEnumerable(ITestNode Node) {
		var register = Node.MkTestFnRegister(
			typeof(TestItblToStream),
			[typeof(Tsinswreng.CsTools.ItblToStream<string>)],
			[nameof(Tsinswreng.CsTools.ItblToStream<string>.ToStream)],
			"ToStream(AsyncEnumerable):"
		);
		var r = register.Register;

		r("concatenates async bytes in source order", async _ => {
			var sut = new Tsinswreng.CsTools.ItblToStream<string>(Encoding.UTF8.GetBytes);
			using var stream = await sut.ToStream(GetInput(), CancellationToken.None);
			var actual = ReadAllText(stream);
			AssertEqual("x中y", actual, "ToStreamAsyncEnumerable/concatenate");
			return null;
		});
	}

	/// <summary>
	/// Async source data for overload verification.
	/// </summary>
	/// <returns>Input values yielded asynchronously.</returns>
	private static async IAsyncEnumerable<string> GetInput() {
		yield return "x";
		await Task.Yield();
		yield return "中";
		await Task.Yield();
		yield return "y";
	}
}
