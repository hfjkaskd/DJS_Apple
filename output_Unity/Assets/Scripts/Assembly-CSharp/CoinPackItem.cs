using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>金币包条目。</summary>
public class CoinPackItem : ShopItem
{
	[SerializeField]
	private TextMeshProUGUI m_CoinCount;

	[SerializeField]
	private List<TextMeshProUGUI> m_Prices;

	[SerializeField]
	private GameObject m_BuyBtn;

	public void InitCoinPack(COAEOCCIEAO InCoinPackReward)
	{
		if (m_CoinCount != null)
		{
			m_CoinCount.text = "×" + InCoinPackReward.coinCount;
		}
		if (m_BuyBtn != null)
		{
			m_BuyBtn.SetActive(false);
		}
	}

}
