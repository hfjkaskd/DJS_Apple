using System.Collections.Generic;
using System.Globalization;

public class BAHFLOGNJJA
{
	private const string Key_ImpressionData = "IPD";

	private static CultureInfo defCultureInfo;

	private const string Key_RVN001 = "IPD_001";

	private const string Key_RVN0005 = "IPD_0005";

	private const string Key_RVN002 = "IPD_002";

	private const string Key_RVN003 = "IPD_003";

	private static Dictionary<string, object> taichiEventParas;

	public static void OnImpressionTrackedEvent(IronSourceImpressionData impressionData)
	{
	}

	private static void LogToSelf(IronSourceImpressionData impressionData)
	{
	}

	public static void CheckSendValueToTaiChi(double currentImpressionRevenue)
	{
	}

	private static void LogToTaiChi(string key, double currentImpressionRevenue, float Limit, string eventName)
	{
	}

	private static void LogEvent(string eventName, float value)
	{
	}
}
