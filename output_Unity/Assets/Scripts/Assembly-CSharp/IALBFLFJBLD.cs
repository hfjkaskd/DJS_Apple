using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class IALBFLFJBLD
{
	private sealed class POMKDODEPAK : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public FPPAMLCHNKE handle;

		public float duration;

		public IALBFLFJBLD _003C_003E4__this;

		private List<AudioSource> _003Csources_003E5__2;

		private float _003Celapsed_003E5__3;

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
		public POMKDODEPAK(int _003C_003E1__state)
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

	private sealed class BGNEGJPPAIA : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public HLCOONGMLLE transition;

		public uint newContainerId;

		public IALBFLFJBLD _003C_003E4__this;

		public ANCIJFBKBKH syncState;

		public Action onContainerStarted;

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
		public BGNEGJPPAIA(int _003C_003E1__state)
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

	private sealed class OIKGEKHABOE : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public FPPAMLCHNKE handle;

		public float fromVolume;

		public float duration;

		public IALBFLFJBLD _003C_003E4__this;

		private float _003Celapsed_003E5__2;

		private List<AudioSource> _003Csources_003E5__3;

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
		public OIKGEKHABOE(int _003C_003E1__state)
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

	private sealed class JJPFBOOHEFI : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public FPPAMLCHNKE outgoingHandle;

		public HLCOONGMLLE transition;

		public IALBFLFJBLD _003C_003E4__this;

		public float outgoingVolume;

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
		public JJPFBOOHEFI(int _003C_003E1__state)
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

	private readonly CKKIPBAKEMF m_player;

	private readonly MonoBehaviour m_coroutineHost;

	internal FPPAMLCHNKE CurrentHandle { get; private set; }

	public uint CurrentContainerId { get; private set; }

	public float CurrentVolume { get; private set; }

	internal IALBFLFJBLD(CKKIPBAKEMF player, MonoBehaviour coroutineHost)
	{
	}

	private void StartNew(uint containerId, float startVolume)
	{
	}

	public void StopCurrent()
	{
	}

	private void StopHandle(FPPAMLCHNKE handle)
	{
	}

	[IteratorStateMachine(typeof(JJPFBOOHEFI))]
	internal IEnumerator FadeOutBranch(FPPAMLCHNKE outgoingHandle, float outgoingVolume, HLCOONGMLLE transition, ANCIJFBKBKH syncState = null)
	{
		return null;
	}

	[IteratorStateMachine(typeof(BGNEGJPPAIA))]
	internal IEnumerator FadeInBranch(uint newContainerId, HLCOONGMLLE transition, ANCIJFBKBKH syncState = null, Action onContainerStarted = null)
	{
		return null;
	}

	[IteratorStateMachine(typeof(OIKGEKHABOE))]
	private IEnumerator FadeOut(FPPAMLCHNKE handle, float fromVolume, float duration)
	{
		return null;
	}

	[IteratorStateMachine(typeof(POMKDODEPAK))]
	private IEnumerator FadeIn(FPPAMLCHNKE handle, float duration)
	{
		return null;
	}
}
