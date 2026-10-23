using System;
using ArrowOut.Runtime;
using UnityEngine;

namespace WZSDK
{
	public class SoundManager : MonoBehaviour
	{
		public static SoundManager Instance;

		private const int PrefOnValue = 0;

		private const int PrefOffValue = 1;

		[Header("Audio Sources")]
		[SerializeField]
		private AudioSource musicSource;

		[SerializeField]
		private AudioSource sfxSource;

		[Header("Legacy Audio Sources")]
		[SerializeField]
		private AudioSource waterShortFall1;

		[SerializeField]
		private AudioSource waterShortFall2;

		[SerializeField]
		private AudioSource waterLongFall1;

		[SerializeField]
		private AudioSource waterLongFall2;

		[SerializeField]
		private AudioSource waterFull;

		[SerializeField]
		private AudioSource bottleSelect;

		[SerializeField]
		private AudioSource addTube;

		[SerializeField]
		private AudioSource clickBtn;

		[SerializeField]
		private AudioSource flyingCoin;

		[SerializeField]
		private AudioSource cat;

		[SerializeField]
		private AudioSource gameWin;

		[SerializeField]
		private AudioSource backgroundMusic;

		[SerializeField]
		private string backgroundMusicAddress = "audio/bgm";

		[SerializeField]
		private AudioSource[] soundList;

		[Header("SFX")]
		[SerializeField]
		private AudioClip uiClickSfxClip;

		[SerializeField]
		private AudioClip arrowEscapeFailedSfx;

		[SerializeField]
		private AudioClip arrowEscapeSuccessSfx;

		[SerializeField]
		private AudioClip levelCompletedSfx;

		[SerializeField]
		private AudioClip levelFailedSfx;

		[SerializeField]
		private AudioClip collectCoinSfx;

		[SerializeField]
		private AudioClip waterFullSfxClip;

		[SerializeField]
		private AudioClip gameWinSfxClip;


		[Header("Volume")]
		[Range(0f, 1f)]
		public float musicVolume;

		[Range(0f, 1f)]
		public float sfxVolume;

		public int musicState;

		public int soundState;

		public int hapticState;

		private void Awake()
		{
			if (Instance != null && Instance != this)
			{
				Destroy(gameObject);
				return;
			}

			Instance = this;

			ResolveAudioSources();
			RegisterFacadeHandlers();
				musicState = LoadToggleState(FacadePlayerPrefExtend.MusicKey,
					FacadePlayerPrefExtend.LegacyMusicKey);
				soundState = LoadToggleState(FacadePlayerPrefExtend.SoundKey,
					FacadePlayerPrefExtend.LegacySoundKey, musicState == 1);
				hapticState = LoadToggleState(FacadePlayerPrefExtend.HapticKey,
					FacadePlayerPrefExtend.LegacyHapticKey);
			ApplyInitialSettings();
			// Mahjong owns background music; preserve only the source reward/UI SFX.
		}

		private void OnDestroy()
		{
			if (Instance != this)
			{
				return;
			}
			UnregisterFacadeHandlers();
			Instance = null;
		}

		public void PlayMusic(AudioClip clip, bool loop = true)
		{
			if (musicState != 1 || musicSource == null || clip == null)
			{
				return;
			}
			if (musicSource.isPlaying && musicSource.clip == clip)
			{
				return;
			}
			musicSource.clip = clip;
			musicSource.loop = loop;
			musicSource.volume = musicVolume;
			musicSource.Play();
		}

		public void StopMusic()
		{
			StopAudioSource(musicSource);
			if (backgroundMusic != musicSource)
			{
				StopAudioSource(backgroundMusic);
			}
		}

		public void PlaySFX(AudioClip clip)
		{
			if (soundState != 1 || clip == null || sfxSource == null)
			{
				return;
			}
			sfxSource.PlayOneShot(clip, sfxVolume);
		}

		public void PlayUIClickSFX()
		{
			if (uiClickSfxClip != null)
			{
				PlaySFX(uiClickSfxClip);
				return;
			}
			PlayLegacySFX(clickBtn);
		}

		public void PlayArrowEscapeFailedSFX()
		{
			PlaySFX(arrowEscapeFailedSfx);
			Vibrate();
		}

		public void PlayArrowEscapeSuccessSFX()
		{
			PlaySFX(arrowEscapeSuccessSfx);
		}

		public void PlayLevelCompletedSFX()
		{
			AudioClip clip = levelCompletedSfx != null
				? levelCompletedSfx
				: gameWinSfxClip;
			if (clip != null)
			{
				PlaySFX(clip);
				return;
			}
			PlayLegacySFX(gameWin);
		}

