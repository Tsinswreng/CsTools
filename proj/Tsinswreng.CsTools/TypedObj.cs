namespace Tsinswreng.CsTools;

// public interface ITypedObj:IDisposable{
// 	public Type? Type{get;set;}
// 	//用戶自定義
// 	public long TypeCode{get;set;}
// 	public object? Data{get;set;}
// //語法插件提示: 将 ITypedObj.Dispose() 更改为调用 GC.SuppressFinalize(object)。这将使引入终结器的派生类型无需重新实现 "IDisposable" 即可调用它。CA1816
// 	void IDisposable.Dispose() {
// 		if(Data is IDisposable disposable){
// 			disposable.Dispose();
// 		}
// 		GC.SuppressFinalize(this);
// 	}
// }

public interface ITypedObj{
	public Type? Type{get;set;}
	//用戶自定義
	public long TypeCode{get;set;}
	public object? Data{get;set;}
	//不叶 Dispose。取出Data並作類型轉換後 若Data有叶Dispose則用㞢、洏不必叶于ITypedObj
//語法插件提示: 将 ITypedObj.Dispose() 更改为调用 GC.SuppressFinalize(object)。这将使引入终结器的派生类型无需重新实现 "IDisposable" 即可调用它。CA1816
	// void IDisposable.Dispose() {
	// 	if(Data is IDisposable disposable){
	// 		disposable.Dispose();
	// 	}
	// 	GC.SuppressFinalize(this);
	// }
}

public struct TypedObj:ITypedObj{
	public Type? Type{get;set;}
	//用戶自定義
	public long TypeCode{get;set;}
	public object? Data{get;set;}
}
