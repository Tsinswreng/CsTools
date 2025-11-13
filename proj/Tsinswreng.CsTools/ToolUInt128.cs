namespace Tsinswreng.CsTools;
using System.Buffers.Binary;
public static class ToolUInt128{

	public static string ToBase64Url(UInt128 value) {
		var bytes = ToByteArr(value);
		string base64 = Convert.ToBase64String(bytes);
		return base64.TrimEnd('=')
			.Replace('+', '-')
			.Replace('/', '_')
		;
	}

	public static UInt128 Base64UrlToUInt128(string base64Url) {
		// 还原为标准Base64并补填充
		string base64 = base64Url.Replace('-', '+')
			.Replace('_', '/')
		;
		switch (base64.Length % 4) {
			case 2: base64 += "=="; break;
			case 3: base64 += "="; break;
		}

		byte[] bytes = Convert.FromBase64String(base64);
		return ByteArrToUInt128(bytes);
	}

	public static UInt128 ByteArrToUInt128(u8[] bytes){
		return BytesToUInt128(bytes.AsSpan());
	}

/// <summary>
/// 大端序读取：前8字节为upper，后8字节为lower
/// </summary>
/// <param name="bytes"></param>
/// <returns></returns>
/// <exception cref="ArgumentException"></exception>
	public static UInt128 BytesToUInt128(Span<u8> bytes){
		if (bytes.Length != 16){
			throw new ArgumentException("Invalid byte length for UInt128");
		}

		ulong upper = BinaryPrimitives.ReadUInt64BigEndian(bytes.Slice(0, 8));
		ulong lower = BinaryPrimitives.ReadUInt64BigEndian(bytes.Slice(8, 8));
		return (UInt128)upper << 64 | lower;
	}

	public static u8[] ToByteArr(this UInt128 value){
		var R = new u8[16];
		var span = R.AsSpan();
		ToByteSpan(value, ref span);
		return R;
	}

	/// <summary>
	/// 高位在前
	/// </summary>
	public static nil ToByteSpan(
		this UInt128 value
		,ref Span<u8> R
	){
		// 拆分为高64位（upper）和低64位（lower）
		ulong upper = (ulong)(value >> 64);
		ulong lower = (ulong)value;

		// 大端序写入：先upper后lower
		BinaryPrimitives.WriteUInt64BigEndian(R.Slice(0, 8), upper);  // 高64位在前
		BinaryPrimitives.WriteUInt64BigEndian(R.Slice(8, 8), lower);  // 高64位在前8, 8), lower);  // 低64位在后
		return NIL;
	}

	/// <summary>
	/// 高位在前
	/// </summary>
	public static Span<u8> ToByteSpan(
		this UInt128 value
	){
		Span<u8> R = new u8[16];
		ToByteSpan(value, ref R);
		return R;
	}

/// <summary>
/// UInt128轉64進制(不是base64)字串
/// 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-_
/// 從低位始
/// </summary>
/// <param name="num"></param>
/// <returns></returns>
	public static str ToLow64Base(UInt128 num){
		var chars_littleToBig = new List<char>();
		for(var i = 0;;i++){
			var bit6 = (u8)(num & 63);
			var c = Low64Base.Num_Char[bit6];
			chars_littleToBig.Add((char)c);
			num >>= 6;
			if(num == 0){
				break;
			}
		}

		chars_littleToBig.Reverse();
		var ans = str.Join("",chars_littleToBig);

		return ans;
	}


/// <summary>
/// 64進制(不是base64)字串轉UInt128
/// 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-_
/// </summary>
/// <param name="base64LittleEnd"></param>
/// <returns></returns>
	public static UInt128 Low64BaseToUInt128(str base64LittleEnd){
		UInt128 ans = 0;
		for(var i = 0; i < base64LittleEnd.Length; i++){
			var c = (u8)base64LittleEnd[i];
			var bit6 = Low64Base.Char_Num[c];
			if(bit6 > 63){
				throw new ArgumentException($"Invalid character in base64 string: {c} at index {i}");
			}
			ans = (ans << 6) | bit6;
		}
		return ans;
	}

	public static bool TryLow64BaseToUInt128(
		str Base64LittleEnd
		,out UInt128 R
	){
		R = 0;
		for(var i = 0; i < Base64LittleEnd.Length; i++){
			var c = (u8)Base64LittleEnd[i];
			var bit6 = Low64Base.Char_Num[c];
			if(bit6 > 63){
				//throw new ArgumentException($"Invalid character in base64 string: {c} at index {i}");
				R=default;
				return false;
			}
			R = (R << 6) | bit6;
		}
		return true;
	}


}

