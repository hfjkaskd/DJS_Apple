using System.Collections.Generic;
using System.Threading.Tasks;
using ArrowOut.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	public class WithDrawItem : MonoBehaviour
	{
		private const string CompletedSpritePath =
			"Prefab/UI/Withdraw/hui_bj";

		private const string CompletedColor = "#4F4F4F";

		private Image originalBackground;
		private Text originalMoneyText;
		private Text originalStateText;
		private Sprite originalSprite;
		private Color originalMoneyColor;
		private Color originalStateColor;
		private int presentationVersion;

		private Framework2AppleSkinPresenter appleSkin;

		private void Awake()
		{
			appleSkin = GetComponent<Framework2AppleSkinPresenter>();
			originalBackground = transform.Find("PanelA")?.GetComponent<Image>();
			originalMoneyText = transform.Find("PanelA/TextUp")?.GetComponent<Text>();
			originalStateText = transform.Find("PanelA/TextDown")?.GetComponent<Text>();
			if (originalBackground != null) originalSprite = originalBackground.sprite;
			if (originalMoneyText != null) originalMoneyColor = originalMoneyText.color;
			if (originalStateText != null) originalStateColor = originalStateText.color;
		}

		public void ResetStyle()
		{
			presentationVersion++;
			appleSkin?.SetTaskComplete(false);
			if (originalBackground != null) originalBackground.sprite = originalSprite;
			if (originalMoneyText != null) originalMoneyText.color = originalMoneyColor;
			if (originalStateText != null) originalStateText.color = originalStateColor;
		}

		public async void changeStyle(int stage)
		{
			int version = ++presentationVersion;
			Image headPic = transform.Find("PanelA")
				?.GetComponent<Image>();
			Text moneyText = transform.Find("PanelA/TextUp")
				?.GetComponent<Text>();
			Text stateText = transform.Find("PanelA/TextDown")
				?.GetComponent<Text>();

			Task<Sprite> completedTask = appleSkin != null
				? Task.FromResult(Framework2AppleSkinAssets.LoadSprite("inset"))
				: AddressableAssetService.LoadAsync<Sprite>(CompletedSpritePath);

			double money = 0.0;
			List<HistoryModel> history =
				SPlayerPrefs.GetListTem<HistoryModel>(
					FacadePlayerPrefExtend.History);
			if (history != null)
			{
				for (int i = 0; i < history.Count; i++)
				{
					HistoryModel item = history[i];
					if (item != null && item.Stage == stage)
					{
						money = item.Money;
						break;
					}
				}
			}

			Color completedColor;
			if (!ColorUtility.TryParseHtmlString(
					CompletedColor, out completedColor))
			{
				return;
			}
			if (moneyText != null)
			{
				moneyText.color = appleSkin != null ? originalMoneyColor : completedColor;
				GameManagerWZ manager = GameManagerWZ.instance;
				moneyText.text = manager != null
					? manager.FormatRegionalValue(money)
					: "$" + money.ToString("F2");
			}
			if (stateText != null)
			{
				stateText.color = appleSkin != null ? originalStateColor : completedColor;
				stateText.text = LocalizationManager.Instance != null
					? LocalizationManager.Instance.GetText("2013")
					: "Withdrawal completed";
			}

			Sprite completed = await completedTask;
			if (this != null && version == presentationVersion && headPic != null && completed != null)
			{
				if (appleSkin != null) appleSkin.SetTaskComplete(true);
				else headPic.sprite = completed;
			}
		}
	}
}
