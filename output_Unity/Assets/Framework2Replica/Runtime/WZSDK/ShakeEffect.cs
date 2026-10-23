using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace WZSDK
{
	public class ShakeEffect : MonoBehaviour
	{
		public enum SwingMode
		{
			Rotation = 0,
			Position = 1
		}

		[Header("摇晃设置")]
		public float swingAngle = 15f;

		public Ease normalEase = Ease.InOutSine;

		public Ease fastEase = Ease.InOutSine;

		[Header("时间设置")]
		public float normalDuration = 0.3f;

		public float fastDuration = 0.05f;

		public float fastIntensity = 0.8f;

		[Header("自动摇晃")]
		public bool autoSwing = true;

		[Tooltip("动画完成后停顿的时间")]
		public float pauseAfterSwing = 1f;

		[Header("摇晃模式")]
		public SwingMode swingMode;

		private Vector3 originalPosition;

		private Quaternion originalRotation;

		private Sequence swingSequence;

		private Coroutine autoSwingCoroutine;

		private void OnEnable()
		{
			originalPosition = transform.localPosition;
			originalRotation = transform.localRotation;
			if (autoSwing)
			{
				StartAutoSwing();
			}
		}

		private void OnDisable()
		{
			StopAutoSwing();
			if (swingSequence != null && swingSequence.IsActive())
			{
				swingSequence.Kill();
				swingSequence = null;
			}
			ResetToOriginal();
		}

		private void OnDestroy()
		{
			if (swingSequence != null && swingSequence.IsActive())
			{
				swingSequence.Kill();
			}
			StopAutoSwing();
		}

		public void StartAutoSwing()
		{
			StopAutoSwing();
			autoSwingCoroutine = StartCoroutine(AutoSwingRoutine());
		}

		public void StopAutoSwing()
		{
			if (autoSwingCoroutine != null)
			{
				StopCoroutine(autoSwingCoroutine);
				autoSwingCoroutine = null;
			}
		}

		private IEnumerator AutoSwingRoutine()
		{
			while (autoSwing)
			{
				StartSwing();
				yield return new WaitForSeconds(CalculateAnimationTime());
				yield return new WaitForSeconds(pauseAfterSwing);
			}
		}

		private float CalculateAnimationTime()
		{
			return normalDuration * 0.8f + fastDuration * 4f + fastDuration * 0.5f;
		}

		public void StartSwing()
		{
			if (!gameObject.activeInHierarchy)
			{
				return;
			}
			if (swingSequence != null)
			{
				if (swingSequence.IsActive())
				{
					swingSequence.Kill();
				}
				swingSequence = null;
			}
			swingSequence = DOTween.Sequence();
			if (swingMode == SwingMode.Rotation)
			{
				StartRotationSwing();
			}
			else
			{
				StartPositionSwing();
			}
			swingSequence.OnComplete(ResetToOriginal);
		}

		private void StartRotationSwing()
		{
			float fastAngle = swingAngle * fastIntensity;
			swingSequence.Append(transform.DOLocalRotate(new Vector3(0f, 0f, swingAngle), normalDuration * 0.4f).SetEase(Ease.OutSine));
			swingSequence.Append(transform.DOLocalRotate(new Vector3(0f, 0f, -swingAngle), normalDuration * 0.4f).SetEase(Ease.InOutSine));
			swingSequence.Append(transform.DOLocalRotate(new Vector3(0f, 0f, fastAngle), fastDuration).SetEase(fastEase));
			swingSequence.Append(transform.DOLocalRotate(new Vector3(0f, 0f, -fastAngle), fastDuration).SetEase(fastEase));
			swingSequence.Append(transform.DOLocalRotate(new Vector3(0f, 0f, fastAngle * 0.8f), fastDuration * 0.9f).SetEase(fastEase));
			swingSequence.Append(transform.DOLocalRotate(new Vector3(0f, 0f, fastAngle * -0.8f), fastDuration * 0.9f).SetEase(fastEase));
			swingSequence.Append(transform.DOLocalRotate(new Vector3(0f, 0f, fastAngle * 0.6f), fastDuration * 0.8f).SetEase(fastEase));
			swingSequence.Append(transform.DOLocalRotate(new Vector3(0f, 0f, fastAngle * -0.6f), fastDuration * 0.8f).SetEase(fastEase));
			swingSequence.Append(transform.DOLocalRotate(new Vector3(0f, 0f, fastAngle * 0.4f), fastDuration * 0.7f).SetEase(fastEase));
			swingSequence.Append(transform.DOLocalRotate(new Vector3(0f, 0f, fastAngle * -0.4f), fastDuration * 0.7f).SetEase(fastEase));
			swingSequence.Append(transform.DOLocalRotate(Vector3.zero, fastDuration * 0.5f).SetEase(Ease.OutSine));
		}

		private void StartPositionSwing()
		{
			float distance = swingAngle * 0.1f;
			float fastDistance = distance * fastIntensity;
			swingSequence.Append(transform.DOLocalMoveX(originalPosition.x + distance, normalDuration * 0.4f).SetEase(Ease.OutSine));
			swingSequence.Append(transform.DOLocalMoveX(originalPosition.x - distance, normalDuration * 0.4f).SetEase(Ease.InOutSine));
			swingSequence.Append(transform.DOLocalMoveX(originalPosition.x + fastDistance, fastDuration).SetEase(fastEase));
			swingSequence.Append(transform.DOLocalMoveX(originalPosition.x - fastDistance, fastDuration).SetEase(fastEase));
			swingSequence.Append(transform.DOLocalMoveX(originalPosition.x + fastDistance * 0.8f, fastDuration * 0.9f).SetEase(fastEase));
			swingSequence.Append(transform.DOLocalMoveX(originalPosition.x - fastDistance * 0.8f, fastDuration * 0.9f).SetEase(fastEase));
			swingSequence.Append(transform.DOLocalMoveX(originalPosition.x + fastDistance * 0.6f, fastDuration * 0.8f).SetEase(fastEase));
			swingSequence.Append(transform.DOLocalMoveX(originalPosition.x - fastDistance * 0.6f, fastDuration * 0.8f).SetEase(fastEase));
			swingSequence.Append(transform.DOLocalMoveX(originalPosition.x + fastDistance * 0.4f, fastDuration * 0.7f).SetEase(fastEase));
			swingSequence.Append(transform.DOLocalMoveX(originalPosition.x - fastDistance * 0.4f, fastDuration * 0.7f).SetEase(fastEase));
			swingSequence.Append(transform.DOLocalMoveX(originalPosition.x, fastDuration * 0.5f).SetEase(Ease.OutSine));
		}

		public void StopSwing()
		{
			if (swingSequence != null && swingSequence.IsActive())
			{
				swingSequence.Kill();
			}
			swingSequence = null;
			ResetToOriginal();
		}

		private void ResetToOriginal()
		{
			transform.localPosition = originalPosition;
			transform.localRotation = originalRotation;
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.cyan;
			Vector3 positiveDirection = Quaternion.Euler(0f, 0f, swingAngle) * Vector3.up;
			Vector3 negativeDirection = Quaternion.Euler(0f, 0f, -swingAngle) * Vector3.up;
			Gizmos.DrawLine(transform.position, transform.position + positiveDirection);
			Gizmos.DrawLine(transform.position, transform.position + negativeDirection);
		}
	}
}
