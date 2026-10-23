using System;
using Newtonsoft.Json;

namespace WZSDK
{
	[Serializable]
	public class ClientWithdrawRequest
	{
		[JsonProperty("orderNo", NullValueHandling = NullValueHandling.Ignore)]
		public string OrderNo { get; set; }

		[JsonProperty("afDeviceId")]
		public string AfDeviceId { get; set; }

		[JsonProperty("appIndex")]
		public int AppIndex { get; set; }

		[JsonProperty("withdrawType", NullValueHandling = NullValueHandling.Ignore)]
		public string WithdrawType { get; set; }

		[JsonProperty("receiver", NullValueHandling = NullValueHandling.Ignore)]
		public string Receiver { get; set; }

		[JsonProperty("amount", NullValueHandling = NullValueHandling.Ignore)]
		public double? Amount { get; set; }

		[JsonProperty("currency", NullValueHandling = NullValueHandling.Ignore)]
		public string Currency { get; set; }

		[JsonProperty("note", NullValueHandling = NullValueHandling.Ignore)]
		public string Note { get; set; }

		[JsonProperty("emailSubject", NullValueHandling = NullValueHandling.Ignore)]
		public string EmailSubject { get; set; }

		[JsonProperty("recipientType", NullValueHandling = NullValueHandling.Ignore)]
		public string RecipientType { get; set; }
	}
}
