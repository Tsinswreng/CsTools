using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Tsinswreng.CsCore;

namespace Tsinswreng.CsTools;

[Doc(@$"
Stream's whole content is equivalent to
`str.Join(Lines, '\n').GZip()`
")]
public partial class GZipLinesUtf8 {
	public static partial Task<Stream> ToStream(
		IAsyncEnumerable<str> Lines, CT Ct
	);

	public static partial Stream ToStream(
		IEnumerable<str> Lines
	);
	
	public static partial IAsyncEnumerable<str> ToLines(
		Stream Stream, CT Ct
	);
	
}
