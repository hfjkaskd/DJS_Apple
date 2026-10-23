using System;
using System.IO;
using UnityEngine;

namespace FruitEarningVariants
{
    /// <summary>A session's selection is fixed until its scene has been saved and unloaded.</summary>
    public static class EarningVariantSelection
    {
        private const string SelectionKey = "Apple.EarningVariant.v1";
        private static string storageProfile = "";
        private static bool sessionInitialized;
        public static string SaveDirectory => string.IsNullOrEmpty(storageProfile) ? Application.persistentDataPath :
            Path.Combine(Application.persistentDataPath, "TestProfiles", storageProfile);
        public static string PreferenceKey(string key) => string.IsNullOrEmpty(storageProfile) ? key : "Apple.Profile." + storageProfile + "." + key;
        public static string Framework2Namespace => string.IsNullOrEmpty(storageProfile) ? "DJS.Apple.B." : "DJS.Apple.Profile." + storageProfile + ".B.";
        public static bool IsB { get; private set; }
        public static string Label => IsB ? "B / Framework 2" : "A / Original";
        public static void Initialize()
        {
            Directory.CreateDirectory(SaveDirectory);
            IsB = PlayerPrefs.GetInt(PreferenceKey(SelectionKey), 0) == 1;
            sessionInitialized = true;
        }
        /// <summary>Optional isolated QA profile, configured before a gameplay session is created.</summary>
        public static void ConfigureStorageProfile(string profile)
        {
            if (EarningVariantController.Instance != null) throw new InvalidOperationException("Unload the active session before changing its storage profile.");
            profile = profile ?? "";
            if (sessionInitialized && profile != storageProfile) throw new InvalidOperationException("A storage profile must be selected before the first session starts.");
            if (profile.Length > 64) throw new ArgumentException("Profile must be at most 64 characters.");
            foreach (char value in profile)
                if (!char.IsLetterOrDigit(value) && value != '-' && value != '_') throw new ArgumentException("Invalid profile name.");
            storageProfile = profile;
            Directory.CreateDirectory(SaveDirectory);
        }
        public static void SaveNextSelection(bool useB)
        {
            PlayerPrefs.SetInt(PreferenceKey(SelectionKey), useB ? 1 : 0);
            PlayerPrefs.Save();
        }
        public static string ModelFilePath(string key)
        {
            // Audio/language preferences can be shared. Board, assets and progression cannot.
            bool separate = IsB && key != "SettingModel";
            return Path.Combine(SaveDirectory, (separate ? "Framework2Replica-" : "") + key + ".json");
        }
    }
}
