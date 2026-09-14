using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Google.Play.Common;
using Google.Play.Review;
using UnityEngine;

public class InAppReviewCtr : MonoBehaviour
{
	private sealed class LKACNIPHCNA : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public InAppReviewCtr _003C_003E4__this;

		private float _003Ctimer_003E5__2;

		private ReviewManager _003C_reviewManager_003E5__3;

		private PlayAsyncOperation<PlayReviewInfo, ReviewErrorCode> _003CrequestFlowOperation_003E5__4;

		private PlayAsyncOperation<VoidResult, ReviewErrorCode> _003ClaunchFlowOperation_003E5__5;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return null;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return null;
			}
		}

		[DebuggerHidden]
		public LKACNIPHCNA(int _003C_003E1__state)
		{
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
		}

		private bool MoveNext()
		{
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
		}
	}

	private sealed class CDPDHGAKLBB : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public InAppReviewCtr _003C_003E4__this;

		public float restTime;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return null;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return null;
			}
		}

		[DebuggerHidden]
		public CDPDHGAKLBB(int _003C_003E1__state)
		{
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
		}

		private bool MoveNext()
		{
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
		}
	}

	private static InAppReviewCtr instance;

	private Action success;

	private Action fail;

	private const float wait_miliSec = 2000f;

	private const float Pause_miliSec = 410f;

	private bool clicked;

	private bool isBack;

	private DateTime pauseTime;

	private DateTime pauseBackTime;

	private DateTime clickDateTime;

	public static InAppReviewCtr Instance => null;

	public void Init()
	{
	}

	public void InAppReview(Action success, Action fail)
	{
	}

	[IteratorStateMachine(typeof(LKACNIPHCNA))]
	private IEnumerator InAppReviewCor()
	{
		return null;
	}

	private void OnFail()
	{
	}

	private void OnSuccess()
	{
	}

	private void OnFinish()
	{
	}

	[IteratorStateMachine(typeof(CDPDHGAKLBB))]
	private IEnumerator WaitForResult(float restTime)
	{
		return null;
	}

	private void OnApplicationPause(bool pause)
	{
	}

	private void LogError(string value)
	{
	}

	private void LogWarning(string value)
	{
	}

	private void Log(string value)
	{
	}
}
