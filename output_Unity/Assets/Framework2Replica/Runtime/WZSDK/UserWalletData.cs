using System;
using Newtonsoft.Json;

namespace WZSDK
{
	[Serializable]
	public class UserWalletData
	{
		[JsonProperty("id")]
		public long Id { get; set; }

		[JsonProperty("afDeviceId")]
		public string AfDeviceId { get; set; }

		[JsonProperty("appIndex")]
		public int AppIndex { get; set; }

		[JsonProperty("balance")]
		public double Balance { get; set; }

		[JsonProperty("frozenBalance")]
		public double FrozenBalance { get; set; }

		[JsonProperty("totalIncomeAmount")]
		public double TotalIncomeAmount { get; set; }

		[JsonProperty("totalWithdrawAmount")]
		public double TotalWithdrawAmount { get; set; }

		[JsonProperty("currency")]
		public string Currency { get; set; }

		[JsonProperty("status")]
		public int Status { get; set; }

		[JsonProperty("payPalAccountExists")]
		public int? PayPalAccountExists { get; set; }

		[JsonProperty("payPalAccountTip")]
		public string PayPalAccountTip { get; set; }

		[JsonProperty("luckySuccessRewardAmount", NullValueHandling = NullValueHandling.Ignore)]
		public double? LuckySuccessRewardAmount { get; set; }

		[JsonProperty("luckySpinRewardAmount", NullValueHandling = NullValueHandling.Ignore)]
		public double? LuckySpinRewardAmount { get; set; }

		[JsonProperty("createTime")]
		public string CreateTime { get; set; }

		[JsonProperty("updateTime")]
		public string UpdateTime { get; set; }
	}
}
