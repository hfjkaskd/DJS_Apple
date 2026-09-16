using System;
using System.Collections.Generic;

namespace FruitWithdrawal
{
    public enum WithdrawalCurrency { Cash = 0, Diamonds = 1 }
    public enum WithdrawalConditionKind { PassLevel = 1, PassHideLevel = 2, PassStage2Level = 3, GoldTarget = 4, LoginPassLevel = 5, LoginDay = 6, UserLevel = 7 }
    public enum WithdrawalRequestStatus { ConditionsPending = 0, ReviewPending = 1, SimulatedCompleted = 2, Rejected = 3, Cancelled = 4 }
    public enum WithdrawalRewardKind { Triple = 0, Win = 1 }

    [Serializable]
    public sealed class WithdrawalChannelRule
    {
        public string Id;
        public string Label;
    }

    [Serializable]
    public sealed class WithdrawalTierRule
    {
        public string Id;
        public string Label;
        public WithdrawalCurrency Currency;
        public long DisplayCents;
        public long RequiredBalanceUnits;
        public int RequiredPassLevels;
        public int RequiredHiddenPassLevels;
        public int RequiredStage2PassLevels;
        public int RequiredDailyWins;
        public int RequiredCheckInDays;
        public int RequiredUserLevel;
        public long ReviewSeconds;
        public string EvidenceNote;

        public WithdrawalTierRule Copy()
        {
            return new WithdrawalTierRule { Id = Id, Label = Label, Currency = Currency, DisplayCents = DisplayCents,
                RequiredBalanceUnits = RequiredBalanceUnits, RequiredPassLevels = RequiredPassLevels,
                RequiredHiddenPassLevels = RequiredHiddenPassLevels, RequiredStage2PassLevels = RequiredStage2PassLevels,
                RequiredDailyWins = RequiredDailyWins, RequiredCheckInDays = RequiredCheckInDays,
                RequiredUserLevel = RequiredUserLevel, ReviewSeconds = ReviewSeconds, EvidenceNote = EvidenceNote };
        }

        public void Validate()
        {
            WithdrawalRules.RequireId(Id);
            if (string.IsNullOrEmpty(Label) || (Currency != WithdrawalCurrency.Cash && Currency != WithdrawalCurrency.Diamonds) ||
                DisplayCents <= 0 || DisplayCents > WithdrawalRules.MaxUnits || RequiredBalanceUnits <= 0 ||
                RequiredBalanceUnits > WithdrawalRules.MaxUnits || RequiredPassLevels < 0 || RequiredHiddenPassLevels < 0 ||
                RequiredStage2PassLevels < 0 || RequiredDailyWins < 0 || RequiredCheckInDays < 0 || RequiredUserLevel < 0 ||
                ReviewSeconds < 0 || ReviewSeconds > 31536000) throw new InvalidOperationException("Invalid simulation tier.");
        }
    }

    [Serializable]
    public sealed class WithdrawalRules
    {
        public const long MaxUnits = 1000000000000000L;
        public string SimulationLabel = "TEST SIMULATION - NO CASH PAYMENT";
        public string AdaptationNote = "Fruit adaptation: hidden and second-stage gates use cleared fruit levels; user rank is earned from wins. Rewards, main cash target and review duration are test settings, not recovered payment guarantees.";
        public long TripleCashCents = 10;
        public long TripleDiamondUnits = 100;
        public long WinCashCents = 100;
        public long WinDiamondUnits = 1000;
        public long FirstWinBonusDiamondUnits = 30000;
        public bool RewardTutorial = true;
        public int DailyWinsForCheckIn = 10;
        public int WinsPerUserLevel = 1;
        public WithdrawalTierRule[] Tiers;
        public WithdrawalChannelRule[] Channels;

        public static WithdrawalRules CreateDefault()
        {
            var rules = new WithdrawalRules();
            rules.Channels = new[] {
                new WithdrawalChannelRule { Id = "test-wallet", Label = "Test wallet (no account required)" },
                new WithdrawalChannelRule { Id = "test-bank", Label = "Test bank (simulation only)" }
            };
            rules.Tiers = new[] {
                Cash("cash-040", "Cash sample 1", 40, 1), Cash("cash-060", "Cash sample 2", 60, 2),
                new WithdrawalTierRule { Id = "cash-main", Label = "Cash goal", Currency = WithdrawalCurrency.Cash,
                    DisplayCents = 50000, RequiredBalanceUnits = 50000, RequiredPassLevels = 2,
                    RequiredHiddenPassLevels = 22, RequiredStage2PassLevels = 51, RequiredDailyWins = 10,
                    RequiredCheckInDays = 5, RequiredUserLevel = 31, ReviewSeconds = 60,
                    EvidenceNote = "Source third cash tier is MARK, with gates 22/51, 5 qualified days and rank 31. Cash target 500.00 and 60-second simulated review are explicit fruit test adaptations." },
                Diamonds("diamond-500", 50000, 5, 31), Diamonds("diamond-1000", 100000, 7, 32),
                Diamonds("diamond-1500", 150000, 10, 33), Diamonds("diamond-2000", 200000, 15, 34)
            };
            return rules;
        }

