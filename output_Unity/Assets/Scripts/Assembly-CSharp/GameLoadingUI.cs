using TMPro;
using UnityEngine;

/// <summary>
/// 启动加载页。冷启动首次进入时先显示“开始”按钮与隐私文案（对应录屏 0.6–1.8s），
/// 点击后切换为进度条（3.5s 填满）并进入玩法；非首次直接走进度条。
/// </summary>
public class GameLoadingUI : BaseUI
{
	/// <summary>由 GameEntry 在首次冷启动时置真：先显示开始按钮再走进度。</summary>
	public static bool StartMode;

	[SerializeField]
	private GameObject btnStart;

	[SerializeField]
	private GameObject privacyGo;

	[SerializeField]
	private CommonProgressBar m_GameStartLoading;

	private const float PROGRESS_TOTAL_TIME = 3.5f;

	public override PAIEAGDLCBJ Layer => PAIEAGDLCBJ.Top;


	protected override void Init()
	{
		if (btnStart != null)
		{
			InputMono input = MCCIJBJGMCK.Get(btnStart);
			input.playClickSound = true;
			input.onClick = OnStartBtnClick;
		}
		LocalizeTexts();
	}

	private void LocalizeTexts()
	{
		if (btnStart != null)
		{
			SetChildText(btnStart.transform, "LevelText", OJEEJGGLNPC.Instance.GetText("coldstart_start"));
			SetChildText(btnStart.transform, "LevelTextShadow", OJEEJGGLNPC.Instance.GetText("coldstart_start"));
		}
		if (privacyGo != null)
		{
			SetChildText(privacyGo.transform, "Privacy", OJEEJGGLNPC.Instance.GetText("coldstart_start_privacy"));
		}
	}

	private static void SetChildText(Transform root, string childName, string value)
	{
		Transform child = root.Find(childName);
		if (child == null)
		{
			return;
		}
		TextMeshProUGUI text = child.GetComponent<TextMeshProUGUI>();
		if (text != null)
		{
			text.text = value;
		}
	}

	protected override void BeforeOpen()
	{
		bool startMode = StartMode;
		if (privacyGo != null)
		{
			privacyGo.SetActive(startMode);
		}
		if (btnStart != null)
		{
			btnStart.SetActive(startMode);
		}
		if (m_GameStartLoading != null)
		{
			m_GameStartLoading.SetActive(!startMode);
			if (!startMode)
			{
				m_GameStartLoading.SetStartProgress(0f);
			}
		}
	}

	protected override void AfterOpen()
	{
		if (!StartMode)
		{
			ShowProgress();
		}
	}

	protected override void BeforeClose()
	{
	}

	private void OnStartBtnClick(GameObject InGameObject)
	{
		if (!StartMode)
		{
			return;
		}
		StartMode = false;
		AgreePrivacy(InGameObject);
		if (privacyGo != null)
		{
			privacyGo.SetActive(false);
		}
		if (btnStart != null)
		{
			btnStart.SetActive(false);
		}
		if (m_GameStartLoading != null)
		{
			m_GameStartLoading.SetActive(true);
			m_GameStartLoading.SetStartProgress(0f);
		}
		ShowProgress();
	}

	private void CheckShowPrivacy()
	{
	}

	private void AgreePrivacy(GameObject gameObject)
	{
	}

	private void ShowProgress()
	{
		DoProgress(PROGRESS_TOTAL_TIME);
	}

	private void DoProgress(float InTotalTime)
	{
		if (m_GameStartLoading == null)
		{
			EnterCorePlay();
			return;
		}
		MCCIJBJGMCK.Lock();
		m_GameStartLoading.DoProgress(1f, InTotalTime, null, delegate
		{
			Timer.Instance.Delay(0.4f, delegate
			{
				MCCIJBJGMCK.UnlockOnce();
				EnterCorePlay();
			});
		});
	}

	private void EnterCorePlay()
	{
		MgrUI.Instance.Close("home/Home", false, false);
		MgrUI.Instance.Open("coreplay/CorePlayUI", false);
		Timer.Instance.Delay(0.8f, delegate
		{
			MgrUI.Instance.Close("gameloading/GameLoading", true, false);
		});
	}
}
