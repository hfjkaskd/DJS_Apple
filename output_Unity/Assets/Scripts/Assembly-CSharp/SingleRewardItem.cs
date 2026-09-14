using System;
using TMPro;
using UnityEngine;

/// <summary>宝箱奖励单项：按类型显示图标与数量，支持出现/收取两段飞行。</summary>
public class SingleRewardItem : MonoBehaviour
{
	[SerializeField]
	private GameObject m_ItemObject;

	[SerializeField]
	private GameObject m_Coin;

	[SerializeField]
	private GameObject m_Undo;

	[SerializeField]
	private GameObject m_Magic;

	[SerializeField]
	private GameObject m_Shuffle;

	[SerializeField]
	private TextMeshProUGUI m_CoinCount;

	[SerializeField]
	private TextMeshProUGUI m_UndoCount;

	[SerializeField]
	private TextMeshProUGUI m_MagicCount;

	[SerializeField]
	private TextMeshProUGUI m_ShuffleCount;

	[SerializeField]
	private CommonSingleFly m_OpenFly;

	[SerializeField]
	private CommonSingleFly m_CloseScript;

	[SerializeField]
	private GameObject m_EffectTrail;

	[SerializeField]
	private GameObject m_EffectShow;

	[SerializeField]
	private GameObject m_EffectHit;

	public IGOBDCAPLDG RewardItem { get; private set; }

	public void Init(IGOBDCAPLDG InLevelBoxRewardItem)
	{
		RewardItem = InLevelBoxRewardItem;
		if (m_Coin != null)
		{
			m_Coin.SetActive(InLevelBoxRewardItem.itemType == POJCEPBNNIP.Coin);
		}
		if (m_Undo != null)
		{
			m_Undo.SetActive(InLevelBoxRewardItem.itemType == POJCEPBNNIP.Undo);
		}
		if (m_Magic != null)
		{
			m_Magic.SetActive(InLevelBoxRewardItem.itemType == POJCEPBNNIP.Magic);
		}
		if (m_Shuffle != null)
		{
			m_Shuffle.SetActive(InLevelBoxRewardItem.itemType == POJCEPBNNIP.Shuffle);
		}
		string countText = "x" + InLevelBoxRewardItem.count;
		switch (InLevelBoxRewardItem.itemType)
		{
		case POJCEPBNNIP.Coin:
			if (m_CoinCount != null)
			{
				m_CoinCount.text = countText;
			}
			break;
		case POJCEPBNNIP.Undo:
			if (m_UndoCount != null)
			{
				m_UndoCount.text = countText;
			}
			break;
		case POJCEPBNNIP.Magic:
			if (m_MagicCount != null)
			{
				m_MagicCount.text = countText;
			}
			break;
		case POJCEPBNNIP.Shuffle:
			if (m_ShuffleCount != null)
			{
				m_ShuffleCount.text = countText;
			}
			break;
		}
		if (m_EffectTrail != null)
		{
			m_EffectTrail.SetActive(false);
		}
		if (m_EffectHit != null)
		{
			m_EffectHit.SetActive(false);
		}
	}

	public void OpenFlyAct(Vector3 InStartPos, Vector3 InEndpos, float InDirection, Action InFlyEndAction, Action InAfterEndDelayAction)
	{
		if (m_EffectShow != null)
		{
			m_EffectShow.SetActive(true);
		}
		if (m_OpenFly != null)
		{
			m_OpenFly.FlyObject(gameObject, m_ItemObject, InStartPos, InEndpos, InDirection, InFlyEndAction, InAfterEndDelayAction);
		}
		else
		{
			transform.position = InEndpos;
			InFlyEndAction?.Invoke();
			InAfterEndDelayAction?.Invoke();
		}
	}

	public void CloseFlyAct(Vector3 InStartPos, Vector3 InEndpos, float InDirection, Action InFlyEndAction, Action InAfterEndDelayAction)
	{
		if (m_EffectTrail != null)
		{
			m_EffectTrail.SetActive(true);
		}
		if (m_CloseScript != null)
		{
			m_CloseScript.FlyObject(gameObject, m_ItemObject, InStartPos, InEndpos, InDirection, delegate
			{
				if (m_EffectHit != null)
				{
					m_EffectHit.SetActive(true);
				}
				InFlyEndAction?.Invoke();
			}, InAfterEndDelayAction);
		}
		else
		{
			transform.position = InEndpos;
			InFlyEndAction?.Invoke();
			InAfterEndDelayAction?.Invoke();
		}
	}
}
