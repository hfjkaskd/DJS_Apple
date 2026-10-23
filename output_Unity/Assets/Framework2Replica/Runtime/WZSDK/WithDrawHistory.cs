using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArrowOut.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	public class WithDrawHistory : BaseViewWithDraw
	{
		private const bool ForcePayPalErrorOnOrderClick = false;

		private const string DefaultHistoryItemPrefabPath =
			"Prefab/historyItem";

		private const string WalletHistoryItemPrefabPath =
			"Prefab/historyItemBlue";

		private const string DefaultPayPalAccountErrorTip =
			"当前PayPal收款账号不存在，请重新填写正确的PayPal邮箱";

		private Text moneyText;

		private GameObject content;

		private readonly List<HistoryItem> historyItems =
			new List<HistoryItem>();

		private bool isQueryingOrderStatus;

		private bool isRefreshingOrders;

		private int populateVersion;

		internal bool RuntimeGmIsQueryingOrderStatus => isQueryingOrderStatus;

		internal bool RuntimeGmIsRefreshingOrders => isRefreshingOrders;

		internal Text RuntimeGmMoneyText => moneyText;

		protected override void OnAwake()
		{
			moneyText = Find<Text>("Panel/PanelA/Num");
			content = Find("Panel/Scroll View/Viewport/Content");
			Button closeButton = Find<Button>("Panel/bg/close");
			if (closeButton != null)
			{
				closeButton.onClick.AddListener(OnCloseBtnClick);
			}
		}

		protected override void HandleViewArgs(object[] args)
		{
			PopulateOrders(WithdrawApiService.GetCachedOrders());
			StartCoroutine(RefreshOrdersCoroutine());
		}

		private IEnumerator RefreshOrdersCoroutine()
		{
			isRefreshingOrders = true;
			ApplyHistoryItemInteractionState();

			bool requestCompleted = false;
			List<WithdrawOrderData> latestOrders = null;
			yield return WithdrawApiService.QueryWithdrawOrders(
				orders =>
				{
					requestCompleted = true;
					latestOrders = orders ?? new List<WithdrawOrderData>();
					LogFetchedOrderStatuses(latestOrders);
					PopulateOrders(latestOrders);
				},
				error =>
				{
					requestCompleted = true;
					Debug.LogError(
						"[WithDrawHistory] query orders failed: " + error);
				});

			if (!requestCompleted)
			{
				Debug.LogWarning(
					"[WithDrawHistory] query orders completed without callback.");
			}
			if (latestOrders != null && latestOrders.Count > 0)
			{
				yield return RefreshEachOrderStatusCoroutine(latestOrders);
			}

			isRefreshingOrders = false;
			ApplyHistoryItemInteractionState();
		}

		private IEnumerator RefreshEachOrderStatusCoroutine(
			List<WithdrawOrderData> orders)
		{
			for (int i = 0; orders != null && i < orders.Count; i++)
			{
				WithdrawOrderData order = orders[i];
				if (order == null ||
					order.ClientQueuedPlaceholder ||
					string.IsNullOrEmpty(order.OrderNo) ||
					order.OrderStatus == 1)
				{
					continue;
				}

				bool requestCompleted = false;
				string currentOrderNo = order.OrderNo;
				yield return WithdrawApiService.QueryWithdrawOrder(
					currentOrderNo,
					refreshedOrder =>
					{
						requestCompleted = true;
						WithdrawOrderData resolvedOrder =
							refreshedOrder ?? order;
						LogRefreshedOrderRealStatus(resolvedOrder);
						PopulateOrders(WithdrawApiService.GetCachedOrders());
						LogOfficialStatusIfDisplayedAsReview(resolvedOrder);
					},
					error =>
					{
						requestCompleted = true;
						Debug.LogError(
							"[WithDrawHistory] query order " +
							currentOrderNo + " failed: " + error);
					});

				if (!requestCompleted)
				{
					Debug.LogWarning(
						"[WithDrawHistory] query order completed without callback: " +
						currentOrderNo);
				}
			}
		}

		private async void PopulateOrders(List<WithdrawOrderData> orders)
		{
			if (content == null)
			{
				return;
			}
			int currentPopulateVersion = ++populateVersion;

			historyItems.Clear();
			Transform contentTransform = content.transform;
			for (int i = contentTransform.childCount - 1; i >= 0; i--)
			{
				Destroy(contentTransform.GetChild(i).gameObject);
			}

			List<WithdrawOrderData> sortedOrders =
				SortOrdersForDisplay(orders);
			double totalAmount = sortedOrders
				.Where(order => order != null)
				.Sum(order => order.GetAmountValue());
			if (moneyText != null)
			{
				GameManagerWZ manager = GameManagerWZ.instance;
				moneyText.text = manager != null
					? manager.FormatRegionalValue(totalAmount)
					: "$" + totalAmount.ToString("F2");
			}

			List<Task<GameObject>> prefabTasks =
				new List<Task<GameObject>>(sortedOrders.Count);
			for (int i = 0; i < sortedOrders.Count; i++)
			{
				prefabTasks.Add(LoadHistoryItemPrefabAsync(sortedOrders[i]));
			}

			GameObject[] prefabs = await Task.WhenAll(prefabTasks);
			if (this == null ||
				content == null ||
				currentPopulateVersion != populateVersion)
			{
				return;
			}

			for (int i = 0; i < sortedOrders.Count; i++)
			{
				WithdrawOrderData order = sortedOrders[i];
				GameObject prefab = prefabs[i];
				if (prefab == null)
				{
					continue;
				}
				GameObject instance = Instantiate(prefab, contentTransform);
				HistoryItem historyItem =
					instance.GetComponent<HistoryItem>();
				if (historyItem == null)
				{
					continue;
				}
				historyItem.init(order, OnOrderClick);
				historyItem.SetInteractionBlocked(
					isQueryingOrderStatus || isRefreshingOrders);
				historyItems.Add(historyItem);
			}
		}

		private async Task<GameObject> LoadHistoryItemPrefabAsync(
			WithdrawOrderData order)
		{
			string path = ResolveHistoryItemPrefabPath(order);
			GameObject prefab =
				await AddressableAssetService.LoadAsync<GameObject>(path);
			if (prefab != null)
			{
				return prefab;
			}
			if (!string.Equals(
				path,
				DefaultHistoryItemPrefabPath,
				StringComparison.Ordinal))
			{
				Debug.LogWarning(
					"[WithDrawHistory] history item Addressable is missing: " +
					path + "; falling back to " +
					DefaultHistoryItemPrefabPath + ".");
				return await AddressableAssetService.LoadAsync<GameObject>(
					DefaultHistoryItemPrefabPath);
			}
			Debug.LogWarning(
				"[WithDrawHistory] history item Addressable is missing: " +
				path);
			return null;
		}

		private string ResolveHistoryItemPrefabPath(
			WithdrawOrderData order)
		{
			return order != null && order.IsWalletWithdraw
				? WalletHistoryItemPrefabPath
				: DefaultHistoryItemPrefabPath;
		}

		private void OnOrderClick(WithdrawOrderData order)
		{
			if (order == null ||
				isQueryingOrderStatus ||
				isRefreshingOrders ||
				(!GameDefines.EnableWithdrawHistoryAllStatusClick &&
				 !order.RequiresReceiverInput))
			{
				return;
			}

			isQueryingOrderStatus = true;
			ApplyHistoryItemInteractionState();
			Debug.Log(GetOrderStatusLog(order));
			HandleOrderAction(order);
			FinishOrderQueryInteraction();
		}

		private string GetOrderStatusLog(WithdrawOrderData order)
		{
			if (order == null)
			{
				return "[WithDrawHistory] order is null.";
			}
			if (ShouldDisplayReviewStatus(order))
			{
				return string.Format(
					"[WithDrawHistory] order={0}, type={1}, status={2}, " +
					"official={3}, display={4}",
					order.OrderNo,
					order.WithdrawType,
					order.OrderStatus,
					order.GetStatusText(),
					GetDisplayStatusText(order));
			}
			return string.Format(
				"[WithDrawHistory] order={0}, type={1}, status={2}, text={3}",
				order.OrderNo,
				order.WithdrawType,
				order.OrderStatus,
				order.GetStatusText());
		}

		private void OpenForcedPayPalErrorView(WithdrawOrderData order)
		{
			DataModel dataModel =
				WithdrawApiService.CreateRetryWithdrawDataModel(order);
			if (dataModel == null)
			{
				return;
			}
			dataModel.RetryWithdraw = true;
			dataModel.PayPalAccountExists = 0;
			if (string.IsNullOrEmpty(dataModel.PayPalAccountTip))
			{
				dataModel.PayPalAccountTip =
					DefaultPayPalAccountErrorTip;
			}
			if (UIManager.instance != null)
			{
				UIManager.instance.ShowView(
					EUIType.PayPalErrorView, dataModel);
			}
		}

		private void HandleOrderAction(WithdrawOrderData order)
		{
			if (order == null)
			{
				return;
			}
			LogOfficialStatusIfDisplayedAsReview(order);
			if (ForcePayPalErrorOnOrderClick)
			{
				OpenForcedPayPalErrorView(order);
				return;
			}

			bool canRetryStatus =
				!order.ClientQueuedPlaceholder &&
				(order.OrderStatus == 2 || order.OrderStatus == 4);
			if (canRetryStatus && order.RequiresReceiverInput)
			{
				DataModel dataModel =
					WithdrawApiService.CreateRetryWithdrawDataModel(order);
				if (dataModel != null)
				{
					dataModel.PayPalAccountExists = 0;
					if (string.IsNullOrEmpty(dataModel.PayPalAccountTip))
					{
						dataModel.PayPalAccountTip =
							DefaultPayPalAccountErrorTip;
					}
				}
				if (UIManager.instance != null)
				{
					UIManager.instance.ShowView(
						EUIType.PayPalErrorView, dataModel);
				}
				return;
			}

			WithdrawApiService.LogPaymentNotice(
				"WithDrawHistory",
				order.GetStatusText(),
				canRetryStatus ? LogType.Warning : LogType.Log);
		}

		public void OnCloseBtnClick()
		{
			if (GameApp.viewManager != null)
			{
				GameApp.viewManager.Close(ViewId);
			}
		}

		private void FinishOrderQueryInteraction()
		{
			isQueryingOrderStatus = false;
			ApplyHistoryItemInteractionState();
		}

		private void ApplyHistoryItemInteractionState()
		{
			bool blocked = isQueryingOrderStatus || isRefreshingOrders;
			for (int i = 0; i < historyItems.Count; i++)
			{
				if (historyItems[i] != null)
				{
					historyItems[i].SetInteractionBlocked(blocked);
				}
			}
		}

		private void LogFetchedOrderStatuses(
			List<WithdrawOrderData> orders)
		{
			List<WithdrawOrderData> sortedOrders =
				SortOrdersForDisplay(orders);
			if (sortedOrders.Count == 0)
			{
				WithdrawApiService.LogPaymentNotice(
					"WithDrawHistory",
					"No withdraw orders were returned.",
					LogType.Log);
				return;
			}
			for (int i = 0; i < sortedOrders.Count; i++)
			{
				WithdrawApiService.LogPaymentNotice(
					"WithDrawHistory",
					GetFetchedOrderStatusLog(
						sortedOrders[i], i + 1, sortedOrders.Count),
					LogType.Log);
			}
		}

		private void LogRefreshedOrderRealStatus(
			WithdrawOrderData order)
		{
			if (order == null)
			{
				WithdrawApiService.LogPaymentNotice(
					"WithDrawHistory",
					"Refreshed order is null.",
					LogType.Warning);
				return;
			}
			WithdrawApiService.LogPaymentNotice(
				"WithDrawHistory",
				string.Format(
					"Refreshed order={0}, type={1}, status={2}, " +
					"official={3}, display={4}",
					order.OrderNo,
					order.WithdrawType,
					order.OrderStatus,
					order.GetStatusText(),
					GetDisplayStatusText(order)),
				LogType.Log);
		}

		private string GetFetchedOrderStatusLog(
			WithdrawOrderData order,
			int index,
			int totalCount)
		{
			if (order == null)
			{
				return string.Format(
					"Order {0}/{1} is null.", index, totalCount);
			}
			string source = order.ClientQueuedPlaceholder
				? "local"
				: "official";
			return string.Format(
				"Order {0}/{1}: source={2}, no={3}, type={4}, " +
				"status={5}, official={6}, display={7}",
				index,
				totalCount,
				source,
				order.OrderNo,
				order.WithdrawType,
				order.OrderStatus,
				order.GetStatusText(),
				GetDisplayStatusText(order));
		}

		private void LogOfficialStatusIfDisplayedAsReview(
			WithdrawOrderData order)
		{
			if (order == null || !ShouldDisplayReviewStatus(order))
			{
				return;
			}
			WithdrawApiService.LogPaymentNotice(
				"WithDrawHistory",
				string.Format(
					"Official status {0} ({1}) is displayed as {2}.",
					order.GetStatusText(),
					order.OrderStatus,
					GetDisplayStatusText(order)),
				LogType.Log);
		}

		private bool ShouldDisplayReviewStatus(
			WithdrawOrderData order)
		{
			return order != null &&
				(order.OrderStatus == 4 || order.OrderStatus == 5);
		}

		private string GetDisplayStatusText(WithdrawOrderData order)
		{
			if (order == null)
			{
				return string.Empty;
			}
			return ShouldDisplayReviewStatus(order)
				? GetLocalizedText("3030", "审核中")
				: order.GetStatusText();
		}

		private string GetLocalizedText(string key, string fallback)
		{
			if (LocalizationManager.Instance != null)
			{
				string localized =
					LocalizationManager.Instance.GetText(key);
				if (!string.IsNullOrEmpty(localized) &&
					!string.Equals(
						localized, key, StringComparison.Ordinal))
				{
					return localized;
				}
			}
			return fallback;
		}

		private List<WithdrawOrderData> SortOrdersForDisplay(
			List<WithdrawOrderData> orders)
		{
			return (orders ?? new List<WithdrawOrderData>())
				.Where(order => order != null)
				.OrderBy(ResolveOrderDisplayTime)
				.ThenBy(order => order.GetBestTimeText())
				.ThenBy(order => order.OrderNo)
				.ToList();
		}

		private DateTimeOffset ResolveOrderDisplayTime(
			WithdrawOrderData order)
		{
			DateTimeOffset time;
			return order != null &&
				order.TryGetSortAnchorTime(out time)
					? time
					: DateTimeOffset.MinValue;
		}
	}
}
