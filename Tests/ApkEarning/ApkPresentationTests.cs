using System;
using FruitApkEarning;

public static class ApkPresentationTests
{
    private static int checks;
    private static void Check(bool value, string message)
    { if (!value) throw new InvalidOperationException(message); checks++; Console.WriteLine("PASS " + message); }
    private static long Money(long hundredths) { return ApkEarningPresentation.DiamondCashCents(hundredths); }
    public static void Main()
    {
        Check(Money(35511) == 355, "355.11 diamonds display as 355 cents");
        Check(Money(5000000 - 35511) == 49645, "remaining diamond fraction rounds to 49645 cents without truncation");
        Check(Money(35511) + Money(5000000 - 35511) == 50000, "reported 355.11 example displays the full 500 dollar target");
        Check(Money(0) == 0, "empty balance remains zero");
        Check(Money(49) == 0 && Money(50) == 1 && Money(51) == 1, "fractional cent rounds only at the final display boundary");
        Check(Money(149) == 1 && Money(150) == 2 && Money(151) == 2, "half-cent ties round away from zero");
        long[] targets = { 5000000, 10000000, 15000000, 20000000 };
        long[] payouts = { 50000, 100000, 150000, 200000 };
        for (int i = 0; i < targets.Length; i++) {
            long target = targets[i], payout = payouts[i];
            Check(Money(target) == payout, "full diamond tier " + i + " displays its configured payout");
            Check(Money(target - 35511) == payout - 355, "tier " + i + " preserves fractional remaining cents");
            Check(Money(Math.Max(0, target - target)) == 0, "completed tier " + i + " has no remaining amount");
            long above = target + 12345;
            Check(Money(above) == payout + 123 && Money(Math.Max(0, target - above)) == 0 &&
                Money(Math.Min(above, target)) == payout,
                "overfilled tier " + i + " retains real balance while remaining and progress are clamped by the caller");
        }
        Check(Money(long.MaxValue) == 92233720368547758L, "maximum stored balance converts without integer overflow or floating loss");
        bool rejected = false;
        try { Money(-1); } catch (ArgumentOutOfRangeException) { rejected = true; }
        Check(rejected, "negative diamond value is rejected");
        Console.WriteLine("APK_PRESENTATION_PASS " + checks + " checks");
    }
}
