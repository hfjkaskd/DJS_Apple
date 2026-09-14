using System;
using UnityEngine;

namespace InfiniteScrolling
{
	public abstract class BaseRow : MonoBehaviour
	{
		private RectTransform rect;

		private float rowH;

		private float worldHalfH;

		private int index;

		private Vector2 margin;

		private int row;

		protected int Total { get; private set; }

		public void Init(int index, Vector2 margin, int row, int total)
		{
		}

		public virtual void InitBeforeUpdatePos()
		{
		}

		public float GetTopY()
		{
			return 0f;
		}

		public float GetBottomY()
		{
			return 0f;
		}

		public void UpdatePos()
		{
		}

		public bool Move(int x)
		{
			return false;
		}

		public bool CheckCanMove(int x)
		{
			return false;
		}

		protected abstract void OnChangeRow(int index);

		public abstract void UpdateUI();

		public virtual void CamouflageByIndex(int index)
		{
		}

		public virtual void ChangeUIByIndex(int index)
		{
		}

		public virtual float BeforeMoveAnim(Action onComplete)
		{
			return 0f;
		}

		public virtual float OnMoveAnim(Action onComplete)
		{
			return 0f;
		}

		public virtual float AfterMoveAnim(Action onComplete)
		{
			return 0f;
		}

		public int GetIndex()
		{
			return 0;
		}

		public abstract void Dispose();
	}
}
