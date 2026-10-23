using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	public class LocalContentErrorTips : MonoBehaviour
	{
		private const string ErrorTextKey = "3083";

		private const string ErrorFallbackText =
			"Local content could not be prepared. Please restart";

		private static readonly string[] DotSuffixes = { ".", "..", "..." };

		public Text text;

		private int count;

		private bool isEnd;

		private bool hasStarted;

		private Coroutine changeContentCoroutine;

		private void Start()
		{
			hasStarted = true;
			isEnd = false;
			StartContentUpdates();
		}

		private void OnEnable()
		{
			if (hasStarted && !isEnd)
			{
				StartContentUpdates();
			}
		}

		private void OnDisable()
		{
			StopContentUpdates();
		}

		private void OnDestroy()
		{
			StopContentUpdates();
		}

		public void ChangeContent()
		{
			if (isEnd)
			{
				return;
			}

			string content = GetLocalizedErrorText();
			string suffix = DotSuffixes[count % DotSuffixes.Length];
			if (text != null)
			{
				text.text = content + suffix;
			}
			count++;
		}

		public void CanelShow()
		{
			isEnd = true;
			if (text != null)
			{
				text.text = string.Empty;
			}
			StopContentUpdates();
		}

		private void StartContentUpdates()
		{
			StopContentUpdates();
			if (!isActiveAndEnabled || isEnd)
			{
				return;
			}
			changeContentCoroutine = StartCoroutine(ChangeContentRepeatedly());
		}

		private IEnumerator ChangeContentRepeatedly()
		{
			while (!isEnd)
			{
				ChangeContent();
				yield return new WaitForSeconds(0.5f);
			}
			changeContentCoroutine = null;
		}

		private void StopContentUpdates()
		{
			if (changeContentCoroutine == null)
			{
				return;
			}
			StopCoroutine(changeContentCoroutine);
			changeContentCoroutine = null;
		}

		private string GetLocalizedErrorText()
		{
			LocalizationManager manager = LocalizationManager.Instance;
			if (manager != null)
			{
				string localized = manager.GetText(ErrorTextKey);
				if (!string.IsNullOrEmpty(localized) && localized != ErrorTextKey)
				{
					return localized;
				}
			}
			return ErrorFallbackText;
		}
	}
}
