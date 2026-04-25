using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tsinswreng.CsCore;

namespace Tsinswreng.CsTools;


public partial class GZipLinesUtf8 {
	/// 把異步行序列壓縮成 gzip 流；內容等價於以 '\n' 連接後再 gzip。
	/// 全程流式寫入，不把全部內容一次性載入內存。
	/// <param name="Lines">輸入行序列。</param>
	/// <param name="Ct">取消令牌。</param>
	/// <returns>可讀的壓縮流。</returns>
	public static async partial Task<Stream> ToStream(
		IAsyncEnumerable<str> Lines, CT Ct
	){
		if(Lines is null){
			throw new ArgumentNullException(nameof(Lines));
		}
		Ct.ThrowIfCancellationRequested();
		var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(Ct);
		var pipe = new Pipe();
		// producer 放到後台任務啟動，避免在返回 Stream 之前同步跑太多工作。
		var producerTask = Task.Run(async()=>{
			await WriteCompressedFromAsyncEnumerable(
				Lines: Lines,
				Writer: pipe.Writer,
				Ct: linkedCts.Token
			);
		}, linkedCts.Token);
		await Task.CompletedTask;
		return new ProducerBackedReadStream(
			Inner: pipe.Reader.AsStream(),
			ProducerTask: producerTask,
			Cts: linkedCts
		);
	}

	/// 把同步行序列壓縮成 gzip 流；內容等價於以 '\n' 連接後再 gzip。
	/// 全程流式寫入，不把全部內容一次性載入內存。
	/// <param name="Lines">輸入行序列。</param>
	/// <returns>可讀的壓縮流。</returns>
	public static partial Stream ToStream(
		IEnumerable<str> Lines
	){
		if(Lines is null){
			throw new ArgumentNullException(nameof(Lines));
		}
		var cts = new CancellationTokenSource();
		var pipe = new Pipe();
		// 同步源也放到後台任務，確保讀寫雙方真正以流式並發進行。
		var producerTask = Task.Run(async()=>{
			await WriteCompressedFromEnumerable(
				Lines: Lines,
				Writer: pipe.Writer,
				Ct: cts.Token
			);
		}, cts.Token);
		return new ProducerBackedReadStream(
			Inner: pipe.Reader.AsStream(),
			ProducerTask: producerTask,
			Cts: cts
		);
	}

