using DG.Tweening;
using UnityEngine;

namespace WZSDK
{
	public class AutoHandScale : MonoBehaviour
	{
		public bool ifAwake = true;

		public float STime = 1f;

		public float Size = 1.25f;

		private void Start()
		{
			transform.localScale = Vector3.one;
			if (ifAwake)
			{
				StartScale();
			}
		}

		private void StartScale()
		{
			transform.DOScale(Size, STime * 0.5f).SetLoops(-1, LoopType.Yoyo);
		}

		public void Reset()
		{
		}
	}
}
