//幫我寫個嵌套的IDict<str, obj> 或 IList<obj>轉json的函數。要求兼容AOT編譯
namespace Tsinswreng.CsTools;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;

public static class ToolJson {
	public static IDictionary<string, object?>? JsonStrToDict(string? json) {
		if(str.IsNullOrEmpty(json)){
			return null;
		}
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

	/// <summary>
	/// 将嵌套的IDictionary<string, object?>或IEnumerable<object?>递归序列化为JSON字符串，兼容AOT编译。
	/// </summary>
	public static str CollectionToJson(IDictionary<str, obj?> Dict){
		return ObjCollectionToJson(Dict);
	}
	public static str CollectionToJson(IEnumerable IEnumrb){
		return ObjCollectionToJson(IEnumrb);
	}
	static string ObjCollectionToJson(object? obj){
		using var stream = new System.IO.MemoryStream();
		using (var writer = new Utf8JsonWriter(stream)){
			WriteJsonValue(writer, obj);
		}
		return System.Text.Encoding.UTF8.GetString(stream.ToArray());
	}

	private static void WriteJsonValue(Utf8JsonWriter writer, object? value){
		if (value == null){
			writer.WriteNullValue();
		}
		else if (value is string s){
			writer.WriteStringValue(s);
		}
		else if (value is bool b){
			writer.WriteBooleanValue(b);
		}
		else if (value is int i){
			writer.WriteNumberValue(i);
		}
		else if (value is long l){
			writer.WriteNumberValue(l);
		}
		else if (value is double d){
			writer.WriteNumberValue(d);
		}
		else if (value is float f){
			writer.WriteNumberValue(f);
		}
		else if (value is IDictionary<string, object?> dict){
			writer.WriteStartObject();
			foreach (var kv in dict){
				writer.WritePropertyName(kv.Key);
				WriteJsonValue(writer, kv.Value);
			}
			writer.WriteEndObject();
		}
		else if (value is IEnumerable list && !(value is string)){
			writer.WriteStartArray();
			foreach (var item in list){
				WriteJsonValue(writer, item);
			}
			writer.WriteEndArray();
		}
		else if (value is decimal dec){
			writer.WriteNumberValue(dec);
		}
		else{
			// 其他类型一律转字符串
			writer.WriteStringValue(value.ToString());
		}
	}
}
