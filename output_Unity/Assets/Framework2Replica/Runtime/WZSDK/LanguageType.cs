using System;
using System.Globalization;

namespace WZSDK
{
	public enum LanguageType
	{
		Chinese = 0,
		English = 1,
		Japanese = 2,
		Brazilian = 3,
		Indonesian = 4,
		Korean = 5
	}

	internal static class LanguageTypeContract
	{
		public static string GetCode(LanguageType value)
		{
			switch (value)
			{
				case LanguageType.Chinese: return "zh-CN";
				case LanguageType.English: return "en-US";
				case LanguageType.Japanese: return "ja-JP";
				case LanguageType.Brazilian: return "pt-BR";
				case LanguageType.Indonesian: return "id-ID";
				case LanguageType.Korean: return "ko-KR";
				default:
					return "unknown(" +
						((int)value).ToString(CultureInfo.InvariantCulture) + ")";
			}
		}

		public static bool TryParseCode(
			string code,
			out LanguageType value)
		{
			if (string.Equals(code, "zh-CN", StringComparison.OrdinalIgnoreCase))
			{
				value = LanguageType.Chinese;
				return true;
			}
			if (string.Equals(code, "en-US", StringComparison.OrdinalIgnoreCase) || code == "en")
			{
				value = LanguageType.English;
				return true;
			}
			if (string.Equals(code, "ja-JP", StringComparison.OrdinalIgnoreCase) || code == "ja")
			{
				value = LanguageType.Japanese;
				return true;
			}
			if (string.Equals(code, "pt-BR", StringComparison.OrdinalIgnoreCase) || code == "pt")
			{
				value = LanguageType.Brazilian;
				return true;
			}
			if (string.Equals(code, "id-ID", StringComparison.OrdinalIgnoreCase) || code == "id")
			{
				value = LanguageType.Indonesian;
				return true;
			}
			if (string.Equals(code, "ko-KR", StringComparison.OrdinalIgnoreCase) || code == "ko")
			{
				value = LanguageType.Korean;
				return true;
			}

			value = LanguageType.English;
			return false;
		}
	}
}
