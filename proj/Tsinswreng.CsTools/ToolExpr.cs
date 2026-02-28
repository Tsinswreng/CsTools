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
			throw new ArgumentNullException(nameof(ExprMemb), "表达式不能为空");
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
				$"表达式 '{ExprMemb}' 不是有效的成员访问表达式（如 x=>x.属性/字段）",
				nameof(ExprMemb));
		}
		// 返回成员名称（属性/字段名都适用）
		return memberExpr.Member.Name;
	}
	
	///  x=>new{x.Id, x.Name} => ["Id", "Name"]
	/// x=>x.Id => ["Id"]
	/// 兼容AOT
	public static IList<string> GetMemberNames<T>(Expression<Func<T, obj?>> ExprMembs) {
		if (ExprMembs == null) {
			throw new ArgumentNullException(nameof(ExprMembs));
		}

		Expression body = ExprMembs.Body;

		// 处理值类型装箱
		if (body is UnaryExpression unary && unary.NodeType == ExpressionType.Convert && unary.Type == typeof(object)) {
			body = unary.Operand;
		}

		// 情况1：单成员访问 x=>x.Id
		if (body is MemberExpression singleMember) {
			return new List<string> { singleMember.Member.Name };
		}

		// 情况2：匿名对象初始化 x=>new{x.Id, x.Name}
		if (body is NewExpression newExpr) {
			var names = new List<string>(newExpr.Arguments.Count);
			foreach (var arg in newExpr.Arguments) {
				Expression expr = arg;
				// 处理参数内的装箱
				if (expr is UnaryExpression argUnary && argUnary.NodeType == ExpressionType.Convert) {
					expr = argUnary.Operand;
				}
				if (expr is MemberExpression memberExpr) {
					names.Add(memberExpr.Member.Name);
				}
				else {
					throw new ArgumentException(
						$"匿名对象初始化中包含非成员访问表达式: '{arg}'",
						nameof(ExprMembs));
				}
			}
			return names;
		}

		throw new ArgumentException(
			$"表达式 '{ExprMembs}' 必须是成员访问（x=>x.Prop）或匿名对象初始化（x=>new{{x.A, x.B}}）",
			nameof(ExprMembs));
	}
	
	///  x=>new{x.Id, x.Name} => ["Id", "Name"]
	/// 兼容AOT
	static IList<string> GetMemberNamesOnlySupportNewExpr<T>(Expression<Func<T, obj?>> ExprMembs) {
		if (ExprMembs == null) {
			throw new ArgumentNullException(nameof(ExprMembs), "表达式不能为空");
		}

		Expression body = ExprMembs.Body;

		// 处理值类型装箱的情况
		if (body is UnaryExpression unaryExpr && unaryExpr.NodeType == ExpressionType.Convert && unaryExpr.Type == typeof(object)) {
			body = unaryExpr.Operand;
		}

		// 验证是否为匿名类型初始化表达式（NewExpression）
		if (body is not NewExpression newExpr) {
			throw new ArgumentException(
				$"表达式 '{ExprMembs}' 不是有效的匿名对象初始化表达式（如 x=>new{{x.Id, x.Name}}）",
				nameof(ExprMembs));
		}

		// 提取所有成员绑定
		var memberNames = new List<string>();
		
		foreach (var arg in newExpr.Arguments) {
			Expression memberAccess = arg;
			
			// 处理参数中的装箱转换
			if (memberAccess is UnaryExpression argUnary && argUnary.NodeType == ExpressionType.Convert) {
				memberAccess = argUnary.Operand;
			}
			
			// 验证是否为成员访问
			if (memberAccess is not MemberExpression memberExpr) {
				throw new ArgumentException(
					$"表达式包含非成员访问参数: '{arg}'",
					nameof(ExprMembs));
			}
			
			memberNames.Add(memberExpr.Member.Name);
		}
		return memberNames;
	}
}

