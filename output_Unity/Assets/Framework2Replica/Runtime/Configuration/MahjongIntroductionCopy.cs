using System;
using UnityEngine;

namespace MahjongSlide.Withdrawal
{
    [CreateAssetMenu(menuName = "Mahjong Slide/Startup Introduction Copy", fileName = "FrameworkIntroduction")]
    public sealed class MahjongIntroductionCopy : ScriptableObject
    {
        public string heading;
        [TextArea(2, 5)] public string[] paragraphs;
        public string continueLabel;
        [SerializeField] private MahjongIntroductionLocale[] locales = Array.Empty<MahjongIntroductionLocale>();

        public MahjongIntroductionLocale ResolveLocale(string language)
        {
            MahjongIntroductionLocale fallback = null;
            for (int index = 0; locales != null && index < locales.Length; index++)
            {
                var locale = locales[index];
                if (locale == null) continue;
                if (string.Equals(locale.language, "en", StringComparison.OrdinalIgnoreCase)) fallback = locale;
                if (string.Equals(locale.language, language, StringComparison.OrdinalIgnoreCase)) return locale;
            }
            return fallback;
        }
    }

    [Serializable]
    public sealed class MahjongIntroductionLocale
    {
        public string language;
        public Font font => MahjongFrameworkResources.Active?.FindIntroductionFont(language);
        public string heading;
        [TextArea(2, 5)] public string[] paragraphs;
        public string continueLabel;
    }
}
