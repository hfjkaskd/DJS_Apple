using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	public class WithdrawMissionView : BaseView
	{
		private const int VisibleStageCount = 3;
		private const int Stage3Id = 3;
		private const int AdMissionType = 4;
		private static int DefaultAdMissionTarget => Framework2Host.Settings.economy.rewards.stage3AdTargetFallback;
		private const float RefreshInterval = 0.2f;
		private const string EmptyPayPalValue = "--";
		private static float Stage3AdDisplayIncrease => Framework2Host.Settings.economy.rewards.stage3AdDisplayIncrease;
		private const string Stage3AdIncreaseColor = "#E60000";
		private const float TestButtonYOffset = 120f;
		private const string TestSubmitButtonName = "TestFirstWithdrawBtn";
		private const string TestSubmitButtonText = "测试首提请求";
		private const string TestSubmitMissingPayPalText =
			"PayPal账号为空，无法发送首提请求";
		private const string TestSubmitTimeoutText =
			"首提测试请求超时或未完成";

		private Text timeText;
		private Text stateText;
		private Text curCoinText;
		private Text queueSizeText;
		private Image progress;
		private RectTransform progressStateRoot;
		private Text decText;
		private Button generateMoreBtn;
		private Button continueBtn;
		private Button testFirstWithdrawBtn;
		private readonly WithdrawMissionStateItem[] progressStates =
			new WithdrawMissionStateItem[VisibleStageCount];
		private bool isInitialized;
		private bool isSubmittingWithdrawRequest;
		private float nextRefreshTime;
		private string defaultStateValue;
		private string defaultQueueValue;
		private int lastTaskStage = -1;
		private int lastTaskLevel;
		private int lastTaskAds;
		private int lastTaskAdOwner;
		private int lastTaskCycle;
		private float lastTaskCoin;
		private string lastTaskEmail;
		private LanguageType lastTaskLanguage;

		public override void InitView()
		{
			InitializeIfNeeded();
			RefreshView();
		}

		public override void Start()
		{
			InitializeIfNeeded();
			RefreshView();
		}

		public override void Update()
		{
			if (!isInitialized || Time.unscaledTime < nextRefreshTime)
			{
				return;
			}

			RefreshView();
			nextRefreshTime = Time.unscaledTime + RefreshInterval;
		}

		private void InitializeIfNeeded()
		{
			if (isInitialized)
			{
				return;
			}

			timeText = FindComponent<Text>("Time");
			stateText = FindComponent<Text>("State");
			curCoinText = FindComponent<Text>("CurCoin");
			if (curCoinText != null)
			{
				curCoinText.supportRichText = true;
			}
			queueSizeText = FindComponent<Text>("QueueSize");
			progress = FindComponent<Image>("Progress");
			progressStateRoot = FindComponent<RectTransform>(
				"ProgressStateRoot");
			decText = FindComponent<Text>("Dec");
			WithdrawalTaskPresentation.OwnDynamicText(timeText);
			WithdrawalTaskPresentation.OwnDynamicText(stateText);
			WithdrawalTaskPresentation.OwnDynamicText(curCoinText);
			WithdrawalTaskPresentation.OwnDynamicText(queueSizeText);
			WithdrawalTaskPresentation.OwnDynamicText(decText);
			generateMoreBtn = FindComponent<Button>("GenerateMoreBtn");
			continueBtn = FindComponent<Button>("continuebtn");
			if (continueBtn == null)
			{
				continueBtn = FindComponent<Button>("continue");
			}

			EnsureTestFirstWithdrawButton();

			if (continueBtn != null)
			{
				continueBtn.onClick.AddListener(ContinueClick);
			}
			if (generateMoreBtn != null)
			{
				generateMoreBtn.onClick.AddListener(
					GenerateMoreBtnClick);
			}
			if (testFirstWithdrawBtn != null)
			{
				testFirstWithdrawBtn.onClick.AddListener(
					TestSubmitFirstWithdrawClick);
			}

			if (stateText != null)
			{
				defaultStateValue = stateText.text;
			}
			if (queueSizeText != null)
			{
				defaultQueueValue = queueSizeText.text;
			}

			if (progressStateRoot != null)
			{
				int count = Mathf.Min(
					VisibleStageCount,
					progressStateRoot.childCount);
				for (int i = 0; i < count; i++)
				{
					progressStates[i] = GetOrAddStateItem(
						progressStateRoot.GetChild(i).gameObject);
				}
			}

			isInitialized = true;
		}

		private T FindComponent<T>(string childName)
			where T : Component
		{
			GameObject child =
				FindChildUtility.FindChild(gameObject, childName);
			return child != null ? child.GetComponent<T>() : null;
		}

		private void ContinueClick()
		{
			HandleActionButtonClick();
		}

		private void GenerateMoreBtnClick()
		{
			HandleActionButtonClick();
		}

		private void TestSubmitFirstWithdrawClick()
		{
			if (isSubmittingWithdrawRequest)
			{
				return;
			}

			SoundManager.Instance?.PlayUIClickSFX();
			StartCoroutine(SubmitStage3WithdrawTestCoroutine());
		}

		private void HandleActionButtonClick()
		{
			Framework2BusinessAnalytics.Withdraw("ad_mission_action", null);
			if (isSubmittingWithdrawRequest)
			{
				return;
			}

			GameManagerWZ gm = GameManagerWZ.instance;
			if (gm != null &&
				gm.currentStage == Stage3Id &&
				gm.IsStageCompleted(Stage3Id))
			{
				CompleteStage3WithdrawIfReady();
				return;
			}

			UIManager.instance?.CloseView(
				EUIType.WithdrawMissionView);
		}

		internal override void RefreshRuntimeGmPresentation()
		{
			lastTaskStage = -1;
			RefreshView();
		}

		private void RefreshView()
		{
			GameManagerWZ gm = GameManagerWZ.instance;
			if (gm == null)
			{
				return;
			}

			RefreshTime();
			LanguageType language = LocalizationManager.Instance != null
				? LocalizationManager.Instance.GetCurrentLanguage() : default(LanguageType);
			string email = gm.getPlayerEmail();
			if (lastTaskStage != gm.currentStage || lastTaskLevel != gm.currentLv ||
				lastTaskAds != gm.currentStageAdWatchCount || lastTaskAdOwner != gm.currentStageAdWatchOwnerStage ||
				lastTaskCycle != gm.stage5CurrentCycle || lastTaskCoin != gm.currentCoin ||
				lastTaskEmail != email || lastTaskLanguage != language)
			{
				RefreshMoney(gm);
				RefreshMissionState(gm);
				lastTaskStage = gm.currentStage;
				lastTaskLevel = gm.currentLv;
				lastTaskAds = gm.currentStageAdWatchCount;
				lastTaskAdOwner = gm.currentStageAdWatchOwnerStage;
				lastTaskCycle = gm.stage5CurrentCycle;
				lastTaskCoin = gm.currentCoin;
				lastTaskEmail = email;
				lastTaskLanguage = language;
			}
			RefreshTestFirstWithdrawButton(gm);
		}

		private void RefreshTime()
		{
			if (timeText == null)
			{
				return;
			}

			DateTimeOffset createdAt;
			timeText.text =
				WithdrawApiService.TryGetFirstAdOrderCreateTime(
					out createdAt)
					? createdAt.ToLocalTime().ToString(
						"yyyy-MM-dd HH:mm:ss",
						CultureInfo.InvariantCulture)
					: EmptyPayPalValue;
		}

		private void RefreshMoney(GameManagerWZ gm)
		{
			if (curCoinText != null && gm != null)
			{
				curCoinText.text = GetDisplayedWithdrawText(gm);
			}
		}

		private void RefreshMissionState(GameManagerWZ gm)
		{
			if (gm == null) return;
			WithdrawalTaskPresentation task = WithdrawalTaskPresentation.Read(gm, gm.currentStage);
			bool isAdFlow = gm.currentStage == Stage3Id && task.Type == AdMissionType && task.IsValid;
			if (progressStateRoot != null) progressStateRoot.gameObject.SetActive(isAdFlow);
			if (isAdFlow)
			{
				RefreshStage3Mission(gm);
				return;
			}
			if (stateText != null)
				stateText.text = task.IsValid ? WithdrawalTaskPresentation.Text(task.IsComplete ? "2149" : "3106") : string.Empty;
			SetMissionDescription(task.Objective(gm));
			SetQueueText(task.IsValid
				? string.Format(CultureInfo.CurrentCulture, WithdrawalTaskPresentation.Text("3105"), task.Progress(gm))
				: string.Empty, task.IsValid);
			SetGenerateMoreState(false);
			if (progress != null) progress.fillAmount = task.Fill;
		}

		private int analyticsMissionAds = -1;
		private int analyticsMissionTarget = -1;

		private void RefreshStage3Mission(GameManagerWZ gm)
		{
			int missionTarget = GetStageTarget(
				gm,
				Stage3Id,
				DefaultAdMissionTarget);
			int watchedAds = (int)WithdrawalTaskPresentation.Read(gm, Stage3Id).Current;
			int displayedAds = Mathf.Min(watchedAds, missionTarget);
			bool isCompleted = watchedAds >= missionTarget;
			if (analyticsMissionAds != watchedAds || analyticsMissionTarget != missionTarget)
			{
				analyticsMissionAds = watchedAds;
				analyticsMissionTarget = missionTarget;
				MahjongProductAnalytics.Track("f2_ad_task_presentation", "stage", Stage3Id, "watched_ads", watchedAds, "displayed_ads", displayedAds, "target", missionTarget, "complete", isCompleted, "display_amount", GetDisplayedWithdrawAmount(gm), "task_text", GetStage3MissionDescription(gm, displayedAds, missionTarget), "secondary_text", GetStage3SecondaryText(gm, displayedAds, missionTarget));
			}

			RestoreStaticTexts();
			SetStage3StateText(isCompleted);
			SetMissionDescription(
				GetStage3MissionDescription(
					gm,
					displayedAds,
					missionTarget));
			SetQueueText(
				GetStage3SecondaryText(
					gm,
					displayedAds,
					missionTarget),
				ShouldShowStage3SecondaryText(
					displayedAds,
					missionTarget));
			SetGenerateMoreState(isCompleted);
			SetProgressState(displayedAds, missionTarget);
		}

		private void RestoreStaticTexts()
		{
			if (stateText != null)
			{
				stateText.text = defaultStateValue ?? string.Empty;
			}
			if (queueSizeText == null)
			{
				return;
			}

			bool hasDefaultQueue =
				!string.IsNullOrEmpty(defaultQueueValue);
			queueSizeText.gameObject.SetActive(hasDefaultQueue);
			queueSizeText.text = hasDefaultQueue ? defaultQueueValue : string.Empty;
		}

		private void SetMissionDescription(string description)
		{
			if (decText != null)
			{
				decText.text = description ?? string.Empty;
			}
		}

		private void SetQueueText(
			string description,
			bool isVisible)
		{
			if (queueSizeText == null)
			{
				return;
			}

			queueSizeText.gameObject.SetActive(isVisible);
			queueSizeText.text = isVisible ? description ?? string.Empty : string.Empty;
		}

		private void SetStage3StateText(bool isCompleted)
		{
			if (stateText == null)
			{
				return;
			}

			stateText.text = isCompleted
				? GetText("3032", "success")
				: GetText("3029", "In queue");
		}

		private string GetStage3MissionDescription(
			GameManagerWZ gm,
			int watchedAds,
			int missionTarget)
		{
			int target = Mathf.Max(missionTarget, 1);
			int watched = Mathf.Clamp(watchedAds, 0, target);
			int remaining = target - watched;
			if (remaining <= 0)
			{
				string completedTemplate = GetText(
					"3085",
					"{0} successful authentications: Funds will be transferred immediately.");
				return string.Format(completedTemplate, target) +
					"\n" +
					GetStage3PayPalValue(gm);
			}
			if (watched < 1)
			{
				string startTemplate = GetText(
					"3084",
					"Watch ads to complete active verification. Watch {0} times for direct payout.");
				return string.Format(startTemplate, target);
			}

			string progressTemplate = GetText(
				"3043",
				"Successful authentication {0} times: Withdrawal amount +0.1");
			progressTemplate = progressTemplate
				.Replace("+0.1", "+{1}")
				.Replace("+0,1", "+{1}");
			return string.Format(
				progressTemplate,
				watched,
				FormatCoin(Stage3AdDisplayIncrease));
		}

		private bool ShouldShowStage3SecondaryText(
			int watchedAds,
			int missionTarget)
		{
			int target = Mathf.Max(missionTarget, 1);
			int watched = Mathf.Clamp(watchedAds, 0, target);
			return target - watched > 0;
		}

		private string GetStage3SecondaryText(
			GameManagerWZ gm,
			int watchedAds,
			int missionTarget)
		{
			int target = Mathf.Max(missionTarget, 1);
			int watched = Mathf.Clamp(watchedAds, 0, target);
			int remaining = target - watched;
			if (remaining <= 0)
			{
				return string.Empty;
			}
			if (watched < 1)
			{
				return GetStage3PayPalText(gm);
			}

			string template = GetText(
				"3044",
				"Watch {0} more times for immediate arrival.");
			return string.Format(template, Mathf.Max(remaining, 0));
		}

		private string GetStage3PayPalText(GameManagerWZ gm)
		{
			string template = GetText(
				"3086",
				"PayPal account: {0}");
			return string.Format(
				template,
				GetStage3PayPalValue(gm));
		}

		private string GetStage3PayPalValue(GameManagerWZ gm)
		{
			string email = gm != null
				? gm.getPlayerEmail()
				: string.Empty;
			return string.IsNullOrEmpty(email)
				? EmptyPayPalValue
				: email;
		}

		private void SetGenerateMoreState(bool isReady)
		{
			if (generateMoreBtn == null)
			{
				return;
			}

			generateMoreBtn.gameObject.SetActive(isReady);
			generateMoreBtn.interactable = isReady;
		}

		private void RefreshTestFirstWithdrawButton(GameManagerWZ gm)
		{
			if (testFirstWithdrawBtn == null)
			{
				return;
			}

			bool isVisible =
				GameDefines.EnableWithdrawMissionFirstAdTestButton &&
				gm != null &&
				gm.currentStage == Stage3Id;
			if (testFirstWithdrawBtn.gameObject.activeSelf != isVisible)
			{
				testFirstWithdrawBtn.gameObject.SetActive(isVisible);
			}
			testFirstWithdrawBtn.interactable =
				isVisible && !isSubmittingWithdrawRequest;
		}

		private void SetProgressState(
			int completedCount,
			int targetCount)
		{
			int target = Mathf.Max(targetCount, 1);
			int completed = Mathf.Clamp(
				completedCount,
				0,
				target);
			for (int i = 0; i < progressStates.Length; i++)
			{
				WithdrawMissionStateItem state = progressStates[i];
				if (state != null)
				{
					state.SetState(i + 1, i < completed);
				}
			}

			if (progress != null)
			{
				progress.fillAmount = ResolveProgressFillAmount(
					completedCount,
					targetCount);
			}
		}

		private float ResolveProgressFillAmount(
			int completedCount,
			int targetCount)
		{
			int target = Mathf.Max(targetCount, 1);
			int completed = Mathf.Clamp(
				completedCount,
				0,
				target);
			if (target <= 1 || completed <= 1)
			{
				return 0f;
			}

			return Mathf.Clamp01(
				(float)(completed - 1) / (target - 1));
		}

		private int GetStageTarget(
			GameManagerWZ gm,
			int stageId,
			int fallbackValue)
		{
			if (gm == null)
			{
				return fallbackValue;
			}

			int missionType;
			int missionTarget;
			if (!gm.TryGetStageRequirement(
					stageId,
					out missionType,
					out missionTarget) ||
				missionType != AdMissionType)
			{
				return fallbackValue;
			}
			return missionTarget > 0
				? missionTarget
				: fallbackValue;
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
			return coin.ToString("F2");
		}

		private float GetDisplayedWithdrawAmount(GameManagerWZ gm)
		{
			if (gm == null)
			{
				return 0f;
			}
			return gm.currentStage == Stage3Id
				? GetStage3WithdrawAmount(gm)
				: gm.currentCoin;
		}

		private float GetStage3WithdrawAmount(GameManagerWZ gm)
		{
			int target = GetStageTarget(
				gm,
				Stage3Id,
				DefaultAdMissionTarget);
			int watched = (int)WithdrawalTaskPresentation.Read(gm, Stage3Id).Current;
			int rewardedAds = Mathf.Min(
				watched,
				Mathf.Max(target - 1, 0));
			return GameDefines.AddCoin +
				rewardedAds * Stage3AdDisplayIncrease;
		}

		private string GetDisplayedWithdrawText(GameManagerWZ gm)
		{
			if (gm == null)
			{
				return "0";
			}

			float amount = GetDisplayedWithdrawAmount(gm);
			if (gm.currentStage != Stage3Id)
			{
				return FormatCoin(amount);
			}

			int watchedAds = (int)WithdrawalTaskPresentation.Read(gm, Stage3Id).Current;
			if (watchedAds < 1)
			{
				return gm.FormatRegionalValue(
					(double)GameDefines.AddCoin);
			}

			int target = GetStageTarget(
				gm,
				Stage3Id,
				DefaultAdMissionTarget);
			if (watchedAds <= Mathf.Max(target - 1, 0))
			{
				return gm.FormatRegionalValue(
						(double)Mathf.Max(
							0f,
							amount - Stage3AdDisplayIncrease)) +
					FormatStage3IncreaseText(gm);
			}
			return gm.FormatRegionalValue((double)amount);
		}

		private string FormatStage3IncreaseText(GameManagerWZ gm)
		{
			return "<color=" +
				Stage3AdIncreaseColor +
				">+" +
				gm.FormatRegionalValue(
					(double)Stage3AdDisplayIncrease) +
				"</color>";
		}

		private string FormatPlainCoin(float coin)
		{
			return coin.ToString("F2")
				.TrimEnd('0')
				.TrimEnd('.');
		}

		private string GetText(string key, string fallback)
		{
			if (LocalizationManager.Instance == null)
			{
				return fallback;
			}

			string value = LocalizationManager.Instance.GetText(key);
			return string.IsNullOrEmpty(value) ? fallback : value;
		}

		private WithdrawMissionStateItem GetOrAddStateItem(
			GameObject target)
		{
			if (target == null)
			{
				return null;
			}

			WithdrawMissionStateItem state =
				target.GetComponent<WithdrawMissionStateItem>();
			return state != null
				? state
				: target.AddComponent<WithdrawMissionStateItem>();
		}

		private void CompleteStage3WithdrawIfReady()
		{
			Framework2BusinessAnalytics.Withdraw("first_ad_withdraw_eligibility", null);
			GameManagerWZ gm = GameManagerWZ.instance;
			if (gm == null ||
				gm.currentStage != Stage3Id ||
				!gm.IsStageCompleted(Stage3Id))
			{
				return;
			}
			if (TryOpenStage3EmailInputIfNeeded(gm))
			{
				return;
			}

			StartCoroutine(SubmitStage3WithdrawCoroutine(gm));
		}

		private bool TryOpenStage3EmailInputIfNeeded(
			GameManagerWZ gm)
		{
			if (gm == null ||
				!string.IsNullOrEmpty(gm.getPlayerEmail()))
			{
				return false;
			}
			if (GameApp.viewManager == null)
			{
				return true;
			}

			DataModel dataModel =
				CreateStage3EmailInputDataModel(gm);
			Framework2BusinessAnalytics.Withdraw("ad_mission_receiver_required", dataModel, "open_input");
			GameApp.viewManager.Open(
				ViewType.WithDrawMethod,
				dataModel);
			return true;
		}

		private DataModel CreateStage3EmailInputDataModel(
			GameManagerWZ gm)
		{
			return new DataModel
			{
				Level = gm != null ? gm.currentLv : 0,
				Num = Stage3Id,
				Money = GetDisplayedWithdrawAmount(gm),
				Index = Stage3Id,
				Name = gm != null ? gm.getPlayerName() : string.Empty,
				Phone = string.Empty,
				Email = gm != null ? gm.getPlayerEmail() : string.Empty,
				CloseType = 1,
				Special = DataModel.SpecialWithdrawMissionBindEmail,
				RetryWithdraw = false,
				SubmitOnOpen = false,
				UseProcessViewOnMethodExit = false,
				SubmitRetryWithdrawOnMethodExit = false,
				CloseWithoutAdvance = true
			};
		}

		private IEnumerator SubmitStage3WithdrawCoroutine(
			GameManagerWZ gm)
		{
			if (gm == null)
			{
				yield break;
			}

			isSubmittingWithdrawRequest = true;
			SetActionButtonsInteractable(false);
			HistoryModel pendingHistory =
				GetPendingStage2History();
			float withdrawAmount =
				GetDisplayedWithdrawAmount(gm);
			MahjongProductAnalytics.Track("f2_ad_mission_submit_started", "stage", gm.currentStage, "amount", withdrawAmount, "ad_progress", gm.currentStageAdWatchCount, "balance", gm.GetCoin());
			WithdrawApiService.EnsureQueuedFirstAdOrder(
				withdrawAmount);
			WithdrawApiService.MarkQueuedFirstAdOrderUnderReview();

			ClientWithdrawRequest request =
				BuildStage3WithdrawRequest(
					gm,
					withdrawAmount);
			if (request == null)
			{
				Debug.LogWarning(
					"Stage3 local withdrawal skipped: PayPal receiver is empty.");
			}
			else
			{
				gm.StartCoroutine(
					SubmitStage3WithdrawLocallyCoroutine(request));
			}

			CompleteStage3WithdrawLocally(
				gm,
				pendingHistory,
				withdrawAmount,
				null);
		}

		private static IEnumerator
			SubmitStage3WithdrawLocallyCoroutine(
				ClientWithdrawRequest request)
		{
			bool requestCompleted = false;
			yield return WithdrawApiService.SubmitWithdraw(
				request,
				order =>
				{
					requestCompleted = true;
					Debug.Log(
						string.Format(
							"Stage3 local withdrawal saved: orderNo={0}, status={1}",
							order != null ? order.OrderNo : null,
							order != null
								? (int?)order.OrderStatus
								: null));
					if (order != null)
					{
						WithdrawApiService
							.ReplaceQueuedFirstAdOrder(order);
					}
				},
				error =>
				{
					requestCompleted = true;
					Debug.LogWarning(
						"Stage3 local withdrawal could not be saved: " + error);
				});

			if (!requestCompleted)
			{
				Debug.LogWarning(
					"Stage3 local withdrawal did not complete.");
			}
		}

		private IEnumerator SubmitStage3WithdrawTestCoroutine()
		{
			GameManagerWZ gm = GameManagerWZ.instance;
			if (gm == null)
			{
				yield break;
			}

			isSubmittingWithdrawRequest = true;
			SetActionButtonsInteractable(false);
			GetPendingStage2History();
			float withdrawAmount =
				GetDisplayedWithdrawAmount(gm);
			ClientWithdrawRequest request =
				BuildStage3WithdrawRequest(
					gm,
					withdrawAmount);
			if (request == null)
			{
				isSubmittingWithdrawRequest = false;
				SetActionButtonsInteractable(true);
				Debug.LogWarning(
					"Stage3 FIRST_AD test request skipped: PayPal receiver is empty.");
				WithdrawApiService.LogPaymentNotice(
					"WithdrawMissionView",
					TestSubmitMissingPayPalText,
					LogType.Warning);
				yield break;
			}

			bool requestCompleted = false;
			Debug.Log(
				string.Format(
					CultureInfo.InvariantCulture,
					"Stage3 FIRST_AD test request start: amount={0:F2}, receiver={1}",
					withdrawAmount,
					request.Receiver));
			yield return WithdrawApiService.SubmitWithdraw(
				request,
				order =>
				{
					requestCompleted = true;
					isSubmittingWithdrawRequest = false;
					SetActionButtonsInteractable(true);
					Debug.Log(
						string.Format(
							"Stage3 FIRST_AD test request succeeded: orderNo={0}, status={1}",
							order != null ? order.OrderNo : null,
							order != null
								? (int?)order.OrderStatus
								: null));
					WithdrawApiService.LogPaymentNotice(
						"WithdrawMissionView",
						"首提测试成功: " +
							(order != null &&
							 !string.IsNullOrEmpty(order.OrderNo)
								? order.OrderNo
								: EmptyPayPalValue),
						LogType.Log);
				},
				error =>
				{
					requestCompleted = true;
					isSubmittingWithdrawRequest = false;
					SetActionButtonsInteractable(true);
					Debug.LogWarning(
						"Stage3 FIRST_AD test request failed: " +
						error);
					WithdrawApiService.LogPaymentNotice(
						"WithdrawMissionView",
						error,
						LogType.Warning);
				});

			if (!requestCompleted)
			{
				isSubmittingWithdrawRequest = false;
				SetActionButtonsInteractable(true);
				Debug.LogWarning(
					"Stage3 FIRST_AD test request failed: request did not complete.");
				WithdrawApiService.LogPaymentNotice(
					"WithdrawMissionView",
					TestSubmitTimeoutText,
					LogType.Warning);
			}
		}

		private void CompleteStage3WithdrawLocally(
			GameManagerWZ gm,
			HistoryModel pendingHistory,
			double withdrawAmount,
			WithdrawOrderData order)
		{
			isSubmittingWithdrawRequest = false;
			SetActionButtonsInteractable(true);
			if (order != null)
			{
				WithdrawApiService.ReplaceQueuedFirstAdOrder(order);
			}
			MarkStage2HistorySubmitted(pendingHistory);
			DeductCurrentWithdrawMoney(
				gm,
				(float)withdrawAmount);
			bool showFeedback =
				ShouldShowWithdrawFeedbackViewAfterStage3(gm);
			MahjongProductAnalytics.Track("f2_first_ad_withdraw_flow_completed", "amount", withdrawAmount, "order_available", order != null, "balance_after", gm != null ? gm.GetCoin() : 0d, "show_feedback", showFeedback, "source", "local");
			UIManager.instance?.CloseView(
				EUIType.WithdrawMissionView);
			if (showFeedback)
			{
				UIManager.instance?.ShowView(
					EUIType.WithdrawFeedbackView);
			}
		}

		private bool ShouldShowWithdrawFeedbackViewAfterStage3(
			GameManagerWZ gm)
		{
			return !GameDefines.ifIAA &&
				gm != null &&
				gm.currentStage == Stage3Id &&
				gm.IsStageCompleted(Stage3Id);
		}

		private ClientWithdrawRequest BuildStage3WithdrawRequest(
			GameManagerWZ gm,
			double withdrawAmount)
		{
			string receiver = gm != null
				? gm.getPlayerEmail()
				: string.Empty;
			if (string.IsNullOrEmpty(receiver))
			{
				return null;
			}

			return new ClientWithdrawRequest
			{
				WithdrawType =
					WithdrawConstants.WithdrawTypeFirstAd,
				Receiver = receiver,
				Amount = Math.Round(withdrawAmount, 2),
				Currency = WithdrawConstants.CurrencyUsd,
				Note = "Stage3 first ad withdraw",
				EmailSubject = string.Empty,
				RecipientType =
					WithdrawConstants.GetCurrentRecipientType()
			};
		}

		private HistoryModel GetPendingStage2History()
		{
			List<HistoryModel> history =
				SPlayerPrefs.GetListTem<HistoryModel>(
					FacadePlayerPrefExtend.History);
			if (history == null)
			{
				return null;
			}

			for (int i = history.Count - 1; i >= 0; i--)
			{
				HistoryModel item = history[i];
				if (item != null &&
					item.Stage == 2 &&
					item.Status != 1)
				{
					return item;
				}
			}
			return null;
		}

		private void MarkStage2HistorySubmitted(
			HistoryModel pendingHistory)
		{
			List<HistoryModel> history =
				SPlayerPrefs.GetListTem<HistoryModel>(
					FacadePlayerPrefExtend.History);
			if (history == null)
			{
				return;
			}

			for (int i = history.Count - 1; i >= 0; i--)
			{
				HistoryModel item = history[i];
				if (item == null ||
					item.Stage != 2 ||
					item.Status == 1)
				{
					continue;
				}
				if (pendingHistory != null &&
					!string.IsNullOrEmpty(pendingHistory.ID) &&
					!string.Equals(
						item.ID,
						pendingHistory.ID,
						StringComparison.Ordinal))
				{
					continue;
				}

				item.Status = 1;
				break;
			}

			SPlayerPrefs.SetListTem(
				FacadePlayerPrefExtend.History,
				history);
		}

		private void HandleStage3WithdrawError(string error)
		{
			Framework2BusinessAnalytics.Withdraw("ad_mission_submit_failed", null, "failed");
			isSubmittingWithdrawRequest = false;
			SetActionButtonsInteractable(true);
			if (!string.IsNullOrEmpty(error))
			{
				Debug.LogWarning(
					"Stage3 FIRST_AD withdraw error: " +
					error);
			}
			UIManager.instance?.CloseView(
				EUIType.WithdrawMissionView);
		}

		private void SetActionButtonsInteractable(
			bool isInteractable)
		{
			if (continueBtn != null)
			{
				continueBtn.interactable = isInteractable;
			}
			if (generateMoreBtn != null)
			{
				generateMoreBtn.interactable = isInteractable;
			}
			if (testFirstWithdrawBtn != null)
			{
				testFirstWithdrawBtn.interactable = isInteractable;
			}
		}

		private void DeductCurrentWithdrawMoney(
			GameManagerWZ gm,
			float withdrawMoney)
		{
			if (withdrawMoney <= 0f || gm == null)
			{
				return;
			}

			gm.AddCoinTem(-withdrawMoney);
			gm.UpdateCoinUI();
		}

		private void EnsureTestFirstWithdrawButton()
		{
			if (!GameDefines.EnableWithdrawMissionFirstAdTestButton ||
				testFirstWithdrawBtn != null)
			{
				return;
			}

			GameObject existing = FindChildUtility.FindChild(
				gameObject,
				TestSubmitButtonName);
			if (existing != null)
			{
				testFirstWithdrawBtn =
					existing.GetComponent<Button>();
				ConfigureTestFirstWithdrawButton(existing);
				return;
			}
			if (continueBtn == null)
			{
				return;
			}

			GameObject buttonObject = Instantiate(
				continueBtn.gameObject,
				continueBtn.transform.parent);
			buttonObject.name = TestSubmitButtonName;
			buttonObject.SetActive(true);
			testFirstWithdrawBtn =
				buttonObject.GetComponent<Button>();

			RectTransform source =
				continueBtn.GetComponent<RectTransform>();
			RectTransform target =
				buttonObject.GetComponent<RectTransform>();
			if (source != null && target != null)
			{
				target.anchorMin = source.anchorMin;
				target.anchorMax = source.anchorMax;
				target.pivot = source.pivot;
				target.sizeDelta = source.sizeDelta;
				Vector2 position = source.anchoredPosition;
				position.y += TestButtonYOffset;
				target.anchoredPosition = position;
			}

			ConfigureTestFirstWithdrawButton(buttonObject);
		}

		private void ConfigureTestFirstWithdrawButton(
			GameObject buttonObject)
		{
			if (buttonObject == null)
			{
				return;
			}

			Text label =
				buttonObject.GetComponentInChildren<Text>(true);
			if (label == null)
			{
				return;
			}

			LocalizedText localized =
				label.GetComponent<LocalizedText>();
			if (localized != null)
			{
				localized.textId = string.Empty;
				localized.enabled = false;
			}
			label.text = TestSubmitButtonText;
			label.resizeTextForBestFit = true;
			label.fontSize = 52;
		}
	}
}
