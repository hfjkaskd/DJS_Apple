using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	public class WithDrawProcess : BaseViewWithDraw
	{
		private GameObject panel2;

		private GameObject panel3;

		private GameObject buttonContinue;

		private GameObject image;

		private GameObject imageA;

		private GameObject imageB;

		private GameObject wait;

		private GameObject wait1;

		private GameObject wait2;

		private double time;

		private bool isStart = true;

		private double money;

		private int level;

		private Text moneyContent;

		private Text Num;

		private DataModel dataModel;

		internal DataModel RuntimeGmDataModel => dataModel;

		internal Text RuntimeGmMoneyContent => moneyContent;

		internal Text RuntimeGmNumberText => Num;

		protected override void OnAwake()
		{
			Button continueButton = Find<Button>("PanelA/Continue");
			if (continueButton != null)
			{
				continueButton.onClick.AddListener(OnButtonClick);
			}

			Button closeButton = Find<Button>("PanelA/close");
			if (closeButton != null)
			{
				closeButton.onClick.AddListener(OnCloseBtnClick);
			}

			moneyContent = Find<Text>("PanelA/PanelC/content");
			Num = Find<Text>("Panel/titleContent");
			panel2 = Find("PanelA/PanelB/Panel2");
			panel3 = Find("PanelA/PanelB/Panel3");
			image = Find("PanelA/PanelB/Panel1/Image");
			wait = Find("PanelA/PanelB/Panel1/wait");
			imageA = Find("PanelA/PanelB/Panel2/ImageA");
			wait1 = Find("PanelA/PanelB/Panel2/wait1");
			imageB = Find("PanelA/PanelB/Panel3/ImageB");
			wait2 = Find("PanelA/PanelB/Panel3/wait2");
			buttonContinue = Find("PanelA/Continue");
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
			Num.text = string.Format(
				LocalizationManager.Instance.GetText("2062"),
				dataModel.Num);
		}

		public void OnButtonClick()
		{
			GameManagerWZ.instance.StartSourceFlow(DelayedNextLevel());
		}

		public void OnCloseBtnClick()
		{
			GameApp.viewManager.Close(ViewId);
			GameManagerWZ.instance.StartSourceFlow(DelayedNextLevel());
		}

		private IEnumerator DelayedNextLevel()
		{
			yield return new WaitForSeconds(0.2f);
			GameManagerWZ.instance.NextLevel();
			GameApp.viewManager.Close(ViewId);
		}

		private void Update()
		{
			if (!isStart)
			{
				return;
			}

			double elapsed = time;
			if (elapsed > 1.0)
			{
				wait.SetActive(false);
				image.SetActive(true);
				panel2.SetActive(true);
			}

			if (elapsed > 2.0)
			{
				wait1.SetActive(false);
				imageA.SetActive(true);
				panel3.SetActive(true);
			}

			if (elapsed > 3.0)
			{
				wait2.SetActive(false);
				imageB.SetActive(true);
				buttonContinue.SetActive(true);
				isStart = false;
			}

			time = elapsed + Time.deltaTime;
		}
	}
}
