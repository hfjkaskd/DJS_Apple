using System;
using System.Collections.Generic;

namespace PlayerModel
{
	[Serializable]
	public class PlayerAssetSaveData
	{
		public List<PropertyInfo> playerProperties;

		public int totalGoldIncome;

		public int totalGoldSpend;
	}
}
