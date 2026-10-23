using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using UnityEngine;

namespace WZSDK
{
	public static class SPlayerPrefs
	{
		public const string Separator1 = ",";

		public const string Separator2 = "/";

		public static void SetInt(string key, int value)
		{
			PlayerPrefs.SetInt(key, value);
		}

		public static int GetInt(string key, int defaultValue = 0)
		{
			return PlayerPrefs.HasKey(key)
				? PlayerPrefs.GetInt(key)
				: defaultValue;
		}

		public static void SetFloat(string key, float value)
		{
			PlayerPrefs.SetFloat(key, value);
		}

		public static float GetFloat(string key, float defaultValue = 0f)
		{
			return PlayerPrefs.GetFloat(key, defaultValue);
		}

		public static void SetString(string key, string value)
		{
			PlayerPrefs.SetString(key, value);
		}

		public static string GetString(string key, string defaultValue = "")
		{
			return PlayerPrefs.HasKey(key)
				? PlayerPrefs.GetString(key)
				: defaultValue;
		}

		public static void SetDouble(string key, double value)
		{
			PlayerPrefs.SetString(
				key,
				value.ToString("R", CultureInfo.InvariantCulture));
		}

		public static double GetDouble(string key, double defaultValue = 0.0)
		{
			return PlayerPrefs.HasKey(key)
				? DeserializeValue<double>(PlayerPrefs.GetString(key))
				: defaultValue;
		}

		public static void SetBool(string key, bool boolean)
		{
			SetInt(key, boolean ? 1 : 0);
		}

		public static bool GetBool(string key, bool defaultValue = false)
		{
			return PlayerPrefs.HasKey(key)
				? PlayerPrefs.GetInt(key) == 1
				: defaultValue;
		}

		public static void SetDateTime(string key, DateTime value)
		{
			PlayerPrefs.SetString(
				key,
				value.ToString("O", CultureInfo.InvariantCulture));
		}

		public static DateTime GetDateTime(string key, DateTime defaultValue = default(DateTime))
		{
			return PlayerPrefs.HasKey(key)
				? DeserializeValue<DateTime>(PlayerPrefs.GetString(key))
				: defaultValue;
		}

		public static void SetListTem<T>(string key, List<T> list)
		{
			PlayerPrefs.SetString(key, JsonConvert.SerializeObject(list));
		}

		public static List<T> GetListTem<T>(string key, List<T> defaultValue = null)
		{
			return PlayerPrefs.HasKey(key)
				? JsonConvert.DeserializeObject<List<T>>(PlayerPrefs.GetString(key))
				: defaultValue;
		}

		public static void SetList<T>(string key, List<T> list, string sepa = ",")
		{
			List<string> values = new List<string>();
			if (list != null)
			{
				foreach (T item in list)
				{
					values.Add(SerializeValue(item));
				}
			}
			PlayerPrefs.SetString(key, string.Join(sepa, values));
		}

		public static List<T> GetList<T>(string key, List<T> defaultValue = null, string sepa = ",")
		{
			if (!PlayerPrefs.HasKey(key))
			{
				return defaultValue;
			}

			try
			{
				string[] values = PlayerPrefs.GetString(key).Split(
					sepa, StringSplitOptions.None);
				List<T> result = new List<T>();
				foreach (string value in values)
				{
					result.Add(DeserializeValue<T>(value));
				}
				return result;
			}
			catch (Exception)
			{
				Debug.LogWarning("List '" + key + "' parsing failed");
				return defaultValue;
			}
		}

		public static void SetStack<T>(string key, Stack<T> stack, string sepa = ",")
		{
			SetList(key, new List<T>(stack), sepa);
		}

		public static Stack<T> GetStack<T>(string key, Stack<T> defaultValue = null, string sepa = ",")
		{
			if (!PlayerPrefs.HasKey(key))
			{
				return defaultValue;
			}

			try
			{
				string[] values = PlayerPrefs.GetString(key).Split(
					sepa, StringSplitOptions.None);
				Array.Reverse(values);
				Stack<T> result = new Stack<T>();
				foreach (string value in values)
				{
					result.Push(DeserializeValue<T>(value));
				}
				return result;
			}
			catch (Exception)
			{
				Debug.LogWarning("Stack '" + key + "' parsing failed");
				return defaultValue;
			}
		}

