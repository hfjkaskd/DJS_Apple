using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>局内道具按钮：撤销/洗牌/魔法/扩容。管理锁定、直接使用、金币购买三种状态。</summary>
public class CorePlayItemBtn : MonoBehaviour
{
	[SerializeField]
	private Image m_ItemBG;

	[SerializeField]
	private Image m_ItemIcon;

	[SerializeField]
	private GameObject m_UseEff;

	[SerializeField]
	private Animation m_BtnAnim;

	[SerializeField]
	private GameObject m_OnlyUnlockIcon;

	[SerializeField]
	private GameObject m_LockIcon;

	[SerializeField]
	private GameObject m_UseCoin;

	[SerializeField]
	private GameObject m_WatchAD;

	[SerializeField]
	private GameObject m_UseDirectly;

	[SerializeField]
	private TextMeshProCustom m_ConsumeCoinNum;

	[SerializeField]
	private TextMeshProCustom m_LeftItemNum;

	private POJCEPBNNIP itemType;

	private DBMNIMJGPGN itemBtnStatus;

	private DPOCDHPGHMF itemSpendReason;

	private NMNPFKEJMHO itemAddSource;

	private string VADStr;

	private bool clickDisabled;

	public void Init(POJCEPBNNIP InType)
	{
		itemType = InType;
		switch (InType)
		{
		case POJCEPBNNIP.Undo:
			itemSpendReason = DPOCDHPGHMF.Undo;
			itemAddSource = NMNPFKEJMHO.Undo;
			break;
		case POJCEPBNNIP.Shuffle:
			itemSpendReason = DPOCDHPGHMF.Shuffle;
			itemAddSource = NMNPFKEJMHO.Shuffle;
			break;
		case POJCEPBNNIP.Magic:
			itemSpendReason = DPOCDHPGHMF.Magic;
			itemAddSource = NMNPFKEJMHO.Magic;
			break;
		case POJCEPBNNIP.Extra:
			itemSpendReason = DPOCDHPGHMF.Extra;
			itemAddSource = NMNPFKEJMHO.Extra;
			break;
		}
		InputMono input = MCCIJBJGMCK.Get(gameObject);
		input.playClickSound = true;
		input.onClick = OnClick;
		GNEJJHEDEBL.OnItemChanged += OnItemChanged;
		UpdateStatus();
	}

	private void OnItemChanged(POJCEPBNNIP InType)
	{
		if (InType == itemType || InType == POJCEPBNNIP.Coin)
		{
			UpdateStatus();
		}
	}

	/// <summary>关卡切换等场景下重刷状态（解锁等级依赖当前主关索引）。</summary>
	public void Refresh()
	{
		UpdateStatus();
	}

	/// <summary>首次解锁表现：入口粒子发光（新道具图标飞达后触发）。</summary>
	public void PlayUnlockEffect()
	{
		if (m_UseEff != null)
		{
			m_UseEff.SetActive(false);
			m_UseEff.SetActive(true);
		}
	}

