namespace Tsinswreng.CsTools;

using System.Collections;

//TODO 作公共庫 //TODO 把Dict相關操作 獨立作CsDictTools
public interface IJsonNode{
	/// <summary>
	/// IDictionary | IList | IDictionary<str, obj?> | IList<obj?>
	/// </summary>
	public obj? ValueObj{get;set;}
	/// <summary>
	/// get 取不到旹返null
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
	public IJsonNode? this[int index] { get; set; }
	/// <summary>
	/// get 取不到旹返null
	/// </summary>
	/// <param name="prop"></param>
	/// <returns></returns>
	public IJsonNode? this[str prop] { get; set; }
}


public struct JsonNode:IJsonNode{
	public JsonNode(obj? Value){
		ValueObj = Value;
	}

	//[Impl]
	public obj? ValueObj{get;set;}
	//[Impl]
	public IJsonNode? this[int index] {
		get{
			if(ValueObj is IList l){
				return new JsonNode(l[index]);
			}
			if(ValueObj is IList<obj?> l2){
				return new JsonNode(l2[index]);
			}
			return null;
		}
		set{
			if(ValueObj is IList l){
				l[index] = value;
			}
		}
	}
	//[Impl]
	public IJsonNode? this[str prop] {
		get{
			if(ValueObj is IDictionary d){
				return new JsonNode(d[prop]);
			}
			if(ValueObj is IDictionary<str, obj?> d2){
				return new JsonNode(d2[prop]);
			}
			return null;
		}
		set{
			if(ValueObj is IDictionary d){
				d[prop] = value;
			}
			if(ValueObj is IDictionary<str, obj?> d2){
				d2[prop] = value;
			}
		}
	}

	/// <summary>
	/// 把形如 "foo.bar[0].baz[2].qux" 的路径拆成对象列表：
	/// ["foo", "bar", 0, "baz", 2, "qux"]
	/// </summary>
	public static IList<object> ResolvePath(string Path){
		if (Path == null){
			throw new ArgumentNullException(nameof(Path));
		}

		var tokens = new List<object>();

		// 1. 先按“.”切成若干段
		foreach (var segment in Path.Split('.', StringSplitOptions.RemoveEmptyEntries)){
			// 2. 看这一段里有没有“[”
			var bracketIndex = segment.IndexOf('[');
			if (bracketIndex < 0){
				// 没有下标，直接当作字符串 key
				tokens.Add(segment);
				continue;
			}

			// 3. 有下标，先取 key 部分
			var key = segment[..bracketIndex];
			if (key.Length > 0){
				tokens.Add(key);
			}


			// 4. 把中括号里的数字全部提出来
			int pos = bracketIndex;
			while (pos < segment.Length && segment[pos] == '['){
				var closePos = segment.IndexOf(']', pos);
				if (closePos < 0){
					throw new FormatException($"路径格式错误：缺少闭合的 \"]\"，位置 {pos}");
				}

				var numSpan = segment.AsSpan(pos + 1, closePos - pos - 1);
				if (!int.TryParse(numSpan, out var index)){
					throw new FormatException($"路径格式错误：\"{numSpan.ToString()}\" 不是有效整数");
				}

				tokens.Add(index);
				pos = closePos + 1;
			}
		}

		return tokens;
	}

}

public static class ExtnKvNode{
	public static bool IsNull<TSelf>(
		TSelf? z
	)where TSelf:IJsonNode
	{
		if(z is null || z.ValueObj is null){
			return true;
		}
		return false;
	}
	extension(IJsonNode z){
		public bool IsEndPoint(){
			if(z.ValueObj is IList l){
				return l.Count == 0;
			}
			if(z.ValueObj is IList<obj?> l2){
				return l2.Count == 0;
			}
			if(z.ValueObj is IDictionary d){
				return d.Count == 0;
			}
			if(z.ValueObj is IDictionary<str, obj?> d2){
				return d2.Count == 0;
			}
			return false;
		}

