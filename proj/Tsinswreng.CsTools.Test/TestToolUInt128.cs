// using System;
// using Xunit;
// using Tsinswreng.CsTools;

// public class ToolUInt128Tests
// {
//     /* -------------------------------------------------
//      * ToBase64Url / Base64UrlToUInt128  往返测试
//      * -------------------------------------------------*/
//     [Theory]
//     [InlineData(0UL)]                           // 全 0
//     [InlineData(1UL)]                           // 最小非 0
//     [InlineData(ulong.MaxValue)]                // 仅低 64 位全 1
//     [InlineData("ffffffffffffffffffffffffffffffff", 16)] // 全 1
//     [InlineData("0123456789abcdefdeadbeefcafebabe", 16)] // 随机大数
//     public void Base64Url_RoundTrip(string hexOrUlong)
//     {
//         UInt128 original = hexOrUlong.StartsWith("0x") || hexOrUlong.Length > 20
//             ? UInt128.Parse(hexOrUlong, System.Globalization.NumberStyles.HexNumber)
//             : UInt128.Parse(hexOrUlong);

//         string encoded = ToolUInt128.ToBase64Url(original);
//         UInt128 decoded = ToolUInt128.Base64UrlToUInt128(encoded);

//         Assert.Equal(original, decoded);
//     }

//     /* -------------------------------------------------
//      * 手动指定的 base64url 反序列化验证
//      * -------------------------------------------------*/
//     [Theory]
//     [InlineData("AAAAAAAAAAAAAAAAAAAAAA", 0)]                         // 16 字节全 0
//     [InlineData("_____________________w", ulong.MaxValue)]            // 仅低 64 位为 1
//     [InlineData("AP_____________________w", 1UL << 64 | ulong.MaxValue)]
//     public void Base64UrlToUInt128_Manual(string b64u, ulong expectedLow)
//     {
//         UInt128 expected = expectedLow;
//         UInt128 actual = ToolUInt128.Base64UrlToUInt128(b64u);
//         Assert.Equal(expected, actual);
//     }

//     /* -------------------------------------------------
//      * 非法 base64url 输入
//      * -------------------------------------------------*/
//     [Theory]
//     [InlineData("A")]           // 长度不足
//     [InlineData("A=")]          // 填充符被提前剪掉
//     [InlineData("A@@")]         // 非法字符
//     public void Base64UrlToUInt128_Invalid(string b64u)
//     {
//         Assert.Throws<FormatException>(() => ToolUInt128.Base64UrlToUInt128(b64u));
//     }

//     /* -------------------------------------------------
//      * BytesToUInt128 / ByteArrToUInt128
//      * -------------------------------------------------*/
//     [Fact]
//     public void BytesToUInt128_RoundTrip()
//     {
//         var bytes = Convert.FromHexString("0123456789ABCDEF0123456789ABCDEF");
//         UInt128 value = ToolUInt128.BytesToUInt128(bytes);
//         byte[] back = ToolUInt128.ToByteArr(value);

//         Assert.Equal(bytes, back);
//     }

//     [Fact]
//     public void BytesToUInt128_WrongLength_Throws()
//     {
//         Assert.Throws<ArgumentException>(() => ToolUInt128.BytesToUInt128(new byte[15]));
//     }

//     /* -------------------------------------------------
//      * ToLow64Base / Low64BaseToUInt128
//      * -------------------------------------------------*/
//     [Theory]
//     [InlineData(0, "0")]
//     [InlineData(1, "1")]
//     [InlineData(63, "-")]
//     [InlineData(64, "10")]
//     [InlineData(0xFFFFFFFFFFFFFFFF, "-__________")] // 64 位全 1
//     [InlineData((UInt128)0xFFFFFFFFFFFFFFFF << 64, "1__________0")] // 高位为 1，低位为 0
//     public void Low64Base_RoundTrip(UInt128 value, string _)
//     {
//         string encoded = ToolUInt128.ToLow64Base(value);
//         UInt128 decoded = ToolUInt128.Low64BaseToUInt128(encoded);

//         Assert.Equal(value, decoded);
//     }

//     [Theory]
//     [InlineData("")]        // 空串
//     [InlineData("!")]       // 非法字符
//     public void Low64BaseToUInt128_Invalid(string text)
//     {
//         Assert.Throws<KeyNotFoundException>(() => ToolUInt128.Low64BaseToUInt128(text));
//     }
// }
