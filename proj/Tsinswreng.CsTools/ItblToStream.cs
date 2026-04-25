using System;
using System.Collections.Generic;
using System.IO;

namespace Tsinswreng.CsTools;

[Obsolete]
public class ItblToStreamOld<T>{
	public ItblToStreamOld(Func<T, byte[]> FnToBytes){
		this.FnToBytes = FnToBytes;
	}
	public Func<T, byte[]> FnToBytes{get;set;}
	public Stream ToStream(IAsyncEnumerable<T> AsyE, CT Ct){
		if(AsyE is null){
			throw new ArgumentNullException(nameof(AsyE));
		}
		var ms = new MemoryStream();//TODO 會撐爆內存 不符合代碼規範
		var enumerator = AsyE.GetAsyncEnumerator(Ct);
		try{
			while(enumerator.MoveNextAsync().AsTask().GetAwaiter().GetResult()){
				Ct.ThrowIfCancellationRequested();
				var bytes = FnToBytes(enumerator.Current) ?? [];
				ms.Write(bytes, 0, bytes.Length);
			}
		}finally{
			enumerator.DisposeAsync().AsTask().GetAwaiter().GetResult();
		}
		ms.Position = 0;
		return ms;
	}
	public Stream ToStream(IEnumerable<T> AsyE, CT Ct){
		if(AsyE is null){
			throw new ArgumentNullException(nameof(AsyE));
		}
		var ms = new MemoryStream();
		foreach(var item in AsyE){
			Ct.ThrowIfCancellationRequested();
			var bytes = FnToBytes(item) ?? [];
			ms.Write(bytes, 0, bytes.Length);
		}
		ms.Position = 0;
		return ms;
	}
}
