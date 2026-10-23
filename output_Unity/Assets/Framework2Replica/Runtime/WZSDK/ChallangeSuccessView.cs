using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ArrowOut.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	public class ChallangeSuccessView : BaseView
	{
		[CompilerGenerated]
		private sealed class _003CDelayedAction_003Ed__21 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public ChallangeSuccessView _003C_003E4__this;

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
			public _003CDelayedAction_003Ed__21(int _003C_003E1__state)
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

		[CompilerGenerated]
		private sealed class _003CDelayedNextLevel_003Ed__38 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public ChallangeSuccessView _003C_003E4__this;

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
			public _003CDelayedNextLevel_003Ed__38(int _003C_003E1__state)
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

		private static int LuckySpinAdTarget => Framework2Host.Settings.economy.rewards.luckySpinAdTarget;

		private const string LuckySpinAdCountKey = "ChallengeSuccessLuckySpinAdCount_Global";

		private const string LuckySpinReadyKey = "ChallengeSuccessLuckySpinReady_Global";

		private const int GemRewardType = 1;

		private Button AdBtn;

		private Button OnlyBtn;
		internal Button HostOnlyButton => OnlyBtn;

		private RectTransform RewardRoot;

		private Text OnlyText;

		private Text Dec;

		private RectTransform ProgressRoot;

		private int currentLevel;

		private List<LevelRewardStatic.RewardItem> rewards;

		private bool _isEventsBound;

		private bool _isReferencesResolved;

		private int _rewardIconRefreshVersion;

		private Image progress;

		private RectTransform progressStateRoot;

		private WithdrawMissionStateItem[] progressStates = Array.Empty<WithdrawMissionStateItem>();

		private bool IsOnly;

		private bool shouldShowLuckySpin;

		private bool isProcessingAction;

		private bool isClosing;

		private void Awake()
		{
			ResolveReferencesIfNeeded();
		}

		public override void InitView()
		{
			ResolveReferencesIfNeeded();
			isProcessingAction = false;
			isClosing = false;
			SetButtonsInteractable(true);
			if (OnlyBtn != null)
			{
				OnlyBtn.gameObject.SetActive(false);
			}
			GameManagerWZ.instance?.StartSourceFlow(DelayedAction());
			IsOnly = false;
			shouldShowLuckySpin = HasPendingLuckySpinReward();
			rewards = BuildRewardsForCurrentLevel();
			RefreshModeUI();
			RefreshRewardUI();
			RefreshLuckySpinProgressUI();
			BindEvents();
		}

		[IteratorStateMachine(typeof(_003CDelayedAction_003Ed__21))]
		private IEnumerator DelayedAction()
		{
			return DelayedActionRoutine();
		}

		private void OnDisable()
		{
			_rewardIconRefreshVersion++;
		}

		public List<LevelRewardStatic.RewardItem> GetRewards()
		{
			List<LevelRewardStatic.RewardItem> result = new List<LevelRewardStatic.RewardItem>();
			float amount = GameManagerWZ.instance != null
				? GameManagerWZ.instance.GetCoinRewardAmount(1, currentLevel)
				: 0f;
			result.Add(new LevelRewardStatic.RewardItem
			{
				type = (int)ELuckySpinRewardType.Money,
				amount = amount
			});
			return result;
		}

		private List<LevelRewardStatic.RewardItem> BuildRewardsForCurrentLevel()
		{
			return FilterChallengeRewards(GetRewards());
		}

		private List<LevelRewardStatic.RewardItem> FilterChallengeRewards(List<LevelRewardStatic.RewardItem> sourceRewards)
		{
			List<LevelRewardStatic.RewardItem> result = new List<LevelRewardStatic.RewardItem>();
			if (sourceRewards == null)
			{
				return result;
			}
			foreach (LevelRewardStatic.RewardItem reward in sourceRewards)
			{
				if (reward == null || reward.type == GemRewardType ||
					(reward.type != (int)ELuckySpinRewardType.Money && reward.amount <= 0f))
				{
					continue;
				}
				result.Add(new LevelRewardStatic.RewardItem
				{
					type = reward.type,
					amount = reward.type == (int)ELuckySpinRewardType.Money
						? Mathf.Max(0f, reward.amount)
						: reward.amount
				});
			}
			return result;
		}

		internal override void RefreshRuntimeGmPresentation()
		{
			RefreshModeUI();
			RefreshRewardUI();
			RefreshLuckySpinProgressUI();
		}

		private void RefreshRewardUI()
		{
			if (rewards != null) for (int analyticsIndex = 0; analyticsIndex < rewards.Count; analyticsIndex++)
			{
				var analyticsReward = rewards[analyticsIndex];
				if (analyticsReward != null) MahjongProductAnalytics.State("f2_reward_offer", "settlement_" + analyticsIndex, "reward_source", "settlement", "index", analyticsIndex, "type", analyticsReward.type, "amount", analyticsReward.amount);
			}
			int refreshVersion = ++_rewardIconRefreshVersion;
			if (RewardRoot == null)
			{
				return;
			}
			bool hasRewards = rewards != null && rewards.Count > 0;
			RewardRoot.gameObject.SetActive(hasRewards);
			for (int i = 0; i < RewardRoot.childCount; i++)
			{
				GameObject item = RewardRoot.GetChild(i).gameObject;
				bool visible = hasRewards && i < rewards.Count;
				item.SetActive(visible);
				if (!visible)
				{
					continue;
				}
				LevelRewardStatic.RewardItem reward = rewards[i];
				Text count = item.GetComponentInChildren<Text>(true);
				if (count != null)
				{
					count.text = reward.type == GemRewardType
						? ((int)reward.amount).ToString()
						: FormatMoneyAmount(reward.amount);
				}
				Image icon = item.GetComponentInChildren<Image>(true);
				if (icon != null)
				{
					RefreshRewardIcon(
						icon,
						ConvertToLuckySpinType(reward.type),
						refreshVersion);
				}
			}
			if (hasRewards)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(RewardRoot);
			}
			if (OnlyText == null)
			{
				return;
			}
			Transform onlyTextParent = OnlyText.transform.parent;
			if (onlyTextParent != null)
			{
				onlyTextParent.gameObject.SetActive(hasRewards);
			}
			if (hasRewards)
			{
				float onlyAmount = rewards[0].amount / Framework2Host.Settings.economy.rewards.settlementNonAdDivisor;
				string formattedAmount = rewards[0].type == GemRewardType
					? onlyAmount.ToString("F1")
					: FormatMoneyAmount(onlyAmount);
				OnlyText.text = GetOnlyRewardPrefix() + formattedAmount;
			}
		}

		private string FormatMoneyAmount(float amount)
		{
			GameManagerWZ gm = GameManagerWZ.instance;
			if (gm != null)
			{
				return gm.FormatRegionalValue((double)amount);
			}
			return FacadePayTypeExtend.RegionalChangeHandle != null
				? FacadePayTypeExtend.RegionalChangeHandle(amount)
				: amount.ToString("0.##");
		}

		private string GetOnlyRewardPrefix()
		{
			return LocalizationManager.Instance != null
				? LocalizationManager.Instance.GetText("1021")
				: string.Empty;
		}

		private void BindEvents()
		{
			ResolveReferencesIfNeeded();
			if (_isEventsBound || (AdBtn == null && OnlyBtn == null))
			{
				return;
			}
			if (AdBtn != null)
			{
				AdBtn.onClick.AddListener(OnAdClick);
			}
			if (OnlyBtn != null)
			{
				OnlyBtn.onClick.AddListener(OnOnlyClick);
			}
			_isEventsBound = true;
		}

		private void RemoveEvents()
		{
			if (AdBtn != null)
			{
				AdBtn.onClick.RemoveAllListeners();
			}
			if (OnlyBtn != null)
			{
				OnlyBtn.onClick.RemoveAllListeners();
			}
			_isEventsBound = false;
		}

		private void ResolveReferencesIfNeeded()
		{
			if (_isReferencesResolved)
			{
				return;
			}
			AdBtn = FindByPath<Button>("Mask/Plane/AdBtn", "AdBtn");
			OnlyBtn = FindByPath<Button>("Mask/Plane/OnlyBtn", "OnlyBtn");
			RewardRoot = FindByPath<RectTransform>("Mask/Plane/RewardRoot", "RewardRoot");
			OnlyText = FindByPath<Text>("Mask/Plane/OnlyBtn/OnlyText", "OnlyText");
			Dec = FindByPath<Text>("Mask/Plane/ProgressRoot/Dec", "Dec");
			ProgressRoot = FindByPath<RectTransform>("Mask/Plane/ProgressRoot", "ProgressRoot");
			progress = FindByPath<Image>("Mask/Plane/ProgressRoot/Progress", "Progress");
			progressStateRoot = FindByPath<RectTransform>(
				"Mask/Plane/ProgressRoot/ProgressStateRoot",
				"ProgressStateRoot");
			if (progressStateRoot != null)
			{
				progressStates = new WithdrawMissionStateItem[progressStateRoot.childCount];
				int count = Mathf.Min(progressStateRoot.childCount, progressStates.Length);
				for (int i = 0; i < count; i++)
				{
					progressStates[i] = GetOrAddStateItem(progressStateRoot.GetChild(i).gameObject);
				}
			}
			_isReferencesResolved = AdBtn != null && OnlyBtn != null &&
				RewardRoot != null && OnlyText != null;
		}

		private void RefreshModeUI()
		{
			bool whitePackageMode = !GameDefines.ifIAA;
			if (ProgressRoot != null)
			{
				ProgressRoot.gameObject.SetActive(whitePackageMode);
				SetProgressRootRaycastEnabled(false);
			}
			if (!whitePackageMode)
			{
				ResetLuckySpinProgressState();
			}
		}

		private void SetProgressRootRaycastEnabled(bool enabled)
		{
			if (ProgressRoot == null)
			{
				return;
			}
			foreach (Graphic graphic in ProgressRoot.GetComponentsInChildren<Graphic>(true))
			{
				graphic.raycastTarget = enabled;
			}
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
			foreach (T component in GetComponentsInChildren<T>(true))
			{
				if (component.name == fallbackName)
				{
					return component;
				}
			}
			return null;
		}

		private void OnAdClick()
		{
			MahjongProductAnalytics.Track("f2_reward_choice", "reward_source", "settlement", "choice", "rewarded");
			if (isProcessingAction || isClosing || AdsControl.Instance == null)
			{
				return;
			}

			SoundManager.Instance?.PlayUIClickSFX();
			isProcessingAction = true;
			SetButtonsInteractable(false);
			AdsControl.Instance.ShowRewardedAd("challenge_success_reward", success =>
			{
				if (isClosing)
				{
					return;
				}
				if (success)
				{
					IsOnly = false;
					shouldShowLuckySpin = HasPendingLuckySpinReward();
				}
				CloseAndNextLevel();
			},
			autoShowWithdrawMissionView: false,
			grantStageAdReward: false);
		}

		private void OnOnlyClick()
		{
			MahjongProductAnalytics.Track("f2_reward_choice", "reward_source", "settlement", "choice", "normal");
			if (isProcessingAction || isClosing || AdsControl.Instance == null)
			{
				return;
			}

			SoundManager.Instance?.PlayUIClickSFX();
			isProcessingAction = true;
			SetButtonsInteractable(false);
			AdsControl.Instance.HandleRefuseAd(
				"force_reward",
				() =>
				{
					IsOnly = false;
					shouldShowLuckySpin = HasPendingLuckySpinReward();
					CloseAndNextLevel();
				},
				failMsg =>
				{
					IsOnly = true;
					CloseAndNextLevel();
				},
				autoShowWithdrawMissionView: false,
				grantStageAdReward: false);
		}

		private void PlayRewardEffects(
			List<LevelRewardStatic.RewardItem> rewardsToPlay,
			Action finishAction = null)
		{
			if (rewardsToPlay == null || rewardsToPlay.Count == 0)
			{
				finishAction?.Invoke();
				return;
			}
			List<ERewardItemStruct> items = new List<ERewardItemStruct>();
			foreach (LevelRewardStatic.RewardItem reward in rewardsToPlay)
			{
				if (reward == null)
				{
					continue;
				}
				MahjongProductAnalytics.Track("f2_reward_award_requested", "reward_source", "settlement", "type", reward.type, "amount", reward.amount);
				items.Add(new ERewardItemStruct
				{
					Type = ConvertToERewardType(ConvertToLuckySpinType(reward.type)),
					Count = reward.amount
				});
			}
			if (items.Count > 0)
			{
				Action<ERewardItemStruct[], Action> playEffect =
					FacadeEffectExtend.PlayGetRewardEffectHandle;
				if (playEffect != null)
				{
					playEffect.Invoke(items.ToArray(), finishAction);
					return;
				}
			}

			finishAction?.Invoke();
		}

		private ERewardType ConvertToERewardType(ELuckySpinRewardType type)
		{
			return (int)type >= 1 && (int)type <= 4
				? (ERewardType)type
				: ERewardType.Money;
		}

		private void CloseAndNextLevel()
		{
			MahjongProductAnalytics.Track("f2_settlement_continue", "only_reward", IsOnly, "pending_spin", shouldShowLuckySpin, "already_closing", isClosing);
			if (isClosing)
			{
				return;
			}

			isClosing = true;
			isProcessingAction = true;
			SetButtonsInteractable(false);
			Time.timeScale = 1f;
			if (GameManagerWZ.instance != null)
			{
				GameManagerWZ.instance.StartSourceFlow(DelayedNextLevel());
				return;
			}

			isClosing = false;
			isProcessingAction = false;
			SetButtonsInteractable(true);
		}

		private void SetButtonsInteractable(bool isInteractable)
		{
			if (AdBtn != null)
			{
				AdBtn.interactable = isInteractable;
			}
			if (OnlyBtn != null)
			{
				OnlyBtn.interactable = isInteractable;
			}
		}

		[IteratorStateMachine(typeof(_003CDelayedNextLevel_003Ed__38))]
		private IEnumerator DelayedNextLevel()
		{
			return DelayedNextLevelRoutine();
		}

		private ELuckySpinRewardType ConvertToLuckySpinType(int type)
		{
			return type >= 1 && type <= 4
				? (ELuckySpinRewardType)type
				: ELuckySpinRewardType.Money;
		}

		private async void RefreshRewardIcon(
			Image icon,
			ELuckySpinRewardType type,
			int refreshVersion)
		{
			Sprite sprite = await GetRewardIconAsync(type);
			if (this == null || refreshVersion != _rewardIconRefreshVersion ||
				icon == null || sprite == null)
			{
				return;
			}
			icon.sprite = sprite;
		}

		private async Task<Sprite> GetRewardIconAsync(ELuckySpinRewardType type)
		{
			switch (type)
			{
				case ELuckySpinRewardType.Money:
					if (!GameDefines.ifIAA &&
						FacadePayTypeExtend.GetMultCoinCoinAct != null)
					{
						Sprite coin =
							FacadePayTypeExtend.GetMultCoinCoinAct();
						if (coin != null)
						{
							return coin;
						}
					}
					return await LoadRewardSpriteAsync(
						GameDefines.ifIAA
							? GameDefines.LSIAAMoneyIconPath
							: null,
						GameDefines.ifIAA
							? "UI/LuckySpinIcons/icon_qianbi_IAA"
							: "UI/Money/Multiple/coin_big_us");
				case ELuckySpinRewardType.Gem:
					if (FacadePayTypeExtend.GetMulteGem != null)
					{
						Sprite gem = FacadePayTypeExtend.GetMulteGem();
						if (gem != null)
						{
							return gem;
						}
					}
					return await LoadRewardSpriteAsync(
						GameDefines.Default_SingDuileGem,
						"UI/Money/zuanshi_image");
				case ELuckySpinRewardType.AddBottle:
					return await LoadRewardSpriteAsync(
						GameDefines.ERAddSpaceIconPath,
						"UI/FuncIcon/Bottle");
				case ELuckySpinRewardType.Clear:
					return await LoadRewardSpriteAsync(
						GameDefines.ERClearIconPath,
						"UI/FuncIcon/icon_mofab");
				case ELuckySpinRewardType.Undo:
					return await LoadRewardSpriteAsync(
						GameDefines.ERHammerIconPath,
						"UI/FuncIcon/icon_shuaxin");
				default:
					return null;
			}
		}

		private static async Task<Sprite> LoadRewardSpriteAsync(
			string configuredAddress,
			string fallbackAddress)
		{
			Sprite sprite = !string.IsNullOrEmpty(configuredAddress)
				? await AddressableAssetService.LoadAsync<Sprite>(configuredAddress)
				: null;
			return sprite != null
				? sprite
				: await AddressableAssetService.LoadAsync<Sprite>(fallbackAddress);
		}

		protected override void HandleViewArgs(object[] args)
		{
			if (args != null && args.Length > 0 && args[0] is int)
			{
				currentLevel = (int)args[0];
			}
		}

		private void OnDestroy()
		{
			_rewardIconRefreshVersion++;
			if (_isEventsBound)
			{
				RemoveEvents();
			}
		}

		private void RefreshLuckySpinProgressUI()
		{
			if (GameDefines.ifIAA)
			{
				if (ProgressRoot != null)
				{
					ProgressRoot.gameObject.SetActive(false);
				}
				return;
			}
			int count = Mathf.Clamp(GetLuckySpinAdCount(), 0, LuckySpinAdTarget);
			for (int i = 0; i < progressStates.Length; i++)
			{
				progressStates[i]?.SetState(i + 1, i < count);
			}
			if (progress != null)
			{
				progress.fillAmount = count / (float)LuckySpinAdTarget;
			}
			if (Dec != null)
			{
				Dec.text = GetLuckySpinDecText(LuckySpinAdTarget - count);
			}
		}

		private string GetLuckySpinDecText(int remainingCount)
		{
			string template = LocalizationManager.Instance != null
				? LocalizationManager.Instance.GetText("3070")
				: null;
			if (string.IsNullOrEmpty(template))
			{
				template = "Watch {0} more ads to unlock the Lucky Wheel";
			}
			return string.Format(template, remainingCount);
		}

		private WithdrawMissionStateItem GetOrAddStateItem(GameObject target)
		{
			if (target == null)
			{
				return null;
			}
			WithdrawMissionStateItem item = target.GetComponent<WithdrawMissionStateItem>();
			return item != null ? item : target.AddComponent<WithdrawMissionStateItem>();
		}

		private int GetLuckySpinAdCount()
		{
			return PlayerPrefs.GetInt(LuckySpinAdCountKey, 0);
		}

		private void SaveLuckySpinAdCount(int count)
		{
			PlayerPrefs.SetInt(LuckySpinAdCountKey, Mathf.Clamp(count, 0, LuckySpinAdTarget));
			PlayerPrefs.Save();
		}

		private static void ResetLuckySpinProgressState()
		{
			if (!PlayerPrefs.HasKey(LuckySpinAdCountKey) &&
				!PlayerPrefs.HasKey(LuckySpinReadyKey))
			{
				return;
			}
			PlayerPrefs.DeleteKey(LuckySpinReadyKey);
			PlayerPrefs.DeleteKey(LuckySpinAdCountKey);
			PlayerPrefs.Save();
		}

		public static bool HasPendingLuckySpinReward()
		{
			if (GameDefines.ifIAA)
			{
				ResetLuckySpinProgressState();
				return false;
			}
			return PlayerPrefs.GetInt(LuckySpinReadyKey, 0) == 1;
		}

		public static void ConsumePendingLuckySpinReward()
		{
			ResetLuckySpinProgressState();
		}

		public static int GetLuckySpinAdCountForTesting()
		{
			return Mathf.Clamp(
				PlayerPrefs.GetInt(LuckySpinAdCountKey, 0),
				0,
				LuckySpinAdTarget);
		}

		public static int GetLuckySpinAdTargetForTesting()
		{
			return LuckySpinAdTarget;
		}

		public static void ResetLuckySpinAdProgressForTesting()
		{
			ResetLuckySpinProgressState();
		}

		private bool HasPendingLuckySpin()
		{
			return HasPendingLuckySpinReward();
		}

		private void ConsumePendingLuckySpin()
		{
			ResetLuckySpinProgressState();
		}

		public static void RecordAdSuccessProgress()
		{
			if (GameDefines.ifIAA)
			{
				ResetLuckySpinProgressState();
				return;
			}
			int count = PlayerPrefs.GetInt(LuckySpinAdCountKey, 0);
			if (PlayerPrefs.GetInt(LuckySpinReadyKey, 0) != 1 && count < LuckySpinAdTarget)
			{
				if (count == LuckySpinAdTarget - 1)
				{
					PlayerPrefs.SetInt(LuckySpinReadyKey, 1);
				}
				PlayerPrefs.SetInt(LuckySpinAdCountKey, count + 1);
				PlayerPrefs.Save();
			}
		}

		public override void Start()
		{
		}

		public override void Update()
		{
		}

		private IEnumerator DelayedActionRoutine()
		{
			yield return new WaitForSeconds(Framework2Host.Settings.economy.rewards.settlementNonAdButtonDelaySeconds);
			if (OnlyBtn != null)
			{
				OnlyBtn.gameObject.SetActive(true);
			}
		}

		private IEnumerator DelayedNextLevelRoutine()
		{
			UIManager.instance?.CloseView(EUIType.ChallangeSuccessView);
			float earliestTransitionTime = Time.realtimeSinceStartup +
				GameDefines.HideAnimTime;
			if (!IsOnly)
			{
				bool rewardEffectCompleted = false;
				PlayRewardEffects(
					rewards,
					() => rewardEffectCompleted = true);
				float rewardDeadline = Time.realtimeSinceStartup +
					Mathf.Max(1f, GameDefines.GRE_StayTime + 1f);
				while (!rewardEffectCompleted &&
					Time.realtimeSinceStartup < rewardDeadline)
				{
					yield return null;
				}
				if (!rewardEffectCompleted)
				{
					UnityEngine.Debug.LogWarning(
						"Reward effect timed out; settling it before the next level.");
					UIManager.instance?
						.GetView<UIEffectView>(EUIType.UIEffectView)?
						.CompletePendingRewardEffect();
				}
			}

			while (Time.realtimeSinceStartup < earliestTransitionTime)
			{
				yield return null;
			}

			GameManagerWZ manager = GameManagerWZ.instance;
			if (IsOnly && rewards != null && rewards.Count > 0 &&
				rewards[0] != null &&
				rewards[0].type == (int)ELuckySpinRewardType.Money &&
				rewards[0].amount > 0f && manager != null)
			{
				float amount = rewards[0].amount / Framework2Host.Settings.economy.rewards.settlementNonAdDivisor;
				manager.AddCoin(amount, currentLevel);
				FacadeEffectExtend.PlayFlyMoneyHandle?.Invoke(
					transform,
					GameDefines.Elimination_FlyMoneyCount,
					amount,
					null,
					ERewardType.Money);
			}
			manager?.NextLevel();

			if (shouldShowLuckySpin)
			{
				shouldShowLuckySpin = false;
				ResetLuckySpinProgressState();
				yield return null;
				UIManager.instance?.ShowView(EUIType.LuckySpinView);
			}
		}
	}
}
