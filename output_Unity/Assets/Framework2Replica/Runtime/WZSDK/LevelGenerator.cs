using UnityEngine;

namespace WZSDK
{
	public class LevelGenerator : MonoBehaviour
	{
		public bool NewGamePlayIsTrue;

		private GameManagerWZ Gm => GameManagerWZ.instance;

		public void InitLvGen()
		{
			NewGamePlayIsTrue = false;
			ExecuteStageSettings();
		}

		private void CheckAndShowTutorial()
		{
			NewGamePlayIsTrue = false;
		}

		public void ExecuteStageSettings()
		{
			if (GameDefines.ifIAA)
			{
				return;
			}

			int currentStage = Gm.currentStage;
			Debug.LogError("当前阶段" + currentStage);

			DataModel dataModel = new DataModel
			{
				Level = Gm.currentLv
			};
			dataModel = new DataModel
			{
				Money = Gm.GetCoin()
			};

			bool showLuckyWalletUnlock = false;
			if (Gm.currentLv >= GameDefines.LuckyWalletUnlockLevel)
			{
				if (PlayerPrefs.GetInt(
						FacadePlayerPrefExtend.HasShownLuckyWalletAtLevel,
						0) == 0)
				{
					showLuckyWalletUnlock = true;
				}
				else if (UserDataManager.Instance != null &&
					UserDataManager.Instance.IsLuckyRewardLevel(Gm.currentLv))
				{
					UIManager.instance.ShowView(EUIType.LuckyTips);
					return;
				}
			}

			if (currentStage == 1)
			{
				GameApp.viewManager.Open(ViewType.kaiju_ui_1);
			}

			if (currentStage == 3)
			{
				UIManager.instance.ShowView(EUIType.WithdrawMissionView);
			}

			if (currentStage == 4)
			{
				GameApp.viewManager.Open(ViewType.kaiju_ui_4, dataModel);
			}
			else if (currentStage == 5)
			{
				GameApp.viewManager.Open(ViewType.kaiju_ui_5, dataModel);
			}

			if (!showLuckyWalletUnlock)
			{
				return;
			}

			PlayerPrefs.SetInt(
				FacadePlayerPrefExtend.HasShownLuckyWalletAtLevel,
				1);
			PlayerPrefs.Save();
			UIManager.instance.ShowView(EUIType.LuckyWalletView);
		}
	}
}
