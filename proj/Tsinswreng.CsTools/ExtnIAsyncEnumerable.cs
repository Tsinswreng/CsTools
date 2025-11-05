using System.Runtime.CompilerServices;

namespace Tsinswreng.CsTools;

public static class ExtnIEnumerable{
	// [Obsolete("用Linq Select")]
	// public static IEnumerable<TDst> Convert<TSrc, TDst>(
	// 	this IEnumerable<TSrc> Src
	// 	,Func<TSrc, TDst> FnConv
	// ){
	// 	foreach(var item in Src){
	// 		yield return FnConv(item);
	// 	}
	// }

	// [Obsolete("用Linq Select")]
	// public static async IAsyncEnumerable<TDst> Convert<TSrc, TDst>(
	// 	this IAsyncEnumerable<TSrc> Src
	// 	,Func<TSrc, TDst> FnConv
	// ){
	// 	await foreach(var item in Src){
	// 		yield return FnConv(item);
	// 	}
	// }

	public static async IAsyncEnumerable<T> FlatAsync<T>(
		this IAsyncEnumerable<Task<T>> z,
		[EnumeratorCancellation] CT Ct = default
	){
		await foreach (var task in z.WithCancellation(Ct)){
			yield return await task.ConfigureAwait(false);
		}
	}

	public static async IAsyncEnumerable<T> EmptyAsyE<T>(){
		yield break;   // 什么都不 yield，直接结束
	}




/// <summary>
/// 支持倒數
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="z"></param>
/// <param name="Index"></param>
/// <param name="Default"></param>
/// <returns></returns>
	// public static T AtOrDefault<T>(
	// 	this IList<T> z
	// 	,i32 Index
	// 	,T Default = default!
	// )

	// {
	// 	if(Index < 0){
	// 		Index += z.Count;
	// 	}
	// 	if(Index >= z.Count){
	// 		return Default;
	// 	}
	// 	return z[Index];
	// }




}
