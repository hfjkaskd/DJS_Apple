using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace InfiniteScrolling
{
	public class InfiniteScrollingVertical : MonoBehaviour
	{
		private sealed class LNEBHJAPFOC
		{
			public Action onComplete;

			internal void _003CQuickSlideToRow_003Eb__0()
			{
			}
		}

		[Header("基础信息")]
		public RectTransform point1;

		public RectTransform point2;

		public RectTransform contentRect;

		public ScrollRect scrollRect;

		[Header("自定义信息")]
		public GameObject rowTemp;

		public Vector2 margin;

		private float viewH;

		private float rowH;

		private List<BaseRow> rowList;

		private int rowListCount;

		private Action<float> onValueChanged;

		private bool hasMove;

		private bool doAnim;

		private bool initOnlyOnce;

		private BaseRow intermediateProduct;

		private const float apearSpaceTime = 0.03f;

		private const float waitOpen = 0.2f;

		private const float animTime = 0.3f;

		public float lastContentY { get; private set; }

		public bool inited { get; private set; }

		public void ChangeScrollRect(float height)
		{
		}

		public void Init(int total, int col, bool initOnlyOnce = true, Action<float> onValueChanged = null)
		{
		}

		public void ReInit(int total, int col)
		{
		}

		private void OnValueChanged(Vector2 v)
		{
		}

		private void RealOnValueChanged()
		{
		}

		public void Clear()
		{
		}

		public float GetAnyRowWorldPosY(int index)
		{
			return 0f;
		}

		public void UpdateAllRow()
		{
		}

		public void QuickSlideToRow(int index, Action onComplete = null)
		{
		}

		public void QuickSetToRow(int index, bool needOnValueChanged = false)
		{
		}

		public BaseRow GetIntermediateProduct()
		{
			return null;
		}

		public float GetOffset(int index, float maxOffset)
		{
			return 0f;
		}

		public float GetRowPos(int index)
		{
			return 0f;
		}

		public BaseRow GetRow(int index)
		{
			return null;
		}

		public List<BaseRow> GetRowList()
		{
			return null;
		}

		public void Dispose()
		{
		}
	}
}
