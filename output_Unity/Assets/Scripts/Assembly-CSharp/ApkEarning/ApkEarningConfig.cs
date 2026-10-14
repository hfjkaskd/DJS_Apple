using System;
using UnityEngine;

namespace FruitApkEarning
{
    [Serializable]
    public sealed class ApkPaymentChannel
    {
        public string Name, SmallIconPath, LargeIconPath;
        public bool UsesEmail, RequiresName = true;

        public string Validate(string name, string email, string phone)
        {
            if (RequiresName && (name == null || name.Trim().Length <= 1)) return "Enter a test name with at least two characters.";
            if (UsesEmail && (email == null || email.Trim().Length <= 5 || email.IndexOf('@') < 1)) return "Enter a valid test email address.";
            if (!UsesEmail && (phone == null || phone.Trim().Length <= 4)) return "Enter a test phone number with at least five digits.";
            return string.Empty;
        }

        public string FormatAccount(string name, string email, string area, string phone)
        { return name + "\n" + (UsesEmail ? email : area + " " + phone); }
    }

    [CreateAssetMenu(menuName = "Fruit/APK Earning Simulation Config")]
    public sealed class ApkEarningConfig : ScriptableObject
    {
        public ApkEarningRules Rules = ApkEarningRules.CreateDefault();
        public int[] DisplayLevels = Array.Empty<int>();
        public ApkPaymentChannel[] PaymentChannels = {
            new ApkPaymentChannel { Name = "Venmo" },
            new ApkPaymentChannel { Name = "Zelle" },
            new ApkPaymentChannel { Name = "PayPal", UsesEmail = true }
        };
        public string OtherChannelIconPath;
        public long DiamondPreviewHundredths = 3000000;
        public float DiamondPreviewSeconds = 2;

        public int GetDisplayLevel(int actual)
        { return DisplayLevels != null && actual >= 0 && actual < DisplayLevels.Length && DisplayLevels[actual] > 0 ? DisplayLevels[actual] : actual; }
    }
}
