using System;
using System.Collections.Generic;
using System.IO;

namespace Tsinswreng.CsTools;

public partial class ItblToStream<T>{
	public Func<T, ReadOnlyMemory<byte>> FnToBytes{get;set;}
	public partial Task<Stream> ToStream(IAsyncEnumerable<T> AsyE, CT Ct);
	public partial Stream ToStream(IEnumerable<T> Itbl);
}
