namespace Tsinswreng.CsTools;

public static class ExtnEnum{

	/// <summary>
	/// Case sensitive
	/// </summary>
	/// <typeparam name="TEnum"></typeparam>
	/// <param name="z"></param>
	/// <param name="o"></param>
	/// <returns></returns>
	public static bool Eq<TEnum>(
		this TEnum z,
		TEnum o
	)where TEnum : struct, Enum
	{
		return z.Equals(o);
	}

	/// <summary>
	/// Case sensitive
	/// </summary>
	/// <typeparam name="TEnum"></typeparam>
	/// <param name="z"></param>
	/// <param name="o"></param>
	/// <returns></returns>
	public static bool Eq<TEnum>(
		this TEnum z,
		str o
	)where TEnum : struct, Enum
	{
		return z.ToString() == o;
	}

	public static bool Eq(this Enum z, obj o){
		if(z.Equals(o)){
			return true;
		}
		if(o is str s && s == z.ToString()){
			return true;
		}
		return false;
	}
}
