using System.Collections;
using System.Collections.Generic;
using Framework.Base.UtilModule;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>商店覆盖层：礼包、去广告、免费金币与金币包，关闭时不影响局内状态。</summary>
public class ShopUI : BaseUI
{
	[SerializeField]
	private GameObject m_CloseBtn;

	[SerializeField]
	private CommonCoinBtn m_CommonCoinBtn;

	[SerializeField]
	private Transform m_CoinTrans;

	[SerializeField]
	private Transform m_UndoTrans;

	[SerializeField]
	private Transform m_MagicTrans;

	[SerializeField]
	private Transform m_ShuffleTrans;

	[SerializeField]
	private GameObject m_CoinEffect;

	[SerializeField]
	private GameObject m_UndoEffect;

	[SerializeField]
	private GameObject m_MagicEffect;

	[SerializeField]
	private GameObject m_ShuffleEffect;

	[SerializeField]
	private TextMeshProUGUI m_CurUndoCount;

	[SerializeField]
	private TextMeshProUGUI m_CurMagicCount;

	[SerializeField]
	private TextMeshProUGUI m_CurShuffleCount;

	[SerializeField]
	private ScrollRect m_ScrollRect;

	[SerializeField]
	private RectTransform m_ScrollContent;

	[SerializeField]
	private List<GiftPackItem> m_GiftPacks;

	[SerializeField]
	private RemoveADItem m_NoADProduct;

	[SerializeField]
	private FreeCoinItem m_FreeCoinItem;

	[SerializeField]
	private List<CoinPackItem> m_CoinPacks;

	[SerializeField]
	private float m_AniGap;

	[SerializeField]
	private GameObject m_MoreOfferBtn;

	private bool removeADState;

	private List<ShopItem> shopItems = new List<ShopItem>();

	private bool moreOfferState;

	private const int TOP_HEIGHT = 296;

	private const int BOTTOM_HEIGHT = 225;

	private const int GIFTPACK_HEIGHT = 485;

	private const int OTHERITEM_HEIGHT = 244;

	private const int CAN_COVER_HEIGHT = 60;

	public static FCCMMLGODPL CurPurchaseActScene;

	public static ShopUI Instance { get; private set; }

	public Vector3 UndoIconPos => m_UndoTrans != null ? m_UndoTrans.position : Vector3.zero;

	public Vector3 MagicIconPos => m_MagicTrans != null ? m_MagicTrans.position : Vector3.zero;

	public Vector3 ShuffleIconPos => m_ShuffleTrans != null ? m_ShuffleTrans.position : Vector3.zero;

	public Vector3 CoinBtnPos => m_CoinTrans != null ? m_CoinTrans.position : Vector3.zero;

	public override PAIEAGDLCBJ Layer => PAIEAGDLCBJ.Top;

	public override int OwnLayerCnt => 5;


	protected override void Init()
	{
		Instance = this;
		if (m_CloseBtn != null)
		{
			MCCIJBJGMCK.Get(m_CloseBtn).onClick = Close;
		}
		if (m_MoreOfferBtn != null)
		{
			MCCIJBJGMCK.Get(m_MoreOfferBtn).onClick = OnMoreOfferBtnClick;
		}
		if (m_CommonCoinBtn != null)
		{
			m_CommonCoinBtn.Init(FCCMMLGODPL.ShopinPlay, false);
		}
		List<FPHGLNCPHJP> giftRewards = FPHGLNCPHJP.GetGiftPackRewards();
		if (m_GiftPacks != null)
		{
			for (int i = 0; i < m_GiftPacks.Count; i++)
			{
				if (m_GiftPacks[i] != null && i < giftRewards.Count)
				{
					m_GiftPacks[i].InitGiftPack(giftRewards[i]);
					shopItems.Add(m_GiftPacks[i]);
				}
			}
		}
		if (m_NoADProduct != null)
		{
			m_NoADProduct.InitRemoveAD();
			shopItems.Add(m_NoADProduct);
		}
		if (m_FreeCoinItem != null)
		{
			m_FreeCoinItem.InitFreeCoin();
			shopItems.Add(m_FreeCoinItem);
		}
		List<COAEOCCIEAO> coinRewards = COAEOCCIEAO.GetCoinPackRewards();
		if (m_CoinPacks != null)
		{
			for (int i = 0; i < m_CoinPacks.Count; i++)
			{
				if (m_CoinPacks[i] != null && i < coinRewards.Count)
				{
					m_CoinPacks[i].InitCoinPack(coinRewards[i]);
					shopItems.Add(m_CoinPacks[i]);
				}
			}
		}
	}