	private void UpdateStatus()
	{
		if (itemType == POJCEPBNNIP.None)
		{
			return;
		}
		int unlockLevel = GNEJJHEDEBL.Instance.GetItemUnlockLevel(itemType);
		int curLevel = JEFOMCDAPGK.Instance.CurMainLevelIndex;
		int count = GNEJJHEDEBL.Instance.GetItemCount(itemType, true);
		int coinCount = GNEJJHEDEBL.Instance.GetItemCount(POJCEPBNNIP.Coin, true);
		int goldCost = GNEJJHEDEBL.Instance.GetItemGoldCost(itemType);
		if (curLevel < unlockLevel)
		{
			itemBtnStatus = DBMNIMJGPGN.Lock;
		}
		else if (itemType == POJCEPBNNIP.Extra && !EDLHEMMBABM.Instance.CheckCanAddOne())
		{
			itemBtnStatus = DBMNIMJGPGN.CanNotUseItem;
		}
		else if (count > 0)
		{
			itemBtnStatus = DBMNIMJGPGN.UseDirectly;
		}
		else if (coinCount >= goldCost)
		{
			itemBtnStatus = DBMNIMJGPGN.UseGold;
		}
		else
		{
			itemBtnStatus = DBMNIMJGPGN.WatchAds;
		}
		if (m_LockIcon != null)
		{
			m_LockIcon.SetActive(itemBtnStatus == DBMNIMJGPGN.Lock);
		}
		if (m_UseDirectly != null)
		{
			m_UseDirectly.SetActive(itemBtnStatus == DBMNIMJGPGN.UseDirectly);
		}
		if (m_UseCoin != null)
		{
			m_UseCoin.SetActive(itemBtnStatus == DBMNIMJGPGN.UseGold);
		}
		if (m_WatchAD != null)
		{
			m_WatchAD.SetActive(itemBtnStatus == DBMNIMJGPGN.WatchAds);
		}
		if (m_OnlyUnlockIcon != null)
		{
			m_OnlyUnlockIcon.SetActive(false);
		}
		if (m_LeftItemNum != null)
		{
			m_LeftItemNum.text = count.ToString();
		}
		if (m_ConsumeCoinNum != null)
		{
			m_ConsumeCoinNum.text = GNEJJHEDEBL.Instance.GetItemGoldCost(itemType).ToString();
		}
		UpdateIconBehavior();
	}

	private void OnClick(GameObject InGo)
	{
		if (clickDisabled || MCCIJBJGMCK.IsLock())
		{
			return;
		}
		switch (itemBtnStatus)
		{
		case DBMNIMJGPGN.Lock:
		{
			int unlockLevel = GNEJJHEDEBL.Instance.GetItemUnlockLevel(itemType);
			string tip = string.Format(OJEEJGGLNPC.Instance.GetText("gameplay_proplock_tips"), unlockLevel);
			MgrGlobalUI.Instance.ShowGlobalText(tip, FFMLGGBCOOO.neutral);
			break;
		}
		case DBMNIMJGPGN.CanNotUseItem:
			ShowCanNotUseTip();
			break;
		case DBMNIMJGPGN.WatchAds:
			MgrUI.Instance.Open("shop/prefab/ShopUI");
			break;
		case DBMNIMJGPGN.UseDirectly:
			UseItem();
			break;
		case DBMNIMJGPGN.UseGold:
		{
			int cost = GNEJJHEDEBL.Instance.GetItemGoldCost(itemType);
			if (GNEJJHEDEBL.Instance.GetItemCount(POJCEPBNNIP.Coin, true) >= cost)
			{
				if (!CheckCanUse())
				{
					ShowCanNotUseTip();
					return;
				}
				GNEJJHEDEBL.Instance.UseItem(POJCEPBNNIP.Coin, cost, itemSpendReason, 0f, EDLHEMMBABM.Instance.GetTotalItems().Count);
				JEFOMCDAPGK.Instance.MainLevelData.goldSpendCount += cost;
				DoUseItem();
			}
			else
			{
				MgrUI.Instance.Open("shop/prefab/ShopUI");
			}
			break;
		}
		}
	}

	private bool CheckCanUse()
	{
		switch (itemType)
		{
		case POJCEPBNNIP.Undo:
			return EDLHEMMBABM.Instance.CheckCanUndo();
		case POJCEPBNNIP.Shuffle:
			return EDLHEMMBABM.Instance.CheckCanShuffle();
		case POJCEPBNNIP.Magic:
			return EDLHEMMBABM.Instance.CheckCanMagic();
		case POJCEPBNNIP.Extra:
			return EDLHEMMBABM.Instance.CheckCanAddOne();
		default:
			return false;
		}
	}

	private void ShowCanNotUseTip()
	{
		string key = itemType == POJCEPBNNIP.Undo ? "gameplay_undogray_tips" : "gameplay_undogray_tips";
		MgrGlobalUI.Instance.ShowGlobalText(OJEEJGGLNPC.Instance.GetText(key), FFMLGGBCOOO.neutral);
	}

	private void UseItem()
	{
		if (!CheckCanUse())
		{
			ShowCanNotUseTip();
			return;
		}
		DoUseItem();
	}

