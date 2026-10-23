using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace MahjongSlide.Withdrawal
{
    [Serializable]
    public sealed class MahjongFramework2EconomyData
    {
        [Header("原版奖励与提现常量")]
        public MahjongFramework2RewardEconomy rewards = new MahjongFramework2RewardEconomy();
        public float initialCoin;
        public float initialGems;
        public int initialUndo = 5;
        public int initialClear = 1;
        public int initialAddBottle = 1;
        public float normalCoinHardCap = 2999.5f;
        public int noCoinRewardEndLevel = 2;
        public float stage3AdRewardAmount = 0.1f;
        public float stage4SlotRewardAmount = 50f;
        public float stage5CompensationTargetCoin = 2700f;
        [Header("原版 LocalSettings 生效值（不再被 JSON 覆盖）")]
        public float wheelReward = 0.05f;
        public int luckyWalletUnlockLevel = 6;
        public int[] luckyRewardLevels = Array.Empty<int>();
        public LevelTimer[] adShow = Array.Empty<LevelTimer>();
        public LevelMapping[] levelMap = Array.Empty<LevelMapping>();
        [Header("原版投放基础值")]
        public float stageCompletionCoin = 0.1f;
        public float eliminationGemCoefficient = 1f;
        public int experiencePerElimination = 2;
        public float missingManagerEliminationCoin = 5f;
        public float missingManagerIaaEliminationCoin = 1f;
        [Header("麻将侧教学（原版不额外赠送钞票）")]
        [Min(0)] public long gameplayGuideRewardMinorUnits;
        [Header("原版现金 / 钻石 / 提现任务 / 转盘 / 等级 / 地区数据")]
        public GearRow[] gear = Array.Empty<GearRow>();
        public GemRow[] gem = Array.Empty<GemRow>();
        public StageRow[] stages = Array.Empty<StageRow>();
        public SpinRow[] luckySpin = Array.Empty<SpinRow>();
        public LuckyRow[] luckyRewardFallback = Array.Empty<LuckyRow>();
        public LevelRow[] userLevels = Array.Empty<LevelRow>();
        public RegionRow[] payRegions = Array.Empty<RegionRow>();
        public BarrageRow[] withdrawalBarrage = Array.Empty<BarrageRow>();

        [Serializable] public sealed class LevelTimer { public int level; public float time; }
        [Serializable] public sealed class LevelMapping { public int level; public int mappedLevel; }
        private interface ISourceRow { string[] ToCells(); }

        public void ValidateOrThrow()
        {
            if (rewards == null || rewards.settlementNonAdDivisor <= 0f || rewards.inGameNonAdDivisor <= 0f ||
                rewards.luckySpinAdTarget < 1 || rewards.stage3AdTargetFallback < 1 ||
                rewards.gemExchangeCosts == null || rewards.gemExchangeCashAmounts == null ||
                rewards.gemExchangeCosts.Length == 0 || rewards.gemExchangeCosts.Length != rewards.gemExchangeCashAmounts.Length)
                throw new InvalidOperationException("Framework 2 economy has invalid reward divisors, targets or exchange tiers.");
            foreach (int cost in rewards.gemExchangeCosts)
                if (cost <= 0) throw new InvalidOperationException("Framework 2 gem exchange costs must be positive.");
            if (normalCoinHardCap < 0f || noCoinRewardEndLevel < 0 || luckyWalletUnlockLevel < 1 ||
                adShow == null || levelMap == null || luckyRewardLevels == null || gameplayGuideRewardMinorUnits < 0)
                throw new InvalidOperationException("Framework 2 economy source settings are invalid.");
            RequireRows(gear); RequireRows(gem); RequireRows(stages); RequireRows(luckySpin);
            RequireRows(luckyRewardFallback); RequireRows(userLevels); RequireRows(payRegions); RequireRows(withdrawalBarrage);
        }

        private static void RequireRows<T>(T[] entries) where T : class
        {
            if (entries == null || entries.Length == 0)
                throw new InvalidOperationException("Framework 2 economy SO requires its source tables.");
            foreach (T entry in entries)
                if (entry == null) throw new InvalidOperationException("Framework 2 economy SO contains an empty row.");
        }
        [Serializable] public sealed class GearRow : ISourceRow
        {
            public int id;
            public int CurrencyMax;
            public int CurrencyMin;
            public float LuckMax;
            public float LuckMin;
            public float EliminateMax;
            public float EliminateMin;
            public float TurntableMax;
            public string[] ToCells() => Cells(id, CurrencyMax, CurrencyMin, LuckMax, LuckMin, EliminateMax, EliminateMin, TurntableMax);
        }
        [Serializable] public sealed class GemRow : ISourceRow
        {
            public int id;
            public int CurrencyMax;
            public int CurrencyMin;
            public float LuckMax;
            public string LuckMin = string.Empty;
            public float TurntableMin;
            public string[] ToCells() => Cells(id, CurrencyMax, CurrencyMin, LuckMax, LuckMin, TurntableMin);
        }
        [Serializable] public sealed class StageRow : ISourceRow
        {
            public int Id;
            public int Stage;
            public int Type;
            public int Value;
            public int AutoNextStage;
            public string[] ToCells() => Cells(Id, Stage, Type, Value, AutoNextStage);
        }
        [Serializable] public sealed class SpinRow : ISourceRow
        {
            public int sn;
            public int type;
            public float count;
            public double probability;
            public string[] ToCells() => Cells(sn, type, count, probability);
        }
        [Serializable] public sealed class LuckyRow : ISourceRow
        {
            public int Id;
            public int Level;
            public int ShowIcon;
            public string[] ToCells() => Cells(Id, Level, ShowIcon);
        }
        [Serializable] public sealed class LevelRow : ISourceRow
        {
            public int sn;
            public int level;
            public int nextLvNeedExp;
            public string[] ToCells() => Cells(sn, level, nextLvNeedExp);
        }
        [Serializable] public sealed class RegionRow : ISourceRow
        {
            public int sn;
            public string code = string.Empty;
            public string mark = string.Empty;
            public string lang = string.Empty;
            public string[] ToCells() => Cells(sn, code, mark, lang);
        }
        [Serializable] public sealed class BarrageRow : ISourceRow
        {
            public string play = string.Empty;
            public string Stage = string.Empty;
            public string[] ToCells() => Cells(play, Stage);
        }

        // Adapt typed SO rows once at startup to the original table consumers.
        // No CSV/JSON asset can override these economy values at runtime.
        public bool TryGetTableRows(string address, out List<string[]> rows)
        {
            switch (address.ToLowerInvariant())
            {
                case "csv/gear": rows = Rows(gear, "id", "CurrencyMax", "CurrencyMin", "LuckMax", "LuckMin", "EliminateMax", "EliminateMin", "TurntableMax"); return true;
                case "csv/gem": rows = Rows(gem, "id", "CurrencyMax", "CurrencyMin", "LuckMax", "LuckMin", "TurntableMin"); return true;
                case "csv/stage": rows = Rows(stages, "Id", "Stage", "Type", "Value", "AutoNextStage"); return true;
                case "csv/luckyspin": rows = Rows(luckySpin, "sn", "type", "count", "probability"); return true;
                case "csv/luckyreward": rows = Rows(luckyRewardFallback, "Id", "Level", "ShowIcon"); return true;
                case "csv/userlevel": rows = Rows(userLevels, "sn", "level", "nextLvNeedExp"); return true;
                case "csv/payregion": rows = Rows(payRegions, "sn", "code", "mark", "lang"); return true;
                case "csv/extractdata": rows = Rows(withdrawalBarrage, "play", "Stage"); return true;
                default: rows = null; return false;
            }
        }

        private static List<string[]> Rows<T>(T[] entries, params string[] header) where T : ISourceRow
        {
            if (entries == null || entries.Length == 0)
                throw new InvalidOperationException("Framework 2 economy SO has an empty required table.");
            var rows = new List<string[]>(entries.Length + 1) { header };
            foreach (T entry in entries) rows.Add(entry.ToCells());
            return rows;
        }

        private static string[] Cells(params object[] values)
        {
            var cells = new string[values.Length];
            for (int i = 0; i < values.Length; i++)
                cells[i] = Convert.ToString(values[i], CultureInfo.InvariantCulture) ?? string.Empty;
            return cells;
        }
    }
}
