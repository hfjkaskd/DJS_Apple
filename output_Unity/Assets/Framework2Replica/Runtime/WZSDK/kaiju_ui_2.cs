using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace WZSDK
{
	public class kaiju_ui_2 : BaseViewWithDraw
	{
		private Text titleContent;

		private Text greenBallContent;

		private int level;

		private DataModel dataModel;

		protected override void OnAwake()
		{
			Find<Button>("continue").onClick.AddListener(onButtonClick);
			greenBallContent =
				Find<Text>("dial/Panel/PanalIn/Panel/greenBall/content");
		}

		protected override void HandleViewArgs(object[] args)
		{
			dataModel = (DataModel)args[0];
			level = dataModel.Level;

			Dictionary<string, object> stageData =
				UserDataManager.Instance.getStageData(3);
			if (stageData == null)
			{
				return;
			}

			int value = int.Parse(stageData["Value"] as string);
			int type = int.Parse(stageData["Type"] as string);
			string textId = type == 4 ? "2148" : "2101";
			greenBallContent.text = string.Format(
				LocalizationManager.Instance.GetText(textId),
				value);
		}

		public void onButtonClick()
		{
			GameApp.viewManager.Close(ViewId);
			FacadeEffectExtend.PlayCongratulationEffectHandle?.Invoke();
		}
	}
}
