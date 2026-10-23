using System;
using UnityEngine;

namespace WZSDK
{
	public static class FacadePayTypeExtend
	{
		public static Func<float, string> RegionalChangeHandle;

		public static Func<int> GetNANPHandle;

		public static Func<string> GetCountryCodeHandle;

		public static Func<string> GetLanguageHandle;

		public static Func<Sprite> GetSingleCoin;

		public static Func<Sprite> GetMultCoinCoinAct;

		public static Func<Sprite> GetSingleGem;

		public static Func<Sprite> GetMulteGem;

		public static Func<Sprite> GetMultCoin;
	}
}
