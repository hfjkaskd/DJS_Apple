using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using OCES.Audio;
using UnityEngine;
using UnityEngine.Audio;

internal class CKKIPBAKEMF
{
	private sealed class MMHMAOPMIPD
	{
		public CKKIPBAKEMF _003C_003E4__this;

		public int remaining;

		public bool allDone;

		public FPPAMLCHNKE handle;

		public Action _003C_003E9__3;

		internal float _003CPlayBlend_003Eb__0(uint id)
		{
			return 0f;
		}

		internal void _003CPlayBlend_003Eb__3()
		{
		}

		internal bool _003CPlayBlend_003Eb__2()
		{
			return false;
		}
	}

	private sealed class FNKLIFEAFPI
	{
		public bool done;

		public FPPAMLCHNKE parentHandle;

		internal void _003CPlayChildAndWait_003Eb__0()
		{
		}

		internal bool _003CPlayChildAndWait_003Eb__1()
		{
			return false;
		}
	}

	private sealed class FBGEBDGIKHG
	{
		public AudioSource source;

		public FPPAMLCHNKE handle;

		public double effectiveTime;

		internal bool _003CWaitSegmentFinish_003Eb__0()
		{
			return false;
		}

		internal bool _003CWaitSegmentFinish_003Eb__1()
		{
			return false;
		}
	}

	private sealed class PNILFMOAFJF
	{
		public HashSet<uint> history;

		internal bool _003CPickRandomChild_003Eb__0(uint id)
		{
			return false;
		}
	}

	private sealed class HPMHEGKGJLB : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public CKKIPBAKEMF _003C_003E4__this;

		public FPPAMLCHNKE handle;

		public MusicContainer container;

		public float volumeScale;

		public float inheritedBpm;

		private MMHMAOPMIPD _003C_003E8__1;

		private List<FPPAMLCHNKE> _003CchildHandles_003E5__2;

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
		public HPMHEGKGJLB(int _003C_003E1__state)
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

	private sealed class IMENFGCOILL : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public FPPAMLCHNKE parentHandle;

		public CKKIPBAKEMF _003C_003E4__this;

		public uint id;

		public float volumeScale;

		public float inheritedBpm;

		public bool isLoop;

		private FNKLIFEAFPI _003C_003E8__1;

		private FPPAMLCHNKE _003Cchild_003E5__2;

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
		public IMENFGCOILL(int _003C_003E1__state)
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

	private sealed class DONJEEHMHBK : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public MusicContainer container;

		public float inheritedBpm;

		public CKKIPBAKEMF _003C_003E4__this;

		public FPPAMLCHNKE handle;

		public Action onFinished;

		private float _003CeffectiveBpm_003E5__2;

		private int _003CloopsCompleted_003E5__3;

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
		public DONJEEHMHBK(int _003C_003E1__state)
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

	private sealed class CPMJIIAJFHH : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public MusicContainer container;

		public CKKIPBAKEMF _003C_003E4__this;

		public float volumeScale;

		public FPPAMLCHNKE handle;

		public float inheritedBpm;

		public bool isLoop;

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
		public CPMJIIAJFHH(int _003C_003E1__state)
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

	private sealed class NKGPNHPMIMC : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public MusicContainer container;

		public CKKIPBAKEMF _003C_003E4__this;

		public float volumeScale;

		public FPPAMLCHNKE handle;

		public float inheritedBpm;

		public bool isLoop;

		private List<uint> _003Cremaining_003E5__2;

		private uint _003Cchosen_003E5__3;

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
		public NKGPNHPMIMC(int _003C_003E1__state)
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

	private sealed class EANKDJNOMPG : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public MusicContainer container;

		public CKKIPBAKEMF _003C_003E4__this;

		public float volumeScale;

		public FPPAMLCHNKE handle;

		public float inheritedBpm;

		public bool isLoop;

		private int _003Ci_003E5__2;

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
		public EANKDJNOMPG(int _003C_003E1__state)
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

	private sealed class FOILALPOBMA : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public AudioSource source;