	private void DoUseItem()
	{
		bool used = false;
		switch (itemType)
		{
		case POJCEPBNNIP.Undo:
			used = EDLHEMMBABM.Instance.Undo();
			break;
		case POJCEPBNNIP.Shuffle:
			used = EDLHEMMBABM.Instance.Shuffle();
			break;
		case POJCEPBNNIP.Magic:
			used = EDLHEMMBABM.Instance.Magic();
			break;
		case POJCEPBNNIP.Extra:
			used = EDLHEMMBABM.Instance.AddOne();
			if (used)
			{
				PlayAddOneAnim();
			}
			break;
		}
		if (used && m_UseEff != null)
		{
			m_UseEff.SetActive(false);
			m_UseEff.SetActive(true);
		}
		UpdateStatus();
	}

	private void PlayAddOneAnim()
	{
		if (m_BtnAnim != null && m_BtnAnim["anim_AddOneBtn_unlock"] != null)
		{
			m_BtnAnim.Play("anim_AddOneBtn_unlock");
		}
	}

	public void PlayAddOneLockAnim()
	{
		if (m_BtnAnim != null && m_BtnAnim["anim_AddOneBtn_lock"] != null)
		{
			m_BtnAnim.Play("anim_AddOneBtn_lock");
		}
	}

	public void DisableClick()
	{
		clickDisabled = true;
	}

	public void EnableClick()
	{
		clickDisabled = false;
	}

	private void OnDestroy()
	{
		GNEJJHEDEBL.OnItemChanged -= OnItemChanged;
	}

	/// <summary>
	/// 按状态换图。结构：外框 BG(214x217) + 内底 Button(174x174) + 图标。
	/// 锁定（对齐录屏）= 隐藏木质外框，内底(174)换 ItemBg_Lock 灰石、图标用 {Type}_Lock；
	/// 正常 = 外框 ItemBg_Gray（木框）、内底 ItemBg_Normal（蓝）、图标 {Type}_Normal。
	/// </summary>
	private void UpdateIconBehavior()
	{
		string prefix;
		switch (itemType)
		{
		case POJCEPBNNIP.Undo:
			prefix = "Undo";
			break;
		case POJCEPBNNIP.Shuffle:
			prefix = "Shuffle";
			break;
		case POJCEPBNNIP.Magic:
			prefix = "Magic";
			break;
		default:
			// Extra按钮使用独立样式（ExtraButton/Extra_Lock），由预制体状态节点控制
			return;
		}
		bool locked = itemBtnStatus == DBMNIMJGPGN.Lock;
		bool gray = itemBtnStatus == DBMNIMJGPGN.CanNotUseItem;
		if (m_ItemBG != null)
		{
			// 外框是 m_ItemBG(Button) 的同级节点 "BG"
			Transform frameTrans = m_ItemBG.transform.parent != null ? m_ItemBG.transform.parent.Find("BG") : null;
			Image outerFrame = frameTrans != null ? frameTrans.GetComponent<Image>() : null;
			if (outerFrame != null)
			{
				outerFrame.enabled = !locked;
				if (!locked)
				{
					Sprite frame = GameRes.LoadSprite("res/local/coreplay/sprite/item/ItemBg_Gray");
					if (frame != null)
					{
						outerFrame.sprite = frame;
					}
				}
			}
			Sprite inner = GameRes.LoadSprite("res/local/coreplay/sprite/item/" + (locked ? "ItemBg_Lock" : "ItemBg_Normal"));
			if (inner != null)
			{
				m_ItemBG.sprite = inner;
			}
			m_ItemBG.enabled = true;
		}
		if (m_ItemIcon != null)
		{
			string suffix = locked ? "_Lock" : (gray ? "_Gray" : "_Normal");
			Sprite icon = GameRes.LoadSprite("res/local/coreplay/sprite/item/" + prefix + suffix);
			if (icon != null)
			{
				m_ItemIcon.sprite = icon;
				m_ItemIcon.SetNativeSize();
			}
		}
	}
}
