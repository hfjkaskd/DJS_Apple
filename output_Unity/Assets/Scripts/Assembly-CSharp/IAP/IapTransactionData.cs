using System;

namespace IAP
{
	[Serializable]
	public class IapTransactionData
	{
		public string productId;

		public string transactionId;

		public string orderId;

		public string purchaseToken;
	}
}