		public static void SetQueue<T>(string key, Queue<T> queue, string sepa = ",")
		{
			SetList(key, new List<T>(queue), sepa);
		}

		public static Queue<T> GetQueue<T>(string key, Queue<T> defaultValue = null, string sepa = ",")
		{
			if (!PlayerPrefs.HasKey(key))
			{
				return defaultValue;
			}

			try
			{
				string[] values = PlayerPrefs.GetString(key).Split(
					sepa, StringSplitOptions.None);
				Queue<T> result = new Queue<T>();
				foreach (string value in values)
				{
					result.Enqueue(DeserializeValue<T>(value));
				}
				return result;
			}
			catch (Exception)
			{
				Debug.LogWarning("Queue '" + key + "' parsing failed");
				return defaultValue;
			}
		}

		public static void SetDictionary<T, U>(string key, Dictionary<T, U> dictionary, string sepa = ",", string sepa2 = "/")
		{
			List<string> keys = new List<string>();
			List<string> values = new List<string>();
			if (dictionary != null)
			{
				foreach (T item in dictionary.Keys)
				{
					keys.Add(SerializeValue(item));
				}
				foreach (U item in dictionary.Values)
				{
					values.Add(SerializeValue(item));
				}
			}
			PlayerPrefs.SetString(
				key,
				string.Join(sepa, keys) + sepa2 + string.Join(sepa, values));
		}

		public static Dictionary<T, U> GetDictionary<T, U>(string key, Dictionary<T, U> defaultValue = null, string sepa = ",", string sepa2 = "/")
		{
			if (!PlayerPrefs.HasKey(key))
			{
				return defaultValue;
			}

			try
			{
				string[] parts = PlayerPrefs.GetString(key).Split(
					sepa2, StringSplitOptions.None);
				string[] keys = parts[0].Split(
					sepa, StringSplitOptions.None);
				string[] values = parts[1].Split(
					sepa, StringSplitOptions.None);
				Dictionary<T, U> result = new Dictionary<T, U>();
				for (int i = 0; i < keys.Length; i++)
				{
					result.Add(
						DeserializeValue<T>(keys[i]),
						DeserializeValue<U>(values[i]));
				}
				return result;
			}
			catch (Exception)
			{
				Debug.LogWarning("Dictionary '" + key + "' parsing failed");
				return defaultValue;
			}
		}

		private static string SerializeValue<T>(T value)
		{
			if (ReferenceEquals(value, null))
			{
				throw new InvalidOperationException(
					"SPlayerPrefs collection values cannot be null.");
			}

			Type valueType = typeof(T);
			if (valueType.IsEnum)
			{
				Type underlyingType = Enum.GetUnderlyingType(valueType);
				object numericValue = Convert.ChangeType(
					value,
					underlyingType,
					CultureInfo.InvariantCulture);
				return Convert.ToString(
					numericValue,
					CultureInfo.InvariantCulture);
			}

			switch (Type.GetTypeCode(valueType))
			{
				case TypeCode.String:
					return (string)(object)value;
				case TypeCode.Char:
					return new string((char)(object)value, 1);
				case TypeCode.Boolean:
					return (bool)(object)value ? "True" : "False";
				case TypeCode.Single:
					return ((float)(object)value).ToString(
						"R", CultureInfo.InvariantCulture);
				case TypeCode.Double:
					return ((double)(object)value).ToString(
						"R", CultureInfo.InvariantCulture);
				case TypeCode.DateTime:
					return ((DateTime)(object)value).ToString(
						"O", CultureInfo.InvariantCulture);
				case TypeCode.SByte:
				case TypeCode.Byte:
				case TypeCode.Int16:
				case TypeCode.UInt16:
				case TypeCode.Int32:
				case TypeCode.UInt32:
				case TypeCode.Int64:
				case TypeCode.UInt64:
				case TypeCode.Decimal:
					return Convert.ToString(
						value,
						CultureInfo.InvariantCulture);
				default:
					throw new NotSupportedException(
						"SPlayerPrefs only supports primitive values, decimal, " +
						"DateTime, string, and enums.");
			}
		}

