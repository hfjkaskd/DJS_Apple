using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace WZSDK
{
	public class LuckySuccessView : BaseView
	{
		[CompilerGenerated]
		private sealed class _003CDelayedNextLevel_003Ed__25 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public bool shouldShowLuckySpin;

			object IEnumerator<object>.Current
			{
				[DebuggerHidden]
				get
				{
					return null;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					return null;
				}
			}

			[DebuggerHidden]
			public _003CDelayedNextLevel_003Ed__25(int _003C_003E1__state)
			{
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
			}

			private bool MoveNext()
			{
				return false;
			}

			bool IEnumerator.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				return this.MoveNext();
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
			}
		}

		private const float RefreshInterval = 0.2f;

		private static float DefaultLuckyWalletAdReward => Framework2Host.Settings.economy.rewards.luckyWalletSuccessFallbackAmount;

		private const string RewardedPlacement =
			"challenge_success_reward";

		private Text CurrentLuckyCoin;

		private Text CurrentLuckyCoinTxT;

		private Button AdBtn;

		private Button AbandonBtn;
		internal Button HostOnlyButton => AbandonBtn;

		private bool isReferencesResolved;

		private bool isEventsBound;

		private bool isProcessingAd;

		private bool isClosing;

		private int completedLuckyLevel;

		private float nextRefreshTime;

		private float currentRewardAmount;

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
			isProcessingAd = false;
			isClosing = false;
			SetButtonsInteractable(true);
			currentRewardAmount = DefaultLuckyWalletAdReward;
			RefreshView();
			RefreshConfiguredRewardAmount();
		}

		public override void Start()
		{
			ResolveReferencesIfNeeded();
			RefreshView();
		}

		public override void Update()
		{
			if (isReferencesResolved && Time.unscaledTime >= nextRefreshTime)
			{
				RefreshView();
				nextRefreshTime = Time.unscaledTime + RefreshInterval;
			}
		}

		protected override void HandleViewArgs(object[] args)
		{
			if (args != null && args.Length > 0 && args[0] is int)
			{
				completedLuckyLevel = Mathf.Max(0, (int)args[0]);
			}
		}

		private void ResolveReferencesIfNeeded()
		{
			if (isReferencesResolved)
			{
				return;
			}
			CurrentLuckyCoin = FindChildUtility.FindChild<Text>(
				gameObject, "CurrentLuckyCoin");
			CurrentLuckyCoinTxT = FindChildUtility.FindChild<Text>(
				gameObject, "CurrentLuckyCoinTxT");
			AdBtn = FindChildUtility.FindChild<Button>(gameObject, "AdBtn");
			AbandonBtn = FindChildUtility.FindChild<Button>(gameObject, "AbandonBtn");
			isReferencesResolved = CurrentLuckyCoin != null ||
				CurrentLuckyCoinTxT != null || AdBtn != null || AbandonBtn != null;
		}

		private void BindEvents()
		{
			if (isEventsBound)
			{
				return;
			}
			if (AdBtn != null)
			{
				AdBtn.onClick.AddListener(OnAdClick);
			}
			if (AbandonBtn != null)
			{
				AbandonBtn.onClick.AddListener(OnAbandonClick);
			}
			isEventsBound = true;
		}

		private void RemoveEvents()
		{
			if (!isEventsBound)
			{
				return;
			}
			if (AdBtn != null)
			{
				AdBtn.onClick.RemoveListener(OnAdClick);
			}
			if (AbandonBtn != null)
			{
				AbandonBtn.onClick.RemoveListener(OnAbandonClick);
			}
			isEventsBound = false;
		}

		private void OnAdClick()
		{
			MahjongProductAnalytics.Track("f2_lucky_settlement_choice", "choice", "rewarded", "amount", currentRewardAmount, "lucky_level", completedLuckyLevel);
			if (isProcessingAd || isClosing || AdsControl.Instance == null)
			{
				return;
			}
			SoundManager.Instance?.PlayUIClickSFX();
			isProcessingAd = true;
			SetButtonsInteractable(false);
			AdsControl.Instance.ShowRewardedAd(RewardedPlacement, success =>
			{
				isProcessingAd = false;
				MahjongProductAnalytics.Track("f2_lucky_settlement_ad_result", "success", success, "amount", currentRewardAmount, "lucky_level", completedLuckyLevel);
				if (success)
				{
					int level = ResolveCompletedLuckyLevel();
					if (level > 0)
					{
						WithdrawApiService.AddWalletIncome(
							currentRewardAmount,
							WithdrawConstants.WalletIncomeTypeLucky,
							level);
					}
					else
					{
						WithdrawApiService.AddWalletIncome(
							currentRewardAmount,
							WithdrawConstants.WalletIncomeTypeLucky);
					}
					CloseAndNextLevel(
						ChallangeSuccessView.HasPendingLuckySpinReward());
				}
				else if (!isClosing)
				{
					SetButtonsInteractable(true);
				}
			}, false);
		}

		private void OnAbandonClick()
		{
			MahjongProductAnalytics.Track("f2_lucky_settlement_choice", "choice", "abandon", "amount", currentRewardAmount, "lucky_level", completedLuckyLevel);
			if (isProcessingAd || isClosing)
			{
				return;
			}
			SoundManager.Instance?.PlayUIClickSFX();
			CloseAndNextLevel(ChallangeSuccessView.HasPendingLuckySpinReward());
		}

		private void CloseAndNextLevel(bool shouldShowLuckySpin)
		{
			if (isClosing)
			{
				return;
			}
			isClosing = true;
			SetButtonsInteractable(false);
			Time.timeScale = 1f;
			if (GameManagerWZ.instance != null)
			{
				GameManagerWZ.instance.StartSourceFlow(
					DelayedNextLevel(shouldShowLuckySpin));
			}
			else
			{
				UIManager.instance?.CloseView(EUIType.LuckySuccessView);
			}
		}

		[IteratorStateMachine(typeof(_003CDelayedNextLevel_003Ed__25))]
		private IEnumerator DelayedNextLevel(bool shouldShowLuckySpin)
		{
			return DelayedNextLevelRoutine(shouldShowLuckySpin);
		}

		private void SetButtonsInteractable(bool isInteractable)
		{
			if (AdBtn != null)
			{
				AdBtn.interactable = isInteractable;
			}
			if (AbandonBtn != null)
			{
				AbandonBtn.interactable = isInteractable;
			}
		}

		internal override void RefreshRuntimeGmPresentation()
		{
			RefreshView();
		}

		private void RefreshView()
		{
			GameManagerWZ gameManager = GameManagerWZ.instance;
			if (gameManager == null)
			{
				return;
			}
			string formattedCoin = gameManager.FormatRegionalValue(
				(double)currentRewardAmount);
			if (CurrentLuckyCoin != null)
			{
				CurrentLuckyCoin.text = formattedCoin;
			}
			if (CurrentLuckyCoinTxT != null)
			{
				CurrentLuckyCoinTxT.text = formattedCoin;
			}
		}

		private void RefreshConfiguredRewardAmount()
		{
			int level = ResolveCompletedLuckyLevel();
			StartCoroutine(WithdrawApiService.GetConfiguredLuckyReward(
				config =>
				{
					float amount = config != null &&
						config.LuckyAmount.HasValue
							? (float)config.LuckyAmount.Value
							: DefaultLuckyWalletAdReward;
					currentRewardAmount = RoundRewardAmount(amount);
					RefreshView();
				},
				error =>
				{
					currentRewardAmount = DefaultLuckyWalletAdReward;
					Debug.LogWarning(
						$"[LuckySuccess] level={level} local config failed: {error}");
					RefreshView();
				},
				WithdrawConstants.CurrencyUsd,
				level > 0 ? (int?)level : null));
		}

		private int ResolveCompletedLuckyLevel()
		{
			if (completedLuckyLevel > 0)
			{
				return completedLuckyLevel;
			}
			int currentLevel = GameManagerWZ.instance != null
				? GameManagerWZ.instance.currentLv
				: PlayerPrefs.GetInt(FacadePlayerPrefExtend.currentLevel, 1);
			return Mathf.Max(0, currentLevel - 1);
		}

		private static float RoundRewardAmount(float amount)
		{
			return (float)Math.Round(
				amount, 2, MidpointRounding.AwayFromZero);
		}

		private IEnumerator DelayedNextLevelRoutine(bool shouldShowLuckySpin)
		{
			UIManager.instance?.CloseView(EUIType.LuckySuccessView);
			yield return null;
			GameManagerWZ.instance?.NextLevel();
			if (!shouldShowLuckySpin)
			{
				yield break;
			}
			ChallangeSuccessView.ConsumePendingLuckySpinReward();
			yield return null;
			UIManager.instance?.ShowView(
				EUIType.LuckySpinView,
				new LuckySpinOpenData());
		}
	}
}
