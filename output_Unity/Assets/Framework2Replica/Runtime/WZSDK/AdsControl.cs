using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace WZSDK
{
	/// <summary>
	/// Gameplay-facing advertising facade backed by the project SDK adapter.
	/// </summary>
	public class AdsControl : MonoBehaviour
	{
		public enum MockRewardedOutcome
		{
			Success,
			HiddenWithoutReward,
			DisplayFailed
		}

		internal static string GetMockRewardedOutcomeName(
			MockRewardedOutcome value)
		{
			switch (value)
			{
				case MockRewardedOutcome.Success: return "Success";
				case MockRewardedOutcome.HiddenWithoutReward:
					return "HiddenWithoutReward";
				case MockRewardedOutcome.DisplayFailed: return "DisplayFailed";
				default: return "UnknownRewardedOutcome";
			}
		}

		private const string RemoveAdsPreferenceKey = "remove_ads";

		private static AdsControl instance;

		[Header("Local compatibility settings")]
		public bool showBanner;

		public bool showInterstitial = true;

		public int interstitialFrequency = 3;

		public UnityEvent onAdsInitialized;

		public UnityEvent onRewardedAdReady;

		public UnityEvent onInterstitialAdReady;

		private bool isInitialized;

		private Framework2AdsAdapter sdkAdapter;

		private int refuseCount;

		private int currentMaxRefuseCount;
		private MahjongSlide.Withdrawal.Framework2ProductConfig refusalConfiguration;

		private MockRewardedOutcome nextRewardedOutcome;

		private bool hasMockRewardedOutcome;

		private bool forceRewardedReadyForTesting;

		private bool nextRefuseShouldFail;

		private bool hasMockInterstitialOutcome;

		private bool forceInterstitialReadyForTesting;

		private Action<string> callbackTrace;

		public static AdsControl Instance => instance;

		// Read-only diagnostics: never advance the refusal counter or roll an ad.
		private bool IsRefusalInterstitialLevel
		{
			get
			{
				int level = GameManagerWZ.instance != null ? GameManagerWZ.instance.currentLv : 0;
				return Framework2Host.Settings != null &&
					level >= Framework2Host.Settings.refuseInterstitialLevelRange.x &&
					level <= Framework2Host.Settings.refuseInterstitialLevelRange.y;
			}
		}

		public int RefusalInterstitialPercent => IsRefusalInterstitialLevel ? 100 :
			(Framework2Host.Settings != null ? Framework2Host.Settings.refuseInterstitialPercent : 0);

		private void Awake()
		{
			if (instance != null && instance != this)
			{
				Destroy(gameObject);
				return;
			}

			instance = this;


			ApplyProductConfiguration();
			InitializeLocalState();
		}

		private void OnDestroy()
		{
			if (instance != this)
			{
				return;
			}


			instance = null;
		}

















		private void InitializeLocalState()
		{
			if (isInitialized)
			{
				return;
			}

			sdkAdapter = Framework2AdsAdapter.GetOrCreate();
			isInitialized = sdkAdapter != null;
			onAdsInitialized?.Invoke();
			if (sdkAdapter != null && sdkAdapter.IsRewardVideoReady)
			{
				onRewardedAdReady?.Invoke();
			}
			if (sdkAdapter != null && sdkAdapter.IsInterstitialReady)
			{
				onInterstitialAdReady?.Invoke();
			}
		}

		private void RandomizeMaxRefuseCount()
		{
			var configuration = Framework2Host.Settings;
			if (configuration == null)
				throw new InvalidOperationException("Framework 2 advertising requires its startup product.");
			Vector2Int range = configuration.refusalsBeforeAd;
			if (range.x < 1 || range.y < range.x || range.y == int.MaxValue)
				throw new InvalidOperationException("Framework 2 refusalsBeforeAd must be a positive inclusive range.");
			currentMaxRefuseCount = UnityEngine.Random.Range(range.x, range.y + 1);
			refusalConfiguration = configuration;
		}

		internal void ApplyProductConfiguration()
		{
			if (ReferenceEquals(refusalConfiguration, Framework2Host.Settings)) return;
			refuseCount = 0;
			RandomizeMaxRefuseCount();
		}

		private void ResetRefuseCount()
		{
			refuseCount = 0;
			RandomizeMaxRefuseCount();
		}

		public void HandleRefuseAd(
			string source,
			Action successCallback,
			Action<string> failCallback,
			bool autoShowWithdrawMissionView = true,
			bool grantStageAdReward = true)
		{
			refuseCount++;
			MahjongProductAnalytics.Track("f2_ad_refusal_count", "source", source, "before", refuseCount - 1, "after", refuseCount, "threshold", currentMaxRefuseCount, "triggered", refuseCount >= currentMaxRefuseCount, "show_task", autoShowWithdrawMissionView, "grant_stage_reward", grantStageAdReward);
			if (refuseCount < currentMaxRefuseCount)
			{
				failCallback?.Invoke(string.Empty);
				return;
			}

			ResetRefuseCount();
			if (hasMockInterstitialOutcome)
			{
				bool shouldFail = nextRefuseShouldFail;
				nextRefuseShouldFail = false;
				hasMockInterstitialOutcome = false;
				StartCoroutine(CompleteRefuseOnNextFrame(
					successCallback,
					failCallback,
					autoShowWithdrawMissionView,
					grantStageAdReward,
					shouldFail));
				return;
			}

			bool shouldShowInterstitial = IsRefusalInterstitialLevel;
			if (!shouldShowInterstitial)
			{
				shouldShowInterstitial =
					UnityEngine.Random.Range(0, 100) < Framework2Host.Settings.refuseInterstitialPercent;
			}

			MahjongProductAnalytics.Track("f2_refusal_ad_selected", "source", source, "format", shouldShowInterstitial ? "interstitial" : "rewarded", "interstitial_percent", RefusalInterstitialPercent, "next_threshold", currentMaxRefuseCount);
			if (!shouldShowInterstitial)
			{
				ShowRewardedAd(
					"force_reward",
					success =>
					{
						if (!success)
						{
							failCallback?.Invoke(
								"rewarded_display_failed");
							return;
						}

						successCallback?.Invoke();
					},
					autoShowWithdrawMissionView,
					grantStageAdReward);
				return;
			}

			ShowInterstitial(
				source,
				success =>
				{
					if (!success)
					{
						failCallback?.Invoke("interstitial_display_failed");
						return;
					}

					RecordLocalCompletion(
						autoShowWithdrawMissionView,
						grantStageAdReward);
					successCallback?.Invoke();
				});
		}

		private IEnumerator CompleteRefuseOnNextFrame(
			Action successCallback,
			Action<string> failCallback,
			bool autoShowWithdrawMissionView,
			bool grantStageAdReward,
			bool shouldFail)
		{
			yield return null;

			if (shouldFail)
			{
				callbackTrace?.Invoke("interstitial_display_failed");
				failCallback?.Invoke("local_test_failure");
				yield break;
			}

			RecordLocalCompletion(
				autoShowWithdrawMissionView,
				grantStageAdReward);
			successCallback?.Invoke();
		}

		public void LoadBannerAd()
		{
		}

		public void ShowBannerAd()
		{
		}

		public void HideBannerAd()
		{
		}

		public void DestroyBannerAd()
		{
		}

		public void LoadRewardedAd()
		{
			InitializeLocalState();
		}

		public void ShowRewardedAd(
			string rewardType,
			Action<bool> onComplete = null,
			bool autoShowWithdrawMissionView = true,
			bool grantStageAdReward = true)
		{
			InitializeLocalState();
			if (hasMockRewardedOutcome)
			{
				MockRewardedOutcome outcome = nextRewardedOutcome;
				nextRewardedOutcome = MockRewardedOutcome.Success;
				hasMockRewardedOutcome = false;
				StartCoroutine(CompleteRewardedOnNextFrame(
					onComplete,
					autoShowWithdrawMissionView,
					grantStageAdReward,
					outcome));
				return;
			}

			if (sdkAdapter == null)
			{
				onComplete?.Invoke(false);
				return;
			}

			callbackTrace?.Invoke("rewarded_displayed");
			sdkAdapter.ShowRewardVideo(
				rewardType,
				success =>
				{
					callbackTrace?.Invoke(
						success ? "rewarded_reward" : "rewarded_display_failed");
					if (success)
					{
						RecordLocalCompletion(
							autoShowWithdrawMissionView,
							grantStageAdReward);
					}
					onComplete?.Invoke(success);
				});
		}

		private IEnumerator CompleteRewardedOnNextFrame(
			Action<bool> onComplete,
			bool autoShowWithdrawMissionView,
			bool grantStageAdReward,
			MockRewardedOutcome outcome)
		{
			yield return null;

			callbackTrace?.Invoke("rewarded_displayed");
			switch (outcome)
			{
				case MockRewardedOutcome.HiddenWithoutReward:
					callbackTrace?.Invoke("rewarded_hidden");
					onComplete?.Invoke(false);
					yield break;
				case MockRewardedOutcome.DisplayFailed:
					callbackTrace?.Invoke("rewarded_display_failed");
					onComplete?.Invoke(false);
					yield break;
			}

			callbackTrace?.Invoke("rewarded_reward");
			RecordLocalCompletion(
				autoShowWithdrawMissionView,
				grantStageAdReward);
			onComplete?.Invoke(true);
		}

		public bool IsRewardedVideoAvailable()
		{
			InitializeLocalState();
			return forceRewardedReadyForTesting ||
				(sdkAdapter != null && sdkAdapter.IsRewardVideoReady);
		}

		public void SetNextRewardedOutcomeForTesting(
			MockRewardedOutcome outcome)
		{
			nextRewardedOutcome = outcome;
			hasMockRewardedOutcome = true;
		}

		public void ForceRewardedReadyForTesting()
		{
			InitializeLocalState();
			forceRewardedReadyForTesting = true;
			onRewardedAdReady?.Invoke();
		}

		public void SetNextInterstitialDisplayFailureForTesting(
			bool shouldFail)
		{
			nextRefuseShouldFail = shouldFail;
			hasMockInterstitialOutcome = true;
		}

		public void ForceInterstitialReadyForTesting()
		{
			forceInterstitialReadyForTesting = true;
			onInterstitialAdReady?.Invoke();
		}

		public void SetNextRefuseInterstitialOutcomeForTesting(
			bool shouldDisplayFail)
		{
			nextRefuseShouldFail = shouldDisplayFail;
			hasMockInterstitialOutcome = true;
			forceInterstitialReadyForTesting = true;
			refuseCount = Math.Max(0, currentMaxRefuseCount - 1);
		}

		public int GetRefuseCountForTesting()
		{
			return refuseCount;
		}

		public int GetRefuseThresholdForTesting()
		{
			return currentMaxRefuseCount;
		}

		public int AdvanceRefuseProgressForTesting()
		{
			refuseCount = Math.Min(
				refuseCount + 1,
				Math.Max(0, currentMaxRefuseCount - 1));
			return refuseCount;
		}

		public void SetRefuseOneBeforeTriggerForTesting()
		{
			refuseCount = Math.Max(0, currentMaxRefuseCount - 1);
		}

		public void ResetRefuseProgressForTesting()
		{
			ResetRefuseCount();
		}

		public void RecordSuccessfulAdForTesting(
			bool autoShowWithdrawMissionView = true)
		{
			RecordLocalCompletion(autoShowWithdrawMissionView, true);
		}

		public void ClearTestingOverrides()
		{
			nextRewardedOutcome = MockRewardedOutcome.Success;
			hasMockRewardedOutcome = false;
			forceRewardedReadyForTesting = false;
			nextRefuseShouldFail = false;
			hasMockInterstitialOutcome = false;
			forceInterstitialReadyForTesting = false;
			callbackTrace = null;
		}

		public void SetCallbackTraceForTesting(Action<string> trace)
		{
			callbackTrace = trace;
		}

		public void LoadInterstitialAd()
		{
			InitializeLocalState();
		}

		public void ShowInterstitialAd()
		{
			ShowInterstitial("interstitial", null);
		}

		private void ShowInterstitial(
			string placement,
			Action<bool> onComplete)
		{
			InitializeLocalState();
			if (hasMockInterstitialOutcome)
			{
				bool shouldFail = nextRefuseShouldFail;
				nextRefuseShouldFail = false;
				hasMockInterstitialOutcome = false;
				StartCoroutine(CompleteInterstitialOnNextFrame(
					onComplete,
					shouldFail));
				return;
			}

			if (sdkAdapter == null)
			{
				onComplete?.Invoke(false);
				return;
			}

			sdkAdapter.ShowInterstitial(
				placement,
				success =>
				{
					callbackTrace?.Invoke(
						success
							? "interstitial_closed"
							: "interstitial_display_failed");
					onComplete?.Invoke(success);
				});
		}

		private IEnumerator CompleteInterstitialOnNextFrame(
			Action<bool> onComplete,
			bool shouldFail)
		{
			yield return null;
			callbackTrace?.Invoke(
				shouldFail
					? "interstitial_display_failed"
					: "interstitial_closed");
			onComplete?.Invoke(!shouldFail);
		}

		public void RemoveAds()
		{
			PlayerPrefs.SetInt(RemoveAdsPreferenceKey, 1);
			PlayerPrefs.Save();
		}

		public bool IsRemoveAds()
		{
			return PlayerPrefs.GetInt(RemoveAdsPreferenceKey, 0) == 1;
		}

		private static void RecordLocalCompletion(
			bool autoShowWithdrawMissionView,
			bool grantStageAdReward)
		{
			ChallangeSuccessView.RecordAdSuccessProgress();
			GameManagerWZ gameManager = GameManagerWZ.instance;
			if (gameManager != null)
			{
				if (grantStageAdReward)
				{
					gameManager.RecordStageAdWatch(1);
				}
				else
				{
					gameManager.RecordStageAdWatchWithoutReward(1);
				}
			}

			ShowWithdrawMissionIfNeeded(autoShowWithdrawMissionView);
		}

		private static void ShowWithdrawMissionIfNeeded(bool autoShow)
		{
			MahjongProductAnalytics.Track("f2_ad_mission_presentation_check", "auto_show", autoShow, "stage", GameManagerWZ.instance != null ? GameManagerWZ.instance.currentStage : 0, "already_visible", UIManager.instance != null && UIManager.instance.HasActiveView(EUIType.WithdrawMissionView));
			GameManagerWZ gameManager = GameManagerWZ.instance;
			if (gameManager != null &&
				gameManager.currentStage == 3 &&
				autoShow &&
				UIManager.instance != null &&
				!UIManager.instance.HasActiveView(EUIType.WithdrawMissionView))
			{
				UIManager.instance.ShowView(EUIType.WithdrawMissionView);
			}
		}
	}
}
