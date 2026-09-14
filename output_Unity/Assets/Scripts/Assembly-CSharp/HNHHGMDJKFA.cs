using System;
using System.Collections.Generic;
using UnityEngine;

internal static class HNHHGMDJKFA
{
	private sealed class PINKNCHJLKO
	{
		public List<Action<float, float>> progList;

		public string path;

		internal void _003CStartDownload_003Eb__0(float downloadedBytes, float progress)
		{
		}

		internal void _003CStartDownload_003Eb__1(AssetBundle ab, string p)
		{
		}
	}

	private sealed class JHFONGFPIFM
	{
		public string failPath;

		internal void _003CStartDownload_003Eb__3()
		{
		}
	}

	private const int MAX_RETRY_COUNT = 1;

	private const float RETRY_INTERVAL = 120f;

	public static int MaxConcurrency;

	private static Queue<string> downloadQueue;

	private static HashSet<string> downloadPending;

	private static Dictionary<string, List<Action>> successCallbacks;

	private static Dictionary<string, List<Action>> failCallbacks;

	private static Dictionary<string, List<Action<float, float>>> progressCallbacks;

	private static Dictionary<string, int> retryCounts;

	private static int activeDownloads;

	public static void Enqueue(string path, Action onSuccess = null, Action onFail = null, Action<float, float> onProgress = null)
	{
	}

	public static void EnqueueFront(string path, Action onSuccess = null, Action onFail = null, Action<float, float> onProgress = null)
	{
	}

	private static void EnqueueInternal(string path, Action onSuccess, Action onFail, Action<float, float> onProgress, bool toFront)
	{
	}

	private static void ProcessNext()
	{
	}

	private static void StartDownload(string path)
	{
	}

	private static void InvokeCallbacks(Dictionary<string, List<Action>> callbackDict, string path)
	{
	}
}
