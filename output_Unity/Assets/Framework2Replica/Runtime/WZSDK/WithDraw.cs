using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	public class WithDraw : BaseViewWithDraw
	{
		private static double MinimumCashOutAmount => Framework2Host.Settings.economy.rewards.minimumCashOutAmount;

		private Text titleContent;

		private Text moneyContent;

		private Text numContent;

		private Text withdrawProcess;

		private Text Tips;

		private Text CurLuckyCoin;

		private Image process;

		private GameObject panelE;

		private int lastLevel = -1;
		private int lastStage = -1;
		private int lastAds = -1;
		private int lastAdOwner = -1;
		private int lastCycle = -1;
		private double lastCoin = double.NaN;
		private LanguageType lastLanguage;
		private WithdrawalTaskPresentation task;

		private int num;

		private double money;

		private DataModel dataModel;

		private bool flag;

		private Button CashOut;

		protected override void OnAwake()
		{
			titleContent = Find<Text>("Panel/title");
			moneyContent = Find<Text>(
				"Panel/PanelA/PanelD/PanelE/content");
			numContent = Find<Text>(
				"Panel/PanelA/PanelD/Process/num");
			withdrawProcess = Find<Text>(
				"Panel/PanelB/withdrawProcess");
			Tips = Find<Text>("Panel/PanelC/Tips");
			CurLuckyCoin = Find<Text>(
				"Panel/PanelC/CurLuckyCoin");
			process = Find<Image>(
				"Panel/PanelA/PanelD/Process/Fill");
			panelE = Find("PanelE");
			CashOut = Find<Button>("Panel/PanelC/CashOut");

			Button recordButton = Find<Button>(
				"Panel/PanelB/RecordBtn");
			if (recordButton != null)
			{
				recordButton.onClick.AddListener(
					OnRecordBtnClickHandle);
			}

			Button withdrawButton = Find<Button>(
				"Panel/PanelB/WithDraw");
			if (withdrawButton != null)
			{
				withdrawButton.onClick.AddListener(OnButtonClick);
			}

			if (CashOut != null)
			{
				CashOut.onClick.AddListener(onCashOutBtnClick);
			}

			Button closeButton = Find<Button>("Panel/close");
			if (closeButton != null)
			{
				closeButton.onClick.AddListener(onCloseBtnClick);
			}
		}

		protected override void HandleViewArgs(object[] args)
		{
			dataModel = args != null && args.Length > 0 ? args[0] as DataModel : null;
			num = dataModel != null ? dataModel.Num : 0;
			panelE?.SetActive(false);
			RefreshTaskPresentation();
		}

		public override void InitView()
		{
			RefreshTaskPresentation();
			RefreshLuckyWalletBalance();
		}

		private void Update()
		{
			GameManagerWZ manager = GameManagerWZ.instance;
			if (!isShow || manager == null) return;
			LanguageType language = LocalizationManager.Instance != null
				? LocalizationManager.Instance.GetCurrentLanguage() : default(LanguageType);
			if (lastLevel != manager.currentLv || lastStage != manager.currentStage ||
				lastAds != manager.currentStageAdWatchCount || lastAdOwner != manager.currentStageAdWatchOwnerStage ||
				lastCycle != manager.stage5CurrentCycle || lastCoin != manager.GetCoin() ||
				lastLanguage != language)
				RefreshTaskPresentation();
		}

		private void RefreshTaskPresentation()
		{
			GameManagerWZ manager = GameManagerWZ.instance;
			task = WithdrawalTaskPresentation.Read(manager, num);
			Framework2BusinessAnalytics.TaskPresentation("withdraw_home", num, task, isShow);
			flag = dataModel != null && task.IsComplete;
			if (numContent != null) numContent.text = task.Progress(manager);
			if (process != null) process.fillAmount = task.Fill;
			if (withdrawProcess != null) withdrawProcess.text = task.Objective(manager);
			if (titleContent != null)
				titleContent.text = num > 0 ? string.Format(CultureInfo.CurrentCulture,
					GetLocalizedText("2062", "This is your <color=#FFEB3B>#{0}</color> withdrawal"), num) : string.Empty;
			money = manager != null ? manager.GetCoin() : 0d;
			if (moneyContent != null)
				moneyContent.text = manager != null ? manager.FormatRegionalValue(money) : string.Empty;
			if (manager != null)
			{
				lastLevel = manager.currentLv;
				lastStage = manager.currentStage;
				lastAds = manager.currentStageAdWatchCount;
				lastAdOwner = manager.currentStageAdWatchOwnerStage;
				lastCycle = manager.stage5CurrentCycle;
				lastCoin = money;
				lastLanguage = LocalizationManager.Instance != null
					? LocalizationManager.Instance.GetCurrentLanguage() : default(LanguageType);
			}
			RefreshLuckyWalletSection();
		}

		internal void RefreshLuckyWalletPresentation()
		{
			RefreshTaskPresentation();
			RefreshLuckyWalletBalance();
		}

		private void RefreshLuckyWalletSection()
		{
			GameManagerWZ gameManager = GameManagerWZ.instance;
			if (gameManager == null)
			{
				return;
			}

			bool unlocked =
				gameManager.currentLv >= GameDefines.LuckyWalletUnlockLevel;
			MahjongProductAnalytics.State("f2_lucky_wallet_entry_state", "withdraw_wallet_entry", "unlocked", unlocked, "unlock_level", GameDefines.LuckyWalletUnlockLevel, "level", gameManager.currentLv);
			if (!unlocked)
			{
				if (Tips != null)
				{
					string template = GetLocalizedText(
						"3070",
						"Clear {0} more levels to unlock the Lucky Wallet");
					Tips.text = string.Format(
						CultureInfo.CurrentCulture,
						template,
						Mathf.Max(0, GameDefines.LuckyWalletUnlockLevel - gameManager.currentLv));
					Tips.gameObject.SetActive(true);
				}
				if (CashOut != null)
				{
					CashOut.gameObject.SetActive(false);
				}
				if (CurLuckyCoin != null)
				{
					CurLuckyCoin.gameObject.SetActive(false);
				}
				return;
			}

			if (Tips != null)
			{
				Tips.gameObject.SetActive(false);
			}
			if (CashOut != null)
			{
				CashOut.gameObject.SetActive(true);
			}
			if (CurLuckyCoin != null)
			{
				CurLuckyCoin.gameObject.SetActive(true);
				CurLuckyCoin.text = FormatLuckyWalletBalance(
					WithdrawApiService.GetCachedWalletBalance());
			}
		}

		private void RefreshLuckyWalletBalance()
		{
			GameManagerWZ gameManager = GameManagerWZ.instance;
			if (gameManager == null ||
				gameManager.currentLv < GameDefines.LuckyWalletUnlockLevel)
			{
				return;
			}

			StartCoroutine(WithdrawApiService.QueryWalletBalance(
				wallet =>
				{
					double balance = wallet != null
						? wallet.Balance
						: WithdrawApiService.GetCachedWalletBalance();
					if (CurLuckyCoin != null)
					{
						CurLuckyCoin.text =
							FormatLuckyWalletBalance(balance);
					}
				},
				error =>
				{
					UnityEngine.Debug.LogError(
						"[WithDraw] Wallet balance request failed: " +
						error);
					if (CurLuckyCoin != null)
					{
						CurLuckyCoin.text =
							FormatLuckyWalletBalance(
								WithdrawApiService
									.GetCachedWalletBalance());
					}
				},
				"USD"));
		}

		private string FormatLuckyWalletBalance(double balance)
		{
			GameManagerWZ gameManager = GameManagerWZ.instance;
			return gameManager != null
				? gameManager.FormatRegionalValue(balance)
				: "$" + balance.ToString(
					"F2", CultureInfo.CurrentCulture);
		}

		public void OnButtonClick()
		{
			Framework2BusinessAnalytics.Withdraw("main_withdraw_click", dataModel);
			GameManagerWZ gameManager = GameManagerWZ.instance;
			if (gameManager == null || dataModel == null)
			{
				return;
			}

			RefreshTaskPresentation();
			dataModel.Name = gameManager.getPlayerName();
			dataModel.Num = num;
			dataModel.Phone = gameManager.getPlayerPhone();
			dataModel.Email = gameManager.getPlayerEmail();
			dataModel.Index = gameManager.getPlayerIndex();
			dataModel.Special = DataModel.SpecialNone;
			dataModel.Money = gameManager.GetCoin();
			dataModel.UseProcessViewOnMethodExit = num == 3;

			if (!flag)
			{
				Framework2BusinessAnalytics.Withdraw("main_withdraw_check", dataModel, "task_not_complete");
				if (panelE != null)
				{
					panelE.SetActive(true);
					StartCoroutine(DelayedAction());
				}
				return;
			}

			// The real button can be pressed before the asynchronously loaded
			// tutorial proxy is ready. Advancing the view must also dismiss that
			// request, otherwise its late mask points at this destroyed button.
			Tutorial.CancelPendingPresentation();
			gameManager.TrySyncStageProgress(num);
			ViewType destination;
			if (dataModel.Type == 2)
			{
				destination = ViewType.WithDrawProcess1;
			}
			else
			{
				dataModel.Index = 3;
				destination = string.IsNullOrEmpty(dataModel.Email)
					? ViewType.WithDrawMethod
					: ViewType.WithDrawProcess1;
			}

			Framework2BusinessAnalytics.Withdraw("main_withdraw_next", dataModel, "eligible", (int)destination);
			GameApp.viewManager?.Open(destination, dataModel);
			GameApp.viewManager?.Close(ViewId);
		}

		public void onCloseBtnClick()
		{
			Framework2BusinessAnalytics.Withdraw("withdraw_home_close", dataModel);
			Tutorial.CancelPendingPresentation();
			GameManagerWZ manager = GameManagerWZ.instance;
			bool continueCompletedLevel =
				dataModel != null && dataModel.CloseType == 1 &&
				manager != null &&
				manager.currentState == GameManagerWZ.GAME_STATE.FINISH &&
				!GameDefines.ifIAA &&
				manager.currentLv <= GameDefines.CloseInt;
			GameApp.viewManager?.Close(ViewId);
			if (continueCompletedLevel)
			{
				manager.NextLevel();
			}
		}

		public void onCashOutBtnClick()
		{
			Framework2BusinessAnalytics.Withdraw("lucky_wallet_entry", dataModel, "requested", (int)EUIType.GameBalancView);
			UIManager.instance?.ShowView(EUIType.GameBalancView);
		}

		private void OnRecordBtnClickHandle()
		{
			Framework2BusinessAnalytics.Withdraw("withdraw_history_entry", dataModel, "requested", (int)ViewType.WithDrawHistory);
			GameManagerWZ gameManager = GameManagerWZ.instance;
			if (gameManager == null)
			{
				return;
			}

			DataModel historyData = new DataModel
			{
				Name = gameManager.getPlayerName(),
				Phone = gameManager.getPlayerPhone(),
				Email = gameManager.getPlayerEmail(),
				Level = gameManager.currentLv - 1,
				Money = gameManager.GetCoin(),
				CloseType = 1
			};
			GameApp.viewManager?.Open(
				ViewType.WithDrawHistory, historyData);
		}

		private IEnumerator DelayedAction()
		{
			yield return new WaitForSeconds(2f);
			if (panelE != null)
			{
				panelE.SetActive(false);
			}
		}

		private static string GetLocalizedText(
			string key, string fallback)
		{
			return LocalizationManager.Instance != null
				? LocalizationManager.Instance.GetText(key)
				: fallback;
		}
	}
}
