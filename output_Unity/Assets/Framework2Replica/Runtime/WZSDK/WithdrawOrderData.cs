using System;
using System.Globalization;
using Newtonsoft.Json;

namespace WZSDK
{
	[Serializable]
	public class WithdrawOrderData
	{
		[JsonProperty("orderNo")]
		public string OrderNo { get; set; }

		[JsonProperty("withdrawType")]
		public string WithdrawType { get; set; }

		[JsonProperty("orderStatus")]
		public int OrderStatus { get; set; }

		[JsonProperty("walletFrozen")]
		public int WalletFrozen { get; set; }

		[JsonProperty("senderBatchId")]
		public string SenderBatchId { get; set; }

		[JsonProperty("senderItemId")]
		public string SenderItemId { get; set; }

		[JsonProperty("payoutBatchId")]
		public string PayoutBatchId { get; set; }

		[JsonProperty("batchStatus")]
		public string BatchStatus { get; set; }

		[JsonProperty("timeCreated")]
		public string TimeCreated { get; set; }

		[JsonProperty("payoutItemId")]
		public string PayoutItemId { get; set; }

		[JsonProperty("transactionId")]
		public string TransactionId { get; set; }

		[JsonProperty("transactionStatus")]
		public string TransactionStatus { get; set; }

		[JsonProperty("receiver")]
		public string Receiver { get; set; }

		[JsonProperty("amount")]
		public string Amount { get; set; }

		[JsonProperty("currency")]
		public string Currency { get; set; }

		[JsonProperty("errorName")]
		public string ErrorName { get; set; }

		[JsonProperty("errorMessage")]
		public string ErrorMessage { get; set; }

		[JsonProperty("lastQueryTime")]
		public string LastQueryTime { get; set; }

		[JsonProperty("successTime")]
		public string SuccessTime { get; set; }

		[JsonProperty("createTime")]
		public string CreateTime { get; set; }

		[JsonProperty("updateTime")]
		public string UpdateTime { get; set; }

		[JsonProperty("clientQueuedPlaceholder", NullValueHandling = NullValueHandling.Ignore)]
		public bool ClientQueuedPlaceholder { get; set; }

		[JsonProperty("localStatusText", NullValueHandling = NullValueHandling.Ignore)]
		public string LocalStatusText { get; set; }

		[JsonProperty("clientSortAnchorTime", NullValueHandling = NullValueHandling.Ignore)]
		public string ClientSortAnchorTime { get; set; }

		public bool IsFirstAdWithdraw => string.Equals(
			WithdrawType,
			WithdrawConstants.WithdrawTypeFirstAd,
			StringComparison.OrdinalIgnoreCase);

		public bool IsWalletWithdraw => string.Equals(
			WithdrawType,
			WithdrawConstants.WithdrawTypeWallet,
			StringComparison.OrdinalIgnoreCase);

		public bool CanClientRetry => OrderStatus == 2 || OrderStatus == 4;

		public bool RequiresNewReceiver => OrderStatus == 2;

		public bool RequiresReceiverInput =>
			OrderStatus == 2 ||
			(!string.IsNullOrEmpty(ErrorMessage) &&
				(ErrorMessage.Contains("PayPal收款账号不能为空") ||
				 ErrorMessage.Contains("PayPal收款账号不存在")));

		public bool IsPending =>
			ClientQueuedPlaceholder || OrderStatus == 0 || OrderStatus == 3;

		public double GetAmountValue()
		{
			double amount;
			return double.TryParse(
				Amount,
				NumberStyles.Float,
				CultureInfo.InvariantCulture,
				out amount)
				? amount
				: 0.0;
		}

		public string GetSortAnchorTimeText()
		{
			if (!string.IsNullOrEmpty(ClientSortAnchorTime))
			{
				return ClientSortAnchorTime;
			}
			if (!string.IsNullOrEmpty(CreateTime))
			{
				return CreateTime;
			}
			if (!string.IsNullOrEmpty(TimeCreated))
			{
				return TimeCreated;
			}
			if (!string.IsNullOrEmpty(UpdateTime))
			{
				return UpdateTime;
			}
			return SuccessTime;
		}

		public bool TryGetSortAnchorTime(out DateTimeOffset time)
		{
			time = default(DateTimeOffset);
			string[] candidates =
			{
				ClientSortAnchorTime,
				CreateTime,
				TimeCreated,
				UpdateTime,
				SuccessTime
			};
			for (int i = 0; i < candidates.Length; i++)
			{
				if (DateTimeOffset.TryParse(
					candidates[i],
					CultureInfo.InvariantCulture,
					DateTimeStyles.AssumeLocal,
					out time))
				{
					return true;
				}
			}
			return false;
		}

		public string GetBestTimeText()
		{
			if (!string.IsNullOrEmpty(SuccessTime))
			{
				return SuccessTime;
			}
			if (!string.IsNullOrEmpty(UpdateTime))
			{
				return UpdateTime;
			}
			return !string.IsNullOrEmpty(CreateTime)
				? CreateTime
				: TimeCreated;
		}

		public string GetStatusText()
		{
			if (!string.IsNullOrEmpty(LocalStatusText))
			{
				return LocalStatusText;
			}
			switch (OrderStatus)
			{
				case 0:
					return "未打款";
				case 1:
					return "打款成功";
				case 2:
					return "收款账户不存在";
				case 3:
					return "打款中";
				case 4:
					return "提现失败";
				case 5:
					return "PayPal余额不足";
				default:
					return "未知状态";
			}
		}
	}
}
