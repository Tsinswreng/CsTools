using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Tsinswreng.CsTools.Tools;


/// <summary>
/// 非線程安全
/// </summary>
/// <typeparam name="TItem"></typeparam>
/// <typeparam name="TRet"></typeparam>
public class BatchListAsy<TItem, TRet>
	//:IDisposable
	:IAsyncDisposable
{
	public BatchListAsy(Func<
			IEnumerable<TItem>
			, CT
			, Task<TRet>
		> FnAsy
		,u64 BatchSize = 0xfff
	){
		this.FnAsy = FnAsy;
		this.BatchSize = BatchSize;
	}
	//public IList<TItem> FullList{get;set;} = new List<TItem>();
	public IList<TItem> UnHandledList{get;set;} = new List<TItem>();
	public u64 BatchSize{get;set;} = 0xfff;
	public Func<
		IEnumerable<TItem>
		, CT
		, Task<TRet>
	> FnAsy{get;set;}

	public async Task<TRet?> Add(
		TItem item
		, CT ct
	){
		UnHandledList.Add(item);
		//FullList.Add(item);
		if((u64)UnHandledList.Count >= BatchSize){
			var Ans = await Run(ct);
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
	public async Task<nil> AddMany(
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
	public async IAsyncEnumerable<TRet?> AddRangeAsy(
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


	bool _IsEnd{get;set;} = false;
	public async Task<TRet?> End(
		CT ct
	){
		if(_IsEnd){return default;}
		if((u64)UnHandledList.Count > 0){
			return await Run(ct);
		}
		_IsEnd = true;
		return default;
	}

	protected nil Clear(){
		UnHandledList.Clear();
		return NIL;
	}

	protected async Task<TRet?> Run(
		CT ct
	){
		var Ans = await FnAsy(UnHandledList, ct);
		Clear();
		return Ans;
	}

	[Obsolete]
	public void Dispose(){
		if(!_IsEnd){
			End(default).Wait();
		}
	}

	public async ValueTask DisposeAsync() {
		if(!_IsEnd){
			await End(default);
		}
	}
}
