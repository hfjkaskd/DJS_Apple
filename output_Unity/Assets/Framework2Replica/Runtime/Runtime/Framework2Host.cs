using System;
using UnityEngine;
using UnityEngine.UI;
using MahjongSlide.Withdrawal;

namespace WZSDK
{
    // Explicit host boundary: the imported views never initialize a second SDK or game.
    internal static class Framework2Host
    {
        internal static MahjongFramework2Runtime Runtime;
        internal static Framework2ProductConfig Settings { get; private set; }
        internal static string PreferencePrefix { get; private set; }
        internal static string ApplicationScope { get; set; } = "DJS.Apple.B.";
        static string preferenceScope;
        internal static void ApplySettings(Framework2ProductConfig value)
        {
            if (ReferenceEquals(Settings, value) && preferenceScope == ApplicationScope) return;
            Settings = value;
            if (value != null)
            {
                preferenceScope = ApplicationScope;
                PreferencePrefix = preferenceScope + value.savePrefix;
            }
            // A new runtime snapshot owns new table caches, even within one process.
            UserDataManager.ResetConfigurationCache();
            LuckySpinView.ResetConfigurationCache();
        }
        internal static Action<string, Action<bool>> Rewarded;
        internal static Action<string, Action<bool>> Interstitial;
        internal static Func<DateTime> Clock;
        internal static Func<string> Region;
        internal static Func<bool> GameplayActive;
        internal static RectTransform CashTarget;
        internal static Button WithdrawButton;
        internal static DateTime LocalNow => Clock != null ? Clock() : DateTime.Now;
        internal static WaterSort.WaterSortCurrencyProfile Currency => Settings?.currencyDatabase?.Resolve(
            Region != null ? Region() : "US");
        internal static WaterSort.WaterSortPayChannelConfig PaymentChannel =>
            Settings?.currencyDatabase?.FindChannel(Currency?.PrimaryChannel ?? 1);
        internal static void CurrencyChanged() { Runtime?.NotifyCurrencyChanged(); }
        internal static void NextRound() { Runtime?.NotifySourceNextRound(); }
    }

    // Shadows UnityEngine.PlayerPrefs only inside the imported WZSDK namespace.
    // Both direct source PlayerPrefs and SPlayerPrefs therefore use isolated keys.
    internal static class PlayerPrefs
    {
        private static string Key(string key) => (Framework2Host.PreferencePrefix ?? ("DJS.Apple.B." + "NativeFramework2.")) + key;
        internal static bool HasKey(string key) => UnityEngine.PlayerPrefs.HasKey(Key(key));
        internal static int GetInt(string key, int value = 0) => UnityEngine.PlayerPrefs.GetInt(Key(key), value);
        internal static float GetFloat(string key, float value = 0f) => UnityEngine.PlayerPrefs.GetFloat(Key(key), value);
        internal static string GetString(string key, string value = "") => UnityEngine.PlayerPrefs.GetString(Key(key), value);
        internal static void SetInt(string key, int value) => UnityEngine.PlayerPrefs.SetInt(Key(key), value);
        internal static void SetFloat(string key, float value) => UnityEngine.PlayerPrefs.SetFloat(Key(key), value);
        internal static void SetString(string key, string value) => UnityEngine.PlayerPrefs.SetString(Key(key), value);
        internal static void DeleteKey(string key) => UnityEngine.PlayerPrefs.DeleteKey(Key(key));
        internal static void Save() => UnityEngine.PlayerPrefs.Save();
    }
}
