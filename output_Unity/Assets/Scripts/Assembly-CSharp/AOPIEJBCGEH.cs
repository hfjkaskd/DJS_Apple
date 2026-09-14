using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using OCES.Audio;
using UnityEngine;

internal class AOPIEJBCGEH
{
	private sealed class OKAPBLMEAAH
	{
		public AOPIEJBCGEH _003C_003E4__this;

		public uint newContainerId;

		public ANCIJFBKBKH syncState;

		public MusicTransition transition;

		internal void _003CDoTransition_003Eb__0()
		{
		}
	}

	private sealed class KFCAMNDEEBN
	{
		public double target;

		internal bool _003CWaitForAlignment_003Eb__0()
		{
			return false;
		}
	}

	private sealed class AGDGIOJHBMP : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public AOPIEJBCGEH _003C_003E4__this;

		public uint newContainerId;

		public MusicTransition transition;

		private OKAPBLMEAAH _003C_003E8__1;

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
		public AGDGIOJHBMP(int _003C_003E1__state)
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

	private sealed class POLIPKNHHFE : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public AFHILMODOLO mode;

		public MusicContainer container;

		public AOPIEJBCGEH _003C_003E4__this;

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
		public POLIPKNHHFE(int _003C_003E1__state)
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

	private readonly MusicTransitionConfig m_transitionConfig;

	private readonly MusicSegmentConfig m_segmentConfig;

	private readonly MonoBehaviour m_coroutineHost;

	private readonly IALBFLFJBLD m_fader;

	private readonly PIPOJPDNGJP m_beatClock;

	private readonly List<MusicTransition> m_transitionCandidates;

	private Coroutine m_currentFadeInCoroutine;

	private Coroutine m_currentFadeOutCoroutine;

	private MusicContainer m_currentContainer;

	private Coroutine m_transitionCoroutine;

	internal AOPIEJBCGEH(MusicContainerConfig containerConfig, MusicSegmentConfig segmentConfig, MusicTransitionConfig transitionConfig, CKKIPBAKEMF player, MonoBehaviour coroutineHost, Action<uint> onBeat, Action<uint> onBar, Action<uint> onGrid)
	{
	}

	internal void SwitchTo(uint newContainerId)
	{
	}

	internal void Stop()
	{
	}

	[IteratorStateMachine(typeof(AGDGIOJHBMP))]
	private IEnumerator DoTransition(uint newContainerId, MusicTransition transition)
	{
		return null;
	}

	[IteratorStateMachine(typeof(POLIPKNHHFE))]
	private IEnumerator WaitForAlignment(AFHILMODOLO mode, MusicContainer container)
	{
		return null;
	}

	private MusicTransition ResolveTransition(int sourceContainerId, int destinationContainerId)
	{
		return null;
	}

	private double GetEffectiveStartOffset(MusicContainer container)
	{
		return 0.0;
	}

	private void OnContainerLooped(MusicContainer container, float bpm, double dspTime)
	{
	}
}
