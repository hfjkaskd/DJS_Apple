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

	private COAEOCCIEAO coinPackReward;

	public void InitCoinPack(COAEOCCIEAO InCoinPackReward)
	{
		coinPackReward = InCoinPackReward;
		if (m_CoinCount != null)
		{
			m_CoinCount.text = "×" + InCoinPackReward.coinCount;
		}
		if (m_BuyBtn != null)
		{
			MCCIJBJGMCK.Get(m_BuyBtn).onClick = OnBuyBtnClick;
		}
	}

	private void OnBuyBtnClick(GameObject gameObject)
	{
		if (coinPackReward == null)
		{
			return;
		}
		// 无真实 IAP：直接发放金币
		GNEJJHEDEBL.Instance.AddItem(POJCEPBNNIP.Coin, coinPackReward.coinCount, false, NMNPFKEJMHO.Iap);
		GameAudio.Play(DLMJOHCOJKN.Play_sfx_notice_purchased);
		CommonCoinBtn.FlyCoins(10, transform.position);
		MgrGlobalUI.Instance.ShowGlobalText(OJEEJGGLNPC.Instance.GetText("shop_purchase_completed_text"), FFMLGGBCOOO.positive);
	}
}
