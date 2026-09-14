using System;
using Project.UI;
using UnityEngine;

/// <summary>失败页：倒计时复活（广告/免费）或重开本关。</summary>
public class LoseUI : BaseUI
{
	[SerializeField]
	private GameObject m_RestartBtn;

	[SerializeField]
	private AdButtonState m_ADReviveState;

	[SerializeField]
	private LoseCycleProgress m_progress;

	[SerializeField]
	private GameObject m_FreeReviveGuide;

	[SerializeField]
	private GameObject m_FreeReviveBtn;

	private bool HasLeaveLoseUI;

	private float startTime;

	private const float REVIVE_COUNTDOWN = 9f;

	public override PAIEAGDLCBJ Layer => PAIEAGDLCBJ.Mid;


	protected override void Init()
	{
		if (m_RestartBtn != null)
		{
			MCCIJBJGMCK.Get(m_RestartBtn).onClick = OnRestartBtnClick;
		}
		if (m_ADReviveState != null)
		{
			m_ADReviveState.InitClickAction(OnADReviveBtnClick);
		}
		if (m_FreeReviveBtn != null)
		{
			MCCIJBJGMCK.Get(m_FreeReviveBtn).onClick = OnFreeReviveBtnClick;
		}
	}

	protected override void BeforeOpen()
	{
		HasLeaveLoseUI = false;
		startTime = Time.time;
		GameAudio.Play(DLMJOHCOJKN.Play_sfx_ui_panel_game_lose);
		bool showFreeRevive = !JEFOMCDAPGK.Instance.Data.hasShowFreeRevive;
		if (m_FreeReviveGuide != null)
		{
			m_FreeReviveGuide.SetActive(showFreeRevive);
		}
		if (m_FreeReviveBtn != null)
		{
			m_FreeReviveBtn.SetActive(showFreeRevive);
		}
	}

	protected override void AfterOpen()
	{
		if (m_progress != null)
		{
			m_progress.DoProgress(REVIVE_COUNTDOWN, delegate
			{
				if (!HasLeaveLoseUI)
				{
					OnRestartBtnClick(null);
				}
			});
		}
	}

	protected override void BeforeClose()
	{
	}

	private void OnADReviveBtnClick(GameObject InGameObject)
	{
		if (HasLeaveLoseUI)
		{
			return;
		}
		HasLeaveLoseUI = true;
		CloseSelf();
		EDLHEMMBABM.Instance.Revive("ad");
	}

	private void OnRestartBtnClick(GameObject InGameObject)
	{
		if (HasLeaveLoseUI)
		{
			return;
		}
		HasLeaveLoseUI = true;
		CloseSelf();
		JEFOMCDAPGK.Instance.LevelLose();
		LevelConfig config = PLMIHDHFAAL.Instance.GetMainLevelConfig();
		if (config != null)
		{
			JEFOMCDAPGK.Instance.MainLevelData.Reset(true);
			Framework.Base.UtilModule.CoroutineManager.Instance.StartCor(EDLHEMMBABM.Instance.ParseConfig(config, true, null));
		}
	}

	private void OnFreeReviveBtnClick(GameObject InGameObject)
	{
		if (HasLeaveLoseUI)
		{
			return;
		}
		HasLeaveLoseUI = true;
		JEFOMCDAPGK.Instance.SetShowFreeReviveState(true);
		CloseSelf();
		EDLHEMMBABM.Instance.Revive("free");
	}

	private void CloseSelf()
	{
		MgrUI.Instance.Close("coreplaywin/LoseUI", false, false);
	}
}
