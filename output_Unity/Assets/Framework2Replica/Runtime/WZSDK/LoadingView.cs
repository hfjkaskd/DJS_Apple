using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	public class LoadingView : BaseView
	{
		private const int OverlaySortingOrder = 32000;

		public Image progressImage;

		public Text progressText;

		public Text LoadingText;

		private Tween progressTween;

		private Tween backgroundScaleTween;

		private Sequence logoFloatSequence;

		private RectTransform backgroundRect;

		private Vector3 backgroundStartScale;

		private RectTransform logoRect;

		private Vector2 logoStartAnchoredPosition;

		private Vector3 logoStartEulerAngles;

		private Vector3 logoStartScale;

		private Tween loadingTextTween;

		private string[] loadingTexts = { "LOADING", "LOADING.", "LOADING..", "LOADING..." };

		private int loadingTextIndex;

		public override void Start()
		{
			EnsureLoadingOverlayOnTop();
			CacheBackground();
			CacheLogo();
		}

		public override void Update()
		{
		}

		public override void InitView()
		{
			EnsureLoadingOverlayOnTop();
			ResetProgress();
			PlayBackgroundIntro();
			PlayLogoFloat();
			StartLoadingTextAnimation();
		}

		private void StartLoadingTextAnimation()
		{
			StopLoadingTextAnimation();
			loadingTextIndex = 0;
			if (LoadingText == null || loadingTexts == null || loadingTexts.Length == 0)
			{
				return;
			}
			LoadingText.text = loadingTexts[0];
			loadingTextTween = DOTween.To(() => loadingTextIndex, value => loadingTextIndex = value, loadingTexts.Length - 1, 1.5f)
				.SetEase(Ease.Linear)
				.OnUpdate(() =>
				{
					if (LoadingText != null && loadingTexts != null && (uint)loadingTextIndex < (uint)loadingTexts.Length)
					{
						LoadingText.text = loadingTexts[loadingTextIndex];
					}
				})
				.OnComplete(() =>
				{
					loadingTextIndex = 0;
					if (LoadingText != null && loadingTexts != null && loadingTexts.Length > 0)
					{
						LoadingText.text = loadingTexts[0];
					}
					StartLoadingTextAnimation();
				});
		}

		private void StopLoadingTextAnimation()
		{
			loadingTextTween?.Kill();
			loadingTextTween = null;
		}

		public void StartFill(float duration, Action onComplete = null)
		{
			StopFill();
			if (progressImage == null)
			{
				return;
			}
			progressImage.fillAmount = 0f;
			progressTween = progressImage.DOFillAmount(1f, duration)
				.SetEase(Ease.Linear)
				.OnUpdate(UpdateProgressText)
				.OnComplete(() => onComplete?.Invoke());
		}

		public void StartSimulatedProgress(float duration = 8f, float targetProgress = 0.92f)
		{
			StopFill();
			if (progressImage == null)
			{
				return;
			}
			progressImage.fillAmount = 0f;
			UpdateProgressText();
			progressTween = progressImage.DOFillAmount(Mathf.Clamp01(targetProgress), Mathf.Max(0.1f, duration))
				.SetEase(Ease.Linear)
				.OnUpdate(UpdateProgressText);
		}

		public void CompleteProgress(Action onComplete = null, float duration = 0.25f)
		{
			StopFill();
			if (progressImage == null)
			{
				onComplete?.Invoke();
				return;
			}
			float remaining = Mathf.Clamp01(1f - progressImage.fillAmount);
			float completionDuration = remaining <= 0f
				? 0f
				: Mathf.Max(0.05f, remaining * duration);
			progressTween = progressImage.DOFillAmount(1f, completionDuration)
				.SetEase(Ease.OutCubic)
				.OnUpdate(UpdateProgressText)
				.OnComplete(() => onComplete?.Invoke());
		}

		public void ResetProgress()
		{
			StopFill();
			if (progressImage != null)
			{
				progressImage.fillAmount = 0f;
				UpdateProgressText();
			}
		}

		public void SetProgress(float value)
		{
			if (progressImage != null)
			{
				progressImage.fillAmount = Mathf.Clamp01(value);
			}
			UpdateProgressText();
		}

		private void UpdateProgressText()
		{
			if (progressText != null && progressImage != null)
			{
				progressText.text = Mathf.RoundToInt(progressImage.fillAmount * 100f) + "%";
			}
		}

		private void StopFill()
		{
			progressTween?.Kill();
			progressTween = null;
		}

		private void EnsureLoadingOverlayOnTop()
		{
			Canvas overlayCanvas = GetComponent<Canvas>();
			if (overlayCanvas == null)
			{
				overlayCanvas = gameObject.AddComponent<Canvas>();
			}

			overlayCanvas.overrideSorting = true;
			overlayCanvas.sortingLayerID = SortingLayer.NameToID("Default");
			overlayCanvas.sortingOrder = OverlaySortingOrder;
			if (GetComponent<GraphicRaycaster>() == null)
			{
				gameObject.AddComponent<GraphicRaycaster>();
			}

			transform.SetAsLastSibling();
		}

		private void CacheLogo()
		{
			if (logoRect != null)
			{
				return;
			}
			Transform logo = transform.Find("Top/logo");
			if (logo != null)
			{
				logoRect = logo as RectTransform;
				logoStartAnchoredPosition = logoRect.anchoredPosition;
				logoStartEulerAngles = logoRect.localEulerAngles;
				logoStartScale = logoRect.localScale;
			}
		}

		private void CacheBackground()
		{
			if (backgroundRect != null)
			{
				return;
			}
			Transform background = transform.Find("bg");
			if (background != null)
			{
				backgroundRect = background as RectTransform;
				backgroundStartScale = backgroundRect.localScale;
			}
		}

		private void PlayBackgroundIntro()
		{
			CacheBackground();
			if (backgroundRect == null)
			{
				return;
			}
			StopBackgroundIntro();
			backgroundRect.localScale = backgroundStartScale * 1.5f;
			backgroundScaleTween = backgroundRect.DOScale(backgroundStartScale, 1.2f).SetEase(Ease.OutCubic);
		}

		private void PlayLogoFloat()
		{
			CacheLogo();
			if (logoRect == null)
			{
				return;
			}
			StopLogoFloat();
			logoRect.anchoredPosition = logoStartAnchoredPosition;
			logoRect.localEulerAngles = logoStartEulerAngles;
			logoRect.localScale = logoStartScale;
			logoFloatSequence = DOTween.Sequence()
				.Append(logoRect.DOScale(logoStartScale * 1.12f, 1.2f).SetEase(Ease.OutBack))
				.Append(logoRect.DOScale(logoStartScale * 0.92f, 1.2f).SetEase(Ease.InOutBack))
				.SetLoops(-1, LoopType.Yoyo);
		}

		private void StopLogoFloat()
		{
			logoFloatSequence?.Kill();
			logoFloatSequence = null;
			if (logoRect != null)
			{
				logoRect.anchoredPosition = logoStartAnchoredPosition;
				logoRect.localEulerAngles = logoStartEulerAngles;
				logoRect.localScale = logoStartScale;
			}
		}

		private void StopBackgroundIntro()
		{
			backgroundScaleTween?.Kill();
			backgroundScaleTween = null;
			if (backgroundRect != null)
			{
				backgroundRect.localScale = backgroundStartScale;
			}
		}

		public void Close()
		{
			StopFill();
			StopBackgroundIntro();
			StopLogoFloat();
			StopLoadingTextAnimation();
			HideView();
		}

		private void OnDestroy()
		{
			StopFill();
			StopBackgroundIntro();
			StopLogoFloat();
			StopLoadingTextAnimation();
		}
	}
}
