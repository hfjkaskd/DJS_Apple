using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArrowOut.Runtime;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	public class LuckySpinView : BaseView
	{
		private const double ProbabilityPercentTotal = 100.0;
		private const double ProbabilityTolerance = 0.0001;
		private static float DefaultLuckyWalletRewardAmount => Framework2Host.Settings.economy.rewards.luckyWalletSpinFallbackAmount;
		private const float FirstLuckyWalletRewardDisplayDelay = 1f;

		protected RectTransform mPlane;
		protected Button mExitBtn;
		protected Button mLotteryBtn;
		internal Button HostSpinButton => mLotteryBtn;
		protected RectTransform mTable;
		protected Text mTTItem1name;
		protected Image mTTItem1Icon;
		protected Text mTTItem2name;
		protected Image mTTItem2Icon;
		protected Text mTTItem3name;
		protected Image mTTItem3Icon;
		protected Text mTTItem4name;
		protected Image mTTItem4Icon;
		protected Text mTTItem5name;
		protected Image mTTItem5Icon;
		protected Text mTTItem6name;
		protected Image mTTItem6Icon;

		private static Dictionary<int, ConfLuckySpin> s_SpinData;
		private static Dictionary<int, float> s_Angles;
		private static Dictionary<ELuckySpinRewardType, (string name, Sprite sprite)> s_SpriteCache;

		private static readonly ELuckySpinRewardType[] allRewardTypes =
		{
			ELuckySpinRewardType.Money,
			ELuckySpinRewardType.Gem,
			ELuckySpinRewardType.AddBottle,
			ELuckySpinRewardType.Clear,
			ELuckySpinRewardType.Undo,
			ELuckySpinRewardType.Prompt,
			ELuckySpinRewardType.Guid,
			ELuckySpinRewardType.LuckyWalletMoney
		};
		private static bool s_IsDataLoaded;
		private static int s_ConfigurationGeneration;

		internal static void ResetConfigurationCache()
		{
			s_ConfigurationGeneration++;
			s_IsDataLoaded = false;
			s_SpinData = null;
			s_Angles = null;
			s_SpriteCache = null;
		}

		private Dictionary<int, ConfLuckySpin> spinData;
		private Dictionary<int, float> angles;
		private bool mIsEventsBound;

		[SerializeField] private Framework2AppleRewardPresenter appleSkin;
		public void ConfigureAppleSkin(Framework2AppleRewardPresenter presenter) { appleSkin = presenter; }
		private bool mIsInitialized;
		private LuckySpinOpenData mOpenData;
		private bool mHasAutoStartedSpin;
		private int mSpriteLoadVersion;
		private bool mSpritePreloadInProgress;
		private bool mSpritePreloadUsesIaa;
		private static bool s_SpriteCacheUsesIaa;
		private string analyticsSpinId;
		private string analyticsDrawMode;

		public void Awake()
		{
			InitUIReferences();
		}

		protected override void HandleViewArgs(object[] args)
		{
			mOpenData = null;
			mHasAutoStartedSpin = false;
			if (args == null)
			{
				return;
			}

			for (int i = 0; i < args.Length; i++)
			{
				LuckySpinOpenData openData = args[i] as LuckySpinOpenData;
				if (openData != null)
				{
					mOpenData = openData;
					return;
				}
			}
		}

		public override void InitView()
		{
			if (!mIsInitialized)
			{
				if (mPlane == null)
				{
					InitUIReferences();
				}
				EnsureSpinDataLoaded();
				spinData = s_SpinData;
				angles = s_Angles;
				UpdateSpinUI();
				mIsInitialized = true;
			}
			RequestSpritePreload();

			OnEnableView();
			BindButtonEvents();
			StartAutoSpinIfNeeded();
		}

		private void OnDisable()
		{
			InvalidateSpritePreload();
		}

		public override void Start()
		{
		}

		public override void Update()
		{
		}

		private void InitUIReferences()
		{
			mPlane = FindRect("Plane");
			mExitBtn = FindComponent<Button>("ExitBtn");
			mLotteryBtn = FindComponent<Button>("Plane/LotteryBtn");
			mTable = FindRect("Plane/Turntable/Table");
			mTTItem1name = FindComponent<Text>("Plane/Turntable/Table/TTItem1/TTItem1name");
			mTTItem1Icon = FindComponent<Image>("Plane/Turntable/Table/TTItem1/TTItem1Icon");
			mTTItem2name = FindComponent<Text>("Plane/Turntable/Table/TTItem2/TTItem2name");
			mTTItem2Icon = FindComponent<Image>("Plane/Turntable/Table/TTItem2/TTItem2Icon");
			mTTItem3name = FindComponent<Text>("Plane/Turntable/Table/TTItem3/TTItem3name");
			mTTItem3Icon = FindComponent<Image>("Plane/Turntable/Table/TTItem3/TTItem3Icon");
			mTTItem4name = FindComponent<Text>("Plane/Turntable/Table/TTItem4/TTItem4name");
			mTTItem4Icon = FindComponent<Image>("Plane/Turntable/Table/TTItem4/TTItem4Icon");
			mTTItem5name = FindComponent<Text>("Plane/Turntable/Table/TTItem5/TTItem5name");
			mTTItem5Icon = FindComponent<Image>("Plane/Turntable/Table/TTItem5/TTItem5Icon");
			mTTItem6name = FindComponent<Text>("Plane/Turntable/Table/TTItem6/TTItem6name");
			mTTItem6Icon = FindComponent<Image>("Plane/Turntable/Table/TTItem6/TTItem6Icon");
		}

		private RectTransform FindRect(string path)
		{
			Transform child = transform.Find(path);
			return child as RectTransform;
		}

		private T FindComponent<T>(string path) where T : Component
		{
			Transform child = transform.Find(path);
			return child != null ? child.GetComponent<T>() : null;
		}

		private void EnsureSpinDataLoaded()
		{
			if (s_IsDataLoaded && s_SpinData != null && s_SpinData.Count > 0)
			{
				return;
			}

			LoadSpinDataFromCSVStatic();
			InitSpinDataStatic();
			s_IsDataLoaded = s_SpinData != null && s_SpinData.Count > 0;
			Debug.Log("[LuckySpin] Loaded " + (s_SpinData != null ? s_SpinData.Count : 0) + " rewards.");
		}

		private void LoadSpinDataFromCSVStatic()
		{
			UserDataManager dataManager = UserDataManager.Instance;
			if (dataManager == null)
			{
				Debug.LogError("[LuckySpin] UserDataManager is unavailable.");
				s_SpinData = new Dictionary<int, ConfLuckySpin>();
				return;
			}

			Dictionary<int, ConfLuckySpin> loaded = dataManager.GetLuckySpinData();
			s_SpinData = loaded != null
				? new Dictionary<int, ConfLuckySpin>(loaded)
				: new Dictionary<int, ConfLuckySpin>();
		}

		private static void InitSpinDataStatic()
		{
			s_Angles = new Dictionary<int, float>();
			if (s_SpinData == null || s_SpinData.Count == 0)
			{
				return;
			}

			List<float> sourceAngles = GameDefines.LS_Angles;
			int index = 0;
			foreach (KeyValuePair<int, ConfLuckySpin> pair in s_SpinData.OrderBy(pair => pair.Key))
			{
				float angle = sourceAngles != null && index < sourceAngles.Count
					? sourceAngles[index]
					: 0f;
				s_Angles[pair.Key] = angle;
				index++;
			}
		}

		internal override void RefreshRuntimeGmPresentation()
		{
			RequestSpritePreload(true);
			UpdateSpinUI();
		}

		private void RequestSpritePreload(bool force = false)
		{
			if (this == null || !isActiveAndEnabled)
			{
				return;
			}

			bool useIaa = GameDefines.ifIAA;
			if (!force && s_SpriteCache != null &&
				s_SpriteCacheUsesIaa == useIaa)
			{
				return;
			}
			if (!force && mSpritePreloadInProgress &&
				mSpritePreloadUsesIaa == useIaa)
			{
				return;
			}

			int loadVersion = ++mSpriteLoadVersion;
			mSpritePreloadInProgress = true;
			mSpritePreloadUsesIaa = useIaa;
			PreloadSprites(loadVersion, useIaa, s_ConfigurationGeneration);
		}

		private async void PreloadSprites(int loadVersion, bool useIaa, int configurationGeneration)
		{
			try
			{
				Dictionary<ELuckySpinRewardType, (string name, Sprite sprite)> cache =
					await PreloadSpritesAsync(useIaa);
				if (configurationGeneration != s_ConfigurationGeneration ||
					!CanApplySpritePreload(loadVersion, useIaa))
				{
					return;
				}

				s_SpriteCache = cache;
				s_SpriteCacheUsesIaa = useIaa;
				UpdateSpinUI();
			}
			catch (Exception exception)
			{
				if (configurationGeneration == s_ConfigurationGeneration &&
					CanApplySpritePreload(loadVersion, useIaa))
				{
					Debug.LogWarning(
						"[LuckySpin] Sprite preload failed: " + exception.Message);
				}
			}
			finally
			{
				if (this != null && loadVersion == mSpriteLoadVersion)
				{
					mSpritePreloadInProgress = false;
				}
			}
		}

		private async Task<Dictionary<ELuckySpinRewardType, (string name, Sprite sprite)>>
			PreloadSpritesAsync(bool useIaa)
		{
			ELuckySpinRewardType[] types = allRewardTypes;
			Task<(string name, Sprite sprite)>[] loadTasks = types
				.Select(type => GetItemInfoAsync(type, useIaa))
				.ToArray();
			(string name, Sprite sprite)[] items = await Task.WhenAll(loadTasks);

			Dictionary<ELuckySpinRewardType, (string name, Sprite sprite)> cache =
				new Dictionary<ELuckySpinRewardType, (string name, Sprite sprite)>();
			for (int i = 0; i < types.Length; i++)
			{
				cache[types[i]] = items[i];
			}
			return cache;
		}

		private bool CanApplySpritePreload(int loadVersion, bool useIaa)
		{
			return this != null && isActiveAndEnabled &&
				loadVersion == mSpriteLoadVersion &&
				useIaa == GameDefines.ifIAA;
		}

		private void InvalidateSpritePreload()
		{
			mSpriteLoadVersion++;
			mSpritePreloadInProgress = false;
		}

		private void UpdateSpinUI()
		{
			if (spinData == null)
			{
				spinData = s_SpinData;
			}
			if (spinData == null)
			{
				return;
			}

			int position = 0;
			foreach (KeyValuePair<int, ConfLuckySpin> pair in spinData.OrderBy(pair => pair.Key))
			{
				if (position >= 6 || pair.Value == null)
				{
					break;
				}

				ELuckySpinRewardType type = (ELuckySpinRewardType)pair.Value.Type;
				float count = GetResolvedRewardCount(pair.Value);
				(string name, Sprite sprite) item = GetItemInfoFromCache(type, count);
				SetItemShow(position, item.name, item.sprite);
				position++;
			}
		}

		private (string, Sprite) GetItemInfoFromCache(ELuckySpinRewardType type, float count)
		{
			if (appleSkin != null && !GameDefines.ifIAA)
			{
				Sprite skinSprite = appleSkin.ResolveRewardSprite(type, false);
				if (skinSprite != null) return (GetLocalizedTextStatic(GetLocalizationKey(type)), skinSprite);
			}
			(string name, Sprite sprite) item;
			if (s_SpriteCache != null && s_SpriteCacheUsesIaa == GameDefines.ifIAA &&
				s_SpriteCache.TryGetValue(type, out item))
			{
				return (item.name ?? string.Empty, item.sprite);
			}

			RequestSpritePreload();
			if (s_SpriteCache != null && s_SpriteCacheUsesIaa == GameDefines.ifIAA &&
				s_SpriteCache.TryGetValue(type, out item))
			{
				return (item.name ?? string.Empty, item.sprite);
			}
			return (GetLocalizedTextStatic(GetLocalizationKey(type)), null);
		}

		private async Task<(string name, Sprite sprite)> GetItemInfoAsync(
			ELuckySpinRewardType type,
			bool useIaa)
		{
			string name = GetLocalizedTextStatic(GetLocalizationKey(type));
			Sprite sprite = null;
			switch (type)
			{
				case ELuckySpinRewardType.Money:
					sprite = useIaa
						? await AddressableAssetService.LoadAsync<Sprite>(
							GameDefines.LSIAAMoneyIconPath)
						: await GetSingleCoinSpriteAsync();
					break;
				case ELuckySpinRewardType.Gem:
					sprite = await GetSingleGemSpriteAsync();
					break;
				case ELuckySpinRewardType.AddBottle:
					sprite = await AddressableAssetService.LoadAsync<Sprite>(
						GameDefines.ERAddSpaceIconPath);
					break;
				case ELuckySpinRewardType.Clear:
					sprite = await AddressableAssetService.LoadAsync<Sprite>(
						GameDefines.ERClearIconPath);
					break;
				case ELuckySpinRewardType.Undo:
					sprite = await AddressableAssetService.LoadAsync<Sprite>(
						GameDefines.ERHammerIconPath);
					break;
				case ELuckySpinRewardType.Prompt:
					sprite = await AddressableAssetService.LoadAsync<Sprite>(
						GameDefines.ERAddPromptIconPath);
					break;
				case ELuckySpinRewardType.Guid:
					sprite = await AddressableAssetService.LoadAsync<Sprite>(
						GameDefines.ERAddGuidIconPath);
					break;
				case ELuckySpinRewardType.LuckyWalletMoney:
					sprite = await AddressableAssetService.LoadAsync<Sprite>(
						GameDefines.LSLuckyWalletIconPath);
					break;
			}
			return (name, sprite);
		}

		private static string GetLocalizationKey(ELuckySpinRewardType type)
		{
			switch (type)
			{
				case ELuckySpinRewardType.Gem:
					return "1007";
				case ELuckySpinRewardType.AddBottle:
					return "1008";
				case ELuckySpinRewardType.Clear:
					return "1009";
				case ELuckySpinRewardType.Undo:
					return "1010";
				case ELuckySpinRewardType.Prompt:
					return "1009";
				case ELuckySpinRewardType.Guid:
					return "1010";
				case ELuckySpinRewardType.LuckyWalletMoney:
					return "3013";
				default:
					return "1006";
			}
		}

		private static string GetLocalizedTextStatic(string key)
		{
			return LocalizationManager.Instance != null
				? LocalizationManager.Instance.GetText(key)
				: key;
		}

		private void SetItemShow(int pos, string name, Sprite icon)
		{
			Text label = null;
			Image image = null;
			switch (pos)
			{
				case 0: label = mTTItem1name; image = mTTItem1Icon; break;
				case 1: label = mTTItem2name; image = mTTItem2Icon; break;
				case 2: label = mTTItem3name; image = mTTItem3Icon; break;
				case 3: label = mTTItem4name; image = mTTItem4Icon; break;
				case 4: label = mTTItem5name; image = mTTItem5Icon; break;
				case 5: label = mTTItem6name; image = mTTItem6Icon; break;
			}

			if (label != null)
			{
				label.text = name;
			}
			if (image != null)
			{
				image.sprite = icon;
			}
		}

		private void BindButtonEvents()
		{
			if (mIsEventsBound)
			{
				RemoveButtonEvents();
			}
			if (mExitBtn != null)
			{
				mExitBtn.onClick.AddListener(OnExitBtnClickHandle);
			}
			if (mLotteryBtn != null)
			{
				mLotteryBtn.onClick.AddListener(OnLotteryBtnClickHandle);
			}
			mIsEventsBound = true;
		}

		private void RemoveButtonEvents()
		{
			if (mExitBtn != null)
			{
				mExitBtn.onClick.RemoveAllListeners();
			}
			if (mLotteryBtn != null)
			{
				mLotteryBtn.onClick.RemoveAllListeners();
			}
			mIsEventsBound = false;
		}

		public void OnEnableView()
		{
			if (mExitBtn != null)
			{
				mExitBtn.gameObject.SetActive(mOpenData == null || !mOpenData.IsMandatory);
			}
			if (mLotteryBtn != null)
			{
				mLotteryBtn.gameObject.SetActive(true);
			}
			if (mTable != null)
			{
				mTable.rotation = Quaternion.identity;
			}
		}

		private void StartAutoSpinIfNeeded()
		{
			if (mHasAutoStartedSpin || mOpenData == null || !mOpenData.AutoStartSpin ||
				mLotteryBtn == null || !mLotteryBtn.gameObject.activeInHierarchy)
			{
				return;
			}

			mHasAutoStartedSpin = true;
			StartManagedCoroutine(AutoStartSpinCoroutine());
		}

		private IEnumerator AutoStartSpinCoroutine()
		{
			yield return null;
			if (mLotteryBtn != null && mLotteryBtn.gameObject.activeInHierarchy)
			{
				OnLotteryBtnClickHandle();
			}
		}

		private void OnExitBtnClickHandle()
		{
			MahjongProductAnalytics.Track("f2_lucky_spin_exit", "operation_id", analyticsSpinId);
			if (UIManager.instance != null)
			{
				UIManager.instance.CloseView(EUIType.LuckySpinView, this);
			}
		}

		private void OnLotteryBtnClickHandle()
		{
			if (mExitBtn != null)
			{
				mExitBtn.gameObject.SetActive(false);
			}
			if (mLotteryBtn != null)
			{
				mLotteryBtn.gameObject.SetActive(false);
			}
			RotateTable();
		}

		private void RotateTable()
		{
			analyticsSpinId = MahjongProductAnalytics.Begin("f2_lucky_spin", "mandatory", mOpenData != null && mOpenData.IsMandatory, "auto_spin", mOpenData != null && mOpenData.AutoStartSpin, "guarantee_already_claimed", HasClaimedLuckyWalletReward());
			analyticsDrawMode = "weighted";
			if (spinData != null) foreach (var analyticsEntry in spinData)
			{
				if (analyticsEntry.Value != null)
					MahjongProductAnalytics.Track("f2_lucky_spin_option", "operation_id", analyticsSpinId, "index", analyticsEntry.Key, "reward_type", analyticsEntry.Value.Type, "amount", analyticsEntry.Value.Count, "weight", analyticsEntry.Value.Probability);
			}
			if (spinData == null || spinData.Count == 0)
			{
				Debug.LogError("[LuckySpin] Reward data is empty.");
				OnEnableView();
				return;
			}

			int target;
			float count;
			bool useForcedMoneyReward = TryGetForcedMoneyReward(out target, out count);
			if (!useForcedMoneyReward)
			{
				target = GetRewardIndexByProbability();
			}
			if (!spinData.ContainsKey(target))
			{
				Debug.LogError("[LuckySpin] Missing target reward " + target + ".");
				target = 0;
				useForcedMoneyReward = false;
			}

			ConfLuckySpin conf = spinData[target];
			ELuckySpinRewardType type = (ELuckySpinRewardType)conf.Type;
			if (!useForcedMoneyReward)
			{
				count = GetResolvedRewardCount(conf);
			}

			MahjongProductAnalytics.Track("f2_lucky_spin_selected", "operation_id", analyticsSpinId, "draw_mode", useForcedMoneyReward ? "forced" : analyticsDrawMode, "index", target, "reward_type", (int)type, "display_amount", count, "weight", conf.Probability, "money_target", (int)GetMoneyRewardTarget());
			float angle;
			if (angles == null || !angles.TryGetValue(target, out angle))
			{
				Debug.LogError("[LuckySpin] Missing target angle " + target + ".");
				angle = 0f;
				if (angles != null)
				{
					angles[target] = angle;
				}
			}

			if (mTable == null)
			{
				OnEnableView();
				return;
			}

			float targetAngle = angle - GameDefines.LS_rotateCount * 360f;
			mTable
				.DOLocalRotate(
					Vector3.forward * targetAngle,
					GameDefines.LS_rotateTime,
					RotateMode.FastBeyond360)
				.SetEase(Ease.OutQuint)
				.OnComplete(() =>
				{
					StartManagedCoroutine(DelayedAction(0.5f, () =>
					{
						if (UIManager.instance != null)
						{
							UIManager.instance.CloseView(EUIType.LuckySpinView, this);
						}
					}));
					MahjongProductAnalytics.Track("f2_lucky_spin_stopped", "operation_id", analyticsSpinId, "index", target, "reward_type", (int)type, "amount", count);
					PlayRewardEffect(
						type,
						count,
						mOpenData != null ? mOpenData.OnRewardComplete : null,
						useForcedMoneyReward);
				});
			StartManagedCoroutine(PlaySpinSound());
		}

		private bool TryGetForcedMoneyReward(out int target, out float rewardAmount)
		{
			target = -1;
			rewardAmount = 0f;
			if (mOpenData == null || !mOpenData.ForceMoneyReward || mOpenData.ForcedMoneyAmount <= 0f)
			{
				return false;
			}

			List<int> moneyTargets = spinData
				.Where(pair => pair.Value != null && pair.Value.Type == (int)ELuckySpinRewardType.Money)
				.Select(pair => pair.Key)
				.ToList();
			if (moneyTargets.Count == 0)
			{
				return false;
			}

			target = moneyTargets[UnityEngine.Random.Range(0, moneyTargets.Count)];
			rewardAmount = mOpenData.ForcedMoneyAmount;
			return true;
		}

		private int GetRewardIndexByProbability()
		{
			int guaranteedTarget;
			if (TryGetGuaranteedLuckyWalletReward(out guaranteedTarget))
			{
				analyticsDrawMode = "first_wallet_guarantee";
				return guaranteedTarget;
			}
			if (spinData == null || spinData.Count == 0)
			{
				Debug.LogError("[LuckySpin] Reward data is empty.");
				return 0;
			}

			List<KeyValuePair<int, ConfLuckySpin>> ordered = spinData.OrderBy(pair => pair.Key).ToList();
			double total = ordered.Sum(pair => pair.Value != null ? pair.Value.Probability : 0d);
			if (total <= 0d)
			{
				analyticsDrawMode = "uniform_fallback";
				return ordered[UnityEngine.Random.Range(0, ordered.Count)].Key;
			}
			if (Math.Abs(total - ProbabilityPercentTotal) > ProbabilityTolerance)
			{
				Debug.LogWarning("[LuckySpin] Probability total is " + total + ", expected 100.");
			}
			else
			{
				total = ProbabilityPercentTotal;
			}

			double roll = UnityEngine.Random.value * total;
			MahjongProductAnalytics.Track("f2_lucky_spin_roll", "operation_id", analyticsSpinId, "roll", roll, "total_weight", total);
			double accumulated = 0d;
			for (int i = 0; i < ordered.Count; i++)
			{
				accumulated += Math.Max(ordered[i].Value != null ? ordered[i].Value.Probability : 0d, 0d);
				if (roll < accumulated)
				{
					return ordered[i].Key;
				}
			}
			return ordered[0].Key;
		}

		private bool TryGetGuaranteedLuckyWalletReward(out int target)
		{
			target = -1;
			if (spinData == null || spinData.Count == 0 ||
				HasClaimedLuckyWalletReward() || !CanUseLuckyWalletReward())
			{
				return false;
			}

			foreach (KeyValuePair<int, ConfLuckySpin> pair in spinData.OrderBy(pair => pair.Key))
			{
				if (pair.Value != null && pair.Value.Type == (int)ELuckySpinRewardType.LuckyWalletMoney)
				{
					target = pair.Key;
					return true;
				}
			}
			return false;
		}

		private IEnumerator PlaySpinSound()
		{
			for (int i = 0; i <= 6; i++)
			{
				yield return new WaitForSeconds(
					GameDefines.LS_rotateTime / 20f);
				PlaySoundEffect(AudioStCategory.ELuckySpin);
			}
			yield return new WaitForSeconds(
				GameDefines.LS_rotateTime - 1f);
			PlaySoundEffect(AudioStCategory.ELuckySpin2);
		}

		private IEnumerator DelayedAction(float delay, Action action)
		{
			yield return new WaitForSeconds(delay);
			action?.Invoke();
		}

		private Coroutine StartManagedCoroutine(IEnumerator routine)
		{
			return routine != null && isActiveAndEnabled ? Framework2Host.Runtime.RunSourceFlow(routine) : null;
		}

		private void PlayRewardEffect(
			ELuckySpinRewardType type,
			float count,
			Action finishAction = null,
			bool useExactCount = false)
		{
			ERewardType rewardType = (ERewardType)type;
			GameManagerWZ manager = GameManagerWZ.instance;
			if (type == ELuckySpinRewardType.Money)
			{
				if (!useExactCount)
				{
					count = manager != null ? manager.GetCoinRewardAmount(3) : count;
				}
				else if ((mOpenData == null || mOpenData.MoneyRewardTarget == LuckySpinMoneyRewardTarget.CurrentCoin) &&
					manager != null && !manager.CanGrantCoinRewardForCurrentLevel())
				{
					count = 0f;
				}
			}
			else if (type == ELuckySpinRewardType.Gem && manager != null)
			{
				count = manager.getResourceNum(2, 0, 2);
			}

			MahjongProductAnalytics.Track("f2_lucky_spin_award", "operation_id", analyticsSpinId, "reward_type", (int)type, "actual_requested_amount", count, "money_target", (int)GetMoneyRewardTarget(), "exact_amount", useExactCount, "eligible", count > 0f);
			if (count <= 0f)
			{
				finishAction?.Invoke();
				return;
			}

			if (type == ELuckySpinRewardType.Money &&
				GetMoneyRewardTarget() == LuckySpinMoneyRewardTarget.LuckyWallet)
			{
				PlayLuckyWalletRewardEffect(
					count,
					() => ApplyLuckyWalletMoneyReward(count, finishAction));
				return;
			}
			if (type == ELuckySpinRewardType.LuckyWalletMoney)
			{
				PlayLuckyWalletRewardEffect(
					count,
					() => ApplyConfiguredLuckyWalletReward(count, finishAction),
					FirstLuckyWalletRewardDisplayDelay);
				return;
			}

			if (FacadeEffectExtend.PlayGetRewardEffectHandle != null)
			{
				FacadeEffectExtend.PlayGetRewardEffectHandle(
					new[] { new ERewardItemStruct { Type = rewardType, Count = count } },
					finishAction);
				return;
			}

			ApplyRewardDirectly(rewardType, count);
			finishAction?.Invoke();
		}

		private void PlayLuckyWalletRewardEffect(
			float count,
			Action applyRewardAction,
			float delayBeforeOpen = 0f)
		{
			if (count <= 0f || FacadeEffectExtend.PlayGetRewardEffectHandle == null)
			{
				applyRewardAction?.Invoke();
				return;
			}

			Action showReward = () =>
				FacadeEffectExtend.PlayGetRewardEffectHandle(
					new[] { new ERewardItemStruct { Type = ERewardType.LuckyWalletMoney, Count = count } },
					applyRewardAction);
			if (delayBeforeOpen > 0f)
			{
				StartManagedCoroutine(DelayedAction(delayBeforeOpen, showReward));
			}
			else
			{
				showReward();
			}
		}

		private void ApplyRewardDirectly(ERewardType rewardType, float count)
		{
			GameManagerWZ manager = GameManagerWZ.instance;
			if (manager == null)
			{
				return;
			}

			switch (rewardType)
			{
				case ERewardType.Money:
					manager.AddCoin(count);
					break;
				case ERewardType.Gem:
					manager.AddGems(count);
					break;
				case ERewardType.AddBottle:
					manager.RewardHintBottle(Mathf.RoundToInt(count));
					break;
				case ERewardType.Clear:
					manager.RewardClear(Mathf.RoundToInt(count));
					break;
				case ERewardType.Undo:
					manager.RewardUndo(Mathf.RoundToInt(count));
					break;
				case ERewardType.LuckyWalletMoney:
					MarkLuckyWalletRewardClaimed();
					WithdrawApiService.AddWalletIncome(ResolveLuckyWalletRewardAmount(count), "LUCKY");
					break;
			}
		}

		private LuckySpinMoneyRewardTarget GetMoneyRewardTarget()
		{
			return mOpenData != null
				? mOpenData.MoneyRewardTarget
				: LuckySpinMoneyRewardTarget.CurrentCoin;
		}

		private void ApplyLuckyWalletMoneyReward(float fallbackAmount, Action finishAction)
		{
			float cachedAmount = WithdrawApiService.GetCachedLuckySpinRewardAmount(fallbackAmount);
			GameManagerWZ gameManager = GameManagerWZ.instance;
			if (gameManager == null)
			{
				WithdrawApiService.AddWalletIncome(cachedAmount);
				finishAction?.Invoke();
				return;
			}

			gameManager.StartCoroutine(WithdrawApiService.QueryLuckySpinRewardAmount(
				cachedAmount,
				amount =>
				{
					WithdrawApiService.AddWalletIncome(amount);
					finishAction?.Invoke();
				},
				error =>
				{
					Debug.LogWarning("[LuckySpin] Reward query failed: " + error);
				}));
		}

		private void ApplyConfiguredLuckyWalletReward(
			float fallbackAmount,
			Action finishAction)
		{
			MarkLuckyWalletRewardClaimed();
			WithdrawApiService.AddWalletIncome(
				ResolveLuckyWalletRewardAmount(fallbackAmount),
				"LUCKY");
			finishAction?.Invoke();
		}

		private float GetResolvedRewardCount(ConfLuckySpin conf)
		{
			if (conf == null)
			{
				return 0f;
			}
			return conf.Type == (int)ELuckySpinRewardType.LuckyWalletMoney
				? ResolveLuckyWalletRewardAmount(conf.Count)
				: conf.Count;
		}

		private float ResolveLuckyWalletRewardAmount(float fallbackAmount)
		{
			if (GameDefines.WheelReward > 0f)
			{
				return GameDefines.WheelReward;
			}
			return fallbackAmount > 0f ? fallbackAmount : DefaultLuckyWalletRewardAmount;
		}

		private bool CanUseLuckyWalletReward()
		{
			int currentLevel = GameManagerWZ.instance != null
				? GameManagerWZ.instance.currentLv
				: PlayerPrefs.GetInt(FacadePlayerPrefExtend.currentLevel, 1);
			return currentLevel >= GameDefines.LuckyWalletUnlockLevel;
		}

		private bool HasClaimedLuckyWalletReward()
		{
			return PlayerPrefs.GetInt(FacadePlayerPrefExtend.LuckySpinWalletRewardClaimed, 0) != 0;
		}

		private void MarkLuckyWalletRewardClaimed()
		{
			if (HasClaimedLuckyWalletReward())
			{
				return;
			}
			MahjongProductAnalytics.Track("f2_wallet_guarantee_consumed", "operation_id", analyticsSpinId, "before", false, "after", true);
			PlayerPrefs.SetInt(FacadePlayerPrefExtend.LuckySpinWalletRewardClaimed, 1);
			PlayerPrefs.Save();
		}

		private void CleanupResources()
		{
			spinData = null;
			angles = null;
			mOpenData = null;
		}

		private void OnDestroy()
		{
			InvalidateSpritePreload();
			if (mIsEventsBound)
			{
				RemoveButtonEvents();
			}
			CleanupResources();
			mIsInitialized = false;
		}

		private string GetLocalizedText(string key)
		{
			return GetLocalizedTextStatic(key);
		}

		private async Task<Sprite> GetSingleCoinSpriteAsync()
		{
			if (FacadePayTypeExtend.GetSingleCoin != null)
			{
				Sprite sprite = FacadePayTypeExtend.GetSingleCoin();
				if (sprite != null)
				{
					return sprite;
				}
			}
			return await AddressableAssetService.LoadAsync<Sprite>(
				"UI/Money/Single/coin_us");
		}

		private async Task<Sprite> GetSingleGemSpriteAsync()
		{
			if (FacadePayTypeExtend.GetSingleGem != null)
			{
				Sprite sprite = FacadePayTypeExtend.GetSingleGem();
				if (sprite != null)
				{
					return sprite;
				}
			}
			return await AddressableAssetService.LoadAsync<Sprite>("UI/Money/gem");
		}

		private void PlaySoundEffect(AudioStCategory category)
		{
			FacadeAudioExtend.PlayEffectHandle?.Invoke(category);
		}
	}
}
