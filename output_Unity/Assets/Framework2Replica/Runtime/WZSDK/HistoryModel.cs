using System;
using Newtonsoft.Json;

namespace WZSDK
{
	[Serializable]
	public class HistoryModel
	{
		private string id;

		private string dateTime;

		private double money;

		private int stage;

		private int status;

		[JsonProperty("DateTime")]
		public string DateTime
		{
			get => dateTime;
			set => dateTime = value;
		}

		[JsonProperty("Money")]
		public double Money
		{
			get => money;
			set => money = value;
		}

		[JsonProperty("Status")]
		public int Status
		{
			get => status;
			set => status = value;
		}

		[JsonProperty("ID")]
		public string ID
		{
			get => id;
			set => id = value;
		}

		[JsonProperty("Stage")]
		public int Stage
		{
			get => stage;
			set => stage = value;
		}
	}
}
