using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	public class ProgressView : BaseView
	{
		[CompilerGenerated]
		private sealed class _003CCompleteStage4SlotFlowAfterClose_003Ed__36 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

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
			public _003CCompleteStage4SlotFlowAfterClose_003Ed__36(int _003C_003E1__state)
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

		private const int Stage4Id = 4;

		private const string DescriptionTextId = "3095";

		private const float ProgressAnimationSpeed = 0.4f;

		private Image progress;

		private Text countText;

		private Text decText;

		private RectTransform progressRoot;

		private Button closeBtn;

		private Button continueBtn;

		private bool isReferencesResolved;

		private bool isEventsBound;

		private bool isClosing;

		private float openRewardAmount;

		internal float AppleSkinRewardAmount { get; private set; }
		internal float AppleSkinProgress => displayedProgressValue;

		private float targetCoinValue;

		private float displayedProgressValue = -1f;

		private float targetProgressValue;

		private void Awake()
		{
			ResolveReferencesIfNeeded();
		}

		public override void InitView()
		{
			ResolveReferencesIfNeeded();
			BindEvents();
			InitializeProgressState();
		}

		public override void Start()
		{
		}

		public override void Update()
		{
			UpdateProgressAnimation();
		}

		private void OnDestroy()
		{
			RemoveEvents();
			StopAllCoroutines();
		}

		protected override void HandleViewArgs(object[] args)
		{
			openRewardAmount = 0f;
			if (args == null)
			{
				return;
			}
			for (int i = 0; i < args.Length; i++)
			{
				object value = args[i];
				float parsedValue;
				if (value is float)
				{
					parsedValue = (float)value;
				}
				else if (value is int)
				{
					parsedValue = Mathf.Abs((int)value);
				}
				else if (value is double)
				{
					parsedValue = (float)(double)value;
				}
				else
				{
					continue;
				}
				openRewardAmount = Mathf.Max(0f, parsedValue);
			}
		}

		private void ResolveReferencesIfNeeded()
		{
			if (isReferencesResolved)
			{
				return;
			}
			progress = FindComponent<Image>("Progress");
			countText = FindComponent<Text>("Count");
			decText = FindComponent<Text>("dec");
			progressRoot = FindComponent<RectTransform>("ProgressRoot");
			closeBtn = FindComponent<Button>("close");
			continueBtn = FindComponent<Button>("continuebtn");
			isReferencesResolved = true;
		}

		private T FindComponent<T>(string childName) where T : Component
		{
			GameObject child = FindChildUtility.FindChild(transform, childName);
			return child != null ? child.GetComponent<T>() : null;
		}

		private void BindEvents()
		{
			if (isEventsBound)
			{
				RemoveEvents();
			}
			if (closeBtn != null)
			{
				closeBtn.onClick.AddListener(OnCloseClick);
			}
			if (continueBtn != null)
			{
				continueBtn.onClick.AddListener(OnCloseClick);
			}
			isEventsBound = true;
		}

		private void RemoveEvents()
		{
			if (!isEventsBound)
			{
				return;
			}
			if (closeBtn != null)
			{
				closeBtn.onClick.RemoveListener(OnCloseClick);
			}
			if (continueBtn != null)
			{
				continueBtn.onClick.RemoveListener(OnCloseClick);
			}
			isEventsBound = false;
		}

		private void InitializeProgressState()
		{
			if (progressRoot != null &&
				!progressRoot.gameObject.activeSelf)
			{
				progressRoot.gameObject.SetActive(true);
			}
			isClosing = false;
			GameManagerWZ gm = GameManagerWZ.instance;
			targetCoinValue = GetTargetCoin(gm);
			float currentCoin = gm != null
				? Mathf.Max(0f, gm.currentCoin)
				: 0f;
			float startCoin = openRewardAmount > 0f
				? currentCoin - openRewardAmount
				: currentCoin;
			displayedProgressValue =
				Mathf.Clamp01(startCoin / targetCoinValue);
			targetProgressValue =
				Mathf.Clamp01(currentCoin / targetCoinValue);
			ApplyProgressVisualState(displayedProgressValue);
			ApplyStaticTexts(currentCoin, targetCoinValue);
			AppleSkinRewardAmount = openRewardAmount > 0f ? openRewardAmount : currentCoin;
			MahjongProductAnalytics.Track("f2_stage4_progress_presented", "reward_amount", openRewardAmount, "current", currentCoin, "target", targetCoinValue, "start_fill", displayedProgressValue, "end_fill", targetProgressValue, "description_key", DescriptionTextId);
			openRewardAmount = 0f;
		}

		internal override void RefreshRuntimeGmPresentation()
		{
			UpdateProgressAnimation();
		}

		private void UpdateProgressAnimation()
		{
			GameManagerWZ gm = GameManagerWZ.instance;
			targetCoinValue = GetTargetCoin(gm);
			float currentCoin = gm != null
				? Mathf.Max(0f, gm.currentCoin)
				: 0f;
			targetProgressValue =
				Mathf.Clamp01(currentCoin / targetCoinValue);
			displayedProgressValue = Mathf.MoveTowards(
				displayedProgressValue,
				targetProgressValue,
				Time.unscaledDeltaTime * ProgressAnimationSpeed);
			ApplyProgressVisualState(displayedProgressValue);
			ApplyStaticTexts(currentCoin, targetCoinValue);
		}

		private void ApplyProgressVisualState(float progressValue)
		{
			if (progress != null)
			{
				progress.fillAmount = Mathf.Clamp01(progressValue);
			}
		}

		private void ApplyStaticTexts(float currentCoin, float targetCoin)
		{
			currentCoin = Mathf.Max(0f, currentCoin);
			targetCoin = Mathf.Max(1f, targetCoin);
			if (countText != null)
			{
				countText.text =
					FormatCoin(currentCoin) + "/" + FormatCoin(targetCoin);
			}
			if (decText != null)
			{
				string descriptionTemplate = GetText(
					DescriptionTextId,
					"You just won {0}! Get to {1} to trigger the cash-out prize draw!");
				bool templateHasCurrencyMark =
					descriptionTemplate.Contains("$") ||
					descriptionTemplate.Contains("Rp") ||
					descriptionTemplate.Contains("RP");
				decText.text = string.Format(
					descriptionTemplate,
					templateHasCurrencyMark
						? FormatRegionalNumber(currentCoin)
						: FormatCoin(currentCoin),
					templateHasCurrencyMark
						? FormatRegionalNumber(targetCoin)
						: FormatCoin(targetCoin));
			}
		}

		private float GetTargetCoin(GameManagerWZ gm)
		{
			int type;
			int value;
			if (gm != null &&
				gm.TryGetStageRequirement(Stage4Id, out type, out value) &&
				type == 2)
			{
				return Mathf.Max(1, value);
			}
			return 1f;
		}

		private string FormatCoin(float coin)
		{
			GameManagerWZ gm = GameManagerWZ.instance;
			if (gm != null)
			{
				return gm.FormatRegionalValue((double)coin);
			}
			if (FacadePayTypeExtend.RegionalChangeHandle != null)
			{
				return FacadePayTypeExtend.RegionalChangeHandle(coin);
			}
			return GetCurrencyMark() + FormatPlainCoin(coin);
		}

		private string FormatRegionalNumber(float coin)
		{
			GameManagerWZ gm = GameManagerWZ.instance;
			return gm != null
				? gm.FormatRegionalValue((double)coin)
				: FormatPlainCoin(coin);
		}

		private string FormatPlainCoin(float coin)
		{
			int precision = GameManagerWZ.instance != null
				? Mathf.Max(GameManagerWZ.instance.decimals, 0)
				: 2;
			string value = coin.ToString("F" + precision);
			return value.TrimEnd('0').TrimEnd('.');
		}

		private string GetCurrencyMark()
		{
			GameManagerWZ gm = GameManagerWZ.instance;
			return gm != null && !string.IsNullOrEmpty(gm.mark)
				? gm.mark
				: "$";
		}

		private void OnCloseClick()
		{
			if (isClosing)
			{
				return;
			}
			isClosing = true;
			SoundManager.Instance?.PlayUIClickSFX();
			CloseCurrentView();
			StartCoroutine(CompleteStage4SlotFlowAfterClose());
		}

		private void CloseCurrentView()
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

		[IteratorStateMachine(typeof(_003CCompleteStage4SlotFlowAfterClose_003Ed__36))]
		private IEnumerator CompleteStage4SlotFlowAfterClose()
		{
			yield return new WaitForSecondsRealtime(GameDefines.HideAnimTime);
			GameManagerWZ.instance?.CompleteStage4SlotFlow();
		}

		private string GetText(string key, string fallback)
		{
			LocalizationManager localization = LocalizationManager.Instance;
			if (localization == null)
			{
				return fallback;
			}
			string value = localization.GetText(key);
			return string.IsNullOrEmpty(value) ? fallback : value;
		}
	}
}
