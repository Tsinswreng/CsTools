namespace Tsinswreng.CsCore.IF;

using System.Diagnostics;
using System.Runtime.CompilerServices;

public interface I_ShallowCloneSelf{
	public object ShallowCloneSelf()
#if Impl
	{
		return MemberwiseClone();
	}
#else
	;
#endif
}
