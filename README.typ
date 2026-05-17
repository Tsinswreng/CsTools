#import "@preview/tsinswreng-auto-heading:0.1.0": auto-heading
#let H = auto-heading;

#H[Tsinswreng.CsTools][
	Tsinswreng.CsTools 是一組偏實用型的通用工具函數集合。

	它目前包含的內容比較雜，但大體集中在：

	- 路徑與文件工具
	- JSON / 字典輔助
	- 異步可迭代工具
	- 表達式與集合工具
	- 若干二進制/壓縮工具

	#H[安裝][
		```bash
		dotnet add package Tsinswreng.CsTools --version 0.0.1-alpha
		```
	]

	#H[示例][
		```csharp
		using Tsinswreng.CsTools;

		var path = ToolPath.SlashTrimEtJoin(["a/", "b", "c/"]);
		```
	]
]
