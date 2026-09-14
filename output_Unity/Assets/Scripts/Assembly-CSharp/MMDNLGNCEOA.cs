using System;
using System.Collections.Generic;
using UnityEngine.Purchasing;

public class MMDNLGNCEOA
{
	private sealed class NHBNGLPBPLL
	{
		public string eventName;

		public Dictionary<string, object> finalParameters;

		internal void _003CLogEvent_003Eb__0()
		{
		}
	}

	private static bool inner_user_deny;

	private const int MAX_PENDING = 100;

	private static readonly Queue<Action> pendingEvents;

	public static bool FacebookReady { get; private set; }

	public static void init(bool user_deny)
	{
	}

	public static void configSDK(bool user_deny)
	{
	}

	private static void startSDK()
	{
	}

	private static void InitCallback()
	{
	}

	private static void OnHideUnity(bool isGameShown)
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

	private static void LogEventInternal(string eventName, Dictionary<string, object> parameters)
	{
	}

	public static void TrackIAP(Product InProduct)
	{
	}
}
