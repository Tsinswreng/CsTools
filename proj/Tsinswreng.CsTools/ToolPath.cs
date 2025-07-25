namespace Tsinswreng.CsTools;

public class ToolPath {
	/// <summary>
	/// ["a", "b", "c"] => "a/b/c"
	/// ["a/", "b", "c/"] => "a/b/c"
	/// 首元素.始ʹ位ʸʹ斜槓ˇ不處理
	/// ["/a", "b", "c"] => "/a/b/c"
	/// </summary>
	/// <param name="Segments"></param>
	/// <returns></returns>
	public static string SlashTrimEtJoin(IEnumerable<str> Segments) {
		List<str> SegList = [];
		foreach(var (i, Seg) in Segments.Index()){
			var U = Seg;
			if(i == 0){
				U = U.TrimEnd('/');
				SegList.Add(U);
				continue;
			}
			U=U.TrimEnd('/');
			U=U.TrimStart('/');
			SegList.Add(U);
		}
		return str.Join("/", SegList);
	}
}
