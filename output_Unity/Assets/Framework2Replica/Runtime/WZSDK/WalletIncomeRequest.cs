using System;
using Newtonsoft.Json;

namespace WZSDK
{
	[Serializable]
	public class WalletIncomeRequest
	{
		[JsonProperty("afDeviceId")]
		public string AfDeviceId { get; set; }

		[JsonProperty("appIndex")]
		public int AppIndex { get; set; }

		[JsonProperty("amount")]
		public double Amount { get; set; }

		[JsonProperty("currency")]
		public string Currency { get; set; }

		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("level", NullValueHandling = NullValueHandling.Ignore)]
		public int? Level { get; set; }
	}
}
