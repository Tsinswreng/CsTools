using System.IO.Compression;
using System.Text;

namespace Tsinswreng.CsTools.Test.GZipLinesCases;

public class GZipLinesUtf8_ToStreamEnumerable_Tests {
	[Fact]
	public void ToStream_Enumerable_EqualsJoinWithLfThenGzip() {
		var lines = new[] { "alpha", "中", "omega" };
		using var stream = Tsinswreng.CsTools.GZipLinesUtf8.ToStream(lines, CancellationToken.None);
		var actual = DecompressToString(stream);
		Assert.Equal("alpha\n中\nomega", actual);
	}

	private static string DecompressToString(Stream compressed) {
		compressed.Position = 0;
		using var gzip = new GZipStream(compressed, CompressionMode.Decompress, leaveOpen: true);
		using var reader = new StreamReader(gzip, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
		return reader.ReadToEnd();
	}
}
