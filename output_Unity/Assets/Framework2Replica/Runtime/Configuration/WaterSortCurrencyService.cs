using System;
using System.Globalization;
using UnityEngine;

namespace WaterSort
{
    public static class WaterSortCurrencyService
    {
        public const int MinorUnitsPerUnit = 100;

        private static WaterSortCurrencyData database;
        private static WaterSortCurrencyProfile current;
        private static string requestedCountry = string.Empty;
        private static bool hasCountryOverride;

        public static WaterSortCurrencyData Database => database;
        public static WaterSortCurrencyProfile Current => current;
        public static string Country => current == null ? string.Empty : current.Id;
        public static bool HasCountryOverride => hasCountryOverride;
        public static string Language => current == null ? string.Empty : current.Language;
        public static string LocalizationLanguage => string.IsNullOrEmpty(Language) ? "en" : Language;
        public static string Culture => current == null ? string.Empty : current.CultureInfo;
        public static string Symbol => current == null ? string.Empty : current.Format;
        public static int Multiplier => current == null ? 1 : current.Multiplier;

        public static event Action Changed;

        public static void ApplyDatabase(WaterSortCurrencyData value)
        {
            database = value;
            if (database == null)
            {
                ApplyCurrent(null);
                return;
            }

            string country = hasCountryOverride
                ? requestedCountry
                : WaterSortLocalizationDatabase.ResolveSystemCountryCode();
            SelectCountry(country);
        }

        /// <summary>
        /// Selects a currency profile explicitly. Passing null or an empty
        /// value clears the override and returns to the current system language.
        /// </summary>
        /// <returns>True when the requested country exists in the database.</returns>
        public static bool SetCountry(string country)
        {
            string normalized = NormalizeCountry(country);
            bool previousOverride = hasCountryOverride;
            hasCountryOverride = !string.IsNullOrEmpty(normalized);
            requestedCountry = hasCountryOverride
                ? normalized
                : WaterSortLocalizationDatabase.ResolveSystemCountryCode();
            return SelectCountry(requestedCountry, previousOverride != hasCountryOverride);
        }

        public static bool RefreshFromSystemLanguage()
        {
            return SetResolvedSystemCountry(WaterSortLocalizationDatabase.ResolveSystemCountryCode());
        }

        public static bool SetResolvedSystemCountry(string country)
        {
            hasCountryOverride = false;
            requestedCountry = NormalizeCountry(country);
            // Language can change even when the resolved currency profile is the
            // same (system locale versus a forced country-language selection).
            return SelectCountry(requestedCountry, true);
        }

        private static bool SelectCountry(string country, bool forceNotify = false)
        {
            if (database == null)
            {
                return false;
            }

            WaterSortCurrencyProfile exact = database.Find(country);
            ApplyCurrent(exact ?? database.DefaultProfile, forceNotify);
            return exact != null;
        }

        private static void ApplyCurrent(WaterSortCurrencyProfile selected, bool forceNotify = false)
        {
            bool changed = !ReferenceEquals(current, selected);
            current = selected;
            if (changed || forceNotify)
            {
                Changed?.Invoke();
            }
        }

        private static string NormalizeCountry(string country)
        {
            return string.IsNullOrWhiteSpace(country)
                ? string.Empty
                : country.Trim().ToUpperInvariant();
        }

        public static string FormatMoney(long minorUnits)
        {
            return FormatMoney(current ?? database?.DefaultProfile, minorUnits);
        }

        public static string FormatMoney(WaterSortCurrencyProfile profile, long minorUnits)
        {
            return FormatBaseAmount(profile, (decimal)minorUnits / MinorUnitsPerUnit);
        }

        // All ledgers retain base USD units. The regional rate is applied exactly once here.
        public static string FormatBaseAmount(WaterSortCurrencyProfile profile, decimal baseAmount,
            bool includeSymbol = true, bool compact = false)
        {
            decimal amount = baseAmount * (decimal)(profile?.ExchangeRate ?? 1d);
            int digits = profile?.FractionDigits ?? 2;
            amount = decimal.Round(amount, digits, MidpointRounding.AwayFromZero);
            if (compact && decimal.Truncate(amount) == amount) digits = 0;
            string number = Math.Abs(amount).ToString("N" + digits, ResolveCulture(profile));
            string sign = amount < 0 ? "-" : string.Empty;
            if (!includeSymbol) return sign + number;
            string symbol = profile?.Format ?? "$";
            string separator = profile?.SymbolSeparator ?? string.Empty;
            return profile != null && profile.SymbolAfterAmount
                ? sign + number + separator + symbol
                : sign + symbol + separator + number;
        }

        public static string FormatMoneyCompact(long minorUnits)
        {
            return FormatMoneyCompact(current ?? database?.DefaultProfile, minorUnits);
        }

        public static string FormatMoneyCompact(
            WaterSortCurrencyProfile profile,
            long minorUnits)
        {
            return FormatBaseAmount(profile, (decimal)minorUnits / MinorUnitsPerUnit, true, true);
        }

        private static CultureInfo ResolveCulture(WaterSortCurrencyProfile profile)
        {
            if (profile != null && !string.IsNullOrEmpty(profile.CultureInfo))
            {
                try
                {
                    return new CultureInfo(profile.CultureInfo);
                }
                catch (Exception)
                {
                }
            }

            return CultureInfo.GetCultureInfo("en-US");
        }
    }
}
