using UnityEngine.UI;

namespace WZSDK
{
	public class RewardMessageDia : BaseViewWithDraw
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

			LevelRewardStatic.RewardItem reward =
				new LevelRewardStatic.RewardItem
				{
					type = (int)ELuckySpinRewardType.Gem,
					amount = 5f
				};
			UIManager.instance?.ShowView(
				EUIType.WelcomeView,
				reward,
				3);
		}
	}
}
