using System;
using UnityEngine.UI;

namespace WZSDK
{
	public class kaiju_ui_5 : BaseViewWithDraw
	{
		private Text content1;

		private Text Num1;

		private double Money;

		private DataModel dataModel;

		protected override void OnAwake()
		{
			Find<Button>("continue").onClick.AddListener(onButtonClick);
			Find<Button>("dial/Panel/GenerateMoreBtn").onClick
				.AddListener(onButtonClick);
			Find<Button>("dial/Panel/bg/close").onClick
				.AddListener(onButtonClick);
			content1 = Find<Text>("dial/Panel/content1");
		}

		protected override void HandleViewArgs(object[] args)
		{
			dataModel = (DataModel)args[0];
			Money = dataModel.Money;

			int type;
			int value;
			if (!GameManagerWZ.instance.TryGetStageRequirement(
					5,
					out type,
					out value))
			{
				return;
			}

			if (type == 3)
			{
				int remainingDays = Math.Max(
					0,
					value - GameManagerWZ.instance.stage5CurrentCycle);
				content1.text = string.Format(
					LocalizationManager.Instance.GetText("2098"),
					remainingDays);
				return;
			}

			double remainingMoney = Math.Max(0d, value - Money);
			content1.text = string.Format(
				LocalizationManager.Instance.GetText("2100"),
				GameManagerWZ.instance.FormatRegionalValue(
					remainingMoney),
				GameManagerWZ.instance.FormatRegionalValue(
					(double)value));
		}

		public void onButtonClick()
		{
			GameApp.viewManager.Close(ViewId);
			FacadeEffectExtend.PlayCongratulationEffectHandle?.Invoke();
		}
	}
}
