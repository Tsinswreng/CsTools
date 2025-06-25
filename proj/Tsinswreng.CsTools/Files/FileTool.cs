namespace Tsinswreng.CsTools.Files;

public class FileTool {
	public static void EnsureFile(string filePath) {
		if (string.IsNullOrWhiteSpace(filePath)){
			return;
		}

		var directory = Path.GetDirectoryName(filePath);
		if (!string.IsNullOrEmpty(directory)) {
			Directory.CreateDirectory(directory);
		}

		if (!File.Exists(filePath)) {
			// 创建空文件，使用 FileStream 立即释放资源
			using (File.Create(filePath)) { }
		}
	}
}
