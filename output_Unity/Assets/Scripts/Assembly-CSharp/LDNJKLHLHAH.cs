using System;

/// <summary>胜利结算弹窗队列：评分/通知弹窗优先，最后打开 WinUI。</summary>
public class LDNJKLHLHAH : global::FOLJNEPEKCA<LDNJKLHLHAH>
{
	private static readonly int[] RateLevels = new int[] { 6, 16, 36, 66, 106 };

	public void StartQueue(bool delay, OGPEIPAJIDB InOpenWinUIFrom)
	{
		WinUI.openWinUIFrom = InOpenWinUIFrom;
		if (TryShowRateUsPopUI(delegate
		{
			ShowWInUI(delay, InOpenWinUIFrom);
		}, false))
		{
			return;
		}
		ShowWInUI(delay, InOpenWinUIFrom);
	}

	private void ShowWInUI(bool InDelay, OGPEIPAJIDB InOpenWinUIFrom)
	{
		if (InDelay)
		{
			MgrUI.Instance.DelayOpen("coreplaywin/WinUI", 0.3f);
		}
		else
		{
			MgrUI.Instance.Open("coreplaywin/WinUI");
		}
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
