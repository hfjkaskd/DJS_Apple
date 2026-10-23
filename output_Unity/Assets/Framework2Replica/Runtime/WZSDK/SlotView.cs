using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	public class SlotView : BaseView
	{
		[CompilerGenerated]
		private sealed class _003CDelaySendReward_003Ed__45 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public SlotView _003C_003E4__this;

			public LotteryPanel panel;

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
			public _003CDelaySendReward_003Ed__45(int _003C_003E1__state)
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

		private const int DefaultPanelIndex = 1;

		private const float DefaultClosedDisplayValue = 0f;

		public LotteryPanel LotteryPanel1;

		public Button DrawButton;

		public Button CloseButton;

		[Header("Panel 1 Reward")]
		[SerializeField]
		private ERewardType lotteryPanel1RewardType;

		[SerializeField]
		private float lotteryPanel1RewardAmount;

		[SerializeField]
		private float rewardPopupDelay;

		private int currentPanelIndex;

		private LotteryPanel currentPanel;

		private bool isRolling;

		private bool hasRewardClaimed;

		private bool buttonsBound;

		private Coroutine rewardDelayCoroutine;

		private float currentLotteryPanel1RewardAmount;

		internal float AppleSkinRewardAmount => currentLotteryPanel1RewardAmount;

		private bool hasPendingOpenConfig;

		private Action rewardEffectFinishAction;

		public override void InitView()
		{
			PrepareOpenConfig();
			ResetState();
			BindButtons();
			BindPanelCallbacks();
			ApplyPanelState();
			RefreshButtons();
		}

		public override void Start()
		{
		}

		public override void Update()
		{
		}

		private void OnDestroy()
		{
			StopRewardDelay();
			UnbindButtons();
			UnbindPanelCallback(LotteryPanel1);
		}

		protected override void HandleViewArgs(object[] args)
		{
			currentPanelIndex = DefaultPanelIndex;
			rewardEffectFinishAction = null;
			SetPanelRewardValue(DefaultPanelIndex, 0f);
			hasPendingOpenConfig = true;

			if (args == null || args.Length == 0)
			{
				return;
			}

			int firstValueIndex = 0;
			int panelIndex;
			if (args.Length >= 2 && TryGetIntValue(args[0], out panelIndex))
			{
				currentPanelIndex = Mathf.Clamp(panelIndex, 1, 3);
				firstValueIndex = 1;
			}
			for (int i = firstValueIndex; i < args.Length; i++)
			{
				Action action = args[i] as Action;
				if (action != null)
				{
					rewardEffectFinishAction = action;
					continue;
				}
				float rewardValue;
				if (TryGetNumericValue(args[i], out rewardValue))
				{
					SetPanelRewardValue(currentPanelIndex, rewardValue);
				}
			}
		}

		public void SetCurrentPanel(int panelIndex, int? randomValue = null)
		{
			currentPanelIndex = Mathf.Clamp(panelIndex, 1, 3);
			if (randomValue.HasValue)
			{
				SetPanelRewardValue(currentPanelIndex, randomValue.Value);
			}
			StopRewardDelay();
			isRolling = false;
			hasRewardClaimed = false;
			currentPanel = null;
			ApplyPanelState();
			RefreshButtons();
		}

		public void SetPanelRewardValue(int panelIndex, float rewardValue)
		{
			float nonNegativeValue = Mathf.Max(0f, rewardValue);
			int randomValue = Mathf.Max(0, Mathf.RoundToInt(nonNegativeValue));
			if (panelIndex == DefaultPanelIndex)
			{
				currentLotteryPanel1RewardAmount = nonNegativeValue;
				if (LotteryPanel1 != null)
				{
					LotteryPanel1.SetRandomInt(randomValue);
				}
			}
		}

		public void SetCurrentPanelRewardValue(float rewardValue)
		{
			SetPanelRewardValue(currentPanelIndex, rewardValue);
		}

		public void SetPanelRandomInt(int panelIndex, int randomValue)
		{
			SetPanelRewardValue(panelIndex, randomValue);
		}

		public void SetCurrentPanelRandomInt(int randomValue)
		{
			SetPanelRewardValue(currentPanelIndex, randomValue);
		}

		private void ResetState()
		{
			StopRewardDelay();
			isRolling = false;
			hasRewardClaimed = false;
		}

		private void PrepareOpenConfig()
		{
			if (!hasPendingOpenConfig)
			{
				ResetOpenConfigToDefaults();
			}
			hasPendingOpenConfig = false;
		}

		private void ResetOpenConfigToDefaults()
		{
			currentPanelIndex = DefaultPanelIndex;
			rewardEffectFinishAction = null;
			SetPanelRewardValue(DefaultPanelIndex, DefaultClosedDisplayValue);
		}

		private void ApplyPanelState()
		{
			currentPanel = null;
			UpdatePanelState(
				LotteryPanel1,
				currentPanelIndex == DefaultPanelIndex);
		}

		private void UpdatePanelState(LotteryPanel panel, bool shouldShow)
		{
			if (panel == null)
			{
				return;
			}
			if (shouldShow)
			{
				currentPanel = panel;
			}
			if (panel.gameObject.activeSelf != shouldShow)
			{
				panel.gameObject.SetActive(shouldShow);
			}
			if (shouldShow)
			{
				panel.ShowStaticValue(
					Mathf.Max(0, Mathf.RoundToInt(DefaultClosedDisplayValue)));
			}
		}

		private void BindButtons()
		{
			if (buttonsBound)
			{
				return;
			}
			if (DrawButton != null)
			{
				DrawButton.onClick.AddListener(OnDrawButtonClick);
			}
			if (CloseButton != null)
			{
				CloseButton.onClick.AddListener(OnCloseButtonClick);
			}
			buttonsBound = true;
		}

		private void UnbindButtons()
		{
			if (!buttonsBound)
			{
				return;
			}
			if (DrawButton != null)
			{
				DrawButton.onClick.RemoveListener(OnDrawButtonClick);
			}
			if (CloseButton != null)
			{
				CloseButton.onClick.RemoveListener(OnCloseButtonClick);
			}
			buttonsBound = false;
		}

		private void OnDrawButtonClick()
		{
			MahjongProductAnalytics.Track("f2_stage_slot_draw_click", "already_claimed", hasRewardClaimed, "rolling", isRolling, "panel_ready", currentPanel != null, "amount", GetRewardAmount(currentPanel));
			if (isRolling || hasRewardClaimed || currentPanel == null)
			{
				return;
			}
			SoundManager.Instance?.PlayUIClickSFX();
			currentPanel.PlayResultRoll();
		}

		private void OnCloseButtonClick()
		{
			SoundManager.Instance?.PlayUIClickSFX();
			CloseSlotView();
		}

		private void CloseSlotView()
		{
			if (canvasGroup != null)
			{
				HideView();
			}
			else
			{
				gameObject.SetActive(false);
			}
		}

		private LotteryPanel GetCurrentPanel()
		{
			return currentPanel;
		}

		private LotteryPanel GetPanelByIndex(int panelIndex)
		{
			return panelIndex < 2 ? LotteryPanel1 : null;
		}

		private void BindPanelCallbacks()
		{
			BindPanelCallback(LotteryPanel1);
		}

		private void UnbindPanelCallbacks()
		{
			UnbindPanelCallback(LotteryPanel1);
		}

		private void BindPanelCallback(LotteryPanel panel)
		{
			if (panel == null)
			{
				return;
			}
			panel.RollStarted -= OnPanelRollStarted;
			panel.RollCompleted -= OnPanelRollCompleted;
			panel.RollStarted += OnPanelRollStarted;
			panel.RollCompleted += OnPanelRollCompleted;
		}

		private void UnbindPanelCallback(LotteryPanel panel)
		{
			if (panel == null)
			{
				return;
			}
			panel.RollStarted -= OnPanelRollStarted;
			panel.RollCompleted -= OnPanelRollCompleted;
		}

		private void OnPanelRollStarted(LotteryPanel panel, bool isRewardRoll)
		{
			MahjongProductAnalytics.Track("f2_stage_slot_started", "reward_roll", isRewardRoll, "current_panel", panel == currentPanel, "configured_amount", GetRewardAmount(panel));
			if (panel != currentPanel)
			{
				return;
			}
			isRolling = true;
			RefreshButtons();
		}

		private void OnPanelRollCompleted(LotteryPanel panel, bool isRewardRoll, int finalValue)
		{
			MahjongProductAnalytics.Track("f2_stage_slot_stopped", "reward_roll", isRewardRoll, "display_value", finalValue, "current_panel", panel == currentPanel, "already_claimed", hasRewardClaimed);
			if (panel != currentPanel)
			{
				return;
			}
			if (!isRewardRoll || hasRewardClaimed)
			{
				isRolling = false;
			}
			else
			{
				StopRewardDelay();
				rewardDelayCoroutine = StartCoroutine(DelaySendReward(panel));
			}
			RefreshButtons();
		}

		[IteratorStateMachine(typeof(_003CDelaySendReward_003Ed__45))]
		private IEnumerator DelaySendReward(LotteryPanel panel)
		{
			yield return new WaitForSeconds(rewardPopupDelay);
			rewardDelayCoroutine = null;
			if (panel != currentPanel || hasRewardClaimed)
			{
				yield break;
			}
			isRolling = false;
			hasRewardClaimed = true;
			SendReward(panel);
			RefreshButtons();
		}

		private void StopRewardDelay()
		{
			if (rewardDelayCoroutine == null)
			{
				return;
			}
			StopCoroutine(rewardDelayCoroutine);
			rewardDelayCoroutine = null;
		}

		private void SendReward(LotteryPanel panel)
		{
			ERewardType rewardType = GetRewardType(panel);
			float rewardAmount = GetRewardAmount(panel);
			MahjongProductAnalytics.Track("f2_stage_slot_award_requested", "amount", rewardAmount, "reward_type", (int)rewardType, "effect_available", FacadeEffectExtend.PlayGetRewardEffectHandle != null);
			if (FacadeEffectExtend.PlayGetRewardEffectHandle != null)
			{
				FacadeEffectExtend.PlayGetRewardEffectHandle(
					new[]
					{
						new ERewardItemStruct
						{
							Type = rewardType,
							Count = rewardAmount
						}
					},
					rewardEffectFinishAction);
			}
			CloseSlotView();
		}

		private ERewardType GetRewardType(LotteryPanel panel)
		{
			return panel == LotteryPanel1
				? lotteryPanel1RewardType
				: ERewardType.Money;
		}

		private float GetRewardAmount(LotteryPanel panel)
		{
			return panel == LotteryPanel1
				? currentLotteryPanel1RewardAmount
				: 0f;
		}

		private bool TryGetIntValue(object value, out int result)
		{
			if (value is int)
			{
				result = (int)value;
				return true;
			}
			if (value is long)
			{
				long longValue = (long)value;
				if (longValue >= int.MinValue && longValue <= int.MaxValue)
				{
					result = (int)longValue;
					return true;
				}
			}
			if (value is string &&
				int.TryParse((string)value, out result))
			{
				return true;
			}
			result = 0;
			return false;
		}

		private bool TryGetNumericValue(object value, out float result)
		{
			if (value is int)
			{
				result = (int)value;
				return true;
			}
			if (value is long)
			{
				result = (long)value;
				return true;
			}
			if (value is float)
			{
				result = (float)value;
				return true;
			}
			if (value is double)
			{
				result = (float)(double)value;
				return true;
			}
			if (value is decimal)
			{
				result = (float)(decimal)value;
				return true;
			}
			if (value is string &&
				float.TryParse((string)value, out result))
			{
				return true;
			}
			result = 0f;
			return false;
		}

		internal override void RefreshRuntimeGmPresentation()
		{
			RefreshButtons();
		}

		private void RefreshButtons()
		{
			bool canDraw = !isRolling && !hasRewardClaimed;
			if (DrawButton != null)
			{
				DrawButton.gameObject.SetActive(canDraw);
				DrawButton.interactable = canDraw;
			}
			if (CloseButton != null)
			{
				CloseButton.interactable = !isRolling && hasRewardClaimed;
			}
		}
	}
}
