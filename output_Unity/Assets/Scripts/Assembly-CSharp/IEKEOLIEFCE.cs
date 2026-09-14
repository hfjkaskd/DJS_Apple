using System;

public class IEKEOLIEFCE
{
	public static void LoadVAD()
	{
	}

	public static void LoadIAD()
	{
	}

	public static void LoadBanner()
	{
	}

	public static bool HasValidVAD()
	{
		return false;
	}

	public static bool HasValidIAD()
	{
		return false;
	}

	public static void ShowVAD(Action<string, string> showAction = null, Action<string, string> rewardAction = null, Action<bool> closeAction = null, Action<string> beforeAction = null, Action<string, string> clickAction = null)
	{
	}

	public static void ShowIAD(Action<string, string> showAction = null, Action<bool> closeAction = null, Action<string> beforeAction = null, Action<string, string> clickAction = null)
	{
	}

	public static void ShowBAD()
	{
	}

	public static void HideBAD()
	{
	}

	public static void DestroyBAD()
	{
	}

	public static void RemoveIAD()
	{
	}

	public static void AddOnADStatusChanged(Action<bool> action)
	{
	}

	public static void RemoveOnADStatusChanged(Action<bool> action)
	{
	}

	public static float GetBannerHeightInCanvas()
	{
		return 0f;
	}
}