		public void PlayCollectCoinSFX()
		{
			if (collectCoinSfx != null)
			{
				PlaySFX(collectCoinSfx);
				return;
			}
			PlayLegacySFX(flyingCoin);
		}

		public void PlayLevelFailedSFX()
		{
			if (levelFailedSfx != null)
			{
				PlaySFX(levelFailedSfx);
			}
			else
			{
				PlayLegacySFX(waterFull);
			}
			Vibrate();
		}

		public void PlayWaterFullSFX()
		{
			if (waterFullSfxClip != null)
			{
				PlaySFX(waterFullSfxClip);
				return;
			}
			PlayLegacySFX(waterFull);
		}

		public void PlayGameWinSFX()
		{
			AudioClip clip = gameWinSfxClip != null
				? gameWinSfxClip
				: levelCompletedSfx;
			if (clip != null)
			{
				PlaySFX(clip);
				return;
			}
			PlayLegacySFX(gameWin);
		}

		public void ToogleMusic(bool toggle)
		{
			musicState = toggle ? 1 : 0;
			SaveToggleState(FacadePlayerPrefExtend.MusicKey,
				FacadePlayerPrefExtend.LegacyMusicKey, toggle);
			if (toggle)
			{
				PlayConfiguredMusic();
			}
			else
			{
				StopMusic();
			}
		}

		public void ToogleSound(bool toggle)
		{
			soundState = toggle ? 1 : 0;
			SaveToggleState(FacadePlayerPrefExtend.SoundKey,
				FacadePlayerPrefExtend.LegacySoundKey, toggle);
			ApplySfxVolume();
			if (!toggle)
			{
				StopAllSfxSources();
			}
		}

		public void ToogleAllAudio(bool toggle)
		{
			ToogleMusic(toggle);
			ToogleSound(toggle);
		}

		public void ToogleHaptic(bool toggle)
		{
			hapticState = toggle ? 1 : 0;
			SaveToggleState(FacadePlayerPrefExtend.HapticKey,
				FacadePlayerPrefExtend.LegacyHapticKey, toggle);
		}

		public void Vibrate()
		{
			if (hapticState != 1)
			{
				return;
			}
#if UNITY_ANDROID || UNITY_IOS
			Handheld.Vibrate();
#endif
		}

		private void ResolveAudioSources()
		{
			if (musicSource == null)
			{
				musicSource = backgroundMusic;
			}
			if (backgroundMusic == null)
			{
				backgroundMusic = musicSource;
			}
			if (sfxSource == null)
			{
				AudioSource[] sources = GetComponents<AudioSource>();
				for (int i = 0; i < sources.Length; i++)
				{
					if (sources[i] != musicSource && sources[i] != backgroundMusic)
					{
						sfxSource = sources[i];
						break;
					}
				}
			}
		}

		private void RegisterFacadeHandlers()
		{
			FacadeAudioExtend.PlayBgmHandle += PlayConfiguredMusic;
			FacadeAudioExtend.StopBgmHandle += StopMusic;
			FacadeAudioExtend.PlayEffectHandle += PlayEffect;
			FacadeAudioExtend.PlayVibrateHandle += Vibrate;
			FacadeAudioExtend.SetMusicVolumeHandle += SetMusicVolume;
			FacadeAudioExtend.SetEffectsVolumeHandle += SetEffectsVolume;
			FacadeAudioExtend.SetVibrateHandle += ToogleHaptic;
			FacadeAudioExtend.GetMusicVolumeHandle += GetMusicVolume;
			FacadeAudioExtend.GetEffectsVolumeHandle += GetEffectsVolume;
			FacadeAudioExtend.GetVibrateHandle += GetVibrate;
			FacadeAudioExtend.GetEATypeByStrHandle += GetEATypeByStr;
		}

		private void UnregisterFacadeHandlers()
		{
			FacadeAudioExtend.PlayBgmHandle -= PlayConfiguredMusic;
			FacadeAudioExtend.StopBgmHandle -= StopMusic;
			FacadeAudioExtend.PlayEffectHandle -= PlayEffect;
			FacadeAudioExtend.PlayVibrateHandle -= Vibrate;
			FacadeAudioExtend.SetMusicVolumeHandle -= SetMusicVolume;
			FacadeAudioExtend.SetEffectsVolumeHandle -= SetEffectsVolume;
			FacadeAudioExtend.SetVibrateHandle -= ToogleHaptic;
			FacadeAudioExtend.GetMusicVolumeHandle -= GetMusicVolume;
			FacadeAudioExtend.GetEffectsVolumeHandle -= GetEffectsVolume;
			FacadeAudioExtend.GetVibrateHandle -= GetVibrate;
			FacadeAudioExtend.GetEATypeByStrHandle -= GetEATypeByStr;
		}

