namespace Tsinswreng.CsTools;

using System.Collections;

//TODO 作公共庫 //TODO 把Dict相關操作 獨立作CsDictTools
public interface IKvNode{
	/// <summary>
	/// IDictionary | IList
	/// </summary>
	public obj? ValueObj{get;set;}
	/// <summary>
	/// get 取不到旹返null
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
	public IKvNode? this[int index] { get; set; }
	/// <summary>
	/// get 取不到旹返null
	/// </summary>
	/// <param name="prop"></param>
	/// <returns></returns>
	public IKvNode? this[str prop] { get; set; }
}


public struct KvNode:IKvNode{
	public KvNode(obj? Value){
		ValueObj = Value;
	}

	//[Impl]
	public obj? ValueObj{get;set;}
	//[Impl]
	public IKvNode? this[int index] {
		get{
			if(ValueObj is IList l){
				return new KvNode(l[index]);
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
	public IKvNode? this[str prop] {
		get{
			if(ValueObj is IDictionary d){
				return new KvNode(d[prop]);
			}
			return null;
		}
		set{
			if(ValueObj is IDictionary d){
				d[prop] = value;
			}
		}
	}
}
