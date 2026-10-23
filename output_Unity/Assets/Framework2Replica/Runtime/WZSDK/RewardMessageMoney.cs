using UnityEngine.UI;

namespace WZSDK
{
	public class RewardMessageMoney : BaseViewWithDraw
	{
		protected override void OnAwake()
		{
			Button close = Find<Button>("Panel/close");
			if (close != null)
			{
				close.onClick.AddListener(onCloseBtnClick);
			}

			Button continueButton = Find<Button>("Bottom/continue");
			if (continueButton != null)
			{
				continueButton.onClick.AddListener(onCloseBtnClick);
			}
		}

		protected override void HandleViewArgs(object[] args)
		{
		}

		private void onCloseBtnClick()
		{
			GameApp.viewManager?.Close(ViewId);
			GameApp.viewManager?.Open(ViewType.RewardMessageDia);
		}
	}
}