		private void PlayConfiguredMusic()
		{
			PlayMusicSource(backgroundMusic != null ? backgroundMusic : musicSource);
		}

		private async void LoadBackgroundMusicAsync()
		{
			AudioSource target = backgroundMusic != null
				? backgroundMusic
				: musicSource;
			if (target == null || target.clip != null ||
				string.IsNullOrWhiteSpace(backgroundMusicAddress))
			{
				return;
			}

			AudioClip clip = await AddressableAssetService.LoadAsync<AudioClip>(
				backgroundMusicAddress);
			if (this == null || target == null || clip == null)
			{
				return;
			}

			target.clip = clip;
			target.loop = true;
			if (musicState == 1)
			{
				PlayConfiguredMusic();
			}
		}

		private void PlayEffect(AudioStCategory category)
		{
			switch (category)
			{
				case AudioStCategory.EBgm:
					PlayConfiguredMusic();
					break;
				case AudioStCategory.EButton:
				case AudioStCategory.EClick2:
					PlayUIClickSFX();
					break;
				case AudioStCategory.ESpoolClick:
					PlayLegacySFX(bottleSelect, uiClickSfxClip);
					break;
				case AudioStCategory.EUnlock:
					PlayLegacySFX(addTube, arrowEscapeSuccessSfx);
					break;
				case AudioStCategory.EChangeTarget:
				case AudioStCategory.EMerge_1:
					PlayLegacySFX(waterShortFall1, arrowEscapeSuccessSfx);
					break;
				case AudioStCategory.EChangeTarget2:
				case AudioStCategory.EMerge_2:
					PlayLegacySFX(waterShortFall2, arrowEscapeSuccessSfx);
					break;
				case AudioStCategory.ESettlement:
				case AudioStCategory.ELevelComplete:
				case AudioStCategory.EPopRewad:
					PlayLevelCompletedSFX();
					break;
				case AudioStCategory.EMerge_3:
				case AudioStCategory.ELuckySpin:
					PlayLegacySFX(waterLongFall1, arrowEscapeSuccessSfx);
					break;
				case AudioStCategory.EMerge_4:
					PlayLegacySFX(waterLongFall2, arrowEscapeSuccessSfx);
					break;
				case AudioStCategory.ESaoBa:
					PlayLegacySFX(cat, uiClickSfxClip);
					break;
				case AudioStCategory.EBoardBreak:
				case AudioStCategory.ELevelFailed:
					PlayLevelFailedSFX();
					break;
				case AudioStCategory.EFlyMoneyTip:
					PlayCollectCoinSFX();
					break;
				case AudioStCategory.ELuckySpin2:
					PlayLegacySFX(waterLongFall2, collectCoinSfx);
					break;
				case AudioStCategory.EMerge_5:
					PlayWaterFullSFX();
					break;
			}
		}

		private void PlayMusicSource(AudioSource source)
		{
			if (musicState != 1 || source == null)
			{
				return;
			}
			source.loop = true;
			source.volume = musicVolume;
			if (!source.isPlaying)
			{
				source.Play();
			}
		}

		private void PlayLegacySFX(AudioSource source, AudioClip fallbackClip = null)
		{
			if (soundState != 1)
			{
				return;
			}
			if (source != null)
			{
				source.volume = sfxVolume;
				source.Play();
				return;
			}
			PlaySFX(fallbackClip);
		}

		private void StopAudioSource(AudioSource source)
		{
			if (source != null)
			{
				source.Stop();
			}
		}

		private void StopAllSfxSources()
		{
			StopAudioSource(sfxSource);
			StopAudioSource(waterShortFall1);
			StopAudioSource(waterShortFall2);
			StopAudioSource(waterLongFall1);
			StopAudioSource(waterLongFall2);
			StopAudioSource(waterFull);
			StopAudioSource(bottleSelect);
			StopAudioSource(addTube);
			StopAudioSource(clickBtn);
			StopAudioSource(flyingCoin);
			StopAudioSource(cat);
			StopAudioSource(gameWin);
			if (soundList != null)
			{
				for (int i = 0; i < soundList.Length; i++)
				{
					StopAudioSource(soundList[i]);
				}
			}
		}

		private void SetMusicVolume(float volume)
		{
			musicVolume = Mathf.Clamp01(volume);
			ApplyMusicVolume();
		}

		private void SetEffectsVolume(float volume)
		{
			sfxVolume = Mathf.Clamp01(volume);
			ApplySfxVolume();
		}

		private float GetMusicVolume()
		{
			return musicVolume;
		}

		private float GetEffectsVolume()
		{
			return sfxVolume;
		}

		private bool GetVibrate()
		{
			return hapticState == 1;
		}

