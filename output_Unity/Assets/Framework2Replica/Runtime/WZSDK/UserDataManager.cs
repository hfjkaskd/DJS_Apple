using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArrowOut.Runtime;
using UnityEngine;

namespace WZSDK
{
	public class UserDataManager : MonoSingleton<UserDataManager>, ILoad, IDispose
	{

		private Sprite singleCoin;

		private Sprite MultCoin;

		private Sprite singlGem;

		private Sprite GetMulteGem;

		private List<UserLevelData> levelDataList = new List<UserLevelData>();

		private List<Dictionary<string, object>> stageData =
			new List<Dictionary<string, object>>();

		private List<Dictionary<string, object>> gearData =
			new List<Dictionary<string, object>>();

		private List<Dictionary<string, object>> gemData =
			new List<Dictionary<string, object>>();

		private List<Dictionary<string, object>> barrageData =
			new List<Dictionary<string, object>>();

		private List<Dictionary<string, object>> PayRegionData =
			new List<Dictionary<string, object>>();

		private Dictionary<int, List<LevelRewardStatic.RewardItem>> levelRewardData =
			new Dictionary<int, List<LevelRewardStatic.RewardItem>>();

		private Dictionary<int, ConfLuckySpin> luckySpinData =
			new Dictionary<int, ConfLuckySpin>();

		private HashSet<int> luckyRewardLevels = new HashSet<int>();

		private HashSet<int> luckyRewardIconLevels = new HashSet<int>();

		private PlayerLevelData playerData = new PlayerLevelData();

		private static readonly Dictionary<string, List<string[]>> csvRowsCache =
			new Dictionary<string, List<string[]>>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, Task<List<string[]>>> csvRowsLoadTasks =
			new Dictionary<string, Task<List<string[]>>>(StringComparer.OrdinalIgnoreCase);

		// These are the core tables required by InitializeAsyncCore. LuckyReward
		// is deliberately optional because configured reward levels can replace
		// its CSV data.
		private static readonly HashSet<string> requiredStartupCsvPaths =
			new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				"csv/PayRegion",
				"csv/UserLevel",
				"csv/Stage",
				"csv/Gear",
				"csv/Extractdata",
				"csv/Gem",
				"csv/LuckySpin"
			};

		private static readonly HashSet<string> reportedCsvCacheMisses =
			new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		private static readonly object csvRowsLock = new object();
		private static int csvConfigurationGeneration;

		internal static void ResetConfigurationCache()
		{
			lock (csvRowsLock)
			{
				csvConfigurationGeneration++;
				csvRowsCache.Clear();
				csvRowsLoadTasks.Clear();
				reportedCsvCacheMisses.Clear();
			}
		}

		private static readonly UTF8Encoding strictUtf8 =
			new UTF8Encoding(false, true);

		public LanguageType initialLanguage = LanguageType.English;

		public string targetLanguage;

		private string countryCode = string.Empty;

		private Task initializationTask;

		private readonly object initializationLock = new object();

		private bool isInitialized;

		private bool facadePayTypeHandlersRegistered;

		public bool IsInitialized
		{
			get
			{
				lock (initializationLock)
				{
					return isInitialized;
				}
			}
		}

		public Task InitializeAsync()
		{
			lock (initializationLock)
			{
				if (isInitialized)
				{
					return initializationTask ?? Task.CompletedTask;
				}

				Task currentTask = initializationTask;
				if (currentTask == null)
				{
					currentTask = InitializeAsyncCore();
					initializationTask = currentTask;
					_ = ResetInitializationTaskOnFailureAsync(
						currentTask);
				}
				return currentTask;
			}
		}

		private async Task ResetInitializationTaskOnFailureAsync(Task task)
		{
			try
			{
				await task;
			}
			catch
			{
				lock (initializationLock)
				{
					if (ReferenceEquals(initializationTask, task))
					{
						initializationTask = null;
					}
				}
			}
		}

