namespace Tsinswreng.CsTools;

using System;
using System.Collections.Generic;
using System.Text.Json;

public static class ToolJson {
	public static IDictionary<string, object?> JsonStrToDict(string json) {
		using JsonDocument doc = JsonDocument.Parse(json,
			new JsonDocumentOptions{
				CommentHandling = JsonCommentHandling.Skip
			}
		);
		JsonElement root = doc.RootElement;

		if (root.ValueKind != JsonValueKind.Object){
			throw new ArgumentException("JSON 根元素不是对象类型");
		}
		return JsonElementToDict(root);
	}

	private static IDictionary<string, object?> JsonElementToDict(JsonElement element) {
		var dict = new Dictionary<string, object?>();

		foreach (var property in element.EnumerateObject()) {
			dict[property.Name] = ParseJsonElement(property.Value);
		}
		return dict;
	}

	public static object? ParseJsonElement(JsonElement element) {
		switch (element.ValueKind) {
			case JsonValueKind.Null:
			case JsonValueKind.Undefined:
				return null;

			case JsonValueKind.String:
				return element.GetString();

			case JsonValueKind.Number:
				// 尝试先用long，不能转则用double
				if (element.TryGetInt64(out long l))
					return l;
				else if (element.TryGetDouble(out double d))
					return d;
				else
					return null;

			case JsonValueKind.True:
			case JsonValueKind.False:
				return element.GetBoolean();

			case JsonValueKind.Object:
				return JsonElementToDict(element);

			case JsonValueKind.Array:
				var list = new List<object?>();
				foreach (var item in element.EnumerateArray()) {
					list.Add(ParseJsonElement(item));
				}
				return list;

			default:
				// 其他类型抛异常或返回null都可，根据需求调整
				throw new NotSupportedException($"不支持的JSON数据类型: {element.ValueKind}");
		}
	}
}
