using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	public class BonusWheelView : BaseView
	{
		private const int BonusRewardCount = 8;

		protected Button mExitBtn;
		protected Button mLotteryBtn;
		internal Button HostSpinButton => mLotteryBtn;
		protected RectTransform mTable;

		private List<float> mRewardAngles;
		private bool mIsEventsBound;
		private bool mIsInitialized;
		private bool mHasAutoStartedSpin;
		private LuckySpinOpenData mOpenData;

		private string analyticsSpinId;

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
				InitializeBonusWheel();
				mIsInitialized = true;
			}

			OnEnableView();
			BindButtonEvents();
			StartAutoSpinIfNeeded();
		}

		public override void Start()
		{
		}

		public override void Update()
		{
		}

		private void InitUIReferences()
		{
			Transform exitTransform = transform.Find("ExitBtn");
			mExitBtn = exitTransform != null ? exitTransform.GetComponent<Button>() : null;

			Transform lotteryTransform = transform.Find("Plane/LotteryBtn");
			mLotteryBtn = lotteryTransform != null ? lotteryTransform.GetComponent<Button>() : null;

			Transform tableTransform = transform.Find("Plane/Turntable/Table");
			mTable = tableTransform != null ? tableTransform.GetComponent<RectTransform>() : null;
		}

		private void InitializeBonusWheel()
		{
			mRewardAngles = GameDefines.BonusWheel_Angles;
		}

		private int GetConfiguredRewardCount()
		{
			return mRewardAngles != null
				? Math.Min(mRewardAngles.Count, BonusRewardCount)
				: 0;
		}

		private int GetTargetIndex(int rewardCount)
		{
			return rewardCount > 0
				? Mathf.Clamp(Framework2Host.Settings.economy.rewards.bonusWheelRewardIndex, 0, rewardCount - 1)
				: -1;
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
			Framework2Host.Runtime.RunSourceFlow(AutoStartSpinCoroutine());
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
			if (UIManager.instance != null)
			{
				UIManager.instance.CloseView(EUIType.BonusWheelView, this);
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
			analyticsSpinId = MahjongProductAnalytics.Begin("f2_bonus_wheel", "draw_mode", "fixed_index", "mandatory", mOpenData != null && mOpenData.IsMandatory, "auto_spin", mOpenData != null && mOpenData.AutoStartSpin);
			int rewardCount = GetConfiguredRewardCount();
			int targetIndex = GetTargetIndex(rewardCount);
			if (targetIndex < 0 || mTable == null)
			{
				Debug.LogError("[BonusWheel] Reward angles or table are unavailable.");
				OnEnableView();
				return;
			}

			float rewardAmount = GetBaseRewardAmount();
			MahjongProductAnalytics.Track("f2_bonus_wheel_selected", "operation_id", analyticsSpinId, "index", targetIndex, "amount", rewardAmount, "money_target", (int)GetMoneyRewardTarget(), "compensation", mOpenData != null && mOpenData.ForceMoneyReward);
			float targetAngle = mRewardAngles[targetIndex] - GameDefines.LS_rotateCount * 360f;
			mTable
				.DOLocalRotate(Vector3.forward * targetAngle, GameDefines.LS_rotateTime, RotateMode.FastBeyond360)
				.SetEase(Ease.OutQuint)
				.OnComplete(() =>
				{
					Framework2Host.Runtime.RunSourceFlow(DelayedAction(0.5f, () =>
					{
						if (UIManager.instance != null)
						{
							UIManager.instance.CloseView(EUIType.BonusWheelView, this);
						}
					}));
					MahjongProductAnalytics.Track("f2_bonus_wheel_stopped", "operation_id", analyticsSpinId, "index", targetIndex, "amount", rewardAmount);
					PlayMoneyRewardEffect(
						rewardAmount,
						mOpenData != null ? mOpenData.OnRewardComplete : null);
				});
			Framework2Host.Runtime.RunSourceFlow(PlaySpinSound());
		}

		private float GetBaseRewardAmount()
		{
			GameManagerWZ manager = GameManagerWZ.instance;
			if (mOpenData != null && mOpenData.ForcedMoneyAmount > 0f)
			{
				return manager != null && manager.CanGrantCoinRewardForCurrentLevel()
					? mOpenData.ForcedMoneyAmount
					: 0f;
			}
			return manager != null ? manager.GetCoinRewardAmount(3, -1) : 0f;
		}

		private IEnumerator PlaySpinSound()
		{
			for (int i = 0; i <= 6; i++)
			{
				yield return new WaitForSeconds(GameDefines.LS_rotateTime / 20f);
				PlaySoundEffect(AudioStCategory.ELuckySpin);
			}
			yield return new WaitForSeconds(GameDefines.LS_rotateTime - 1f);
			PlaySoundEffect(AudioStCategory.ELuckySpin2);
		}

		private IEnumerator DelayedAction(float delay, Action action)
		{
			yield return new WaitForSeconds(delay);
			action?.Invoke();
		}

		private void PlayMoneyRewardEffect(float count, Action finishAction = null)
		{
			MahjongProductAnalytics.Track("f2_bonus_wheel_award", "operation_id", analyticsSpinId, "amount", count, "money_target", (int)GetMoneyRewardTarget(), "eligible", count > 0f);
			if (count <= 0f)
			{
				finishAction?.Invoke();
				return;
			}

			if (GetMoneyRewardTarget() == LuckySpinMoneyRewardTarget.LuckyWallet)
			{
				WithdrawApiService.AddWalletIncome(count);
				finishAction?.Invoke();
				return;
			}

			if (FacadeEffectExtend.PlayGetRewardEffectHandle != null)
			{
				FacadeEffectExtend.PlayGetRewardEffectHandle(
					new[]
					{
						new ERewardItemStruct
						{
							Type = ERewardType.Money,
							Count = count
						}
					},
					finishAction);
				return;
			}

			if (GameManagerWZ.instance != null)
			{
				GameManagerWZ.instance.AddCoin(count);
			}
			finishAction?.Invoke();
		}

		private LuckySpinMoneyRewardTarget GetMoneyRewardTarget()
		{
			return mOpenData != null
				? mOpenData.MoneyRewardTarget
				: LuckySpinMoneyRewardTarget.CurrentCoin;
		}

		private void CleanupResources()
		{
			mRewardAngles = null;
			mOpenData = null;
		}

		private void OnDestroy()
		{
			if (mIsEventsBound)
			{
				RemoveButtonEvents();
			}
			CleanupResources();
			mIsInitialized = false;
		}

		private void PlaySoundEffect(AudioStCategory category)
		{
			FacadeAudioExtend.PlayEffectHandle?.Invoke(category);
		}
	}
}
