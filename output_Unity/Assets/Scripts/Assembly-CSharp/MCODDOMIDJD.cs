using System;
using System.Collections.Generic;
using Firebase.Analytics;
using UnityEngine.Purchasing;

public class MCODDOMIDJD
{
	private sealed class MPGHPPDEIOJ
	{
		public string eventName;

		public Parameter[] eventParams;

		internal void _003CSendOrQueueFirebaseEvent_003Eb__0()
		{
		}
	}

	private const int MAX_PENDING = 100;

	private static readonly Queue<Action> pendingEvents;

	public static bool FirebaseReady { get; private set; }

	public static void init(bool user_deny)
	{
	}

	public static void configSDK(bool user_deny)
	{
	}

	public static void startSDK()
	{
	}

	private static void LogFirebaseEvent(string eventName, Parameter[] paras)
	{
	}

	public static void LogEvent(string eventName, Dictionary<string, object> parameters)
	{
	}

	private static void EnqueuePending(Action sendAction, string eventName)
	{
	}

	private static void FlushPendingEvents()
	{
	}

	private static void SendOrQueueFirebaseEvent(string eventName, Parameter[] eventParams)
	{
	}

	public static void LogRevenueEvnet(string eventName, float value)
	{
	}

	public static void LogAdImpression(IronSourceImpressionData impressionData)
	{
	}

	public static void LogAdImpression(MaxSdkBase.AdInfo impressionData)
	{
	}

	public static void TrackIAP(Product InProduct)
	{
	}
}
