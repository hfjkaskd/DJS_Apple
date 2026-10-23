using System.Collections;
using ArrowOut.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	public class WithDrawConfirm : BaseViewWithDraw
	{
		private const string VenmoSpritePath =
			"Prefab/UI/Pay/venmo";

		private const string ZelleSpritePath =
			"Prefab/UI/Pay/zelle";

		private GameObject Content;

		private Image method;

		private DataModel dataModel;

		protected override void OnAwake()
		{
			Content = Find("PanelA/PanelC/phone");
			Button returnButton =
				Find<Button>("PanelA/ReEnter");
			if (returnButton != null)
			{
				returnButton.onClick.AddListener(
					OnReturnClick);
			}
			Button confirm = Find<Button>("PanelA/Confirm");
			if (confirm != null)
			{
				confirm.onClick.AddListener(
					OnCloseBtnClick);
			}
			Button close = Find<Button>("PanelA/close");
			if (close != null)
			{
				close.onClick.AddListener(
					OnCloseBtnClick);
			}
			method = Find<Image>("PanelA/method");
		}

		protected override async void HandleViewArgs(object[] args)
		{
			dataModel = args != null && args.Length > 0
				? args[0] as DataModel
				: null;
			if (dataModel == null)
			{
				dataModel = new DataModel();
			}

			Text contentText =
				Content != null
					? Content.GetComponent<Text>()
					: null;
			if (contentText != null)
			{
				contentText.text =
					!string.IsNullOrEmpty(dataModel.Phone)
						? dataModel.Phone
						: dataModel.Email;
			}
			if (dataModel.Index == 3 || method == null)
			{
				return;
			}

			string spritePath = dataModel.Index == 2
				? ZelleSpritePath
				: VenmoSpritePath;
			DataModel requestedModel = dataModel;
			Sprite sprite =
				await AddressableAssetService.LoadAsync<Sprite>(
					spritePath);
			if (this != null &&
				method != null &&
				object.ReferenceEquals(dataModel, requestedModel) &&
				sprite != null)
			{
				method.sprite = sprite;
			}
		}

		public void OnReturnClick()
		{
			Framework2BusinessAnalytics.Withdraw("receiver_reenter", dataModel, "requested", (int)ViewType.WithDrawMethod);
			GameApp.viewManager?.Open(
				ViewType.WithDrawMethod, dataModel);
			GameApp.viewManager?.Close(ViewId);
		}

		public void OnCloseBtnClick()
		{
			Framework2BusinessAnalytics.Withdraw("receiver_confirm_continue", dataModel, ShouldCloseWithoutAdvance() ? "close_only" : ShouldOpenProcessView() ? "open_process" : "continue");
			if (ShouldCloseWithoutAdvance())
			{
				GameApp.viewManager?.Close(ViewId);
				return;
			}
			if (dataModel == null)
			{
				return;
			}
			if (dataModel.Special !=
					DataModel.SpecialLuckyWalletWithdraw &&
				!dataModel.RetryWithdraw)
			{
				if (dataModel.CloseType == 1)
				{
					GameManagerWZ.instance.StartSourceFlow(DelayedNextLevel());
					return;
				}
				GameApp.viewManager?.Close(ViewId);
				GameApp.viewManager?.Open(
					ViewType.WithDrawContinue,
					dataModel);
				return;
			}

			GameApp.viewManager?.Close(ViewId);
			GameApp.viewManager?.Open(
				ViewType.WithDrawProcess1, dataModel);
		}

		private bool ShouldOpenProcessView()
		{
			return dataModel != null &&
				(dataModel.Special ==
					DataModel.SpecialLuckyWalletWithdraw ||
				 dataModel.RetryWithdraw);
		}

		private bool ShouldCloseWithoutAdvance()
		{
			if (dataModel != null &&
				!dataModel.CloseWithoutAdvance &&
				dataModel.Special !=
					DataModel.SpecialLuckyWalletWithdraw &&
				dataModel.Special !=
					DataModel.SpecialWithdrawMissionBindEmail &&
				!dataModel.RetryWithdraw)
			{
				return GameManagerWZ.instance == null;
			}
			return true;
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
