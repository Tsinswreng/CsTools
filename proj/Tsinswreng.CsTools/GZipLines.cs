using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Tsinswreng.CsCore;

namespace Tsinswreng.CsTools;


public class GZipLinesUtf8 {
	/// Compress UTF-8 lines as one gzip stream, equivalent to joining lines with '\n' then gzip.
	/// <param name="Lines">Input lines.</param>
	/// <param name="Ct">Cancellation token.</param>
	/// <returns>Readable stream positioned at 0.</returns>
	public static Task<Stream> ToStream(
		IAsyncEnumerable<str> Lines, CT Ct
	) {
		if(Lines is null) {
			throw new ArgumentNullException(nameof(Lines));
		}

		var output = new MemoryStream();//TODO: 不應用 MemoryStream
		using(var gzip = new GZipStream(output, CompressionLevel.Optimal, leaveOpen: true))
		using(var writer = new StreamWriter(gzip, new UTF8Encoding(false), leaveOpen: true)) {
			var e = Lines.GetAsyncEnumerator(Ct);
			try {
				var first = true;
				while(e.MoveNextAsync().AsTask().GetAwaiter().GetResult()) {
					Ct.ThrowIfCancellationRequested();
					if(!first) {
						writer.Write('\n');
					}
					writer.Write(e.Current);
					first = false;
				}
			} finally {
				e.DisposeAsync().AsTask().GetAwaiter().GetResult();
			}
		}
		output.Position = 0;
		return output;
	}

	/// Compress UTF-8 lines as one gzip stream, equivalent to joining lines with '\n' then gzip.
	/// <param name="Lines">Input lines.</param>
	/// <param name="Ct">Cancellation token.</param>
	/// <returns>Readable stream positioned at 0.</returns>
	public static Stream ToStream(
		IEnumerable<str> Lines, CT Ct
	) {
		if(Lines is null) {
			throw new ArgumentNullException(nameof(Lines));
		}

		var output = new MemoryStream();
		using(var gzip = new GZipStream(output, CompressionLevel.Optimal, leaveOpen: true))
		using(var writer = new StreamWriter(gzip, new UTF8Encoding(false), leaveOpen: true)) {
			var first = true;
			foreach(var line in Lines) {
				Ct.ThrowIfCancellationRequested();
				if(!first) {
					writer.Write('\n');
				}
				writer.Write(line);
				first = false;
			}
		}
		output.Position = 0;
		return output;
	}

	/// Decompress a gzip stream and lazily yield lines split by line breaks.
	/// <param name="Stream">Gzip stream.</param>
	/// <param name="Ct">Cancellation token.</param>
	/// <returns>Async line sequence.</returns>
	public static async IAsyncEnumerable<str> ToLines(
		Stream Stream, [System.Runtime.CompilerServices.EnumeratorCancellation] CT Ct
	) {
		if(Stream is null) {
			throw new ArgumentNullException(nameof(Stream));
		}

		using var gzip = new GZipStream(Stream, CompressionMode.Decompress, leaveOpen: true);
		using var reader = new StreamReader(gzip, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
		while(true) {
			Ct.ThrowIfCancellationRequested();
			var line = await reader.ReadLineAsync(Ct);
			if(line is null) {
				yield break;
			}
			yield return line;
		}
	}
}
