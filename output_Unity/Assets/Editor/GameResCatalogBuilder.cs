using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 扫描 Assets/res 与 Assets/res_server，生成 Resources/GameResCatalog.asset。
/// 进入播放模式前若目录表缺失或过期会自动重建。
/// </summary>
public static class GameResCatalogBuilder
{
	private const string CatalogPath = "Assets/Resources/GameResCatalog.asset";

	private static readonly string[] ScanRoots = { "Assets/res", "Assets/res_server" };

	[MenuItem("Tools/Build Resource Catalog")]
	public static void Build()
	{
		if (!Directory.Exists("Assets/Resources"))
		{
			Directory.CreateDirectory("Assets/Resources");
		}
		GameResCatalog catalog = AssetDatabase.LoadAssetAtPath<GameResCatalog>(CatalogPath);
		if (catalog == null)
		{
			catalog = ScriptableObject.CreateInstance<GameResCatalog>();
			AssetDatabase.CreateAsset(catalog, CatalogPath);
		}
		catalog.entries.Clear();
		var seen = new HashSet<string>();

		foreach (string root in ScanRoots)
		{
			if (!Directory.Exists(root))
			{
				continue;
			}
			string[] guids = AssetDatabase.FindAssets("", new[] { root });
			foreach (string guid in guids)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(guid);
				if (AssetDatabase.IsValidFolder(assetPath))
				{
					continue;
				}
				string key = KeyFromPath(assetPath);
				Object main = AssetDatabase.LoadMainAssetAtPath(assetPath);
				if (main == null)
				{
					continue;
				}
				AddEntry(catalog, seen, key, main);

				// 子资源：图集中的 Sprite、嵌套资源等
				Object[] all = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);
				foreach (Object sub in all)
				{
					if (sub == null || sub == main)
					{
						continue;
					}
					AddEntry(catalog, seen, key + "#" + sub.name.ToLowerInvariant(), sub);
				}
			}
		}

		EditorUtility.SetDirty(catalog);
		AssetDatabase.SaveAssets();
		Debug.Log($"[GameResCatalogBuilder] 目录表生成完成，共 {catalog.entries.Count} 条");
	}

	private static void AddEntry(GameResCatalog catalog, HashSet<string> seen, string key, Object asset)
	{
		if (seen.Add(key))
		{
			catalog.entries.Add(new GameResCatalog.Entry { key = key, asset = asset });
		}
	}

	private static string KeyFromPath(string assetPath)
	{
		string p = assetPath.Replace('\\', '/');
		if (p.StartsWith("Assets/"))
		{
			p = p.Substring("Assets/".Length);
		}
		int dot = p.LastIndexOf('.');
		int slash = p.LastIndexOf('/');
		if (dot > slash)
		{
			p = p.Substring(0, dot);
		}
		return p.ToLowerInvariant();
	}

	[InitializeOnLoadMethod]
	private static void AutoBuildIfMissing()
	{
		EditorApplication.delayCall += () =>
		{
			if (AssetDatabase.LoadAssetAtPath<GameResCatalog>(CatalogPath) == null)
			{
				Build();
			}
		};
	}
}
