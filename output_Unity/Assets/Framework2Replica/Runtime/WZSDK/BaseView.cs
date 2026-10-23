using System;
using DG.Tweening;
using UnityEngine;

namespace WZSDK
{
	public abstract class BaseView : UIManagedViewBase
	{
		public CanvasGroup canvasGroup;

		[HideInInspector]
		public bool isShow;

		[Header("Animation Settings")]
		public bool useAnimations = true;
		internal bool IsClosing { get; private set; }

		public abstract void Start();

		public abstract void Update();

		public abstract void InitView();

		public override bool IsShow()
		{
			return isShow;
		}

		public override void InitUi()
		{
		}

		public override void InitData()
		{
			MarkInitialized();
		}

		private bool ShouldPlayAnimation()
		{
			if (!useAnimations)
			{
				return false;
			}
			if (UIManager.instance == null)
			{
				Debug.LogWarning(
					"UIManager instance is unavailable while checking view animation.");
				return true;
			}

			string viewName = gameObject.name;
			if (viewName.Contains("(Clone)"))
			{
				viewName = viewName.Replace("(Clone)", string.Empty);
			}
			return UIManager.noAnimationViewNames == null ||
				!UIManager.noAnimationViewNames.Contains(viewName);
		}

		public virtual void ShowView()
		{
			ShowView(Array.Empty<object>());
		}

		public virtual void ShowView(params object[] args)
		{
			IsClosing = false;
			if (args != null && args.Length > 0)
			{
				HandleViewArgs(args);
			}

			if (!ShouldPlayAnimation())
			{
				SetCanvasState(true);
				InitializeShownView();
				return;
			}

			RectTransform target = transform as RectTransform;
			if (target != null)
			{
				target.DOKill(false);
				target.localScale = Vector3.zero;
			}

			if (canvasGroup != null)
			{
				canvasGroup.alpha = 1f;
				canvasGroup.interactable = true;
				canvasGroup.blocksRaycasts = true;
			}
			ShowAnim(target, CompleteShowAnimation);
			InitializeShownView();
		}

		public virtual void HideView()
		{
			if (IsClosing) return;
			IsClosing = true;
            MahjongAnalyticsView.Closed(gameObject, "hide");
			isShow = false;
			if (canvasGroup != null) canvasGroup.interactable = false;
			// The source show/hide tweens otherwise both drive the same scale.
			// Do not complete the old callback or jump back to full size.
			transform.DOKill(false);
			if (!ShouldPlayAnimation())
			{
				SetCanvasState(false);
				return;
			}

			HideAnim(transform as RectTransform, () => SetCanvasState(false));
		}

		protected virtual void HandleViewArgs(object[] args)
		{
		}

		internal virtual void RefreshRuntimeGmPresentation()
		{
		}

		public override void Open(params object[] args)
		{
			ShowView(args);
		}

		public override void Close(params object[] args)
		{
			HideView();
		}

		public override void SetVisible(bool value)
		{
			gameObject.SetActive(value);
			isShow = value;
		}

		protected void ShowAnim(RectTransform target, Action successAction = null)
		{
			if (target == null)
			{
				successAction?.Invoke();
				return;
			}

			target.localScale = Vector3.zero;
			target.DOScale(Vector3.one, GameDefines.ShowAnimTime)
				.SetEase(Ease.OutBack)
				.OnComplete(() => successAction?.Invoke());
		}

		protected void HideAnim(RectTransform target, Action successAction = null)
		{
			if (target == null)
			{
				successAction?.Invoke();
				return;
			}

			target.DOScale(Vector3.zero, GameDefines.HideAnimTime)
				.SetEase(Ease.InBack)
				.OnComplete(() =>
				{
					target.localScale = Vector3.zero;
					successAction?.Invoke();
				});
		}

		private void CompleteShowAnimation()
		{
            MahjongAnalyticsView.Snapshot(gameObject, "entrance_completed");
			if (canvasGroup != null)
			{
				canvasGroup.blocksRaycasts = true;
			}

			isShow = true;
		}

		private void InitializeShownView()
		{
			MarkInitialized();
			InitView();
			LocalizationManager.Instance?.ApplyView(gameObject);
            MahjongAnalyticsView.Observe(gameObject, "f2." + gameObject.name.Replace("(Clone)", ""));
		}

		private void SetCanvasState(bool visible)
		{
			IsClosing = false;
			if (canvasGroup != null)
			{
				canvasGroup.alpha = visible ? 1f : 0f;
				canvasGroup.interactable = visible;
				canvasGroup.blocksRaycasts = visible;
			}

			isShow = visible;
		}
	}
}
