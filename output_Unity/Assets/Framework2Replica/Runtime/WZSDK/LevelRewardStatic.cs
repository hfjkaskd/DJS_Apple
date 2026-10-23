using System.Collections.Generic;

namespace WZSDK
{
	public static class LevelRewardStatic
	{
		public class RewardItem
		{
			public int type;

			public float amount;
		}

		public static List<RewardItem> GetRewards(int level)
		{
			UserDataManager dataManager = UserDataManager.Instance;
			List<RewardItem> configured = dataManager != null
				? dataManager.GetLevelRewards(level)
				: null;
			return configured != null && configured.Count > 0
				? FilterRewardsForLevel(level, configured)
				: BuildFallbackRewards(level);
		}

		private static List<RewardItem> FilterRewardsForLevel(int level, List<RewardItem> sourceRewards)
		{
			List<RewardItem> result = new List<RewardItem>();
			GameManagerWZ manager = GameManagerWZ.instance;
			bool canGrantMoney = manager == null ||
				manager.CanGrantCoinRewardForLevel(level);
			if (sourceRewards == null)
			{
				return result;
			}

			foreach (RewardItem reward in sourceRewards)
			{
				if (reward == null ||
					(reward.type == (int)ELuckySpinRewardType.Money &&
						!canGrantMoney))
				{
					continue;
				}
				result.Add(new RewardItem
				{
					type = reward.type,
					amount = reward.amount
				});
			}
			return result;
		}

		private static List<RewardItem> BuildFallbackRewards(int level)
		{
			List<RewardItem> result = new List<RewardItem>();
			GameManagerWZ manager = GameManagerWZ.instance;
			if (manager == null)
			{
				return result;
			}

			float amount = manager.GetCoinRewardAmount(1, level);
			if (amount > 0f)
			{
				result.Add(new RewardItem
				{
					type = (int)ELuckySpinRewardType.Money,
					amount = amount
				});
			}
			return result;
		}
	}
}
