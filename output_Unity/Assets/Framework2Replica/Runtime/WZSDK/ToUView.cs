using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	public class ToUView : BaseView
	{
		private Button exitBtn;

		private ScrollRect scrollRect;

		private bool isEventBound;

		private bool hasSavedScrollPosition;

		private bool isRestoringScrollPosition;

		private Coroutine restoreScrollPositionCoroutine;

		private float savedVerticalPosition = 1f;

		private void Awake()
		{
			Transform exitTransform = transform.Find("Plane/ExitBtn");
			exitBtn = exitTransform != null
				? exitTransform.GetComponent<Button>()
				: null;

			Transform scrollTransform = transform.Find("Plane/Bg2/Scroll View");
			scrollRect = scrollTransform != null
				? scrollTransform.GetComponent<ScrollRect>()
				: null;
		}

		public override void InitView()
		{
			BindButtonEvent();
			RestoreScrollPosition();
		}

		private void BindButtonEvent()
		{
			if (isEventBound)
			{
				RemoveButtonEvent();
			}

			if (exitBtn == null)
			{
				Transform exitTransform = transform.Find("Plane/ExitBtn");
				exitBtn = exitTransform != null
					? exitTransform.GetComponent<Button>()
					: null;
			}

			if (scrollRect == null)
			{
				Transform scrollTransform = transform.Find("Plane/Bg2/Scroll View");
				scrollRect = scrollTransform != null
					? scrollTransform.GetComponent<ScrollRect>()
					: null;
			}

			if (exitBtn == null)
			{
				Debug.LogError("ToUView close button not found: Plane/ExitBtn");
				return;
			}

			exitBtn.onClick.AddListener(CloseBtn);
			if (scrollRect != null)
			{
				scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
			}
			isEventBound = true;
		}

		private void CloseBtn()
		{
			UIManager.instance.CloseView(EUIType.ToUView);
		}

		public override void Close(params object[] args)
		{
			SaveScrollPosition();
			base.Close(args);
		}

		private void OnScrollValueChanged(Vector2 value)
		{
			if (!isRestoringScrollPosition)
			{
				SaveScrollPosition();
			}
		}

		private void SaveScrollPosition()
		{
			if (scrollRect != null)
			{
				savedVerticalPosition = scrollRect.verticalNormalizedPosition;
				hasSavedScrollPosition = true;
			}
		}

		private void RestoreScrollPosition()
		{
			if (scrollRect == null || !hasSavedScrollPosition)
			{
				return;
			}

			StopRestoreScrollPosition();
			restoreScrollPositionCoroutine = StartCoroutine(
				RestoreScrollPositionCoroutine());
		}

		private IEnumerator RestoreScrollPositionCoroutine()
		{
			isRestoringScrollPosition = true;
			yield return null;
			Canvas.ForceUpdateCanvases();
			if (scrollRect != null)
			{
				scrollRect.verticalNormalizedPosition = savedVerticalPosition;
				scrollRect.velocity = Vector2.zero;
			}
			isRestoringScrollPosition = false;
			restoreScrollPositionCoroutine = null;
		}

		private void StopRestoreScrollPosition()
		{
			if (restoreScrollPositionCoroutine != null)
			{
				StopCoroutine(restoreScrollPositionCoroutine);
				restoreScrollPositionCoroutine = null;
			}
			isRestoringScrollPosition = false;
		}

		private void RemoveButtonEvent()
		{
			if (exitBtn != null)
			{
				exitBtn.onClick.RemoveListener(CloseBtn);
			}
			if (scrollRect != null)
			{
				scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
			}
			isEventBound = false;
		}

		public override void Start()
		{
		}

		public override void Update()
		{
		}

		private void OnDisable()
		{
			StopRestoreScrollPosition();
		}

		private void OnDestroy()
		{
			StopRestoreScrollPosition();
			RemoveButtonEvent();
		}
	}
}
