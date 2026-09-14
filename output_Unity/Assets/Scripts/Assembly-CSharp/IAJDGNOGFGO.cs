using System.IO;
using UnityEngine;

/// <summary>存档模型基类：JSON 序列化到 persistentDataPath。</summary>
public abstract class IAJDGNOGFGO<DataT, ModelT> : HAHGPHDEOLA where DataT : class, new() where ModelT : class, new()
{
	private static ModelT instance;

	protected DataT data;

	private bool needSave;

	protected bool isFirstInit;

	public static ModelT Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new ModelT();
			}
			return instance;
		}
		private set
		{
			instance = value;
		}
	}

	public virtual void Init()
	{
		string path = GetFilePath();
		if (File.Exists(path))
		{
			try
			{
				data = JsonUtility.FromJson<DataT>(File.ReadAllText(path));
			}
			catch
			{
				data = null;
			}
		}
		if (data == null)
		{
			data = new DataT();
			isFirstInit = true;
			AfterFirstInitData();
			TrySaveToDisk();
		}
	}

	protected virtual void AfterFirstInitData()
	{
	}

	public void SaveData()
	{
		needSave = true;
		TrySaveToDisk();
	}

	protected abstract string GetKey();

	private string GetFilePath()
	{
		return Path.Combine(Application.persistentDataPath, GetKey() + ".json");
	}

	public bool TrySaveToDisk()
	{
		if (data == null)
		{
			return false;
		}
		try
		{
			File.WriteAllText(GetFilePath(), JsonUtility.ToJson(data));
			needSave = false;
			return true;
		}
		catch
		{
			return false;
		}
	}
}
