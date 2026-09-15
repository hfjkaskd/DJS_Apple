using System.Collections.Generic;
using Project.UI;
using TMPro;
using UnityEngine;

/// <summary>胜利结算页：Well Done、宝箱进度、+50 广告金币与下一关按钮。</summary>
public class WinUI : BaseUI
{


	[SerializeField]
	private List<TextMeshProUGUI> m_BtnLevels;

	[SerializeField]
	private TextMeshProUGUI m_WinLevel;

	[SerializeField]
	private List<GameObject> m_PlayBtns;

	[SerializeField]
	private List<TextMeshProUGUI> m_ADCoinCounts;

	[SerializeField]
	private AdButtonState m_AdBtn;

	[SerializeField]
	private CommonCoinBtn m_CommonCoinBtn;

	[SerializeField]
	private float openLockTime;

	[SerializeField]
	private LevelBoxProgress m_LevelBoxProgress;

	[SerializeField]
	private LevelBoxReward m_LevelBoxReward;

	private const int WIN_AD_GOLD_NUM = 50;

	private const int WIN_AD_FLY_GOLD_NUM = 20;

	public static OGPEIPAJIDB openWinUIFrom;

	public static int wonLevel;

	public static int prevBoxProgress;

	public static int newBoxProgress;

	public static bool boxCompleted;

	/// <summary>本次结算时所处的宝箱序号（OnLevelPass 之前捕获），用于累计式进度文本（5/5、5/10…）。</summary>
	public static int boxIndexAtWin;

	private bool adRewardClaimed;
	private bool harvestPromptWaitingForBox;

	public bool IsHarvestRewardPromptReady => IsOpening && !harvestPromptWaitingForBox
		&& (m_LevelBoxReward == null || !m_LevelBoxReward.gameObject.activeInHierarchy);

	public override PAIEAGDLCBJ Layer => PAIEAGDLCBJ.Mid;

	// Victory content reaches +50; the chest modal reserves +60 through +92.
	public override int OwnLayerCnt => 100;


	protected override void Init()
	{

		if (m_PlayBtns != null)
		{
			foreach (GameObject btn in m_PlayBtns)
			{
				if (btn != null)
				{
					MCCIJBJGMCK.Get(btn).onClick = OnPlayBtn;
				}
			}
		}
		if (m_AdBtn != null)
		{
			m_AdBtn.InitClickAction(OnADPlayBtn);
		}
		if (m_CommonCoinBtn != null)
		{
			m_CommonCoinBtn.Init(FCCMMLGODPL.ShopinWin, false);
		}
		if (m_LevelBoxReward != null)
		{
			m_LevelBoxReward.Init(m_CommonCoinBtn != null ? m_CommonCoinBtn.transform : transform);
		}
	}

	protected override void BeforeOpen()
	{
		adRewardClaimed = false;
		harvestPromptWaitingForBox = boxCompleted && m_LevelBoxProgress != null && m_LevelBoxReward != null;
		GameAudio.Play(DLMJOHCOJKN.Play_sfx_ui_panel_game_win);
		string levelFormat = OJEEJGGLNPC.Instance.GetText("common_levelwithindex");
		if (m_WinLevel != null)
		{
			m_WinLevel.text = string.Format(levelFormat, wonLevel);
		}
		int nextLevel = JEFOMCDAPGK.Instance.CurMainLevelIndex;
		if (m_BtnLevels != null)
		{
			foreach (TextMeshProUGUI text in m_BtnLevels)
			{
				if (text != null)
				{
					text.text = string.Format(levelFormat, nextLevel);
				}
			}
		}
		if (m_ADCoinCounts != null)
		{
			foreach (TextMeshProUGUI text in m_ADCoinCounts)
			{
				if (text != null)
				{
					text.text = "+" + WIN_AD_GOLD_NUM;
				}
			}
		}
		if (m_LevelBoxProgress != null)
		{
			m_LevelBoxProgress.InitState(m_LevelBoxReward);
		}
		if (m_LevelBoxReward != null)
		{
			m_LevelBoxReward.onHidden += OnHarvestChestHidden;
		}
		if (m_CommonCoinBtn != null)
		{
			m_CommonCoinBtn.AddCurCommonCoinBtn();
		}
	}

	protected override void AfterOpen()
	{
		if (openLockTime > 0f)
		{
			MCCIJBJGMCK.Lock();
			Timer.Instance.Delay(openLockTime, delegate
			{
				MCCIJBJGMCK.UnlockOnce();
			});
		}
		Timer.Instance.Delay(0.2f, delegate
		{
			if (this != null && m_LevelBoxProgress != null)
			{
				m_LevelBoxProgress.DoProgressAni();
			}
		});
	}

	protected override void BeforeClose()
	{
		if (m_LevelBoxReward != null)
		{
			m_LevelBoxReward.onHidden -= OnHarvestChestHidden;
		}
		if (m_CommonCoinBtn != null)
		{
			m_CommonCoinBtn.RemoveCurCommonCoinBtn();
		}
	}

	private void ShowContentForTest()
	{
	}

	private void OnHarvestChestHidden()
	{
		harvestPromptWaitingForBox = false;
	}

	private void OnPlayBtn(GameObject obj)
	{
		if (MCCIJBJGMCK.IsLock())
		{
			return;
		}
		Close();
		if (CorePlay.CorePlayUI.Instance != null)
		{
			CorePlay.CorePlayUI.Instance.UpdateBehavior();
			LevelConfig config = PLMIHDHFAAL.Instance.GetMainLevelConfig();
			if (config != null)
			{
				JEFOMCDAPGK.Instance.MainLevelData.Reset(false);
				Framework.Base.UtilModule.CoroutineManager.Instance.StartCor(EDLHEMMBABM.Instance.ParseConfig(config, false, null));
			}
		}
	}

	private bool NeedShowTreeDownloadPopup()
	{
		return false;
	}

	private void OnADPlayBtn(GameObject obj)
	{
		if (adRewardClaimed || MCCIJBJGMCK.IsLock())
		{
			return;
		}
		adRewardClaimed = true;
		GNEJJHEDEBL.Instance.AddItem(POJCEPBNNIP.Coin, WIN_AD_GOLD_NUM, false, NMNPFKEJMHO.WinUIAD);
		CommonCoinBtn.FlyCoins(WIN_AD_FLY_GOLD_NUM, obj != null ? obj.transform.position : transform.position);
	}

	private void Close()
	{
		MgrUI.Instance.Close("coreplaywin/WinUI", false, false);
	}
}
