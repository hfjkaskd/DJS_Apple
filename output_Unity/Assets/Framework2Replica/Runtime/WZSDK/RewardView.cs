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
	public class RewardView : BaseView
	{
		[CompilerGenerated]
		private sealed class _003CDelayGoToStep_003Ed__23 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public float delay;

			public int step;

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
			public _003CDelayGoToStep_003Ed__23(int _003C_003E1__state)
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
		private sealed class _003CDelayedAction_003Ed__11 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public RewardView _003C_003E4__this;

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
			public _003CDelayedAction_003Ed__11(int _003C_003E1__state)
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

		public Button AdBtn;

		public Button OnlyBtn;

		public RectTransform RewardRoot;

		public Text OnlyText;

		private bool _isEventsBound;

		private List<LevelRewardStatic.RewardItem> rewards;

		private bool hasCustomRewards;

		private bool isRewardGiven;

		private bool isProcessing;

		private int guideType;

		private int rewardIconLoadVersion;

		public override void InitView()
		{
			if (OnlyBtn != null)
			{
				OnlyBtn.gameObject.SetActive(false);
			}

			GameManagerWZ manager = GameManagerWZ.instance;
			if (manager != null)
			{
				manager.StartSourceFlow(DelayedAction());
			}
			else if (OnlyBtn != null)
			{
				OnlyBtn.gameObject.SetActive(true);
			}

			isRewardGiven = false;
			isProcessing = false;
			if (!hasCustomRewards)
			{
				if (rewards != null)
				{
					rewards.Clear();
				}
				rewards = GetRewards();
			}
			RefreshRewardUI();
			BindEvents();
			MahjongSlide.Withdrawal.MahjongRewardAudio.PopupOpened();
		}

		[IteratorStateMachine(typeof(_003CDelayedAction_003Ed__11))]
		private IEnumerator DelayedAction()
		{
			yield return new WaitForSeconds(Framework2Host.Settings.economy.rewards.inGameNonAdButtonDelaySeconds);
			if (OnlyBtn != null)
			{
				OnlyBtn.gameObject.SetActive(true);
			}
		}

		public List<LevelRewardStatic.RewardItem> GetRewards()
		{
			List<LevelRewardStatic.RewardItem> result =
				new List<LevelRewardStatic.RewardItem>();
			GameManagerWZ manager = GameManagerWZ.instance;
			float amount = manager != null
				? manager.GetCoinRewardAmount(1, -1)
				: 0f;
			result.Add(new LevelRewardStatic.RewardItem
			{
				type = (int)ELuckySpinRewardType.Money,
				amount = Mathf.Max(0f, amount)
			});
			return result;
		}

		private List<LevelRewardStatic.RewardItem> BuildRewardsForCurrentLevel()
		{
			return GetRewards();
		}

		protected override void HandleViewArgs(object[] args)
		{
			hasCustomRewards = false;
			if (rewards == null)
			{
				rewards = new List<LevelRewardStatic.RewardItem>();
			}
			else
			{
				rewards.Clear();
			}
			guideType = -1;

			if (args == null)
			{
				return;
			}

			foreach (object arg in args)
			{
				List<LevelRewardStatic.RewardItem> rewardList =
					arg as List<LevelRewardStatic.RewardItem>;
				if (rewardList != null && rewardList.Count > 0)
				{
					hasCustomRewards = true;
					rewards.Clear();
					foreach (LevelRewardStatic.RewardItem reward in rewardList)
					{
						if (reward == null)
						{
							continue;
						}
						rewards.Add(CloneReward(reward));
					}
					continue;
				}

				LevelRewardStatic.RewardItem singleReward =
					arg as LevelRewardStatic.RewardItem;
				if (singleReward != null)
				{
					hasCustomRewards = true;
					rewards.Clear();
					rewards.Add(CloneReward(singleReward));
					continue;
				}

				if (arg is int)
				{
					guideType = (int)arg;
				}
			}
		}

		internal override void RefreshRuntimeGmPresentation()
		{
			RefreshRewardUI();
		}

		private void RefreshRewardUI()
		{
			if (rewards != null) for (int analyticsIndex = 0; analyticsIndex < rewards.Count; analyticsIndex++)
			{
				var analyticsReward = rewards[analyticsIndex];
				if (analyticsReward != null) MahjongProductAnalytics.State("f2_reward_offer", "in_game_" + analyticsIndex, "reward_source", "in_game", "index", analyticsIndex, "type", analyticsReward.type, "amount", analyticsReward.amount);
			}
			if (RewardRoot == null)
			{
				return;
			}
			int iconLoadVersion = ++rewardIconLoadVersion;
			List<Image> iconTargets = new List<Image>();
			List<Task<Sprite>> iconTasks = new List<Task<Sprite>>();

			int rewardCount = rewards != null ? rewards.Count : 0;
			for (int i = 0; i < RewardRoot.childCount; i++)
			{
				GameObject item = RewardRoot.GetChild(i).gameObject;
				bool visible = i < rewardCount;
				item.SetActive(visible);
				if (!visible)
				{
					continue;
				}

				LevelRewardStatic.RewardItem reward = rewards[i];
				Text amountText = item.GetComponentInChildren<Text>();
				if (amountText != null)
				{
					amountText.text = FormatRewardAmount(reward);
				}

				Image icon = item.GetComponentInChildren<Image>();
				if (icon != null)
				{
					iconTargets.Add(icon);
					iconTasks.Add(GetRewardIconAsync(
						ConvertToLuckySpinType(reward.type)));
				}
			}
			ApplyRewardIconsAsync(iconLoadVersion, iconTargets, iconTasks);

			if (OnlyText == null)
			{
				return;
			}

			GameObject onlyTextRoot = OnlyText.transform.parent != null
				? OnlyText.transform.parent.gameObject
				: null;
			if (rewardCount < 1)
			{
				if (onlyTextRoot != null)
				{
					onlyTextRoot.SetActive(false);
				}
				return;
			}

			float onlyAmount = GetSafeRewardAmount(rewards[0]) / Framework2Host.Settings.economy.rewards.inGameNonAdDivisor;
			string amount = rewards[0].type ==
				(int)ELuckySpinRewardType.Gem
					? onlyAmount.ToString("F1")
					: FormatMoneyAmount(onlyAmount);
			OnlyText.text = GetOnlyRewardPrefix() + amount;
			if (onlyTextRoot != null)
			{
				onlyTextRoot.SetActive(true);
			}
		}

		private void BindEvents()
		{
			if (AdBtn != null)
			{
				AdBtn.onClick.RemoveAllListeners();
			}
			if (OnlyBtn != null)
			{
				OnlyBtn.onClick.RemoveAllListeners();
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

		private void OnAdClick()
		{
			MahjongProductAnalytics.Track("f2_reward_choice", "reward_source", "in_game", "choice", "rewarded");
			if (isProcessing || isRewardGiven)
			{
				return;
			}
			isProcessing = true;
			SoundManager.Instance?.PlayUIClickSFX();

			AdsControl ads = AdsControl.Instance;
			if (ads == null)
			{
				UnityEngine.Debug.LogWarning(
					"AdsControl instance is unavailable.");
				CloseAndReset();
				return;
			}
			ads.ShowRewardedAd(
				"reward_success_reward",
				success =>
				{
					if (success && !isRewardGiven)
					{
						isRewardGiven = true;
						PlayRewardEffects(rewards);
						UnityEngine.Debug.Log(string.Format(
							"广告观看成功，获得{0}个奖励（10倍）",
							rewards != null ? rewards.Count : 0));
					}
					else
					{
						UnityEngine.Debug.Log(
							"广告观看失败或奖励已发放");
					}
					CloseAndReset();
				},
				true);
		}

		private void OnOnlyClick()
		{
			MahjongProductAnalytics.Track("f2_reward_choice", "reward_source", "in_game", "choice", "normal");
			if (isProcessing || isRewardGiven)
			{
				return;
			}
			isProcessing = true;
			SoundManager.Instance?.PlayUIClickSFX();

			AdsControl ads = AdsControl.Instance;
			if (ads == null)
			{
				UnityEngine.Debug.LogWarning(
					"AdsControl instance is unavailable.");
				CloseAndReset();
				return;
			}
			ads.HandleRefuseAd(
				"force_reward",
				() =>
				{
					if (!isRewardGiven &&
						rewards != null &&
						rewards.Count > 0)
					{
						isRewardGiven = true;
						PlayRewardEffects(rewards);
					}
					CloseAndReset();
				},
				failMsg =>
				{
					if (!isRewardGiven &&
						rewards != null &&
						rewards.Count > 0)
					{
						isRewardGiven = true;
						LevelRewardStatic.RewardItem money =
							GetFirstMoneyReward();
						if (money != null)
						{
							float amount =
								GetSafeRewardAmount(money) / Framework2Host.Settings.economy.rewards.inGameNonAdDivisor;
							MahjongProductAnalytics.Track("f2_reward_award_requested", "reward_source", "in_game", "choice", "normal", "amount", amount, "divisor", Framework2Host.Settings.economy.rewards.inGameNonAdDivisor);
							GameManagerWZ.instance?.AddCoin(amount);
							FacadeEffectExtend.PlayFlyMoneyHandle?.Invoke(
								transform,
								GameDefines.Elimination_FlyMoneyCount,
								amount,
								null,
								ERewardType.Money);
						}
					}
					CloseAndReset();
				},
				true);
		}

		private void PlayRewardEffects(List<LevelRewardStatic.RewardItem> rewardsToPlay)
		{
			if (rewardsToPlay == null || rewardsToPlay.Count == 0)
			{
				return;
			}

			List<ERewardItemStruct> rewardItems =
				new List<ERewardItemStruct>();
			foreach (LevelRewardStatic.RewardItem reward in rewardsToPlay)
			{
				if (reward == null)
				{
					continue;
				}
				MahjongProductAnalytics.Track("f2_reward_award_requested", "reward_source", "in_game", "type", reward.type, "amount", reward.amount);
				rewardItems.Add(new ERewardItemStruct
				{
					Type = ConvertToERewardType(
						ConvertToLuckySpinType(reward.type)),
					Count = reward.amount
				});
			}

			if (rewardItems.Count == 0)
			{
				return;
			}
			if (FacadeEffectExtend.PlayGetRewardEffectHandle != null)
			{
				FacadeEffectExtend.PlayGetRewardEffectHandle(
					rewardItems.ToArray(), null);
			}
			else
			{
				GrantRewardsDirectly(rewardItems);
			}
		}

		private ERewardType ConvertToERewardType(ELuckySpinRewardType type)
		{
			return (int)type >= 1 && (int)type <= 4
				? (ERewardType)type
				: ERewardType.Money;
		}

		private void CloseAndReset()
		{
			MahjongProductAnalytics.Track("f2_in_game_reward_closed", "reward_given", isRewardGiven, "guide_type", guideType);
			hasCustomRewards = false;
			if (rewards != null)
			{
				rewards.Clear();
			}
			isRewardGiven = false;
			isProcessing = false;
			UIManager.instance?.CloseView(EUIType.RewardView, this);

			GameManagerWZ manager = GameManagerWZ.instance;
			if (manager != null)
			{
				manager.RestartLevelTimer();
				if (guideType == 3)
				{
					manager.StartSourceFlow(DelayGoToStep(1, Framework2Host.Settings.guideRewardDelaySeconds));
				}
			}
		}

		[IteratorStateMachine(typeof(_003CDelayGoToStep_003Ed__23))]
		private IEnumerator DelayGoToStep(int step, float delay)
		{
			yield return new WaitForSeconds(delay);
			GameManagerWZ manager = GameManagerWZ.instance;
			if (manager == null)
			{
				yield break;
			}
			Task<bool> presentation = Tutorial.ShowStepAsync(Tutorial.TYPE.TYPE3, step);
			while (!presentation.IsCompleted) yield return null;
		}

		private ELuckySpinRewardType ConvertToLuckySpinType(int type)
		{
			return type >= 1 && type <= 4
				? (ELuckySpinRewardType)type
				: ELuckySpinRewardType.Money;
		}

		private async Task<Sprite> GetRewardIconAsync(
			ELuckySpinRewardType type)
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

		private async void ApplyRewardIconsAsync(
			int loadVersion,
			List<Image> targets,
			List<Task<Sprite>> tasks)
		{
			Sprite[] sprites = await Task.WhenAll(tasks);
			if (this == null || loadVersion != rewardIconLoadVersion)
			{
				return;
			}
			for (int i = 0; i < targets.Count; i++)
			{
				if (targets[i] != null && sprites[i] != null)
				{
					targets[i].sprite = sprites[i];
				}
			}
		}

		private float GetSafeRewardAmount(LevelRewardStatic.RewardItem reward)
		{
			if (reward == null)
			{
				return 0f;
			}
			return reward.type == (int)ELuckySpinRewardType.Money
				? Mathf.Max(0f, reward.amount)
				: reward.amount;
		}

		private string FormatRewardAmount(LevelRewardStatic.RewardItem reward)
		{
			if (reward == null)
			{
				return string.Empty;
			}
			if (reward.type == (int)ELuckySpinRewardType.Gem)
			{
				return ((int)reward.amount).ToString();
			}
			return FormatMoneyAmount(reward.amount);
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

		private LevelRewardStatic.RewardItem GetFirstMoneyReward()
		{
			if (rewards == null)
			{
				return null;
			}
			foreach (LevelRewardStatic.RewardItem reward in rewards)
			{
				if (reward != null &&
					reward.type == (int)ELuckySpinRewardType.Money)
				{
					return reward;
				}
			}
			return null;
		}

		private void GrantRewardsDirectly(List<ERewardItemStruct> rewardStructs)
		{
			GameManagerWZ manager = GameManagerWZ.instance;
			if (rewardStructs == null || manager == null)
			{
				return;
			}
			foreach (ERewardItemStruct reward in rewardStructs)
			{
				switch (reward.Type)
				{
					case ERewardType.Money:
						manager.AddCoin(reward.Count);
						break;
					case ERewardType.Gem:
						manager.AddGems(reward.Count);
						break;
				}
			}
		}

		private void OnDestroy()
		{
			rewardIconLoadVersion++;
			RemoveEvents();
		}

		private void OnDisable()
		{
			rewardIconLoadVersion++;
			RemoveEvents();
		}

		public override void Start()
		{
		}

		public override void Update()
		{
		}

		private static LevelRewardStatic.RewardItem CloneReward(
			LevelRewardStatic.RewardItem reward)
		{
			return new LevelRewardStatic.RewardItem
			{
				type = reward.type,
				amount = reward.type ==
					(int)ELuckySpinRewardType.Money
						? Mathf.Max(0f, reward.amount)
						: reward.amount
			};
		}
	}
}
