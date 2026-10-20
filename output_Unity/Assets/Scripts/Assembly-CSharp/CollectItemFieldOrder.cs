using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Keeps the visible fruit stack in the same order as occlusion priority.</summary>
public static class CollectItemFieldOrder
{
	private static readonly Comparison<CollectItem> CompareItems = Compare;

	public static void Restore(Transform fieldLayer, List<CollectItem> fieldItems)
	{
		if (fieldLayer == null || fieldItems == null)
		{
			return;
		}
		fieldItems.Sort(CompareItems);
		for (int i = 0; i < fieldItems.Count; i++)
		{
			CollectItem item = fieldItems[i];
			// Returning fruits belong to the field before their animation ends.
			if (item != null && item.transform.parent == fieldLayer)
			{
				item.transform.SetAsLastSibling();
			}
		}
	}

	private static int Compare(CollectItem left, CollectItem right)
	{
		if (left == right) return 0;
		if (left == null) return 1;
		if (right == null) return -1;
		int priority = left.Priority.CompareTo(right.Priority);
		return priority != 0 ? priority : left.Id.CompareTo(right.Id);
	}
}
