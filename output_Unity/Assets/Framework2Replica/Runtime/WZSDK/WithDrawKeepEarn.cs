using UnityEngine.UI;

namespace WZSDK
{
	public class WithDrawKeepEarn : BaseViewWithDraw
	{
		private Text moneyContent;

		private double money;

		private int level;

		private DataModel dataModel;

		internal DataModel RuntimeGmDataModel => dataModel;

		internal Text RuntimeGmMoneyContent => moneyContent;

		protected override void OnAwake()
		{
			Find<Button>("PanelA/Continue").onClick.AddListener(OnButtonClick);
			Find<Button>("PanelA/close").onClick.AddListener(OnCloseBtnClick);
			moneyContent = Find<Text>("PanelA/PanelC/content");
		}

		protected override void HandleViewArgs(object[] args)
		{
			dataModel = (DataModel)args[0];
			money = dataModel.Money;
			level = dataModel.Level;
			GameManagerWZ manager = GameManagerWZ.instance;
			moneyContent.text = manager != null
				? manager.FormatRegionalValue(money)
				: "$" + money.ToString();
		}

		public void OnButtonClick()
		{
			GameApp.viewManager.Open(ViewType.WithDrawContinue, dataModel);
			GameApp.viewManager.Close(ViewId);
		}

		public void OnCloseBtnClick()
		{
			GameApp.viewManager.Close(ViewId);
		}
	}
}
