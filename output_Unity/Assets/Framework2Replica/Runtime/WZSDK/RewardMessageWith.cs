using UnityEngine.UI;

namespace WZSDK
{
	public class RewardMessageWith : BaseViewWithDraw
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

			GameManagerWZ manager = GameManagerWZ.instance;
			Text range =
				Find<Text>("Panel/PanelA/contentB");
			if (range != null)
			{
				range.text = manager != null
					? manager.FormatRegionalValue(200d) + "—" +
						manager.FormatRegionalValue(1000d)
					: "$200—$1000";
			}
			Text maximum =
				Find<Text>("Panel/PanelA/contentC");
			if (maximum != null)
			{
				maximum.text = manager != null
					? manager.FormatRegionalValue(1000d)
					: "$1000";
			}
		}

		protected override void HandleViewArgs(object[] args)
		{
		}

		private void onCloseBtnClick()
		{
			GameApp.viewManager?.Close(ViewId);
			GameApp.viewManager?.Open(ViewType.RewardMessageMoney);
		}
	}
}
