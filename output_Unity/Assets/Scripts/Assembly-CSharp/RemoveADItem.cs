using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>去广告条目（本工程无广告，仅保留展示）。</summary>
public class RemoveADItem : ShopItem
{
	[SerializeField]
	private List<TextMeshProUGUI> m_Prices;

	[SerializeField]
	private GameObject m_BuyBtn;

	public void InitRemoveAD()
	{
		if (m_BuyBtn != null)
		{
			MCCIJBJGMCK.Get(m_BuyBtn).onClick = OnBuyBtnClick;
		}
	}

	private void OnBuyBtnClick(GameObject gameObject)
	{
		GameAudio.Play(DLMJOHCOJKN.Play_sfx_notice_purchased);
		MgrGlobalUI.Instance.ShowGlobalText(OJEEJGGLNPC.Instance.GetText("shop_purchase_completed_text"), FFMLGGBCOOO.positive);
	}
}
