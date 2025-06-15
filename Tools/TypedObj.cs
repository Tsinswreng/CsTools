namespace Tsinswreng.CsCore.Tools;


public interface ITypedObj{
	public Type? Type{get;set;}
	public long TypeCode{get;set;}
	public object? Data{get;set;}
}

public struct TypedObj{
	public Type? Type{get;set;}
	public long TypeCode{get;set;}
	public object? Data{get;set;}
}
