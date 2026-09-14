using System;
using IAP;
using TMPro;
using UnityEngine;

/// <summary>全局悬浮UI：飘字提示、切换背景加载遮罩等，挂在场景 GlobalUI 节点（sortingOrder 9000）。</summary>
public class MgrGlobalUI : MonoBehaviour
{
	private GameObject GlobalText;

	private GameObject SwitchBgLoading;

	private bool hasInited;

	[SerializeField]
	public Transform gameObjectPoolRoot;

	private int showGlobalTextCnt;

	private int showSwitchBgLoadingCnt;

	public static MgrGlobalUI Instance { get; private set; }

	public IAPBg IAPBg { get; private set; }

	public MyLines MyLines { get; private set; }

	private void Start()
	{
		Instance = this;
	}

	public void Init()
	{
		if (hasInited)
		{
			return;
		}
		hasInited = true;
		Instance = this;
		LoadGlobalText();
	}

	private void LoadGlobalText()
	{
		GameObject prefab = GameRes.LoadPrefab("res/local/globalui/GlobalText");
		if (prefab == null)
		{
			return;
		}
		GlobalText = UnityEngine.Object.Instantiate(prefab, transform, false);
		GlobalText.name = "GlobalText";
		GlobalText.SetActive(false);
	}

	private void LoadADSplashUI()
	{
	}

	private void LoadIAPBg()
	{
	}

	private void LoadSwitchBgLoading()
	{
		GameObject prefab = GameRes.LoadPrefab("res/local/globalui/SwitchBgLoading");
		if (prefab == null)
		{
			return;
		}
		SwitchBgLoading = UnityEngine.Object.Instantiate(prefab, transform, false);
		SwitchBgLoading.name = "SwitchBgLoading";
		SwitchBgLoading.SetActive(false);
	}

	public void ShowGlobalText(string InShowText, FFMLGGBCOOO InGlobalTip)
	{
		if (GlobalText == null)
		{
			LoadGlobalText();
		}
		if (GlobalText == null)
		{
			return;
		}
		TextMeshProUGUI text = GlobalText.GetComponentInChildren<TextMeshProUGUI>(true);
		if (text != null)
		{
			text.text = InShowText;
		}
		GlobalText.SetActive(true);
		Animation ani = GlobalText.GetComponent<Animation>();
		float duration = 1.5f;
		if (ani != null && ani["GlobalText_in"] != null)
		{
			ani.Play("GlobalText_in");
			duration = Mathf.Max(duration, ani["GlobalText_in"].length);
		}
		switch (InGlobalTip)
		{
		case FFMLGGBCOOO.positive:
			GameAudio.Play(DLMJOHCOJKN.Play_sfx_notice_common_positive);
			break;
		case FFMLGGBCOOO.negative:
			GameAudio.Play(DLMJOHCOJKN.Play_sfx_notice_common_negative);
			break;
		default:
			GameAudio.Play(DLMJOHCOJKN.Play_sfx_notice_common_neutral);
			break;
		}
		int cnt = ++showGlobalTextCnt;
		Timer.Instance.Delay(duration, delegate
		{
			if (cnt == showGlobalTextCnt && GlobalText != null)
			{
				GlobalText.SetActive(false);
			}
		});
	}

	public void ShowSwitchBgLoading()
	{
		if (SwitchBgLoading == null)
		{
			LoadSwitchBgLoading();
		}
		if (SwitchBgLoading == null)
		{
			return;
		}
		SwitchBgLoading.SetActive(true);
		Animation ani = SwitchBgLoading.GetComponent<Animation>();
		float idleTime = 0.5f;
		float outTime = 0.3f;
		string idleAniName = null;
		string outAniName = null;
		if (ani != null)
		{
			foreach (AnimationState state in ani)
			{
				if (state.name.ToLowerInvariant().Contains("out"))
				{
					outAniName = state.name;
					outTime = state.length;
				}
				else
				{
					idleAniName = state.name;
					idleTime = state.length;
				}
			}
			if (idleAniName != null)
			{
				ani.Play(idleAniName);
			}
		}
		int cnt = ++showSwitchBgLoadingCnt;
		MCCIJBJGMCK.Lock();
		Timer.Instance.Delay(idleTime, delegate
		{
			if (cnt != showSwitchBgLoadingCnt || SwitchBgLoading == null)
			{
				MCCIJBJGMCK.UnlockOnce();
				return;
			}
			if (ani != null && outAniName != null)
			{
				ani.Play(outAniName);
			}
			Timer.Instance.Delay(outTime, delegate
			{
				MCCIJBJGMCK.UnlockOnce();
				if (cnt == showSwitchBgLoadingCnt && SwitchBgLoading != null)
				{
					SwitchBgLoading.SetActive(false);
				}
			});
		});
	}
}
