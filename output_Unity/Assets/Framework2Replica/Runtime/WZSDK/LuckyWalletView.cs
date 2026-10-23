using System;
using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	public class LuckyWalletView : BaseView
	{
		private const float RefreshInterval = 0.2f;

		private Text currentCoinText;
		private Button withdrawButton;
		private Button close;
		private bool isInitialized;
		private bool isEventsBound;
		private float nextRefreshTime;

		private void Awake()
		{
			ResolveReferencesIfNeeded();
		}

		private void OnDestroy()
		{
			RemoveEvents();
		}

		public override void InitView()
		{
			ResolveReferencesIfNeeded();
			BindEvents();
			RefreshWalletBalance();
			RefreshView();
		}

		public override void Start()
		{
		}

		public override void Update()
		{
			if (isInitialized && Time.unscaledTime >= nextRefreshTime)
			{
				RefreshView();
				nextRefreshTime = Time.unscaledTime + RefreshInterval;
			}
		}

		private void ResolveReferencesIfNeeded()
		{
			if (isInitialized)
			{
				return;
			}

			currentCoinText = FindByPath<Text>(
				"Panel/Bg/Card/CurCoin", "CurCoin");
			withdrawButton = FindByPath<Button>(
				"Panel/Bg/Bottom/With", "With");
			close = FindByPath<Button>(
				"Panel/close", "close");
			isInitialized = currentCoinText != null || withdrawButton != null;
		}

		private T FindByPath<T>(string path, string fallbackName) where T : Component
		{
			GameObject child = FindChildUtility.FindChildByPath(gameObject, path);
			if (child == null)
			{
				child = FindChildUtility.FindChild(gameObject, fallbackName);
			}
			return child != null ? child.GetComponent<T>() : null;
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
			if (close != null)
			{
				close.onClick.AddListener(CloseClick);
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
			if (close != null)
			{
				close.onClick.RemoveListener(CloseClick);
			}
			isEventsBound = false;
		}

		private void CloseClick()
		{
			if (UIManager.instance != null)
			{
				UIManager.instance.CloseView(EUIType.LuckyWalletView, this);
				return;
			}
			CloseCurrentView();
		}

		private void OnWithdrawClick()
		{
			SoundManager.Instance?.PlayUIClickSFX();
			CloseCurrentView();
		}

		private void CloseCurrentView()
		{
			if (canvasGroup != null)
			{
				HideView();
				return;
			}
			gameObject.SetActive(false);
		}

		internal override void RefreshRuntimeGmPresentation()
		{
			RefreshView();
		}

		private void RefreshView()
		{
			if (currentCoinText == null || GameManagerWZ.instance == null)
			{
				return;
			}
			currentCoinText.text =
				GameManagerWZ.instance.FormatRegionalValue(
					WithdrawApiService.GetCachedWalletBalance());
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
					RefreshView();
					if (wallet != null &&
						wallet.PayPalAccountExists.GetValueOrDefault() == 0 &&
						wallet.PayPalAccountExists.HasValue &&
						!string.IsNullOrEmpty(wallet.PayPalAccountTip))
					{
						WithdrawApiService.LogPaymentNotice(
							"LuckyWallet", wallet.PayPalAccountTip, LogType.Warning);
					}
				},
				error =>
				{
					Debug.LogError("[LuckyWallet] Wallet balance request failed: " + error);
					RefreshView();
				}));
		}

	}
}
