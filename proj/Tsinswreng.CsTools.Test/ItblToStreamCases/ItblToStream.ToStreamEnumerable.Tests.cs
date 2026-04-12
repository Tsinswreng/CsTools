using System.Text;

namespace Tsinswreng.CsTools.Test.ItblToStreamCases;

public class ItblToStream_ToStreamEnumerable_Tests {
	[Fact]
	public void ToStream_Enumerable_ConcatenatesBytesInOrder() {
		var sut = new Tsinswreng.CsTools.ItblToStream<string>(Encoding.UTF8.GetBytes);
		var input = new[] { "ab", "中", "!" };
		using var stream = sut.ToStream(input, CancellationToken.None);
		using var ms = new MemoryStream();
		stream.CopyTo(ms);
		var actual = Encoding.UTF8.GetString(ms.ToArray());
		Assert.Equal("ab中!", actual);
	}
}
