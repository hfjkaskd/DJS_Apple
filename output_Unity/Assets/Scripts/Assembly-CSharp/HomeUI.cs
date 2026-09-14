using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>首页：Play 按钮、金币入口、设置入口。</summary>
public class HomeUI : BaseUI
{
	[SerializeField] private Button m_HarvestRewardsButton;

	[SerializeField]
	private GameObject m_SettingBtn;

	[SerializeField]
	private CommonCoinBtn m_CommonCoinBtn;

	[SerializeField]
	private GameObject m_NoADBtn;

	[SerializeField]
	private GameObject m_PlayBtn;

	[SerializeField]
	private GameObject m_PlayBtnHard;

	[SerializeField]
	private GameObject m_PlayBtnSuperHard;

	[SerializeField]
	private List<TextMeshProUGUI> m_LevelTexts;

	public override PAIEAGDLCBJ Layer => PAIEAGDLCBJ.Bottom;


	protected override void Init()
	{
		if (m_HarvestRewardsButton != null)
		{
			m_HarvestRewardsButton.onClick.AddListener(HarvestRewardsUI.Open);
		}
		if (m_SettingBtn != null)
		{
			MCCIJBJGMCK.Get(m_SettingBtn).onClick = OnSettingBtnClick;
		}
		if (m_NoADBtn != null)
		{
			MCCIJBJGMCK.Get(m_NoADBtn).onClick = OnNoADBtnClick;
		}
		if (m_PlayBtn != null)
		{
			MCCIJBJGMCK.Get(m_PlayBtn).onClick = OnPlayBtnClick;
		}
		if (m_PlayBtnHard != null)
		{
			MCCIJBJGMCK.Get(m_PlayBtnHard).onClick = OnPlayBtnClick;
		}
		if (m_PlayBtnSuperHard != null)
		{
			MCCIJBJGMCK.Get(m_PlayBtnSuperHard).onClick = OnPlayBtnClick;
		}
		if (m_CommonCoinBtn != null)
		{
			m_CommonCoinBtn.Init(FCCMMLGODPL.ShopinHome, true);
		}
	}

	protected override void BeforeOpen()
	{
		RefreshPlayBtnState();
		RefreshADBtnState();
		if (m_CommonCoinBtn != null)
		{
			m_CommonCoinBtn.AddCurCommonCoinBtn();
		}
	}

	protected override void AfterOpen()
	{
	}

	protected override void BeforeClose()
	{
		if (m_CommonCoinBtn != null)
		{
			m_CommonCoinBtn.RemoveCurCommonCoinBtn();
		}
	}

	private void OnSettingBtnClick(GameObject obj)
	{
		MgrUI.Instance.Open("pops/setting/HomeSettingUI");
	}

	private void OnNoADBtnClick(GameObject InGameObject)
	{
		MgrUI.Instance.Open("pops/removead/RemoveADUI");
	}

	private void OnPlayBtnClick(GameObject obj)
	{
		if (MCCIJBJGMCK.IsLock())
		{
			return;
		}
		MgrUI.Instance.Open("gameloading/GameLoading", false);
	}

	private void ShowIAD()
	{
	}

	private void CheckUpdateLanguage()
	{
	}

	private void RefreshPlayBtnState()
	{
		int level = JEFOMCDAPGK.Instance.CurMainLevelIndex;
		string levelStr = string.Format(OJEEJGGLNPC.Instance.GetText("common_levelwithindex"), level);
		if (m_LevelTexts != null)
		{
			foreach (TextMeshProUGUI text in m_LevelTexts)
			{
				if (text != null)
				{
					text.text = levelStr;
				}
			}
		}
		if (m_PlayBtn != null)
		{
			m_PlayBtn.SetActive(true);
		}
		if (m_PlayBtnHard != null)
		{
			m_PlayBtnHard.SetActive(false);
		}
		if (m_PlayBtnSuperHard != null)
		{
			m_PlayBtnSuperHard.SetActive(false);
		}
	}

	private void RefreshADBtnState()
	{
		if (m_NoADBtn != null)
		{
			m_NoADBtn.SetActive(true);
		}
	}

	private void Close()
	{
		MgrUI.Instance.Close("home/Home", false, false);
	}

	private void Update()
	{
	}
}
