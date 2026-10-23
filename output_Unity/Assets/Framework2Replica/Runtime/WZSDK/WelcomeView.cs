using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArrowOut.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	public class WelcomeView : BaseView
	{
		public Button AdBtn;

		public Button OnlyBtn;

		public Button Claim;

		public RectTransform RewardRoot;

		public Text OnlyText;

		private bool _isEventsBound;

		private List<LevelRewardStatic.RewardItem> rewards =
			new List<LevelRewardStatic.RewardItem>();

		private bool hasCustomRewards;

		private bool isRewardGiven;

		private bool isProcessing;

		private int guideType = -1;

		private int rewardIconLoadVersion;

		public override void InitView()
		{
			isRewardGiven = false;
			isProcessing = false;
			if (!hasCustomRewards)
			{
				rewards.Clear();
				rewards = GetRewards();
			}
			RefreshRewardUI();
			BindEvents();
		}

		public List<LevelRewardStatic.RewardItem> GetRewards()
		{
			List<LevelRewardStatic.RewardItem> result =
				new List<LevelRewardStatic.RewardItem>();
			GameManagerWZ manager = GameManagerWZ.instance;
			if (manager == null)
			{
				return result;
			}

			float coin = manager.GetCoinRewardAmount(1, -1);
			if (coin > 0f)
			{
				result.Add(new LevelRewardStatic.RewardItem
				{
					type = (int)ELuckySpinRewardType.Money,
					amount = coin
				});
			}

			float gem = manager.getResourceNum(2, 1, 0);
			if (gem > 0f)
			{
				result.Add(new LevelRewardStatic.RewardItem
				{
					type = (int)ELuckySpinRewardType.Gem,
					amount = gem
				});
			}
			return result;
		}

		protected override void HandleViewArgs(object[] args)
		{
			hasCustomRewards = false;
			rewards.Clear();
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
					rewards = CloneRewards(rewardList);
					continue;
				}

				LevelRewardStatic.RewardItem reward =
					arg as LevelRewardStatic.RewardItem;
				if (reward != null)
				{
					hasCustomRewards = true;
					rewards = new List<LevelRewardStatic.RewardItem>
					{
						CloneReward(reward)
					};
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
			if (RewardRoot == null)
			{
				return;
			}
			int iconLoadVersion = ++rewardIconLoadVersion;
			List<Image> iconTargets = new List<Image>();
			List<Task<Sprite>> iconTasks = new List<Task<Sprite>>();

			for (int i = 0; i < RewardRoot.childCount; i++)
			{
				GameObject rewardObject =
					RewardRoot.GetChild(i).gameObject;
				bool visible = i < rewards.Count;
				rewardObject.SetActive(visible);
				if (!visible)
				{
					continue;
				}

				LevelRewardStatic.RewardItem reward = rewards[i];
				Text amountText =
					rewardObject.GetComponentInChildren<Text>();
				if (amountText != null)
				{
					amountText.text = reward.type ==
						(int)ELuckySpinRewardType.Gem
							? ((int)reward.amount).ToString()
							: FormatRegionalAmount(reward.amount);
				}

				Image icon =
					rewardObject.GetComponentInChildren<Image>();
				if (icon != null)
				{
					iconTargets.Add(icon);
					iconTasks.Add(GetRewardIconAsync(
						ConvertToLuckySpinType(reward.type)));
				}
			}
			ApplyRewardIconsAsync(iconLoadVersion, iconTargets, iconTasks);

			if (OnlyText != null &&
				OnlyText.transform.parent != null)
			{
				OnlyText.transform.parent.gameObject.SetActive(false);
			}
		}

		private void BindEvents()
		{
			if (AdBtn != null)
			{
				AdBtn.onClick.RemoveAllListeners();
			}
			if (Claim != null)
			{
				Claim.onClick.RemoveAllListeners();
			}
			if (OnlyBtn != null)
			{
				OnlyBtn.onClick.RemoveAllListeners();
			}

			if (AdBtn != null)
			{
				AdBtn.onClick.AddListener(OnAdClick);
			}
			if (Claim != null)
			{
				Claim.onClick.AddListener(OnClalm);
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

		private void OnClalm()
		{
			PlayRewardEffects(rewards);
			CloseAndReset();
		}

		private void OnAdClick()
		{
			if (isProcessing || isRewardGiven)
			{
				return;
			}

			isProcessing = true;
			SoundManager.Instance?.PlayUIClickSFX();
			AdsControl.Instance.ShowRewardedAd(
				"challenge_success_reward",
				success =>
				{
					if (success && !isRewardGiven)
					{
						isRewardGiven = true;
						PlayRewardEffects(rewards);
						Debug.Log(string.Format(
							"广告观看成功，获得{0}个奖励（10倍）",
							rewards.Count));
					}
					else
					{
						Debug.Log("广告观看失败或奖励已发放");
					}
					CloseAndReset();
				},
				true);
		}

		private void OnOnlyClick()
		{
			if (isProcessing || isRewardGiven)
			{
				return;
			}

			isProcessing = true;
			SoundManager.Instance?.PlayUIClickSFX();
			AdsControl.Instance.HandleRefuseAd(
				"challenge_success",
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
				failMessage =>
				{
					if (!isRewardGiven &&
						rewards != null &&
						rewards.Count > 0)
					{
						isRewardGiven = true;
						float amount = rewards[0].amount / 10f;
						FacadeEffectExtend.PlayFlyMoneyHandle?.Invoke(
							transform,
							GameDefines.Elimination_FlyMoneyCount,
							amount,
							null,
							ERewardType.Money);
					}
					CloseAndReset();
				},
				true);
		}

		private void PlayRewardEffects(
			List<LevelRewardStatic.RewardItem> rewardsToPlay)
		{
			if (rewardsToPlay == null || rewardsToPlay.Count == 0)
			{
				return;
			}

			List<ERewardItemStruct> effectRewards =
				new List<ERewardItemStruct>();
			foreach (LevelRewardStatic.RewardItem reward in rewardsToPlay)
			{
				if (reward == null)
				{
					continue;
				}
				effectRewards.Add(new ERewardItemStruct
				{
					Type = ConvertToERewardType(
						ConvertToLuckySpinType(reward.type)),
					Count = reward.amount
				});
			}

			if (effectRewards.Count == 0)
			{
				return;
			}
			if (FacadeEffectExtend.PlayGetRewardEffectHandle != null)
			{
				FacadeEffectExtend.PlayGetRewardEffectHandle(
					effectRewards.ToArray(), null);
			}
		}

		private ERewardType ConvertToERewardType(
			ELuckySpinRewardType type)
		{
			return (int)type >= 1 && (int)type <= 4
				? (ERewardType)type
				: ERewardType.Money;
		}

		private void CloseAndReset()
		{
			hasCustomRewards = false;
			rewards.Clear();
			isRewardGiven = false;
			isProcessing = false;
			UIManager.instance?.CloseView(EUIType.WelcomeView, this);

			GameManagerWZ manager = GameManagerWZ.instance;
			if (manager == null)
			{
				return;
			}
			manager.RestartLevelTimer();
			if (guideType == 3)
			{
				manager.StartSourceFlow(DelayGoToStep(1, Framework2Host.Settings.guideRewardDelaySeconds));
			}
		}

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

		private static string FormatRegionalAmount(float amount)
		{
			GameManagerWZ gm = GameManagerWZ.instance;
			if (gm != null)
			{
				return gm.FormatRegionalValue((double)amount);
			}
			return FacadePayTypeExtend.RegionalChangeHandle != null
				? FacadePayTypeExtend.RegionalChangeHandle(amount)
				: amount.ToString();
		}

		private static List<LevelRewardStatic.RewardItem> CloneRewards(
			IEnumerable<LevelRewardStatic.RewardItem> source)
		{
			List<LevelRewardStatic.RewardItem> result =
				new List<LevelRewardStatic.RewardItem>();
			foreach (LevelRewardStatic.RewardItem reward in source)
			{
				if (reward != null)
				{
					result.Add(CloneReward(reward));
				}
			}
			return result;
		}

		private static LevelRewardStatic.RewardItem CloneReward(
			LevelRewardStatic.RewardItem reward)
		{
			return new LevelRewardStatic.RewardItem
			{
				type = reward.type,
				amount = reward.amount
			};
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
	}
}
