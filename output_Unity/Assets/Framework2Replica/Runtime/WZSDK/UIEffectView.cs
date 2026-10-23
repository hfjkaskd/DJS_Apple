using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ArrowOut.Runtime;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	public class UIEffectView : BaseView
	{
		private sealed class RewardEffectRequest
		{
			public readonly ERewardItemStruct[] Items;

			public readonly Action FinishAction;

			public RewardEffectRequest(
				ERewardItemStruct[] items,
				Action finishAction)
			{
				Items = items != null
					? (ERewardItemStruct[])items.Clone()
					: Array.Empty<ERewardItemStruct>();
				FinishAction = finishAction;
			}
		}

		[CompilerGenerated]
		private sealed class _003CDelayedRefreshEffectDataFixed_003Ed__62 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public UIEffectView _003C_003E4__this;

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
			public _003CDelayedRefreshEffectDataFixed_003Ed__62(int _003C_003E1__state)
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
		private sealed class _003CGetRewardEffectCoroutine_003Ed__74 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public UIEffectView _003C_003E4__this;

			public Action finishAction;

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
			public _003CGetRewardEffectCoroutine_003Ed__74(int _003C_003E1__state)
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

		protected RectTransform mLTEMask;

		protected RectTransform mLTEPlane;

		protected RectTransform mLTEStartPoint;

		protected Button mDisappearEarlyBtn;

		protected RectTransform mLevelTargetEffect;

		protected Text mNextTargetText;

		protected RectTransform mMiniDialog;

		protected Text mMiniTargetText;

		protected Text mDialogText;

		protected RectTransform mGREMask;

		protected RectTransform mGetRewardEffect;

		protected RectTransform mRewardGroup;

		protected EffectRewardItem mEffectRewardItem;

		protected RectTransform mCongratulationEffect;

		protected Text mCEContent;

		protected ParticleSystem mFireworkEffectL;

		protected ParticleSystem mFireworkEffectR;

		protected RectTransform mFlyEffectParent;

		protected RectTransform mFlyProp;

		protected RectTransform mFlyMoney;

		protected RectTransform mFlyIAAMoney;

		protected RectTransform mFlyMoneyTip;

		protected RectTransform mFlyIAAMoneyTip;

		protected SkeletonGraphic mShaobaEffect;

		protected SkeletonGraphic mHammerEffect;

		protected RectTransform mClickEffectParent;

		protected SkeletonGraphic mClickEffect;

		protected SkeletonGraphic mDifficultyUpEffect;

		protected Text mDUEText;

		protected Text mNextTargetSubtitle;

		protected RectTransform mFlyGem;

		protected RectTransform mFlyGemTip;

		private Coroutine getRewardEffectCoroutine;

		private GameManagerWZ getRewardEffectCoroutineOwner;

		private Action getRewardEffectFinishAction;

		private bool isCompletingRewardEffect;

		private bool isProcessingRewardEffectRequests;

		private bool isCleaningUpRewardEffects;

		private Queue<EffectRewardItem> ERItemPool;

		private List<EffectRewardItem> curERItem;

		private Queue<RewardEffectRequest> pendingRewardEffectRequests;

		private Transform LTEGoldTrans;

		private float CongratulationStartY;

		private float CongratulationEndY;

		private float CongratulationStartAnchoredY;

		private float congratulationProgress;

		private Sequence congratulationSequence;

		private char[] nameChars;

		private Stack<GameObject> flyMoneyPool;

		private Stack<GameObject> flyMoneyTipPool;

		private Stack<GameObject> flyGemPool;

		private Stack<GameObject> flyGemTipPool;

		private Stack<GameObject> flyPropPool;

		private Stack<SkeletonGraphic> clickPool;

		private Dictionary<ERewardType, Vector3> flyObjGoldDic;

		private Sequence targetEffectSequence;

		private Action targetEffectFinishAction;

		private bool mIsEventsBound;

		private RectTransform streamer;

		public Transform startPoint;

		private Tween moneyImpactTween;

		private bool isMoneyImpactPlaying;

		private Queue<Action> pendingMoneyImpacts;

		public void Awake()
		{
			FindUIComponents();
			InitializeObjectPools();
			InitializeEffectState();
		}

		public override void InitView()
		{
			RefreshEffectData();
			BindButtonEvent();
		}

		public override void Start()
		{
		}

		public override void Update()
		{
		}

		private void FindUIComponents()
		{
			mLTEMask = FindRect("LTEMask");
			mLTEPlane = FindRect("LTEPlane");
			mLTEStartPoint = FindRect("LTEStartPoint");
			mDisappearEarlyBtn = FindComponent<Button>("DisappearEarlyBtn");
			mLevelTargetEffect = FindRect("LevelTargetEffect");
			mNextTargetText = FindComponent<Text>("NextTargetText");
			mMiniDialog = FindRect("MiniDialog");
			mMiniTargetText = FindComponent<Text>("MiniTargetText");
			mDialogText = FindComponent<Text>("DialogText");
			mGREMask = FindRect("GREMask");
			mGetRewardEffect = FindRect("GetRewardEffect");
			mRewardGroup = FindRect("RewardGroup");
			mEffectRewardItem =
				FindComponent<EffectRewardItem>("EffectRewardItem");
			mCongratulationEffect = FindRect("CongratulationEffect");
			mCEContent = FindComponent<Text>("CEContent");
			mFireworkEffectL =
				FindComponent<ParticleSystem>("FireworkEffectL");
			mFireworkEffectR =
				FindComponent<ParticleSystem>("FireworkEffectR");
			mFlyEffectParent = FindRect("FlyEffectParent");
			mFlyProp = FindRect("FlyProp");
			mFlyMoney = FindRect("FlyMoney");
			mFlyIAAMoney = FindRect("FlyIAAMoney");
			mFlyMoneyTip = FindRect("FlyMoneyTip");
			mFlyIAAMoneyTip = FindRect("FlyIAAMoneyTip");
			mShaobaEffect =
				FindComponent<SkeletonGraphic>("ShaobaEffect");
			mHammerEffect =
				FindComponent<SkeletonGraphic>("HammerEffect");
			mClickEffectParent = FindRect("ClickEffectParent");
			mClickEffect =
				FindComponent<SkeletonGraphic>("ClickEffect");
			mDifficultyUpEffect =
				FindComponent<SkeletonGraphic>("DifficultyUpEffect");
			mDUEText = FindComponent<Text>("DUEText");
			mNextTargetSubtitle =
				FindComponent<Text>("NextTargetSubtitle");
			mFlyGem = FindRect("FlyGem");
			mFlyGemTip = FindRect("FlyGemTip");
			startPoint = FindComponent<Transform>("FlyEffectParent");
			streamer = FindRect("streamer");
		}

		private void InitializeObjectPools()
		{
			ERItemPool = new Queue<EffectRewardItem>();
			curERItem = new List<EffectRewardItem>();
			pendingRewardEffectRequests =
				new Queue<RewardEffectRequest>();
			flyMoneyPool = new Stack<GameObject>();
			flyMoneyTipPool = new Stack<GameObject>();
			flyGemPool = new Stack<GameObject>();
			flyGemTipPool = new Stack<GameObject>();
			flyPropPool = new Stack<GameObject>();
			clickPool = new Stack<SkeletonGraphic>();
			pendingMoneyImpacts = new Queue<Action>();
		}

		private void InitializeEffectState()
		{
			if (mCongratulationEffect != null)
			{
				CongratulationStartAnchoredY =
					mCongratulationEffect.anchoredPosition.y;
				congratulationProgress = 0f;
				RefreshCanvasSizedLayout();
			}
			SetActive(mEffectRewardItem, false);
			SetActive(mLevelTargetEffect, false);
			SetActive(mGetRewardEffect, false);
			SetActive(mFlyProp, false);
			SetActive(mFlyMoney, false);
			SetActive(mFlyIAAMoney, false);
			SetActive(mFlyMoneyTip, false);
			SetActive(mFlyIAAMoneyTip, false);
			SetActive(mFlyGem, false);
			SetActive(mFlyGemTip, false);
			SetActive(mShaobaEffect, false);
			SetActive(mHammerEffect, false);
			SetActive(mClickEffect, false);
			SetActive(mDifficultyUpEffect, false);

			if (mFlyMoney != null && mFlyMoneyTip != null &&
				FacadePayTypeExtend.GetSingleCoin != null)
			{
				Image flyMoneyImage = mFlyMoney.GetComponent<Image>();
				if (flyMoneyImage != null)
				{
					flyMoneyImage.sprite = FacadePayTypeExtend.GetSingleCoin();
				}

				Transform tipText = mFlyMoneyTip.childCount > 0
					? mFlyMoneyTip.GetChild(0)
					: null;
				Transform tipIcon = tipText != null && tipText.childCount > 0
					? tipText.GetChild(0)
					: null;
				Image flyMoneyTipImage = tipIcon != null
					? tipIcon.GetComponent<Image>()
					: null;
				if (flyMoneyTipImage != null)
				{
					flyMoneyTipImage.sprite = FacadePayTypeExtend.GetSingleCoin();
				}
			}
		}

		internal void RefreshCanvasSizedLayout()
		{
			if (mCongratulationEffect == null)
			{
				return;
			}

			float animationOffset =
				mCongratulationEffect.anchoredPosition.y -
				CongratulationStartAnchoredY;
			CongratulationStartY =
				mCongratulationEffect.localPosition.y - animationOffset;
			CongratulationEndY = CongratulationStartY - 650f;
			ApplyCongratulationProgress();
		}

		private void SetCongratulationProgress(float progress)
		{
			congratulationProgress = Mathf.Clamp01(progress);
			ApplyCongratulationProgress();
		}

		private void ApplyCongratulationProgress()
		{
			if (mCongratulationEffect == null)
			{
				return;
			}

			Vector3 position = mCongratulationEffect.localPosition;
			position.y = Mathf.Lerp(
				CongratulationStartY,
				CongratulationEndY,
				congratulationProgress);
			mCongratulationEffect.localPosition = position;
		}

		internal override void RefreshRuntimeGmPresentation()
		{
			RefreshEffectData();
		}

		public void RefreshEffectData()
		{
			StartCoroutine(DelayedRefreshEffectDataFixed());
		}

		private void RefreshEffectDataNow()
		{
			flyObjGoldDic =
				FacadeGamePlayExtend.GetFlyObjGoldHandle != null
					? FacadeGamePlayExtend.GetFlyObjGoldHandle()
					: null;
		}

		[IteratorStateMachine(typeof(_003CDelayedRefreshEffectDataFixed_003Ed__62))]
		private IEnumerator DelayedRefreshEffectDataFixed()
		{
			for (int i = 0; i < 6; i++)
			{
				yield return null;
			}
			RefreshEffectDataNow();
		}

		protected void BindButtonEvent()
		{
			if (mIsEventsBound)
			{
				RemoveButtonEvents();
			}
			if (mDisappearEarlyBtn != null)
			{
				mDisappearEarlyBtn.onClick.AddListener(
					OnDisappearEarlyBtnClickHandle);
			}
			RegisterEventListeners();
			mIsEventsBound = true;
		}

		protected void RemoveButtonEvents()
		{
			if (mDisappearEarlyBtn != null)
			{
				mDisappearEarlyBtn.onClick.RemoveAllListeners();
			}
			UnregisterEventListeners();
			mIsEventsBound = false;
		}

		private void RegisterEventListeners()
		{
			FacadeEffectExtend.PlayGetRewardEffectHandle +=
				PlayGetRewardEffect;
			FacadeEffectExtend.PlayCongratulationEffectHandle +=
				PlayCongratulationEffect;
			FacadeEffectExtend.PlayFlyMoneyHandle += PlayFlyMoney;
			FacadeEffectExtend.PlayFlyMoneyTipHandle += PlayFlyMoneyTip;
		}

		private void UnregisterEventListeners()
		{
			FacadeEffectExtend.PlayGetRewardEffectHandle -=
				PlayGetRewardEffect;
			FacadeEffectExtend.PlayCongratulationEffectHandle -=
				PlayCongratulationEffect;
			FacadeEffectExtend.PlayFlyMoneyHandle -= PlayFlyMoney;
			FacadeEffectExtend.PlayFlyMoneyTipHandle -= PlayFlyMoneyTip;
		}

		private void PlayLevelTargetEffect(Action finishAction)
		{
			targetEffectFinishAction = finishAction;
			if (mLevelTargetEffect == null ||
				mLTEPlane == null ||
				mLTEStartPoint == null)
			{
				targetEffectFinishAction?.Invoke();
				return;
			}

			SetActive(mLevelTargetEffect, true);
			mLTEPlane.localScale = Vector3.one;
			SetLevelTargetText();
			mLTEPlane.position = mLTEStartPoint.position;

			Transform goldTarget = LTEGoldTrans;
			if (goldTarget == null)
			{
				goldTarget = ResolveCurMoneyRect();
				LTEGoldTrans = goldTarget;
			}

			targetEffectSequence = DOTween.Sequence();
			targetEffectSequence.Append(
				mLTEPlane.DOLocalMoveY(0f, GameDefines.LTE_MoveTime));
			targetEffectSequence.AppendInterval(GameDefines.LTE_StayTime);
			if (goldTarget != null)
			{
				targetEffectSequence.Append(
					mLTEPlane.DOMove(
						goldTarget.position,
						GameDefines.LTE_GoAwayTime));
			}
			else
			{
				targetEffectSequence.AppendInterval(
					GameDefines.LTE_GoAwayTime);
			}
			targetEffectSequence.Join(
				mLTEPlane.DOScale(
					Vector3.zero,
					GameDefines.LTE_GoAwayTime));
			targetEffectSequence.OnComplete(CompleteLevelTargetEffect);
		}

		private void OnDisappearEarlyBtnClickHandle()
		{
			targetEffectSequence?.Kill();
			CompleteLevelTargetEffect();
		}

		private void SetLevelTargetText()
		{
			GameManagerWZ gameManager = GameManagerWZ.instance;
			if (gameManager == null)
			{
				return;
			}

			int currentLevel = gameManager.currentLv;
			if (mMiniTargetText != null)
			{
				mMiniTargetText.text = GetLevelDisplayText(currentLevel);
			}

			bool isInSmallLevelGroup =
				IsInSmallLevelGroup(currentLevel);
			bool usesCurrentLevelTarget =
				currentLevel >= GameDefines.SmallLevelGroupStart &&
				currentLevel <= GameDefines.SmallLevelGroupEnd;
			SetActive(mMiniDialog, isInSmallLevelGroup);

			if (mNextTargetText != null)
			{
				if (currentLevel == 2 || usesCurrentLevelTarget)
				{
					mNextTargetText.text = string.Format(
						GetFacadeText("10022", "Pass this Level"),
						GameDefines.SmallLevelGroupStart);
				}
				else if (currentLevel > 2 &&
					currentLevel < GameDefines.SmallLevelGroupStart)
				{
					mNextTargetText.text = string.Format(
						GetFacadeText("10021", "Pass Level {0}"),
						GameDefines.SmallLevelGroupStart);
				}
			}

			if (mDialogText != null)
			{
				int arriveTime = (int)UnityEngine.Random.Range(
					GameDefines.TargetArriveTime.x,
					GameDefines.TargetArriveTime.y + 1f);
				mDialogText.text = string.Format(
					GetFacadeText(
						"10020",
						"Expected to reach within <color=#FFE600>{0} minutes</color>"),
					arriveTime);
			}
		}

		private string GetLevelDisplayText(int actualLevel)
		{
			int groupStart = GameDefines.SmallLevelGroupStart;
			int groupEnd = GameDefines.SmallLevelGroupEnd;
			if (actualLevel <= groupStart)
			{
				return string.Format(
					GetFacadeText("10010", "Level {0}"),
					actualLevel);
			}
			if (IsInSmallLevelGroup(actualLevel))
			{
				return string.Format(
					GetFacadeText("1033", "Level {0} ({1}/{2})"),
					groupStart,
					actualLevel - groupStart + 1,
					groupEnd - groupStart + 1);
			}
			return string.Format(
				GetFacadeText("10010", "Level {0}"),
				groupStart + actualLevel - groupEnd);
		}

		private bool IsInSmallLevelGroup(int actualLevel)
		{
			return actualLevel > GameDefines.SmallLevelGroupStart &&
				actualLevel <= GameDefines.SmallLevelGroupEnd;
		}

		private string GetFacadeText(string key, string fallback)
		{
			if (FacadeLanguageExtend.GetTextHandle != null)
			{
				string text = FacadeLanguageExtend.GetTextHandle(key);
				if (!string.IsNullOrEmpty(text) && text != key)
				{
					return text;
				}
			}
			LocalizationManager localization = LocalizationManager.Instance;
			if (localization != null && localization.HasText(key))
			{
				return localization.GetText(key);
			}
			return fallback;
		}

		private void CompleteLevelTargetEffect()
		{
			PlayCongratulationEffect();
			SetActive(mLevelTargetEffect, false);
			targetEffectFinishAction?.Invoke();
		}

		private void PlayGetRewardEffect(ERewardItemStruct[] items, Action finishAction)
		{
			if (items == null || items.Length == 0)
			{
				InvokeRewardFinishAction(finishAction);
				return;
			}
			if (pendingRewardEffectRequests == null)
			{
				pendingRewardEffectRequests =
					new Queue<RewardEffectRequest>();
			}

			pendingRewardEffectRequests.Enqueue(
				new RewardEffectRequest(items, finishAction));
			ProcessQueuedRewardEffects();
		}

		private void ProcessQueuedRewardEffects()
		{
			if (isProcessingRewardEffectRequests ||
				isCompletingRewardEffect ||
				pendingRewardEffectRequests == null ||
				pendingRewardEffectRequests.Count == 0)
			{
				return;
			}

			isProcessingRewardEffectRequests = true;
			try
			{
				while (pendingRewardEffectRequests.Count > 0)
				{
					if (HasPendingRewardEffect())
					{
						CompleteGetRewardEffect(true);
					}

					RewardEffectRequest request =
						pendingRewardEffectRequests.Dequeue();
					StartRewardEffect(request);
					if (HasPendingRewardEffect())
					{
						if (isCleaningUpRewardEffects)
						{
							CompleteGetRewardEffect(true);
							continue;
						}
						break;
					}
				}
			}
			finally
			{
				isProcessingRewardEffectRequests = false;
			}
		}

		private void StartRewardEffect(RewardEffectRequest request)
		{
			if (request == null || request.Items.Length == 0)
			{
				InvokeRewardFinishAction(request?.FinishAction);
				return;
			}

			SetActive(mGetRewardEffect, true);
			SetActive(mGREMask, true);
			curERItem.Clear();
			getRewardEffectFinishAction = request.FinishAction;

			List<ERewardItemStruct> orderedItems = request.Items
				.Select((item, index) => new { item, index })
				.OrderBy(entry => GetRewardTypeOrder(entry.item.Type))
				.ThenBy(entry => entry.index)
				.Select(entry => entry.item)
				.ToList();
			Dictionary<ERewardType, Queue<EffectRewardItem>> typedPool =
				new Dictionary<ERewardType, Queue<EffectRewardItem>>();
			while (ERItemPool.Count > 0)
			{
				EffectRewardItem pooledItem = ERItemPool.Dequeue();
				if (!typedPool.ContainsKey(pooledItem.ErType))
				{
					typedPool[pooledItem.ErType] =
						new Queue<EffectRewardItem>();
				}
				typedPool[pooledItem.ErType].Enqueue(pooledItem);
			}

			foreach (ERewardItemStruct item in orderedItems)
			{
				EffectRewardItem rewardItem;
				if (typedPool.ContainsKey(item.Type) &&
					typedPool[item.Type].Count > 0)
				{
					rewardItem = typedPool[item.Type].Dequeue();
				}
				else
				{
					rewardItem = Instantiate(
						mEffectRewardItem.gameObject,
						mRewardGroup).GetComponent<EffectRewardItem>();
				}
				rewardItem.gameObject.SetActive(true);
				rewardItem.Show(item.Type, item.Count);
				curERItem.Add(rewardItem);
			}
			foreach (Queue<EffectRewardItem> remainingItems in
				typedPool.Values)
			{
				while (remainingItems.Count > 0)
				{
					ERItemPool.Enqueue(remainingItems.Dequeue());
				}
			}

			getRewardEffectCoroutineOwner = GameManagerWZ.instance;
			if (getRewardEffectCoroutineOwner == null ||
				isCleaningUpRewardEffects)
			{
				CompleteGetRewardEffect(false);
				return;
			}
			getRewardEffectCoroutine =
				getRewardEffectCoroutineOwner.StartCoroutine(
					GetRewardEffectCoroutine());
		}

		private int GetRewardTypeOrder(ERewardType type)
		{
			switch (type)
			{
				case ERewardType.Money:
					return 0;
				case ERewardType.Gem:
					return 1;
				case ERewardType.AddBottle:
					return 2;
				case ERewardType.Clear:
					return 3;
				case ERewardType.Undo:
					return 4;
				case ERewardType.Prompt:
					return 5;
				case ERewardType.Guid:
					return 6;
				case ERewardType.LuckyWalletMoney:
					return 7;
				default:
					return 999;
			}
		}

		[IteratorStateMachine(typeof(_003CGetRewardEffectCoroutine_003Ed__74))]
		private IEnumerator GetRewardEffectCoroutine()
		{
			yield return new WaitForSecondsRealtime(GameDefines.GRE_StayTime);
			CompleteGetRewardEffect(false);
		}

		public void CompletePendingRewardEffect()
		{
			CompleteGetRewardEffect(true);
		}

		private void CompleteGetRewardEffect(bool stopCoroutine)
		{
			if (!HasPendingRewardEffect() || isCompletingRewardEffect)
			{
				return;
			}

			isCompletingRewardEffect = true;
			Coroutine activeCoroutine = getRewardEffectCoroutine;
			GameManagerWZ coroutineOwner = getRewardEffectCoroutineOwner;
			Action finishAction = getRewardEffectFinishAction;
			getRewardEffectCoroutine = null;
			getRewardEffectCoroutineOwner = null;
			getRewardEffectFinishAction = null;
			if (stopCoroutine && activeCoroutine != null && coroutineOwner != null)
			{
				coroutineOwner.StopCoroutine(activeCoroutine);
			}

			try
			{
				SetActive(mGetRewardEffect, false);
				SetActive(mGREMask, false);
				EffectRewardItem[] completedItems = curERItem != null
					? curERItem.ToArray()
					: Array.Empty<EffectRewardItem>();
				curERItem?.Clear();
				for (int i = 0; i < completedItems.Length; i++)
				{
					EffectRewardItem rewardItem = completedItems[i];
					if (rewardItem == null)
					{
						continue;
					}
					try
					{
						ERItemPool?.Enqueue(rewardItem);
						rewardItem.gameObject.SetActive(false);
						GetRewardEffectAfterFly(rewardItem);
					}
					catch (Exception exception)
					{
						UnityEngine.Debug.LogError(
							"Unable to settle reward effect item: " +
							exception);
					}
				}
			}
			finally
			{
				isCompletingRewardEffect = false;
			}

			InvokeRewardFinishAction(finishAction);
			ProcessQueuedRewardEffects();
		}

		internal bool HasPendingRewardEffect()
		{
			return getRewardEffectCoroutine != null ||
				getRewardEffectFinishAction != null ||
				(curERItem != null && curERItem.Count > 0);
		}

		private static void InvokeRewardFinishAction(Action finishAction)
		{
			try
			{
				finishAction?.Invoke();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogError(
					"Reward effect completion callback failed: " + exception);
			}
		}

		private void GetRewardEffectAfterFly(EffectRewardItem ERItem)
		{
			if (ERItem == null)
			{
				return;
			}
			GameManagerWZ gm = GameManagerWZ.instance;
			switch (ERItem.ErType)
			{
				case ERewardType.Money:
					if (gm != null)
					{
						gm.AddCoin(ERItem.Count);
					}
					PlayFlyMoney(
						ERItem.transform,
						GameDefines.GRE_FlyMoneyCount,
						ERItem.Count,
						null,
						ERewardType.Money);
					break;
				case ERewardType.Gem:
					if (gm != null)
					{
						gm.AddGems(ERItem.Count);
					}
					PlayFlyMoney(
						ERItem.transform,
						GameDefines.GRE_FlyMoneyCount,
						ERItem.Count,
						null,
						ERewardType.Gem);
					break;
				case ERewardType.AddBottle:
					PlayFlyProp(
						ERItem.transform,
						ERewardType.AddBottle,
						null);
					break;
				case ERewardType.Clear:
					PlayFlyProp(
						ERItem.transform,
						ERewardType.Clear,
						null);
					break;
				case ERewardType.Undo:
					PlayFlyProp(
						ERItem.transform,
						ERewardType.Undo,
						null);
					break;
				case ERewardType.Prompt:
					Framework2Host.Runtime?.GrantImportedHint();
					PlayFlyProp(
						ERItem.transform,
						ERewardType.Prompt,
						null);
					break;
				case ERewardType.Guid:
					Framework2Host.Runtime?.GrantImportedHint();
					PlayFlyProp(
						ERItem.transform,
						ERewardType.Guid,
						null);
					break;
			}
		}

		private void PlayCongratulationEffect()
		{
			if (GameDefines.ifIAA ||
				mCongratulationEffect == null ||
				mCEContent == null)
			{
				return;
			}

			(string playerName, int stage) =
				UserDataManager.Instance.GetRandomBarragePlayAndStage();
			int attempt = GetRandomAttempt();
			LocalizationManager localization = LocalizationManager.Instance;
			string template = localization != null
				? localization.GetText("1030")
				: "{0} ({1}) {2}";
			string moneyText =
				FacadePayTypeExtend.RegionalChangeHandle != null
					? FacadePayTypeExtend.RegionalChangeHandle(stage)
					: stage.ToString();
			mCEContent.text = FormatCongratulationContent(
				string.Format(
					template, playerName, attempt, moneyText));

			mFireworkEffectL?.Stop();
			mFireworkEffectR?.Stop();
			SetActive(streamer, true);
			ApplyCongratulationProgress();

			Sequence sequence = DOTween.Sequence();
			congratulationSequence = sequence;
			sequence.SetTarget(mCongratulationEffect);
			sequence.Append(
				DOTween.To(
					() => congratulationProgress,
					SetCongratulationProgress,
					1f,
					GameDefines.CE_MoveTime)
					.SetTarget(mCongratulationEffect));
			sequence.AppendInterval(GameDefines.CE_StayTime);
			sequence.AppendCallback(() =>
			{
				SetCongratulationProgress(1f);
				SetActive(streamer, false);
			});
			sequence.Append(
				DOTween.To(
					() => congratulationProgress,
					SetCongratulationProgress,
					0f,
					GameDefines.CE_MoveTime)
					.SetTarget(mCongratulationEffect));
			sequence.OnComplete(() =>
			{
				if (congratulationSequence == sequence)
				{
					congratulationSequence = null;
				}

				SetCongratulationProgress(0f);
			});
		}

		private void KillCongratulationTweens()
		{
			congratulationSequence?.Kill(false);
			congratulationSequence = null;
		}

		private static string FormatCongratulationContent(string content)
		{
			if (string.IsNullOrEmpty(content) || content.IndexOf('\n') >= 0)
			{
				return content;
			}

			int commaIndex = content.LastIndexOf(", ", StringComparison.Ordinal);
			if (commaIndex >= 0)
			{
				return content.Substring(0, commaIndex + 1) + "\n" +
					content.Substring(commaIndex + 2);
			}

			int fullWidthCommaIndex = content.LastIndexOf('，');
			return fullWidthCommaIndex >= 0
				? content.Insert(fullWidthCommaIndex + 1, "\n")
				: content;
		}

		private string GetRandomPlayerName()
		{
			if (string.IsNullOrEmpty(GameDefines.nameString))
			{
				return string.Empty;
			}
			nameChars = GameDefines.nameString.ToCharArray();
			char first = nameChars[UnityEngine.Random.Range(0, nameChars.Length)];
			char second = nameChars[UnityEngine.Random.Range(0, nameChars.Length)];
			LocalizationManager localization = LocalizationManager.Instance;
			string prefix = localization != null
				? localization.GetText("1005")
				: string.Empty;
			return string.Format("{0}_{1}{2}", prefix, first, second);
		}

		private int GetRandomAttempt()
		{
			return (int)UnityEngine.Random.Range(
				GameDefines.CE_Content_attemptTimes.x,
				GameDefines.CE_Content_attemptTimes.y + 1f);
		}

		internal void PlayHostEliminationMoney(Vector2 screenPosition, int count, float money)
		{
			RectTransform plane = mFlyEffectParent as RectTransform;
			if (plane != null && RectTransformUtility.ScreenPointToWorldPointInRectangle(plane, screenPosition, null, out Vector3 worldPoint))
				PlayFlyMoneyCore(null, count, money, null, ERewardType.Money, worldPoint);
			else
				PlayFlyMoney(null, count, money, null, ERewardType.Money);
		}

		private void PlayFlyMoney(Transform startPoint, int count, float money, Action successAction, ERewardType targetType = ERewardType.Money)
		{
			PlayFlyMoneyCore(startPoint, count, money, successAction, targetType, null);
		}

		private void PlayFlyMoneyCore(Transform startPoint, int count, float money, Action successAction, ERewardType targetType, Vector3? worldStart)
		{
			if (flyObjGoldDic == null)
			{
				RefreshEffectDataNow();
			}

			Transform actualStart = startPoint != null ? startPoint : this.startPoint;
			bool impactPlayed = false;
			bool flightSoundPlayed = false;
			var balanceFlight = targetType == ERewardType.Money
				? UIManager.instance?.GetView<GamePlayerView>(EUIType.GamePlayerView)?.HoldHostCashFlight(money)
				: null;
			int startedFlights = 0;
			for (int i = 0; i < count; i++)
			{
				Stack<GameObject> pool = targetType == ERewardType.Gem && mFlyGem != null
					? flyGemPool
					: flyMoneyPool;
				RectTransform template = targetType == ERewardType.Gem && mFlyGem != null
					? mFlyGem
					: (GameDefines.ifIAA ? mFlyIAAMoney : mFlyMoney);
				if (pool == null || template == null || mFlyEffectParent == null)
				{
					continue;
				}

				GameObject flyObject = pool.Count > 0
					? pool.Pop()
					: Instantiate(template.gameObject, mFlyEffectParent);
				flyObject.transform.SetParent(mFlyEffectParent, false);
				flyObject.SetActive(true);

				Image image = flyObject.GetComponent<Image>();
				Sprite currencySprite = GetCurrencySprite(targetType);
				if (image != null && currencySprite != null)
				{
					image.sprite = currencySprite;
				}

				if (worldStart.HasValue)
				{
					flyObject.transform.position = worldStart.Value;
				}
				else if (actualStart != null)
				{
					flyObject.transform.position = actualStart.position;
				}
				RectTransform flyRect = flyObject.transform as RectTransform;
				if (flyRect != null)
				{
					flyRect.localPosition += new Vector3(
						UnityEngine.Random.Range(
							0f, GameDefines.FlyMoney_RandomSpawnDist),
						UnityEngine.Random.Range(
							0f, GameDefines.FlyMoney_RandomSpawnDist),
						0f);
				}

				if (flyObjGoldDic == null ||
					!flyObjGoldDic.TryGetValue(targetType, out Vector3 target))
				{
					pool.Push(flyObject);
					flyObject.SetActive(false);
					continue;
				}

				GameObject completedObject = flyObject;
				Stack<GameObject> completedPool = pool;
				startedFlights++;
				flyObject.transform.DOMove(target, GameDefines.FlyMoney_MoveTime)
					.SetDelay(
						GameDefines.FlyMoney_DelayTime +
						GameDefines.FlyMoney_IntervalTime * i)
					.OnStart(() =>
					{
						if (!flightSoundPlayed && targetType == ERewardType.Money)
						{
							MahjongSlide.Withdrawal.MahjongRewardAudio.FlightStarted();
							flightSoundPlayed = true;
						}
					})
					.OnComplete(
						() =>
						{
							balanceFlight?.Release();
							successAction?.Invoke();
							completedPool.Push(completedObject);
							completedObject.SetActive(false);
							if (!impactPlayed &&
								targetType == ERewardType.Money)
							{
								PlayCurMoneyImpact();
								MahjongSlide.Withdrawal.MahjongRewardAudio.CashArrived();
								impactPlayed = true;
							}
						})
					.OnKill(() => balanceFlight?.Release());
			}
			if (startedFlights == 0) balanceFlight?.Release();

			STimerManager.Instance.CreateSDelay(
				GameDefines.FlyMoneyTip_DelayTime,
				() => PlayFlyMoneyTip(money, targetType));
		}

		private void PlayCurMoneyImpact()
		{
			RectTransform curMoneyRect = ResolveCurMoneyRect();
			if (curMoneyRect == null ||
				!curMoneyRect.gameObject.activeInHierarchy)
			{
				return;
			}
			if (!isMoneyImpactPlaying)
			{
				ExecuteMoneyImpact(curMoneyRect);
				return;
			}
			pendingMoneyImpacts.Enqueue(
				() => ExecuteMoneyImpact(curMoneyRect));
		}

		private void ExecuteMoneyImpact(RectTransform curMoneyRect)
		{
			if (curMoneyRect == null)
			{
				return;
			}
			isMoneyImpactPlaying = true;
			Vector3 originalScale = curMoneyRect.localScale;
			if (moneyImpactTween != null && moneyImpactTween.IsActive())
			{
				moneyImpactTween.Kill();
			}
			Sequence sequence = DOTween.Sequence();
			sequence.Append(
				curMoneyRect.DOScale(originalScale * 1.22f, 0.045f)
					.SetEase(Ease.OutQuad));
			sequence.Append(
				curMoneyRect.DOScale(originalScale * 0.9f, 0.045f)
					.SetEase(Ease.InQuad));
			sequence.Append(
				curMoneyRect.DOScale(originalScale * 1.16f, 0.05f)
					.SetEase(Ease.OutQuad));
			sequence.Append(
				curMoneyRect.DOScale(originalScale * 0.94f, 0.05f)
					.SetEase(Ease.InQuad));
			sequence.Append(
				curMoneyRect.DOScale(originalScale * 1.1f, 0.055f)
					.SetEase(Ease.OutQuad));
			sequence.Append(
				curMoneyRect.DOScale(originalScale * 0.97f, 0.055f)
					.SetEase(Ease.InQuad));
			sequence.Append(
				curMoneyRect.DOScale(originalScale * 1.04f, 0.06f)
					.SetEase(Ease.OutQuad));
			sequence.Append(
				curMoneyRect.DOScale(originalScale, 0.08f)
					.SetEase(Ease.OutSine));
			sequence.SetUpdate(true);
			sequence.OnComplete(
				() =>
				{
					curMoneyRect.localScale = originalScale;
					moneyImpactTween = null;
					isMoneyImpactPlaying = false;
					if (pendingMoneyImpacts != null &&
						pendingMoneyImpacts.Count > 0)
					{
						pendingMoneyImpacts.Dequeue()?.Invoke();
					}
				});
			moneyImpactTween = sequence;
		}

		private RectTransform ResolveCurMoneyRect()
		{
			RectTransform direct =
				FacadeGamePlayExtend.GetGamePlayMoneyRectHandle != null
					? FacadeGamePlayExtend.GetGamePlayMoneyRectHandle()
					: null;
			if (direct != null) return direct;
			RectTransform named = FindNamedParentRect(direct, "CurMoney");
			if (named != null)
			{
				return named;
			}

			GamePlayerView playerView = FindObjectOfType<GamePlayerView>();
			if (playerView != null)
			{
				GameObject child =
					FindChildUtility.FindChild(playerView.transform, "CurMoney");
				if (child != null)
				{
					return child.GetComponent<RectTransform>();
				}
			}

			GameView gameView = FindObjectOfType<GameView>();
			if (gameView != null)
			{
				GameObject child =
					FindChildUtility.FindChild(gameView.transform, "CurMoney");
				if (child != null)
				{
					return child.GetComponent<RectTransform>();
				}
			}
			return null;
		}

		private RectTransform FindNamedParentRect(Transform start, string targetName)
		{
			Transform current = start;
			while (current != null)
			{
				if (current.name == targetName)
				{
					return current as RectTransform;
				}
				current = current.parent;
			}
			return null;
		}

		private void PlayFlyMoneyTip(float money, ERewardType targetType = ERewardType.Money)
		{
			if (flyObjGoldDic == null)
			{
				RefreshEffectData();
			}
			Stack<GameObject> pool = targetType == ERewardType.Gem && mFlyGemTip != null
				? flyGemTipPool
				: flyMoneyTipPool;
			RectTransform template = targetType == ERewardType.Gem && mFlyGemTip != null
				? mFlyGemTip
				: (GameDefines.ifIAA ? mFlyIAAMoneyTip : mFlyMoneyTip);
			if (pool == null || template == null || mFlyEffectParent == null)
			{
				return;
			}

			GameObject tipObject = pool.Count > 0
				? pool.Pop()
				: Instantiate(template.gameObject, mFlyEffectParent);
			tipObject.transform.SetParent(mFlyEffectParent, false);
			tipObject.SetActive(true);
			Image image = tipObject.GetComponentInChildren<Image>(true);
			Sprite sprite = GetCurrencySprite(targetType);
			if (image != null && sprite != null)
			{
				image.sprite = sprite;
			}
			Text text = tipObject.GetComponentInChildren<Text>(true);
			if (text != null)
			{
				bool isMoney = targetType == ERewardType.Money ||
					targetType == ERewardType.LuckyWalletMoney;
				GameManagerWZ gm = GameManagerWZ.instance;
				if (isMoney && gm != null)
				{
					text.text = gm.FormatRegionalValue((double)money);
				}
				else if (isMoney &&
					FacadePayTypeExtend.RegionalChangeHandle != null)
				{
					text.text =
						FacadePayTypeExtend.RegionalChangeHandle(money);
				}
				else
				{
					text.text = money.ToString();
				}
			}

			if (flyObjGoldDic == null ||
				!flyObjGoldDic.TryGetValue(targetType, out Vector3 target))
			{
				pool.Push(tipObject);
				tipObject.SetActive(false);
				return;
			}
			tipObject.transform.position = target;
			float targetLocalY = tipObject.transform.localPosition.y +
				GameDefines.FlyMoneyTip_MoveDist;
			tipObject.transform
				.DOLocalMoveY(targetLocalY, GameDefines.FlyMoneyTip_MoveTime)
				.OnComplete(
					() =>
					{
						pool.Push(tipObject);
						tipObject.SetActive(false);
					});
		}

		private async void PlayFlyProp(
			Transform startPoint,
			ERewardType rewardType,
			Action successAction)
		{
			if (flyObjGoldDic == null)
			{
				RefreshEffectData();
			}
			Vector3? startPosition = startPoint != null
				? startPoint.position
				: (Vector3?)null;
			Sprite sprite = await GetPropSpriteAsync(rewardType);
			if (this == null)
			{
				return;
			}
			if (flyPropPool == null ||
				mFlyProp == null ||
				mFlyEffectParent == null)
			{
				return;
			}
			GameObject propObject = flyPropPool.Count > 0
				? flyPropPool.Pop()
				: Instantiate(mFlyProp.gameObject, mFlyEffectParent);
			propObject.transform.SetParent(mFlyEffectParent, false);
			propObject.SetActive(true);
			if (startPosition.HasValue)
			{
				propObject.transform.position = startPosition.Value;
			}
			Image image = propObject.GetComponent<Image>();
			if (image != null && sprite != null)
			{
				image.sprite = sprite;
			}
			if (flyObjGoldDic == null ||
				!flyObjGoldDic.TryGetValue(rewardType, out Vector3 target))
			{
				flyPropPool.Push(propObject);
				propObject.SetActive(false);
				return;
			}
			propObject.transform.DOMove(target, GameDefines.FlyProp_MoveTime)
				.OnComplete(
					() =>
					{
						successAction?.Invoke();
						flyPropPool.Push(propObject);
						propObject.SetActive(false);
					});
		}

		private static Sprite GetCurrencySprite(ERewardType targetType)
		{
			if (targetType == ERewardType.Gem)
			{
				return FacadePayTypeExtend.GetSingleGem != null
					? FacadePayTypeExtend.GetSingleGem()
					: null;
			}
			if (GameDefines.ifIAA)
			{
				return null;
			}
			return FacadePayTypeExtend.GetSingleCoin != null
				? FacadePayTypeExtend.GetSingleCoin()
				: null;
		}

		private static async Task<Sprite> GetPropSpriteAsync(
			ERewardType rewardType)
		{
			string configuredAddress;
			string fallbackAddress;
			switch (rewardType)
			{
				case ERewardType.AddBottle:
					configuredAddress = GameDefines.ERAddSpaceIconPath;
					fallbackAddress = "UI/FuncIcon/Bottle";
					break;
				case ERewardType.Clear:
					configuredAddress = GameDefines.ERClearIconPath;
					fallbackAddress = "UI/FuncIcon/icon_mofab";
					break;
				case ERewardType.Undo:
					configuredAddress = GameDefines.ERHammerIconPath;
					fallbackAddress = "UI/FuncIcon/icon_shuaxin";
					break;
				case ERewardType.Prompt:
					configuredAddress = GameDefines.ERAddPromptIconPath;
					fallbackAddress = "UI/FuncIcon/icon_mofab";
					break;
				case ERewardType.Guid:
					configuredAddress = GameDefines.ERAddGuidIconPath;
					fallbackAddress = "UI/FuncIcon/icon_shuaxin";
					break;
				default:
					return null;
			}
			Sprite sprite = string.IsNullOrEmpty(configuredAddress)
				? null
				: await AddressableAssetService.LoadAsync<Sprite>(configuredAddress);
			return sprite != null
				? sprite
				: await AddressableAssetService.LoadAsync<Sprite>(fallbackAddress);
		}

		private void OnDestroy()
		{
			if (mIsEventsBound)
			{
				RemoveButtonEvents();
			}
			CleanupResources();
		}

		private void CleanupResources()
		{
			UnregisterEventListeners();
			KillCongratulationTweens();
			isCleaningUpRewardEffects = true;
			CompleteGetRewardEffect(true);
			ProcessQueuedRewardEffects();
			getRewardEffectCoroutineOwner = null;
			getRewardEffectFinishAction = null;
			isCompletingRewardEffect = false;
			isProcessingRewardEffectRequests = false;
			isCleaningUpRewardEffects = false;
			pendingRewardEffectRequests?.Clear();
			pendingRewardEffectRequests = null;
			ERItemPool?.Clear();
			ERItemPool = null;
			curERItem?.Clear();
			curERItem = null;
			flyMoneyPool?.Clear();
			flyMoneyPool = null;
			flyMoneyTipPool?.Clear();
			flyMoneyTipPool = null;
			flyGemPool?.Clear();
			flyGemPool = null;
			flyGemTipPool?.Clear();
			flyGemTipPool = null;
			flyPropPool?.Clear();
			flyPropPool = null;
			clickPool?.Clear();
			clickPool = null;
			flyObjGoldDic?.Clear();
			flyObjGoldDic = null;
			LTEGoldTrans = null;
		}

		private RectTransform FindRect(string name)
		{
			return FindComponent<RectTransform>(name);
		}

		private T FindComponent<T>(string name) where T : Component
		{
			GameObject child = FindChildUtility.FindChild(transform, name);
			return child != null ? child.GetComponent<T>() : null;
		}

		private static void SetActive(Component component, bool active)
		{
			if (component != null &&
				component.gameObject.activeSelf != active)
			{
				component.gameObject.SetActive(active);
			}
		}
	}
}
