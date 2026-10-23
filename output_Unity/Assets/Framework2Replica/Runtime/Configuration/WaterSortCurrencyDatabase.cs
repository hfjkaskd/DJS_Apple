using System;
using UnityEngine;

namespace WaterSort
{
    [CreateAssetMenu(menuName = "WaterSort/Currency Database", fileName = "WaterSortCurrencyDatabase")]
    public sealed class WaterSortCurrencyDatabase : ScriptableObject
    {
        public const string Address = "watersort/config/currency";

        [SerializeField] private WaterSortCurrencyProfile[] profiles = Array.Empty<WaterSortCurrencyProfile>();
        [SerializeField] private WaterSortPayChannelConfig[] channels = Array.Empty<WaterSortPayChannelConfig>();

        public int Count => profiles == null ? 0 : profiles.Length;
        public WaterSortCurrencyProfile[] Profiles => profiles ?? Array.Empty<WaterSortCurrencyProfile>();

        public WaterSortCurrencyProfile DefaultProfile
        {
            get
            {
                WaterSortCurrencyProfile us = Find("US");
                return us;
            }
        }

        public WaterSortCurrencyProfile Find(string country)
        {
            if (profiles == null || string.IsNullOrEmpty(country))
            {
                return null;
            }

            for (int index = 0; index < profiles.Length; index++)
            {
                WaterSortCurrencyProfile profile = profiles[index];
                if (profile != null && string.Equals(profile.Id, country.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return profile;
                }
            }

            return null;
        }

        public WaterSortCurrencyProfile Resolve(string country)
        {
            return Find(country) ?? DefaultProfile;
        }

        public void SetProfiles(WaterSortCurrencyProfile[] value)
        {
            profiles = value ?? Array.Empty<WaterSortCurrencyProfile>();
        }

        public WaterSortPayChannelConfig FindChannel(int id)
        {
            if (channels != null)
                foreach (var channel in channels)
                    if (channel != null && channel.Id == id) return channel;
            return null;
        }
    }

    [Serializable]
    public sealed class WaterSortCurrencyProfile
    {
        [SerializeField] private string id;
        [SerializeField] private string language;
        [SerializeField] private string cultureInfo;
        [SerializeField] private int[] payTypes = Array.Empty<int>();
        [SerializeField] private string format;
        [SerializeField] private int multiplier = 1;
        [Header("地区表现配置：基础余额不换算，仅显示换算")]
        [SerializeField] private string currencyCode;
        [SerializeField, Min(0)] private double exchangeRate;
        [SerializeField, Range(0, 4)] private int fractionDigits = 2;
        [SerializeField] private string symbolSeparator;
        [SerializeField] private bool symbolAfterAmount;

        public string Id => id ?? string.Empty;
        public string Language => language ?? string.Empty;
        public string CultureInfo => cultureInfo ?? string.Empty;
        public int[] PayTypes => payTypes ?? Array.Empty<int>();
        public string Format => format ?? string.Empty;
        public int Multiplier => multiplier;
        public double ExchangeRate => exchangeRate > 0 ? exchangeRate : Math.Max(1, multiplier);
        public int FractionDigits => Mathf.Clamp(fractionDigits, 0, 4);
        public string CurrencyCode => currencyCode ?? string.Empty;
        public string SymbolSeparator => symbolSeparator ?? string.Empty;
        public bool SymbolAfterAmount => symbolAfterAmount;
        public Font NativeFont => MahjongSlide.Withdrawal.MahjongFrameworkResources.Active?.FindCurrencyFont(Id);
        public int PrimaryChannel => PayTypes.Length > 0 ? PayTypes[0] : 1;

        public WaterSortCurrencyProfile(
            string id,
            string language,
            string cultureInfo,
            int[] payTypes,
            string format,
            int multiplier)
        {
            this.id = id ?? string.Empty;
            this.language = language ?? string.Empty;
            this.cultureInfo = cultureInfo ?? string.Empty;
            this.payTypes = payTypes ?? Array.Empty<int>();
            this.format = format ?? string.Empty;
            this.multiplier = multiplier;
        }
    }
}

