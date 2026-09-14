using Project.UI;
using TMPro;
using UnityEngine;

/// <summary>免费金币条目（看广告获得，本工程直接发放）。</summary>
public class FreeCoinItem : ShopItem
{
	private const int FREE_COIN_COUNT = 50;

	private const int FLY_COIN_NUM = 20;

	[SerializeField]
	private TextMeshProUGUI m_CoinCount;

	[SerializeField]
	private AdButtonState m_ADCoinBtn;

	[SerializeField]
	private Transform m_CoinIcon;

	public void InitFreeCoin()
	{
		if (m_CoinCount != null)
		{
			m_CoinCount.text = "×" + FREE_COIN_COUNT;
		}
		if (m_ADCoinBtn != null)
		{
			m_ADCoinBtn.InitClickAction(OnADCoinBtnClick);
		}
	}

	private void OnADCoinBtnClick(GameObject gameObject)
	{
		GNEJJHEDEBL.Instance.AddItem(POJCEPBNNIP.Coin, FREE_COIN_COUNT, false, NMNPFKEJMHO.ShopADCoins);
		Vector3 startPos = m_CoinIcon != null ? m_CoinIcon.position : transform.position;
		CommonCoinBtn.FlyCoins(FLY_COIN_NUM, startPos);
	}
}
