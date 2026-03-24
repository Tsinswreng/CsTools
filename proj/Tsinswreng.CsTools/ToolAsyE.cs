namespace Tsinswreng.CsTools;

public class ToolAsyE{
	public static IAsyncEnumerable<T> ToAsyE<T>(IEnumerable<T> Enumer){
		return Enumer.ToAsyncEnumerable();
	}
}
