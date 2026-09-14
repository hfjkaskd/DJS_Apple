using System.Collections.Generic;
using Project.UI;
using TMPro;
using UnityEngine;

/// <summary>五胜宝箱奖励展示：宝箱开启 → 奖励飞出 → 领取后飞入 HUD。</summary>
public class LevelBoxReward : MonoBehaviour
{
	[SerializeField]
	private Animation m_Anim;

	[SerializeField]
	private string m_OpenName;

	[SerializeField]
	private string m_IdleName;

	[SerializeField]
	private string m_CloseName;

	[SerializeField]
	private Transform m_RewardParent;

	[SerializeField]
	private RectTransform m_RewardStartPos;

	[SerializeField]
	private List<RectTransform> m_RewardItemPoses;

	[SerializeField]
	private List<TextMeshProUGUI> m_ADCoinCount;

	[SerializeField]
	private AdButtonState m_ADCoinBtn;

	[SerializeField]
	private GameObject m_ClaimBtn;

	[SerializeField]
	private float m_StartFlyDelayTime;

	[SerializeField]
	private float m_EndFlyDelayTime;

	[SerializeField]
	private float m_AfterEndFlyToCloseTime;

	private static GameObject rewardItemPrefab;

	private List<SingleRewardItem> rewardItems = new List<SingleRewardItem>();

	private List<IGOBDCAPLDG> levelBoxRewardItems;

	private List<float> boxRewardDirections = new List<float>();

	private bool hasCoinReward;

	private bool hasADCoinReward;

	private bool hasPlayedItemToBtnSound;

	private Transform itemFlyEndPos;

	private const int FLY_COIN_NUM = 20;

	private float Fly1Time;

	/// <summary>宝箱关闭（领取完成）后回调，用于进度条切换到下一箱显示。</summary>
	public System.Action onHidden;

	public void Init(Transform InItemFlyEndPos)
	{
		itemFlyEndPos = InItemFlyEndPos;
		gameObject.SetActive(false);
		if (m_ClaimBtn != null)
		{
			MCCIJBJGMCK.Get(m_ClaimBtn).onClick = OnClaimBtnClick;
		}
		if (m_ADCoinBtn != null)
		{
			m_ADCoinBtn.InitClickAction(OnADCoinBtnClick);
		}
		if (m_ADCoinCount != null)
		{
			foreach (TextMeshProUGUI text in m_ADCoinCount)
			{
				if (text != null)
				{
					text.text = "+" + FPFGGCMEDND.AD_COIN_COUNT;
				}
			}
		}
	}

	public void Open()
	{
		MCCIJBJGMCK.Lock();
		gameObject.SetActive(true);
		ApplySortingAboveHost();
		levelBoxRewardItems = FPFGGCMEDND.Instance.GetLastBoxRewards();
		hasPlayedItemToBtnSound = false;
		if (m_ClaimBtn != null)
		{
			m_ClaimBtn.SetActive(false);
		}
		GameAudio.Play(DLMJOHCOJKN.Play_sfx_anim_chest_idle);
		float openLen = 0.5f;
		if (m_Anim != null && !string.IsNullOrEmpty(m_OpenName) && m_Anim[m_OpenName] != null)
		{
			m_Anim.Play(m_OpenName);
			openLen = m_Anim[m_OpenName].length;
			if (!string.IsNullOrEmpty(m_IdleName) && m_Anim[m_IdleName] != null)
			{
				m_Anim.PlayQueued(m_IdleName, QueueMode.CompleteOthers);
			}
		}
		Timer.Instance.Delay(openLen + Mathf.Max(m_StartFlyDelayTime, 0f), delegate
		{
			if (this != null)
			{
				GameAudio.Play(DLMJOHCOJKN.Play_sfx_anim_chest_open);
				StartFlyAct();
			}
		});
	}

	private void StartFlyAct()
	{
		ClearRewardItems();
		if (rewardItemPrefab == null)
		{
			rewardItemPrefab = GameRes.LoadPrefab("res/local/boxreward/RewardItem");
		}
		if (rewardItemPrefab == null || levelBoxRewardItems == null || levelBoxRewardItems.Count == 0)
		{
			ShowClaim();
			return;
		}
		int pending = 0;
		for (int i = 0; i < levelBoxRewardItems.Count; i++)
		{
			IGOBDCAPLDG rewardData = levelBoxRewardItems[i];
			GameObject go = Instantiate(rewardItemPrefab, m_RewardParent != null ? m_RewardParent : transform, false);
			SingleRewardItem item = go.GetComponent<SingleRewardItem>();
			if (item == null)
			{
				Destroy(go);
				continue;
			}
			item.Init(rewardData);
			rewardItems.Add(item);
			Vector3 startPos = m_RewardStartPos != null ? m_RewardStartPos.position : transform.position;
			Vector3 endPos = m_RewardItemPoses != null && i < m_RewardItemPoses.Count && m_RewardItemPoses[i] != null
				? m_RewardItemPoses[i].position
				: startPos + new Vector3((i - (levelBoxRewardItems.Count - 1) * 0.5f) * 2f, 1f, 0f);
			float direction = i % 2 == 0 ? -1f : 1f;
			boxRewardDirections.Add(direction);
			pending++;
			item.OpenFlyAct(startPos, endPos, direction, delegate
			{
				GameAudio.Play(DLMJOHCOJKN.Play_sfx_anim_reward_land);
			}, delegate
			{
				pending--;
				if (pending == 0 && this != null)
				{
					ShowClaim();
				}
			});
		}
		if (pending == 0)
		{
			ShowClaim();
		}
	}

