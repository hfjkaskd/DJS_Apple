using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Purchasing;

public class MgrAppsflyer : MonoBehaviour
{
	private static MgrAppsflyer _instance;

	private string AFUID;

	private CultureInfo cultureInfo;

	private Action<bool, string, string> ConversionCallback;

	private Dictionary<string, string> rvnParas;

	private readonly Dictionary<string, string> purchaseEvent;

	public static MgrAppsflyer Instance => null;

	public static bool AppsflyerReady { get; private set; }

	public void init(bool user_deny)
	{
	}

	public void configSDK(bool user_deny)
	{
	}

	public void startSDK(Action<bool, string, string> AFCallback)
	{
	}

	public void onConversionDataSuccess(string conversionData)
	{
	}

	public void onConversionDataFail(string error)
	{
	}

	public void LogEvent(string eventName, Dictionary<string, object> parameters)
	{
	}

	public void LogEventWithValue(string eventName, string currency, string value)
	{
	}

	public void LogADRevenue(IronSourceImpressionData impressionData)
	{
	}

	public void LogADRevenue(MaxSdkBase.AdInfo impressionData)
	{
	}

	public void TrackIAP(Product InProduct)
	{
	}
}