		private async Task InitializeAsyncCore()
		{
			try
			{
				LocalizationManager localizationManager = LocalizationManager.Instance;
				ImageLocalizationManager imageLocalizationManager =
					ImageLocalizationManager.Instance;

				Task localizationTask = localizationManager != null
					? localizationManager.InitializeAsync()
					: Task.CompletedTask;
				Task imageLocalizationTask = imageLocalizationManager != null
					? imageLocalizationManager.InitializeAsync()
					: Task.CompletedTask;

				Task<List<string[]>> payRegionTask = GetCsvRowsAsync("PayRegion");
				Task<List<string[]>> userLevelTask = GetCsvRowsAsync("UserLevel");
				Task<List<string[]>> stageTask = GetCsvRowsAsync("Stage");
				Task<List<string[]>> gearTask = GetCsvRowsAsync("Gear");
				Task<List<string[]>> extractDataTask = GetCsvRowsAsync("Extractdata");
				Task<List<string[]>> gemTask = GetCsvRowsAsync("Gem");
				Task<List<string[]>> luckySpinTask = GetCsvRowsAsync("LuckySpin");
				Task<List<string[]>> luckyRewardTask = GetCsvRowsAsync("LuckyReward");
				Task<Sprite> singleGemTask =
					AddressableAssetService.LoadAsync<Sprite>(
						GameDefines.Default_SingleGem);
				Task<Sprite> multipleGemTask =
					AddressableAssetService.LoadAsync<Sprite>(
						GameDefines.Default_SingDuileGem);

				await Task.WhenAll(
					localizationTask,
					imageLocalizationTask,
					payRegionTask);

				if (this == null)
				{
					return;
				}

				localizationManager = LocalizationManager.Instance;
				imageLocalizationManager = ImageLocalizationManager.Instance;
				await Task.WhenAll(
					localizationManager != null
						? localizationManager.InitializeAsync()
						: Task.CompletedTask,
					imageLocalizationManager != null
						? imageLocalizationManager.InitializeAsync()
						: Task.CompletedTask);
				if (this == null)
				{
					return;
				}

				LoadPayRegionFromCSV(await payRegionTask);
				GetLanguageData();
				await ApplyLanguageAsync();

				Task<Sprite> multipleCoinTask = imageLocalizationManager != null
					? imageLocalizationManager.GetSpriteAsync("1001")
					: Task.FromResult<Sprite>(null);
				Task<Sprite> singleCoinTask = imageLocalizationManager != null
					? imageLocalizationManager.GetSpriteAsync("1002")
					: Task.FromResult<Sprite>(null);

				await Task.WhenAll(
					userLevelTask,
					stageTask,
					gearTask,
					extractDataTask,
					gemTask,
					luckySpinTask,
					luckyRewardTask,
					singleGemTask,
					multipleGemTask,
					multipleCoinTask,
					singleCoinTask);

				if (this == null)
				{
					return;
				}

				LoadLevelConfigFromCSV(await userLevelTask);
				LoadStageFromCSV(await stageTask);
				LoadGearFromCSV(await gearTask);
				LoadPlayerData();
				LoadExtractDataFromCSV(await extractDataTask);
				LoadGemFromCSV(await gemTask);
				LoadLuckySpinFromCSV(await luckySpinTask);
				LoadLuckyRewardFromCSV(await luckyRewardTask);

				singlGem = await singleGemTask;
				GetMulteGem = await multipleGemTask;
				MultCoin = await multipleCoinTask;
				singleCoin = await singleCoinTask;

				RegisterFacadePayTypeHandlers();
				lock (initializationLock)
				{
					isInitialized = true;
				}
			}
			catch (Exception exception)
			{
				Debug.LogError(
					"UserDataManager initialization failed: " +
					exception.Message);
				throw;
			}
		}

		private void RegisterFacadePayTypeHandlers()
		{
			if (facadePayTypeHandlersRegistered)
			{
				return;
			}

			FacadePayTypeExtend.GetSingleCoin += GetSingleCoin;
			FacadePayTypeExtend.GetMultCoinCoinAct += GetMultCoinCoinAct;
			FacadePayTypeExtend.GetSingleGem += GetSingleGem;
			FacadePayTypeExtend.GetMulteGem += GetMulteGemAct;
			facadePayTypeHandlersRegistered = true;
		}

		private Sprite GetSingleCoin()
		{
			return singleCoin;
		}

		private Sprite GetMultCoinCoinAct()
		{
			return MultCoin;
		}

		private Sprite GetSingleGem()
		{
			return singlGem;
		}

		private Sprite GetMulteGemAct()
		{
			return GetMulteGem;
		}

		public void Load()
		{
			// Explicitly initialized by the Mahjong host after all source managers are awake.
		}

		public static List<string[]> GetCsvRows(string csvName)
		{
			string path = NormalizeCsvResourcePath(csvName);
			lock (csvRowsLock)
			{
				List<string[]> cached;
				if (csvRowsCache.TryGetValue(path, out cached))
				{
					if (cached != null && cached.Count > 0)
					{
						return cached;
					}

					csvRowsCache.Remove(path);
				}

				if (reportedCsvCacheMisses.Add(path))
				{
					Debug.LogWarning(
						"CSV is not ready in the synchronous cache: " + path +
						". Await GetCsvRowsAsync instead.");
				}
			}

			_ = GetCsvRowsAsync(path);
			return new List<string[]>();
		}

