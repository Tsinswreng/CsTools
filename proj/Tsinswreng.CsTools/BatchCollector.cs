using System.Runtime.CompilerServices;
namespace Tsinswreng.CsTools;



/// 線程安全?
/// 攢夠定ʹ量ʹ批次ⁿ後發
public partial class BatchCollector<TItem, TRet>
	//:IDisposable
	:IAsyncDisposable
{
	public static u64 DfltBatchSize{get;set;} = 100;

	public BatchCollector(){}
	public BatchCollector(
		Func<
			IList<TItem>
			,CT
			,Task<TRet>
		> FnAsy
		,u64 BatchSize = 0
	){
		if(BatchSize == 0){
			BatchSize = DfltBatchSize;
		}
		Init(FnAsy, BatchSize);
	}
	protected void Init(
		Func<
			IList<TItem>
			,CT
			,Task<TRet>
		> FnAsy
		,u64 BatchSize = 0
	){
		if(BatchSize == 0){
			BatchSize = DfltBatchSize;
		}
		this.FnAsy = FnAsy;
		this.BatchSize = BatchSize;
	}
	public static BatchCollector<TItem, TRet> Mk(
		Func<
			IList<TItem>
			,CT
			,Task<TRet>
		> FnAsy
		,u64 BatchSize = 0
	){
		if(BatchSize == 0){
			BatchSize = DfltBatchSize;
		}
		var R = new BatchCollector<TItem, TRet>();
		R.Init(FnAsy, BatchSize);
		return R;
	}
	//public IList<TItem> FullList{get;set;} = new List<TItem>();
	public IList<TItem> UnHandledList{get;set;} = new List<TItem>();
	public u64 BatchSize{get;set;} = DfltBatchSize;
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


/// <param name="OnRet">返匪0旹break</param>
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


/// 慎用。優先用AddMany
/// 如await NeoLearns.AddRangeAsy(NeoPoLearns, Ct).ToListAsync(Ct);
/// 當配ToListAsync(Ct)用、勿用.First() 否則只內部foreach只珩一次
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
