using System.Collections;

namespace Tsinswreng.CsTools;

public static class ExtnIList{
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

	public static nil FillUpTo<T>(
		this ICollection<T> z
		,u64 TotalCount
		,T Value
	){
		for(u64 i = (u64)z.Count; i < TotalCount; i++){
			z.Add(Value);
		}
		return NIL;
	}



}