		public static Task<List<string[]>> GetCsvRowsAsync(string csvName)
		{
			string path = NormalizeCsvResourcePath(csvName);
			lock (csvRowsLock)
			{
				List<string[]> cached;
				if (csvRowsCache.TryGetValue(path, out cached))
				{
					if (cached != null && cached.Count > 0)
					{
						return Task.FromResult(cached);
					}

					// Older failed loads could leave an empty cache entry. Do
					// not let that suppress a real retry.
					csvRowsCache.Remove(path);
				}

				Task<List<string[]>> loadTask;
				if (csvRowsLoadTasks.TryGetValue(path, out loadTask))
				{
					return loadTask;
				}

				int generation = csvConfigurationGeneration;
				loadTask = LoadCsvRowsAsync(path, generation, Framework2Host.Settings);
				csvRowsLoadTasks[path] = loadTask;
				_ = RemoveCsvLoadTaskWhenCompletedAsync(path, loadTask, generation);
				return loadTask;
			}
		}

		private static async Task RemoveCsvLoadTaskWhenCompletedAsync(
			string path,
			Task<List<string[]>> loadTask,
			int generation)
		{
			try
			{
				await loadTask;
			}
			catch
			{
				lock (csvRowsLock)
				{
					if (generation == csvConfigurationGeneration)
					{
						csvRowsCache.Remove(path);
						reportedCsvCacheMisses.Remove(path);
					}
				}
			}
			finally
			{
				lock (csvRowsLock)
				{
					Task<List<string[]>> currentTask;
					if (csvRowsLoadTasks.TryGetValue(path, out currentTask) &&
						ReferenceEquals(currentTask, loadTask))
					{
						csvRowsLoadTasks.Remove(path);
					}
				}
			}
		}

		private static async Task<List<string[]>> LoadCsvRowsAsync(string path, int generation,
			MahjongSlide.Withdrawal.Framework2ProductConfig configuration)
		{
			try
			{
				// Economy tables are materialized once from framework 2's own SO.
				// Keep the original dictionary/formula consumers; CSV is presentation-only.
				if (configuration == null || configuration.economy == null)
					throw new InvalidOperationException("Framework 2 table loading requires its startup product.");
				if (configuration.economy.TryGetTableRows(path, out List<string[]> economyRows))
				{
					lock (csvRowsLock)
					{
						if (generation == csvConfigurationGeneration)
						{
							csvRowsCache[path] = economyRows;
							reportedCsvCacheMisses.Remove(path);
						}
					}
					return economyRows;
				}
				TextAsset asset =
					await AddressableAssetService.LoadAsync<TextAsset>(path);
				if (asset == null)
				{
					if (IsRequiredStartupCsv(path))
					{
						throw new InvalidOperationException(
							"Required CSV Addressable is missing: " + path);
					}

					Debug.LogWarning("CSV Addressable is missing: " + path);
					return new List<string[]>();
				}

				List<string[]> rows = ParseCsvRows(DecodeCsvText(asset));
				if (rows.Count == 0 && IsRequiredStartupCsv(path))
				{
					throw new FormatException(
						"Required CSV contains no rows: " + path);
				}

				lock (csvRowsLock)
				{
					// An old async request may finish after a scene/configuration reload.
					// Return it only to its original caller; never poison the new cache.
					if (generation == csvConfigurationGeneration)
					{
						csvRowsCache[path] = rows;
						reportedCsvCacheMisses.Remove(path);
					}
				}
				return rows;
			}
			catch (Exception exception)
			{
				Debug.LogError(
					"CSV Addressable load or parse failed for " + path + ": " +
					exception.Message);
				if (IsRequiredStartupCsv(path))
				{
					throw;
				}

				// Optional tables remain optional, but the empty result is not
				// cached so a later request can retry the Addressable load.
				return new List<string[]>();
			}
		}

		private static bool IsRequiredStartupCsv(string path)
		{
			return requiredStartupCsvPaths.Contains(path);
		}

		private static string DecodeCsvText(TextAsset asset)
		{
			byte[] bytes = asset != null ? asset.bytes : null;
			if (bytes == null || bytes.Length == 0)
			{
				return string.Empty;
			}

			int offset = bytes.Length >= 3 &&
				bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF
					? 3
					: 0;
			try
			{
				return strictUtf8.GetString(bytes, offset, bytes.Length - offset);
			}
			catch (DecoderFallbackException)
			{
				// The recovered payload has exactly two legacy text assets:
				// Gem and PayRegion. Decode their audited byte set directly so
				// Android IL2CPP does not depend on reflection-loaded I18N.CJK.
				return DecodeRecoveredCp936(bytes);
			}
		}