/// <summary>
/// 64進制(不是base64)對照表
/// 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-_
/// </summary>
class Low64Base{
	static List<List<object>> list2d= [
 [(u8)0b000000,(u8)'0']
,[(u8)0b000001,(u8)'1']
,[(u8)0b000010,(u8)'2']
,[(u8)0b000011,(u8)'3']
,[(u8)0b000100,(u8)'4']
,[(u8)0b000101,(u8)'5']
,[(u8)0b000110,(u8)'6']
,[(u8)0b000111,(u8)'7']
,[(u8)0b001000,(u8)'8']
,[(u8)0b001001,(u8)'9']
,[(u8)0b001010,(u8)'A']
,[(u8)0b001011,(u8)'B']
,[(u8)0b001100,(u8)'C']
,[(u8)0b001101,(u8)'D']
,[(u8)0b001110,(u8)'E']
,[(u8)0b001111,(u8)'F']
,[(u8)0b010000,(u8)'G']
,[(u8)0b010001,(u8)'H']
,[(u8)0b010010,(u8)'I']
,[(u8)0b010011,(u8)'J']
,[(u8)0b010100,(u8)'K']
,[(u8)0b010101,(u8)'L']
,[(u8)0b010110,(u8)'M']
,[(u8)0b010111,(u8)'N']
,[(u8)0b011000,(u8)'O']
,[(u8)0b011001,(u8)'P']
,[(u8)0b011010,(u8)'Q']
,[(u8)0b011011,(u8)'R']
,[(u8)0b011100,(u8)'S']
,[(u8)0b011101,(u8)'T']
,[(u8)0b011110,(u8)'U']
,[(u8)0b011111,(u8)'V']
,[(u8)0b100000,(u8)'W']
,[(u8)0b100001,(u8)'X']
,[(u8)0b100010,(u8)'Y']
,[(u8)0b100011,(u8)'Z']
,[(u8)0b100100,(u8)'a']
,[(u8)0b100101,(u8)'b']
,[(u8)0b100110,(u8)'c']
,[(u8)0b100111,(u8)'d']
,[(u8)0b101000,(u8)'e']
,[(u8)0b101001,(u8)'f']
,[(u8)0b101010,(u8)'g']
,[(u8)0b101011,(u8)'h']
,[(u8)0b101100,(u8)'i']
,[(u8)0b101101,(u8)'j']
,[(u8)0b101110,(u8)'k']
,[(u8)0b101111,(u8)'l']
,[(u8)0b110000,(u8)'m']
,[(u8)0b110001,(u8)'n']
,[(u8)0b110010,(u8)'o']
,[(u8)0b110011,(u8)'p']
,[(u8)0b110100,(u8)'q']
,[(u8)0b110101,(u8)'r']
,[(u8)0b110110,(u8)'s']
,[(u8)0b110111,(u8)'t']
,[(u8)0b111000,(u8)'u']
,[(u8)0b111001,(u8)'v']
,[(u8)0b111010,(u8)'w']
,[(u8)0b111011,(u8)'x']
,[(u8)0b111100,(u8)'y']
,[(u8)0b111101,(u8)'z']
,[(u8)0b111110,(u8)'-']
,[(u8)0b111111,(u8)'_']
	];

	public static Dictionary<u8, u8> Num_Char = new Dictionary<u8, u8>();
	public static Dictionary<u8, u8> Char_Num = new Dictionary<u8, u8>();

	static Low64Base(){
		for(var i = 0; i < list2d.Count; i++){
			var line = list2d[i];
			var num = (u8)line[0];
			var char_ = (u8)line[1];
			Num_Char[num] = char_;
			Char_Num[char_] = num;
		}
	}

}



/*

function base64UrlToDecimal(base64UrlString) {
  // 1. 将 Base64URL 转换为标准的 Base64 编码
  let base64 = base64UrlString.replace(/-/g, '+').replace(/_/g, '/');

  // 2. 补齐 Base64 字符串的长度，使其长度是 4 的倍数
  while (base64.length % 4) {
    base64 += '=';
  }

  // 3. 使用 atob() 解码 Base64 字符串
  const decodedString = atob(base64);

  // 4. 将解码后的字符串转换为 BigInt
  let decimalValue = BigInt(0);
  for (let i = 0; i < decodedString.length; i++) {
    decimalValue = (decimalValue * BigInt(256)) + BigInt(decodedString.charCodeAt(i));
  }

  return decimalValue.toString(); // 返回字符串形式的 BigInt
}

// 示例
const base64UrlString = 'AZabdYHHTZPXGVRxYkQw8g';
const decimalString = base64UrlToDecimal(base64UrlString);
console.log(decimalString);//2111225607913229049473489086032654578

 */
