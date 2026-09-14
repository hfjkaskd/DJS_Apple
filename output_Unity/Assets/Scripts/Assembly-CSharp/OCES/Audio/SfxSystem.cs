using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Audio;

namespace OCES.Audio
{
	public class SfxSystem : MonoBehaviour
	{
		[StructLayout((LayoutKind)3)]
		private struct HLIEJLPPIHF
		{
			public AudioMixer mixer;
		}

		private sealed class HFGIHDMMCIN
		{
			public uint audioId;

			internal bool _003CStop_003Eb__0(NDIOHEGNNAC activeSound)
			{
				return false;
			}
		}

		private sealed class AOPEHJDJDEK
		{
			public uint audioId;

			internal bool _003CSetVolume_003Eb__0(NDIOHEGNNAC activeSound)
			{
				return false;
			}
		}

		private sealed class PBFCKDKDBJN
		{
			public uint audioId;

			internal bool _003CSetPitch_003Eb__0(NDIOHEGNNAC activeSound)
			{
				return false;
			}
		}

		private sealed class DMNEPCBKDHA
		{
			public uint audioId;

			internal bool _003CResetVolume_003Eb__0(NDIOHEGNNAC activeSound)
			{
				return false;
			}
		}

		private sealed class JAINFPPGBPP
		{
			public uint audioId;

			internal bool _003CResetPitch_003Eb__0(NDIOHEGNNAC activeSound)
			{
				return false;
			}
		}

		private sealed class FKDGIAIENMP
		{
			public AudioSource source;

			public Func<bool> _003C_003E9__0;

			internal bool _003CPlayContainerContinuous_003Eb__0()
			{
				return false;
			}
		}

		private sealed class JAJMOBAINBF
		{
			public NDIOHEGNNAC active;

			internal bool _003CRemoveWhenFinished_003Eb__0()
			{
				return false;
			}
		}

		private sealed class KOPEBABJGOA : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public AudioSource source;

			public AudioObject audioObject;

			public NDIOHEGNNAC chainActive;

			public int startIndex;

			public SfxSystem _003C_003E4__this;

			private FKDGIAIENMP _003C_003E8__1;

			private bool _003CisRandom_003E5__2;

			private List<int> _003CremainingPool_003E5__3;

			private Queue<int> _003CrecentPlayed_003E5__4;

			private int _003ClimitRepetition_003E5__5;

			private int _003Cindex_003E5__6;

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
			public KOPEBABJGOA(int _003C_003E1__state)
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

		private sealed class DHCKJLJELCJ : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public NDIOHEGNNAC active;

			private JAJMOBAINBF _003C_003E8__1;

			public SfxSystem _003C_003E4__this;

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
			public DHCKJLJELCJ(int _003C_003E1__state)
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

		private readonly Dictionary<uint, double> m_lastPlayTime;

		private readonly Dictionary<uint, int> m_clipConcurrentCount;

		private readonly List<NDIOHEGNNAC> m_activeSounds;

		private readonly List<NDIOHEGNNAC> m_tempSameObject;

		private readonly List<NDIOHEGNNAC> m_tempSameGroup;

		private readonly List<NDIOHEGNNAC> m_tempLowerPriority;

		private AudioGroupConfig m_groupConfig;

		private AudioMixerGroup m_sfxGroup;

		private AudioMixerGroup m_voiceGroup;

		private AudioMixerGroup m_accentSfxGroup;

		private AFLBDHLOMLA m_pool;

		private MBHIDEFJKGP m_containerSelector;

		private AFHLABPLLIO m_pitchStepResolver;

		private FCDKOIDOEBC m_volumeStepResolver;

		internal Action<NDIOHEGNNAC> OnSoundStarted;

		internal Action<NDIOHEGNNAC> OnSoundStopped;

		private int GetClipCount(uint id)
		{
			return 0;
		}

		private void IncrementClipCount(uint id)
		{
		}

		private void DecrementClipCount(uint id)
		{
		}

		public void Initialize(AudioGroupConfig groups, AudioMixer mixer, AFLBDHLOMLA pool)
		{
		}

		internal void Stop(uint audioId)
		{
		}

		internal void SetVolume(uint audioId, float targetVolume)
		{
		}

		internal void SetPitch(uint audioId, float targetPitch)
		{
		}

		internal void ResetVolume(uint audioId)
		{
		}

		internal void ResetPitch(uint audioId)
		{
		}

		internal void TryPlay(AudioObject audioObject, Action onPlay = null)
		{
		}

		private void PlayNewSound(AudioObject audioObject, float pitch, float volume)
		{
		}

		[IteratorStateMachine(typeof(KOPEBABJGOA))]
		private IEnumerator PlayContainerContinuous(AudioSource source, AudioObject audioObject, NDIOHEGNNAC chainActive, int startIndex)
		{
			return null;
		}

		private static void StartPlayback(AudioSource source, float delay = 0f)
		{
		}

		private bool TryKill(List<NDIOHEGNNAC> candidates, OBHAFELJJIN killMode, string logPrefix)
		{
			return false;
		}

		private NDIOHEGNNAC SelectOldest(List<NDIOHEGNNAC> candidates)
		{
			return null;
		}

		private AudioMixerGroup GetMixerGroup(OCCNJHKFEEE type)
		{
			return null;
		}

		private AudioMixerGroup GetDefaultGroup(OCCNJHKFEEE type)
		{
			return null;
		}

		private bool SetupSource(AudioSource source, NDIOHEGNNAC activeSound, int clipIndex = 0)
		{
			return false;
		}

		private void RegisterActiveSound(NDIOHEGNNAC activeSound, AudioSource audioSource, bool isRegistered = false)
		{
		}

		private void ExecutePlay(NDIOHEGNNAC active, bool isRegistered = false)
		{
		}

		[IteratorStateMachine(typeof(DHCKJLJELCJ))]
		private IEnumerator RemoveWhenFinished(NDIOHEGNNAC active)
		{
			return null;
		}

		private bool PlayAgain(NDIOHEGNNAC active)
		{
			return false;
		}

		private void StopSound(NDIOHEGNNAC active)
		{
		}
	}
}
