using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	public class GameBalancView : BaseView
	{
		private static double MinimumCashOutAmount => Framework2Host.Settings.economy.rewards.minimumCashOutAmount;

		private static double CashOutHandlingFee => Framework2Host.Settings.economy.rewards.cashOutHandlingFee;

		private static double FeeFreeCashOutThreshold => Framework2Host.Settings.economy.rewards.feeFreeCashOutThreshold;

		private const float RefreshInterval = 0.2f;

		private static readonly string[] LevelProgressDescriptionKeys =
		{
			"3073",
			"3074",
			"3075",
			"3076"
		};

		private Text currentCoinText;

		private RectTransform levelProgressRoot;

		private Button withdrawButton;

		private Button closeButton;

		private bool isInitialized;

		private bool isEventsBound;

		private float nextRefreshTime;

		public override void Start()
		{
		}

		public override void InitView()
		{
			MahjongProductAnalytics.Track("f2_wallet_cash_out_offer", "available", WithdrawApiService.GetCachedWalletBalance(), "minimum_exclusive", MinimumCashOutAmount, "displayed_handling_fee", CashOutHandlingFee, "fee_free_threshold", FeeFreeCashOutThreshold, "fee_presentation_only", true);
			ResolveReferencesIfNeeded();
			BindEvents();
			RefreshWalletBalance();
			RefreshCurrentCoin();
			RefreshLevelProgressTexts();
		}

		private void OnDestroy()
		{
			RemoveEvents();
		}

		public override void Update()
		{
			if (!isInitialized || Time.unscaledTime < nextRefreshTime)
			{
				return;
			}
			nextRefreshTime = Time.unscaledTime + RefreshInterval;
			RefreshView();
		}

		private void ResolveReferencesIfNeeded()
		{
			if (isInitialized)
			{
				return;
			}
			currentCoinText = FindByPath<Text>(
				"Plane/CurrrCoin", "CurrrCoin");
			levelProgressRoot = FindByPath<RectTransform>(
				"Plane/LevelProgressTxt", "LevelProgressTxt");
			withdrawButton = FindByPath<Button>(
				"Plane/With", "With");
			closeButton = FindByPath<Button>(
				"Plane/ExitBtn", "ExitBtn");
			isInitialized = currentCoinText != null ||
				levelProgressRoot != null ||
				withdrawButton != null ||
				closeButton != null;
		}

		private T FindByPath<T>(string path, string fallbackName) where T : Component
		{
			Transform target = transform.Find(path);
			if (target != null)
			{
				T component = target.GetComponent<T>();
				if (component != null)
				{
					return component;
				}
			}
			if (string.IsNullOrEmpty(fallbackName))
			{
				return null;
			}
			Transform[] descendants = GetComponentsInChildren<Transform>(true);
			for (int i = 0; i < descendants.Length; i++)
			{
				if (descendants[i].name == fallbackName)
				{
					T component = descendants[i].GetComponent<T>();
					if (component != null)
					{
						return component;
					}
				}
			}
			return null;
		}

		private void BindEvents()
		{
			if (isEventsBound)
			{
				return;
			}
			if (withdrawButton != null)
			{
				withdrawButton.onClick.AddListener(OnWithdrawClick);
			}
			if (closeButton != null)
			{
				closeButton.onClick.AddListener(OnCloseClick);
			}
			isEventsBound = true;
		}

		private void RemoveEvents()
		{
			if (!isEventsBound)
			{
				return;
			}
			if (withdrawButton != null)
			{
				withdrawButton.onClick.RemoveListener(OnWithdrawClick);
			}
			if (closeButton != null)
			{
				closeButton.onClick.RemoveListener(OnCloseClick);
			}
			isEventsBound = false;
		}

		private void OnWithdrawClick()
		{
			MahjongProductAnalytics.Track("f2_wallet_cash_out_check", "available", WithdrawApiService.GetCachedWalletBalance(), "minimum_exclusive", MinimumCashOutAmount, "eligible", WithdrawApiService.GetCachedWalletBalance() > MinimumCashOutAmount);
			if (WithdrawApiService.GetCachedWalletBalance() <=
				MinimumCashOutAmount)
			{
				string message = LocalizationManager.Instance != null
					? LocalizationManager.Instance.GetText("3093")
					: "Withdrawal amount must be greater than $0.10";
				if (string.IsNullOrEmpty(message))
				{
					message =
						"Withdrawal amount must be greater than $0.10";
				}
				// Legacy CSV fallback only; the host SO text resolves its economy token.
				if (Framework2Host.Settings.localizationDatabase == null)
					message = message.Replace("$0.10", FormatCoin(MinimumCashOutAmount));
				UIManager.instance?.ShowView(EUIType.NoticeView, message);
				return;
			}
			DataModel dataModel = CreateWithdrawData(GameManagerWZ.instance);
			WithdrawOrderData queuedOrder =
				WithdrawApiService.CreateQueuedWalletOrder(
					dataModel.Money, dataModel.Email);
			if (queuedOrder != null)
			{
				dataModel.OrderNo = queuedOrder.OrderNo;
				dataModel.QueuedOrderNo = queuedOrder.OrderNo;
				dataModel.WithdrawType = queuedOrder.WithdrawType;
			}
			if (!TryStartWalletWithdrawRequest(dataModel))
			{
				return;
			}
			GameApp.viewManager?.Open(ViewType.WithDrawProcess1, dataModel);
			UIManager.instance?.CloseView(EUIType.GameBalancView, this);
		}

		private bool TryStartWalletWithdrawRequest(DataModel dataModel)
		{
			Framework2BusinessAnalytics.Withdraw("wallet_submit_request", dataModel);
			if (dataModel == null || GameManagerWZ.instance == null)
			{
				Debug.LogWarning(
					"Wallet withdraw request skipped: invalid data or missing GameManager.");
				return false;
			}
			ClientWithdrawRequest request = BuildWalletWithdrawRequest(dataModel);
			if (request == null)
			{
				Debug.LogWarning(
					"Wallet withdraw request skipped: invalid request.");
				return false;
			}
			string queuedOrderNo = dataModel.QueuedOrderNo;
			GameManagerWZ.instance.StartCoroutine(
				WithdrawApiService.SubmitWithdraw(
					request,
					order =>
					{
						if (order == null)
						{
							Debug.LogWarning(
								"Wallet withdraw request returned empty order.");
							return;
						}
						if (!string.IsNullOrEmpty(queuedOrderNo))
						{
							WithdrawApiService.ReplaceQueuedWalletOrder(
								queuedOrderNo, order);
						}
						dataModel.QueuedOrderNo = string.Empty;
						dataModel.OrderNo = order.OrderNo;
						dataModel.WithdrawType = order.WithdrawType;
						dataModel.OrderStatus = order.OrderStatus;
						dataModel.Receiver = order.Receiver;
						dataModel.WalletFrozen = order.WalletFrozen;
						Framework2BusinessAnalytics.Withdraw("wallet_submit_result", dataModel, "stored_locally");
						GameApp.viewManager?
							.GetView<WithDraw>((int)ViewType.WithDraw)?
							.RefreshLuckyWalletPresentation();
					},
					error => Debug.LogWarning(
						"Wallet withdraw request returned error: " +
						error)));
			return true;
		}

		private ClientWithdrawRequest BuildWalletWithdrawRequest(DataModel dataModel)
		{
			if (dataModel == null)
			{
				return null;
			}
			string receiver = !string.IsNullOrEmpty(dataModel.Email)
				? dataModel.Email
				: GameManagerWZ.instance != null
					? GameManagerWZ.instance.getPlayerEmail()
					: string.Empty;
			return new ClientWithdrawRequest
			{
				WithdrawType = WithdrawConstants.WithdrawTypeWallet,
				Receiver = string.IsNullOrEmpty(receiver) ? null : receiver,
				Amount = Math.Round(dataModel.Money, 2, MidpointRounding.ToEven),
				Currency = WithdrawConstants.CurrencyUsd,
				Note = "Wallet withdraw",
				EmailSubject = string.Empty,
				RecipientType = WithdrawConstants.GetCurrentRecipientType()
			};
		}

		private void HandleWalletWithdrawError(DataModel dataModel, string error)
		{
			if (dataModel != null &&
				!string.IsNullOrEmpty(dataModel.QueuedOrderNo))
			{
				WithdrawApiService.RemoveQueuedWalletOrder(
					dataModel.QueuedOrderNo);
				dataModel.QueuedOrderNo = string.Empty;
			}
			if (!string.IsNullOrEmpty(error))
			{
				WithdrawApiService.LogPaymentNotice(
					"GameBalancView", error, LogType.Warning);
			}
			if (ShouldShowPayPalErrorView(error))
			{
				UIManager.instance?.ShowView(
					EUIType.PayPalErrorView, dataModel);
			}
		}

		private bool ShouldShowPayPalErrorView(string error)
		{
			return !string.IsNullOrEmpty(error) &&
				(error.Contains("PayPal收款账号不存在") ||
				 error.Contains("PayPal收款账号不能为空"));
		}

		private void OnCloseClick()
		{
			UIManager.instance?.CloseView(EUIType.GameBalancView, this);
		}

		private DataModel CreateWithdrawData(GameManagerWZ gm)
		{
			return WithdrawApiService.CreateWalletWithdrawDataModel();
		}

		internal override void RefreshRuntimeGmPresentation()
		{
			RefreshView();
		}

		private void RefreshView()
		{
			RefreshCurrentCoin();
			RefreshLevelProgressTexts();
		}

		private void RefreshCurrentCoin()
		{
			if (currentCoinText != null && GameManagerWZ.instance != null)
			{
				currentCoinText.text = FormatCoin(GetWithdrawMoney());
			}
		}

		private double GetWithdrawMoney()
		{
			return WithdrawApiService.GetCachedWalletBalance();
		}

		private int GetCurrentStage(GameManagerWZ gm)
		{
			return gm != null ? Math.Max(1, gm.getPlayerStage()) : 1;
		}

		private void RefreshLevelProgressTexts()
		{
			if (levelProgressRoot == null)
			{
				return;
			}
			int visibleCount = LevelProgressDescriptionKeys.Length;
			for (int i = 0; i < levelProgressRoot.childCount; i++)
			{
				Transform child = levelProgressRoot.GetChild(i);
				Text text = child.GetComponent<Text>();
				if (text == null)
				{
					continue;
				}
				child.gameObject.SetActive(i < visibleCount);
				if (i < visibleCount)
				{
					text.text = GetLocalizedLevelProgressDescription(
						LevelProgressDescriptionKeys[i]);
				}
			}
		}

		private string GetLocalizedLevelProgressDescription(string key)
		{
			if (LocalizationManager.Instance == null)
			{
				return key;
			}
			string value = LocalizationManager.Instance.GetText(key);
			if (string.IsNullOrEmpty(value))
			{
				return key;
			}
			if (Framework2Host.Settings.localizationDatabase == null && key == "3074")
			{
				return value.Replace(
					"$0.2", FormatCoin(CashOutHandlingFee));
			}
			if (Framework2Host.Settings.localizationDatabase == null && key == "3075")
			{
				return value.Replace(
					"$2", FormatCoin(FeeFreeCashOutThreshold));
			}
			return value;
		}

		private string FormatCoin(double coin)
		{
			GameManagerWZ gm = GameManagerWZ.instance;
			if (gm != null)
			{
				return gm.FormatRegionalValue(coin);
			}
			if (FacadePayTypeExtend.RegionalChangeHandle != null)
			{
				return FacadePayTypeExtend.RegionalChangeHandle((float)coin);
			}
			return GetCurrencyMark() + FormatPlainCoin(coin);
		}

		private string FormatPlainCoin(double coin)
		{
			int decimals = GameManagerWZ.instance != null
				? Mathf.Max(0, GameManagerWZ.instance.decimals)
				: 2;
			string value = coin.ToString("F" + decimals);
			return value.TrimEnd('0').TrimEnd('.');
		}

		private string GetCurrencyMark()
		{
			return GameManagerWZ.instance != null &&
				!string.IsNullOrEmpty(GameManagerWZ.instance.mark)
				? GameManagerWZ.instance.mark
				: "$";
		}

		private void RefreshWalletBalance()
		{
			if (GameManagerWZ.instance == null)
			{
				return;
			}
			StartCoroutine(WithdrawApiService.QueryWalletBalance(
				wallet =>
				{
					RefreshCurrentCoin();
					RefreshLevelProgressTexts();
					if (wallet != null &&
						wallet.PayPalAccountExists.GetValueOrDefault() == 0 &&
						wallet.PayPalAccountExists.HasValue &&
						!string.IsNullOrEmpty(wallet.PayPalAccountTip))
					{
						WithdrawApiService.LogPaymentNotice(
							"GameBalancView",
							wallet.PayPalAccountTip,
							LogType.Warning);
					}
				},
				error =>
				{
					Debug.LogError("Wallet balance query failed: " + error);
					RefreshCurrentCoin();
					RefreshLevelProgressTexts();
				}));
		}
	}
}