	/// 解壓 gzip 流並按行懶加載返回。
	/// <param name="Stream">gzip 流。</param>
	/// <param name="Ct">取消令牌。</param>
	/// <returns>異步行序列。</returns>
	public static partial async IAsyncEnumerable<str> ToLines(
		Stream Stream, [System.Runtime.CompilerServices.EnumeratorCancellation] CT Ct
	){
		if(Stream is null){
			throw new ArgumentNullException(nameof(Stream));
		}

		using var gzip = new GZipStream(Stream, CompressionMode.Decompress, leaveOpen: true);
		using var reader = new StreamReader(gzip, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
		while(true){
			Ct.ThrowIfCancellationRequested();
			var line = await reader.ReadLineAsync(Ct);
			if(line is null){
				yield break;
			}
			yield return line;
		}
	}

	/// 後台任務：把異步行序列逐行寫入 gzip，再輸出到 PipeWriter。
	/// <param name="Lines">輸入行序列。</param>
	/// <param name="Writer">Pipe 寫端。</param>
	/// <param name="Ct">取消令牌。</param>
	/// <returns>完成任務。</returns>
	private static async Task<nil> WriteCompressedFromAsyncEnumerable(
		IAsyncEnumerable<str> Lines,
		PipeWriter Writer,
		CT Ct
	){
		Exception? err = null;
		try{
			await using var outStream = Writer.AsStream(leaveOpen: true);
			await using var gzip = new GZipStream(
				outStream, CompressionLevel.Optimal, leaveOpen: true
			);
			await using var writer = new StreamWriter(
				gzip, new UTF8Encoding(false), leaveOpen: true
			);
			var isFirst = true;
			await foreach(var line in Lines.WithCancellation(Ct)){
				Ct.ThrowIfCancellationRequested();
				if(!isFirst){
					await writer.WriteAsync('\n');
				}
				await writer.WriteAsync(line);
				isFirst = false;
			}
			await writer.FlushAsync(Ct);
		}catch(Exception e){
			err = e;
		}finally{
			await Writer.CompleteAsync(err);
		}
		return NIL;
	}

	/// 後台任務：把同步行序列逐行寫入 gzip，再輸出到 PipeWriter。
	/// <param name="Lines">輸入行序列。</param>
	/// <param name="Writer">Pipe 寫端。</param>
	/// <param name="Ct">取消令牌。</param>
	/// <returns>完成任務。</returns>
	private static async Task<nil> WriteCompressedFromEnumerable(
		IEnumerable<str> Lines,
		PipeWriter Writer,
		CT Ct
	){
		Exception? err = null;
		try{
			await using var outStream = Writer.AsStream(leaveOpen: true);
			await using var gzip = new GZipStream(
				outStream, CompressionLevel.Optimal, leaveOpen: true
			);
			await using var writer = new StreamWriter(
				gzip, new UTF8Encoding(false), leaveOpen: true
			);
			var isFirst = true;
			foreach(var line in Lines){
				Ct.ThrowIfCancellationRequested();
				if(!isFirst){
					await writer.WriteAsync('\n');
				}
				await writer.WriteAsync(line);
				isFirst = false;
			}
			await writer.FlushAsync(Ct);
		}catch(Exception e){
			err = e;
		}finally{
			await Writer.CompleteAsync(err);
		}
		return NIL;
	}

	/// 由後台 producer 任務驅動的只讀流。
	/// Dispose 時會取消 producer，避免後台任務懸掛。
	private sealed class ProducerBackedReadStream:Stream{
		private readonly Stream _inner;
		private readonly Task _producerTask;
		private readonly CancellationTokenSource _cts;
		private bool _disposed = false;

		/// 建立包裝流。
		/// <param name="Inner">底層可讀流。</param>
		/// <param name="ProducerTask">對應 producer 任務。</param>
		/// <param name="Cts">用於停止 producer 的 CTS。</param>
		public ProducerBackedReadStream(
			Stream Inner,
			Task ProducerTask,
			CancellationTokenSource Cts
		){
			_inner = Inner ?? throw new ArgumentNullException(nameof(Inner));
			_producerTask = ProducerTask ?? throw new ArgumentNullException(nameof(ProducerTask));
			_cts = Cts ?? throw new ArgumentNullException(nameof(Cts));
		}

		public override bool CanRead => !_disposed && _inner.CanRead;
		public override bool CanSeek => false;
		public override bool CanWrite => false;
		public override i64 Length => throw new NotSupportedException();
		public override i64 Position{
			get => throw new NotSupportedException();
			set => throw new NotSupportedException();
		}

		public override i32 Read(byte[] Buffer, i32 Offset, i32 Count){
			ThrowIfDisposed();
			return _inner.Read(Buffer, Offset, Count);
		}

		public override i32 Read(Span<byte> Buffer){
			ThrowIfDisposed();
			return _inner.Read(Buffer);
		}

		public override Task<i32> ReadAsync(
			byte[] Buffer, i32 Offset, i32 Count, CT Ct
		){
			ThrowIfDisposed();
			return _inner.ReadAsync(Buffer, Offset, Count, Ct);
		}

		public override ValueTask<i32> ReadAsync(Memory<byte> Buffer, CT Ct = default){
			ThrowIfDisposed();
			return _inner.ReadAsync(Buffer, Ct);
		}

		public override void Flush(){
		}

		public override Task FlushAsync(CT Ct){
			Ct.ThrowIfCancellationRequested();
			return Task.CompletedTask;
		}

		public override i64 Seek(i64 Offset, SeekOrigin Origin){
			throw new NotSupportedException();
		}

		public override void SetLength(i64 Value){
			throw new NotSupportedException();
		}

		public override void Write(byte[] Buffer, i32 Offset, i32 Count){
			throw new NotSupportedException();
		}

		protected override void Dispose(bool Disposing){
			if(_disposed){
				return;
			}
			if(Disposing){
				_cts.Cancel();
				_inner.Dispose();
				try{
					_producerTask.GetAwaiter().GetResult();
				}catch(OperationCanceledException){
				}
				_cts.Dispose();
			}
			_disposed = true;
			base.Dispose(Disposing);
		}

		public override async ValueTask DisposeAsync(){
			if(_disposed){
				return;
			}
			_cts.Cancel();
			await _inner.DisposeAsync();
			try{
				await _producerTask;
			}catch(OperationCanceledException){
			}
			_cts.Dispose();
			_disposed = true;
			await base.DisposeAsync();
		}

		private void ThrowIfDisposed(){
			ObjectDisposedException.ThrowIf(_disposed, this);
		}
	}
}
