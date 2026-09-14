using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using OCES.Audio;
using UnityEngine;

public class PIPOJPDNGJP
{
	private sealed class NMPDFNGLIOK
	{
		public double nextTime;

		internal bool _003CBeatCoroutine_003Eb__0()
		{
			return false;
		}
	}

	private sealed class OGNANPICIHE
	{
		public double nextTime;

		internal bool _003CBarCoroutine_003Eb__0()
		{
			return false;
		}
	}

	private sealed class CPICKJEJKJM
	{
		public double nextTime;

		internal bool _003CGridCoroutine_003Eb__0()
		{
			return false;
		}
	}

	private sealed class FMAFFIMBBJP : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PIPOJPDNGJP _003C_003E4__this;

		private OGNANPICIHE _003C_003E8__1;

		private double _003CsecondsPerBar_003E5__2;

		private long _003Cindex_003E5__3;

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
		public FMAFFIMBBJP(int _003C_003E1__state)
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

	private sealed class AIIMJBJEOED : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PIPOJPDNGJP _003C_003E4__this;

		private NMPDFNGLIOK _003C_003E8__1;

		private long _003Cindex_003E5__2;

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
		public AIIMJBJEOED(int _003C_003E1__state)
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

	private sealed class BEMNJIBMNGN : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PIPOJPDNGJP _003C_003E4__this;

		public bool hasTimeSig;

		public bool hasGrid;

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
		public BEMNJIBMNGN(int _003C_003E1__state)
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

	private sealed class HPMEKCIHDMI : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PIPOJPDNGJP _003C_003E4__this;

		private CPICKJEJKJM _003C_003E8__1;

		private double _003CsecondsPerGrid_003E5__2;

		private long _003Cindex_003E5__3;

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
		public HPMEKCIHDMI(int _003C_003E1__state)
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

	private int m_beatsPerBar;

	private int m_barsPerGrid;

	private double m_startDspTime;

	private double m_secondsPerBeat;

	private Coroutine m_beatCoroutine;

	private Coroutine m_barCoroutine;

	private Coroutine m_gridCoroutine;

	private Coroutine m_delayCoroutine;

	private readonly Action<uint> m_onBeat;

	private readonly Action<uint> m_onBar;

	private readonly Action<uint> m_onGrid;

	private readonly MonoBehaviour m_host;

	private uint m_containerId;

	private bool m_stopped;

	private bool m_blendError;

	internal PIPOJPDNGJP(MonoBehaviour host, Action<uint> onBeat, Action<uint> onBar, Action<uint> onGrid)
	{
	}

	internal void Restart(MusicContainer container, float inheritedBpm, double dspTime, double startOffset = 0.0)
	{
	}

	[IteratorStateMachine(typeof(BEMNJIBMNGN))]
	private IEnumerator DelayedStart(bool hasTimeSig, bool hasGrid)
	{
		return null;
	}

	private void StartCoroutines(bool hasTimeSig, bool hasGrid)
	{
	}

	internal void OnBlendError(MusicContainer container)
	{
	}

	internal void StopAll()
	{
	}

	internal double GetNextDspTime(AFHILMODOLO mode)
	{
		return 0.0;
	}

	[IteratorStateMachine(typeof(AIIMJBJEOED))]
	private IEnumerator BeatCoroutine()
	{
		return null;
	}

	[IteratorStateMachine(typeof(FMAFFIMBBJP))]
	private IEnumerator BarCoroutine()
	{
		return null;
	}

	[IteratorStateMachine(typeof(HPMEKCIHDMI))]
	private IEnumerator GridCoroutine()
	{
		return null;
	}
}
