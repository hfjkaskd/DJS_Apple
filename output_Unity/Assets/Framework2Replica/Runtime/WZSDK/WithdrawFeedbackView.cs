using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	public class WithdrawFeedbackView : BaseView
	{
		private const int Stage3Id = 3;

		private Text titleText;

		private Button generateMoreBtn;

		private Button closeBtn;

		private Button continueBtn;

		private bool isReferencesResolved;

		private bool isEventsBound;

		internal double AppleSkinSavedAmount { get; private set; }

		private void Awake()
		{
			ResolveReferencesIfNeeded();
		}

		public override void InitView()
		{
			RefreshAppleSkinSavedAmount();
			ResolveReferencesIfNeeded();
			BindEvents();
			ApplyLocalizedTexts();
			RefreshGameplayWithdrawPrompt();
		}

		private void OnDestroy()
		{
			RemoveEvents();
		}

		private void ResolveReferencesIfNeeded()
		{
			if (isReferencesResolved)
			{
				return;
			}

			titleText = FindByPath<Text>(
				"RewardMessage/Panel/Top/Title");
			generateMoreBtn = FindByPath<Button>(
				"RewardMessage/Panel/Bottom/GenerateMoreBtn");
			closeBtn = FindByPath<Button>(
				"RewardMessage/Panel/bg/close");
			continueBtn = FindByPath<Button>(
				"RewardMessage/Panel/continuebtn");
			isReferencesResolved =
				titleText != null ||
				generateMoreBtn != null ||
				closeBtn != null ||
				continueBtn != null;
		}

		private T FindByPath<T>(string path) where T : Component
		{
			GameObject child =
				FindChildUtility.FindChildByPath(gameObject, path);
			return child != null ? child.GetComponent<T>() : null;
		}

		private void BindEvents()
		{
			if (isEventsBound)
			{
				return;
			}
			if (generateMoreBtn != null)
			{
				generateMoreBtn.onClick.AddListener(
					OnGenerateMoreClick);
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
			if (generateMoreBtn != null)
			{
				generateMoreBtn.onClick.RemoveListener(
					OnGenerateMoreClick);
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

		internal override void RefreshRuntimeGmPresentation()
		{
			RefreshAppleSkinSavedAmount();
			ApplyLocalizedTexts();
			RefreshGameplayWithdrawPrompt();
		}

		private void RefreshAppleSkinSavedAmount()
		{
			AppleSkinSavedAmount = 0d;
			// One cache read on opening/explicit GM refresh, never in a frame loop.
			var orders = WithdrawApiService.GetCachedOrders();
			for (int i = orders.Count - 1; i >= 0; --i)
			{
				if (orders[i] == null || !orders[i].IsFirstAdWithdraw) continue;
				AppleSkinSavedAmount = orders[i].GetAmountValue();
				break;
			}
		}

		private void ApplyLocalizedTexts()
		{
			if (titleText == null)
			{
				return;
			}

			LocalizedText localizedText =
				titleText.GetComponent<LocalizedText>();
			if (localizedText != null)
			{
				localizedText.SetTextId("3046");
				return;
			}
			titleText.text = GetText("3046", titleText.text);
		}

		private void OnGenerateMoreClick()
		{
			Framework2BusinessAnalytics.Withdraw("withdraw_feedback_continue", null);
			SoundManager.Instance?.PlayUIClickSFX();
			CloseCurrentView();
			AdvanceToStage4IfNeeded();
		}

		private void OnCloseClick()
		{
			Framework2BusinessAnalytics.Withdraw("withdraw_feedback_close", null, "advance_stage_if_ready");
			SoundManager.Instance?.PlayUIClickSFX();
			CloseCurrentView();
			AdvanceToStage4IfNeeded();
		}

		private void CloseCurrentView()
		{
			if (canvasGroup != null)
			{
				HideView();
				return;
			}
			gameObject.SetActive(false);
		}

		private void AdvanceToStage4IfNeeded()
		{
			GameManagerWZ gm = GameManagerWZ.instance;
			if (GameDefines.ifIAA ||
				gm == null ||
				gm.currentStage != Stage3Id ||
				!gm.IsStageCompleted(Stage3Id))
			{
				return;
			}
			gm.TrySyncStageProgress(Stage3Id);
		}

		private void RefreshGameplayWithdrawPrompt()
		{
			if (GameDefines.ifIAA ||
				UIManager.instance == null)
			{
				return;
			}
			GamePlayerView gamePlayerView =
				UIManager.instance.GetView<GamePlayerView>(
					EUIType.GamePlayerView);
			if (gamePlayerView != null)
			{
				gamePlayerView.UpdateWithdrawPrompt();
			}
		}

		private string GetText(string key, string fallback)
		{
			LocalizationManager localization =
				LocalizationManager.Instance;
			if (localization == null)
			{
				return fallback;
			}
			string value = localization.GetText(key);
			return string.IsNullOrEmpty(value)
				? fallback
				: value;
		}

		public override void Start()
		{
		}

		public override void Update()
		{
		}
	}
}
