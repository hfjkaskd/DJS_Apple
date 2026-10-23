using System;
using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	public class LuckyTips : BaseView
	{
		private Text curLuckyCoin;

		private Button closeButton;

		private Button continuebtn;

		private bool isBound;

		private void Awake()
		{
			EnsureCanvasGroup();
			ResolveReferencesIfNeeded();
		}

		private void OnDestroy()
		{
			RemoveEvents();
		}

		public override void InitView()
		{
			EnsureCanvasGroup();
			ResolveReferencesIfNeeded();
			BindEvents();
			RefreshLuckyWalletCoin();
			RefreshWalletBalance();
		}

		public override void Start()
		{
		}

		public override void Update()
		{
		}

		private void ResolveReferencesIfNeeded()
		{
			if (curLuckyCoin == null)
			{
				curLuckyCoin = FindChildUtility.FindChild<Text>(
					gameObject, "CurLuckyCoin");
			}
			if (closeButton == null)
			{
				closeButton = FindChildUtility.FindChild<Button>(
					gameObject, "close");
			}
			if (continuebtn == null)
			{
				continuebtn = FindChildUtility.FindChild<Button>(
					gameObject, "continuebtn");
			}
		}

		private void EnsureCanvasGroup()
		{
			if (canvasGroup == null)
			{
				canvasGroup = GetComponent<CanvasGroup>();
			}
			if (canvasGroup == null)
			{
				canvasGroup = gameObject.AddComponent<CanvasGroup>();
			}
		}

		private void BindEvents()
		{
			if (isBound)
			{
				return;
			}
			closeButton?.onClick.AddListener(OnCloseClick);
			continuebtn?.onClick.AddListener(OnCloseClick);
			WithdrawApiService.WalletBalanceChanged += OnWalletBalanceChanged;
			isBound = true;
		}

		private void RemoveEvents()
		{
			if (!isBound)
			{
				return;
			}
			closeButton?.onClick.RemoveListener(OnCloseClick);
			continuebtn?.onClick.RemoveListener(OnCloseClick);
			WithdrawApiService.WalletBalanceChanged -= OnWalletBalanceChanged;
			isBound = false;
		}

		private void OnWalletBalanceChanged(double balance)
		{
			RefreshLuckyWalletCoin(balance);
		}

		private void OnCloseClick()
		{
			SoundManager.Instance?.PlayUIClickSFX();
			UIManager.instance?.CloseView(EUIType.LuckyTips);
		}

		internal override void RefreshRuntimeGmPresentation()
		{
			RefreshLuckyWalletCoin();
		}

		private void RefreshLuckyWalletCoin()
		{
			RefreshLuckyWalletCoin(
				WithdrawApiService.GetCachedWalletBalance());
		}

		private void RefreshLuckyWalletCoin(double balance)
		{
			if (curLuckyCoin == null || GameManagerWZ.instance == null)
			{
				return;
			}
			curLuckyCoin.text = GameManagerWZ.instance.FormatRegionalValue(
				Math.Max(0.0, balance));
		}

		private void RefreshWalletBalance()
		{
			if (GameManagerWZ.instance == null)
			{
				return;
			}
			StartCoroutine(WithdrawApiService.QueryWalletBalance(
				wallet => RefreshLuckyWalletCoin(
					wallet != null
						? wallet.Balance
						: WithdrawApiService.GetCachedWalletBalance()),
				error =>
				{
					Debug.LogError(
						"LuckyTips wallet balance query failed: " +
						error);
					RefreshLuckyWalletCoin();
				},
				"USD"));
		}

	}
}
