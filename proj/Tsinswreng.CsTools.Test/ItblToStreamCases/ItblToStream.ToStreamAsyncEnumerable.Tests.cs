using System.Text;

namespace Tsinswreng.CsTools.Test.ItblToStreamCases;

public class ItblToStream_ToStreamAsyncEnumerable_Tests {
	[Fact]
	public void ToStream_AsyncEnumerable_ConcatenatesBytesInOrder() {
		var sut = new Tsinswreng.CsTools.ItblToStream<string>(Encoding.UTF8.GetBytes);
		using var stream = sut.ToStream(GetInput(), CancellationToken.None);
		using var ms = new MemoryStream();
		stream.CopyTo(ms);
		var actual = Encoding.UTF8.GetString(ms.ToArray());
		Assert.Equal("x中y", actual);
	}

	private static async IAsyncEnumerable<string> GetInput() {
		yield return "x";
		await Task.Yield();
		yield return "中";
		await Task.Yield();
		yield return "y";
	}
}