	/// <summary>
	/// 预制体内烘焙的 sortingOrder（底板550、内容580–582）低于 WinUI 运行时动态层级
	/// （welldone+25、LevelText+30、FlyCoinRoot+50），整体平移到宿主 UI+60 之上，保持内部相对顺序。
	/// </summary>
	private void ApplySortingAboveHost()
	{
		BaseUI rootUI = GetComponentInParent<BaseUI>();
		if (rootUI == null)
		{
			return;
		}
		const int BAKED_BASE = 550;
		Canvas[] canvases = GetComponentsInChildren<Canvas>(true);
		if (bakedOrders == null)
		{
			bakedOrders = new Dictionary<Canvas, int>();
			foreach (Canvas canvas in canvases)
			{
				if (canvas.overrideSorting)
				{
					bakedOrders[canvas] = canvas.sortingOrder;
				}
			}
		}
		int delta = rootUI.MyOrder + 60 - BAKED_BASE;
		foreach (Canvas canvas in canvases)
		{
			if (canvas.overrideSorting && bakedOrders.TryGetValue(canvas, out int baked))
			{
				canvas.sortingOrder = baked + delta;
			}
		}
	}

	private Dictionary<Canvas, int> bakedOrders;

	private void ShowClaim()
	{
		MCCIJBJGMCK.UnlockAll();
		if (m_ClaimBtn != null)
		{
			m_ClaimBtn.SetActive(true);
		}
	}

	private void Hide()
	{
		float closeLen = 0f;
		if (m_Anim != null && !string.IsNullOrEmpty(m_CloseName) && m_Anim[m_CloseName] != null)
		{
			m_Anim.Play(m_CloseName);
			closeLen = m_Anim[m_CloseName].length;
		}
		Timer.Instance.Delay(closeLen, delegate
		{
			if (this != null)
			{
				ClearRewardItems();
				gameObject.SetActive(false);
				onHidden?.Invoke();
			}
			FPFGGCMEDND.Instance.CurBoxHasOpened = true;
			MCCIJBJGMCK.UnlockAll();
		});
	}

	private void ClearRewardItems()
	{
		foreach (SingleRewardItem item in rewardItems)
		{
			if (item != null)
			{
				Destroy(item.gameObject);
			}
		}
		rewardItems.Clear();
		boxRewardDirections.Clear();
	}

	private void ItemFlyToBtnEnd()
	{
		if (!hasPlayedItemToBtnSound)
		{
			hasPlayedItemToBtnSound = true;
			GameAudio.Play(DLMJOHCOJKN.Play_sfx_noitce_coin_land);
		}
	}

	private void OnADCoinBtnClick(GameObject InGameObject)
	{
		GNEJJHEDEBL.Instance.AddItem(POJCEPBNNIP.Coin, FPFGGCMEDND.AD_COIN_COUNT, false, NMNPFKEJMHO.WinUIAD);
		CommonCoinBtn.FlyCoins(FLY_COIN_NUM, InGameObject != null ? InGameObject.transform.position : transform.position);
		if (m_ADCoinBtn != null)
		{
			m_ADCoinBtn.gameObject.SetActive(false);
		}
	}

	private void OnClaimBtnClick(GameObject InGameObject)
	{
		if (m_ClaimBtn != null)
		{
			m_ClaimBtn.SetActive(false);
		}
		MCCIJBJGMCK.Lock();
		int pending = rewardItems.Count;
		if (pending == 0)
		{
			Hide();
			return;
		}
		GameAudio.Play(DLMJOHCOJKN.Play_sfx_anim_coin_fly);
		for (int i = 0; i < rewardItems.Count; i++)
		{
			SingleRewardItem item = rewardItems[i];
			float direction = i < boxRewardDirections.Count ? boxRewardDirections[i] : 1f;
			Vector3 endPos = itemFlyEndPos != null ? itemFlyEndPos.position : transform.position;
			item.CloseFlyAct(item.transform.position, endPos, direction, ItemFlyToBtnEnd, delegate
			{
				pending--;
				if (pending == 0 && this != null)
				{
					Timer.Instance.Delay(Mathf.Max(m_AfterEndFlyToCloseTime, 0f), Hide);
				}
			});
		}
	}
}
