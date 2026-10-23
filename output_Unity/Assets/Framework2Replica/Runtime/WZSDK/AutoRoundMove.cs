using DG.Tweening;
using UnityEngine;

namespace WZSDK
{
	public class AutoRoundMove : MonoBehaviour
	{
		public enum MoveDir
		{
			Horizontal = 0,
			Vertical = 1
		}

		public float roundTime = 1f;

		public float roundDis = 100f;

		public MoveDir roundDir;

		public bool ifAwake = true;

		private Vector3 orginPos;

		private RectTransform rect;

		private void Awake()
		{
			orginPos = transform.position;
			rect = GetComponent<RectTransform>();
			if (ifAwake)
			{
				StartMove();
			}
		}

		public void StartMove()
		{
			transform.position = orginPos;
			Sequence sequence = DOTween.Sequence();
			if (roundDir == MoveDir.Horizontal)
			{
				float start = rect.localPosition.x;
				sequence.Append(transform.DOLocalMoveX(start + roundDis, roundTime * 0.5f).SetEase(Ease.Linear));
				sequence.Append(transform.DOLocalMoveX(start, roundTime * 0.5f).SetEase(Ease.Linear));
			}
			else
			{
				float start = rect.localPosition.y;
				sequence.Append(transform.DOLocalMoveY(start + roundDis, roundTime * 0.5f).SetEase(Ease.Linear));
				sequence.Append(transform.DOLocalMoveY(start, roundTime * 0.5f).SetEase(Ease.Linear));
			}
			sequence.SetLoops(-1);
		}

		public void StopMove()
		{
			transform.position = orginPos;
			transform.DOKill(false);
		}
	}
}
