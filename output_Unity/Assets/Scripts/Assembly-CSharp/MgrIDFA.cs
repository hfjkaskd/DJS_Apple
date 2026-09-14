using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class MgrIDFA : MonoBehaviour
{
	private sealed class EHNIDHKPLBD : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Action<int, string> action;

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
		public EHNIDHKPLBD(int _003C_003E1__state)
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

	public const string BIND_OBJ_NAME_S = "MgrIDFA";

	private static MgrIDFA instance;

	private const float TIMEOUT_SECONDS = 20f;

	private const float POLL_INTERVAL_SECONDS = 0.25f;

	private const string EMPTY_IDFA = "00000000-0000-0000-0000-000000000000";

	public static MgrIDFA Instance => null;

	public void Wait(Action<int, string> action)
	{
	}

	[IteratorStateMachine(typeof(EHNIDHKPLBD))]
	private IEnumerator WaitForATT(Action<int, string> action)
	{
		return null;
	}
}
