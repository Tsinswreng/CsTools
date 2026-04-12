namespace Tsinswreng.CsTools.Test.GZipLinesCases;

public class GZipLinesUtf8_ToLines_Tests {
	[Fact]
	public async Task ToLines_DecompressesBackToOriginalLines() {
		var source = new[] { "a", "中", "c" };
		using var compressed = Tsinswreng.CsTools.GZipLinesUtf8.ToStream(source, CancellationToken.None);
		var actual = new List<string>();
		await foreach(var line in Tsinswreng.CsTools.GZipLinesUtf8.ToLines(compressed, CancellationToken.None)) {
			actual.Add(line);
		}
		Assert.Equal(source, actual);
	}
}
