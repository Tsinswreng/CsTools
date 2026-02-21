#pragma warning disable CS1998
using System.Runtime.CompilerServices;

namespace Tsinswreng.CsTools;

public static class ExtnIEnumerable{
	extension<T>(IAsyncEnumerable<T> z){
		public IAsyncEnumerable<T> Concat(
			IEnumerable<T> Other
		){
			return z.Concat(Other.ToAsyncEnumerable());
		}
	}
	public static async IAsyncEnumerable<T> Flat<T>(
		this IAsyncEnumerable<Task<T>> z,
		[EnumeratorCancellation] CT Ct = default
	){
		await foreach (var task in z.WithCancellation(Ct)){
			yield return await task.ConfigureAwait(false);
		}
	}


	public static async IAsyncEnumerable<T> Flat<T>(
		this IAsyncEnumerable<IEnumerable<T>> z,
		[EnumeratorCancellation] CT Ct = default
	){
		await foreach (var itbl in z.WithCancellation(Ct)){
			foreach(var item in itbl){
				yield return item;
			}
		}
	}

	public static async IAsyncEnumerable<T> Flat<T>(
		this IAsyncEnumerable<IAsyncEnumerable<T>> z,
		[EnumeratorCancellation] CT Ct = default
	){
		await foreach (var itbl in z.WithCancellation(Ct)){
			await foreach(var item in itbl){
				yield return item;
			}
		}
	}

	public static async IAsyncEnumerable<T> Flat<T>(
		this IAsyncEnumerable<IList<T>> z,
		[EnumeratorCancellation] CT Ct = default
	){
		await foreach (var itbl in z.WithCancellation(Ct)){
			foreach(var item in itbl){
				yield return item;
			}
		}
	}






}
