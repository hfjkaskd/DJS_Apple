using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace WZSDK
{
	/// <summary>
	/// Player-owned wallet and withdrawal persistence.
	/// All operations are resolved from local state and never leave the device.
	/// </summary>
	public static class WithdrawApiService
	{
		private const string LuckyRewardConfigCachePrefsKey =
			"wallet_lucky_reward_config_cache";

		private const string FirstAdQueuedStatusText = "Queued";

		private const string WalletReviewStatusText = "Under review";

		private static float DefaultLuckyRewardAmount => Framework2Host.Settings.economy.rewards.localLuckyRewardAmount;

		private const string WalletLogPrefix = "[LocalWallet]";

		private const string PaymentNoticeLogPrefix = "[PaymentNotice]";

		public static event Action<double> WalletBalanceChanged;

		private static readonly JsonSerializerSettings JsonSettings =
			new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore
			};

		public static UserWalletData GetCachedWallet()
		{
			string json = SPlayerPrefs.GetString(
				FacadePlayerPrefExtend.WalletBalanceCache,
				string.Empty);
			if (string.IsNullOrEmpty(json))
			{
				return null;
			}

			try
			{
				return JsonConvert.DeserializeObject<UserWalletData>(json);
			}
			catch (Exception exception)
			{
				Debug.LogError(
					WalletLogPrefix + " wallet cache could not be read: " +
					exception.Message);
				return null;
			}
		}

		public static List<WithdrawOrderData> GetCachedOrders()
		{
			string json = SPlayerPrefs.GetString(
				FacadePlayerPrefExtend.WalletWithdrawOrdersCache,
				string.Empty);
			if (string.IsNullOrEmpty(json))
			{
				return new List<WithdrawOrderData>();
			}

			try
			{
				return JsonConvert.DeserializeObject<List<WithdrawOrderData>>(
					json) ?? new List<WithdrawOrderData>();
			}
			catch (Exception exception)
			{
				Debug.LogError(
					WalletLogPrefix + " order cache could not be read: " +
					exception.Message);
				return new List<WithdrawOrderData>();
			}
		}

		public static WithdrawOrderData GetLatestOrderForTesting()
		{
			List<WithdrawOrderData> orders = SortOrders(GetCachedOrders());
			return orders.Count > 0 ? orders[0] : null;
		}

		public static void ClearWalletAndOrdersForTesting()
		{
			SPlayerPrefs.Delete(FacadePlayerPrefExtend.WalletBalanceCache);
			SPlayerPrefs.Delete(
				FacadePlayerPrefExtend.WalletWithdrawOrdersCache);
			SPlayerPrefs.Save();
			GameDefines.LuckyChestPlaceholderCoin = 0f;
			WalletBalanceChanged?.Invoke(0.0);
		}

		public static void SetWalletBalanceForTesting(double value)
		{
			UserWalletData wallet = GetOrCreateLocalWallet(
				WithdrawConstants.CurrencyUsd);
			wallet.Balance = RoundAmount(Math.Max(0.0, value));
			wallet.TotalIncomeAmount = RoundAmount(Math.Max(
				wallet.TotalIncomeAmount,
				wallet.Balance + Math.Max(0.0, wallet.TotalWithdrawAmount)));
			wallet.Status = 1;
			wallet.UpdateTime = GetUtcTimestamp();
			CacheWallet(wallet);
		}

		public static bool AddWalletBalanceForTesting(double amount)
		{
			if (double.IsNaN(amount) || double.IsInfinity(amount) || amount <= 0.0)
			{
				return false;
			}

			UserWalletData wallet = GetOrCreateLocalWallet(
				WithdrawConstants.CurrencyUsd);
			ApplyWalletIncome(wallet, amount, WithdrawConstants.CurrencyUsd);
			CacheWallet(wallet);
			return true;
		}

		public static bool ResolveLatestOrderForTesting(int orderStatus)
		{
			if (orderStatus < 0 || orderStatus > 5)
			{
				return false;
			}

			List<WithdrawOrderData> orders = SortOrders(GetCachedOrders());
			if (orders.Count == 0 || orders[0] == null ||
				(orders[0].ClientQueuedPlaceholder && orderStatus != 0))
			{
				return false;
			}

			WithdrawOrderData order = orders[0];
			ApplyWalletOrderResolutionForTesting(order, orderStatus);
			order.OrderStatus = orderStatus;
			order.LocalStatusText = null;
			order.UpdateTime = GetLocalTimestamp();
			order.LastQueryTime = order.UpdateTime;
			order.ErrorName = null;
			order.ErrorMessage = null;
			order.SuccessTime = null;
			if (orderStatus == 1)
			{
				order.SuccessTime = order.UpdateTime;
			}
			else if (orderStatus == 2)
			{
				order.ErrorName = "RECEIVER_NOT_FOUND";
				order.ErrorMessage =
					"PayPal receiver account does not exist.";
			}
			else if (orderStatus == 4)
			{
				order.ErrorName = "PAYOUT_FAILED";
				order.ErrorMessage = "Withdrawal failed.";
			}
			else if (orderStatus == 5)
			{
				order.ErrorName = "PAYOUT_BALANCE_INSUFFICIENT";
				order.ErrorMessage = "PayPal balance is insufficient.";
			}

			CacheOrders(orders);
			return true;
		}

		private static void ApplyWalletOrderResolutionForTesting(
			WithdrawOrderData order,
			int orderStatus)
		{
			if (order == null || !order.IsWalletWithdraw ||
				order.WalletFrozen != 1 ||
				(orderStatus != 1 && orderStatus != 5))
			{
				return;
			}

			UserWalletData wallet = GetCachedWallet();
			if (wallet == null)
			{
				return;
			}

			double amount = Math.Max(0.0, order.GetAmountValue());
			wallet.FrozenBalance = RoundAmount(
				Math.Max(0.0, wallet.FrozenBalance - amount));
			if (orderStatus == 5)
			{
				wallet.Balance = RoundAmount(wallet.Balance + amount);
				wallet.TotalWithdrawAmount = RoundAmount(
					Math.Max(0.0, wallet.TotalWithdrawAmount - amount));
			}
			wallet.UpdateTime = GetUtcTimestamp();
			order.WalletFrozen = 0;
			CacheWallet(wallet);
		}

		public static bool TryGetFirstAdOrderCreateTime(
			out DateTimeOffset time)
		{
			time = default(DateTimeOffset);
			WithdrawOrderData fallback = null;
			List<WithdrawOrderData> orders = GetCachedOrders();
			for (int i = 0; i < orders.Count; i++)
			{
				WithdrawOrderData order = orders[i];
				if (order == null || !order.IsFirstAdWithdraw)
				{
					continue;
				}

				if (order.ClientQueuedPlaceholder)
				{
					return order.TryGetSortAnchorTime(out time);
				}

				if (fallback == null)
				{
					fallback = order;
				}
			}

			return fallback != null && fallback.TryGetSortAnchorTime(out time);
		}

		public static double GetCachedWalletBalance()
		{
			UserWalletData wallet = GetCachedWallet();
			return wallet == null
				? GameDefines.LuckyChestPlaceholderCoin
				: wallet.Balance;
		}

		public static LuckyRewardConfigData GetCachedLuckyRewardConfig()
		{
			string json = SPlayerPrefs.GetString(
				LuckyRewardConfigCachePrefsKey,
				string.Empty);
			if (string.IsNullOrEmpty(json))
			{
				return null;
			}

			try
			{
				LuckyRewardConfigData config = JsonConvert.DeserializeObject<LuckyRewardConfigData>(json);
				if (config != null)
				{
					// Reward configuration belongs to the SO, not the player's old
					// configuration cache. Wallet balances and orders are untouched.
					config.LuckyAmount = DefaultLuckyRewardAmount;
					config.LuckySuccessRewardAmount = null;
					config.LuckySpinRewardAmount = null;
				}
				return config;
			}
			catch (Exception exception)
			{
				Debug.LogError(
					WalletLogPrefix + " reward cache could not be read: " +
					exception.Message);
				return null;
			}
		}

		public static float GetCachedLuckySuccessRewardAmount(
			float fallbackAmount)
		{
			LuckyRewardConfigData config = GetCachedLuckyRewardConfig();
			double? reward = config != null
				? config.LuckyAmount ?? config.LuckySuccessRewardAmount
				: GetCachedWallet()?.LuckySuccessRewardAmount;
			return ResolveLuckyRewardAmount(reward, fallbackAmount);
		}

		public static float GetCachedLuckySpinRewardAmount(
			float fallbackAmount)
		{
			LuckyRewardConfigData config = GetCachedLuckyRewardConfig();
			double? reward = config != null
				? config.LuckySpinRewardAmount
				: GetCachedWallet()?.LuckySpinRewardAmount;
			return ResolveLuckyRewardAmount(reward, fallbackAmount);
		}

		public static string GetCachedPayPalTip()
		{
			return GetCachedWallet()?.PayPalAccountTip;
		}

		public static int? GetCachedPayPalAccountExists()
		{
			return GetCachedWallet()?.PayPalAccountExists;
		}

		public static void LogPaymentNotice(
			string source,
			string message,
			LogType logType = LogType.Warning)
		{
			if (string.IsNullOrEmpty(message))
			{
				return;
			}

			string owner = string.IsNullOrEmpty(source) ? "Unknown" : source;
			string text = PaymentNoticeLogPrefix + "[" + owner + "] " + message;
			if (logType == LogType.Error || logType == LogType.Exception)
			{
				Debug.LogError(text);
			}
			else if (logType == LogType.Log)
			{
				Debug.Log(text);
			}
			else
			{
				Debug.LogWarning(text);
			}
		}

		public static void AddWalletIncome(
			float amount,
			bool showError = false)
		{
			AddWalletIncomeInternal(amount, showError, "unspecified", -1);
		}

		public static void AddWalletIncome(
			float amount,
			string incomeTypeOverride,
			bool showError = false)
		{
			AddWalletIncomeInternal(amount, showError, incomeTypeOverride, -1);
		}

		public static void AddWalletIncome(
			float amount,
			string incomeTypeOverride,
			int incomeLevelOverride,
			bool showError = false)
		{
			AddWalletIncomeInternal(amount, showError, incomeTypeOverride, incomeLevelOverride);
		}

		private static void AddWalletIncomeInternal(
			double amount,
			bool showError,
			string incomeType,
			int incomeLevel)
		{
			string analyticsOperation = MahjongProductAnalytics.Begin("f2_wallet_income", "amount", amount, "income_type", incomeType, "income_level", incomeLevel, "source", "local");
			if (amount <= 0.0)
			{
				MahjongProductAnalytics.Track("f2_wallet_income_result", "operation_id", analyticsOperation, "result", "rejected", "reason", "non_positive_amount");
				return;
			}

			if (!CanAddWalletIncomeAtCurrentLevel())
			{
				MahjongProductAnalytics.Track("f2_wallet_income_result", "operation_id", analyticsOperation, "result", "rejected", "reason", "wallet_locked");
				Debug.Log(
					WalletLogPrefix + " income ignored below wallet unlock level.");
				return;
			}

			try
			{
				UserWalletData wallet = GetOrCreateLocalWallet(
					WithdrawConstants.CurrencyUsd);
				double analyticsBefore = wallet.Balance;
				ApplyWalletIncome(wallet, amount, WithdrawConstants.CurrencyUsd);
				CacheWallet(wallet);
				MahjongProductAnalytics.Track("f2_wallet_income_result", "operation_id", analyticsOperation, "result", "credited", "before", analyticsBefore, "after", wallet.Balance, "actual_delta", wallet.Balance - analyticsBefore, "currency", wallet.Currency);
			}
			catch (Exception exception)
			{
				MahjongProductAnalytics.Track("f2_wallet_income_result", "operation_id", analyticsOperation, "result", "failed", "reason", "local_storage_error");
				Debug.LogError(
					WalletLogPrefix + " income could not be stored: " +
					exception.Message);
				if (showError)
				{
					LogPaymentNotice(
						"WalletIncome",
						exception.Message,
						LogType.Warning);
				}
			}
		}

		public static IEnumerator QueryLuckySuccessRewardAmount(
			float fallbackAmount,
			Action<float> onSuccess,
			Action<string> onError = null)
		{
			string analyticsOperation = MahjongProductAnalytics.Begin("f2_lucky_success_amount_query", "source", "local");
			yield return null;
			try
			{
				LuckyRewardConfigData config =
					GetOrCreateLocalLuckyRewardConfig(
						WithdrawConstants.CurrencyUsd);
				double? reward = config.LuckyAmount ??
					config.LuckySuccessRewardAmount;
				MahjongProductAnalytics.Track("f2_lucky_success_amount_query_result", "operation_id", analyticsOperation, "result", "resolved", "configured_amount", reward, "fallback", fallbackAmount, "amount", ResolveLuckyRewardAmount(reward, fallbackAmount));
				onSuccess?.Invoke(
					ResolveLuckyRewardAmount(reward, fallbackAmount));
			}
			catch (Exception exception)
			{
				onSuccess?.Invoke(fallbackAmount);
				MahjongProductAnalytics.Track("f2_lucky_success_amount_query_result", "operation_id", analyticsOperation, "result", "failed");
				onError?.Invoke(exception.Message);
			}
		}

		public static IEnumerator QueryLuckySpinRewardAmount(
			float fallbackAmount,
			Action<float> onSuccess,
			Action<string> onError = null)
		{
			string analyticsOperation = MahjongProductAnalytics.Begin("f2_lucky_spin_amount_query", "source", "local");
			yield return null;
			try
			{
				LuckyRewardConfigData config =
					GetOrCreateLocalLuckyRewardConfig(
						WithdrawConstants.CurrencyUsd);
				MahjongProductAnalytics.Track("f2_lucky_spin_amount_query_result", "operation_id", analyticsOperation, "result", "resolved", "configured_amount", config.LuckySpinRewardAmount, "fallback", fallbackAmount, "amount", ResolveLuckyRewardAmount(config.LuckySpinRewardAmount, fallbackAmount));
				onSuccess?.Invoke(
					ResolveLuckyRewardAmount(
						config.LuckySpinRewardAmount,
						fallbackAmount));
			}
			catch (Exception exception)
			{
				onSuccess?.Invoke(fallbackAmount);
				MahjongProductAnalytics.Track("f2_lucky_spin_amount_query_result", "operation_id", analyticsOperation, "result", "failed");
				onError?.Invoke(exception.Message);
			}
		}

		public static IEnumerator GetConfiguredLuckyReward(
			Action<LuckyRewardConfigData> onSuccess,
			Action<string> onError = null,
			string currency = WithdrawConstants.CurrencyUsd,
			int? luckyLevelOverride = null)
		{
			_ = luckyLevelOverride;
			string analyticsOperation = MahjongProductAnalytics.Begin("f2_lucky_config_query", "source", "local");
			yield return null;
			try
			{
				LuckyRewardConfigData analyticsConfig = GetOrCreateLocalLuckyRewardConfig(currency);
				MahjongProductAnalytics.Track("f2_lucky_config_query_result", "operation_id", analyticsOperation, "result", "resolved", "lucky_amount", analyticsConfig.LuckyAmount, "spin_amount", analyticsConfig.LuckySpinRewardAmount, "success_amount", analyticsConfig.LuckySuccessRewardAmount, "currency", analyticsConfig.Currency);
				onSuccess?.Invoke(analyticsConfig);
			}
			catch (Exception exception)
			{
				MahjongProductAnalytics.Track("f2_lucky_config_query_result", "operation_id", analyticsOperation, "result", "failed");
				onError?.Invoke(exception.Message);
			}
		}

		public static IEnumerator QueryWalletBalance(
			Action<UserWalletData> onSuccess,
			Action<string> onError = null,
			string currency = WithdrawConstants.CurrencyUsd)
		{
			string analyticsOperation = MahjongProductAnalytics.Begin("f2_wallet_query", "source", "local");
			yield return null;
			try
			{
				UserWalletData analyticsWallet = GetOrCreateLocalWallet(currency);
				TrackWallet("query", analyticsWallet);
				MahjongProductAnalytics.Track("f2_wallet_query_result", "operation_id", analyticsOperation, "result", "resolved", "available", analyticsWallet.Balance, "frozen", analyticsWallet.FrozenBalance, "currency", analyticsWallet.Currency);
				onSuccess?.Invoke(analyticsWallet);
			}
			catch (Exception exception)
			{
				MahjongProductAnalytics.Track("f2_wallet_query_result", "operation_id", analyticsOperation, "result", "failed");
				onError?.Invoke(exception.Message);
			}
		}

		public static IEnumerator ReportWalletIncome(
			double amount,
			Action<UserWalletData> onSuccess,
			Action<string> onError = null,
			string currency = WithdrawConstants.CurrencyUsd,
			string incomeTypeOverride = null)
		{
			_ = incomeTypeOverride;
			string analyticsOperation = MahjongProductAnalytics.Begin("f2_wallet_income_report", "source", "local");
			yield return null;
			if (amount <= 0.0)
			{
				MahjongProductAnalytics.Track("f2_wallet_income_report_result", "operation_id", analyticsOperation, "result", "failed");
				onError?.Invoke("Wallet income must be greater than zero.");
				yield break;
			}

			try
			{
				UserWalletData wallet = GetOrCreateLocalWallet(currency);
				double analyticsBefore = wallet.Balance;
				ApplyWalletIncome(wallet, amount, currency);
				CacheWallet(wallet);
				MahjongProductAnalytics.Track("f2_wallet_income_report_result", "operation_id", analyticsOperation, "result", "credited", "before", analyticsBefore, "after", wallet.Balance, "amount", amount, "currency", currency, "income_type", incomeTypeOverride);
				onSuccess?.Invoke(wallet);
			}
			catch (Exception exception)
			{
				MahjongProductAnalytics.Track("f2_wallet_income_report_result", "operation_id", analyticsOperation, "result", "failed");
				onError?.Invoke(exception.Message);
			}
		}

		public static IEnumerator SubmitWithdraw(
			ClientWithdrawRequest submission,
			Action<WithdrawOrderData> onSuccess,
			Action<string> onError = null)
		{
			string analyticsOperation = MahjongProductAnalytics.Begin("f2_withdraw_submit", "source", "local");
			yield return null;
			if (submission == null)
			{
				MahjongProductAnalytics.Track("f2_withdraw_submit_result", "operation_id", analyticsOperation, "result", "failed");
				onError?.Invoke("Withdrawal data is missing.");
				yield break;
			}

			try
			{
				MahjongProductAnalytics.Track("f2_withdraw_submit_payload", "operation_id", analyticsOperation, "order_id", MahjongProductAnalytics.Id(submission.OrderNo), "withdraw_type", submission.WithdrawType, "amount", submission.Amount, "currency", submission.Currency, "receiver_present", !string.IsNullOrEmpty(submission.Receiver));
				WithdrawOrderData order = SubmitLocalWithdrawal(submission);
				if (order == null)
				{
					MahjongProductAnalytics.Track("f2_withdraw_submit_result", "operation_id", analyticsOperation, "result", "failed");
					onError?.Invoke("Withdrawal could not be stored.");
					yield break;
				}

				TrackOrder("submitted", order, analyticsOperation);
				MahjongProductAnalytics.Track("f2_withdraw_submit_result", "operation_id", analyticsOperation, "result", "stored_locally", "order_id", MahjongProductAnalytics.Id(order.OrderNo), "status", order.OrderStatus);
				onSuccess?.Invoke(order);
			}
			catch (InvalidOperationException exception)
			{
				MahjongProductAnalytics.Track("f2_withdraw_submit_result", "operation_id", analyticsOperation, "result", "failed");
				onError?.Invoke(exception.Message);
			}
			catch (Exception exception)
			{
				Debug.LogError(
					WalletLogPrefix + " withdrawal could not be stored: " +
					exception.Message);
				MahjongProductAnalytics.Track("f2_withdraw_submit_result", "operation_id", analyticsOperation, "result", "failed");
				onError?.Invoke(exception.Message);
			}
		}

		public static IEnumerator QueryWithdrawOrders(
			Action<List<WithdrawOrderData>> onSuccess,
			Action<string> onError = null)
		{
			string analyticsOperation = MahjongProductAnalytics.Begin("f2_withdraw_history_query", "source", "local");
			yield return null;
			try
			{
				List<WithdrawOrderData> analyticsOrders = SortOrders(GetCachedOrders());
				for (int i = 0; i < analyticsOrders.Count; i++) TrackOrder("history_query", analyticsOrders[i], analyticsOperation);
				MahjongProductAnalytics.Track("f2_withdraw_history_query_result", "operation_id", analyticsOperation, "result", "resolved", "count", analyticsOrders.Count);
				onSuccess?.Invoke(analyticsOrders);
			}
			catch (Exception exception)
			{
				MahjongProductAnalytics.Track("f2_withdraw_history_query_result", "operation_id", analyticsOperation, "result", "failed");
				onError?.Invoke(exception.Message);
			}
		}

		public static IEnumerator QueryWithdrawOrder(
			string orderNo,
			Action<WithdrawOrderData> onSuccess,
			Action<string> onError = null)
		{
			string analyticsOperation = MahjongProductAnalytics.Begin("f2_withdraw_order_query", "source", "local");
			yield return null;
			if (string.IsNullOrEmpty(orderNo))
			{
				MahjongProductAnalytics.Track("f2_withdraw_order_query_result", "operation_id", analyticsOperation, "result", "failed");
				onError?.Invoke("Withdrawal order number is empty.");
				yield break;
			}

			WithdrawOrderData order = GetCachedOrders().FirstOrDefault(item =>
				item != null &&
				string.Equals(
					item.OrderNo,
					orderNo,
					StringComparison.OrdinalIgnoreCase));
			if (order == null)
			{
				MahjongProductAnalytics.Track("f2_withdraw_order_query_result", "operation_id", analyticsOperation, "result", "failed");
				onError?.Invoke("Withdrawal order was not found.");
				yield break;
			}

			TrackOrder("query", order, analyticsOperation);
			MahjongProductAnalytics.Track("f2_withdraw_order_query_result", "operation_id", analyticsOperation, "result", "resolved", "order_id", MahjongProductAnalytics.Id(order.OrderNo), "status", order.OrderStatus);
			onSuccess?.Invoke(order);
		}

		public static DataModel CreateWalletWithdrawDataModel()
		{
			GameManagerWZ gameManager = GameManagerWZ.instance;
			return new DataModel
			{
				Level = Math.Max(
					0,
					gameManager != null ? gameManager.currentLv - 1 : 0),
				Money = GetCachedWalletBalance(),
				Num = Math.Max(
					1,
					gameManager != null ? gameManager.getPlayerStage() : 1),
				Index = 3,
				CloseType = 1,
				Special = DataModel.SpecialLuckyWalletWithdraw,
				Name = gameManager != null
					? gameManager.getPlayerName()
					: string.Empty,
				Phone = gameManager != null
					? gameManager.getPlayerPhone()
					: string.Empty,
				Email = gameManager != null
					? gameManager.getPlayerEmail()
					: string.Empty,
				PayPalAccountExists = GetCachedPayPalAccountExists(),
				PayPalAccountTip = GetCachedPayPalTip()
			};
		}

		public static WithdrawOrderData CreateQueuedWalletOrder(
			double amount,
			string receiver = null)
		{
			GameManagerWZ gameManager = GameManagerWZ.instance;
			if (string.IsNullOrEmpty(receiver) && gameManager != null)
			{
				receiver = gameManager.getPlayerEmail();
			}

			string now = GetLocalTimestamp();
			WithdrawOrderData order = new WithdrawOrderData
			{
				OrderNo = CreateLocalOrderNumber("wallet"),
				WithdrawType = WithdrawConstants.WithdrawTypeWallet,
				OrderStatus = 0,
				Receiver = receiver,
				Amount = FormatAmount(Math.Max(0.0, amount)),
				Currency = WithdrawConstants.CurrencyUsd,
				CreateTime = now,
				UpdateTime = now,
				ClientSortAnchorTime = now,
				ClientQueuedPlaceholder = true,
				LocalStatusText = WalletReviewStatusText
			};

			List<WithdrawOrderData> orders = GetCachedOrders();
			orders.Add(order);
			CacheOrders(orders);
			TrackOrder("wallet_queued", order);
			return order;
		}

		public static WithdrawOrderData EnsureQueuedFirstAdOrder(double amount)
		{
			double safeAmount = RoundAmount(
				amount > 0.0 ? amount : DefaultLuckyRewardAmount);
			GameManagerWZ gameManager = GameManagerWZ.instance;
			string receiver = gameManager != null
				? gameManager.getPlayerEmail()
				: string.Empty;
			string now = GetLocalTimestamp();
			List<WithdrawOrderData> orders = GetCachedOrders();
			WithdrawOrderData queued = null;
			WithdrawOrderData existing = null;

			for (int i = 0; i < orders.Count; i++)
			{
				WithdrawOrderData order = orders[i];
				if (order == null || !order.IsFirstAdWithdraw)
				{
					continue;
				}

				if (order.ClientQueuedPlaceholder)
				{
					queued = order;
					break;
				}

				if (existing == null)
				{
					existing = order;
				}
			}

			if (queued == null && existing != null)
			{
				return existing;
			}

			if (queued == null)
			{
				queued = new WithdrawOrderData
				{
					OrderNo = CreateLocalOrderNumber("first-ad"),
					WithdrawType = WithdrawConstants.WithdrawTypeFirstAd,
					OrderStatus = 0,
					Receiver = receiver,
					Amount = FormatAmount(safeAmount),
					Currency = WithdrawConstants.CurrencyUsd,
					CreateTime = now,
					UpdateTime = now,
					ClientSortAnchorTime = now,
					ClientQueuedPlaceholder = true,
					LocalStatusText = FirstAdQueuedStatusText
				};
				orders.Add(queued);
			}
			else
			{
				string previousAnchor = queued.GetSortAnchorTimeText();
				queued.Amount = FormatAmount(safeAmount);
				queued.Receiver = receiver;
				queued.Currency = WithdrawConstants.CurrencyUsd;
				queued.LocalStatusText = FirstAdQueuedStatusText;
				queued.UpdateTime = now;
				if (string.IsNullOrEmpty(queued.ClientSortAnchorTime))
				{
					queued.ClientSortAnchorTime =
						string.IsNullOrEmpty(previousAnchor)
							? now
							: previousAnchor;
				}
			}

			CacheOrders(orders);
			TrackOrder("first_ad_queued", queued);
			return queued;
		}

		public static void MarkQueuedFirstAdOrderUnderReview()
		{
			List<WithdrawOrderData> orders = GetCachedOrders();
			for (int i = 0; i < orders.Count; i++)
			{
				WithdrawOrderData order = orders[i];
				if (order == null || !order.IsFirstAdWithdraw ||
					!order.ClientQueuedPlaceholder)
				{
					continue;
				}

				string anchor = order.GetSortAnchorTimeText();
				order.LocalStatusText = WalletReviewStatusText;
				order.UpdateTime = GetLocalTimestamp();
				if (string.IsNullOrEmpty(order.ClientSortAnchorTime))
				{
					order.ClientSortAnchorTime = anchor;
				}
				CacheOrders(orders);
				return;
			}
		}

		public static void ReplaceQueuedFirstAdOrder(
			WithdrawOrderData order)
		{
			if (order == null || string.IsNullOrEmpty(order.OrderNo))
			{
				return;
			}

			List<WithdrawOrderData> orders = GetCachedOrders();
			ApplyCachedSortAnchor(orders, order);
			int replacementIndex = -1;
			for (int i = orders.Count - 1; i >= 0; i--)
			{
				WithdrawOrderData cached = orders[i];
				bool sameOrder = cached != null && string.Equals(
					cached.OrderNo,
					order.OrderNo,
					StringComparison.OrdinalIgnoreCase);
				bool queuedFirstAd = cached != null &&
					cached.IsFirstAdWithdraw &&
					cached.ClientQueuedPlaceholder;
				if (sameOrder || queuedFirstAd)
				{
					replacementIndex = i;
					orders.RemoveAt(i);
				}
			}

			if (replacementIndex < 0 || replacementIndex > orders.Count)
			{
				orders.Add(order);
			}
			else
			{
				orders.Insert(replacementIndex, order);
			}
			CacheOrders(orders);
		}

		public static void ReplaceQueuedWalletOrder(
			string localOrderNo,
			WithdrawOrderData order)
		{
			if (order == null || string.IsNullOrEmpty(order.OrderNo))
			{
				return;
			}

			List<WithdrawOrderData> orders = GetCachedOrders();
			ApplyCachedSortAnchor(orders, order, localOrderNo);
			orders = orders.Where(item =>
				item == null ||
				!item.ClientQueuedPlaceholder ||
				!item.IsWalletWithdraw ||
				!string.Equals(
					item.OrderNo,
					localOrderNo,
					StringComparison.OrdinalIgnoreCase))
				.ToList();

			int orderIndex = orders.FindIndex(item =>
				item != null && string.Equals(
					item.OrderNo,
					order.OrderNo,
					StringComparison.OrdinalIgnoreCase));
			if (orderIndex >= 0)
			{
				orders[orderIndex] = order;
			}
			else
			{
				orders.Add(order);
			}
			CacheOrders(orders);
		}

		public static void RemoveQueuedWalletOrder(string localOrderNo)
		{
			MahjongProductAnalytics.Track("f2_withdraw_queue_removed", "order_id", MahjongProductAnalytics.Id(localOrderNo));
			if (string.IsNullOrEmpty(localOrderNo))
			{
				return;
			}

			List<WithdrawOrderData> orders = GetCachedOrders();
			orders.RemoveAll(order =>
				order != null &&
				order.ClientQueuedPlaceholder &&
				order.IsWalletWithdraw &&
				string.Equals(
					order.OrderNo,
					localOrderNo,
					StringComparison.OrdinalIgnoreCase));
			CacheOrders(orders);
		}

		public static DataModel CreateRetryWithdrawDataModel(
			WithdrawOrderData order)
		{
			GameManagerWZ gameManager = GameManagerWZ.instance;
			bool requiresReceiver = order != null && order.RequiresReceiverInput;
			TrackOrder("retry_requested", order);
			return new DataModel
			{
				Level = Math.Max(
					0,
					gameManager != null ? gameManager.currentLv - 1 : 0),
				Money = order != null ? order.GetAmountValue() : 0.0,
				Num = Math.Max(
					1,
					gameManager != null ? gameManager.getPlayerStage() : 1),
				Index = 3,
				CloseType = 1,
				Special = order != null && order.IsWalletWithdraw
					? DataModel.SpecialLuckyWalletWithdraw
					: DataModel.SpecialNone,
				Name = gameManager != null
					? gameManager.getPlayerName()
					: string.Empty,
				Phone = gameManager != null
					? gameManager.getPlayerPhone()
					: string.Empty,
				Email = order != null
					? order.Receiver
					: gameManager != null
						? gameManager.getPlayerEmail()
						: string.Empty,
				OrderNo = order?.OrderNo ?? string.Empty,
				Receiver = order?.Receiver ?? string.Empty,
				WithdrawType = order?.WithdrawType ?? string.Empty,
				OrderStatus = order?.OrderStatus ?? 0,
				RetryWithdraw = true,
				PayPalAccountExists = requiresReceiver
					? 0
					: GetCachedPayPalAccountExists(),
				PayPalAccountTip = requiresReceiver
					? "Please enter a valid PayPal email address."
					: GetCachedPayPalTip()
			};
		}

		private static WithdrawOrderData SubmitLocalWithdrawal(
			ClientWithdrawRequest submission)
		{
			List<WithdrawOrderData> orders = GetCachedOrders();
			WithdrawOrderData order = FindOrder(orders, submission.OrderNo);
			string withdrawType = !string.IsNullOrEmpty(submission.WithdrawType)
				? submission.WithdrawType
				: order?.WithdrawType;
			if (string.IsNullOrEmpty(withdrawType))
			{
				withdrawType = WithdrawConstants.WithdrawTypeWallet;
			}

			double amount = submission.Amount ??
				(order != null ? order.GetAmountValue() : 0.0);
			amount = RoundAmount(Math.Max(0.0, amount));
			if (amount <= 0.0)
			{
				throw new InvalidOperationException(
					"Withdrawal amount must be greater than zero.");
			}

			string receiver = !string.IsNullOrEmpty(submission.Receiver)
				? submission.Receiver
				: order?.Receiver;
			if (string.IsNullOrEmpty(receiver))
			{
				throw new InvalidOperationException(
					"PayPal receiver email is required.");
			}

			if (order == null)
			{
				order = FindMatchingQueuedOrder(
					orders,
					withdrawType,
					amount,
					receiver);
			}

			bool isWalletWithdrawal = string.Equals(
				withdrawType,
				WithdrawConstants.WithdrawTypeWallet,
				StringComparison.OrdinalIgnoreCase);
			UserWalletData wallet = null;
			bool hasFrozenReservation = false;
			if (isWalletWithdrawal)
			{
				wallet = GetOrCreateLocalWallet(submission.Currency);
				hasFrozenReservation = order != null &&
					!order.ClientQueuedPlaceholder &&
					order.WalletFrozen == 1;
				if (!hasFrozenReservation && wallet.Balance + 0.0001 < amount)
				{
					throw new InvalidOperationException(
						"Wallet balance is insufficient.");
				}
			}

			if (order == null)
			{
				order = string.Equals(
					withdrawType,
					WithdrawConstants.WithdrawTypeFirstAd,
					StringComparison.OrdinalIgnoreCase)
					? EnsureQueuedFirstAdOrder(amount)
					: CreateQueuedWalletOrder(amount, receiver);
				orders = GetCachedOrders();
			}

			if (isWalletWithdrawal && !hasFrozenReservation)
			{
				wallet.Balance = RoundAmount(wallet.Balance - amount);
				wallet.FrozenBalance = RoundAmount(
					Math.Max(0.0, wallet.FrozenBalance) + amount);
				wallet.TotalWithdrawAmount = RoundAmount(
					Math.Max(0.0, wallet.TotalWithdrawAmount) + amount);
				wallet.PayPalAccountExists = 1;
				wallet.PayPalAccountTip = null;
				wallet.UpdateTime = GetUtcTimestamp();
				CacheWallet(wallet);
			}

			string now = GetLocalTimestamp();
			if (string.IsNullOrEmpty(order.OrderNo))
			{
				order.OrderNo = !string.IsNullOrEmpty(submission.OrderNo)
					? submission.OrderNo
					: CreateLocalOrderNumber(
						isWalletWithdrawal ? "wallet" : "first-ad");
			}
			if (string.IsNullOrEmpty(order.CreateTime))
			{
				order.CreateTime = now;
			}
			if (string.IsNullOrEmpty(order.ClientSortAnchorTime))
			{
				order.ClientSortAnchorTime = order.CreateTime;
			}

			MahjongProductAnalytics.Track("f2_withdraw_reservation", "order_id", MahjongProductAnalytics.Id(order.OrderNo), "amount", amount, "wallet_withdraw", isWalletWithdrawal, "already_reserved", hasFrozenReservation, "wallet_available", wallet != null ? wallet.Balance : 0.0, "wallet_frozen", wallet != null ? wallet.FrozenBalance : 0.0, "currency", submission.Currency);
			order.WithdrawType = withdrawType;
			order.OrderStatus = 3;
			order.WalletFrozen = isWalletWithdrawal ? 1 : order.WalletFrozen;
			order.Receiver = receiver;
			order.Amount = FormatAmount(amount);
			order.Currency = NormalizeCurrency(submission.Currency);
			order.ErrorName = null;
			order.ErrorMessage = null;
			order.UpdateTime = now;
			order.ClientQueuedPlaceholder = false;
			order.LocalStatusText = WalletReviewStatusText;
			CacheOrder(order);
			return order;
		}

		private static WithdrawOrderData FindOrder(
			List<WithdrawOrderData> orders,
			string orderNo)
		{
			if (string.IsNullOrEmpty(orderNo))
			{
				return null;
			}

			return orders.FirstOrDefault(item =>
				item != null && string.Equals(
					item.OrderNo,
					orderNo,
					StringComparison.OrdinalIgnoreCase));
		}

		private static WithdrawOrderData FindMatchingQueuedOrder(
			List<WithdrawOrderData> orders,
			string withdrawType,
			double amount,
			string receiver)
		{
			return orders.FirstOrDefault(candidate =>
				candidate != null &&
				candidate.ClientQueuedPlaceholder &&
				string.Equals(
					candidate.WithdrawType,
					withdrawType,
					StringComparison.OrdinalIgnoreCase) &&
				Math.Abs(candidate.GetAmountValue() - amount) < 0.0001 &&
				(string.IsNullOrEmpty(candidate.Receiver) ||
				 string.Equals(
					candidate.Receiver,
					receiver,
					StringComparison.OrdinalIgnoreCase)));
		}

		private static void ApplyWalletIncome(
			UserWalletData wallet,
			double amount,
			string currency)
		{
			double safeAmount = RoundAmount(Math.Max(0.0, amount));
			wallet.Balance = RoundAmount(
				Math.Max(0.0, wallet.Balance) + safeAmount);
			wallet.TotalIncomeAmount = RoundAmount(
				Math.Max(0.0, wallet.TotalIncomeAmount) + safeAmount);
			wallet.Currency = NormalizeCurrency(currency);
			wallet.Status = 1;
			wallet.UpdateTime = GetUtcTimestamp();
		}

		private static bool CanAddWalletIncomeAtCurrentLevel()
		{
			GameManagerWZ gameManager = GameManagerWZ.instance;
			int currentLevel = gameManager != null
				? gameManager.currentLv
				: PlayerPrefs.GetInt(
					FacadePlayerPrefExtend.currentLevel,
					1);
			return Math.Max(0, currentLevel) >=
				GameDefines.LuckyWalletUnlockLevel;
		}

		private static LuckyRewardConfigData
			GetOrCreateLocalLuckyRewardConfig(string currency)
		{
			LuckyRewardConfigData config = GetCachedLuckyRewardConfig();
			if (config != null)
			{
				return config;
			}

			config = new LuckyRewardConfigData
			{
				LuckyAmount = DefaultLuckyRewardAmount,
				LuckySuccessRewardAmount = null,
				LuckySpinRewardAmount = null,
				Currency = NormalizeCurrency(currency)
			};
			CacheLuckyRewardConfig(config);
			return config;
		}

		private static UserWalletData GetOrCreateLocalWallet(string currency)
		{
			UserWalletData wallet = GetCachedWallet();
			if (wallet != null)
			{
				return wallet;
			}

			string now = GetUtcTimestamp();
			wallet = new UserWalletData
			{
				Balance = 0.0,
				FrozenBalance = 0.0,
				TotalIncomeAmount = 0.0,
				TotalWithdrawAmount = 0.0,
				Currency = NormalizeCurrency(currency),
				Status = 1,
				CreateTime = now,
				UpdateTime = now
			};
			CacheWallet(wallet);
			return wallet;
		}

		private static void TrackWallet(string reason, UserWalletData wallet)
		{
			try
			{
				if (wallet == null) return;
				MahjongProductAnalytics.State("f2_wallet_snapshot", "f2_wallet", "source", "local", "reason", reason, "available", wallet.Balance, "frozen", wallet.FrozenBalance, "total_income", wallet.TotalIncomeAmount, "total_withdrawn", wallet.TotalWithdrawAmount, "currency", wallet.Currency, "status", wallet.Status, "account_exists", wallet.PayPalAccountExists, "updated_at", wallet.UpdateTime);
			}
			catch { }
		}

		private static void TrackOrder(string reason, WithdrawOrderData order, string operationId = null)
		{
			try
			{
				if (order == null) return;
				MahjongProductAnalytics.Track("f2_withdraw_order", "operation_id", operationId, "source", "local", "reason", reason, "order_id", MahjongProductAnalytics.Id(order.OrderNo), "withdraw_type", order.WithdrawType, "status", order.OrderStatus, "queued_placeholder", order.ClientQueuedPlaceholder, "amount", order.GetAmountValue(), "currency", order.Currency, "wallet_frozen", order.WalletFrozen, "requires_receiver", order.RequiresReceiverInput, "created_at", order.CreateTime, "updated_at", order.UpdateTime, "query_at", order.LastQueryTime, "success_at", order.SuccessTime, "error_present", !string.IsNullOrEmpty(order.ErrorName));
			}
			catch { }
		}

		private static void CacheWallet(UserWalletData wallet)
		{
			if (wallet == null)
			{
				return;
			}

			SPlayerPrefs.SetString(
				FacadePlayerPrefExtend.WalletBalanceCache,
				JsonConvert.SerializeObject(wallet, JsonSettings));
			double availableBalance = Math.Max(wallet.Balance, 0.0);
			GameDefines.LuckyChestPlaceholderCoin = (float)availableBalance;
			SPlayerPrefs.Save();
			TrackWallet("balance_updated", wallet);
			WalletBalanceChanged?.Invoke(availableBalance);
		}

		private static void CacheLuckyRewardConfig(
			LuckyRewardConfigData config)
		{
			if (config == null)
			{
				return;
			}

			MahjongProductAnalytics.State("f2_wallet_reward_config", "f2_reward_config", "source", "local_config", "lucky_amount", config.LuckyAmount, "lucky_success", config.LuckySuccessRewardAmount, "lucky_spin", config.LuckySpinRewardAmount, "currency", config.Currency);
			SPlayerPrefs.SetString(
				LuckyRewardConfigCachePrefsKey,
				JsonConvert.SerializeObject(config, JsonSettings));
			SPlayerPrefs.Save();
		}

		private static void CacheOrders(List<WithdrawOrderData> orders)
		{
			List<WithdrawOrderData> sortedOrders = SortOrders(orders);
			SPlayerPrefs.SetString(
				FacadePlayerPrefExtend.WalletWithdrawOrdersCache,
				JsonConvert.SerializeObject(sortedOrders, JsonSettings));
			SPlayerPrefs.Save();
			for (int analyticsIndex = 0; analyticsIndex < sortedOrders.Count; analyticsIndex++)
			{
				WithdrawOrderData analyticsOrder = sortedOrders[analyticsIndex];
				MahjongProductAnalytics.State("f2_withdraw_order_state", "f2_order_" + MahjongProductAnalytics.Id(analyticsOrder.OrderNo), "order_id", MahjongProductAnalytics.Id(analyticsOrder.OrderNo), "amount", analyticsOrder.GetAmountValue(), "currency", analyticsOrder.Currency, "status", analyticsOrder.OrderStatus, "placeholder", analyticsOrder.ClientQueuedPlaceholder, "frozen", analyticsOrder.WalletFrozen, "withdraw_type", analyticsOrder.WithdrawType, "status_text", analyticsOrder.LocalStatusText);
			}
		}

		private static void CacheOrder(WithdrawOrderData order)
		{
			if (order == null || string.IsNullOrEmpty(order.OrderNo))
			{
				return;
			}

			List<WithdrawOrderData> orders = GetCachedOrders();
			ApplyCachedSortAnchor(orders, order);
			int orderIndex = orders.FindIndex(item =>
				item != null && string.Equals(
					item.OrderNo,
					order.OrderNo,
					StringComparison.OrdinalIgnoreCase));
			if (orderIndex >= 0)
			{
				orders[orderIndex] = order;
			}
			else
			{
				orders.Add(order);
			}
			CacheOrders(orders);
		}

		private static List<WithdrawOrderData> SortOrders(
			List<WithdrawOrderData> orders)
		{
			return (orders ?? new List<WithdrawOrderData>())
				.Where(item => item != null)
				.OrderByDescending(GetOrderCreateTime)
				.ThenByDescending(item => item.GetBestTimeText())
				.ThenByDescending(item => item.OrderNo)
				.ToList();
		}

		private static void ApplyCachedSortAnchor(
			List<WithdrawOrderData> cachedOrders,
			WithdrawOrderData order,
			string localPlaceholderOrderNo = null)
		{
			if (cachedOrders == null || order == null ||
				!string.IsNullOrEmpty(order.ClientSortAnchorTime))
			{
				return;
			}

			WithdrawOrderData source = cachedOrders.FirstOrDefault(cached =>
				cached != null &&
				((!string.IsNullOrEmpty(localPlaceholderOrderNo) &&
				  string.Equals(
					cached.OrderNo,
					localPlaceholderOrderNo,
					StringComparison.OrdinalIgnoreCase)) ||
				 string.Equals(
					cached.OrderNo,
					order.OrderNo,
					StringComparison.OrdinalIgnoreCase)));
			if (source == null)
			{
				source = cachedOrders.FirstOrDefault(cached =>
					IsMatchingOrderForPlaceholder(cached, order));
			}

			if (source != null)
			{
				string anchor = source.GetSortAnchorTimeText();
				if (!string.IsNullOrEmpty(anchor))
				{
					order.ClientSortAnchorTime = anchor;
				}
			}
		}

		private static bool IsMatchingOrderForPlaceholder(
			WithdrawOrderData placeholder,
			WithdrawOrderData order)
		{
			if (placeholder == null || order == null ||
				!placeholder.ClientQueuedPlaceholder ||
				order.ClientQueuedPlaceholder ||
				!string.Equals(
					placeholder.WithdrawType,
					order.WithdrawType,
					StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			if (!string.IsNullOrEmpty(placeholder.Currency) &&
				!string.IsNullOrEmpty(order.Currency) &&
				!string.Equals(
					placeholder.Currency,
					order.Currency,
					StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			if (Math.Abs(
					placeholder.GetAmountValue() - order.GetAmountValue()) >=
				0.0001)
			{
				return false;
			}

			return string.IsNullOrEmpty(placeholder.Receiver) ||
				string.IsNullOrEmpty(order.Receiver) ||
				string.Equals(
					placeholder.Receiver,
					order.Receiver,
					StringComparison.OrdinalIgnoreCase);
		}

		private static DateTimeOffset GetOrderCreateTime(
			WithdrawOrderData order)
		{
			DateTimeOffset time;
			return order != null && order.TryGetSortAnchorTime(out time)
				? time
				: default(DateTimeOffset);
		}

		private static float ResolveLuckyRewardAmount(
			double? rewardAmount,
			float fallbackAmount)
		{
			return rewardAmount.HasValue && rewardAmount.Value > 0.0
				? (float)RoundAmount(rewardAmount.Value)
				: fallbackAmount;
		}

		private static double RoundAmount(double amount)
		{
			return Math.Round(amount, 2, MidpointRounding.AwayFromZero);
		}

		private static string FormatAmount(double amount)
		{
			return RoundAmount(amount).ToString(
				"0.00",
				CultureInfo.InvariantCulture);
		}

		private static string NormalizeCurrency(string currency)
		{
			return string.IsNullOrEmpty(currency)
				? WithdrawConstants.CurrencyUsd
				: currency;
		}

		private static string CreateLocalOrderNumber(string category)
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"local-{0}-{1:yyyyMMddHHmmssfff}-{2}",
				category,
				Framework2Host.LocalNow.ToUniversalTime(),
				Guid.NewGuid().ToString("N").Substring(0, 8));
		}

		private static string GetLocalTimestamp()
		{
			return new DateTimeOffset(Framework2Host.LocalNow).ToString("O", CultureInfo.InvariantCulture);
		}

		private static string GetUtcTimestamp()
		{
			return new DateTimeOffset(Framework2Host.LocalNow.ToUniversalTime()).ToString(
				"O",
				CultureInfo.InvariantCulture);
		}
	}
}
