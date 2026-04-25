using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Tsinswreng.CsTools;

/// <summary>
/// 把可迭代元素流式轉成可讀取的 <see cref="Stream"/>。
/// 不在內存中一次性聚合全部字節。
/// </summary>
/// <typeparam name="T">源元素類型。</typeparam>
public partial class ItblToStream<T>{
	/// <summary>
	/// 建立轉換器。
	/// </summary>
	/// <param name="FnToBytes">把單個元素轉成字節塊的函數。</param>
	public ItblToStream(Func<T, ReadOnlyMemory<byte>> FnToBytes){
		this.FnToBytes = FnToBytes ?? throw new ArgumentNullException(nameof(FnToBytes));
	}

	/// <summary>
	/// 把異步可迭代源轉成流式讀取的 <see cref="Stream"/>。
	/// </summary>
	/// <param name="AsyE">異步源。</param>
	/// <param name="Ct">取消令牌。</param>
	/// <returns>僅可讀、不支持 seek 的流。</returns>
	public async partial Task<Stream> ToStream(IAsyncEnumerable<T> AsyE, CT Ct){
		if(AsyE is null){
			throw new ArgumentNullException(nameof(AsyE));
		}
		Ct.ThrowIfCancellationRequested();
		// 先預讀一塊，保證此異步函數具備真實 await 流程，
		// 同時讓返回的 Stream 在首次 Read 前就可立即輸出已知數據。
		var enumerator = AsyE.GetAsyncEnumerator(Ct);
		try{
			var firstChunk = await TryReadNextChunk(enumerator, FnToBytes, Ct);
			return new AsyncEnumerableReadStream(
				Enumerator: enumerator,
				FnToBytes: FnToBytes,
				FirstChunk: firstChunk,
				Ct: Ct
			);
		}catch{
			await enumerator.DisposeAsync();
			throw;
		}
	}

	/// <summary>
	/// 把同步可迭代源轉成流式讀取的 <see cref="Stream"/>。
	/// </summary>
	/// <param name="Itbl">同步源。</param>
	/// <returns>僅可讀、不支持 seek 的流。</returns>
	public partial Stream ToStream(IEnumerable<T> Itbl){
		if(Itbl is null){
			throw new ArgumentNullException(nameof(Itbl));
		}
		return new EnumerableReadStream(Itbl.GetEnumerator(), FnToBytes);
	}

	/// <summary>
	/// 從異步枚舉器取下一個非空字節塊；若無更多元素則返回空。
	/// </summary>
	/// <param name="Enumerator">異步枚舉器。</param>
	/// <param name="Ct">取消令牌。</param>
	/// <returns>下一個可讀取的字節塊；無數據則為空 memory。</returns>
	private static async Task<ReadOnlyMemory<byte>> TryReadNextChunk(
		IAsyncEnumerator<T> Enumerator,
		Func<T, ReadOnlyMemory<byte>> FnToBytes,
		CT Ct
	){
		while(await Enumerator.MoveNextAsync()){
			Ct.ThrowIfCancellationRequested();
			var chunk = FnToBytes(Enumerator.Current);
			if(chunk.Length <= 0){
				continue;
			}
			return chunk;
		}
		return ReadOnlyMemory<byte>.Empty;
	}

	/// <summary>
	/// 對同步 <see cref="IEnumerable{T}"/> 的流式讀取器。
	/// </summary>
	private sealed class EnumerableReadStream:Stream{
		private readonly IEnumerator<T> _enumerator;
		private readonly Func<T, ReadOnlyMemory<byte>> _fnToBytes;
		private ReadOnlyMemory<byte> _chunk = ReadOnlyMemory<byte>.Empty;
		private i32 _chunkOffset = 0;
		private bool _sourceEnded = false;
		private bool _disposed = false;

		/// <summary>
		/// 建立同步源讀取流。
		/// </summary>
		/// <param name="Enumerator">同步枚舉器。</param>
		/// <param name="FnToBytes">元素轉字節函數。</param>
		public EnumerableReadStream(
			IEnumerator<T> Enumerator,
			Func<T, ReadOnlyMemory<byte>> FnToBytes
		){
			_enumerator = Enumerator ?? throw new ArgumentNullException(nameof(Enumerator));
			_fnToBytes = FnToBytes ?? throw new ArgumentNullException(nameof(FnToBytes));
		}

		public override bool CanRead => !_disposed;
		public override bool CanSeek => false;
		public override bool CanWrite => false;
		public override i64 Length => throw new NotSupportedException();
		public override i64 Position{
			get => throw new NotSupportedException();
			set => throw new NotSupportedException();
		}