		private static string DecodeRecoveredCp936(byte[] bytes)
		{
			StringBuilder decoded = new StringBuilder(bytes.Length);
			for (int i = 0; i < bytes.Length; i++)
			{
				byte first = bytes[i];
				if (first < 0x80)
				{
					decoded.Append((char)first);
					continue;
				}
				if (first == 0x80)
				{
					decoded.Append('\u20AC');
					continue;
				}
				if (++i >= bytes.Length)
				{
					throw new DecoderFallbackException(
						"Incomplete recovered CP936 byte sequence.");
				}

				int pair = first << 8 | bytes[i];
				switch (pair)
				{
					case 0xA1EA: decoded.Append('\uFFE1'); break;
					case 0xA3A4: decoded.Append('\uFFE5'); break;
					case 0xB4B0: decoded.Append('\u7A97'); break;
					case 0xB4F3: decoded.Append('\u5927'); break;
					case 0xB5AF: decoded.Append('\u5F39'); break;
					case 0xB5C8: decoded.Append('\u7B49'); break;
					case 0xC5CC: decoded.Append('\u76D8'); break;
					case 0xC9CF: decoded.Append('\u4E0A'); break;
					case 0xCAAF: decoded.Append('\u77F3'); break;
					case 0xCFC2: decoded.Append('\u4E0B'); break;
					case 0xCFDE: decoded.Append('\u9650'); break;
					case 0xD0A1: decoded.Append('\u5C0F'); break;
					case 0xD3DA: decoded.Append('\u4E8E'); break;
					case 0xD7AA: decoded.Append('\u8F6C'); break;
					case 0xD7EA: decoded.Append('\u94BB'); break;
					default:
						throw new DecoderFallbackException(
							"Unsupported recovered CP936 byte pair 0x" +
							pair.ToString("X4", CultureInfo.InvariantCulture) + ".");
				}
			}
			return decoded.ToString();
		}

		public static List<string[]> ParseCsvRows(string csvText)
		{
			List<string[]> rows = new List<string[]>();
			if (string.IsNullOrEmpty(csvText))
			{
				return rows;
			}

			List<string> row = new List<string>();
			StringBuilder value = new StringBuilder();
			bool quoted = false;
			for (int i = 0; i < csvText.Length; i++)
			{
				char character = csvText[i];
				if (character == ',')
				{
					if (quoted)
					{
						value.Append(character);
					}
					else
					{
						row.Add(value.ToString());
						value.Length = 0;
					}
				}
				else if (character == '"')
				{
					if (quoted && i + 1 < csvText.Length &&
						csvText[i + 1] == '"')
					{
						value.Append('"');
						i++;
					}
					else
					{
						quoted = !quoted;
					}
				}
				else if ((character == '\r' || character == '\n') && !quoted)
				{
					if (character == '\r' && i + 1 < csvText.Length &&
						csvText[i + 1] == '\n')
					{
						i++;
					}
					row.Add(value.ToString());
					value.Length = 0;
					if (!IsCsvRowEmpty(row))
					{
						rows.Add(row.ToArray());
					}
					row = new List<string>();
					quoted = false;
				}
				else
				{
					value.Append(character);
				}
			}

			if (value.Length > 0 || row.Count > 0)
			{
				row.Add(value.ToString());
				if (!IsCsvRowEmpty(row))
				{
					rows.Add(row.ToArray());
				}
			}
			if (quoted)
			{
				throw new FormatException("CSV contains an unterminated quoted field.");
			}
			return rows;
		}

		public static List<string> ParseCsvLineValues(string line)
		{
			List<string> values = new List<string>();
			if (line == null)
			{
				return values;
			}
			StringBuilder value = new StringBuilder();
			bool quoted = false;
			for (int i = 0; i < line.Length; i++)
			{
				char character = line[i];
				if (character == '"')
				{
					if (quoted && i + 1 < line.Length && line[i + 1] == '"')
					{
						value.Append('"');
						i++;
					}
					else
					{
						quoted = !quoted;
					}
				}
				else if (character == ',' && !quoted)
				{
					values.Add(value.ToString().Trim());
					value.Length = 0;
				}
				else
				{
					value.Append(character);
				}
			}
			values.Add(value.ToString().Trim());
			return values;
		}

		private static bool IsCsvRowEmpty(List<string> row)
		{
			if (row == null || row.Count == 0)
			{
				return true;
			}
			for (int i = 0; i < row.Count; i++)
			{
				if (!string.IsNullOrWhiteSpace(row[i]))
				{
					return false;
				}
			}
			return true;
		}

		private static string NormalizeCsvResourcePath(string csvName)
		{
			string path = (csvName ?? string.Empty).Replace('\\', '/').Trim();
			if (path.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) ||
				path.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
			{
				path = path.Substring(0, path.Length - 4);
			}
			return path.StartsWith("csv/", StringComparison.OrdinalIgnoreCase)
				? path
				: "csv/" + path.TrimStart('/');
		}

		private static List<Dictionary<string, object>> LoadDictionaryRows(string csvName)
		{
			return LoadDictionaryRows(GetCsvRows(csvName));
		}

		private static List<Dictionary<string, object>> LoadDictionaryRows(
			List<string[]> rows)
		{
			List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
			if (rows == null || rows.Count < 2)
			{
				return result;
			}
			string[] headers = rows[0];
			for (int rowIndex = 1; rowIndex < rows.Count; rowIndex++)
			{
				string[] values = rows[rowIndex];
				if (values == null)
				{
					continue;
				}
				Dictionary<string, object> row =
					new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
				int count = Mathf.Min(headers.Length, values.Length);
				for (int column = 0; column < count; column++)
				{
					string header = headers[column] != null ? headers[column].Trim() : string.Empty;
					if (!string.IsNullOrEmpty(header))
					{
						row[header] = values[column] != null ? values[column].Trim() : string.Empty;
					}
				}
				if (row.Count > 0)
				{
					result.Add(row);
				}
			}
			return result;
		}

