namespace Tsinswreng.CsTools.Tools;

public class ToolDict {
	/// <summary>
	/// 递归获取 Dictionary 中对应路径的值
	/// </summary>
	/// <param name="Dict">嵌套字典</param>
	/// <param name="KeyPath">键路径，比如 ["content", "text"]</param>
	/// <returns>对应路径的值，如果路径不存在，返回 null</returns>
	public static object? GetValueByPath<K>(
		IDictionary<K, object?> Dict
		,IList<K> KeyPath
	){
		return _GetValueByPath(Dict, KeyPath);
	}
	protected static object? _GetValueByPath<K,V>(
		IDictionary<K, V> Dict
		,IList<K> KeyPath
	){
		// if(KeyPath.Count >0 && KeyPath[0] is str s && s == "GalleryDirs"){//debug
		// 	System.Console.WriteLine();
		// }
		if (Dict == null || KeyPath == null || KeyPath.Count == 0){
			return null;
		}

		var current = Dict;
		for (int i = 0; i < KeyPath.Count - 1; i++){
			var key = KeyPath[i];
			if (current.TryGetValue(key, out V? Value)
				&& Value is IDictionary<K, V> childDict
			){
				current = childDict;
			}
			else{
				// 路径中断或者对应值不是嵌套字典
				return null;
			}
		}

		var lastKey = KeyPath[KeyPath.Count - 1];
		current.TryGetValue(lastKey, out var value);
		return value;
	}

	/// <summary>
	/// 递归设置 Dictionary 中对应路径的值，如果路径不存在会自动创建中间嵌套字典
	/// </summary>
	/// <param name="Dict">嵌套字典</param>
	/// <param name="KeyPath">键路径，比如 ["content", "text"]</param>
	/// <param name="Value">要设置的值</param>
	public static void PutValueByPath<K>(
		IDictionary<K, object?> Dict
		,IList<K> KeyPath
		,object? Value
	){
		if (Dict == null || KeyPath == null || KeyPath.Count == 0){
			return;
		}

		var current = Dict;
		for (int i = 0; i < KeyPath.Count - 1; i++){
			var key = KeyPath[i];
			if (current.TryGetValue(key, out object? Got)
				&& Got is IDictionary<K, object?> childDict
			){
				current = childDict;
			}
			else{
				var newMap = new Dictionary<K, object?>();
				current[key] = newMap;
				current = newMap;
			}
		}

		var lastKey = KeyPath[KeyPath.Count - 1];
		current[lastKey] = Value;
	}

}
