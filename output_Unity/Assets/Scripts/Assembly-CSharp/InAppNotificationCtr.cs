using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class InAppNotificationCtr : MonoBehaviour
{
	private sealed class KBKACOONIPE : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public InAppNotificationCtr _003C_003E4__this;

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
		public KBKACOONIPE(int _003C_003E1__state)
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

	private sealed class BHNJDBNEJKB : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public InAppNotificationCtr _003C_003E4__this;

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
		public BHNJDBNEJKB(int _003C_003E1__state)
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

	private static InAppNotificationCtr instance;

	private Action success;

	private Action fail;

	private const float wait_miliSec = 2000f;

	private const float Pause_miliSec = 400f;

	private bool clicked;

	private bool isBack;

	private DateTime pauseTime;

	private DateTime pauseBackTime;

	private DateTime clickDateTime;

	public static InAppNotificationCtr Instance => null;

	public void Init()
	{
	}

	public void InAppNotification(Action success, Action fail)
	{
	}

	[IteratorStateMachine(typeof(KBKACOONIPE))]
	private IEnumerator InAppNotificationCor()
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

	[IteratorStateMachine(typeof(BHNJDBNEJKB))]
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
