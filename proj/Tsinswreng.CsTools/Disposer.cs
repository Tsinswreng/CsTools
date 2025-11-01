namespace Tsinswreng.CsTools;
public class DisposableList:IDisposable{
	public static DisposableList Mk(){
		return new DisposableList();
	}
	public IList<obj> List = new List<obj>();

	public DisposableList Add(IDisposable O){
		List.Add(O);
		return this;
	}
	void IDisposable.Dispose(){
		foreach(var obj in List){
			if(obj is IDisposable disposable){
				disposable.Dispose();
			}
		}
	}
}
