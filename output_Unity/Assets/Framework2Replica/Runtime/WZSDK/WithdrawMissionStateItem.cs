using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	[DisallowMultipleComponent]
	public class WithdrawMissionStateItem : MonoBehaviour
	{
		private GameObject finish;

		private GameObject unfinished;

		private Text countText;

		private void Awake()
		{
			CacheReferences();
		}

		public void Initialize()
		{
			CacheReferences();
		}

		public void SetFinished(bool isFinished)
		{
			CacheReferences();
			if (finish != null)
			{
				finish.SetActive(isFinished);
			}
			if (unfinished != null)
			{
				unfinished.SetActive(!isFinished);
			}
		}

		public void SetCount(int count)
		{
			CacheReferences();
			if (countText != null)
			{
				countText.text = count.ToString();
			}
		}

		public void SetState(int count, bool isFinished)
		{
			SetCount(count);
			SetFinished(isFinished);
		}

		private void CacheReferences()
		{
			if (finish == null)
			{
				Transform target = transform.Find("Finish");
				finish = target != null ? target.gameObject : null;
			}
			if (unfinished == null)
			{
				Transform target = transform.Find("Unfinished");
				unfinished = target != null ? target.gameObject : null;
			}
			if (countText == null)
			{
				Transform target = transform.Find("Count");
				countText = target != null ? target.GetComponent<Text>() : GetComponentInChildren<Text>(true);
			}
		}
	}
}
