using System;
using System.Collections;
using CorePlay;
using Framework.Base.UtilModule;
using FruitApkEarning;
using UnityEngine;

/// <summary>胜利结算弹窗队列：评分弹窗后直接显示奖励结算，结束后进入下一关。</summary>
public class LDNJKLHLHAH : global::FOLJNEPEKCA<LDNJKLHLHAH>
{
	private static readonly int[] RateLevels = new int[] { 6, 16, 36, 66, 106 };

	public void StartQueue(bool delay, OGPEIPAJIDB InOpenWinUIFrom)
	{
		WinUI.openWinUIFrom = InOpenWinUIFrom;
		int finishedGeneration = EDLHEMMBABM.Instance.GetGeneration();
		if (TryShowRateUsPopUI(delegate
		{
			ShowSettlement(delay, finishedGeneration);
		}, false))
		{
			return;
		}
		ShowSettlement(delay, finishedGeneration);
	}

	private void ShowSettlement(bool delay, int finishedGeneration)
	{
		CoroutineManager.Instance.StartCor(WaitAndShowSettlement(delay, finishedGeneration));
	}

	private IEnumerator WaitAndShowSettlement(bool delay, int finishedGeneration)
	{
		if (delay) yield return new WaitForSeconds(0.3f);
		// A closing popup or an in-flight lucky reward may still own input. Wait
		// for it instead of opening the old WinUI behind the reward page.
		while (IsCurrentSettlement(finishedGeneration) &&
			(MCCIJBJGMCK.IsLock() || ApkEarningHost.IsOpen))
		{
			yield return null;
		}
		if (!IsCurrentSettlement(finishedGeneration)) yield break;

		ApkEarningService service = ApkEarningService.Instance;
		if ((service.Available && service.State.PendingRewards.Count > 0) ||
			(!service.Available && !string.IsNullOrEmpty(service.Error)))
		{
			GameAudio.Play(DLMJOHCOJKN.Play_sfx_ui_panel_game_win);
			ApkEarningHost.Open(false, true, () => ContinueAfterSettlement(finishedGeneration));
			if (ApkEarningHost.IsOpen) yield break;
			Debug.LogError("[Settlement] The reward page could not open. The reward remains saved.");
			yield break;
		}
		// Already-claimed rewards do not need another settlement screen.
		ContinueAfterSettlement(finishedGeneration);
	}

	private static bool IsCurrentSettlement(int finishedGeneration)
	{
		return EDLHEMMBABM.Instance.GetGeneration() == finishedGeneration &&
			JEFOMCDAPGK.Instance.Data.isLevelFinished &&
			CorePlayUI.Instance != null && CorePlayUI.Instance.IsOpening;
	}

	private static void ContinueAfterSettlement(int finishedGeneration)
	{
		if (!IsCurrentSettlement(finishedGeneration)) return;
		LevelConfig config = PLMIHDHFAAL.Instance.GetMainLevelConfig();
		if (config == null)
		{
			Debug.LogError("[Settlement] The next level configuration is missing.");
			return;
		}
		CorePlayUI.Instance.UpdateBehavior();
		JEFOMCDAPGK.Instance.MainLevelData.Reset(false);
		CoroutineManager.Instance.StartCor(EDLHEMMBABM.Instance.ParseConfig(config, false, null));
	}

	public bool TryShowNotifyPopUI(Action nextAction, bool onlyCheck, bool isLevelPass)
	{
		return false;
	}

	private bool TryShowRateUsPopUI(Action nextAction, bool onlyCheck)
	{
		if (BMNFNJFCPHG.Instance.HasRated())
		{
			return false;
		}
		bool hit = false;
		foreach (int level in RateLevels)
		{
			if (level == WinUI.wonLevel)
			{
				hit = true;
				break;
			}
		}
		if (!hit)
		{
			return false;
		}
		if (onlyCheck)
		{
			return true;
		}
		Module.Setting.UIRate.onCloseAction = nextAction;
		MgrUI.Instance.Open("pops/rate/RateUI");
		return true;
	}
}