		public FPPAMLCHNKE handle;

		public double endOffset;

		private FBGEBDGIKHG _003C_003E8__1;

		public Action onFinished;

		public CKKIPBAKEMF _003C_003E4__this;

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
		public FOILALPOBMA(int _003C_003E1__state)
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

	private readonly MusicContainerConfig m_containerConfig;

	private readonly MusicSegmentConfig m_segmentConfig;

	private readonly AFLBDHLOMLA m_pool;

	private readonly MonoBehaviour m_coroutineHost;

	private AudioMixerGroup m_mixerGroup;

	private readonly Dictionary<uint, int> m_sequenceStepIndex;

	private readonly Dictionary<uint, HashSet<uint>> m_randomHistory;

	internal event Action<MusicContainer, float, double> OnContainerEntered
	{
		[CompilerGenerated]
		add
		{
		}
		[CompilerGenerated]
		remove
		{
		}
	}

	internal event Action<MusicContainer, float, double> OnContainerLooped
	{
		[CompilerGenerated]
		add
		{
		}
		[CompilerGenerated]
		remove
		{
		}
	}

	internal event Action<MusicContainer> OnBlendError
	{
		[CompilerGenerated]
		add
		{
		}
		[CompilerGenerated]
		remove
		{
		}
	}

	public CKKIPBAKEMF(MusicContainerConfig containerConfig, MusicSegmentConfig segmentConfig, AFLBDHLOMLA pool, MonoBehaviour coroutineHost)
	{
	}

	internal FPPAMLCHNKE Play(uint containerId, Action onFinished = null, float inheritedBpm = 0f)
	{
		return null;
	}

	public void Stop(FPPAMLCHNKE handle)
	{
	}

	[IteratorStateMachine(typeof(DONJEEHMHBK))]
	private IEnumerator PlayContainerCoroutine(MusicContainer container, FPPAMLCHNKE handle, float inheritedBpm, Action onFinished)
	{
		return null;
	}

	[IteratorStateMachine(typeof(CPMJIIAJFHH))]
	private IEnumerator PlayContainerOnce(MusicContainer container, float volumeScale, FPPAMLCHNKE handle, float inheritedBpm, bool isLoop = false)
	{
		return null;
	}

	[IteratorStateMachine(typeof(HPMHEGKGJLB))]
	private IEnumerator PlayBlend(MusicContainer container, float volumeScale, FPPAMLCHNKE handle, float inheritedBpm)
	{
		return null;
	}

	[IteratorStateMachine(typeof(EANKDJNOMPG))]
	private IEnumerator PlaySequence(MusicContainer container, float volumeScale, FPPAMLCHNKE handle, float inheritedBpm, bool isLoop = false)
	{
		return null;
	}

	[IteratorStateMachine(typeof(NKGPNHPMIMC))]
	private IEnumerator PlayRandom(MusicContainer container, float volumeScale, FPPAMLCHNKE handle, float inheritedBpm, bool isLoop = false)
	{
		return null;
	}

	[IteratorStateMachine(typeof(IMENFGCOILL))]
	private IEnumerator PlayChildAndWait(uint id, float volumeScale, FPPAMLCHNKE parentHandle, float inheritedBpm, bool isLoop = false)
	{
		return null;
	}

	private FPPAMLCHNKE PlayChild(uint id, float volumeScale, Action onDone, float inheritedBpm, bool isLoop = false)
	{
		return null;
	}

	private FPPAMLCHNKE PlaySegment(uint segmentId, float volumeScale, Action onFinished, bool isLoop = false)
	{
		return null;
	}

	[IteratorStateMachine(typeof(FOILALPOBMA))]
	private IEnumerator WaitSegmentFinish(AudioSource source, FPPAMLCHNKE handle, Action onFinished, double endOffset)
	{
		return null;
	}

	private void ReturnSource(AudioSource source)
	{
	}

	private int GetNextSequenceIndex(MusicContainer container)
	{
		return 0;
	}

	private uint PickRandomChild(MusicContainer container)
	{
		return 0u;
	}
}
