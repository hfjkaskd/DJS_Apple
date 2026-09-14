using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine.Networking;

public static class DBEDFLGDGBE
{
	private sealed class NCIMBJECHJA : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public string url;

		public BKAKFNNAINC fail;

		public JODIIIOGGEM success;

		private UnityWebRequest _003Crequest_003E5__2;

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
		public NCIMBJECHJA(int _003C_003E1__state)
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

	private sealed class ADDNNKGOGFH : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public string url;

		public bool useUtf;

		public string jsonPara;

		public BKAKFNNAINC fail;

		public JODIIIOGGEM success;

		private UnityWebRequest _003Crequest_003E5__2;

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
		public ADDNNKGOGFH(int _003C_003E1__state)
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

	private sealed class ODLEDIMFFHM : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public string url;

		public string method;

		public string jsonPara;

		public BKAKFNNAINC fail;

		public JODIIIOGGEM success;

		private UnityWebRequest _003Crequest_003E5__2;

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
		public ODLEDIMFFHM(int _003C_003E1__state)
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

	public static void Post(string url, string jsonPara, JODIIIOGGEM success = null, BKAKFNNAINC fail = null, bool useUtf = false)
	{
	}

	public static void PostWithoutEncrypt(string url, string jsonPara, JODIIIOGGEM success = null, BKAKFNNAINC fail = null, bool useUtf = false)
	{
	}

	[IteratorStateMachine(typeof(ADDNNKGOGFH))]
	private static IEnumerator PostI(string url, string jsonPara, JODIIIOGGEM success, BKAKFNNAINC fail, bool useUtf)
	{
		return null;
	}

	public static void Get(string url, JODIIIOGGEM success = null, BKAKFNNAINC fail = null)
	{
	}

	[IteratorStateMachine(typeof(NCIMBJECHJA))]
	private static IEnumerator GetI(string url, JODIIIOGGEM success, BKAKFNNAINC fail)
	{
		return null;
	}

	public static void Send(string method, string url, string jsonPara, JODIIIOGGEM success = null, BKAKFNNAINC fail = null)
	{
	}

	[IteratorStateMachine(typeof(ODLEDIMFFHM))]
	private static IEnumerator SendI(string method, string url, string jsonPara, JODIIIOGGEM success, BKAKFNNAINC fail)
	{
		return null;
	}

	public static string EncryptWithXORAndBase64(string input)
	{
		return null;
	}
}
