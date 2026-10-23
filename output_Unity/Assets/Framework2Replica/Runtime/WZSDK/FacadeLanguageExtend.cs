using System;

namespace WZSDK
{
	public static class FacadeLanguageExtend
	{
		public static Action<int> OnLanguageChangeHandle;

		public static Func<string, string> GetTextHandle;
	}
}
