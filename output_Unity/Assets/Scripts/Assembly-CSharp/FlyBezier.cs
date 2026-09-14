using System;
using System.Collections.Generic;
using UnityEngine;

public class FlyBezier : MonoBehaviour
{
	private sealed class DJPIACDCEPG
	{
		public FlyBezier _003C_003E4__this;

		public Action endAction;

		internal float _003CStartMove_003Eb__0()
		{
			return 0f;
		}

		internal void _003CStartMove_003Eb__1(float x)
		{
		}

		internal void _003CStartMove_003Eb__2()
		{
		}
	}

	private sealed class MDKDNKMIFFL
	{
		public FlyBezier _003C_003E4__this;

		public Vector3 end;

		public Action endAction;

		internal float _003CStartMove_003Eb__0()
		{
			return 0f;
		}

		internal void _003CStartMove_003Eb__1(float x)
		{
		}

		internal void _003CStartMove_003Eb__2()
		{
		}
	}

	public Transform target;

	public Transform start;

	public Transform end;

	public List<Transform> controls;

	public float duration;

	public bool enableScale;

	public float startSclae;

	public float endSclae;

	public AnimationCurve curve;

	private float progress;

	private bool moving;

	private readonly List<Vector3> points;

	private uint tween;

	public void StartMove(Action endAction = null)
	{
	}

	public void StartMove(Vector3 start, Vector3 end, List<Vector3> controls, Action endAction = null)
	{
	}

	private void UpdateAnim()
	{
	}

	public void StopMove()
	{
	}

	private static int GetCmn(int m, int n)
	{
		return 0;
	}

	private static int Factorial_n_to_mPlus1(int n, int m)
	{
		return 0;
	}

	private static int Factorial(int n)
	{
		return 0;
	}
}
