using System;
using UnityEngine.UI;

namespace WZSDK
{
	public class WithDrawSuc : BaseViewWithDraw
	{
		private Text moneyContent;

		private double money;

		private double rewardAmount;

		private int level;

		private DataModel dataModel;

		internal double RuntimeGmRewardAmount => rewardAmount;

		internal Text RuntimeGmMoneyContent => moneyContent;

		protected override void OnAwake()
		{
			Button close = Find<Button>(
				"PanelA/bg/close");
			if (close != null)
			{
				close.onClick.AddListener(OnButtonClick);
			}
			moneyContent = Find<Text>(
				"PanelA/bg/content");
		}

		protected override void HandleViewArgs(object[] args)
		{
			if (args == null || args.Length == 0)
			{
				return;
			}
			dataModel = args[0] as DataModel;
			if (dataModel == null)
			{
				return;
			}
			money = dataModel.Money;
			level = dataModel.Level;
			rewardAmount = Math.Max(
				GameManagerWZ.Stage5CompensationTargetCoin -
					money,
				0d);
			GameManagerWZ manager = GameManagerWZ.instance;
			if (manager == null)
			{
				return;
			}
			if (moneyContent != null)
			{
				moneyContent.text =
					manager.FormatRegionalValue(rewardAmount);
			}
			MahjongProductAnalytics.Track("f2_compensation_success_presentation", "amount", rewardAmount, "already_granted", dataModel.Special == DataModel.SpecialRewardAlreadyGranted, "balance", manager.GetCoin());
			if (dataModel.Special ==
				DataModel.SpecialRewardAlreadyGranted)
			{
				return;
			}
			manager.AddCoin((float)rewardAmount);
		}

		public void OnButtonClick()
		{
			Framework2BusinessAnalytics.Withdraw("compensation_success_close", dataModel, "open_progress", (int)EUIType.BloatProgressView);
			ViewManager viewManager = GameApp.viewManager;
			if (viewManager == null)
			{
				return;
			}
			viewManager.Close(ViewId);
			if (UIManager.instance != null)
			{
				UIManager.instance.ShowView(
					EUIType.BloatProgressView);
			}
		}
	}
}
