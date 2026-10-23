using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	[RequireComponent(typeof(Image))]
	public class LocalizedImage : MonoBehaviour
	{
		[Header("图片多语言配置")]
		public string imageId;

		private Image imageComponent;

		private void Start()
		{
			imageComponent = GetComponent<Image>();
			if (ImageLocalizationManager.Instance != null)
			{
				ImageLocalizationManager.Instance.OnImageLanguageChanged += UpdateImage;
			}
			UpdateImage();
		}

		private void OnDestroy()
		{
			if (ImageLocalizationManager.Instance != null)
			{
				ImageLocalizationManager.Instance.OnImageLanguageChanged -= UpdateImage;
			}
		}

		public void UpdateImage()
		{
			if (imageComponent == null || ImageLocalizationManager.Instance == null ||
				string.IsNullOrEmpty(imageId))
			{
				return;
			}
			Sprite sprite = ImageLocalizationManager.Instance.GetSprite(imageId);
			if (sprite != null)
			{
				imageComponent.sprite = sprite;
				if (imageId.StartsWith("payment_provider_", System.StringComparison.Ordinal))
					imageComponent.preserveAspect = true;
			}
		}

		public void SetImageId(string newImageId)
		{
			imageId = newImageId;
			UpdateImage();
		}
	}
}
