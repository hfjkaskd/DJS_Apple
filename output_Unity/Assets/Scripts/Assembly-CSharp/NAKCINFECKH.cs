using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class NAKCINFECKH
{
	private static readonly Dictionary<string, OKHHOKKHEIC> assetBundleDic;

	private static readonly StringBuilder strb;

	private static readonly List<string> unReleaseBundleNameList;

	private static readonly string assetbundle;

	public static void LoadAssetAsync<T>(OLMENPNGMNE InAddress, Action<T> InCallback) where T : UnityEngine.Object
	{
	}

	public static void LoadAssetAsync<T>(string InAbKey, string InAssetName, Action<T> InCallback) where T : UnityEngine.Object
	{
	}

	public static T LoadAssetSync<T>(OLMENPNGMNE InAddress) where T : UnityEngine.Object
	{
		return null;
	}

	public static T LoadAssetSync<T>(string InAbKey, string InAssetName) where T : UnityEngine.Object
	{
		return null;
	}

	internal static OKHHOKKHEIC GetLoader(string InAbKey, bool InIsManually)
	{
		return null;
	}

	private static string GetBundleAddressByKey(string InAbKey)
	{
		return null;
	}

	public static void ReleaseAsset(string InAbKey, bool withForce = false)
	{
	}

	public static void ForceReleaseAllBundleByPrefix(string gameTypePrefix)
	{
	}

	public static string GetUnReleasedBundleInfo(string gamePlayFilter, out bool isSuccessful)
	{
		isSuccessful = default(bool);
		return null;
	}

	public static string GetUnReleasedBundleInfo(List<string> gamePlayList)
	{
		return null;
	}
}
