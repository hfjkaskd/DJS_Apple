using System;

public class AMMDNMBFOMD
{
	private sealed class IGODCLENFGM
	{
		public bool shouldRemoveLock;

		public string pos;

		public Action openAction;

		public Action rewardAction;

		public int index;

		public Action closeAction;

		public Action _003C_003E9__6;

		internal void _003CShowVideo_003Eb__0()
		{
		}

		internal void _003CShowVideo_003Eb__1(string network, string msg)
		{
		}

		internal void _003CShowVideo_003Eb__2(string network, string msg)
		{
		}

		internal void _003CShowVideo_003Eb__6()
		{
		}

		internal void _003CShowVideo_003Eb__3(bool normal_close)
		{
		}

		internal void _003CShowVideo_003Eb__4(string curNetwork)
		{
		}

		internal void _003CShowVideo_003Eb__5(string network, string msg)
		{
		}
	}

	private sealed class OACJPEMIELE
	{
		public string pos;

		public Action openAction;

		public int index;

		public Action closeAction;

		internal void _003CShowInterstitial_003Eb__0(string network, string msg)
		{
		}

		internal void _003CShowInterstitial_003Eb__1(bool normal_close)
		{
		}

		internal void _003CShowInterstitial_003Eb__2(string curNetwork)
		{
		}

		internal void _003CShowInterstitial_003Eb__3(string network, string msg)
		{
		}
	}

	private const string LastIADShowTimeKey = "LastIADShowTimeKey";

	private static DateTime lastIADShowTime;

	private static DateTime nextIADShowTime;

	private static MyDateTime myLastIADShowTime;

	private static int interstitialAds_SpaceTime;

	public static int BannerEnableLevel;

	public static int InterstitialEnableLevel;

	public static int InterstitialCloseDelay;

	public const string BannerAds_EnableLevel = "BannerAds_EnableLevel";

	public const string InterstitialAds_EnableLevel = "InterstitialAds_EnableLevel";

	public const string InterstitialAds_SpaceTime = "InterstitialAds_SpaceTime";

	public static bool isFistStartIAD;

	public static bool IfDelayRewardBackDoor;

	public static float DelayRewardBackDoorTime;

	public static void Init()
	{
	}

	public static void InitConfig()
	{
	}

	private static void SaveIADLastShowTime()
	{
	}

	private static void GetSaveWinIAD()
	{
	}

	public static void CheckLoadAllAD()
	{
	}

	public static void OnLevelPass()
	{
	}

	public static void CheckLoadBanner()
	{
	}

	public static void CheckLoadInterstitial()
	{
	}

	public static void ShowVideo(string pos, Action openAction = null, Action rewardAction = null, Action closeAction = null, bool needLockView = true)
	{
	}

	public static bool CheckReachNextIADShow()
	{
		return false;
	}

	public static bool CheckCanShow()
	{
		return false;
	}

	public static void ShowInterstitial(string pos, Action openAction = null, Action closeAction = null, Action noAdAction = null)
	{
	}

	public static void UpdateNextIADTime()
	{
	}

	private static void LogAD(string adEventName, string pos, string adInfo)
	{
	}

	public static void TryShowBanner()
	{
	}

	public static void HideBAD()
	{
	}

	public static bool CanShowBanner()
	{
		return false;
	}

	public static void OnBecomeRemoveAD()
	{
	}
}
