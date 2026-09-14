using System;
using TMPro;
using UnityEngine;

public class ShopRewardItem : MonoBehaviour
{
	private sealed class OBEFOMEAGHJ
	{
		public ShopRewardItem _003C_003E4__this;

		public Action onComplete;

		internal void _003CPlayFlyToTarget_003Eb__1()
		{
		}
	}

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
	private CommonSingleFly m_CloseScript;

	[SerializeField]
	private Animation m_RewardItemAni;

	[SerializeField]
	private float m_InAniTime;

	private const string ITEM_IN_ANI = "anim_ShopRewardItem_in";

	private const string ITEM_LOOP_ANI = "anim_ShopRewardItem_loop";

	private const string ITEM_START_FLY_STATE = "anim_ShopRewardItem_fly";

	private bool isFlying;

	public POJCEPBNNIP ItemType { get; private set; }

	public int ItemCount { get; private set; }

	public void Init(POJCEPBNNIP InItemType, int InCount)
	{
	}

	public void PlayFlyToTarget(Vector3 targetPos, float direction, Action onComplete = null)
	{
	}
}