		private static T DeserializeValue<T>(string value)
		{
			Type valueType = typeof(T);
			if (valueType.IsEnum)
			{
				Type underlyingType = Enum.GetUnderlyingType(valueType);
				object numericValue = ChangeTypeWithLegacyCultureFallback(
					value,
					underlyingType);
				return (T)Enum.ToObject(valueType, numericValue);
			}

			object result;
			switch (Type.GetTypeCode(valueType))
			{
				case TypeCode.String:
					result = value;
					break;
				case TypeCode.Char:
					if (value == null || value.Length != 1)
					{
						throw new FormatException(
							"A persisted character must contain exactly one character.");
					}
					result = value[0];
					break;
				case TypeCode.DateTime:
					result = ParseDateTimeWithLegacyFallback(value);
					break;
				case TypeCode.Boolean:
				case TypeCode.SByte:
				case TypeCode.Byte:
				case TypeCode.Int16:
				case TypeCode.UInt16:
				case TypeCode.Int32:
				case TypeCode.UInt32:
				case TypeCode.Int64:
				case TypeCode.UInt64:
				case TypeCode.Single:
				case TypeCode.Double:
				case TypeCode.Decimal:
					result = ChangeTypeWithLegacyCultureFallback(value, valueType);
					break;
				default:
					throw new NotSupportedException(
						"SPlayerPrefs only supports primitive values, decimal, " +
						"DateTime, string, and enums.");
			}
			return (T)result;
		}

		private static object ChangeTypeWithLegacyCultureFallback(
			string value,
			Type targetType)
		{
			TypeCode typeCode = Type.GetTypeCode(targetType);
			if (typeCode == TypeCode.Single)
			{
				float parsed;
				if (float.TryParse(
					value,
					NumberStyles.Float,
					CultureInfo.InvariantCulture,
					out parsed) ||
					float.TryParse(
						value,
						NumberStyles.Float,
						CultureInfo.CurrentCulture,
						out parsed))
				{
					return parsed;
				}
				throw new FormatException(
					"Persisted floating-point value has an invalid format.");
			}
			if (typeCode == TypeCode.Double)
			{
				double parsed;
				if (double.TryParse(
					value,
					NumberStyles.Float,
					CultureInfo.InvariantCulture,
					out parsed) ||
					double.TryParse(
						value,
						NumberStyles.Float,
						CultureInfo.CurrentCulture,
						out parsed))
				{
					return parsed;
				}
				throw new FormatException(
					"Persisted floating-point value has an invalid format.");
			}
			if (typeCode == TypeCode.Decimal)
			{
				const NumberStyles styles =
					NumberStyles.AllowLeadingWhite |
					NumberStyles.AllowTrailingWhite |
					NumberStyles.AllowLeadingSign |
					NumberStyles.AllowDecimalPoint;
				decimal parsed;
				if (decimal.TryParse(
					value,
					styles,
					CultureInfo.InvariantCulture,
					out parsed) ||
					decimal.TryParse(
						value,
						styles,
						CultureInfo.CurrentCulture,
						out parsed))
				{
					return parsed;
				}
				throw new FormatException(
					"Persisted decimal value has an invalid format.");
			}

			try
			{
				return Convert.ChangeType(
					value,
					targetType,
					CultureInfo.InvariantCulture);
			}
			catch (FormatException)
			{
				return Convert.ChangeType(
					value,
					targetType,
					CultureInfo.CurrentCulture);
			}
		}

		private static DateTime ParseDateTimeWithLegacyFallback(string value)
		{
			DateTime parsed;
			if (DateTime.TryParseExact(
				value,
				"O",
				CultureInfo.InvariantCulture,
				DateTimeStyles.RoundtripKind,
				out parsed))
			{
				return parsed;
			}
			if (DateTime.TryParse(
				value,
				CultureInfo.CurrentCulture,
				DateTimeStyles.None,
				out parsed))
			{
				return parsed;
			}
			return DateTime.Parse(
				value,
				CultureInfo.InvariantCulture,
				DateTimeStyles.None);
		}

		public static void Save()
		{
			PlayerPrefs.Save();
		}

		public static void DeleteAll()
		{
			throw new InvalidOperationException("Use the framework-specific GM reset; global PlayerPrefs deletion is disabled.");
		}

		public static void Delete(string key)
		{
			PlayerPrefs.DeleteKey(key);
		}

		public static bool HasKey(string key)
		{
			return PlayerPrefs.HasKey(key);
		}

		internal static bool GetBool(object failFreeAd, bool v)
		{
			throw new NotImplementedException();
		}
	}
}
