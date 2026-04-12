using System.IO.Compression;
using System.Text;

namespace Tsinswreng.CsTools.Test.GZipLinesCases;

public class GZipLinesUtf8_ToStreamAsyncEnumerable_Tests {
	[Fact]
	public void ToStream_AsyncEnumerable_EqualsJoinWithLfThenGzip() {
		using var stream = Tsinswreng.CsTools.GZipLinesUtf8.ToStream(GetLines(), CancellationToken.None);
		var actual = DecompressToString(stream);
		Assert.Equal("l1\n第二行\nl3", actual);
	}

	private static async IAsyncEnumerable<string> GetLines() {
		yield return "l1";
		await Task.Yield();
		yield return "第二行";
		await Task.Yield();
		yield return "l3";
	}

	private static string DecompressToString(Stream compressed) {
		compressed.Position = 0;
		using var gzip = new GZipStream(compressed, CompressionMode.Decompress, leaveOpen: true);
		using var reader = new StreamReader(gzip, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
		return reader.ReadToEnd();
	}
}
