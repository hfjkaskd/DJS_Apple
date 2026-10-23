using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	public class SHoleMask : MonoBehaviour, ICanvasRaycastFilter
	{
		public bool ifMask;

		public Transform hole;

		public CanvasGroup holeMask;

		public Sprite SquareHole;

		public Sprite CircularHole;

		public Camera ObjCanvas;

		[Space]
		[Header("Scene Example")]
		public SHoleMaskEnum sGuideEnum;

		public Transform hitPoint;

		public Vector2 correction;

		public float CHitDistance;

		public Vector2 SHitDistance;

		[Space]
		public Color lineColor = Color.white;

		private Vector2 targetPos;

		private void Start()
		{
			SetTarget();
			SetMask();
		}

		public void SetSTarget(Transform target, Vector2 SHitDistance, Vector2 correction)
		{
			sGuideEnum = SHoleMaskEnum.Square;
			this.SHitDistance = SHitDistance;
			SetTarget(target, correction);
		}

		public void SetCTarget(Transform target, float CHitDistance, Vector2 correction)
		{
			sGuideEnum = SHoleMaskEnum.Circular;
			this.CHitDistance = CHitDistance;
			SetTarget(target, correction);
		}

		private void SetTarget(Transform target, Vector2 correction)
		{
			hitPoint = target;
			this.correction = correction;
			SetTarget();
			SetMask();
		}

		public void SetTarget()
		{
			if (hitPoint == null)
			{
				Debug.LogError("SHoleMask hitPoint is null.");
				return;
			}

			RectTransform rectTransform =
				hitPoint.GetComponent<RectTransform>();
			targetPos = rectTransform == null
				? (Vector2)ObjCanvas.WorldToScreenPoint(hitPoint.position) +
					correction
				: (Vector2)hitPoint.position + correction;
		}

		public void SetMask()
		{
			hole.gameObject.SetActive(ifMask);
			holeMask.alpha = ifMask ? 1f : 0f;
			if (!ifMask)
			{
				return;
			}

			hole.transform.position = targetPos;
			RectTransform rectTransform =
				hole.GetComponent<RectTransform>();
			Image image = hole.GetComponent<Image>();
			if (sGuideEnum == SHoleMaskEnum.Circular)
			{
				image.sprite = CircularHole;
				float diameter = CHitDistance * 2f;
				rectTransform.sizeDelta =
					new Vector2(diameter, diameter);
			}
			else if (sGuideEnum == SHoleMaskEnum.Square)
			{
				image.sprite = SquareHole;
				rectTransform.sizeDelta = SHitDistance;
			}
		}

		public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
		{
			return true;
		}

		private void OnDrawGizmos()
		{
			if (hitPoint == null)
			{
				return;
			}

			RectTransform rectTransform =
				hitPoint.GetComponent<RectTransform>();
			if (rectTransform == null && ObjCanvas == null)
			{
				return;
			}

			Vector2 position = rectTransform == null
				? (Vector2)ObjCanvas.WorldToScreenPoint(hitPoint.position)
				: (Vector2)hitPoint.position;
			position += correction;
			Gizmos.color = lineColor;
			if (sGuideEnum == SHoleMaskEnum.Circular)
			{
				Gizmos.DrawWireSphere(position, CHitDistance);
			}
			else if (sGuideEnum == SHoleMaskEnum.Square)
			{
				Gizmos.DrawWireCube(position, SHitDistance);
			}
		}
	}
}
