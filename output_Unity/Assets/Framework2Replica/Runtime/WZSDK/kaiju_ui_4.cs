using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace WZSDK
{
	public class kaiju_ui_4 : BaseViewWithDraw
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

			float target = 0f;
			Dictionary<string, object> stageData =
				GameManagerWZ.instance.GetStageData(4);
			if (stageData != null && stageData.ContainsKey("Value"))
			{
				float.TryParse(
					stageData["Value"] as string,
					out target);
			}

			double remaining = target - Money;
			content1.text = string.Format(
				LocalizationManager.Instance.GetText("2100"),
				GameManagerWZ.instance.FormatRegionalValue(remaining),
				GameManagerWZ.instance.FormatRegionalValue(
					(double)target));
		}

		public void onButtonClick()
		{
			GameApp.viewManager.Close(ViewId);
			FacadeEffectExtend.PlayCongratulationEffectHandle?.Invoke();
		}
	}
}