		private static bool TryGetInt(Dictionary<string, object> row, string key, out int value)
		{
			value = 0;
			object raw;
			return row != null && row.TryGetValue(key, out raw) &&
				int.TryParse(raw as string, NumberStyles.Integer,
					CultureInfo.InvariantCulture, out value);
		}

		private static bool TryGetFloat(Dictionary<string, object> row, string key, out float value)
		{
			value = 0f;
			object raw;
			return row != null && row.TryGetValue(key, out raw) &&
				float.TryParse(raw as string, NumberStyles.Float,
					CultureInfo.InvariantCulture, out value);
		}

		private static bool TryGetDouble(Dictionary<string, object> row, string key, out double value)
		{
			value = 0.0;
			object raw;
			return row != null && row.TryGetValue(key, out raw) &&
				double.TryParse(raw as string, NumberStyles.Float,
					CultureInfo.InvariantCulture, out value);
		}

		private static Dictionary<string, object> FindRowById(
			List<Dictionary<string, object>> rows,
			int id)
		{
			if (rows == null)
			{
				return null;
			}
			foreach (Dictionary<string, object> row in rows)
			{
				int rowId;
				if ((TryGetInt(row, "id", out rowId) || TryGetInt(row, "sn", out rowId)) &&
					rowId == id)
				{
					return row;
				}
			}
			return null;
		}

		private void LoadLevelConfigFromCSV(List<string[]> rows = null)
		{
			levelDataList.Clear();
			foreach (Dictionary<string, object> row in rows != null
				? LoadDictionaryRows(rows)
				: LoadDictionaryRows("UserLevel"))
			{
				int sn;
				int level;
				int needExp;
				if (TryGetInt(row, "sn", out sn) &&
					TryGetInt(row, "level", out level) &&
					TryGetInt(row, "nextLvNeedExp", out needExp))
				{
					levelDataList.Add(new UserLevelData { sn = sn, level = level, needExp = needExp });
				}
			}
		}

		private void LoadStageFromCSV(List<string[]> rows = null)
		{
			stageData.Clear();
			stageData.AddRange(rows != null
				? LoadDictionaryRows(rows)
				: LoadDictionaryRows("Stage"));
		}

		private void LoadGearFromCSV(List<string[]> rows = null)
		{
			gearData.Clear();
			gearData.AddRange(rows != null
				? LoadDictionaryRows(rows)
				: LoadDictionaryRows("Gear"));
		}

		private void LoadGemFromCSV(List<string[]> rows = null)
		{
			gemData.Clear();
			gemData.AddRange(rows != null
				? LoadDictionaryRows(rows)
				: LoadDictionaryRows("Gem"));
		}

		private void LoadPayRegionFromCSV(List<string[]> rows = null)
		{
			PayRegionData.Clear();
			PayRegionData.AddRange(rows != null
				? LoadDictionaryRows(rows)
				: LoadDictionaryRows("PayRegion"));
		}

		private void LoadExtractDataFromCSV(List<string[]> rows = null)
		{
			barrageData.Clear();
			barrageData.AddRange(rows != null
				? LoadDictionaryRows(rows)
				: LoadDictionaryRows("Extractdata"));
		}

		private void LoadLuckySpinFromCSV(List<string[]> rows = null)
		{
			luckySpinData.Clear();
			foreach (Dictionary<string, object> row in rows != null
				? LoadDictionaryRows(rows)
				: LoadDictionaryRows("LuckySpin"))
			{
				int sn;
				int type;
				float count;
				double probability;
				if (TryGetInt(row, "sn", out sn) &&
					TryGetInt(row, "type", out type) &&
					TryGetFloat(row, "count", out count) &&
					TryGetDouble(row, "probability", out probability))
				{
					luckySpinData[sn] = new ConfLuckySpin
					{
						Sn = sn,
						Type = type,
						Count = count,
						Probability = probability
					};
				}
			}
		}

		private void LoadLevelRewardFromCSV()
		{
			levelRewardData.Clear();
			foreach (Dictionary<string, object> row in LoadDictionaryRows("LevelReward"))
			{
				int level;
				object rewardValue;
				if (TryGetInt(row, "level", out level) &&
					row.TryGetValue("reward", out rewardValue))
				{
					levelRewardData[level] = ParseLevelRewardItems(
						rewardValue as string);
				}
			}
		}