		public override i32 Read(byte[] Buffer, i32 Offset, i32 Count){
			ArgumentNullException.ThrowIfNull(Buffer);
			ArgumentOutOfRangeException.ThrowIfNegative(Offset);
			ArgumentOutOfRangeException.ThrowIfNegative(Count);
			if(Buffer.Length - Offset < Count){
				throw new ArgumentException("Invalid offset/count for destination buffer.");
			}
			return Read(Buffer.AsSpan(Offset, Count));
		}

		public override i32 Read(Span<byte> Buffer){
			ThrowIfDisposed();
			if(Buffer.Length <= 0){
				return 0;
			}
			var totalWritten = 0;
			while(totalWritten < Buffer.Length){
				if(!EnsureChunk()){
					break;
				}
				var remaining = Buffer.Length - totalWritten;
				var available = _chunk.Length - _chunkOffset;
				var toCopy = Math.Min(remaining, available);
				_chunk.Span.Slice(_chunkOffset, toCopy).CopyTo(
					Buffer.Slice(totalWritten, toCopy)
				);
				_chunkOffset += toCopy;
				totalWritten += toCopy;
			}
			return totalWritten;
		}

		public override ValueTask<i32> ReadAsync(
			Memory<byte> Buffer, CT Ct = default
		){
			Ct.ThrowIfCancellationRequested();
			return ValueTask.FromResult(Read(Buffer.Span));
		}

		public override Task<i32> ReadAsync(
			byte[] Buffer, i32 Offset, i32 Count, CT Ct
		){
			Ct.ThrowIfCancellationRequested();
			return Task.FromResult(Read(Buffer, Offset, Count));
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

		private bool EnsureChunk(){
			if(_chunkOffset < _chunk.Length){
				return true;
			}
			if(_sourceEnded){
				return false;
			}
			while(_enumerator.MoveNext()){
				var chunk = _fnToBytes(_enumerator.Current);
				if(chunk.Length <= 0){
					continue;
				}
				_chunk = chunk;
				_chunkOffset = 0;
				return true;
			}
			_sourceEnded = true;
			_chunk = ReadOnlyMemory<byte>.Empty;
			_chunkOffset = 0;
			return false;
		}

		private void ThrowIfDisposed(){
			ObjectDisposedException.ThrowIf(_disposed, this);
		}

		protected override void Dispose(bool Disposing){
			if(_disposed){
				return;
			}
			if(Disposing){
				_enumerator.Dispose();
			}
			_disposed = true;
			base.Dispose(Disposing);
		}
	}

	/// <summary>
	/// 對異步 <see cref="IAsyncEnumerable{T}"/> 的流式讀取器。
	/// </summary>
	private sealed class AsyncEnumerableReadStream:Stream{
		private readonly IAsyncEnumerator<T> _enumerator;
		private readonly Func<T, ReadOnlyMemory<byte>> _fnToBytes;
		private readonly CT _ct;
		private ReadOnlyMemory<byte> _chunk;
		private i32 _chunkOffset = 0;
		private bool _sourceEnded = false;
		private bool _disposed = false;

		/// <summary>
		/// 建立異步源讀取流。
		/// </summary>
		/// <param name="Enumerator">異步枚舉器。</param>
		/// <param name="FnToBytes">元素轉字節函數。</param>
		/// <param name="FirstChunk">預讀到的首塊；可為空。</param>
		/// <param name="Ct">與源綁定的取消令牌。</param>
		public AsyncEnumerableReadStream(
			IAsyncEnumerator<T> Enumerator,
			Func<T, ReadOnlyMemory<byte>> FnToBytes,
			ReadOnlyMemory<byte> FirstChunk,
			CT Ct
		){
			_enumerator = Enumerator ?? throw new ArgumentNullException(nameof(Enumerator));
			_fnToBytes = FnToBytes ?? throw new ArgumentNullException(nameof(FnToBytes));
			_chunk = FirstChunk;
			_sourceEnded = FirstChunk.Length <= 0;
			_ct = Ct;
		}

		public override bool CanRead => !_disposed;
		public override bool CanSeek => false;
		public override bool CanWrite => false;
		public override i64 Length => throw new NotSupportedException();
		public override i64 Position{
			get => throw new NotSupportedException();
			set => throw new NotSupportedException();
		}

		public override i32 Read(byte[] Buffer, i32 Offset, i32 Count){
			ArgumentNullException.ThrowIfNull(Buffer);
			ArgumentOutOfRangeException.ThrowIfNegative(Offset);
			ArgumentOutOfRangeException.ThrowIfNegative(Count);
			if(Buffer.Length - Offset < Count){
				throw new ArgumentException("Invalid offset/count for destination buffer.");
			}
			return Read(Buffer.AsSpan(Offset, Count));
		}

