using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	// Follow the target after safe-area adapters and UI layout have moved it.
	[DefaultExecutionOrder(1000)]
	public class Tutorial : BaseView
	{
		private static TimeSpan PresentationTimeout =>
			TimeSpan.FromSeconds(Framework2Host.Settings.sourceUiTimeoutSeconds);

		// The visible fingertip is above the hand root in the source sprite.
		// Move only the hand; the prefab Button accepts taps across the screen.
		private static float TargetHandVerticalOffset => Framework2Host.Settings.guideHandVerticalOffset;

		public enum TYPE
		{
			TYPE1 = 0,
			TYPE3 = 1,
			TYPE19 = 2
		}

		private static string GetStableTypeName(TYPE value)
		{
			switch (value)
			{
				case TYPE.TYPE1: return "TYPE1";
				case TYPE.TYPE3: return "TYPE3";
				case TYPE.TYPE19: return "TYPE19";
				default: return "UnknownTutorialType";
			}
		}

		private static int presentationVersion;
		private static int pendingPresentationVersion;
		public static bool IsPresentationPending => pendingPresentationVersion != 0 &&
			pendingPresentationVersion == presentationVersion;

		public static Tutorial instance;

		public TYPE currentType;

		public GameObject hand1;

		public GameObject mask;

		public GameObject Hole;

		public GameObject ClickPos;

		public GameObject GuideTextFather;

		public GameObject PosRoot;

		public Text GTContent;

		public int step;

		private Action currentStepCallback;

		private Action currentFailureContinuation;

		private RectTransform handRect;

		private Vector3 defaultHandAnchoredPosition;

		private bool hasDefaultHandPosition;

		private Button hostClickButton;
		private RectTransform clickRect;
		private RectTransform guideTarget;
		private Vector2 targetHandOffset;
		private int lastAdvanceFrame = -1;

		private void Awake()
		{
			instance = this;
			if (ClickPos != null) hostClickButton = ClickPos.GetComponent<Button>();
			clickRect = ClickPos != null ? ClickPos.transform as RectTransform : null;
			if (hostClickButton != null) hostClickButton.onClick.AddListener(ExecuteCurrentStepCallback);
			CacheDefaultHandPosition();
		}

		public override void Start()
		{
			instance = this;
			// ShowStepAsync owns the requested step, including the first instantiation.
			// Start must not overwrite a withdrawal step with a gameplay tutorial.
		}

		public override void Update()
		{
		}

		private void LateUpdate()
		{
			if (isShow && !IsClosing && currentStepCallback != null)
			{
				RefreshTargetPosition();
			}
		}

		public override void InitView()
		{
			currentStepCallback = null;
			currentFailureContinuation = null;
			ResetVisualState();
		}

		public override void HideView()
		{
            MahjongProductAnalytics.GuideHidden("f2_" + GetStableTypeName(currentType));
			currentStepCallback = null;
			currentFailureContinuation = null;
			ResetVisualState();
			base.HideView();
		}

		/// <summary>
		/// Shows and configures a tutorial only after its managed view is ready.
		/// All asynchronous tutorial entry points go through this method so a cold
		/// Addressables load cannot leave a pooled tutorial in its previous state.
		/// </summary>
		public static Task<bool> ShowStepAsync(
			TYPE type,
			int targetStep,
			Action onFailure = null)
		{
			int requestVersion = ++presentationVersion;
			MahjongProductAnalytics.Track("f2_guide_requested", "request_version", requestVersion, "guide_type", GetStableTypeName(type), "step", targetStep);
			return TrackPresentationAsync(requestVersion, ShowStepCoreAsync(
				type,
				targetStep,
				requestVersion,
				onFailure));
		}

		private static async Task<bool> TrackPresentationAsync(int version, Task<bool> presentation)
		{
			pendingPresentationVersion = version;
			try { bool shown = await presentation; MahjongProductAnalytics.Track("f2_guide_presentation_result", "request_version", version, "shown", shown); return shown; }
			finally
			{
				if (pendingPresentationVersion == version) pendingPresentationVersion = 0;
			}
		}

		public static void CancelPendingPresentation()
		{
			MahjongProductAnalytics.Track("f2_guide_cancelled", "request_version", presentationVersion, "pending", IsPresentationPending);
			presentationVersion++;
			if (instance != null)
			{
				instance.currentStepCallback = null;
				instance.currentFailureContinuation = null;
				instance.ResetVisualState();
			}
			UIManager.instance?.CloseView(EUIType.Tutorial);
		}

		/// <summary>
		/// Opens the legacy withdraw view first, then places TYPE3 over the real
		/// instantiated target. A fixed frame delay is not sufficient for a cold
		/// Addressables load, so callers must use this awaited path.
		/// </summary>
		public static Task<bool> OpenWithdrawTutorialAsync(
			DataModel dataModel,
			Action onFailure = null)
		{
			int requestVersion = ++presentationVersion;
			Framework2BusinessAnalytics.Withdraw("guide_withdraw_requested", dataModel);
			return TrackPresentationAsync(requestVersion,
				OpenWithdrawTutorialCoreAsync(dataModel, onFailure, requestVersion));
		}

		private static async Task<bool> OpenWithdrawTutorialCoreAsync(
			DataModel dataModel, Action onFailure, int requestVersion)
		{
			UIManager manager = UIManager.instance;
			ViewManager viewManager = GameApp.viewManager;
			if (manager == null || viewManager == null || dataModel == null)
			{
				return FailRequest(
					requestVersion,
					"Withdraw tutorial prerequisites are unavailable.",
					onFailure,
					false);
			}

			// Never leave the old full-screen tutorial intercepting input while the
			// target legacy view is loading.
			manager.CloseView(EUIType.Tutorial);

			try
			{
				Task<IBaseView> openTask = viewManager.OpenAsync(
					ViewType.WithDraw,
					dataModel);
				Task completedTask = await Task.WhenAny(
					openTask,
					Task.Delay(PresentationTimeout));
				if (completedTask != openTask)
				{
					// Cancel this guide-owned request so a late load cannot re-block
					// gameplay after the failure continuation has released the flow.
					return FailRequest(
						requestVersion,
						"Timed out while loading the withdraw tutorial target.",
						onFailure,
						true);
				}

				IBaseView openedView = await openTask;
				if (requestVersion != presentationVersion)
				{
					return false;
				}
				if (!(openedView is BaseViewWithDraw))
				{
					return FailRequest(
						requestVersion,
						"Withdraw view failed to load before the tutorial.",
						onFailure,
						false);
				}

				Canvas.ForceUpdateCanvases();
				await Task.Yield();
				if (requestVersion != presentationVersion)
				{
					return false;
				}

				return await ShowStepCoreAsync(
					TYPE.TYPE3,
					0,
					requestVersion,
					onFailure);
			}
			catch (Exception exception)
			{
				return FailRequest(
					requestVersion,
					"Withdraw tutorial failed: " + exception.Message,
					onFailure,
					false);
			}
		}

		private static async Task<bool> ShowStepCoreAsync(
			TYPE type,
			int targetStep,
			int requestVersion,
			Action onFailure)
		{
			Action presentationFailure = () => ContinueWithoutGuide(type, targetStep, onFailure);
			UIManager manager = UIManager.instance;
			if (manager == null)
			{
				return FailRequest(
					requestVersion,
					"UIManager is unavailable while showing the tutorial.",
					presentationFailure,
					false);
			}

			try
			{
				Task<BaseView> showTask = manager.ShowViewAsync(
					EUIType.Tutorial);
				Task completedTask = await Task.WhenAny(
					showTask,
					Task.Delay(PresentationTimeout));
				if (completedTask != showTask)
				{
					return FailRequest(
						requestVersion,
						"Timed out while loading the tutorial view.",
						presentationFailure,
						false);
				}

				BaseView shownView = await showTask;
				if (requestVersion != presentationVersion)
				{
					return false;
				}

				Tutorial tutorial = shownView as Tutorial;
				if (tutorial == null)
				{
					return FailRequest(
						requestVersion,
						"Tutorial view failed to load.",
						presentationFailure,
						false);
				}

				tutorial.currentType = type;
				tutorial.currentFailureContinuation = onFailure;
				Canvas.ForceUpdateCanvases();
				await Task.Yield();
				if (requestVersion != presentationVersion)
				{
					return false;
				}

				if (!tutorial.TryGoToStep(targetStep))
				{
					return FailRequest(
						requestVersion,
						"Tutorial target is unavailable for " +
						GetStableTypeName(type) +
						" step " + targetStep + ".",
						presentationFailure,
						false);
				}

				return true;
			}
			catch (Exception exception)
			{
				return FailRequest(
					requestVersion,
					"Tutorial setup failed: " + exception.Message,
					presentationFailure,
					false);
			}
		}

		private static bool FailRequest(
			int requestVersion,
			string message,
			Action onFailure,
			bool closeWithdraw)
		{
			if (requestVersion != presentationVersion)
			{
				return false;
			}

			MahjongProductAnalytics.Track("f2_guide_failure", "request_version", requestVersion, "reason", "presentation_unavailable", "close_withdraw", closeWithdraw);
			Debug.LogError(message);
			UIManager.instance?.CloseView(EUIType.Tutorial);
			if (closeWithdraw)
			{
				GameApp.viewManager?.Close((int)ViewType.WithDraw);
			}

			InvokeFailureContinuation(onFailure);
			return false;
		}

		private static void ContinueWithoutGuide(TYPE type, int targetStep, Action onFailure)
		{
			try
			{
				Action callback = CreateStepCallback(type, targetStep, onFailure);
				if (callback != null)
				{
					callback.Invoke();
					return;
				}
			}
			catch (Exception exception)
			{
				Debug.LogError("Tutorial step continuation failed: " + exception.Message);
			}
			InvokeFailureContinuation(onFailure);
		}

		private static void InvokeFailureContinuation(Action onFailure)
		{
			if (onFailure != null)
			{
				try { onFailure.Invoke(); return; }
				catch (Exception exception)
				{
					Debug.LogError("Tutorial failure continuation failed: " + exception.Message);
				}
			}
			// A failed optional guide must not strand a completed round on an empty
			// screen. An open business view keeps its normal input/close flow.
			GameManagerWZ manager = GameManagerWZ.instance;
			if (manager != null && manager.HasPendingHostStageTutorial &&
				(UIManager.instance == null || !UIManager.instance.HasHostBlockingViews))
				manager.NextLevel();
		}

		public void GoToNextStep()
		{
			GoToStep(step + 1);
		}

		public void GoToStep(int targetStep)
		{
			if (!TryGoToStep(targetStep))
			{
				TYPE type = currentType;
				Action failure = currentFailureContinuation;
				FailRequest(presentationVersion,
					"Unable to configure tutorial " +
					GetStableTypeName(currentType) +
					" step " + targetStep + ".",
					() => ContinueWithoutGuide(type, targetStep, failure), false);
			}
		}

		private bool TryGoToStep(int targetStep)
		{
            if (step != targetStep) MahjongProductAnalytics.GuideStepComplete("f2_" + GetStableTypeName(currentType), step);
            step = targetStep;
            currentStepCallback = null;
            ResetVisualState();
            currentStepCallback = CreateStepCallback(currentType, targetStep, currentFailureContinuation);
            bool configured;
            switch (currentType)
            {
                case TYPE.TYPE1: configured = HandleType1Step(targetStep); break;
                case TYPE.TYPE3: configured = HandleType3Step(targetStep); break;
                case TYPE.TYPE19: configured = HandleType19Step(targetStep); break;
                default: configured = false; break;
            }
            if (configured && !(currentType == TYPE.TYPE1 && targetStep == 5))
                MahjongProductAnalytics.GuideShow("f2_" + GetStableTypeName(currentType), step);
            return configured;
		}

		private void OnDestroy()
		{
			if (hostClickButton != null) hostClickButton.onClick.RemoveListener(ExecuteCurrentStepCallback);
			if (instance == this) instance = null;
		}

		private bool HandleType19Step(int targetStep)
		{
			return targetStep == 0 && SetupType19Step0();
		}

		private bool SetupType19Step0()
		{
			GamePlayerView gamePlayerView = UIManager.instance != null
				? UIManager.instance.GetView<GamePlayerView>(
					EUIType.GamePlayerView)
				: null;
			RectTransform target = gamePlayerView != null && gamePlayerView.CoinBoard != null
				? gamePlayerView.CoinBoard.transform as RectTransform : null;
			return SetupTarget(target, new Vector2(0f, TargetHandVerticalOffset));
		}

		private bool HandleType3Step(int targetStep)
		{
			switch (targetStep)
			{
				case 0:
					return SetupType3Step0();
				case 1:
					return SetupType3Step2();
				case 3:
					return SetupType3Step3();
				default:
					return false;
			}
		}

		private bool HandleType1Step(int targetStep)
		{
			switch (targetStep)
			{
				case 0:
					return SetupStep0();
				case 1:
					return SetupStep1();
				case 3:
					return SetupStep3();
				case 4:
					return SetupStep4();
				case 5:
					FinishTutorial();
					return true;
				default:
					Debug.LogWarning(
						"Unhandled TYPE1 tutorial step: " + targetStep);
					return false;
			}
		}

		private bool SetupType3Step0()
		{
			BaseViewWithDraw withdrawView = GameApp.viewManager != null
				? GameApp.viewManager.GetView(
					(int)ViewType.WithDraw) as BaseViewWithDraw
				: null;
			RectTransform target = withdrawView != null ? withdrawView.Find<RectTransform>("pos") : null;
			return SetupTarget(target, new Vector2(-2f, TargetHandVerticalOffset));
		}

		private bool SetupType3Step2()
		{
			GamePlayerView gamePlayerView = UIManager.instance != null
				? UIManager.instance.GetView<GamePlayerView>(
					EUIType.GamePlayerView)
				: null;
			gamePlayerView?.ShowGemCurrencyForGuide();
			Canvas.ForceUpdateCanvases();
			RectTransform target = gamePlayerView != null && gamePlayerView.GemBoard != null
				? gamePlayerView.GemBoard.transform as RectTransform : null;
			return SetupTarget(target, new Vector2(0f, TargetHandVerticalOffset));
		}

		private bool SetupType3Step3()
		{
			GamePlayerView gamePlayerView = UIManager.instance != null
				? UIManager.instance.GetView<GamePlayerView>(
					EUIType.GamePlayerView)
				: null;
			RectTransform target =
				gamePlayerView != null && gamePlayerView.CoinBoard != null
					? gamePlayerView.CoinBoard.GetComponent<RectTransform>()
					: null;
			return SetupTarget(target, new Vector2(-1f, TargetHandVerticalOffset));
		}

		// Navigation belongs to the step, not to the visual target or its listeners.
		// Reuse it when guide art, target bindings or the guide prefab are unavailable.
		private static Action CreateStepCallback(TYPE type, int targetStep, Action onFailure)
		{
			if (type == TYPE.TYPE3 && targetStep == 0)
				return () =>
				{
					WithDraw view = GameApp.viewManager?.GetView((int)ViewType.WithDraw) as WithDraw;
					if (view == null) throw new InvalidOperationException("Withdrawal view is unavailable.");
					view.OnButtonClick();
				};
			if (type == TYPE.TYPE3 && targetStep == 1)
				return () =>
				{
					if (UIManager.instance == null) throw new InvalidOperationException("Withdrawal UI is unavailable.");
					UIManager.instance.ShowView(EUIType.WithdrawView);
				};
			if ((type == TYPE.TYPE19 && targetStep == 0) || (type == TYPE.TYPE3 && targetStep == 3))
				return () =>
				{
					GameManagerWZ manager = GameManagerWZ.instance;
					if (manager == null) throw new InvalidOperationException("Withdrawal state is unavailable.");
					bool reopen = type == TYPE.TYPE19;
					var data = new DataModel
					{
						Level = manager.currentLv - 1,
						Num = reopen ? Mathf.Max(1, manager.currentStage) : manager.currentStage - 1,
						Money = manager.GetCoin(),
						Type = reopen ? 2 : 0,
						CloseType = 1
					};
					_ = OpenWithdrawTutorialAsync(data, onFailure);
				};
			return null;
		}

		private bool SetupStep0()
		{
			SetType1Visuals("1001");
			return SetGuidePosition("Pos1");
		}

		private bool SetupStep1()
		{
			SetType1Visuals("1001");
			return SetGuidePosition("Pos1");
		}

		private bool SetupStep3()
		{
			SetType1Visuals("1003");
			return SetGuidePosition("Pos1");
		}

		private bool SetupStep4()
		{
			SetType1Visuals("1003");
			return SetGuidePosition("Pos1");
		}

		private void SetType1Visuals(string localizationKey)
		{
			if (hand1 != null)
			{
				hand1.SetActive(true);
			}
			if (GuideTextFather != null)
			{
				GuideTextFather.SetActive(true);
			}
			if (GTContent != null && LocalizationManager.Instance != null)
			{
				GTContent.text = LocalizationManager.Instance.GetText(
					localizationKey);
			}
		}

		private bool SetupTarget(
			RectTransform target,
			Vector2 handOffset)
		{
			if (hostClickButton == null || clickRect == null)
			{
				return false;
			}

			guideTarget = target;
			targetHandOffset = handOffset;
			hostClickButton.interactable = true;
			ShowTargetVisuals();
			RefreshTargetPosition();
			return true;
		}

		private void RefreshTargetPosition()
		{
			bool targetVisible = guideTarget != null && guideTarget.gameObject.activeInHierarchy;
			if (hand1 != null && hand1.activeSelf != targetVisible) hand1.SetActive(targetVisible);
			if (!targetVisible) return;

			PlaceHandAtTarget(guideTarget, targetHandOffset);
		}

		private void ResetVisualState()
		{
			guideTarget = null;
			RestoreDefaultHandPosition();
			if (mask != null)
			{
				mask.SetActive(false);
			}
			if (Hole != null)
			{
				Hole.SetActive(false);
			}
			if (ClickPos != null)
			{
				ClickPos.SetActive(false);
			}
			if (hand1 != null)
			{
				hand1.SetActive(false);
			}
			if (GuideTextFather != null)
			{
				GuideTextFather.SetActive(false);
			}
		}

		private void ShowTargetVisuals()
		{
			if (mask != null)
			{
				mask.SetActive(false);
			}
			if (Hole != null)
			{
				Hole.SetActive(false);
			}
			if (ClickPos != null)
			{
				ClickPos.SetActive(true);
			}
			if (hand1 != null)
			{
				hand1.SetActive(true);
			}
			if (GuideTextFather != null)
			{
				GuideTextFather.SetActive(false);
			}
		}

		private bool SetGuidePosition(string posName)
		{
			if (PosRoot == null || GuideTextFather == null)
			{
				return false;
			}

			Transform target = PosRoot.transform.Find(posName);
			RectTransform targetRect = target != null
				? target.GetComponent<RectTransform>()
				: null;
			RectTransform guideRect =
				GuideTextFather.GetComponent<RectTransform>();
			if (targetRect == null || guideRect == null)
			{
				return false;
			}

			guideRect.anchoredPosition = targetRect.anchoredPosition;
			return true;
		}

		private void FinishTutorial()
		{
			MahjongProductAnalytics.GuideEnd("f2_" + GetStableTypeName(currentType));
			currentStepCallback = null;
			currentFailureContinuation = null;
			ResetVisualState();
			UIManager.instance?.CloseView(EUIType.Tutorial);
		}

		public void ExecuteCurrentStepCallback()
		{
			Action callback = currentStepCallback;
			if (callback == null || lastAdvanceFrame == Time.frameCount) return;
			lastAdvanceFrame = Time.frameCount;
			Action failure = currentFailureContinuation;
			currentStepCallback = null;
			if (hostClickButton != null) hostClickButton.interactable = false;
			MahjongProductAnalytics.GuideStepComplete("f2_" + GetStableTypeName(currentType), step);
			if (currentType != TYPE.TYPE1) MahjongProductAnalytics.GuideEnd("f2_" + GetStableTypeName(currentType));
			// Release the full-screen Button before navigation, including an exception
			// or a business prerequisite that leaves the current view open.
			UIManager.instance?.CloseView(EUIType.Tutorial);
			try { callback.Invoke(); }
			catch (Exception exception)
			{
				Debug.LogError("Tutorial click continuation failed: " + exception.Message);
				InvokeFailureContinuation(failure);
			}
		}

		private bool PlaceHandAtTarget(
			Transform target,
			Vector2 anchoredOffset)
		{
			if (target == null || handRect == null)
			{
				return false;
			}

			// First align in world space because the tutorial and target can live
			// under different UI layer roots. Apply the visual correction through
			// anchoredPosition so CanvasScaler converts it consistently.
			handRect.position = target.position;
			handRect.anchoredPosition += anchoredOffset;
			return true;
		}

		private void CacheDefaultHandPosition()
		{
			if (handRect == null && hand1 != null)
			{
				handRect = hand1.GetComponent<RectTransform>();
			}
			if (!hasDefaultHandPosition && handRect != null)
			{
				defaultHandAnchoredPosition =
					handRect.anchoredPosition3D;
				hasDefaultHandPosition = true;
			}
		}

		private void RestoreDefaultHandPosition()
		{
			CacheDefaultHandPosition();
			if (hasDefaultHandPosition && handRect != null)
			{
				handRect.anchoredPosition3D =
					defaultHandAnchoredPosition;
			}
		}
	}
}