		private List<LevelRewardStatic.RewardItem> ParseLevelRewardItems(string rewardValue)
		{
			List<LevelRewardStatic.RewardItem> result =
				new List<LevelRewardStatic.RewardItem>();
			if (string.IsNullOrWhiteSpace(rewardValue))
			{
				return result;
			}
			string[] tokens = rewardValue.Split(new[] { ';', '|', ',' },
				StringSplitOptions.RemoveEmptyEntries);
			foreach (string token in tokens)
			{
				LevelRewardStatic.RewardItem item;
				if (TryParseLevelRewardToken(token, out item))
				{
					result.Add(item);
				}
			}
			return result;
		}

		private bool TryParseLevelRewardToken(string token, out LevelRewardStatic.RewardItem rewardItem)
		{
			rewardItem = null;
			if (string.IsNullOrWhiteSpace(token))
			{
				return false;
			}
			string[] values = token.Split(new[] { ':', '*', '_' },
				StringSplitOptions.RemoveEmptyEntries);
			int type;
			float amount;
			if (values.Length < 2 ||
				!int.TryParse(values[0].Trim(), NumberStyles.Integer,
					CultureInfo.InvariantCulture, out type) ||
				!float.TryParse(values[1].Trim(), NumberStyles.Float,
					CultureInfo.InvariantCulture, out amount))
			{
				return false;
			}
			rewardItem = new LevelRewardStatic.RewardItem { type = type, amount = amount };
			return true;
		}

		internal void LoadLuckyRewardFromCSV()
		{
			LoadLuckyRewardFromCSV(null);
		}

		private void LoadLuckyRewardFromCSV(List<string[]> rows)
		{
			luckyRewardLevels.Clear();
			luckyRewardIconLevels.Clear();
			if (TryApplyConfiguredLuckyRewardLevels())
			{
				return;
			}
			foreach (Dictionary<string, object> row in rows != null
				? LoadDictionaryRows(rows)
				: LoadDictionaryRows("LuckyReward"))
			{
				int level;
				int showIcon;
				if (TryGetInt(row, "Level", out level))
				{
					luckyRewardLevels.Add(level);
					if (TryGetInt(row, "ShowIcon", out showIcon) && showIcon == 1)
					{
						luckyRewardIconLevels.Add(level);
					}
				}
			}
		}

		private bool TryApplyConfiguredLuckyRewardLevels()
		{
			if (GameDefines.ConfiguredLuckyRewardLevels == null ||
				GameDefines.ConfiguredLuckyRewardLevels.Count == 0)
			{
				return false;
			}
			foreach (int level in GameDefines.ConfiguredLuckyRewardLevels)
			{
				if (level > 0)
				{
					luckyRewardLevels.Add(level);
					luckyRewardIconLevels.Add(level);
				}
			}
			return luckyRewardLevels.Count > 0;
		}

		public void RefreshLuckyRewardConfig()
		{
			LoadLuckyRewardFromCSV();
		}

		public Dictionary<string, object> getStageData(int id)
		{
			return FindRowById(stageData, id);
		}

		public Dictionary<string, object> getGearData(int id)
		{
			return FindRowById(gearData, id);
		}

		public List<Dictionary<string, object>> getAllGearData()
		{
			return gearData;
		}

		public List<Dictionary<string, object>> getAllBarrageData()
		{
			return barrageData;
		}

		public List<Dictionary<string, object>> getAllGemData()
		{
			return gemData;
		}

		public List<Dictionary<string, object>> getAllPayRegionData()
		{
			return PayRegionData;
		}

		public Dictionary<string, object> getGemData(int id)
		{
			return FindRowById(gemData, id);
		}

		public List<LevelRewardStatic.RewardItem> GetLevelRewards(int level)
		{
			List<LevelRewardStatic.RewardItem> rewards;
			if (!levelRewardData.TryGetValue(level, out rewards))
			{
				return null;
			}

			return rewards.Select(reward =>
				new LevelRewardStatic.RewardItem
				{
					type = reward.type,
					amount = reward.amount
				}).ToList();
		}

		public Dictionary<int, ConfLuckySpin> GetLuckySpinData()
		{
			return luckySpinData.ToDictionary(
				pair => pair.Key,
				pair => new ConfLuckySpin
				{
					Sn = pair.Value.Sn,
					Type = pair.Value.Type,
					Count = pair.Value.Count,
					Probability = pair.Value.Probability
				});
		}

		public bool IsLuckyRewardLevel(int level)
		{
			return luckyRewardLevels != null && luckyRewardLevels.Contains(level);
		}

		public bool ShouldShowLuckyRewardIcon(int level)
		{
			return luckyRewardIconLevels != null && luckyRewardIconLevels.Contains(level);
		}

