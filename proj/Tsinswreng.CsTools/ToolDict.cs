namespace Tsinswreng.CsTools;

public static class ToolDict {

	public static bool TryGetValueByPath<K>(
		this IDictionary<K, obj?> Dict
		,IList<K> KeyPath
		,out obj? Got
	){
		return _TryGetValueByPath(Dict, KeyPath, out Got);
	}

	static bool _TryGetValueByPath<K,V>(
		IDictionary<K, V> Dict
		,IList<K> KeyPath
		,out V Got
	){
		Got = default!;
		if (Dict == null || KeyPath == null || KeyPath.Count == 0){
			return false;
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
				return false;
			}
		}

		var lastKey = KeyPath[KeyPath.Count - 1];
		return current.TryGetValue(lastKey, out Got!);
	}

	/// <summary>
	/// 递归获取 Dictionary 中对应路径的值
	/// </summary>
	/// <param name="Dict">嵌套字典</param>
	/// <param name="KeyPath">键路径，比如 ["content", "text"]</param>
	/// <returns>对应路径的值，如果路径不存在，返回 null</returns>
	public static object? GetValueByPath<K>(
		this IDictionary<K, obj?> Dict
		,IList<K> KeyPath
	){
		//_GetValueByPath(Dict, KeyPath);
		if( TryGetValueByPath(Dict, KeyPath, out var got) ){
			return got;
		}
		return null;
	}


	static object? _GetValueByPath<K,V>(
		IDictionary<K, V> Dict
		,IList<K> KeyPath
	){
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
	public static void SetValueByPath<K>(
		this IDictionary<K, object?> Dict
		,IList<K> KeyPath
		,object? Value
	)where K:notnull{
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


	public static IDictionary<K, object?> ToDeepMerge<K>(
		this IDictionary<K, object?> dict1
		,IDictionary<K, object?> dict2
	)
		where K : notnull // 确保键类型非空
	{
		// 创建一个新字典作为结果，初始化为 dict1 的浅拷贝（避免修改原字典）
		var mergedDict = new Dictionary<K, object?>(dict1);
		// 遍历 dict2 的每个键值对，应用合并规则
		foreach (var key in dict2.Keys){
			object? value2 = dict2[key];
			// 如果 key 在 dict1 中存在，需要处理合并冲突
			if (dict1.TryGetValue(key, out object? value1)){
				// 情况1：两个值都是嵌套字典 -> 递归深度合并
				if (value1 is IDictionary<K, object> nestedDict1 && value2 is IDictionary<K, object> nestedDict2){
					// 递归调用 DeepMerge，并将结果存入 mergedDict
					mergedDict[key] = ToDeepMerge(nestedDict1, nestedDict2);
				}
				// 情况2：两个值都是列表 -> 连接列表（体现优先级：第二个列表追加到末尾）
				else if (value1 is IList<object> list1 && value2 is IList<object> list2){
					var mergedList = new List<object>(list1); // 先添加 dict1 的列表
					mergedList.AddRange(list2); // 再添加 dict2 的列表（更高优先级内容在后）
					mergedDict[key] = mergedList;
				}
				// 情况3：其他情况（类型不匹配、非可合并类型、或 null）-> 使用 dict2 的值（覆盖）
				else{
					mergedDict[key] = value2; // 后传入字典有更高优先级
				}
			}
			// 如果 key 不在 dict1 中，直接添加 dict2 的键值对
			else{
				mergedDict[key] = value2;
			}
		}
		return mergedDict;
	}

}
