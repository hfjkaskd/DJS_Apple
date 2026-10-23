using DG.Tweening;
using UnityEngine;

namespace WZSDK
{
	public class SplashScreen : MonoBehaviour
	{
		public CanvasGroup canvasGroup;

		private void Start()
		{
			DOTween.To(
					() => canvasGroup.alpha,
					x => canvasGroup.alpha = x,
					0f,
					0.5f)
				.SetDelay(1f)
				.SetEase(Ease.Linear)
				.OnComplete(() =>
				{
					canvasGroup.alpha = 0f;
					canvasGroup.interactable = false;
					canvasGroup.blocksRaycasts = false;
				});
		}

		private void Update()
		{
		}
	}
}