        private static WithdrawalTierRule Cash(string id, string label, long cents, int levels)
        {
            return new WithdrawalTierRule { Id = id, Label = label, Currency = WithdrawalCurrency.Cash,
                DisplayCents = cents, RequiredBalanceUnits = cents, RequiredPassLevels = levels, ReviewSeconds = 60,
                EvidenceNote = "Source sample values 0.40/0.60 and early-level branches; 60-second review is a configurable local test adaptation." };
        }

        private static WithdrawalTierRule Diamonds(string id, long units, int days, int rank)
        {
            return new WithdrawalTierRule { Id = id, Label = "Diamond goal", Currency = WithdrawalCurrency.Diamonds,
                DisplayCents = units, RequiredBalanceUnits = units, RequiredDailyWins = 10,
                RequiredCheckInDays = days, RequiredUserLevel = rank, ReviewSeconds = 60,
                EvidenceNote = "Source Coin tiers use 50000/100000/150000/200000 units for displayed 500/1000/1500/2000, qualified days 5/7/10/15 and ranks 31/32/33/34. Review duration is a test adaptation." };
        }

        public WithdrawalRules Copy()
        {
            var result = new WithdrawalRules { SimulationLabel = SimulationLabel, AdaptationNote = AdaptationNote,
                TripleCashCents = TripleCashCents, TripleDiamondUnits = TripleDiamondUnits, WinCashCents = WinCashCents,
                WinDiamondUnits = WinDiamondUnits, FirstWinBonusDiamondUnits = FirstWinBonusDiamondUnits,
                RewardTutorial = RewardTutorial, DailyWinsForCheckIn = DailyWinsForCheckIn, WinsPerUserLevel = WinsPerUserLevel,
                Tiers = new WithdrawalTierRule[Tiers.Length], Channels = new WithdrawalChannelRule[Channels.Length] };
            for (int i = 0; i < Tiers.Length; i++) result.Tiers[i] = Tiers[i].Copy();
            for (int i = 0; i < Channels.Length; i++) result.Channels[i] = new WithdrawalChannelRule { Id = Channels[i].Id, Label = Channels[i].Label };
            return result;
        }

        public void Validate()
        {
            if (string.IsNullOrEmpty(SimulationLabel) || DailyWinsForCheckIn < 1 || WinsPerUserLevel < 1 ||
                Tiers == null || Tiers.Length < 1 || Tiers.Length > 100 || Channels == null || Channels.Length < 1 || Channels.Length > 20)
                throw new InvalidOperationException("Invalid simulation configuration.");
            CheckUnits(TripleCashCents); CheckUnits(TripleDiamondUnits); CheckUnits(WinCashCents);
            CheckUnits(WinDiamondUnits); CheckUnits(FirstWinBonusDiamondUnits);
            var ids = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < Tiers.Length; i++) {
                if (Tiers[i] == null) throw new InvalidOperationException("Missing simulation tier.");
                Tiers[i].Validate();
                if (!ids.Add(Tiers[i].Id)) throw new InvalidOperationException("Duplicate simulation tier.");
            }
            ids.Clear();
            for (int i = 0; i < Channels.Length; i++) {
                if (Channels[i] == null || string.IsNullOrEmpty(Channels[i].Label)) throw new InvalidOperationException("Missing test channel.");
                RequireId(Channels[i].Id);
                if (!ids.Add(Channels[i].Id)) throw new InvalidOperationException("Duplicate test channel.");
            }
        }

        internal static void RequireId(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length > 128) throw new InvalidOperationException("Invalid simulation identifier.");
        }
        internal static void CheckUnits(long value)
        {
            if (value < 0 || value > MaxUnits) throw new InvalidOperationException("Simulation amount is outside the supported range.");
        }
    }
}
