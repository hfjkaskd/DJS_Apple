using System.Collections.Generic;
using UnityEngine;

public class UploadCrashMono : MonoBehaviour
{
	private static readonly HashSet<string> errorSet;

	private static readonly HashSet<string> errorADSet;

	public static void AdLogError(string strContent)
	{
	}

	private void Start()
	{
	}

	private void LogCallback(string strContent, string stackTrace, LogType type)
	{
	}
}
