using System.Collections.Generic;

/// <summary>五胜宝箱循环奖励表（按 nextLevelBoxIndex % 6 取行）。</summary>
public class MNCNHALABEP
{
	public List<OLMGKOFLIDM> rewards;

	public static MNCNHALABEP CreateDefault()
	{
		return new MNCNHALABEP
		{
			rewards = new List<OLMGKOFLIDM>
			{
				new OLMGKOFLIDM().Add(POJCEPBNNIP.Coin, 50).Add(POJCEPBNNIP.Undo, 1),
				new OLMGKOFLIDM().Add(POJCEPBNNIP.Undo, 1).Add(POJCEPBNNIP.Shuffle, 1),
				new OLMGKOFLIDM().Add(POJCEPBNNIP.Coin, 50).Add(POJCEPBNNIP.Magic, 1),
				new OLMGKOFLIDM().Add(POJCEPBNNIP.Undo, 1).Add(POJCEPBNNIP.Magic, 1),
				new OLMGKOFLIDM().Add(POJCEPBNNIP.Coin, 50).Add(POJCEPBNNIP.Shuffle, 1),
				new OLMGKOFLIDM().Add(POJCEPBNNIP.Magic, 1).Add(POJCEPBNNIP.Shuffle, 1)
			}
		};
	}
}
