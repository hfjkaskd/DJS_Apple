using System.Collections.Generic;
using UnityEngine;

public static class FindChildUtility
{
	public static GameObject FindChild(GameObject parent, string childName)
	{
		return parent == null ? null : FindChildRecursive(parent.transform, childName);
	}

	public static T FindChild<T>(GameObject parent, string childName) where T : Component
	{
		GameObject child = FindChild(parent, childName);
		return child == null ? null : child.GetComponent<T>();
	}

	public static GameObject FindChild(Transform parent, string childName)
	{
		return FindChildRecursive(parent, childName);
	}

	public static T FindChild<T>(Transform parent, string childName) where T : Component
	{
		GameObject child = FindChild(parent, childName);
		return child == null ? null : child.GetComponent<T>();
	}

	private static GameObject FindChildRecursive(Transform parent, string childName)
	{
		if (parent == null)
		{
			return null;
		}
		foreach (Transform child in parent)
		{
			if (child.name == childName)
			{
				return child.gameObject;
			}
			GameObject nested = FindChildRecursive(child, childName);
			if (nested != null)
			{
				return nested;
			}
		}
		return null;
	}

	public static List<GameObject> FindAllChildren(GameObject parent, string childName)
	{
		List<GameObject> results = new List<GameObject>();
		if (parent != null)
		{
			FindAllChildrenRecursive(parent.transform, childName, results);
		}
		return results;
	}

	private static void FindAllChildrenRecursive(Transform parent, string childName, List<GameObject> results)
	{
		if (parent == null || results == null)
		{
			return;
		}
		foreach (Transform child in parent)
		{
			if (child.name == childName)
			{
				results.Add(child.gameObject);
			}
			FindAllChildrenRecursive(child, childName, results);
		}
	}

	public static GameObject FindChildByPath(GameObject parent, string path)
	{
		if (parent == null)
		{
			return null;
		}
		Transform child = parent.transform.Find(path);
		return child == null ? null : child.gameObject;
	}

	public static GameObject FindChildIncludeInactive(GameObject parent, string childName)
	{
		return parent == null
			? null
			: FindChildRecursiveIncludeInactive(parent.transform, childName);
	}

	private static GameObject FindChildRecursiveIncludeInactive(Transform parent, string childName)
	{
		if (parent == null)
		{
			return null;
		}
		for (int i = 0; i < parent.childCount; i++)
		{
			Transform child = parent.GetChild(i);
			if (child.name == childName)
			{
				return child.gameObject;
			}
			GameObject nested = FindChildRecursiveIncludeInactive(child, childName);
			if (nested != null)
			{
				return nested;
			}
		}
		return null;
	}
}
