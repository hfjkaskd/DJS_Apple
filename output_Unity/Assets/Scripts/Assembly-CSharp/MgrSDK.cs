using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class MgrSDK : MonoBehaviour
{
	private sealed class FHPAGLFDOCF
	{
		public Action adokCallback;

		public OGLPGNHCOGO mainMediType;

		public OGLPGNHCOGO subMediType;

		internal void _003CCheckStart_003Eb__0()
		{
		}
	}

	private sealed class HNGNBDECOGE : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Action adokCallback;

		public MgrSDK _003C_003E4__this;

		public Action<int, string> callbackForAdidOrIdfa;

		public Action<bool, string, string> afCallback;

		private FHPAGLFDOCF _003C_003E8__1;

		public bool user_deny;

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
		public HNGNBDECOGE(int _003C_003E1__state)
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

	private sealed class BLCPKCKDDNG : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public bool user_deny;

		public MgrSDK _003C_003E4__this;

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
		public BLCPKCKDDNG(int _003C_003E1__state)
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

	private sealed class HKHNHMOAFGI : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public AFMOOJADBFL mediTypeProvider;

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
		public HKHNHMOAFGI(int _003C_003E1__state)
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

	private sealed class IONIDDADPNP : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public MgrSDK _003C_003E4__this;

		public AFMOOJADBFL mediTypeProvider;

		public bool user_deny;

		public Action<int, string> callbackForAdidOrIdfa;

		public Action<bool, string, string> afCallback;

		public Action adokCallback;

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
		public IONIDDADPNP(int _003C_003E1__state)
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

	private const string BIND_OBJ_NAME_S = "MgrSDK";

	private static MgrSDK instance;

	private bool ump_done;

	private bool before_policy_init_done;

	private bool inited;

	private bool started;

	public static MgrSDK Instance => null;

	public void InitBeforeAgreePolicy(bool user_deny)
	{
	}

	[IteratorStateMachine(typeof(BLCPKCKDDNG))]
	private IEnumerator InitBeforeAgreePolicyRoutine(bool user_deny)
	{
		return null;
	}

	public void StartAfterAgreePolicy(bool user_deny, Action<int, string> callbackForAdidOrIdfa, Action<bool, string, string> afCallback, Action adokCallback, AFMOOJADBFL mediTypeProvider)
	{
	}

	[IteratorStateMachine(typeof(IONIDDADPNP))]
	private IEnumerator StartAfterAgreePolicyRoutine(bool user_deny, Action<int, string> callbackForAdidOrIdfa, Action<bool, string, string> afCallback, Action adokCallback, AFMOOJADBFL mediTypeProvider)
	{
		return null;
	}

	[IteratorStateMachine(typeof(HKHNHMOAFGI))]
	private IEnumerator ResolveMediation(AFMOOJADBFL mediTypeProvider)
	{
		return null;
	}

	[IteratorStateMachine(typeof(HNGNBDECOGE))]
	private IEnumerator CheckStart(bool user_deny, Action<int, string> callbackForAdidOrIdfa, Action<bool, string, string> afCallback, Action adokCallback)
	{
		return null;
	}

	public static void SendSDKLimitPrivacyTag()
	{
	}
}
