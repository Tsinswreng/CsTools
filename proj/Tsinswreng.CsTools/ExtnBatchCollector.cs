using System.Runtime.CompilerServices;

namespace Tsinswreng.CsTools;

public static class ExtnBatchCollector{
	extension<TItem, TRetEle>(BatchCollector<TItem, IAsyncEnumerable<TRetEle>> z){
		public async IAsyncEnumerable<TRetEle> AllFlat(
			IEnumerable<TItem> Items
			,[EnumeratorCancellation]CT Ct
		){
			var d2 = z.AddToEnd(Items, Ct);
			var r = d2.Flat();
			await foreach(var item in r){
				yield return item;
			}
		}
	}
}
