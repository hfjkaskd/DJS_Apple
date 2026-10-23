using System;
using Newtonsoft.Json;

namespace WZSDK
{
	[Serializable]
	public class WithdrawApiResponse<T>
	{
		[JsonProperty("code")]
		public int Code { get; set; }

		[JsonProperty("msg")]
		public string Msg { get; set; }

		[JsonProperty("data")]
		public T Data { get; set; }

		[JsonProperty("ext")]
		public string Ext { get; set; }
	}
}
