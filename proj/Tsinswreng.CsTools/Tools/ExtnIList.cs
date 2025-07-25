using System.Collections;

namespace Tsinswreng.CsTools.Tools;

public static class ExtnIList{
	public static void AddRange<T>(this IList<T> list, IEnumerable<T> items) {
		foreach (var item in items) {
			list.Add(item);
		}
	}

	///不支持倒數
	public static T AtOrDefault<T>(
		this IList<T> z
		,i32 Index
		,T Default = default!
	)
	{
		if(Index < 0 || Index >= z.Count){
			return Default;
		}
		return z[Index];
	}

	public static void Sort<T>(this IList<T> list, Comparison<T> comparison){
		if (list == null) throw new ArgumentNullException(nameof(list));
		if (comparison == null) throw new ArgumentNullException(nameof(comparison));

		// ArrayList.Adapter 能把 IList 包装成 ArrayList，然后 ArrayList.Sort 可以调用 IComparer
		ArrayList.Adapter((IList)list).Sort(new ComparisonComparer<T>(comparison));
	}

	private class ComparisonComparer<T> : IComparer{
		private readonly Comparison<T> _comparison;
		public ComparisonComparer(Comparison<T> comparison){
			_comparison = comparison ?? throw new ArgumentNullException(nameof(comparison));
		}

		public int Compare(object x, object y){
			return _comparison((T)x, (T)y);
		}
	}


	// [Obsolete("用.net9之Index()")]
	// public static IEnumerable<(T, i32)> VIPair<T>(
	// 	this IEnumerable<T> z
	// ){
	// 	var R = z.Select((item, i)=>(item,i));
	// 	return R;
	// }

	public static IList<T> FillUpTo<T>(
		this IList<T> z
		,u64 TotalCount
		,T Value
	){
		for(u64 i = (u64)z.Count; i < TotalCount; i++){
			z.Add(Value);
		}
		return z;
	}



}