		public bool TryGetValue(IList<obj> Path, out IJsonNode Value){
			return z.TryGetNodeByPath(Path, out Value);
		}

		public bool TryGetValue<T>(str Key, out T R){
			if(z.TryGetNode(Key, out var Out)){
				if(Out!.ValueObj is T r){
					R = r;
					return true;
				}
			}
			R=default!;
			return false;
		}

		public bool TryGetValue<T>(i32 Key, out T R){
			R=default!;
			if(z.TryGetNode(Key, out var Out)){
				if(Out!.ValueObj is T r){
					R = r;
					return true;
				}
			}
			return false;
		}

		public bool TryGetValue<T>(IList<obj> Key, out T R){
			R=default!;
			if(z.TryGetValue(Key, out var Out)){
				if(Out!.ValueObj is T r){
					R = r;
					return true;
				}
			}
			return false;
		}


		public bool TryGetNode(str Key, out IJsonNode Value){
			Value = default!;
			if(z.ValueObj is IDictionary<str, obj?> d){
				if(d.TryGetValue(Key, out var Out)){
					Value = new JsonNode(Out);
					return true;
				}
				return false;
			}
			if(z.ValueObj is IDictionary d2){
				if(d2.Contains(Key)){
					Value = new JsonNode(d2[Key]);
					return true;
				}
			}
			return false;
		}
		public bool TryGetNode(int Index, out IJsonNode? Value){
			if(z.ValueObj is IList<obj?> l){
				if(Index < l.Count){
					Value = new JsonNode(l[Index]);
					return true;
				}
			}
			if(z.ValueObj is IList l2){
				if(Index < l2.Count){
					Value = new JsonNode(l2[Index]);
					return true;
				}
			}
			Value = null;
			return false;
		}
		public bool TryGetNodeByPath(str Path, out IJsonNode Value){
			return z.TryGetNodeByPath(JsonNode.ResolvePath(Path), out Value);
		}
		public bool TryGetNodeByPath(IList<obj> Path, out IJsonNode Value){
			Value = default!;
			if (Path == null || Path.Count == 0) return false;

			IJsonNode cur = z;          // 从当前节点出发
			for (int i = 0; i < Path.Count; i++){
				var seg = Path[i];

				/* 1. 整数下标：访问列表 */
				if (seg is int idx){
					if (!cur.TryGetNode(idx, out var tmp) || tmp is not IJsonNode nextIdx)
						return false;
					cur = nextIdx;
				}
				/* 2. 字符串 key：访问字典 */
				else if (seg is string key){
					if (!cur.TryGetNode(key, out var tmp) || tmp is not IJsonNode nextKey)
						return false;
					cur = nextKey;
				}
				/* 3. 其他类型视为非法 */
				else return false;
			}

			Value = cur;     // 走到这里说明路径全部解析成功
			return true;
		}
		public bool SetNodeByPath(str Path, IJsonNode Node){
			return z.SetNodeByPath(JsonNode.ResolvePath(Path), Node);
		}
		public bool SetNodeByPath(IList<obj> Path, IJsonNode Node){
			Node = default!;
			if (Path == null || Path.Count == 0) return false;

			IJsonNode cur = z;          // 从当前节点出发
			for (int i = 0; i < Path.Count; i++){
				var seg = Path[i];

				/* 1. 整数下标：访问列表 */
				if (seg is int idx){
					if (!cur.TryGetNode(idx, out var tmp) || tmp is not IJsonNode nextIdx)
						return false;
					cur = nextIdx;
				}
				/* 2. 字符串 key：访问字典 */
				else if (seg is string key){
					if (!cur.TryGetNode(key, out var tmp) || tmp is not IJsonNode nextKey)
						return false;
					cur = nextKey;
				}
				/* 3. 其他类型视为非法 */
				else return false;
			}

			Node = cur;              // 走到这里说明路径全部解析成功
			return true;
		}

	}
}