	protected override void BeforeOpen()
	{
		GameAudio.Play(DLMJOHCOJKN.Play_sfx_ui_panel_shop_open);
		moreOfferState = false;
		RefreshItemCount();
		if (m_ScrollRect != null)
		{
			m_ScrollRect.verticalNormalizedPosition = 1f;
		}
		if (m_CommonCoinBtn != null)
		{
			m_CommonCoinBtn.AddCurCommonCoinBtn();
		}
		CoroutineManager.Instance.StartCor(PlayItemInitialAni());
	}

	protected override void AfterOpen()
	{
		CoroutineManager.Instance.StartCor(PlayItemInAni());
	}

	protected override void BeforeClose()
	{
		if (m_CommonCoinBtn != null)
		{
			m_CommonCoinBtn.RemoveCurCommonCoinBtn();
		}
	}

	private IEnumerator PlayItemInAni()
	{
		float gap = m_AniGap > 0f ? m_AniGap : 0.05f;
		foreach (ShopItem item in shopItems)
		{
			if (item != null)
			{
				item.PlayInAni();
				yield return new WaitForSeconds(gap);
			}
		}
	}

	private IEnumerator PlayItemInitialAni()
	{
		foreach (ShopItem item in shopItems)
		{
			if (item != null)
			{
				item.PlayInitialAni();
			}
		}
		yield break;
	}

	private void RefreshItemCount()
	{
		if (m_CurUndoCount != null)
		{
			m_CurUndoCount.text = GNEJJHEDEBL.Instance.GetItemCount(POJCEPBNNIP.Undo, true).ToString();
		}
		if (m_CurMagicCount != null)
		{
			m_CurMagicCount.text = GNEJJHEDEBL.Instance.GetItemCount(POJCEPBNNIP.Magic, true).ToString();
		}
		if (m_CurShuffleCount != null)
		{
			m_CurShuffleCount.text = GNEJJHEDEBL.Instance.GetItemCount(POJCEPBNNIP.Shuffle, true).ToString();
		}
	}

	private void ChangeNoAdState()
	{
	}

	private void OnMoreOfferBtnClick(GameObject InGameObject)
	{
		if (moreOfferState)
		{
			return;
		}
		moreOfferState = true;
		if (m_MoreOfferBtn != null)
		{
			m_MoreOfferBtn.SetActive(false);
		}
		foreach (ShopItem item in shopItems)
		{
			if (item != null)
			{
				item.PlayShowMoreAni();
			}
		}
	}

	private void Close(GameObject gameObject)
	{
		GameAudio.Play(DLMJOHCOJKN.Play_sfx_ui_panel_shop_close);
		MgrUI.Instance.Close("shop/prefab/ShopUI", false, false);
	}

	public void PlayItemHitAni(POJCEPBNNIP InItemType)
	{
		GameObject effect = null;
		switch (InItemType)
		{
		case POJCEPBNNIP.Coin:
			effect = m_CoinEffect;
			break;
		case POJCEPBNNIP.Undo:
			effect = m_UndoEffect;
			break;
		case POJCEPBNNIP.Magic:
			effect = m_MagicEffect;
			break;
		case POJCEPBNNIP.Shuffle:
			effect = m_ShuffleEffect;
			break;
		}
		if (effect != null)
		{
			effect.SetActive(false);
			effect.SetActive(true);
		}
		RefreshItemCount();
	}
}
