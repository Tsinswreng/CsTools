using System.Linq.Expressions;

namespace Tsinswreng.CsTools;

public static class ToolExpr {

	extension<T>(Expression<Func<T, obj?>> z){
		public str MemberName{
			get => GetMemberName(z);
		}
	}

	/// <summary>
	/// 从表达式树中提取成员名称（属性/字段），兼容AOT编译
	/// </summary>
	/// <typeparam name="T">目标类型</typeparam>
	/// <param name="getMember">成员访问表达式（如 x=>x.Message）</param>
	/// <returns>成员名称（如 "Message"）</returns>
	/// <exception cref="ArgumentException">表达式不是有效的成员访问时抛出</exception>
	public static string GetMemberName<T>(Expression<Func<T, object?>> getMember) {
		if (getMember == null) {
			throw new ArgumentNullException(nameof(getMember), "表达式不能为空");
		}

		// 核心逻辑：解析表达式树，处理装箱转换（值类型转object）的情况
		Expression body = getMember.Body;

		// 处理值类型装箱的情况（比如 x=>x.Id 中Id是int，会生成UnaryExpression）
		if (body is UnaryExpression unaryExpr && unaryExpr.NodeType == ExpressionType.Convert && unaryExpr.Type == typeof(object)) {
			body = unaryExpr.Operand; // 提取真正的成员表达式
		}

		// 验证是否为成员访问表达式（属性/字段）
		if (body is not MemberExpression memberExpr) {
			throw new ArgumentException(
				$"表达式 '{getMember}' 不是有效的成员访问表达式（如 x=>x.属性/字段）",
				nameof(getMember));
		}
		// 返回成员名称（属性/字段名都适用）
		return memberExpr.Member.Name;
	}
}


