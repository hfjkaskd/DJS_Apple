using System;
using System.Text;
using UnityEngine;

namespace WaterSort
{
    [System.Serializable]
    public sealed class WaterSortLocalizationData
    {
        public const string Address = "watersort/data/localization";

        [SerializeField] private WaterSortLocale[] locales = Array.Empty<WaterSortLocale>();

        public int LocaleCount => locales == null ? 0 : locales.Length;
        public WaterSortLocale[] Locales => locales ?? Array.Empty<WaterSortLocale>();

        public string Get(string key)
        {
            string countryLanguage = WaterSortCurrencyService.LocalizationLanguage;
            return Get(
                key,
                string.IsNullOrEmpty(countryLanguage)
                    ? ResolveSystemLocaleCode()
                    : countryLanguage);
        }

        public string Get(string key, string localeCode)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            WaterSortLocale fallback = null;
            if (locales != null)
            {
                for (int index = 0; index < locales.Length; index++)
                {
                    WaterSortLocale locale = locales[index];
                    if (locale == null)
                    {
                        continue;
                    }

                    if (string.Equals(locale.Code, "en", StringComparison.OrdinalIgnoreCase))
                    {
                        fallback = locale;
                    }

                    if (string.Equals(locale.Code, localeCode, StringComparison.OrdinalIgnoreCase) &&
                        locale.TryGet(key, out string localized) && !string.IsNullOrWhiteSpace(localized))
                    {
                        return SanitizeLocalizedText(localized);
                    }
                }
            }

            return fallback != null && fallback.TryGet(key, out string english) && !string.IsNullOrWhiteSpace(english)
                ? SanitizeLocalizedText(english)
                : key;
        }

        private static string SanitizeLocalizedText(string value)
        {
            return string.IsNullOrEmpty(value)
                ? string.Empty
                : value.Replace("🎁", string.Empty)
                    .Normalize(NormalizationForm.FormC);
        }

        public void SetLocales(WaterSortLocale[] value)
        {
            locales = value ?? Array.Empty<WaterSortLocale>();
        }

        public static string ResolveLocaleCode(SystemLanguage language)
        {
            switch (language)
            {
                case SystemLanguage.Afrikaans: return "af";
                case SystemLanguage.Chinese:
                case SystemLanguage.ChineseTraditional: return "zh_TW";
                case SystemLanguage.ChineseSimplified: return "en";
                case SystemLanguage.German: return "de";
                case SystemLanguage.Spanish: return "es";
                case SystemLanguage.French: return "fr";
                case SystemLanguage.Hindi: return "hi";
                case SystemLanguage.Indonesian: return "id";
                case SystemLanguage.Italian: return "it";
                case SystemLanguage.Japanese: return "ja";
                case SystemLanguage.Korean: return "ko";
                case SystemLanguage.Dutch: return "nl";
                case SystemLanguage.Portuguese: return "pt";
                case SystemLanguage.Russian: return "ru";
                case SystemLanguage.Swedish: return "sv";
                case SystemLanguage.Thai: return "th";
                case SystemLanguage.Turkish: return "tr";
                case SystemLanguage.Vietnamese: return "vi";
                default: return "en";
            }
        }

        public static string ResolveLocaleCode(string languageCode, string countryCode = "")
        {
            if (string.IsNullOrWhiteSpace(languageCode))
            {
                return "en";
            }

            string normalized = languageCode.Trim().Replace('_', '-').ToLowerInvariant();
            string language = normalized.Split('-')[0];
            switch (language)
            {
                case "af":
                case "de":
                case "es":
                case "fr":
                case "hi":
                case "it":
                case "ja":
                case "ko":
                case "ms":
                case "nl":
                case "pt":
                case "ru":
                case "sv":
                case "th":
                case "tr":
                case "uz":
                case "vi":
                    return language;
                case "id":
                case "in":
                    return "id";
                case "zh":
                    string country = string.IsNullOrWhiteSpace(countryCode)
                        ? string.Empty
                        : countryCode.Trim().ToUpperInvariant();
                    return normalized.Contains("hant") || country == "TW" ||
                        country == "HK" || country == "MO"
                        ? "zh_TW"
                        : "en";
                default:
                    return "en";
            }
        }

        public static string ResolveSystemLocaleCode()
        {
            string language = ReadAndroidLocalePart("getLanguage");
            if (!string.IsNullOrEmpty(language))
            {
                return ResolveLocaleCode(language, ReadAndroidLocalePart("getCountry"));
            }
            return ResolveLocaleCode(Application.systemLanguage);
        }

        public static string ResolveSystemCountryCode()
        {
            string country = ReadAndroidLocalePart("getCountry");
            return string.IsNullOrWhiteSpace(country)
                ? ResolveCountryCode(Application.systemLanguage)
                : country.Trim().ToUpperInvariant();
        }

        static string ReadAndroidLocalePart(string method)
        {
            var culture = System.Globalization.CultureInfo.CurrentUICulture;
            if (method == "getLanguage") return culture.TwoLetterISOLanguageName;
            return culture.IsNeutralCulture ? string.Empty : new System.Globalization.RegionInfo(culture.Name).TwoLetterISORegionName;
        }

        public static string ResolveCountryCode(SystemLanguage language)
        {
            switch (language)
            {
                case SystemLanguage.Afrikaans: return "ZA";
                case SystemLanguage.Chinese:
                case SystemLanguage.ChineseTraditional: return "TW";
                case SystemLanguage.German: return "DE";
                case SystemLanguage.Spanish: return "ES";
                case SystemLanguage.French: return "FR";
                case SystemLanguage.Hindi: return "IN";
                case SystemLanguage.Indonesian: return "ID";
                case SystemLanguage.Italian: return "IT";
                case SystemLanguage.Japanese: return "JP";
                case SystemLanguage.Korean: return "KR";
                case SystemLanguage.Dutch: return "NL";
                case SystemLanguage.Portuguese: return "BR";
                case SystemLanguage.Russian: return "RU";
                case SystemLanguage.Swedish: return "SE";
                case SystemLanguage.Thai: return "TH";
                case SystemLanguage.Turkish: return "TR";
                case SystemLanguage.Vietnamese: return "VN";
                default: return "US";
            }
        }

        public static string ResolveCountryCode(string languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode))
            {
                return "US";
            }

            string normalized = languageCode.Trim().Replace('_', '-');
            int separatorIndex = normalized.IndexOf('-');
            if (separatorIndex >= 0)
            {
                normalized = normalized.Substring(0, separatorIndex);
            }

            switch (normalized.ToLowerInvariant())
            {
                case "af": return "ZA";
                case "de": return "DE";
                case "es": return "ES";
                case "fr": return "FR";
                case "hi": return "IN";
                case "id":
                case "in": return "ID";
                case "it": return "IT";
                case "ja": return "JP";
                case "ko": return "KR";
                case "ms": return "MY";
                case "nl": return "NL";
                case "pt": return "BR";
                case "ru": return "RU";
                case "sv": return "SE";
                case "th": return "TH";
                case "tr": return "TR";
                case "uz": return "UZ";
                case "vi": return "VN";
                default: return "US";
            }
        }
    }

}
