using System.Linq.Expressions;

namespace Tsinswreng.CsTools;

public static class ToolExpr {

	extension<T>(Expression<Func<T, obj?>> z){
		public str MemberName{
			get => GetMemberName(z);
		}
	}

	/// 从表达式树中提取成员名称（属性/字段），兼容AOT编译
	/// <typeparam name="T">目标类型</typeparam>
	/// <param name="ExprMemb">成员访问表达式（如 x=>x.Message）</param>
	/// <returns>成员名称（如 "Message"）</returns>
	/// <exception cref="ArgumentException">表达式不是有效的成员访问时抛出</exception>
	public static string GetMemberName<T>(Expression<Func<T, obj?>> ExprMemb) {
		if (ExprMemb == null) {
			throw new ArgumentNullException(nameof(ExprMemb), "Expression cannot be null");
		}

		// 核心逻辑：解析表达式树，处理装箱转换（值类型转object）的情况
		Expression body = ExprMemb.Body;

		// 处理值类型装箱的情况（比如 x=>x.Id 中Id是int，会生成UnaryExpression）
		if (body is UnaryExpression unaryExpr && unaryExpr.NodeType == ExpressionType.Convert && unaryExpr.Type == typeof(object)) {
			body = unaryExpr.Operand; // 提取真正的成员表达式
		}

		// 验证是否为成员访问表达式（属性/字段）
		if (body is not MemberExpression memberExpr) {
			throw new ArgumentException(
				$"Expression '{ExprMemb}' is not a valid member access expression (e.g., x=>x.Property/Field)",
				nameof(ExprMemb));
		}
		// 返回成员名称（属性/字段名都适用）
		return memberExpr.Member.Name;
	}
	
	/// 尝试从单成员访问提取成员名称 x=>x.Id => ["Id"]
	/// <returns>成功返回成员名称列表，失败返回null</returns>
	public static IList<string>? TryGetMemberNamesFromSingleMember<T>(Expression<Func<T, obj?>> expr) {
		Expression body = expr.Body;
		
		// 处理值类型装箱
		if (body is UnaryExpression unary && unary.NodeType == ExpressionType.Convert && unary.Type == typeof(object)) {
			body = unary.Operand;
		}
		
		if (body is MemberExpression singleMember) {
			return new List<string> { singleMember.Member.Name };
		}
		
		return null;
	}

	/// 尝试从匿名对象初始化提取成员名称 x=>new{x.Id, x.Name} => ["Id", "Name"]
	/// <returns>成功返回成员名称列表，失败返回null</returns>
	public static IList<string>? TryGetMemberNamesFromNewExpr<T>(Expression<Func<T, obj?>> expr) {
		Expression body = expr.Body;
		
		// 处理值类型装箱
		if (body is UnaryExpression unary && unary.NodeType == ExpressionType.Convert && unary.Type == typeof(object)) {
			body = unary.Operand;
		}
		
		if (body is not NewExpression newExpr) {
			return null;
		}
		
		var names = new List<string>(newExpr.Arguments.Count);
		foreach (var arg in newExpr.Arguments) {
			Expression argExpr = arg;
			// 处理参数内的装箱
			if (argExpr is UnaryExpression argUnary && argUnary.NodeType == ExpressionType.Convert) {
				argExpr = argUnary.Operand;
			}
			if (argExpr is MemberExpression memberExpr) {
				names.Add(memberExpr.Member.Name);
			}
			else {
				throw new ArgumentException(
					$"Object initialization contains non-member access expression: '{arg}'",
					nameof(expr));
			}
		}
		return names;
	}

	/// 尝试从数组初始化提取成员名称 x=>new[]{x.Id, x.Name} => ["Id", "Name"]
	/// <returns>成功返回成员名称列表，失败返回null</returns>
	public static IList<string>? TryGetMemberNamesFromArrExpr<T>(Expression<Func<T, obj?>> expr) {
		Expression body = expr.Body;
		
		// 处理值类型装箱
		if (body is UnaryExpression unary && unary.NodeType == ExpressionType.Convert && unary.Type == typeof(object)) {
			body = unary.Operand;
		}
		
		if (body is not NewArrayExpression arrayExpr) {
			return null;
		}
		
		var names = new List<string>(arrayExpr.Expressions.Count);
		foreach (var element in arrayExpr.Expressions) {
			Expression elemExpr = element;
			// 处理元素内的装箱
			if (elemExpr is UnaryExpression elemUnary && elemUnary.NodeType == ExpressionType.Convert) {
				elemExpr = elemUnary.Operand;
			}
			if (elemExpr is MemberExpression memberExpr) {
				names.Add(memberExpr.Member.Name);
			}
			else {
				throw new ArgumentException(
					$"Array initialization contains non-member access expression: '{element}'",
					nameof(expr));
			}
		}
		return names;
	}
	
	///  x=>new{x.Id, x.Name} => ["Id", "Name"]
	/// x=>new[]{x.Id, x.Name} => ["Id", "Name"]
	/// x=>x.Id => ["Id"]
	/// 兼容AOT
	public static IList<string> GetMemberNames<T>(Expression<Func<T, obj?>> ExprMembs) {
		if (ExprMembs == null) {
			throw new ArgumentNullException(nameof(ExprMembs));
		}

		// 尝试各种表达式类型
		var result = TryGetMemberNamesFromSingleMember(ExprMembs) 
			?? TryGetMemberNamesFromNewExpr(ExprMembs)
			?? TryGetMemberNamesFromArrExpr(ExprMembs);

		if (result != null) {
			return result;
		}

		throw new ArgumentException(
			$"Expression '{ExprMembs}' must be a member access (x=>x.Prop), anonymous object initialization (x=>new{{x.A, x.B}}), or array initialization (x=>new[]{{x.A, x.B}})",
			nameof(ExprMembs));
	}
	
}

