using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	[RequireComponent(typeof(Text))]
	public class LocalizedText : MonoBehaviour
	{
		[Header("文本多语言配置")]
		public string textId;

		private Text textComponent;

		private void Start()
		{
			textComponent = GetComponent<Text>();
			if (LocalizationManager.Instance != null)
			{
				LocalizationManager.Instance.OnLanguageChanged += UpdateText;
			}
			UpdateText();
		}

		private void OnDestroy()
		{
			if (LocalizationManager.Instance != null)
			{
				LocalizationManager.Instance.OnLanguageChanged -= UpdateText;
			}
		}

		private void UpdateText()
		{
			if (textComponent == null) textComponent = GetComponent<Text>();
			LocalizationManager.Instance?.ApplyFont(textComponent);
			if (textComponent != null && LocalizationManager.Instance != null &&
				!string.IsNullOrWhiteSpace(textId))
			{
				textComponent.text = LocalizationManager.Instance.GetText(textId);
			}
		}

		public void SetTextId(string newTextId)
		{
			textId = newTextId;
			UpdateText();
		}
	}
}
