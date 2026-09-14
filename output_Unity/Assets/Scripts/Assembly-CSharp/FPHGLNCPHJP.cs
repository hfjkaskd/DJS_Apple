using System.Collections.Generic;

/// <summary>礼包商品配置。</summary>
public class FPHGLNCPHJP
{
	public string productID;

	public int coinCount;

	public int undoCount;

	public int magicCount;

	public int shuffleCount;

	public FPHGLNCPHJP(string InProductID, int InCoinCount, int InUndoCount, int InMagicCount, int InShuffleCount)
	{
		productID = InProductID;
		coinCount = InCoinCount;
		undoCount = InUndoCount;
		magicCount = InMagicCount;
		shuffleCount = InShuffleCount;
	}

	public static List<FPHGLNCPHJP> GetGiftPackRewards()
	{
		// 前两档与录屏一致（1000/×3、3000/×6），后两档按倍率外推
		return new List<FPHGLNCPHJP>
		{
			new FPHGLNCPHJP("giftpack_1", 1000, 3, 3, 3),
			new FPHGLNCPHJP("giftpack_2", 3000, 6, 6, 6),
			new FPHGLNCPHJP("giftpack_3", 8000, 12, 12, 12),
			new FPHGLNCPHJP("giftpack_4", 20000, 24, 24, 24)
		};
	}
}
