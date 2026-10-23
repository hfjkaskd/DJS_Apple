using System;
using Newtonsoft.Json;

namespace WZSDK
{
	[Serializable]
	public class LuckyRewardConfigRequest
	{
		[JsonProperty("afDeviceId")]
		public string AfDeviceId { get; set; }

		[JsonProperty("appIndex")]
		public int AppIndex { get; set; }

		[JsonProperty("currency")]
		public string Currency { get; set; }
	}
}
