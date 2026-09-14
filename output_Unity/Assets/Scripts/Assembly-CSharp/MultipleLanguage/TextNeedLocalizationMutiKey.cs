using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MultipleLanguage
{
	public class TextNeedLocalizationMutiKey : MonoBehaviour
	{
		public string key;

		public char separator;

		private TextMeshProUGUI text;

		private Text oldText;

		private void OnEnable()
		{
			BMNFNJFCPHG.OnLanguageSwitch += UpdateText;
			UpdateText();
		}

		private void OnDisable()
		{
			BMNFNJFCPHG.OnLanguageSwitch -= UpdateText;
		}

		private string ResolveText(string keyStr)
		{
			if (string.IsNullOrEmpty(keyStr))
			{
				return string.Empty;
			}
			char sep = separator == '\0' ? '|' : separator;
			string[] keys = keyStr.Split(sep);
			var sb = new System.Text.StringBuilder();
			for (int i = 0; i < keys.Length; i++)
			{
				sb.Append(OJEEJGGLNPC.Instance.GetText(keys[i].Trim()));
			}
			return sb.ToString();
		}

		private void UpdateText()
		{
			if (string.IsNullOrEmpty(key))
			{
				return;
			}
			string value = ResolveText(key);
			if (text == null)
			{
				text = GetComponent<TextMeshProUGUI>();
			}
			if (text != null)
			{
				text.text = value;
				return;
			}
			if (oldText == null)
			{
				oldText = GetComponent<Text>();
			}
			if (oldText != null)
			{
				oldText.text = value;
			}
		}
	}
}
