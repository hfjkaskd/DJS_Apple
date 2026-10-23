using System;
using UnityEngine;

namespace MahjongSlide.Withdrawal
{
    [Serializable]
    public sealed class Framework2ProductConfig
    {
        [Header("框架2独立地区配置（语言、币种、汇率、渠道）")]
        public WaterSort.WaterSortCurrencyData currencyDatabase = new WaterSort.WaterSortCurrencyData();
        public WaterSort.WaterSortLocalizationData localizationDatabase = new WaterSort.WaterSortLocalizationData();
        [Header("框架2独立经济数值（与框架1配置隔离）")]
        public MahjongFramework2EconomyData economy = new MahjongFramework2EconomyData();
        [Header("局内奖励触发")]
        public MahjongFramework2SourceDefaults sourceDefaults = new MahjongFramework2SourceDefaults();
        [Header("经济模式")]
        public bool iaaMode;
        [Header("框架2货币栏数字递增（独立表现参数，0秒关闭）")]
        [Min(0f)] public float hudBalanceAnimationSeconds;
        public int timedRewardStartLevel = 4;
        [Header("局内奖励触发（默认原版计时后二合，独立于框架1）")]
        [Tooltip("关闭时使用原版关卡计时 + 成功二合触发；开启时才使用下方麻将进度阈值。")]
        public bool useMahjongInGameRewardTrigger;
        [Min(0f)] public float inGameRewardTriggerDelaySeconds = 0.5f;
        [Range(0f, 1f)] public float inGameRewardProgressRatio = 0.4f;
        [Range(0f, 1f)] public float inGameRewardForcedProgressRatio = 0.7f;
        [Min(0f)] public float inGameRewardMinimumRoundSeconds = 25f;
        [Header("原版提现新手引导（正式关卡达到阶段目标后衔接）")]
        public int gameplayGuideWithdrawalStage = 1;
        [Min(0f)] public float guideRewardDelaySeconds = 2f;
        [Header("线性麻将关卡")]
        public string linearLevelTitleFormat = "Level {0}";
        [Header("Source refusal advertising rules")]
        public Vector2Int refusalsBeforeAd = new Vector2Int(3, 4);
        public Vector2Int refuseInterstitialLevelRange = new Vector2Int(4, 10);
        [Range(0, 100)] public int refuseInterstitialPercent = 50;

        // Fixed host contracts retain the existing storage keys and presentation
        // behavior. Properties are deliberately excluded from Unity serialization.
        public int schemaVersion => 2;
        public string addressPrefix => "withdrawal-framework2/";
        public string savePrefix => "MahjongWithdrawalFramework2.v1.";
        public int minorUnitsPerCoin => 100;
        public float hudBalanceFlightTimeoutSeconds => 12f;
        public int maximumProgressLevel => int.MaxValue;
        public float sourceUiTimeoutSeconds => 15f;
        public int chainQuietFrames => 3;
        public float guideHandVerticalOffset => -64f;
        public WZSDK.EUIType[] preloadViews => PreloadViews;
        public WZSDK.ViewType[] preloadWithdrawalViews => PreloadWithdrawalViews;

        static readonly WZSDK.EUIType[] PreloadViews =
        {
            WZSDK.EUIType.ChallangeSuccessView, WZSDK.EUIType.RewardView,
            WZSDK.EUIType.LuckySuccessView, WZSDK.EUIType.LuckySpinView,
            WZSDK.EUIType.BloatView, WZSDK.EUIType.BonusWheelView,
            WZSDK.EUIType.WithdrawMissionView
        };
        static readonly WZSDK.ViewType[] PreloadWithdrawalViews =
        {
            WZSDK.ViewType.WithDraw, WZSDK.ViewType.WithDrawMethod,
            WZSDK.ViewType.WithDrawConfirm, WZSDK.ViewType.WithDrawProcess1,
            WZSDK.ViewType.WithDrawContinue
        };
    }
}
