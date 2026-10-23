using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArrowOut.Runtime;
using UnityEngine;

namespace WZSDK
{
	public class ImageLocalizationManager : MonoBehaviour
	{
		[Header("图片多语言配置")]
		private LanguageType defaultLanguage = LanguageType.English;

		private const string ImageCsvName = "ImageLocalization";

		private string imageFolderPath;

		private Dictionary<LanguageType, Dictionary<string, string>> imagePathData;

		private Dictionary<string, Sprite> loadedSprites;

		private LanguageType currentLanguage;

		public Action OnImageLanguageChanged;

		private Task initializationTask;

		private bool isInitialized;

		private LanguageType? pendingLanguage;

		private int languageGeneration;

		public static ImageLocalizationManager Instance { get; private set; }

		public bool IsInitialized
		{
			get { return isInitialized; }
		}

		private void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
				InitializeContainers();
				_ = InitializeAsync();
				return;
			}
			if (Instance != this)
			{
				Destroy(this);
			}
		}

		private void InitializeContainers()
		{
			imagePathData = new Dictionary<LanguageType, Dictionary<string, string>>();
			loadedSprites = new Dictionary<string, Sprite>();
		}

		public Task InitializeAsync()
		{
			if (initializationTask == null)
			{
				initializationTask = InitializeAsyncCore();
			}
			return initializationTask;
		}

		private async Task InitializeAsyncCore()
		{
			try
			{
				await LoadImagePathDataAsync();
				if (this == null)
				{
					return;
				}

				isInitialized = true;
				if (pendingLanguage.HasValue)
				{
					LanguageType language = pendingLanguage.Value;
					pendingLanguage = null;
					await ApplyLanguageAsync(language);
				}
			}
			catch (Exception exception)
			{
				Debug.LogError(
					"ImageLocalizationManager initialization failed: " +
					exception.Message);
			}
		}

		private async Task LoadImagePathDataAsync()
		{
			List<string[]> rows =
				await UserDataManager.GetCsvRowsAsync(ImageCsvName);
			if (this == null)
			{
				return;
			}
			if (rows == null || rows.Count < 2)
			{
				Debug.LogError("Image localization CSV is empty or invalid.");
				return;
			}

			string[] header = rows[0];
			for (int column = 1; column < header.Length; column++)
			{
				if (string.IsNullOrWhiteSpace(header[column]))
				{
					continue;
				}
				LanguageType language = GetLanguageTypeFromCode(header[column]);
				imagePathData[language] =
					new Dictionary<string, string>(StringComparer.Ordinal);
			}

			for (int rowIndex = 1; rowIndex < rows.Count; rowIndex++)
			{
				string[] row = rows[rowIndex];
				if (row == null || row.Length < 2 || string.IsNullOrEmpty(row[0]))
				{
					continue;
				}
				int count = Mathf.Min(header.Length, row.Length);
				for (int column = 1; column < count; column++)
				{
					if (string.IsNullOrWhiteSpace(header[column]) ||
						string.IsNullOrEmpty(row[column]))
					{
						continue;
					}
					LanguageType language = GetLanguageTypeFromCode(header[column]);
					Dictionary<string, string> dictionary;
					if (imagePathData.TryGetValue(language, out dictionary))
					{
						dictionary[row[0]] = row[column];
					}
				}
			}
			if (imagePathData.TryGetValue(LanguageType.English, out var englishPaths))
			{
				foreach (var language in new[] { LanguageType.Japanese, LanguageType.Korean,
					LanguageType.Brazilian, LanguageType.Indonesian })
				{
					if (!imagePathData.TryGetValue(language, out var localizedPaths))
						imagePathData[language] = localizedPaths = new Dictionary<string, string>();
					foreach (var entry in englishPaths)
						if (!localizedPaths.ContainsKey(entry.Key)) localizedPaths[entry.Key] = entry.Value;
				}
			}
			currentLanguage = imagePathData.ContainsKey(defaultLanguage)
				? defaultLanguage
				: currentLanguage;
		}

		private LanguageType GetLanguageTypeFromCode(string languageCode)
		{
			LanguageType language;
			return LanguageTypeContract.TryParseCode(languageCode, out language)
				? language
				: defaultLanguage;
		}

		private Sprite GetCachedSprite(string spritePath)
		{
			if (string.IsNullOrEmpty(spritePath))
			{
				return null;
			}
			string address = string.IsNullOrEmpty(imageFolderPath)
				? spritePath
				: imageFolderPath.TrimEnd('/', '\\') + "/" + spritePath.TrimStart('/', '\\');
			Sprite sprite;
			if (loadedSprites.TryGetValue(address, out sprite))
			{
				return sprite;
			}
			return null;
		}

		private async Task<Sprite> LoadSpriteOnDemandAsync(
			string spritePath,
			int generation)
		{
			if (string.IsNullOrEmpty(spritePath))
			{
				return null;
			}

			string address = BuildSpriteAddress(spritePath);
			Sprite sprite;
			if (loadedSprites.TryGetValue(address, out sprite))
			{
				return sprite;
			}

			sprite = await AddressableAssetService.LoadAsync<Sprite>(address);
			if (this == null || sprite == null)
			{
				return null;
			}

			if (generation != languageGeneration)
			{
				return null;
			}

			if (sprite != null)
			{
				loadedSprites[address] = sprite;
			}
			return sprite;
		}

		private void ClearCurrentLanguageCache()
		{
			loadedSprites?.Clear();
		}

		public void PreloadImage(string imageId)
		{
			_ = PreloadImageAsync(imageId);
		}

		public Task<Sprite> PreloadImageAsync(string imageId)
		{
			return GetSpriteAsync(imageId);
		}

		public async Task PreloadCurrentLanguageSpritesAsync(
			params string[] imageIds)
		{
			await InitializeAsync();
			if (this == null || imagePathData == null)
			{
				return;
			}

			await PreloadCurrentLanguageSpritesCoreAsync(
				languageGeneration,
				imageIds);
		}

		private async Task PreloadCurrentLanguageSpritesCoreAsync(
			int generation,
			string[] imageIds)
		{
			if (imagePathData == null)
			{
				return;
			}

			Dictionary<string, string> paths;
			if (!imagePathData.TryGetValue(currentLanguage, out paths) ||
				paths == null)
			{
				return;
			}

			List<Task<Sprite>> loadTasks = new List<Task<Sprite>>();
			HashSet<string> scheduledAddresses = new HashSet<string>(
				StringComparer.Ordinal);
			if (imageIds != null && imageIds.Length > 0)
			{
				for (int i = 0; i < imageIds.Length; i++)
				{
					string path;
					if (!string.IsNullOrEmpty(imageIds[i]) &&
						paths.TryGetValue(imageIds[i], out path))
					{
						ScheduleSpritePreload(
							path,
							generation,
							scheduledAddresses,
							loadTasks);
					}
				}
			}
			else
			{
				foreach (string path in paths.Values)
				{
					ScheduleSpritePreload(
						path,
						generation,
						scheduledAddresses,
						loadTasks);
				}
			}

			if (loadTasks.Count > 0)
			{
				await Task.WhenAll(loadTasks);
			}
		}

		private void ScheduleSpritePreload(
			string spritePath,
			int generation,
			HashSet<string> scheduledAddresses,
			List<Task<Sprite>> loadTasks)
		{
			if (string.IsNullOrEmpty(spritePath))
			{
				return;
			}

			string address = BuildSpriteAddress(spritePath);
			if (scheduledAddresses.Add(address))
			{
				loadTasks.Add(LoadSpriteOnDemandAsync(spritePath, generation));
			}
		}

		public void SetLanguage(LanguageType language)
		{
			if (!isInitialized)
			{
				pendingLanguage = language;
				_ = InitializeAsync();
				return;
			}

			_ = ApplyLanguageAsync(language);
		}

		public async Task SetLanguageAsync(LanguageType language)
		{
			if (!isInitialized)
			{
				pendingLanguage = language;
				await InitializeAsync();
				return;
			}

			await ApplyLanguageAsync(language);
		}

		private async Task ApplyLanguageAsync(LanguageType language)
		{
			LanguageType appliedLanguage = language;
			if (imagePathData == null || !imagePathData.ContainsKey(appliedLanguage))
			{
				Debug.LogWarning(
					"Image language " + LanguageTypeContract.GetCode(language) +
					" is unavailable; using " +
					LanguageTypeContract.GetCode(defaultLanguage) + ".");
				appliedLanguage = defaultLanguage;
			}

			if (imagePathData == null || !imagePathData.ContainsKey(appliedLanguage))
			{
				return;
			}

			languageGeneration++;
			int generation = languageGeneration;
			ClearCurrentLanguageCache();
			currentLanguage = appliedLanguage;
			await PreloadCurrentLanguageSpritesCoreAsync(generation, null);
			if (this == null || generation != languageGeneration)
			{
				return;
			}

			if (OnImageLanguageChanged != null)
			{
				OnImageLanguageChanged();
			}
		}

		public Sprite GetSprite(string imageId)
		{
			Sprite configured = GetRegionalProviderSprite(imageId);
			if (configured != null) return configured;
			if (!isInitialized)
			{
				_ = GetSpriteAsync(imageId);
				return null;
			}

			string path = GetSpritePath(imageId);
			if (string.IsNullOrEmpty(path))
			{
				Debug.LogWarning("Localized image is missing: " + imageId +
					" (" + LanguageTypeContract.GetCode(currentLanguage) + ")");
				return null;
			}
			Sprite sprite = GetCachedSprite(path);
			if (sprite == null)
			{
				_ = GetSpriteAsync(imageId);
			}
			return sprite;
		}

		public async Task<Sprite> GetSpriteAsync(string imageId)
		{
			Sprite configured = GetRegionalProviderSprite(imageId);
			if (configured != null) return configured;
			await InitializeAsync();
			if (this == null)
			{
				return null;
			}

			string path = GetSpritePath(imageId);
			if (string.IsNullOrEmpty(path))
			{
				Debug.LogWarning("Localized image is missing: " + imageId +
					" (" + LanguageTypeContract.GetCode(currentLanguage) + ")");
				return null;
			}

			return await LoadSpriteOnDemandAsync(path, languageGeneration);
		}

		public string GetSpritePath(string imageId)
		{
			Dictionary<string, string> paths;
			string path;
			if (imagePathData == null || imageId == null) return null;
			if (imagePathData.TryGetValue(currentLanguage, out paths) &&
				paths.TryGetValue(imageId, out path) && !string.IsNullOrWhiteSpace(path)) return path;
			return imagePathData.TryGetValue(LanguageType.English, out paths) &&
				paths.TryGetValue(imageId, out path) ? path : null;
		}

		private static Sprite GetRegionalProviderSprite(string imageId)
		{
			var channel = Framework2Host.PaymentChannel;
			if (channel == null) return null;
			switch (imageId)
			{
				case "payment_provider_logo_large": return channel.Logo;
				case "payment_provider_logo_square":
				case "payment_provider_logo_small": return channel.Icon;
				default: return null;
			}
		}

		public void UnloadSprite(string imageId)
		{
			string path = GetSpritePath(imageId);
			if (string.IsNullOrEmpty(path))
			{
				return;
			}

			string address = BuildSpriteAddress(path);
			loadedSprites?.Remove(address);
		}

		public void ClearAllCache()
		{
			languageGeneration++;
			loadedSprites?.Clear();
		}

		private void OnDestroy()
		{
			languageGeneration++;
			if (Instance == this)
			{
				Instance = null;
			}
		}

		public LanguageType GetCurrentLanguage()
		{
			return currentLanguage;
		}

		private string BuildSpriteAddress(string spritePath)
		{
			return string.IsNullOrEmpty(imageFolderPath)
				? spritePath
				: imageFolderPath.TrimEnd('/', '\\') + "/" + spritePath.TrimStart('/', '\\');
		}

	}
}