		public override i32 Read(Span<byte> Buffer){
			ThrowIfDisposed();
			_ct.ThrowIfCancellationRequested();
			if(Buffer.Length <= 0){
				return 0;
			}
			var totalWritten = 0;
			while(totalWritten < Buffer.Length){
				if(!EnsureChunkSync()){
					break;
				}
				var remaining = Buffer.Length - totalWritten;
				var available = _chunk.Length - _chunkOffset;
				var toCopy = Math.Min(remaining, available);
				_chunk.Span.Slice(_chunkOffset, toCopy).CopyTo(
					Buffer.Slice(totalWritten, toCopy)
				);
				_chunkOffset += toCopy;
				totalWritten += toCopy;
			}
			return totalWritten;
		}

		public override async ValueTask<i32> ReadAsync(
			Memory<byte> Buffer, CT Ct = default
		){
			ThrowIfDisposed();
			_ct.ThrowIfCancellationRequested();
			Ct.ThrowIfCancellationRequested();
			if(Buffer.Length <= 0){
				return 0;
			}
			var totalWritten = 0;
			while(totalWritten < Buffer.Length){
				if(!await EnsureChunkAsync(Ct)){
					break;
				}
				var remaining = Buffer.Length - totalWritten;
				var available = _chunk.Length - _chunkOffset;
				var toCopy = Math.Min(remaining, available);
				_chunk.Slice(_chunkOffset, toCopy).CopyTo(
					Buffer.Slice(totalWritten, toCopy)
				);
				_chunkOffset += toCopy;
				totalWritten += toCopy;
			}
			return totalWritten;
		}

		public override Task<i32> ReadAsync(
			byte[] Buffer, i32 Offset, i32 Count, CT Ct
		){
			ArgumentNullException.ThrowIfNull(Buffer);
			ArgumentOutOfRangeException.ThrowIfNegative(Offset);
			ArgumentOutOfRangeException.ThrowIfNegative(Count);
			if(Buffer.Length - Offset < Count){
				throw new ArgumentException("Invalid offset/count for destination buffer.");
			}
			return ReadAsync(Buffer.AsMemory(Offset, Count), Ct).AsTask();
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

		private bool EnsureChunkSync(){
			if(_chunkOffset < _chunk.Length){
				return true;
			}
			if(_sourceEnded){
				return false;
			}
			while(_enumerator.MoveNextAsync().AsTask().GetAwaiter().GetResult()){
				_ct.ThrowIfCancellationRequested();
				var chunk = _fnToBytes(_enumerator.Current);
				if(chunk.Length <= 0){
					continue;
				}
				_chunk = chunk;
				_chunkOffset = 0;
				return true;
			}
			_sourceEnded = true;
			_chunk = ReadOnlyMemory<byte>.Empty;
			_chunkOffset = 0;
			return false;
		}

		private async ValueTask<bool> EnsureChunkAsync(CT Ct){
			if(_chunkOffset < _chunk.Length){
				return true;
			}
			if(_sourceEnded){
				return false;
			}
			while(await _enumerator.MoveNextAsync()){
				_ct.ThrowIfCancellationRequested();
				Ct.ThrowIfCancellationRequested();
				var chunk = _fnToBytes(_enumerator.Current);
				if(chunk.Length <= 0){
					continue;
				}
				_chunk = chunk;
				_chunkOffset = 0;
				return true;
			}
			_sourceEnded = true;
			_chunk = ReadOnlyMemory<byte>.Empty;
			_chunkOffset = 0;
			return false;
		}

		private void ThrowIfDisposed(){
			ObjectDisposedException.ThrowIf(_disposed, this);
		}

		protected override void Dispose(bool Disposing){
			if(_disposed){
				return;
			}
			if(Disposing){
				try{
					_enumerator.DisposeAsync().AsTask().GetAwaiter().GetResult();
				}catch(OperationCanceledException){
					// Dispose 時若源已取消，不再向外擴散。
				}
			}
			_disposed = true;
			base.Dispose(Disposing);
		}

		public override async ValueTask DisposeAsync(){
			if(_disposed){
				return;
			}
			try{
				await _enumerator.DisposeAsync();
			}catch(OperationCanceledException){
				// 與同步 Dispose 保持一致：取消視為已結束清理。
			}
			_disposed = true;
			await base.DisposeAsync();
		}
	}
}
