using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace FruitWithdrawal
{
    /// <summary>Formatting and previews only; never changes reward or application state.</summary>
    public static class WithdrawalPresentation
    {
        public static string Money(long cents) { return "$" + (cents / 100m).ToString("N2", CultureInfo.InvariantCulture); }
        public static string TierAmount(long cents)
        {
            return cents % 100 == 0 ? "$" + (cents / 100).ToString("N0", CultureInfo.InvariantCulture) : Money(cents);
        }
        public static string Number(long value) { return value.ToString("N0", CultureInfo.InvariantCulture); }
        public static string Units(WithdrawalCurrency currency, long value)
        {
            return currency == WithdrawalCurrency.Cash ? Money(value) : Number(value) + " diamonds";
        }
        public static long Balance(WithdrawalState state, WithdrawalCurrency currency)
        {
            return currency == WithdrawalCurrency.Cash ? state.CashCents : state.DiamondUnits;
        }
        public static float Ratio(long current, long target)
        {
            return target <= 0 ? 1f : (float)Math.Min(1d, Math.Max(0d, (double)current / target));
        }
        public static string Percentage(long current, long target)
        {
            int percent = target <= 0 ? 100 : (int)Math.Min(100d, Math.Max(0d, Math.Floor((double)current * 100d / target)));
            return percent.ToString(CultureInfo.InvariantCulture) + "%";
        }
        public static string Countdown(long seconds)
        {
            seconds = Math.Max(0, seconds);
            string tail = (seconds / 60 % 60).ToString("00", CultureInfo.InvariantCulture) + ":" + (seconds % 60).ToString("00", CultureInfo.InvariantCulture);
            if (seconds < 3600) return tail;
            string hours = (seconds / 3600 % 24).ToString("00", CultureInfo.InvariantCulture) + ":" + tail;
            return seconds < 86400 ? hours : (seconds / 86400).ToString(CultureInfo.InvariantCulture) + "d " + hours;
        }
        public static string ChannelLabel(string label)
        {
            if (string.IsNullOrEmpty(label)) return "Test channel";
            int note = label.IndexOf(" (", StringComparison.Ordinal);
            return note > 0 ? label.Substring(0, note) : label;
        }
        public static void RewardTotals(WithdrawalState state, out long cash, out long diamonds)
        {
            cash = 0; diamonds = 0;
            for (int i = 0; i < state.PendingRewards.Count; i++) {
                cash = checked(cash + state.PendingRewards[i].CashCents);
                diamonds = checked(diamonds + state.PendingRewards[i].DiamondUnits);
            }
        }
        public static string[] RuleRows(WithdrawalTierRule tier, WithdrawalRules rules)
        {
            var rows = new List<string>(8);
            var levels = new StringBuilder();
            AppendLevel(levels, tier.RequiredPassLevels);
            if (tier.RequiredHiddenPassLevels != tier.RequiredPassLevels) AppendLevel(levels, tier.RequiredHiddenPassLevels);
            if (tier.RequiredStage2PassLevels != tier.RequiredPassLevels && tier.RequiredStage2PassLevels != tier.RequiredHiddenPassLevels)
                AppendLevel(levels, tier.RequiredStage2PassLevels);
            if (levels.Length > 0) rows.Add("Clear fruit level milestones: " + levels + ".");
            if (tier.RequiredDailyWins > 0) rows.Add("Win " + Number(tier.RequiredDailyWins) + " levels in one UTC day.");
            if (tier.RequiredCheckInDays > 0) rows.Add("Accumulate " + Number(tier.RequiredCheckInDays) + " qualified days.");
            if (tier.RequiredUserLevel > 0) rows.Add("Reach fruit rank " + Number(tier.RequiredUserLevel) + " from recorded wins.");
            rows.Add("Test review: " + Number(tier.ReviewSeconds) + " seconds after all conditions.");
            rows.Add("Reserve " + Units(tier.Currency, tier.RequiredBalanceUnits) + " when applying.");
            if (tier.RequiredCheckInDays > 0) rows.Add("Each qualified UTC day needs " + Number(rules.DailyWinsForCheckIn) + " wins.");
            rows.Add("Days may be separate; cancelling returns reserved units.");
            return rows.ToArray();
        }
        public static string Milestones(WithdrawalTierRule tier)
        {
            var text = new StringBuilder();
            AppendLevel(text, tier.RequiredPassLevels);
            if (tier.RequiredHiddenPassLevels != tier.RequiredPassLevels) AppendLevel(text, tier.RequiredHiddenPassLevels);
            if (tier.RequiredStage2PassLevels != tier.RequiredPassLevels && tier.RequiredStage2PassLevels != tier.RequiredHiddenPassLevels)
                AppendLevel(text, tier.RequiredStage2PassLevels);
            return text.ToString();
        }
        public static string[] RuleSummaryRows(WithdrawalTierRule tier)
        {
            string levels = Milestones(tier);
            return new[] {
                "Balance: " + Units(tier.Currency, tier.RequiredBalanceUnits),
                levels.Length == 0 ? "" : "Level milestones " + levels,
                tier.RequiredDailyWins == 0 ? "" : "Daily wins: " + Number(tier.RequiredDailyWins),
                tier.RequiredCheckInDays == 0 ? "" : "Qualified days: " + Number(tier.RequiredCheckInDays),
                tier.RequiredUserLevel == 0 ? "" : "Reward rank: " + Number(tier.RequiredUserLevel),
                "Simulated review: " + Number(tier.ReviewSeconds) + " seconds"
            };
        }
        public static string RuleFootnote(WithdrawalTierRule tier, WithdrawalRules rules)
        {
            return (tier.RequiredCheckInDays > 0 ? "Each qualified UTC day needs " + Number(rules.DailyWinsForCheckIn) + " wins." : "No qualified-day requirement.")
                + "\nDays do not need to be consecutive.\nCancel to return reserved units.";
        }
        public static string WalletSummary(WithdrawalTierRule tier)
        {
            if (tier.Currency == WithdrawalCurrency.Cash) return "Review all conditions before applying";
            var parts = new List<string>(3);
            if (tier.RequiredCheckInDays > 0) parts.Add(Number(tier.RequiredCheckInDays) + " qualified days");
            if (tier.RequiredUserLevel > 0) parts.Add("Rank " + Number(tier.RequiredUserLevel));
            if (tier.RequiredDailyWins > 0) parts.Add("Daily " + Number(tier.RequiredDailyWins) + " wins");
            return parts.Count == 0 ? "Review all conditions before applying" : string.Join(" - ", parts);
        }
        public static string GroupedProgress(WithdrawalConditionProgress condition)
        {
            string values = Number(condition.Current) + " / " + Number(condition.Target);
            switch (condition.Kind) {
                case WithdrawalConditionKind.LoginPassLevel: return "Today: " + values + " wins";
                case WithdrawalConditionKind.LoginDay: return "Qualified days: " + values;
                case WithdrawalConditionKind.UserLevel: return "Reward rank: " + values;
                default: return condition.Label + ": " + values;
            }
        }
        public static string RecordStatus(WithdrawalRequestStatus status)
        {
            switch (status) {
                case WithdrawalRequestStatus.ConditionsPending: return "Conditions in progress";
                case WithdrawalRequestStatus.ReviewPending: return "Simulated review";
                case WithdrawalRequestStatus.SimulatedCompleted: return "Test complete - No payment";
                case WithdrawalRequestStatus.Rejected: return "Rejected - Units returned";
                default: return "Cancelled - Units returned";
            }
        }
        public static string RecordDate(long utcSeconds)
        {
            return DateTimeOffset.FromUnixTimeSeconds(utcSeconds).UtcDateTime.ToString("MMM dd", CultureInfo.InvariantCulture);
        }
        public static string ConditionText(WithdrawalConditionProgress condition, WithdrawalCurrency currency)
        {
            long shown = condition.Current;
            if (condition.Kind == WithdrawalConditionKind.GoldTarget)
                return "Reserved: " + Units(currency, Math.Min(shown, condition.Target)) + " / " + Units(currency, condition.Target);
            string label;
            switch (condition.Kind) {
                case WithdrawalConditionKind.PassLevel: label = "Clear fruit level"; break;
                case WithdrawalConditionKind.PassHideLevel: label = "First level milestone"; break;
                case WithdrawalConditionKind.PassStage2Level: label = "Final level milestone"; break;
                case WithdrawalConditionKind.LoginPassLevel: label = "Wins today (UTC)"; break;
                case WithdrawalConditionKind.LoginDay: label = "Qualified days"; break;
                case WithdrawalConditionKind.UserLevel: label = "Fruit rank"; break;
                default: label = condition.Label; break;
            }
            return label + ": " + Number(shown) + " / " + Number(condition.Target);
        }
        private static void AppendLevel(StringBuilder text, int level)
        {
            if (level <= 0) return;
            if (text.Length > 0) text.Append(" / ");
            text.Append(Number(level));
        }
    }
}
