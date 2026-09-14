using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>HUD 金币按钮：显示金币数、承接金币飞行落点、可点击进入商店。</summary>
public class CommonCoinBtn : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI m_CoinCount;

	[SerializeField]
	private CommonFlyScript m_CommonFlyScript;

	[SerializeField]
	private GameObject m_GetEffectGo;

	private static IHJFHNCGPKF coinPool;

	private FCCMMLGODPL purchaseActScene;

	private bool isFlying;

	private int baseCount;

	private int totalFlyValue;

	private int totalFlyCoins;

	private int landedCoins;

	private static List<CommonCoinBtn> allCommonCoinBtn = new List<CommonCoinBtn>();

	private static CommonCoinBtn curCommonCoinBtn => allCommonCoinBtn.Count > 0 ? allCommonCoinBtn[allCommonCoinBtn.Count - 1] : null;

	public void Init(FCCMMLGODPL InPurchaseActScene, bool InCanClick)
	{
		purchaseActScene = InPurchaseActScene;
		if (InCanClick)
		{
			MCCIJBJGMCK.Get(gameObject).onClick = OnCommonCoinBtnClick;
		}
		GNEJJHEDEBL.OnItemChanged += OnItemChanged;
		RefreshCoinCount();
	}

	private void OnDestroy()
	{
		GNEJJHEDEBL.OnItemChanged -= OnItemChanged;
		allCommonCoinBtn.Remove(this);
	}

	private void OnItemChanged(POJCEPBNNIP InType)
	{
		if (InType == POJCEPBNNIP.Coin && !isFlying)
		{
			RefreshCoinCount();
		}
	}

	private void OnCommonCoinBtnClick(GameObject InGameObject)
	{
		if (MCCIJBJGMCK.IsLock())
		{
			return;
		}
		MgrUI.Instance.Open("shop/prefab/ShopUI");
	}

	private void RefreshCoinCount()
	{
		if (m_CoinCount != null)
		{
			m_CoinCount.text = GNEJJHEDEBL.Instance.GetItemCount(POJCEPBNNIP.Coin, true).ToString();
		}
	}

	public void AddCurCommonCoinBtn()
	{
		allCommonCoinBtn.Remove(this);
		allCommonCoinBtn.Add(this);
		RefreshCoinCount();
	}

	public void RemoveCurCommonCoinBtn()
	{
		allCommonCoinBtn.Remove(this);
	}

	/// <summary>从 InStartPos 飞 InNum 枚金币到当前 HUD 金币按钮。</summary>
	public static void FlyCoins(int InNum, Vector3 InStartPos, Vector3? InEndpos = null, Action InFirstEndAction = null, Action InEndAction = null)
	{
		CommonCoinBtn temp = curCommonCoinBtn;
		if (temp == null)
		{
			InFirstEndAction?.Invoke();
			InEndAction?.Invoke();
			return;
		}
		temp.DoFlyCoins(InNum, InStartPos, InEndpos, InFirstEndAction, InEndAction);
	}

	private void FlyCoinFirstEndAction()
	{
		GameAudio.Play(DLMJOHCOJKN.Play_sfx_noitce_coin_land);
		if (m_GetEffectGo != null)
		{
			m_GetEffectGo.SetActive(false);
			m_GetEffectGo.SetActive(true);
		}
	}

	private void DoFlyCoins(int InNum, Vector3 InStartPos, Vector3? InEndpos, Action InFirstEndAction, Action InEndAction)
	{
		if (m_CommonFlyScript == null)
		{
			InFirstEndAction?.Invoke();
			InEndAction?.Invoke();
			RefreshCoinCount();
			return;
		}
		if (coinPool == null)
		{
			GameObject coinPrefab = GameRes.LoadPrefab("res/local/coreplay/effct/prefab/FlyCoin_Glod");
			if (coinPrefab == null)
			{
				InFirstEndAction?.Invoke();
				InEndAction?.Invoke();
				RefreshCoinCount();
				return;
			}
			coinPool = new IHJFHNCGPKF(coinPrefab, 32);
		}
		m_CommonFlyScript.Init(coinPool);
		isFlying = true;
		int targetCount = GNEJJHEDEBL.Instance.GetItemCount(POJCEPBNNIP.Coin, true);
		baseCount = int.TryParse(m_CoinCount != null ? m_CoinCount.text : "0", out int parsed) ? parsed : targetCount;
		totalFlyValue = targetCount - baseCount;
		totalFlyCoins = InNum;
		landedCoins = 0;
		GameAudio.Play(DLMJOHCOJKN.Play_sfx_anim_coin_fly);
		Vector3 endPos = InEndpos ?? transform.position;
		m_CommonFlyScript.DoFlyCoins(InNum, InStartPos, endPos, delegate
		{
			FlyCoinFirstEndAction();
			InFirstEndAction?.Invoke();
		}, delegate
		{
			isFlying = false;
			RefreshCoinCount();
			InEndAction?.Invoke();
		}, delegate(int landed)
		{
			landedCoins = landed;
			UpdateCoinDisplay();
		});
	}

	private void UpdateCoinDisplay()
	{
		if (m_CoinCount != null && totalFlyCoins > 0)
		{
			int shown = baseCount + Mathf.RoundToInt((float)totalFlyValue * landedCoins / totalFlyCoins);
			m_CoinCount.text = shown.ToString();
		}
	}
}
