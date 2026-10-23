using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	public class LevelItem : MonoBehaviour
	{
		public GameObject Ng1;

		public GameObject Ng2;

		public Text WLevelText;

		public GameObject tips;

		public GameObject Completed;

		public GameObject SmallLevelGroup;

		public Text SmallLevelTxt;

		public void SetLevel(int level, int currentLevel)
		{
			SetAllFalse();
			bool showTips = ShouldShowTips(level, currentLevel);

			if (level < currentLevel)
			{
				if (Completed != null)
				{
					Completed.SetActive(true);
				}
				if (WLevelText != null)
				{
					WLevelText.gameObject.SetActive(false);
				}
				if (SmallLevelGroup != null)
				{
					SmallLevelGroup.SetActive(false);
				}
				return;
			}

			bool isCurrentLevel = level == currentLevel;
			bool isInSmallLevelGroup =
				level >= GameDefines.SmallLevelGroupStart &&
				level <= GameDefines.SmallLevelGroupEnd;
			if (isCurrentLevel && isInSmallLevelGroup)
			{
				if (SmallLevelGroup != null)
				{
					SmallLevelGroup.SetActive(true);
				}
				if (SmallLevelTxt != null)
				{
					SmallLevelTxt.text = string.Format(
						"{0}/{1}",
						level - GameDefines.SmallLevelGroupStart + 2,
						10);
				}
				SetWorldLevelText(GameDefines.SmallLevelGroupStart - 1);
				if (tips != null)
				{
					tips.SetActive(true);
				}
				if (Ng2 != null)
				{
					Ng2.SetActive(true);
				}
				return;
			}

			SetWorldLevelText(GetDisplayLevel(level));
			if (SmallLevelGroup != null)
			{
				SmallLevelGroup.SetActive(false);
			}
			if (tips != null)
			{
				tips.SetActive(showTips);
			}
			GameObject state = isCurrentLevel ? Ng2 : Ng1;
			if (state != null)
			{
				state.SetActive(true);
			}
		}

		private bool ShouldShowTips(int level, int currentLevel)
		{
			if (level == 1 || level == 2 || level == 8)
			{
				return true;
			}
			return currentLevel >= GameDefines.SmallLevelGroupStart &&
				currentLevel <= GameDefines.SmallLevelGroupEnd;
		}

		private int GetDisplayLevel(int level)
		{
			if (level < GameDefines.SmallLevelGroupStart)
			{
				return level;
			}
			if (level <= GameDefines.SmallLevelGroupEnd)
			{
				return GameDefines.SmallLevelGroupStart - 1;
			}
			return GameDefines.SmallLevelGroupStart +
				level -
				GameDefines.SmallLevelGroupEnd -
				1;
		}

		private void SetAllFalse()
		{
			if (Ng1 != null)
			{
				Ng1.SetActive(false);
			}
			if (Ng2 != null)
			{
				Ng2.SetActive(false);
			}
			if (Completed != null)
			{
				Completed.SetActive(false);
			}
			if (SmallLevelGroup != null)
			{
				SmallLevelGroup.SetActive(false);
			}
			if (tips != null)
			{
				tips.SetActive(false);
			}
		}

		private bool IsLevelCompleted(int level, int currentLevel)
		{
			return level < currentLevel;
		}

		private void SetWorldLevelText(int displayLevel)
		{
			if (WLevelText == null)
			{
				return;
			}

			string format =
				LocalizationManager.Instance.GetText("1004");
			WLevelText.text = string.Format(format, displayLevel);
			WLevelText.gameObject.SetActive(true);
		}
	}
}
