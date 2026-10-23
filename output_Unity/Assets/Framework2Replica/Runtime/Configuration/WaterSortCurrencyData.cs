using System;
using UnityEngine;

namespace WaterSort
{
    [System.Serializable]
    public sealed class WaterSortCurrencyData
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

}
