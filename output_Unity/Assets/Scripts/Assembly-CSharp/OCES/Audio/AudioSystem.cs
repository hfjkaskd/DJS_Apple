using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;

namespace OCES.Audio
{
	public class AudioSystem : MonoBehaviour
	{
		public enum JFPMLGLMHLB
		{
			Off = 0,
			Log = 1,
			Warning = 2,
			Error = 3
		}

		private sealed class NFKEGLDALLA
		{
			public float startLog;

			public AudioSystem _003C_003E4__this;

			internal float _003CSetLowpass_003Eb__0()
			{
				return 0f;
			}

			internal void _003CSetLowpass_003Eb__1(float x)
			{
			}
		}

		private sealed class IPKCMBCJPPO
		{
			public AudioSystem _003C_003E4__this;

			public uint audioId;

			public JAHEFPDFDBG callbackFlags;

			public Action<uint> callback;

			internal void _003CPlayOnTrigger_003Eb__0(uint id)
			{
			}
		}

		public bool startWithMusic;

		public CNJGPJCNFBL.PKKOCMHNHPJ startMusicWith;

		public JFPMLGLMHLB logLevel;

		internal ResourceLoader ResourceLoader;

		[SerializeField]
		private AudioExtendSettings extendSettings;

		private SfxSystem m_sfxSystem;

		private MusicSystem m_musicSystem;

		private AudioObjectConfig m_audioObjects;

		private AudioGroupConfig m_groups;

		private AudioMixer m_mixer;

		private Tween m_lowpassTween;

		public static AudioSystem Instance { get; private set; }

		public IReadOnlyDictionary<Type, Enum> ActiveStates { get; private set; }

		public event Action<uint> OnBeat
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

		public event Action<uint> OnBar
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

		public event Action<uint> OnGrid
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

		private void OnAwakeComplete()
		{
		}

		public void Play(uint audioId, Action onPlay = null)
		{
		}

		public void Play(AudioObject audioObject, Action onPlay = null)
		{
		}

		public void Play(int audioId)
		{
		}

		[Obsolete("Use Play(uint) instead")]
		public void Play(string audioName)
		{
		}

		public void SetLowpass(bool enable)
		{
		}

		public void SetMusicVolume(float targetVolume)
		{
		}

		public void SetSFXVolume(float targetVolume)
		{
		}

		public void SetMusicState(bool enable)
		{
		}

		public void SetSFXState(bool enable)
		{
		}

		public void SetState<TEnum>(TEnum state) where TEnum : Enum
		{
		}

		public void PlayOnTrigger(uint audioId, JAHEFPDFDBG callbackFlags)
		{
		}

		public void Stop(uint audioId)
		{
		}

		public void SetVolume(uint audioId, float targetVolume)
		{
		}

		public void SetVolume(int audioId, float targetVolume)
		{
		}

		public void SetPitch(uint audioId, float targetPitch)
		{
		}

		public void SetPitch(int audioId, float targetPitch)
		{
		}

		public void ResetVolume(uint audioId)
		{
		}

		public void ResetVolume(int audioId)
		{
		}

		public void ResetPitch(uint audioId)
		{
		}

		public void ResetPitch(int audioId)
		{
		}

		private void Awake()
		{
		}

		private void Start()
		{
		}

		private AudioObject ResolveSwitchContainer(AudioObject switchContainer)
		{
			return null;
		}
	}
}
