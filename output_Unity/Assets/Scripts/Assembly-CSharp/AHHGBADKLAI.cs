using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;

public static class AHHGBADKLAI
{
	private class NKJIAJKKJKA
	{
		public string url;

		public Action<float, float> progressAction;

		public Action<byte[]> dataLoadedAction;

		public Action failAction;
	}

	private sealed class KNJMMAPBCAP : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public string path;

		public Action<string> complete;

		private string _003CstrContent_003E5__2;

		private UnityWebRequest _003Cw_003E5__3;

		private WWW _003Cw_003E5__4;

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
		public KNJMMAPBCAP(int _003C_003E1__state)
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

	private sealed class DFNJGOIEFIA : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public NKJIAJKKJKA task;

		private UnityWebRequest _003Crequest_003E5__2;

		private float _003CstopTimer_003E5__3;

		private bool _003CtimeOut_003E5__4;

		private ulong _003CcurDownloadedBtytes_003E5__5;

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
		public DFNJGOIEFIA(int _003C_003E1__state)
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

	private const int DefaultCountAtSameTime = 5;

	private static readonly List<NKJIAJKKJKA> downloadings;

	private static readonly List<NKJIAJKKJKA> waittings;

	private const int MaxWaitingTimeWhenProgressIsStuck = 30;

	public static void ReadTxt(string path, Action<string> complete)
	{
	}

	[IteratorStateMachine(typeof(KNJMMAPBCAP))]
	private static IEnumerator ReadTxtI(string path, Action<string> complete)
	{
		return null;
	}

	public static void CreateDirOfFile(string fullPath)
	{
	}

	private static void CheckDownloadQueue()
	{
	}

	private static NKJIAJKKJKA GetTask(string url)
	{
		return null;
	}

	public static void RegistDownloadTask(string url, Action<float, float> progressAction, Action<byte[]> dataLoadedAction, Action failAction, bool immediately)
	{
	}

	[IteratorStateMachine(typeof(DFNJGOIEFIA))]
	private static IEnumerator RealDownloadI(NKJIAJKKJKA task)
	{
		return null;
	}
}
