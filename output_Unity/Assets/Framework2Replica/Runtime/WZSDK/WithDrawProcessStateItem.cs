using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	[DisallowMultipleComponent]
	public class WithDrawProcessStateItem : MonoBehaviour
	{
		private Text content1;

		private GameObject line;

		private GameObject wait;

		private GameObject Finsh;

		private string waitingContent;

		private string finishedContent;

		private bool isCompleted;

		private bool useFinishedContent;

		private void Awake()
		{
			CacheReferences();
		}

		public void Initialize()
		{
			CacheReferences();
		}

		public void SetContents(string waitingValue, string finishedValue)
		{
			CacheReferences();
			waitingContent = waitingValue ?? string.Empty;
			finishedContent = string.IsNullOrEmpty(finishedValue)
				? waitingContent
				: finishedValue;
			RefreshContent();
		}

		public void SetContent(string value)
		{
			SetContents(value, value);
		}

		public void SetCompleted(bool isCompleted)
		{
			CacheReferences();
			this.isCompleted = isCompleted;
			useFinishedContent = isCompleted;
			if (wait != null)
			{
				wait.SetActive(!isCompleted);
			}
			if (Finsh != null)
			{
				Finsh.SetActive(isCompleted);
			}
			RefreshContent();
		}

		public void SetFinishedContentVisible(bool isVisible)
		{
			useFinishedContent = isVisible;
			RefreshContent();
		}

		public void ResetState()
		{
			SetCompleted(false);
		}

		public void SetVisible(bool isVisible)
		{
			gameObject.SetActive(isVisible);
		}

		public void SetLineVisible(bool isVisible)
		{
			CacheReferences();
			if (line != null)
			{
				line.SetActive(isVisible);
			}
		}

		private void CacheReferences()
		{
			if (content1 == null)
			{
				Transform target = FindDescendant(transform, "content1");
				content1 = target != null ? target.GetComponent<Text>() : null;
			}
			if (line == null)
			{
				Transform target = FindDescendant(transform, "line");
				line = target != null ? target.gameObject : null;
			}
			if (wait == null)
			{
				Transform target = FindDescendant(transform, "wait");
				wait = target != null ? target.gameObject : null;
			}
			if (Finsh == null)
			{
				Transform target = FindDescendant(transform, "Finsh");
				Finsh = target != null ? target.gameObject : null;
			}
		}

		private void RefreshContent()
		{
			if (content1 != null)
			{
				content1.text = useFinishedContent
					? finishedContent
					: waitingContent;
			}
		}

		private static Transform FindDescendant(
			Transform parent, string childName)
		{
			if (parent == null)
			{
				return null;
			}
			Transform direct = parent.Find(childName);
			if (direct != null)
			{
				return direct;
			}
			for (int i = 0; i < parent.childCount; i++)
			{
				Transform found = FindDescendant(
					parent.GetChild(i), childName);
				if (found != null)
				{
					return found;
				}
			}
			return null;
		}
	}
}
