using TMPro;
using UnityEngine;

/// <summary>
/// 五胜宝箱进度条（WinUI 内）。
/// 文本采用累计式显示：第一箱 1/5…5/5，领取后切换为 5/10…10/10（对应录屏 137s“5/10 仅剩 5 关！”）。
/// 进度满时文案为“宝箱已解锁！”，随后弹出宝箱开启表现。
/// </summary>
public class LevelBoxProgress : MonoBehaviour
{
	[SerializeField]
	private CommonProgressBar m_ProgressBar;

	[SerializeField]
	private TextMeshProUGUI m_ProgressText;

	[SerializeField]
	private TextMeshProUGUI m_EncourageText;

	[SerializeField]
	private float m_ProgreeAniTime;

	private LevelBoxReward levelBoxReward;

	private bool needRefreshProgress;

	public void InitState(LevelBoxReward InLevelBoxReward)
	{
		levelBoxReward = InLevelBoxReward;
		if (levelBoxReward != null)
		{
			levelBoxReward.onHidden = ShowNextBoxState;
		}
		needRefreshProgress = true;
		int total = FPFGGCMEDND.LEVELS_PER_BOX;
		int baseWins = WinUI.boxIndexAtWin * total;
		if (m_ProgressBar != null)
		{
			m_ProgressBar.SetStartProgress((float)WinUI.prevBoxProgress / total);
		}
		if (m_ProgressText != null)
		{
			m_ProgressText.text = (baseWins + WinUI.prevBoxProgress) + "/" + (baseWins + total);
		}
		// 录屏中鼓励文案随进度动画出现，进度未刷新前不显示
		if (m_EncourageText != null)
		{
			m_EncourageText.gameObject.SetActive(false);
		}
	}

	/// <summary>进度未满显示“仅剩 {0} 关！”，满时显示“宝箱已解锁！”。</summary>
	private void UpdateEncourageText(int progress)
	{
		if (m_EncourageText == null)
		{
			return;
		}
		int remaining = FPFGGCMEDND.LEVELS_PER_BOX - progress;
		m_EncourageText.gameObject.SetActive(true);
		if (remaining > 0)
		{
			m_EncourageText.text = string.Format(OJEEJGGLNPC.Instance.GetText("winpage_levelbox"), remaining);
		}
		else
		{
			m_EncourageText.text = OJEEJGGLNPC.Instance.GetText("winpage_levelbox_complete");
		}
	}

	public void DoProgressAni()
	{
		if (!needRefreshProgress)
		{
			return;
		}
		needRefreshProgress = false;
		RefreshProgress();
	}

	private void RefreshProgress()
	{
		int total = FPFGGCMEDND.LEVELS_PER_BOX;
		int baseWins = WinUI.boxIndexAtWin * total;
		float aniTime = m_ProgreeAniTime > 0f ? m_ProgreeAniTime : 0.2f;
		if (m_ProgressText != null)
		{
			m_ProgressText.text = (baseWins + WinUI.newBoxProgress) + "/" + (baseWins + total);
		}
		UpdateEncourageText(WinUI.newBoxProgress);
		if (m_ProgressBar == null)
		{
			OnProgressDone();
			return;
		}
		m_ProgressBar.DoProgress((float)WinUI.newBoxProgress / total, aniTime, null, OnProgressDone);
	}

	private void OnProgressDone()
	{
		if (WinUI.boxCompleted && levelBoxReward != null)
		{
			Timer.Instance.Delay(0.5f, delegate
			{
				// WinUI 可能已被关闭（玩家提前点了下一关），此时不再弹宝箱
				if (levelBoxReward != null && gameObject.activeInHierarchy)
				{
					levelBoxReward.Open();
				}
			});
		}
	}

	/// <summary>宝箱领取后进度条切到下一箱：5/10、空条、“仅剩 5 关！”（录屏 137s）。</summary>
	private void ShowNextBoxState()
	{
		int total = FPFGGCMEDND.LEVELS_PER_BOX;
		int baseWins = FPFGGCMEDND.Instance.NextLevelBoxIndex * total;
		int progress = FPFGGCMEDND.Instance.CurLevelBoxProgress;
		if (m_ProgressBar != null)
		{
			m_ProgressBar.SetStartProgress((float)progress / total);
		}
		if (m_ProgressText != null)
		{
			m_ProgressText.text = (baseWins + progress) + "/" + (baseWins + total);
		}
		UpdateEncourageText(progress);
	}
}
