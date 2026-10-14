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
			m_BuyBtn.SetActive(false);
		}
	}

}
