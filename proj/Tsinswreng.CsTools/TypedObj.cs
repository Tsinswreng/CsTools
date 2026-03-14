namespace Tsinswreng.CsTools;

[Obsolete("宜自定義Dto、用Enum㕥舉類型")]
public partial interface ITypedObj{
	public Type? Type{get;set;}
	//用戶自定義
	public i64 TypeCode{get;set;}
	public obj? Data{get;set;}
}

public partial struct TypedObj:ITypedObj{
	static ITypedObj Mk<T>(T t){
		return new TypedObj{
			Type=typeof(T),
			Data=t,
		};
	}
	public Type? Type{get;set;}
	//用戶自定義
	public long TypeCode{get;set;}
	public object? Data{get;set;}
}