		private AudioStCategory GetEATypeByStr(string value)
		{
			int numericValue;
			if (int.TryParse(value, out numericValue) &&
				numericValue >= (int)AudioStCategory.EBgm &&
				numericValue <= (int)AudioStCategory.EMerge_5)
			{
				return (AudioStCategory)numericValue;
			}

			switch ((value ?? string.Empty).Trim().ToUpperInvariant())
			{
				case "EBGM": return AudioStCategory.EBgm;
				case "EBUTTON": return AudioStCategory.EButton;
				case "ESPOOLCLICK": return AudioStCategory.ESpoolClick;
				case "EUNLOCK": return AudioStCategory.EUnlock;
				case "ECHANGETARGET": return AudioStCategory.EChangeTarget;
				case "ECHANGETARGET2": return AudioStCategory.EChangeTarget2;
				case "ESETTLEMENT": return AudioStCategory.ESettlement;
				case "ECLICK2": return AudioStCategory.EClick2;
				case "EMERGE_1": return AudioStCategory.EMerge_1;
				case "EMERGE_2": return AudioStCategory.EMerge_2;
				case "EMERGE_3": return AudioStCategory.EMerge_3;
				case "EMERGE_4": return AudioStCategory.EMerge_4;
				case "ESAOBA": return AudioStCategory.ESaoBa;
				case "EBOARDBREAK": return AudioStCategory.EBoardBreak;
				case "ELEVELCOMPLETE": return AudioStCategory.ELevelComplete;
				case "ELEVELFAILED": return AudioStCategory.ELevelFailed;
				case "EPOPREWAD": return AudioStCategory.EPopRewad;
				case "EFLYMONEYTIP": return AudioStCategory.EFlyMoneyTip;
				case "ELUCKYSPIN": return AudioStCategory.ELuckySpin;
				case "ELUCKYSPIN2": return AudioStCategory.ELuckySpin2;
				case "EMERGE_5": return AudioStCategory.EMerge_5;
				default: return AudioStCategory.EButton;
			}
		}

		private void ApplyInitialSettings()
		{
			ApplyMusicVolume();
			ApplySfxVolume();
			if (musicState == 1)
			{
				PlayConfiguredMusic();
			}
			else
			{
				StopMusic();
			}
		}

		private void ApplyMusicVolume()
		{
			float volume = musicState == 1 ? musicVolume : 0f;
			if (musicSource != null)
			{
				musicSource.volume = volume;
			}
			if (backgroundMusic != null)
			{
				backgroundMusic.volume = volume;
			}
		}

		private void ApplySfxVolume()
		{
			float volume = soundState == 1 ? sfxVolume : 0f;
			SetSourceVolume(sfxSource, volume);
			SetSourceVolume(waterShortFall1, volume);
			SetSourceVolume(waterShortFall2, volume);
			SetSourceVolume(waterLongFall1, volume);
			SetSourceVolume(waterLongFall2, volume);
			SetSourceVolume(waterFull, volume);
			SetSourceVolume(bottleSelect, volume);
			SetSourceVolume(addTube, volume);
			SetSourceVolume(clickBtn, volume);
			SetSourceVolume(flyingCoin, volume);
			SetSourceVolume(cat, volume);
			SetSourceVolume(gameWin, volume);
			if (soundList != null)
			{
				for (int i = 0; i < soundList.Length; i++)
				{
					SetSourceVolume(soundList[i], volume);
				}
			}
		}

		private static void SetSourceVolume(AudioSource source, float volume)
		{
			if (source != null)
			{
				source.volume = volume;
			}
		}

		private int LoadToggleState(string key, string legacyKey, bool defaultOn = true)
		{
			string sourceKey = key;
			if (!PlayerPrefs.HasKey(key) && !string.IsNullOrEmpty(legacyKey) &&
				legacyKey != key && PlayerPrefs.HasKey(legacyKey))
			{
				sourceKey = legacyKey;
			}
			bool isOn = PlayerPrefs.HasKey(sourceKey)
				? PlayerPrefs.GetInt(sourceKey, defaultOn ? PrefOnValue : PrefOffValue) == PrefOnValue
				: defaultOn;
			SaveToggleState(key, legacyKey, isOn);
			return isOn ? 1 : 0;
		}

		private void SaveToggleState(string key, string legacyKey, bool isOn)
		{
			int value = isOn ? PrefOnValue : PrefOffValue;
			PlayerPrefs.SetInt(key, value);
			if (!string.IsNullOrEmpty(legacyKey) && legacyKey != key)
			{
				PlayerPrefs.SetInt(legacyKey, value);
			}
			PlayerPrefs.Save();
		}
	}
}
