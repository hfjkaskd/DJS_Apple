using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>礼包条目：展示内容数量，本工程不接入真实 IAP。</summary>
public class GiftPackItem : ShopItem
{
	[SerializeField]
	private TextMeshProUGUI m_CoinCount;

	[SerializeField]
	private TextMeshProUGUI m_UndoCount;

	[SerializeField]
	private TextMeshProUGUI m_MagicCount;

	[SerializeField]
	private TextMeshProUGUI m_ShuffleCount;

	[SerializeField]
	private List<TextMeshProUGUI> m_Prices;

	[SerializeField]
	private GameObject m_BuyBtn;

	public void InitGiftPack(FPHGLNCPHJP InGiftPackReward)
	{
		if (m_CoinCount != null)
		{
			m_CoinCount.text = InGiftPackReward.coinCount.ToString();
		}
		if (m_UndoCount != null)
		{
			m_UndoCount.text = "×" + InGiftPackReward.undoCount;
		}
		if (m_MagicCount != null)
		{
			m_MagicCount.text = "×" + InGiftPackReward.magicCount;
		}
		if (m_ShuffleCount != null)
		{
			m_ShuffleCount.text = "×" + InGiftPackReward.shuffleCount;
		}
		if (m_BuyBtn != null)
		{
			m_BuyBtn.SetActive(false);
		}
	}

}
