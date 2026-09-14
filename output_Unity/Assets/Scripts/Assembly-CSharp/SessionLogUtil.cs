using UnityEngine;

public class SessionLogUtil
{
	private static long tsAPBA;

	private static long tsGT;

	private const long GT_GAP = 30000L;

	private static bool enteredGame;

	private static bool sessionStarted;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	public static void LogAPLS()
	{
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	public static void LogSPSH()
	{
	}

	public static void LogLOPR(int InIndex)
	{
	}

	public static void LogENGA()
	{
	}

	public static void LogSEST()
	{
	}

	public static void LogAPBA()
	{
	}

	public static void LogHOST()
	{
	}

	public static void LogSYCH(string InType, int InResult)
	{
	}

	public static void RefreshTsGT()
	{
	}

	public static void TryLogGT()
	{
	}

	public static void LogGT()
	{
	}

	public static KGLCJHCMEGJ GetCommonLog(bool InHasEventSeq = true)
	{
		return null;
	}

	public static string GetCurrentTimeStamp()
	{
		return null;
	}

	private static long GetCurrentTimeStampMilliseconds()
	{
		return 0L;
	}

	public static long GetMonoTimeStamp()
	{
		return 0L;
	}

	private static string GetLaunchSource()
	{
		return null;
	}
}
