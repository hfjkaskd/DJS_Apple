using System;
using System.Collections;
using CorePlay;
using Framework.Base.UtilModule;
using FruitApkEarning;
using FruitEarningVariants;
using UnityEngine;

/// <summary>胜利结算弹窗队列：直接显示奖励结算，结束后进入下一关。</summary>
public class LDNJKLHLHAH : global::FOLJNEPEKCA<LDNJKLHLHAH>
{
	public void StartQueue(bool delay, OGPEIPAJIDB InOpenWinUIFrom)
	{
		WinUI.openWinUIFrom = InOpenWinUIFrom;
		int finishedGeneration = EDLHEMMBABM.Instance.GetGeneration();
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
			(MCCIJBJGMCK.IsLock() || EarningVariantController.HasCommercialPanel ||
			 (EarningVariantSelection.IsB && (EarningVariantController.Instance == null ||
			  EarningVariantController.Instance.Runtime == null || !EarningVariantController.Instance.Runtime.Ready))))
		{
			yield return null;
		}
		if (!IsCurrentSettlement(finishedGeneration)) yield break;
		if (EarningVariantSelection.IsB)
		{
			GameAudio.Play(DLMJOHCOJKN.Play_sfx_ui_panel_game_win);
			EarningVariantController.Instance.ShowSettlement(() => ContinueAfterSettlement(finishedGeneration));
			yield break;
		}

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

}
