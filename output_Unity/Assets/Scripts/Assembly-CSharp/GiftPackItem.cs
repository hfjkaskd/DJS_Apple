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

	private FPHGLNCPHJP giftPackReward;

	public void InitGiftPack(FPHGLNCPHJP InGiftPackReward)
	{
		giftPackReward = InGiftPackReward;
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
			MCCIJBJGMCK.Get(m_BuyBtn).onClick = OnBuyBtnClick;
		}
	}

	private void OnBuyBtnClick(GameObject gameObject)
	{
		if (giftPackReward == null)
		{
			return;
		}
		// 无真实 IAP：直接发放礼包内容
		GNEJJHEDEBL.Instance.AddItem(POJCEPBNNIP.Coin, giftPackReward.coinCount, false, NMNPFKEJMHO.Iap);
		GNEJJHEDEBL.Instance.AddItem(POJCEPBNNIP.Undo, giftPackReward.undoCount, false, NMNPFKEJMHO.Iap);
		GNEJJHEDEBL.Instance.AddItem(POJCEPBNNIP.Magic, giftPackReward.magicCount, false, NMNPFKEJMHO.Iap);
		GNEJJHEDEBL.Instance.AddItem(POJCEPBNNIP.Shuffle, giftPackReward.shuffleCount, false, NMNPFKEJMHO.Iap);
		GameAudio.Play(DLMJOHCOJKN.Play_sfx_notice_purchased);
		MgrGlobalUI.Instance.ShowGlobalText(OJEEJGGLNPC.Instance.GetText("shop_purchase_completed_text"), FFMLGGBCOOO.positive);
	}
}
