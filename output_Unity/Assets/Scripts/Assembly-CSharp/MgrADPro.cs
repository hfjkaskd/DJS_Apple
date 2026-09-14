using System;
using UnityEngine;

public class MgrADPro : MonoBehaviour
{
	private static MgrADPro _instance;

	private bool inited;

	private HFAIEGDEABG innerVAD;

	private MFEGLMEDNLA innerIAD;

	private DDCDJOKDLKF innerBAD;

	private int touchCount;

	private int lastInputTouchCount;

	private bool crashed;

	public static MgrADPro Instance => null;

	public void Init(OGLPGNHCOGO mainMediType, OGLPGNHCOGO subMediType)
	{
	}

	private void Update()
	{
	}

	private void OnApplicationPause(bool pause)
	{
	}

	private void AutoCheckForceFix()
	{
	}

	public static void RegisterBannerEvent(Action<JBKFEONJCPC.HBJFLCPJCAI.COMDLKINBAI> action)
	{
	}

	public static void UnRegisterBannerEvent(Action<JBKFEONJCPC.HBJFLCPJCAI.COMDLKINBAI> action)
	{
	}

	public static void RegisterInterstitialEvent(Action<bool> action)
	{
	}

	public static void UnRegisterInterstitialEvent(Action<bool> action)
	{
	}

	public static void RegisterRewardVideoEvent(Action<bool> action)
	{
	}

	public static void UnRegisterRewardVideoEvent(Action<bool> action)
	{
	}

	public bool HasVAD()
	{
		return false;
	}

	public void CheckLoadVAD()
	{
	}

	public void CheckShowVAD(Action<string, string> rewardAction, Action<bool> unreliable_closeAction, Action<string> beforeAction, Action<string, string> showedAction, Action<string, string> clickAction)
	{
	}

	public bool HasIAD()
	{
		return false;
	}

	public void CheckLoadIAD()
	{
	}

	public void CheckShowIAD(Action<bool> unreliable_closeAction, Action<string> beforeAction, Action<string, string> showedAction, Action<string, string> clickAction)
	{
	}

	public void StopLoadIAD()
	{
	}

	public bool IsBannerShowing()
	{
		return false;
	}

	public void CheckLoadBAD(Action<string, string> clickAction)
	{
	}

	public void CheckShowBAD()
	{
	}

	public void CheckHideBAD()
	{
	}

	public void CheckDestroyBAD()
	{
	}

	private PBJIHLKAFGG GetBADSize()
	{
		return default(PBJIHLKAFGG);
	}

	public float GetBannerAreaHeightInPixel(float marginPhonePixel = 0f)
	{
		return 0f;
	}

	public bool IsTablet()
	{
		return false;
	}
}
