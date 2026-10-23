using UnityEngine.UI;

namespace WZSDK
{
	public class WithDrawQues : BaseViewWithDraw
	{
		protected override void OnAwake()
		{
			Button confirm = Find<Button>("PanelA/Confirm");
			if (confirm != null)
			{
				confirm.onClick.AddListener(OnButtonClick);
			}
			Button close = Find<Button>("PanelA/close");
			if (close != null)
			{
				close.onClick.AddListener(OnCloseBtnClick);
			}
		}

		public void OnButtonClick()
		{
			GameApp.viewManager?.Close(ViewId);
		}

		public void OnCloseBtnClick()
		{
			GameApp.viewManager?.Close(ViewId);
		}
	}
}
