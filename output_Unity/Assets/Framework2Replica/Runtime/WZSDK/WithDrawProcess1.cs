using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	public class WithDrawProcess1 : BaseViewWithDraw
	{
		private const int ProcessStateCount = 5;
		private const string VerificationCompletedTextKey = "2052";
		private const float StepDuration = 1f;
		private const float CompletionActionsShowDelay = 1f;

		private static readonly string[] OtherStageWithdrawStateContentKeys =
		{
			"3017", "3019", "3021"
		};

		private static readonly string[] OtherStageWithdrawFinishedStateContentKeys =
		{
			"3018", "3020", "3046"
		};

		private static readonly string[] Stage3WithdrawStateContentKeys =
		{
			"3017", "3019", "3021"
		};

		private static readonly string[] Stage3WithdrawFinishedStateContentKeys =
		{
			"3018", "3020", "3022"
		};

		private Button continueButton;
		private Button closeButton;
		private GameObject continueRoot;
		private GameObject closeRoot;
		private RectTransform Dec;
		private float time;
		private int completedStateCount;
		private bool isAnimating;
		private double money;
		private Text moneyContent;
		private RectTransform stateRoot;
		private Text Num;
		private DataModel dataModel;
		private readonly WithDrawProcessStateItem[] processStates =
			new WithDrawProcessStateItem[ProcessStateCount];
		private int visibleStateCount;
		private int currentStageId;
		private bool isSubmitStarted;
		private bool isSubmitCompleted;
		private bool isSubmitSuccessful;
		private bool isProgressFlowCompleted;
		private WithdrawOrderData currentOrder;
		private Coroutine showCompletionActionsCoroutine;
		private bool completionActionsVisible;
		private bool isClosing;

		protected override void OnAwake()
		{
			continueButton = FindByPath<Button>("PanelA/Continue", "Continue");
			closeButton = FindByPath<Button>("PanelA/close", "close");
			continueRoot = continueButton != null
				? continueButton.gameObject
				: FindByPath("PanelA/Continue", "Continue");
			closeRoot = closeButton != null
				? closeButton.gameObject
				: FindByPath("PanelA/close", "close");
			Dec = FindByPath<RectTransform>("PanelA/PanelC/Dec", "Dec");
			moneyContent = FindByPath<Text>(
				"PanelA/PanelB/content", "content");
			stateRoot = FindByPath<RectTransform>(
				"PanelA/PanelC/StateRoot", "StateRoot");
			Num = FindByPath<Text>("PanelA/PanelC/content", "Current");
			if (continueButton != null)
			{
				continueButton.onClick.AddListener(OnButtonClick);
			}
			if (closeButton != null)
			{
				closeButton.onClick.AddListener(OnCloseBtnClick);
			}
			CacheProcessStates();
			ResetProgressFlow();
		}

		protected override void HandleViewArgs(object[] args)
		{
			dataModel = args != null && args.Length > 0
				? args[0] as DataModel
				: null;
			if (dataModel == null)
			{
				dataModel = new DataModel();
			}

			currentStageId = GetCurrentStageId();
			money = GetCurrentWithdrawMoney();
			visibleStateCount = GetVisibleStateCount();
			RefreshMoneyText();
			ApplyProcessStateContents();
			if (Num != null)
			{
				Num.text = string.Format(
					LocalizationManager.Instance.GetText("2062"),
					currentStageId);
			}
			currentOrder = null;
			ResetProgressFlow();
		}

		public override void InitView()
		{
		}

		private void CacheProcessStates()
		{
			for (int i = 0; i < ProcessStateCount; i++)
			{
				GameObject stateObject = null;
				if (stateRoot != null)
				{
					Transform state = stateRoot.Find("State" + (i + 1));
					stateObject = state != null ? state.gameObject : null;
				}
				if (stateObject == null)
				{
					stateObject = FindByPath(
						"PanelA/PanelC/StateRoot/State" + (i + 1),
						"State" + (i + 1));
				}
				processStates[i] = GetOrAddProcessStateItem(stateObject);
			}
		}

		private void ApplyProcessStateContents()
		{
			for (int i = 0; i < processStates.Length; i++)
			{
				WithDrawProcessStateItem state = processStates[i];
				if (state == null)
				{
					continue;
				}
				string waiting = GetWaitingStateContent(i, string.Empty);
				string finished = GetFinishedStateContent(i, waiting);
				state.SetContents(waiting, finished);
			}
		}

		private WithDrawProcessStateItem GetOrAddProcessStateItem(GameObject target)
		{
			if (target == null)
			{
				return null;
			}
			WithDrawProcessStateItem item =
				target.GetComponent<WithDrawProcessStateItem>();
			if (item == null)
			{
				item = target.AddComponent<WithDrawProcessStateItem>();
			}
			item.Initialize();
			return item;
		}

		private string GetLocalizedText(string key, string fallback)
		{
			if (LocalizationManager.Instance == null ||
				string.IsNullOrEmpty(key))
			{
				return fallback;
			}
			string value = LocalizationManager.Instance.GetText(key);
			return string.IsNullOrEmpty(value) || value == key
				? fallback
				: value;
		}

		private void ResetProgressFlow()
		{
			isClosing = false;
			if (showCompletionActionsCoroutine != null)
			{
				StopCoroutine(showCompletionActionsCoroutine);
				showCompletionActionsCoroutine = null;
			}
			time = 0f;
			completedStateCount = 0;
			isAnimating = visibleStateCount > 0;
			isProgressFlowCompleted = false;
			completionActionsVisible = false;
			if (Dec != null)
			{
				Dec.gameObject.SetActive(false);
			}

			for (int i = 0; i < processStates.Length; i++)
			{
				WithDrawProcessStateItem state = processStates[i];
				if (state == null)
				{
					continue;
				}
				state.SetCompleted(false);
				state.SetVisible(i == 0 && visibleStateCount > 0);
			}
			RefreshProcessStateLines();
			if (continueRoot != null)
			{
				continueRoot.SetActive(false);
			}
			if (closeRoot != null)
			{
				closeRoot.SetActive(false);
			}
		}

		private void UpdateProgressFlow(int newCompletedCount)
		{
			MahjongProductAnalytics.Track("f2_withdraw_processing_step", "completed_count", newCompletedCount, "stage", GetCurrentStageId(), "amount", GetCurrentWithdrawMoney(), "wallet_withdraw", IsLuckyWalletWithdraw());
			int targetCount = Mathf.Clamp(
				newCompletedCount, 0, GetMaxCompletableCount());
			while (completedStateCount < targetCount)
			{
				WithDrawProcessStateItem currentState =
					processStates[completedStateCount];
				if (currentState != null)
				{
					currentState.SetCompleted(true);
				}
				completedStateCount++;
				if (completedStateCount < visibleStateCount &&
					completedStateCount < processStates.Length)
				{
					WithDrawProcessStateItem nextState =
						processStates[completedStateCount];
					if (nextState != null)
					{
						nextState.SetVisible(true);
						nextState.SetCompleted(false);
					}
				}
			}
			RefreshProcessStateLines();
		}

		private void RefreshProcessStateLines()
		{
			int currentlyVisibleCount = ResolveCurrentlyVisibleStateCount();
			for (int i = 0; i < processStates.Length; i++)
			{
				WithDrawProcessStateItem state = processStates[i];
				if (state == null)
				{
					continue;
				}
				state.SetLineVisible(
					i < currentlyVisibleCount - 1 &&
					i < visibleStateCount - 1);
			}
		}

		internal void RefreshRuntimeGmPresentation()
		{
			ApplyProcessStateContents();
			RefreshProcessStateLines();
			RefreshMoneyText();
		}

		private int ResolveCurrentlyVisibleStateCount()
		{
			if (visibleStateCount <= 0)
			{
				return 0;
			}
			return Mathf.Min(visibleStateCount, completedStateCount + 1);
		}

		private int GetCurrentStageId()
		{
			if (dataModel != null && dataModel.Num > 0)
			{
				return dataModel.Num;
			}
			return GameManagerWZ.instance != null
				? Mathf.Max(1, GameManagerWZ.instance.getPlayerStage())
				: 1;
		}

		private double GetCurrentWithdrawMoney()
		{
			if (IsLuckyWalletWithdraw())
			{
				return WithdrawApiService.GetCachedWalletBalance();
			}
			if (dataModel != null && dataModel.Money > 0.0)
			{
				return dataModel.Money;
			}
			return GameManagerWZ.instance != null
				? GameManagerWZ.instance.GetCoin()
				: 0.0;
		}

		private bool IsLuckyWalletWithdraw()
		{
			return dataModel != null &&
				dataModel.Special == DataModel.SpecialLuckyWalletWithdraw;
		}

		private int GetVisibleStateCount()
		{
			if (!IsWithdrawButtonFlow())
			{
				return 4;
			}
			string[] waitingKeys =
				GetWithdrawStateContentKeysForStage(currentStageId);
			string[] finishedKeys =
				GetWithdrawFinishedStateContentKeysForStage(currentStageId);
			if (waitingKeys == null || finishedKeys == null)
			{
				return 3;
			}
			return Mathf.Clamp(
				Mathf.Max(waitingKeys.Length, finishedKeys.Length),
				1,
				ProcessStateCount);
		}

		private int GetMaxCompletableCount()
		{
			return IsWithdrawButtonFlow()
				? Mathf.Max(0, visibleStateCount - 1)
				: visibleStateCount;
		}

		private bool IsWithdrawButtonFlow()
		{
			return dataModel != null &&
				dataModel.Special == DataModel.SpecialNone;
		}

		private T FindByPath<T>(
			string path, string fallbackName = null) where T : Component
		{
			GameObject target = FindByPath(path, fallbackName);
			return target != null ? target.GetComponent<T>() : null;
		}

		private GameObject FindByPath(string path, string fallbackName = null)
		{
			Transform target = FindTransformByPath(path);
			if (target != null)
			{
				return target.gameObject;
			}
			if (string.IsNullOrEmpty(fallbackName))
			{
				return null;
			}
			Transform[] descendants = GetComponentsInChildren<Transform>(true);
			for (int i = 0; i < descendants.Length; i++)
			{
				if (descendants[i].name == fallbackName)
				{
					return descendants[i].gameObject;
				}
			}
			return null;
		}

		private Transform FindTransformByPath(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return null;
			}
			Transform target = transform.Find(path);
			if (target != null)
			{
				return target;
			}
			if (path.StartsWith(gameObject.name + "/", StringComparison.Ordinal))
			{
				return transform.Find(path.Substring(gameObject.name.Length + 1));
			}
			return null;
		}

		private void StartSubmitIfNeeded()
		{
			Framework2BusinessAnalytics.Withdraw("process_submit_check", dataModel, ShouldSubmitOnOpen() ? "submit_on_open" : "no_auto_submit");
			if (isSubmitStarted)
			{
				return;
			}
			isSubmitStarted = true;
			SetActionButtonsInteractable(false);
			StartCoroutine(SubmitWithdrawCoroutine());
		}

		private IEnumerator SubmitWithdrawCoroutine()
		{
			Framework2BusinessAnalytics.Withdraw("process_submit", dataModel);
			string queuedWalletOrderNo = GetQueuedWalletOrderNo();
			ClientWithdrawRequest request = BuildWithdrawRequest();
			if (request == null)
			{
				CleanupQueuedWalletOrder(queuedWalletOrderNo);
				HandleWithdrawSubmitError("Invalid withdrawal request.");
				yield break;
			}

			bool requestCompleted = false;
			WithdrawOrderData submittedOrder = null;
			string submitError = null;
			yield return WithdrawApiService.SubmitWithdraw(
				request,
				order =>
				{
					submittedOrder = order;
					requestCompleted = true;
				},
				error =>
				{
					submitError = error;
					requestCompleted = true;
				});
			while (!requestCompleted)
			{
				yield return null;
			}

			if (submittedOrder == null)
			{
				CleanupQueuedWalletOrder(queuedWalletOrderNo);
				HandleWithdrawSubmitError(
					string.IsNullOrEmpty(submitError)
						? "Withdrawal returned an empty order."
						: submitError);
				yield break;
			}

			currentOrder = submittedOrder;
			if (IsLuckyWalletWithdraw() &&
				!string.IsNullOrEmpty(queuedWalletOrderNo))
			{
				WithdrawApiService.ReplaceQueuedWalletOrder(
					queuedWalletOrderNo, submittedOrder);
			}
			if (dataModel != null)
			{
				dataModel.QueuedOrderNo = string.Empty;
				dataModel.SubmitOnOpen = false;
				dataModel.OrderNo = submittedOrder.OrderNo;
				dataModel.WithdrawType = submittedOrder.WithdrawType;
				dataModel.OrderStatus = submittedOrder.OrderStatus;
				dataModel.Receiver = submittedOrder.Receiver;
				dataModel.WalletFrozen = submittedOrder.WalletFrozen;
			}
			if (!IsLuckyWalletWithdraw() &&
				TryHandleImmediateOrderState(submittedOrder))
			{
				yield break;
			}
			Framework2BusinessAnalytics.Withdraw("process_submit_result", dataModel, "stored_locally");
			isSubmitCompleted = true;
			isSubmitSuccessful = true;
			if (isProgressFlowCompleted)
			{
				ShowCompletionActions();
			}
			if (IsLuckyWalletWithdraw())
			{
				yield return WithdrawApiService.QueryWalletBalance(
					_ =>
					{
						money = GetCurrentWithdrawMoney();
						RefreshMoneyText();
					});
			}
			SetActionButtonsInteractable(true);
		}

		private bool ShouldHandleImmediateOrderState()
		{
			return dataModel == null ||
				dataModel.Special != DataModel.SpecialLuckyWalletWithdraw;
		}

		private bool ShouldSubmitOnOpen()
		{
			return dataModel != null &&
				dataModel.SubmitOnOpen &&
				dataModel.Special != DataModel.SpecialLuckyWalletWithdraw;
		}

		private string GetQueuedWalletOrderNo()
		{
			if (dataModel == null)
			{
				return string.Empty;
			}
			if (!string.IsNullOrEmpty(dataModel.QueuedOrderNo))
			{
				return dataModel.QueuedOrderNo;
			}
			return !string.IsNullOrEmpty(dataModel.OrderNo) &&
				dataModel.OrderNo.StartsWith(
					"local-wallet-", StringComparison.OrdinalIgnoreCase)
				? dataModel.OrderNo
				: string.Empty;
		}

		private void CleanupQueuedWalletOrder(string queuedWalletOrderNo)
		{
			if (IsLuckyWalletWithdraw() &&
				!string.IsNullOrEmpty(queuedWalletOrderNo))
			{
				WithdrawApiService.RemoveQueuedWalletOrder(
					queuedWalletOrderNo);
			}
			if (dataModel != null)
			{
				dataModel.QueuedOrderNo = string.Empty;
				dataModel.SubmitOnOpen = false;
			}
		}

		private bool TryHandleImmediateOrderState(WithdrawOrderData order)
		{
			if (order == null)
			{
				HandleWithdrawSubmitError("提现结果为空");
				return true;
			}
			switch (order.OrderStatus)
			{
				case 2:
					HandleWithdrawSubmitError(
						"当前PayPal收款账号不存在，请重新填写正确的PayPal邮箱");
					return true;
				case 4:
					HandleWithdrawSubmitError(
						string.IsNullOrEmpty(order.ErrorMessage)
							? "提现失败，请稍后重试"
							: order.ErrorMessage);
					return true;
				case 5:
					HandleWithdrawSubmitError(
						"PayPal余额不足，等待平台补发");
					return true;
				default:
					return false;
			}
		}

		private ClientWithdrawRequest BuildWithdrawRequest()
		{
			string receiver = dataModel != null &&
				!string.IsNullOrEmpty(dataModel.Email)
				? dataModel.Email
				: GameManagerWZ.instance != null
					? GameManagerWZ.instance.getPlayerEmail()
					: string.Empty;
			bool retryWithOrder = dataModel != null &&
				dataModel.RetryWithdraw &&
				!string.IsNullOrEmpty(dataModel.OrderNo);
			bool isLuckyWalletWithdraw = dataModel != null &&
				dataModel.Special == DataModel.SpecialLuckyWalletWithdraw;
			if (string.IsNullOrEmpty(receiver) &&
				(!isLuckyWalletWithdraw || retryWithOrder))
			{
				return null;
			}
			string withdrawType = ResolveWithdrawType();
			return new ClientWithdrawRequest
				{
					OrderNo = retryWithOrder
						? dataModel.OrderNo
						: null,
					WithdrawType = withdrawType,
					Receiver = string.IsNullOrEmpty(receiver)
						? null
						: receiver,
					Amount = retryWithOrder
						? (double?)null
						: Math.Round(
							GetCurrentWithdrawMoney(),
							2,
							MidpointRounding.ToEven),
					Currency = WithdrawConstants.CurrencyUsd,
					Note = string.Equals(
						withdrawType,
						WithdrawConstants.WithdrawTypeWallet,
						StringComparison.OrdinalIgnoreCase)
						? "Wallet withdraw"
						: "First ad withdraw",
					EmailSubject = string.Empty,
					RecipientType =
						WithdrawConstants.GetCurrentRecipientType()
				};
		}

		private string ResolveWithdrawType()
		{
			if (dataModel != null &&
				!string.IsNullOrEmpty(dataModel.WithdrawType))
			{
				return dataModel.WithdrawType;
			}
			return IsLuckyWalletWithdraw()
				? WithdrawConstants.WithdrawTypeWallet
				: WithdrawConstants.WithdrawTypeFirstAd;
		}

		private void HandleWithdrawSubmitError(string error)
		{
			Framework2BusinessAnalytics.Withdraw("process_submit_error", dataModel, "failed");
			isSubmitCompleted = true;
			isSubmitSuccessful = false;
			isAnimating = false;
			if (!string.IsNullOrEmpty(error))
			{
				WithdrawApiService.LogPaymentNotice(
					"WithDrawProcess1", error, LogType.Warning);
			}
			if (!string.IsNullOrEmpty(error) &&
				(error.Contains("PayPal收款账号不存在") ||
				 error.Contains("PayPal收款账号不能为空")))
			{
				UIManager.instance?.ShowView(
					EUIType.PayPalErrorView, dataModel);
			}
			GameApp.viewManager?.Close(ViewId);
		}

		private void RefreshMoneyText()
		{
			if (moneyContent == null)
			{
				return;
			}
			GameManagerWZ manager = GameManagerWZ.instance;
			moneyContent.text = manager != null
				? manager.FormatRegionalValue(money)
				: "$" + money.ToString("F2");
		}

		private void SetActionButtonsInteractable(bool isInteractable)
		{
			if (continueButton != null)
			{
				continueButton.interactable = isInteractable;
			}
			if (closeButton != null)
			{
				closeButton.interactable = isInteractable;
			}
		}

		private void ShowCompletionActions()
		{
			if (completionActionsVisible ||
				showCompletionActionsCoroutine != null)
			{
				return;
			}
			showCompletionActionsCoroutine =
				StartCoroutine(ShowCompletionActionsDelayed());
		}

		private IEnumerator ShowCompletionActionsDelayed()
		{
			yield return new WaitForSeconds(CompletionActionsShowDelay);
			showCompletionActionsCoroutine = null;
			if (this != null && gameObject.activeInHierarchy)
			{
				ShowCompletionActionsImmediately();
			}
		}

		private void ShowCompletionActionsImmediately()
		{
			if (completionActionsVisible)
			{
				return;
			}
			completionActionsVisible = true;
			UpdateLastVisibleStateTextIfNeeded();
			if (Dec != null && IsWithdrawButtonFlow())
			{
				Dec.gameObject.SetActive(true);
			}
			if (continueRoot != null)
			{
				continueRoot.SetActive(true);
			}
			if (closeRoot != null)
			{
				closeRoot.SetActive(true);
			}
			SetActionButtonsInteractable(true);
		}

		public void OnButtonClick()
		{
			Framework2BusinessAnalytics.Withdraw("process_continue_click", dataModel);
			if (isClosing) return;
			isClosing = true;
			SetActionButtonsInteractable(false);
			if (GameManagerWZ.instance != null)
				GameManagerWZ.instance.StartSourceFlow(DelayedNextLevel());
			else StartCoroutine(DelayedNextLevel());
		}

		public void OnCloseBtnClick()
		{
			Framework2BusinessAnalytics.Withdraw("process_close_click", dataModel);
			OnButtonClick();
		}

		private IEnumerator DelayedNextLevel()
		{
			yield return new WaitForSeconds(0.2f);
			if (!ShouldCloseWithoutAdvance() &&
				WithdrawalTaskPresentation.ShouldResumeCompletedLevel(GameManagerWZ.instance, dataModel))
			{
				GameManagerWZ.instance.NextLevel();
			}
			GameApp.viewManager?.Close(ViewId);
		}

		private bool ShouldCloseWithoutAdvance()
		{
			if (dataModel != null &&
				dataModel.Special != DataModel.SpecialLuckyWalletWithdraw &&
				!dataModel.CloseWithoutAdvance)
			{
				return GameManagerWZ.instance == null;
			}
			return true;
		}

		private bool RequiresClickToSubmit()
		{
			return false;
		}

		private void UpdateLastVisibleStateTextIfNeeded()
		{
			int index = visibleStateCount - 1;
			if (index >= 0 && index < processStates.Length &&
				processStates[index] != null)
			{
				processStates[index].SetFinishedContentVisible(true);
			}
		}

		private string[] GetWithdrawStateContentKeysForStage(int stageId)
		{
			return IsWithdrawButtonFlow()
				? Stage3WithdrawStateContentKeys
				: OtherStageWithdrawStateContentKeys;
		}

		private string[] GetWithdrawFinishedStateContentKeysForStage(int stageId)
		{
			return IsWithdrawButtonFlow()
				? Stage3WithdrawFinishedStateContentKeys
				: OtherStageWithdrawFinishedStateContentKeys;
		}

		private string GetWaitingStateContent(
			int index, string fallbackContent)
		{
			string[] keys =
				GetWithdrawStateContentKeysForStage(currentStageId);
			string content = GetLocalizedStateContent(
				keys, index, fallbackContent);
			return ComposeQueueStateContentIfNeeded(index, content);
		}

		private string GetFinishedStateContent(
			int index, string fallbackContent)
		{
			string content = GetLocalizedStateContent(
				GetWithdrawFinishedStateContentKeysForStage(currentStageId),
				index,
				fallbackContent);
			return ComposeQueueStateContentIfNeeded(index, content);
		}

		private string ComposeQueueStateContentIfNeeded(
			int index, string content)
		{
			return ShouldPrefixQueueStateContent(index)
				? GetQueueingStatePrefix() + content
				: content;
		}

		private bool ShouldPrefixQueueStateContent(int index)
		{
			return IsWithdrawButtonFlow() &&
				currentStageId != 3 &&
				visibleStateCount > 0 &&
				index == visibleStateCount - 1;
		}

		private string GetQueueingStatePrefix()
		{
			return GetLocalizedText("3029", "In queue") + " ";
		}

		private string GetLocalizedStateContent(
			string[] keys, int index, string fallbackContent)
		{
			if (keys == null || index < 0 || index >= keys.Length)
			{
				return fallbackContent;
			}
			return GetLocalizedText(keys[index], fallbackContent);
		}

		private void Update()
		{
			if (!isAnimating)
			{
				return;
			}
			time += Time.deltaTime;
			UpdateProgressFlow(Mathf.FloorToInt(time / StepDuration));
			if (completedStateCount >= GetMaxCompletableCount())
			{
				isProgressFlowCompleted = true;
				ShowCompletionActions();
				isAnimating = false;
			}
		}
	}
}
