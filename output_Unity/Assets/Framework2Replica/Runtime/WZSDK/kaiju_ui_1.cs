using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	public class kaiju_ui_1 : BaseViewWithDraw
	{
		[SerializeField] private float ButtonClearance = 170f;

		[SerializeField] private float HeadingClearance = 36f;

		[SerializeField] private float MinimumParagraphGap = 32f;

		[SerializeField] private float MaximumParagraphGap = 160f;

		[SerializeField] private Vector2 referenceResolution = new Vector2(1200f, 2132f);

		public Vector2 ReferenceResolution => referenceResolution;

		private const float TextVisualPadding = 8f;

		private const int MinimumParagraphFontSize = 42;

		private RectTransform rootRect;

		private RectTransform contentRoot;

		private RectTransform headingRect;

		private RectTransform continueRect;

		private Image backgroundImage;

		private Button continueButton;

		private Text[] paragraphs;

		private Vector2[] defaultParagraphPositions;

		private Vector2[] defaultParagraphSizes;

		private int[] defaultParagraphFontSizes;

		private float defaultParagraphTop;

		private Vector2 defaultHeadingPosition;

		private Vector2 lastRootSize = new Vector2(float.NaN, float.NaN);

		private bool layoutDirty = true;

		private LocalizationManager observedLocalizationManager;

		private bool isStartupIntroduction;

		private System.Action startupContinue;

		protected override void OnAwake()
		{
			continueButton = Find<Button>("continue");
			if (continueButton != null)
			{
				continueButton.onClick.AddListener(onButtonClick);
				continueRect = continueButton.transform as RectTransform;
			}

			rootRect = transform as RectTransform;
			backgroundImage = Find<Image>("bg");
			contentRoot = Find<RectTransform>("Top");
			headingRect = Find<RectTransform>("Top/Welcome");
			paragraphs = new[]
			{
				Find<Text>("Top/Dec1"),
				Find<Text>("Top/Dec2"),
				Find<Text>("Top/Dec3")
			};
			CaptureDefaultParagraphLayout();
		}

		protected override void OnStart()
		{
			observedLocalizationManager = LocalizationManager.Instance;
			if (observedLocalizationManager != null)
			{
				observedLocalizationManager.OnLanguageChanged += MarkLayoutDirty;
			}
			layoutDirty = true;
		}

		private void OnEnable()
		{
			layoutDirty = true;
		}

		private void OnDestroy()
		{
			startupContinue = null;
			if (continueButton != null)
			{
				continueButton.onClick.RemoveListener(onButtonClick);
			}
			if (observedLocalizationManager != null)
			{
				observedLocalizationManager.OnLanguageChanged -= MarkLayoutDirty;
			}
		}

		private void OnRectTransformDimensionsChange()
		{
			layoutDirty = true;
		}

		private void LateUpdate()
		{
			if (rootRect != null && rootRect.rect.size != lastRootSize)
			{
				layoutDirty = true;
			}
			if (!layoutDirty)
			{
				return;
			}

			ApplyResponsiveTextLayout();
			lastRootSize = rootRect != null ? rootRect.rect.size : Vector2.zero;
			layoutDirty = false;
		}

		private void CaptureDefaultParagraphLayout()
		{
			if (headingRect != null)
			{
				defaultHeadingPosition = headingRect.anchoredPosition;
			}

			if (paragraphs == null || paragraphs.Length == 0)
			{
				return;
			}

			defaultParagraphPositions = new Vector2[paragraphs.Length];
			defaultParagraphSizes = new Vector2[paragraphs.Length];
			defaultParagraphFontSizes = new int[paragraphs.Length];
			for (int i = 0; i < paragraphs.Length; i++)
			{
				Text paragraph = paragraphs[i];
				if (paragraph == null)
				{
					continue;
				}
				RectTransform rect = paragraph.rectTransform;
				defaultParagraphPositions[i] = rect.anchoredPosition;
				defaultParagraphSizes[i] = rect.sizeDelta;
				defaultParagraphFontSizes[i] = paragraph.fontSize;
			}

			Text firstParagraph = paragraphs[0];
			if (firstParagraph != null)
			{
				RectTransform firstRect = firstParagraph.rectTransform;
				defaultParagraphTop = firstRect.anchoredPosition.y +
					firstRect.rect.height * (1f - firstRect.pivot.y);
			}
		}

		private void MarkLayoutDirty()
		{
			layoutDirty = true;
		}

		private void ApplyResponsiveTextLayout()
		{
			if (contentRoot == null || continueRect == null || paragraphs == null ||
				defaultParagraphPositions == null)
			{
				return;
			}

			RestoreDefaultTextLayout();
			PositionHeadingForCurrentViewport();
			int paragraphCount = 0;
			foreach (Text paragraph in paragraphs)
				if (IsParagraphVisible(paragraph)) paragraphCount++;
			if (paragraphCount == 0) return;
			float lowerLimit = GetButtonTopInContentSpace() + ButtonClearance;
			float maximumParagraphTop = GetMaximumParagraphTop();
			float maximumAvailableHeight = maximumParagraphTop - lowerLimit;
			if (maximumAvailableHeight <= 0f)
			{
				return;
			}

			int maximumFontSize = GetMaximumUniformParagraphFontSize();
			int selectedFontSize = MinimumParagraphFontSize;
			float[] heights = null;
			for (int fontSize = maximumFontSize;
				fontSize >= MinimumParagraphFontSize;
				fontSize--)
			{
				SetParagraphFontSize(fontSize);
				float[] candidateHeights = GetParagraphPreferredHeights();
				float requiredHeight = Sum(candidateHeights) +
					MinimumParagraphGap * (paragraphCount - 1);
				if (requiredHeight <= maximumAvailableHeight)
				{
					selectedFontSize = fontSize;
					heights = candidateHeights;
					break;
				}
			}

			SetParagraphFontSize(selectedFontSize);
			if (heights == null)
			{
				heights = GetParagraphPreferredHeights();
			}

			float heightSum = Sum(heights);
			float paragraphTop = maximumParagraphTop;
			float availableHeight = paragraphTop - lowerLimit;
			float gap = paragraphCount > 1
				? Mathf.Clamp(
					(availableHeight - heightSum) / (paragraphCount - 1),
					MinimumParagraphGap,
					MaximumParagraphGap)
				: 0f;

			float cursor = paragraphTop;
			for (int i = 0; i < paragraphs.Length; i++)
			{
				Text paragraph = paragraphs[i];
				if (!IsParagraphVisible(paragraph))
				{
					continue;
				}
				RectTransform rect = paragraph.rectTransform;
				Vector2 size = rect.sizeDelta;
				size.y = heights[i];
				rect.sizeDelta = size;
				Vector2 position = rect.anchoredPosition;
				position.y = cursor - heights[i] * (1f - rect.pivot.y);
				rect.anchoredPosition = position;
				cursor -= heights[i] + gap;
			}
		}

		private void RestoreDefaultTextLayout()
		{
			if (headingRect != null)
			{
				headingRect.anchoredPosition = defaultHeadingPosition;
			}

			for (int i = 0; i < paragraphs.Length; i++)
			{
				Text paragraph = paragraphs[i];
				if (paragraph == null)
				{
					continue;
				}
				paragraph.fontSize = defaultParagraphFontSizes[i];
				paragraph.rectTransform.anchoredPosition =
					defaultParagraphPositions[i];
				paragraph.rectTransform.sizeDelta = defaultParagraphSizes[i];
			}
		}

		private void PositionHeadingForCurrentViewport()
		{
			if (rootRect == null || contentRoot == null || headingRect == null ||
				backgroundImage == null || backgroundImage.sprite == null)
			{
				return;
			}

			float pixelsPerUnit = backgroundImage.pixelsPerUnit;
			if (pixelsPerUnit <= 0f)
			{
				return;
			}

			Bounds rootBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(
				contentRoot,
				rootRect);
			float fixedTopSliceHeight =
				backgroundImage.sprite.border.w / pixelsPerUnit;
			float responsiveHeadingTop = rootBounds.max.y - fixedTopSliceHeight;
			float headingTop = responsiveHeadingTop;
			Vector2 position = headingRect.anchoredPosition;
			position.y += headingTop - GetRectTop(headingRect);
			headingRect.anchoredPosition = position;
		}

		private float GetButtonTopInContentSpace()
		{
			Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(
				contentRoot,
				continueRect);
			return bounds.max.y;
		}

		private float GetMaximumParagraphTop()
		{
			if (headingRect == null)
			{
				return defaultParagraphTop;
			}
			Bounds headingBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(
				contentRoot,
				headingRect);
			return headingBounds.min.y - HeadingClearance;
		}

		private int GetMaximumUniformParagraphFontSize()
		{
			int fontSize = int.MaxValue;
			for (int i = 0; i < defaultParagraphFontSizes.Length; i++)
			{
				if (defaultParagraphFontSizes[i] > 0)
				{
					fontSize = Mathf.Min(fontSize, defaultParagraphFontSizes[i]);
				}
			}
			return fontSize == int.MaxValue
				? MinimumParagraphFontSize
				: Mathf.Max(fontSize, MinimumParagraphFontSize);
		}

		private void SetParagraphFontSize(int fontSize)
		{
			for (int i = 0; i < paragraphs.Length; i++)
			{
				if (paragraphs[i] != null)
				{
					paragraphs[i].fontSize = fontSize;
				}
			}
		}

		private float[] GetParagraphPreferredHeights()
		{
			float[] heights = new float[paragraphs.Length];
			for (int i = 0; i < paragraphs.Length; i++)
			{
				heights[i] = IsParagraphVisible(paragraphs[i])
					? GetPreferredHeight(paragraphs[i])
					: 0f;
			}
			return heights;
		}

		private static bool IsParagraphVisible(Text paragraph)
		{
			return paragraph != null && paragraph.gameObject.activeSelf;
		}

		private static float GetPreferredHeight(Text text)
		{
			return Mathf.Ceil(text.preferredHeight + TextVisualPadding);
		}

		private static float GetRectTop(RectTransform rect)
		{
			return rect.anchoredPosition.y +
				rect.rect.height * (1f - rect.pivot.y);
		}

		private static float Sum(float[] values)
		{
			float sum = 0f;
			for (int i = 0; i < values.Length; i++)
			{
				sum += values[i];
			}
			return sum;
		}

		// The loading scene can reuse the original presentation without starting
		// Framework 2's view manager, withdrawal flow or save-data services.
		public void ShowForStartup(System.Action onContinue, MahjongSlide.Withdrawal.MahjongIntroductionData copy)
		{
			isStartupIntroduction = true;
			startupContinue = onContinue;
			if (copy != null)
			{
				var locale = copy.ResolveLocale(WaterSort.WaterSortCurrencyService.LocalizationLanguage);
				var font = locale != null ? locale.font : null;
				var localizedParagraphs = locale != null ? locale.paragraphs : copy.paragraphs;
				// Clear source localization IDs so retained Framework 2 services cannot
				// overwrite the framework-specific startup copy.
				SetStartupText(headingRect != null ? headingRect.GetComponent<Text>() : null,
					locale != null ? locale.heading : copy.heading, font);
				SetStartupText(Find<Text>("continue/Text (Legacy)"),
					locale != null ? locale.continueLabel : copy.continueLabel, font);
				for (int i = 0; i < paragraphs.Length; i++)
				{
					string value = localizedParagraphs != null && i < localizedParagraphs.Length ? localizedParagraphs[i] : string.Empty;
					SetStartupText(paragraphs[i], value, font);
					if (paragraphs[i] != null) paragraphs[i].gameObject.SetActive(!string.IsNullOrWhiteSpace(value));
				}
			}
			else Debug.LogError("Startup introduction requires its framework-specific inline copy configuration.", this);
			layoutDirty = true;
			ShowView();
		}

		private static void SetStartupText(Text target, string value, Font font)
		{
			if (target == null) return;
			LocalizedText localized = target.GetComponent<LocalizedText>();
			if (localized != null) localized.SetTextId(string.Empty);
			if (font != null) target.font = font;
			target.text = value;
		}

		public void onButtonClick()
		{
			if (isStartupIntroduction)
			{
				var onContinue = startupContinue;
				startupContinue = null;
				if (continueButton != null) continueButton.interactable = false;
				onContinue?.Invoke();
				return;
			}
			GameApp.viewManager.Close(ViewId);
		}
	}
}
