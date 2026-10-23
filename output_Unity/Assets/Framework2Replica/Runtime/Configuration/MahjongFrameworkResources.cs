using System;
using UnityEngine;

namespace MahjongSlide.Withdrawal
{
    /// <summary>
    /// Presentation assets owned by one framework's runtime prefab. Game tuning
    /// data only stores business identifiers; the selected framework resolves
    /// those identifiers through its own resource bindings.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MahjongFrameworkResources : MonoBehaviour
    {

        [Serializable]
        public sealed class CountryFont
        {
            public string country;
            public Font font;
        }

        [Serializable]
        public sealed class IntroductionFont
        {
            public string language;
            public Font font;
        }

        [Serializable]
        public sealed class PayChannel
        {
            public int id;
            public string imageName;
            public Sprite logo;
            public Sprite icon;

            public Sprite Icon => icon != null ? icon : logo;
        }

        [Serializable]
        public sealed class SourceTable
        {
            public string logicalAddress;
            public TextAsset content;
        }

        [Serializable]
        public sealed class TileFace
        {
            public int id;
            public string image;
        }

        [SerializeField] private CountryFont[] countryFonts = Array.Empty<CountryFont>();
        [SerializeField] private IntroductionFont[] introductionFonts = Array.Empty<IntroductionFont>();
        [SerializeField] private PayChannel[] payChannels = Array.Empty<PayChannel>();
        [SerializeField] private SourceTable[] sourceTables = Array.Empty<SourceTable>();
        [SerializeField] private TileFace[] tileFaces = Array.Empty<TileFace>();

        public static MahjongFrameworkResources Active { get; private set; }

        public static void SetActive(MahjongFrameworkResources resources)
        {
            Active = resources;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void ResetActive()
        {
            Active = null;
        }

        public Font FindCurrencyFont(string country)
        {
            if (countryFonts == null || string.IsNullOrEmpty(country)) return null;
            foreach (CountryFont entry in countryFonts)
            {
                if (entry != null && string.Equals(entry.country, country, StringComparison.OrdinalIgnoreCase))
                    return entry.font;
            }
            return null;
        }

        public Font FindIntroductionFont(string language)
        {
            if (introductionFonts == null || string.IsNullOrEmpty(language)) return null;
            foreach (IntroductionFont entry in introductionFonts)
            {
                if (entry != null && string.Equals(entry.language, language, StringComparison.OrdinalIgnoreCase))
                    return entry.font;
            }
            return null;
        }

        public PayChannel FindPayChannel(int id)
        {
            if (payChannels == null) return null;
            foreach (PayChannel entry in payChannels)
            {
                if (entry != null && entry.id == id) return entry;
            }
            return null;
        }

        public TextAsset FindTable(string logicalAddress)
        {
            if (sourceTables == null || string.IsNullOrEmpty(logicalAddress)) return null;
            foreach (SourceTable entry in sourceTables)
            {
                if (entry != null && string.Equals(entry.logicalAddress, logicalAddress, StringComparison.Ordinal))
                    return entry.content;
            }
            return null;
        }

        public string FindTileImage(int id)
        {
            if (tileFaces == null) return string.Empty;
            foreach (TileFace entry in tileFaces)
            {
                if (entry != null && entry.id == id) return entry.image ?? string.Empty;
            }
            return string.Empty;
        }
    }
}
