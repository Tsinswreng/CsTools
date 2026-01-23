using System.Runtime.CompilerServices;
namespace Tsinswreng.CsTools;


/// <summary>
/// 恐非線程安全
/// 攢夠定ʹ量ʹ批次ⁿ後發
/// </summary>
/// <typeparam name="TItem"></typeparam>
/// <typeparam name="TRet"></typeparam>
public partial class BatchCollector<TItem, TRet>
	//:IDisposable
	:IAsyncDisposable
{
	public BatchCollector(Func<
			IList<TItem>
			,CT
			,Task<TRet>
		> FnAsy
		,u64 BatchSize = 100
	){
		this.FnAsy = FnAsy;
		this.BatchSize = BatchSize;
	}
	//public IList<TItem> FullList{get;set;} = new List<TItem>();
	public IList<TItem> UnHandledList{get;set;} = new List<TItem>();
	public u64 BatchSize{get;set;} = 0xfff;
	public Func<
		IList<TItem>
		, CT
		, Task<TRet>
	> FnAsy{get;set;}

	public async Task<TRet?> Add(
		TItem Item
		,CT Ct
	){
		UnHandledList.Add(Item);
		//FullList.Add(item);
		if((u64)UnHandledList.Count >= BatchSize){
			var Ans = await Run(Ct);
			return Ans;
		}
		return default;
	}

/// <summary>
///
/// </summary>
/// <param name="items"></param>
/// <param name="OnRet">返匪0旹break</param>
/// <param name="Ct"></param>
/// <returns></returns>
	public async Task<nil> AddRange(
		IEnumerable<TItem> items
		,Func<TRet?, i32>? OnRet
		,CT Ct
	){
		foreach(var item in items){
			var Ret = await Add(item, Ct);
			var r = OnRet?.Invoke(Ret)??0;
			if(r != 0){
				break;
			}
		}
		return NIL;
	}

/// <summary>
/// 慎用。優先用AddMany
/// 如await NeoLearns.AddRangeAsy(NeoPoLearns, Ct).ToListAsync(Ct);
/// 當配ToListAsync(Ct)用、勿用.First() 否則只內部foreach只珩一次
/// </summary>
/// <param name="items"></param>
/// <param name="Ct"></param>
/// <returns></returns>
	public async IAsyncEnumerable<TRet?> AddRangeAsyE(
		IEnumerable<TItem> items
		,[EnumeratorCancellation] CT Ct
	){
		//var Ans = new List<TRet>();
		foreach(var item in items){
			var Ret = await Add(item, Ct);
			yield return Ret;
			//Ans.Add(Ret.Value);
		}
		//return Ans;
	}

	public async IAsyncEnumerable<TRet> AddRangeNoNullRtn(
		IEnumerable<TItem> Items
		,[EnumeratorCancellation] CT Ct
	){
		foreach(var item in Items){
			var Ret = await Add(item, Ct);
			if(Ret is null){
				continue;
			}
			yield return Ret;
		}
	}

	public async IAsyncEnumerable<TRet> AddToEnd(
		IEnumerable<TItem> Items
		,[EnumeratorCancellation] CT Ct
	){
		var l1 = AddRangeNoNullRtn(Items, Ct);
		await foreach(var item in l1){
			yield return item;
		}
		if(!IsEnd){
			var l2 = await End(Ct);
			yield return l2!;
		}

	}

	//public bool IsEnd{get;protected set;} = false;
	public bool IsEnd{
		get{return UnHandledList.Count == 0;}
	}
	public async Task<TRet?> End(
		CT Ct
	){
		if(IsEnd){return default;}
		return await Run(Ct);
	}

	protected nil Clear(){
		UnHandledList.Clear();
		return NIL;
	}

	protected async Task<TRet> Run(
		CT Ct
	){
		var Ans = await FnAsy(UnHandledList, Ct);
		Clear();
		return Ans;
	}

	[Obsolete("Use DisposeAsync")]
	public void Dispose(){
		if(!IsEnd){
			End(default).Wait();
		}
	}

	public async ValueTask DisposeAsync() {
		if(!IsEnd){
			await End(default);
		}
	}
}