		private void LoadPlayerData()
		{
			playerData.userName = SPlayerPrefs.HasKey(
				FacadePlayerPrefExtend.userName)
					? SPlayerPrefs.GetString(
						FacadePlayerPrefExtend.userName,
						string.Empty)
					: GetRandomName();
			playerData.userID = SPlayerPrefs.HasKey(
				FacadePlayerPrefExtend.userID)
					? SPlayerPrefs.GetString(
						FacadePlayerPrefExtend.userID,
						string.Empty)
					: GetRandomID();
			playerData.userLevel = SPlayerPrefs.GetInt(
				FacadePlayerPrefExtend.userLevel,
				0);
			playerData.userExp = SPlayerPrefs.GetInt(
				FacadePlayerPrefExtend.userExp,
				0);
			MahjongProductAnalytics.Track("f2_user_level_loaded", "level", playerData.userLevel, "exp", playerData.userExp);
		}

		private string GetRandomName()
		{
			char[] nameChars = GameDefines.nameString.ToCharArray();
			char first = nameChars[UnityEngine.Random.Range(0, nameChars.Length)];
			char second = nameChars[UnityEngine.Random.Range(0, nameChars.Length)];
			string playerName = string.Format("Player_{0}{1}", first, second);
			SPlayerPrefs.SetString(
				FacadePlayerPrefExtend.userName,
				playerName);
			SPlayerPrefs.Save();
			return playerName;
		}

		private string GetRandomID()
		{
			string playerId = Guid.NewGuid().ToString();
			SPlayerPrefs.SetString(FacadePlayerPrefExtend.userID, playerId);
			SPlayerPrefs.Save();
			return playerId;
		}

		public void AddExp(int value)
		{
			if (playerData == null ||
				levelDataList == null ||
				playerData.userLevel >= levelDataList.Count - 1)
			{
				return;
			}
			int analyticsLevelBefore = playerData.userLevel;
			int analyticsExpBefore = playerData.userExp;
			playerData.userExp += value;
			CheckNextLevel();
			MahjongProductAnalytics.Track("f2_user_exp_changed", "exp_before", analyticsExpBefore, "exp_after", playerData.userExp, "requested_delta", value, "level_before", analyticsLevelBefore, "level_after", playerData.userLevel, "next_level_exp", GetNextLevelNeedExp());
			SPlayerPrefs.SetInt(
				FacadePlayerPrefExtend.userLevel,
				playerData.userLevel);
			SPlayerPrefs.SetInt(
				FacadePlayerPrefExtend.userExp,
				playerData.userExp);
			SPlayerPrefs.Save();
		}

		private void CheckNextLevel()
		{
			if (playerData == null)
			{
				return;
			}
			int required = GetNextLevelNeedExp();
			while (required > 0 && playerData.userExp >= required)
			{
				playerData.userExp -= required;
				playerData.userLevel++;
				required = GetNextLevelNeedExp();
			}
		}

		public int GetNextLevelNeedExp()
		{
			if (playerData == null || levelDataList == null)
			{
				return 0;
			}
			int levelIndex = playerData.userLevel;
			if (levelIndex >= levelDataList.Count - 1)
			{
				return 0;
			}
			if (levelIndex < levelDataList.Count)
			{
				return levelDataList[levelIndex].needExp;
			}
			UserLevelData data = levelDataList.Find(
				item => item != null && item.level == levelIndex + 1);
			return data != null ? data.needExp : 100;
		}

		public (string, int) GetRandomBarragePlayAndStage()
		{
			Dictionary<string, object> row = GetRandomBarrageData();
			if (row == null)
			{
				return (string.Empty, 0);
			}
			object player;
			int stage;
			return (row.TryGetValue("play", out player)
					? player as string ?? string.Empty
					: string.Empty,
				TryGetInt(row, "Stage", out stage) ? stage : 0);
		}

		public Dictionary<string, object> GetRandomBarrageData()
		{
			return barrageData != null && barrageData.Count > 0
				? barrageData[UnityEngine.Random.Range(0, barrageData.Count)]
				: null;
		}

		public int GetExp()
		{
			return playerData != null ? playerData.userExp : 0;
		}

		public int GetUserLevel()
		{
			return playerData != null ? playerData.userLevel : 0;
		}

		public string GetUserName()
		{
			return playerData != null ? playerData.userName : string.Empty;
		}

		public string GetUserID()
		{
			return playerData != null ? playerData.userID : string.Empty;
		}

		internal void RefreshHostRegion()
		{
			ApplyRegionCodeForTesting(Framework2Host.Region != null ? Framework2Host.Region() : "US");
		}

		private void GetLanguageData()
		{
			ApplyRegionalProfile(GetStartupLocalizationCountryCode());
		}

		private bool ApplyRegionalProfile(string requestedCode)
		{
			var database = Framework2Host.Settings?.currencyDatabase;
			var profile = database?.Resolve(requestedCode);
			if (profile == null) return false;
			countryCode = profile.Id;
			targetLanguage = profile.CultureInfo;
			var gameManager = GameManagerWZ.instance;
			if (gameManager != null)
			{
				gameManager.mark = profile.Format;
			}
			return database.Find(requestedCode) != null;
		}

		private static string GetStartupLocalizationCountryCode()
		{
			// The host supplies the selected region in Editor and Player alike.
			return Framework2Host.Region != null ? Framework2Host.Region() : "US";
		}

