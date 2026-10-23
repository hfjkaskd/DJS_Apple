using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	public class WithDrawContinue : BaseViewWithDraw
	{
		private Text moneyContent;

		private Text levelContent;

		private GameObject panel;

		private DataModel dataModel;

		internal DataModel RuntimeGmDataModel => dataModel;

		internal Text RuntimeGmLevelContent => levelContent;

		internal GameObject RuntimeGmPanel => panel;

		protected override void OnAwake()
		{
			moneyContent = Find<Text>(
				"PanelA/PanelC/WithDrawItem/PanelA/TextUp");
			panel = Find("PanelA/PanelC");
			levelContent = Find<Text>(
				"PanelA/PanelC/WithDrawItem1/PanelA/TextDown");
			Button continueButton = Find<Button>("PanelA/Continue");
			if (continueButton != null)
			{
				continueButton.onClick.AddListener(
					onContinueBtnClick);
			}
		}

		protected override void HandleViewArgs(object[] args)
		{
			dataModel = args != null && args.Length > 0
				? args[0] as DataModel
				: null;
			if (dataModel == null)
			{
				return;
			}

			double money = dataModel.Money;
			var rewardEconomy = Framework2Host.Settings.economy.rewards;
			int completedCount = dataModel.Num;
			GameManagerWZ manager = GameManagerWZ.instance;

			if (panel != null)
			{
				for (int i = 0; i < panel.transform.childCount; i++)
				{
					Transform child = panel.transform.GetChild(i);
					WithDrawItem taskItem = child.GetComponent<WithDrawItem>();
					taskItem?.ResetStyle();
					Text taskText = child.Find("PanelA/TextDown")?.GetComponent<Text>();
					WithdrawalTaskPresentation.OwnDynamicText(taskText);
					if (taskText != null)
						taskText.text = WithdrawalTaskPresentation.Read(manager, i + 1).Objective(manager);
					Text valueText = child.Find("PanelA/TextUp")
						?.GetComponent<Text>();
					if (valueText != null)
					{
						if (i < 2)
						{
							valueText.text = manager != null
								? manager.FormatRegionalValue(rewardEconomy.earlyTaskDisplayedAmount)
								: "$" + rewardEconomy.earlyTaskDisplayedAmount.ToString("F2");
						}
						else if (i == 2)
						{
							valueText.text = manager != null
								? manager.FormatRegionalValue(rewardEconomy.taskRangeDisplayedMinimum) + "-" +
									manager.FormatRegionalValue(rewardEconomy.taskRangeDisplayedMaximum)
								: "$" + rewardEconomy.taskRangeDisplayedMinimum.ToString("0.##") + "-$" +
									rewardEconomy.taskRangeDisplayedMaximum.ToString("0.##");
						}
						else
						{
							valueText.text = manager != null
								? manager.FormatRegionalValue(money)
								: "$" + money.ToString("F2");
						}
					}

					if (i < completedCount)
					{
						WithDrawItem item =
							child.GetComponent<WithDrawItem>();
						if (item != null)
						{
							item.changeStyle(i + 1);
						}
					}
				}
			}

			if (GameManagerWZ.instance != null)
			{
				Framework2BusinessAnalytics.Withdraw("continue_page_debit_requested", dataModel);
				GameManagerWZ.instance.AddCoinTem(-(float)money);
			}

			List<HistoryModel> history =
				SPlayerPrefs.GetListTem<HistoryModel>(
					FacadePlayerPrefExtend.History);
			if (history == null)
			{
				return;
			}
			for (int i = 0; i < history.Count; i++)
			{
				HistoryModel item = history[i];
				if (item != null &&
					item.ID == dataModel.HistoryId)
				{
					item.Status = 1;
					break;
				}
			}
			SPlayerPrefs.SetListTem(
				FacadePlayerPrefExtend.History, history);
		}

		private void onContinueBtnClick()
		{
			Framework2BusinessAnalytics.Withdraw("continue_earning_click", dataModel);
			GameManagerWZ.instance.StartSourceFlow(DelayedNextLevel());
		}

		private IEnumerator DelayedNextLevel()
		{
			yield return new WaitForSeconds(0.2f);
			if (WithdrawalTaskPresentation.ShouldResumeCompletedLevel(GameManagerWZ.instance, dataModel))
			{
				GameManagerWZ.instance.NextLevel();
			}
			GameApp.viewManager?.Close(ViewId);
		}
	}
}
