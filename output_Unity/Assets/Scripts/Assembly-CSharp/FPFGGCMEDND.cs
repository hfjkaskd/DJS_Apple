using System.Collections.Generic;
using LevelBox;

/// <summary>五胜宝箱模型：进度每5胜满一次，按循环表发奖。</summary>
public class FPFGGCMEDND : global::IAJDGNOGFGO<LevelBoxData, FPFGGCMEDND>
{
	public const int AD_COIN_COUNT = 100;

	public const int LEVELS_PER_BOX = 5;

	private MNCNHALABEP levelBoxRewardConfig;

	private List<IGOBDCAPLDG> lastBoxRewards;

	public bool CurBoxHasOpened
	{
		get
		{
			return data.curBoxHasOpened;
		}
		set
		{
			data.curBoxHasOpened = value;
			SaveData();
		}
	}

	public int CurLevelBoxProgress => data.curLevelBoxProgress;

	/// <summary>已领取的宝箱数（进度条显示累计阈值用：5/5 → 5/10）。</summary>
	public int NextLevelBoxIndex => data.nextLevelBoxIndex;

	protected override string GetKey()
	{
		return "LevelBoxModel";
	}

	public override void Init()
	{
		base.Init();
		levelBoxRewardConfig = MNCNHALABEP.CreateDefault();
	}

	protected override void AfterFirstInitData()
	{
		data.nextLevelBoxIndex = 0;
		data.curLevelBoxProgress = 0;
		data.curBoxHasOpened = false;
	}

	/// <summary>过关时推进宝箱进度；满 5 胜时发奖并重置。</summary>
	public void OnLevelPass()
	{
		data.curLevelBoxProgress++;
		if (data.curLevelBoxProgress >= LEVELS_PER_BOX)
		{
			GrantCurBoxReward();
			data.curLevelBoxProgress = 0;
			data.curBoxHasOpened = false;
			data.nextLevelBoxIndex++;
		}
		SaveData();
	}

	private void GrantCurBoxReward()
	{
		int index = data.nextLevelBoxIndex % levelBoxRewardConfig.rewards.Count;
		OLMGKOFLIDM reward = levelBoxRewardConfig.rewards[index];
		lastBoxRewards = new List<IGOBDCAPLDG>(reward.items);
		foreach (IGOBDCAPLDG item in reward.items)
		{
			NMNPFKEJMHO source = NMNPFKEJMHO.InLevel;
			GNEJJHEDEBL.Instance.AddItem(item.itemType, item.count, false, source);
		}
	}

	public List<IGOBDCAPLDG> GetLastBoxRewards()
	{
		return lastBoxRewards ?? new List<IGOBDCAPLDG>();
	}
}
