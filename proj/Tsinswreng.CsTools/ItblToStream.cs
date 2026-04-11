namespace Tsinswreng.CsTools;
public class ItblToStream<T>{
	public ItblToStream(Func<T, byte[]> FnToBytes){
		this.FnToBytes = FnToBytes;
	}
	public Func<T, byte[]> FnToBytes{get;set;}
	public Stream ToStream(IAsyncEnumerable<T> AsyE, CT Ct){
		
	}
	public Stream ToStream(IEnumerable<T> AsyE, CT Ct){
		
	}
}
