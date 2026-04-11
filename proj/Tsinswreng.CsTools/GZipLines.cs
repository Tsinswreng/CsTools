using Tsinswreng.CsCore;

namespace Tsinswreng.CsTools;

[Doc(@$"
Stream's whole content is equivalent to
`str.Join(Lines, '\n').GZip()`
")]
public class GZipLinesUtf8{
	public static Stream ToStream(
		IAsyncEnumerable<str> Lines, CT Ct
	);
	public static Stream ToStream(
		IEnumerable<str> Lines, CT Ct
	);
	
	public static IAsyncEnumerable<str> ToLines(
		Stream Stream, CT Ct
	);
}


