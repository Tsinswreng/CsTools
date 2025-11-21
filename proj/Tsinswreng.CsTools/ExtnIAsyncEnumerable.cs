#pragma warning disable CS1998
using System.Runtime.CompilerServices;

namespace Tsinswreng.CsTools;

public static class ExtnIEnumerable{
	public static async IAsyncEnumerable<T> FlatAsync<T>(
		this IAsyncEnumerable<Task<T>> z,
		[EnumeratorCancellation] CT Ct = default
	){
		await foreach (var task in z.WithCancellation(Ct)){
			yield return await task.ConfigureAwait(false);
		}
	}


}
