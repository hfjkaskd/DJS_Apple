using System;
using Newtonsoft.Json;

namespace WZSDK
{
	[Serializable]
	public class LuckyRewardConfigData
	{
		[JsonProperty("luckyAmount")]
		public double? LuckyAmount { get; set; }

		[JsonProperty("luckySuccessRewardAmount")]
		public double? LuckySuccessRewardAmount { get; set; }

		[JsonProperty("luckySpinRewardAmount")]
		public double? LuckySpinRewardAmount { get; set; }

		[JsonProperty("currency")]
		public string Currency { get; set; }
	}
}
