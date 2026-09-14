using System;
using System.Collections.Generic;
using Framework.Base.Res.AB;

public class DKGIDMLAIHN
{
	private string dirIndex;

	private string savekey;

	private readonly Dictionary<string, ABLocalCache> dic;

	private ABCacheConfig remoteAssetsLocalCacheConfig;

	private Func<string, string> callback;

	public DKGIDMLAIHN(string InSavekey, string InDirIndex, Func<string, string> InCallback)
	{
	}

	public void Init(string InConfigData)
	{
	}

	private bool CheckFileExist(string InKey)
	{
		return false;
	}

	public void InitByNewDir()
	{
	}

	public bool IsSameDirIndex(string key)
	{
		return false;
	}

	public static string GetDirIndex(string InKey)
	{
		return null;
	}

	public void AddOrUpdateKey(string InKey, int InVer, int InSver)
	{
	}

	public bool ContainsKey(string InKey)
	{
		return false;
	}

	internal ABLocalCache Get(string InKey)
	{
		return null;
	}

	internal void Delete(string InKey)
	{
	}

	public void DoSave()
	{
	}
}
