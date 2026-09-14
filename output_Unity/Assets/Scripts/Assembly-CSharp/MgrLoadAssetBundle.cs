using System;
using System.Collections.Generic;
using UnityEngine;

public class MgrLoadAssetBundle : BaseSingleMono<MgrLoadAssetBundle>
{
	private sealed class AIJHMBFLAIM
	{
		public MgrLoadAssetBundle _003C_003E4__this;

		public string path;

		internal void _003CLoadAB_003Eb__0(AssetBundle ab, string str)
		{
		}

		internal void _003CLoadAB_003Eb__1(string s)
		{
		}
	}

	private sealed class DFGJFOGBLHJ
	{
		public MgrLoadAssetBundle _003C_003E4__this;

		public string path;

		internal void _003CDownloadAB_003Eb__0(AssetBundle ab, string str)
		{
		}

		internal void _003CDownloadAB_003Eb__1(string s)
		{
		}

		internal void _003CDownloadAB_003Eb__2(float cur, float max)
		{
		}
	}

	public const string EmptyName = "null";

	public Dictionary<string, CIFGEPAEBBD> pathLoadDic;

	public void Init()
	{
	}

	public void LoadObjecFromAB(string path, string name, Action<UnityEngine.Object> loaded, Action<string> failed)
	{
	}

	public void DownLoadObjecFromAB(string path, Action success, Action<string> failed, Action<float, float> progressAction = null)
	{
	}

	private void LoadAB(string path)
	{
	}

	private void DownloadAB(string path)
	{
	}

	public void CancelLoad(string path, string name, Action<UnityEngine.Object> loadedAction, Action<string> failed, Action<float, float> progressAction, Action success)
	{
	}

	public void Release(string path)
	{
	}
}
