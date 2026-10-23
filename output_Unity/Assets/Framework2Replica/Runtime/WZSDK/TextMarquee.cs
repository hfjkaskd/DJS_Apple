using System;
using System.Collections;
using System.Threading.Tasks;
using ArrowOut.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	public class TextMarquee : MonoBehaviour
	{
		public float scrollDuration = 10f;

		public float startX = 1500f;

		public float resetOffset = -1600f;

		private RectTransform textRect;

		public Text Marque1;

		public Image icon1;

		public Text Marque2;

		public Image icon2;

		private bool isPaused;

		private Vector2 pausedPosition;

		private Coroutine scrollCoroutine;

		private int contentLoadVersion;

		public Action onScrollComplete;

		private void Awake()
		{
			textRect = GetComponent<RectTransform>();
		}

		private void OnEnable()
		{
		}

		private void OnDisable()
		{
			contentLoadVersion++;
			StopScrolling();
		}

		public async void SetMarqueeText(string barrage1, string barrage2)
		{
			int loadVersion = ++contentLoadVersion;
			if (Marque1 != null)
			{
				Marque1.text = barrage1;
			}
			if (Marque2 != null)
			{
				Marque2.text = barrage2;
			}

			string[] iconPaths =
			{
				"UI/Marquee/paypay_s",
				"UI/Marquee/venmo_s",
				"UI/Marquee/zelle_s"
			};
			int firstIconIndex = UnityEngine.Random.Range(0, iconPaths.Length);
			int secondIconIndex;
			do
			{
				secondIconIndex = UnityEngine.Random.Range(0, iconPaths.Length);
			}
			while (secondIconIndex == firstIconIndex && iconPaths.Length > 1);

			ResetPosition();
			StartScrolling();

			Task<Sprite> firstIconTask =
				AddressableAssetService.LoadAsync<Sprite>(iconPaths[firstIconIndex]);
			Task<Sprite> secondIconTask =
				AddressableAssetService.LoadAsync<Sprite>(iconPaths[secondIconIndex]);
			Sprite[] loadedIcons =
				await Task.WhenAll(firstIconTask, secondIconTask);
			if (this == null || loadVersion != contentLoadVersion || !isActiveAndEnabled)
			{
				return;
			}
			if (icon1 != null && loadedIcons[0] != null)
			{
				icon1.sprite = loadedIcons[0];
			}
			if (icon2 != null && loadedIcons[1] != null)
			{
				icon2.sprite = loadedIcons[1];
			}
		}

		private void ResetPosition()
		{
			if (textRect == null)
			{
				textRect = GetComponent<RectTransform>();
			}
			if (textRect != null)
			{
				textRect.anchoredPosition =
					new Vector2(startX, textRect.anchoredPosition.y);
			}
		}

		private void StartScrolling()
		{
			StopScrolling();
			GameManagerWZ manager = GameManagerWZ.instance;
			if (manager != null)
			{
				scrollCoroutine = manager.StartCoroutine(ScrollCoroutine());
			}
		}

		private void StopScrolling()
		{
			if (scrollCoroutine != null)
			{
				StopCoroutine(scrollCoroutine);
				scrollCoroutine = null;
			}
		}

		private IEnumerator ScrollCoroutine()
		{
			float elapsedTime = 0f;
			if (textRect == null)
			{
				yield break;
			}

			Vector2 startPos =
				new Vector2(startX, textRect.anchoredPosition.y);
			Vector2 endPos =
				new Vector2(resetOffset, textRect.anchoredPosition.y);
			while (elapsedTime < scrollDuration)
			{
				if (!isPaused)
				{
					textRect.anchoredPosition = Vector2.Lerp(
						startPos,
						endPos,
						Mathf.Clamp01(elapsedTime / scrollDuration));
					elapsedTime += Time.deltaTime;
				}
				else if (textRect != null)
				{
					pausedPosition = textRect.anchoredPosition;
				}
				yield return null;
			}

			gameObject.SetActive(false);
			onScrollComplete?.Invoke();
		}

		public void Pause()
		{
			if (!isPaused)
			{
				isPaused = true;
				if (textRect != null)
				{
					pausedPosition = textRect.anchoredPosition;
				}
			}
		}

		public void Resume()
		{
			if (isPaused)
			{
				isPaused = false;
				if (textRect != null)
				{
					textRect.anchoredPosition = pausedPosition;
				}
			}
		}

		public bool IsPaused()
		{
			return isPaused;
		}
	}
}
