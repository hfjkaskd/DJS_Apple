using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using OCES.Audio;
using UnityEngine;

internal class OKDMGHGHAME
{
	private sealed class FANBKJNGCJB : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public uint newContainerId;

		public OKDMGHGHAME _003C_003E4__this;

		public AmbienceTransition transition;

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
		public FANBKJNGCJB(int _003C_003E1__state)
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

	private readonly AmbienceTransitionConfig m_transitionConfig;

	private readonly MonoBehaviour m_coroutineHost;

	private readonly IALBFLFJBLD m_fader;

	private readonly List<AmbienceTransition> m_transitionCandidates;

	private FPPAMLCHNKE m_currentHandle;

	private Coroutine m_transitionCoroutine;

	private Coroutine m_currentFadeOutCoroutine;

	private Coroutine m_currentFadeInCoroutine;

	private uint m_currentContainerId;

	internal OKDMGHGHAME(AmbienceTransitionConfig transitionConfig, CKKIPBAKEMF player, MonoBehaviour coroutineHost)
	{
	}

	internal void SwitchTo(uint newContainerId)
	{
	}

	public void Stop()
	{
	}

	[IteratorStateMachine(typeof(FANBKJNGCJB))]
	private IEnumerator DoTransition(uint newContainerId, AmbienceTransition transition)
	{
		return null;
	}

	private AmbienceTransition ResolveTransition(int sourceContainerId, int destinationContainerId)
	{
		return null;
	}
}
