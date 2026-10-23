using UnityEngine.UI;

namespace WZSDK
{
	public class WithDrawSucMessage : BaseViewWithDraw
	{
		private int level;

		private int num;

		private double money;

		private DataModel dataModel;

		protected override void OnAwake()
		{
			Find<Button>("continue").onClick.AddListener(OnButtonClick);
		}

		protected override void HandleViewArgs(object[] args)
		{
			dataModel = (DataModel)args[0];
			level = dataModel.Level;
			num = dataModel.Num;
			money = dataModel.Money;
		}

		public void OnButtonClick()
		{
			GameManagerWZ manager = GameManagerWZ.instance;
			GameApp.viewManager?.Close(ViewId);
			if (manager != null &&
				(manager.currentStage == 1 || manager.currentStage == 2))
			{
				_ = Tutorial.OpenWithdrawTutorialAsync(dataModel);
			}
			else
			{
				GameApp.viewManager?.Open(ViewType.WithDraw, dataModel);
			}
		}
	}
}
