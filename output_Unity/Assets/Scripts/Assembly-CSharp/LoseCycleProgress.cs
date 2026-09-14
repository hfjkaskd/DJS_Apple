using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>失败页环形倒计时进度。</summary>
public class LoseCycleProgress : MonoBehaviour
{
	[SerializeField]
	private Image m_ProgressM;

	[SerializeField]
	private Transform m_ProgressEMaskTrans;

	[SerializeField]
	private Transform m_ProgressEImageTrans;

	[SerializeField]
	private float m_DefaultRotateZ;

	[SerializeField]
	private TextMeshProCustom m_TimeText;

	private float TotalTime;

	private int LastDisplayedSecond;

	private uint tweenId;

	public void DoProgress(float InTime, Action InEndAction)
	{
		TotalTime = InTime;
		LastDisplayedSecond = -1;
		if (tweenId != 0)
		{
			DGNMMHCBFMI.Kill(tweenId);
		}
		float start = 1f;
		ShowProgress(start);
		float progress = start;
		tweenId = DGNMMHCBFMI.DoNum(0f, InTime, () => progress, x =>
		{
			progress = x;
			if (this != null)
			{
				ShowProgress(x);
				SetTimeText(x);
			}
		}, delegate
		{
			tweenId = 0;
			InEndAction?.Invoke();
		});
	}

	private void SetTimeText(float InProgress)
	{
		int second = Mathf.CeilToInt(InProgress * TotalTime);
		if (second != LastDisplayedSecond)
		{
			LastDisplayedSecond = second;
			if (m_TimeText != null)
			{
				m_TimeText.text = second.ToString();
			}
		}
	}

	private void ShowProgress(float InProgress)
	{
		if (m_ProgressM != null)
		{
			m_ProgressM.fillAmount = InProgress;
		}
		float angle = m_DefaultRotateZ + InProgress * 360f;
		if (m_ProgressEMaskTrans != null)
		{
			m_ProgressEMaskTrans.localEulerAngles = new Vector3(0f, 0f, angle);
		}
		if (m_ProgressEImageTrans != null)
		{
			m_ProgressEImageTrans.localEulerAngles = new Vector3(0f, 0f, -angle);
		}
	}
}
