using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using WaterSort;

namespace WZSDK
{
	public class LocalizationManager : MonoBehaviour
	{
		[Header("文本多语言配置")]
		private LanguageType defaultLanguage = LanguageType.English;

		private const string TextCsvName = "TextLocalization";

		private Dictionary<LanguageType, Dictionary<string, string>> textData;

		private LanguageType currentLanguage;

		private Dictionary<string, string> currentLanguageData;

		public Action OnLanguageChanged;

		private Task initializationTask;

		private bool isInitialized;

		private LanguageType? pendingLanguage;
		private WaterSortLocalizationData configuredDatabase;

		private readonly Dictionary<string, string> staticTextKeys =
			new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		public static LocalizationManager Instance { get; private set; }

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
			textData = new Dictionary<LanguageType, Dictionary<string, string>>();
			currentLanguageData = new Dictionary<string, string>();
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
				await LoadTextDataAsync();
				if (this == null)
				{
					return;
				}

				isInitialized = true;
				if (pendingLanguage.HasValue)
				{
					LanguageType language = pendingLanguage.Value;
					pendingLanguage = null;
					SetLanguage(language);
				}
			}
			catch (Exception exception)
			{
				Debug.LogError(
					"LocalizationManager initialization failed: " +
					exception.Message);
			}
		}

		private bool TryLoadConfiguredDatabase()
		{
			WaterSortLocalizationData database = Framework2Host.Settings != null
				? Framework2Host.Settings.localizationDatabase : null;
			if (database == null) return false;
			if (configuredDatabase != database)
			{
				configuredDatabase = database;
				textData.Clear();
				foreach (WaterSortLocale locale in database.Locales)
				{
					if (locale == null || !LanguageTypeContract.TryParseCode(locale.Code, out LanguageType language)) continue;
					var dictionary = new Dictionary<string, string>(StringComparer.Ordinal);
					foreach (WaterSortLocalizedText entry in locale.Texts)
					{
						if (entry == null || string.IsNullOrWhiteSpace(entry.Key) || string.IsNullOrWhiteSpace(entry.Value)) continue;
						dictionary[entry.Key] = entry.Value;
					}
					textData[language] = dictionary;
				}
				if (!textData.TryGetValue(currentLanguage, out currentLanguageData))
				{
					currentLanguage = defaultLanguage;
					textData.TryGetValue(defaultLanguage, out currentLanguageData);
				}
				BuildStaticTextLookup();
			}
			return true;
		}

		private async Task LoadTextDataAsync()
		{
			if (TryLoadConfiguredDatabase()) return;
			List<string[]> rows =
				await UserDataManager.GetCsvRowsAsync(TextCsvName);
			if (this == null)
			{
				return;
			}
			// The prefab's Awake can precede the host's settings assignment.
			// Never let an earlier CSV request overwrite the configured SO.
			if (TryLoadConfiguredDatabase()) return;
			if (rows == null || rows.Count < 2)
			{
				Debug.LogError("Text localization CSV is empty or invalid.");
				return;
			}

			string[] header = rows[0];
			for (int column = 1; column < header.Length; column++)
			{
				if (string.IsNullOrWhiteSpace(header[column]))
				{
					continue;
				}
				LanguageType language = GetLanguageTypeFromCode(header[column].Trim());
				textData[language] = new Dictionary<string, string>(StringComparer.Ordinal);
			}

			for (int rowIndex = 1; rowIndex < rows.Count; rowIndex++)
			{
				string[] row = rows[rowIndex];
				if (row == null || row.Length < 2 || string.IsNullOrEmpty(row[0]))
				{
					continue;
				}
				string key = row[0].Trim();
				int count = Mathf.Min(header.Length, row.Length);
				for (int column = 1; column < count; column++)
				{
					if (string.IsNullOrWhiteSpace(header[column]))
					{
						continue;
					}
					LanguageType language = GetLanguageTypeFromCode(header[column].Trim());
					Dictionary<string, string> dictionary;
					if (textData.TryGetValue(language, out dictionary))
					{
						string localizedText = row[column];
						if (string.IsNullOrWhiteSpace(localizedText)) continue;
						if (language == LanguageType.Indonesian)
						{
							localizedText = localizedText.Replace("RP", "Rp");
						}
						dictionary[key] = localizedText;
					}
				}
			}

			Dictionary<string, string> defaultData;
			if (textData.TryGetValue(defaultLanguage, out defaultData))
			{
				currentLanguage = defaultLanguage;
				currentLanguageData = defaultData;
			}
			BuildStaticTextLookup();
		}

		private LanguageType GetLanguageTypeFromCode(string languageCode)
		{
			LanguageType language;
			return LanguageTypeContract.TryParseCode(languageCode, out language)
				? language
				: defaultLanguage;
		}

		public void SetLanguage(LanguageType language)
		{
			if (TryLoadConfiguredDatabase()) isInitialized = true;
			if (!isInitialized)
			{
				pendingLanguage = language;
				_ = InitializeAsync();
				return;
			}

			Dictionary<string, string> dictionary;
			if (textData != null && textData.TryGetValue(language, out dictionary))
			{
				currentLanguage = language;
				currentLanguageData = dictionary;
				if (OnLanguageChanged != null)
				{
					OnLanguageChanged();
				}
				return;
			}

			Debug.LogWarning(
				"Language " + LanguageTypeContract.GetCode(language) +
				" is unavailable; using " +
				LanguageTypeContract.GetCode(defaultLanguage) + ".");
			currentLanguage = defaultLanguage;
			if (textData != null)
			{
				textData.TryGetValue(defaultLanguage, out currentLanguageData);
			}
			OnLanguageChanged?.Invoke();
		}

		public string GetText(string key)
		{
			TryLoadConfiguredDatabase();
			string sourceEnglish = null;
			if (key != null && textData != null && textData.TryGetValue(defaultLanguage, out Dictionary<string, string> originalEnglish))
				originalEnglish.TryGetValue(key, out sourceEnglish);
			if (Apple.Framework2.Framework2SimulationCopy.TryGet(key, sourceEnglish, out string simulationText))
				return ResolveMoneyTokens(simulationText);
			string value;
			if (currentLanguageData != null && key != null &&
				currentLanguageData.TryGetValue(key, out value) && !string.IsNullOrWhiteSpace(value))
			{
				return ResolveMoneyTokens(value);
			}
			if (key != null && textData != null && textData.TryGetValue(defaultLanguage, out Dictionary<string, string> fallback) &&
				fallback.TryGetValue(key, out value) && !string.IsNullOrWhiteSpace(value)) return ResolveMoneyTokens(value);
			Debug.LogWarning("Localized text key is missing: " + key);
			return key ?? string.Empty;
		}

		public void PreloadTexts(params string[] keys)
		{
			if (keys == null || currentLanguageData == null)
			{
				return;
			}
			for (int i = 0; i < keys.Length; i++)
			{
				currentLanguageData.ContainsKey(keys[i]);
			}
		}

		public List<string> GetAllTextKeys()
		{
			return currentLanguageData != null
				? new List<string>(currentLanguageData.Keys)
				: new List<string>();
		}

		public bool HasText(string key)
		{
			TryLoadConfiguredDatabase();
			return key != null && ((currentLanguageData != null && currentLanguageData.ContainsKey(key)) ||
				(textData != null && textData.TryGetValue(defaultLanguage, out Dictionary<string, string> fallback) && fallback.ContainsKey(key)));
		}

		private void BuildStaticTextLookup()
		{
			staticTextKeys.Clear();
			foreach (Dictionary<string, string> language in textData.Values)
				foreach (KeyValuePair<string, string> entry in language)
				{
					if (entry.Key.StartsWith("literal:", StringComparison.Ordinal))
						staticTextKeys[NormalizeStaticText(entry.Key.Substring("literal:".Length))] = entry.Key;
					if (!string.IsNullOrWhiteSpace(entry.Value) && entry.Value.IndexOf('{') < 0)
						staticTextKeys[NormalizeStaticText(entry.Value)] = entry.Key;
				}
		}

		private static string NormalizeStaticText(string value) =>
			string.Join(" ", value.Split((char[])null, StringSplitOptions.RemoveEmptyEntries));

		private string ResolveMoneyTokens(string value)
		{
			const string prefix = "[[money:";
			int start = value.IndexOf(prefix, StringComparison.Ordinal);
			if (start < 0) return value;
			var result = new StringBuilder(value.Length);
			int copied = 0;
			while (start >= 0)
			{
				int end = value.IndexOf("]]", start + prefix.Length, StringComparison.Ordinal);
				if (end < 0) break;
				string token = value.Substring(start + prefix.Length, end - start - prefix.Length);
				double amount;
				var economy = Framework2Host.Settings != null ? Framework2Host.Settings.economy : null;
				if (token == "minimumCashOut" && economy != null) amount = economy.rewards.minimumCashOutAmount;
				else if (token == "handlingFee" && economy != null) amount = economy.rewards.cashOutHandlingFee;
				else if (token == "feeFreeThreshold" && economy != null) amount = economy.rewards.feeFreeCashOutThreshold;
				else if (!double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out amount))
				{
					start = value.IndexOf(prefix, end + 2, StringComparison.Ordinal);
					continue;
				}
				result.Append(value, copied, start - copied);
				GameManagerWZ gm = GameManagerWZ.instance;
				result.Append(gm != null ? gm.FormatRegionalValue(amount) : amount.ToString("C2", CultureInfo.GetCultureInfo("en-US")));
				copied = end + 2;
				start = value.IndexOf(prefix, copied, StringComparison.Ordinal);
			}
			result.Append(value, copied, value.Length - copied);
			return result.ToString();
		}

		public void ApplyFont(Text text)
		{
			if (text == null || Framework2Host.Settings == null || Framework2Host.Settings.currencyDatabase == null) return;
			if (text.font != null && text.GetComponentInParent<Framework2AppleSkinMarker>(true) != null) return;
			Font font = Framework2Host.Settings.currencyDatabase.Resolve(Framework2Host.Region?.Invoke()).NativeFont;
			if (font != null && text.font != font) text.font = font;
		}

		// Native prefabs contain both LocalizedText and ordinary, script-filled Text.
		// Refresh at view boundaries, never scan the hierarchy every frame.
		public void ApplyView(GameObject root)
		{
			if (root == null) return;
			Text[] texts = root.GetComponentsInChildren<Text>(true);
			foreach (Text text in texts)
			{
				ApplyFont(text);
				LocalizedText localized = text.GetComponent<LocalizedText>();
				if (localized != null || string.IsNullOrWhiteSpace(text.text)) continue;
				if (staticTextKeys.TryGetValue(NormalizeStaticText(text.text), out string key)) text.text = GetText(key);
			}
		}

		public Dictionary<string, string> GetTexts(params string[] keys)
		{
			Dictionary<string, string> result =
				new Dictionary<string, string>(StringComparer.Ordinal);
			if (keys == null)
			{
				return result;
			}
			for (int i = 0; i < keys.Length; i++)
			{
				result[keys[i]] = GetText(keys[i]);
			}
			return result;
		}

		public LanguageType GetCurrentLanguage()
		{
			return currentLanguage;
		}

		public List<LanguageType> GetAvailableLanguages()
		{
			return textData != null
				? new List<LanguageType>(textData.Keys)
				: new List<LanguageType>();
		}

		public void UnloadLanguageData(LanguageType language)
		{
			if (language == currentLanguage || textData == null)
			{
				return;
			}
			Dictionary<string, string> dictionary;
			if (textData.TryGetValue(language, out dictionary))
			{
				dictionary.Clear();
				Debug.Log(
					"Unloaded text localization data for " +
					LanguageTypeContract.GetCode(language) + ".");
			}
		}

		public void ReloadTextData()
		{
			_ = ReloadTextDataAsync();
		}

		public async Task ReloadTextDataAsync()
		{
			await InitializeAsync();
			if (this == null)
			{
				return;
			}

			if (textData == null)
			{
				textData = new Dictionary<LanguageType, Dictionary<string, string>>();
			}
			else
			{
				textData.Clear();
			}
			if (currentLanguageData != null)
			{
				currentLanguageData.Clear();
			}
			LanguageType language = currentLanguage;
			configuredDatabase = null;
			await LoadTextDataAsync();
			if (this == null)
			{
				return;
			}
			Dictionary<string, string> dictionary;
			if (textData.TryGetValue(language, out dictionary))
			{
				currentLanguage = language;
				currentLanguageData = dictionary;
			}
			else if (textData.TryGetValue(defaultLanguage, out dictionary))
			{
				currentLanguage = defaultLanguage;
				currentLanguageData = dictionary;
			}
			Debug.Log("Text localization data reloaded.");
		}

		private void OnDestroy()
		{
			if (Instance == this)
			{
				Instance = null;
			}
		}

	}
}
