using System;
using UnityEngine;

namespace MahjongSlide.Withdrawal
{
    /// <summary>Framework 2 reward cadence; view resources and animations belong to its runtime.</summary>
    [Serializable]
    public sealed class MahjongFramework2SourceDefaults
    {
        [Min(1)] public int LuckySpin_Elimination_Interval = 40;
        [Min(0f)] public float TimerReward_Interval = 30f;
        public bool EnableLuckyRewardOnPourComplete = true;
        [Min(0)] public int CloseInt = 3;
    }
}
