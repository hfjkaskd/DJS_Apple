using System;
using UnityEngine;

namespace MahjongSlide.Withdrawal
{
    [Serializable]
    public sealed class MahjongFramework2RewardEconomy
    {
        [Header("原版结算与局内奖励")]
        [Min(1f)] public float settlementNonAdDivisor = 10f;
        [Min(1f)] public float inGameNonAdDivisor = 10f;
        [Min(0f)] public float settlementNonAdButtonDelaySeconds = 1f;
        [Min(0f)] public float inGameNonAdButtonDelaySeconds = 1f;

        [Header("原版幸运转盘与钱包")]
        [Min(1)] public int luckySpinAdTarget = 6;
        [Min(0f)] public float luckyWalletSpinFallbackAmount = 0.1f;
        [Min(0f)] public float luckyWalletSuccessFallbackAmount;
        [Min(0f)] public float localLuckyRewardAmount = 0.1f;
        [Min(0)] public int bonusWheelRewardIndex;

        [Header("原版提现门槛与验证广告")]
        [Min(0f)] public double minimumCashOutAmount = 0.1d;
        [Tooltip("原版钱包说明中的手续费金额；沿用原版展示逻辑，不新增扣费。")]
        [Min(0f)] public double cashOutHandlingFee = 0.2d;
        [Min(0f)] public double feeFreeCashOutThreshold = 2d;
        [Min(1)] public int stage3AdTargetFallback = 3;
        [Min(0f)] public float stage3AdDisplayIncrease = 0.1f;

        [Header("原版提现任务金额展示")]
        [Min(0f)] public double earlyTaskDisplayedAmount = 0.04d;
        [Min(0f)] public double taskRangeDisplayedMinimum = 200d;
        [Min(0f)] public double taskRangeDisplayedMaximum = 1000d;

        [Header("原版钻石兑换档位")]
        public int[] gemExchangeCosts = { 500, 1000, 1500, 2000 };
        public float[] gemExchangeCashAmounts = { 500f, 1000f, 1500f, 2000f };
    }
}