		internal static string GetLocalizationCountryCode(SystemLanguage systemLanguage)
		{
			return Framework2Host.Settings?.currencyDatabase?.Resolve(
				WaterSort.WaterSortLocalizationData.ResolveCountryCode(systemLanguage))?.Id ?? "US";
		}

		public string GetCurrentRegionCodeForTesting() => countryCode;

		internal void ApplyUnmatchedRegionForTesting(string fallbackCountryCode)
		{
			ApplyRegionCodeForTesting("US");
		}

		public List<string> GetAvailableRegionCodesForTesting()
		{
			var result = new List<string>();
			var database = Framework2Host.Settings?.currencyDatabase;
			if (database != null)
				foreach (var profile in database.Profiles)
					if (profile != null) result.Add(profile.Id);
			return result;
		}

		public bool ApplyRegionCodeForTesting(string requestedCode)
		{
			bool found = ApplyRegionalProfile(requestedCode);
			ApplyLanguage();
			RefreshRegionPresentationForTesting();
			return found;
		}

		private void RefreshRegionPresentationForTesting()
		{
			_ = RefreshRegionPresentationForTestingAsync();
		}

		private async Task RefreshRegionPresentationForTestingAsync()
		{
			ImageLocalizationManager imageManager =
				ImageLocalizationManager.Instance;
			if (imageManager != null)
			{
				Task<Sprite> multipleCoinTask = imageManager.GetSpriteAsync("1001");
				Task<Sprite> singleCoinTask = imageManager.GetSpriteAsync("1002");
				await Task.WhenAll(multipleCoinTask, singleCoinTask);
				if (this == null)
				{
					return;
				}
				MultCoin = await multipleCoinTask;
				singleCoin = await singleCoinTask;
			}

			GameManagerWZ gameManager = GameManagerWZ.instance;
			if (gameManager != null)
			{
				gameManager.UpdateCoinUI();
			}
			UIManager manager = UIManager.instance;
			if (manager == null)
			{
				return;
			}
			manager.GetView<GameView>(EUIType.GameView)?.RefreshStageUI();
			manager.GetView<GamePlayerView>(
				EUIType.GamePlayerView)?.RefreshStageUI();
		}

		private void ApplyLanguage()
		{
			LanguageType language = GetConfiguredLanguage();
			SetLanguage(language);
			Debug.Log(
				"Applied language: " + LanguageTypeContract.GetCode(language));
		}

		private async Task ApplyLanguageAsync()
		{
			LanguageType language = GetConfiguredLanguage();
			await SetLanguageAsync(language);
			Debug.Log(
				"Applied language: " + LanguageTypeContract.GetCode(language));
		}

		private LanguageType GetConfiguredLanguage()
		{
			return string.IsNullOrEmpty(targetLanguage)
				? initialLanguage
				: ConvertStringToLanguageType(targetLanguage);
		}

		private LanguageType ConvertStringToLanguageType(string lang)
		{
			LanguageType language;
			return LanguageTypeContract.TryParseCode(lang, out language)
				? language
				: LanguageType.English;
		}

		private void SetLanguage(LanguageType language)
		{
			if (LocalizationManager.Instance != null)
			{
				LocalizationManager.Instance.SetLanguage(language);
			}
			else
			{
				Debug.LogError("LocalizationManager is unavailable.");
			}
			if (ImageLocalizationManager.Instance != null)
			{
				ImageLocalizationManager.Instance.SetLanguage(language);
			}
			else
			{
				Debug.LogError("ImageLocalizationManager is unavailable.");
			}
		}

		private async Task SetLanguageAsync(LanguageType language)
		{
			if (LocalizationManager.Instance != null)
			{
				LocalizationManager.Instance.SetLanguage(language);
			}
			else
			{
				Debug.LogError("LocalizationManager is unavailable.");
			}

			if (ImageLocalizationManager.Instance != null)
			{
				await ImageLocalizationManager.Instance.SetLanguageAsync(language);
			}
			else
			{
				Debug.LogError("ImageLocalizationManager is unavailable.");
			}
		}

		public void Dispose()
		{
			UnregisterFacadePayTypeHandlers();
		}

		protected override void OnDestroy()
		{
			UnregisterFacadePayTypeHandlers();
			base.OnDestroy();
		}

		private void UnregisterFacadePayTypeHandlers()
		{
			if (!facadePayTypeHandlersRegistered)
			{
				return;
			}

			FacadePayTypeExtend.GetSingleCoin -= GetSingleCoin;
			FacadePayTypeExtend.GetMultCoinCoinAct -= GetMultCoinCoinAct;
			FacadePayTypeExtend.GetSingleGem -= GetSingleGem;
			FacadePayTypeExtend.GetMulteGem -= GetMulteGemAct;
			facadePayTypeHandlersRegistered = false;
		}
	}
}
