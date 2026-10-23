using System;

namespace WZSDK
{
	public class LuckySpinOpenData
	{
		public bool ForceMoneyReward { get; set; }

		public float ForcedMoneyAmount { get; set; }

		public LuckySpinMoneyRewardTarget MoneyRewardTarget { get; set; }

		public bool IsMandatory { get; set; }

		public bool AutoStartSpin { get; set; }

		public int FixedBonusWheelAngleIndex { get; set; }

		public Action OnRewardComplete { get; set; }

		public LuckySpinOpenData()
		{
			MoneyRewardTarget = LuckySpinMoneyRewardTarget.CurrentCoin;
			FixedBonusWheelAngleIndex = -1;
		}
	}
}
