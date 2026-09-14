using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 资源目录表：把原 AssetBundle 路径风格的 key 映射到工程内直接引用的资源。
/// 由编辑器脚本扫描 Assets/res 与 Assets/res_server 自动生成，存放于 Assets/Resources/GameResCatalog.asset。
/// key 规则：资源相对 Assets 的路径，全小写、去扩展名，例如 "res/local/home/home"。
/// 子资源（如图集内 Sprite）：主资源 key + "#" + 子资源名（小写）。
/// </summary>
public class GameResCatalog : ScriptableObject
{
	[System.Serializable]
	public class Entry
	{
		public string key;

		public Object asset;
	}

	public List<Entry> entries = new List<Entry>();

	private Dictionary<string, Object> map;

	private static GameResCatalog instance;

	public static GameResCatalog Instance
	{
		get
		{
			if (instance == null)
			{
				instance = Resources.Load<GameResCatalog>("GameResCatalog");
				if (instance == null)
				{
					Debug.LogError("[GameResCatalog] 未找到 Resources/GameResCatalog.asset，请先在编辑器执行 Tools/Build Resource Catalog");
					instance = CreateInstance<GameResCatalog>();
				}
				instance.BuildMap();
			}
			return instance;
		}
	}

	private void BuildMap()
	{
		map = new Dictionary<string, Object>(entries.Count);
		for (int i = 0; i < entries.Count; i++)
		{
			Entry e = entries[i];
			if (e != null && !string.IsNullOrEmpty(e.key) && e.asset != null)
			{
				map[e.key] = e.asset;
			}
		}
	}

	public Object Get(string key)
	{
		if (map == null)
		{
			BuildMap();
		}
		if (string.IsNullOrEmpty(key))
		{
			return null;
		}
		map.TryGetValue(key.ToLowerInvariant(), out Object obj);
		return obj;
	}

	public bool Contains(string key)
	{
		if (map == null)
		{
			BuildMap();
		}
		return !string.IsNullOrEmpty(key) && map.ContainsKey(key.ToLowerInvariant());
	}
}
