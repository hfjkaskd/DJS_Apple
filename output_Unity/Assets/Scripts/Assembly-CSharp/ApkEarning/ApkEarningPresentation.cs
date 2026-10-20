using System;

namespace FruitApkEarning
{
    public static class ApkEarningPresentation
    {
        // Stored hundredths of a diamond -> cents. Keep the fraction until the
        // final currency rounding, including the difference from a tier target.
        public static long DiamondCashCents(long hundredths)
        {
            if (hundredths < 0) throw new ArgumentOutOfRangeException(nameof(hundredths));
            return checked((long)decimal.Round(hundredths / 100m, 0, MidpointRounding.AwayFromZero));
        }
    }
}
