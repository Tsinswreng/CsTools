namespace Tsinswreng.CsTools.Tools.MultiDict;


public static class ExtnMultiDict{
	public static IDictionary<K,IList<V>> AddInValues<K,V>(
		this IDictionary<K,IList<V>> z
		,K Key
		,V Value
	){
		if(z.TryGetValue(Key, out var List)){
			List.Add(Value);
		}else{
			z[Key] = [Value];
		}
		return z;
	}
}
