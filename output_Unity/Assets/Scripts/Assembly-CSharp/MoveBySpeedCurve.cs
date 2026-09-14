using System;
using UnityEngine;

public class MoveBySpeedCurve : MyMonoBehaviour
{
	private Vector2 direction;

	private Vector2 startPos;

	private Vector2 endPos;

	private float maxDis;

	private Action complete;

	private AnimationCurve curve;

	private bool canUpdate;

	private float time;

	private float maxTime;

	private float speedRatio;

	public void Move(Vector2 endPos, Action complete, AnimationCurve curve, float maxTime, float speedRatio)
	{
	}

	private void Update()
	{
	}
}
