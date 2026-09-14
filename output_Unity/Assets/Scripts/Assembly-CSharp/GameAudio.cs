using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// 简化音频门面：替代原 OCES 中间件。
/// DLMJOHCOJKN 的静态字段在 Init 时通过反射赋予唯一 ID，并登记 ID→音频文件名。
/// </summary>
public static class GameAudio
{
	private static GameObject root;

	private static AudioSource musicSource;

	private static AudioSource ambienceSource;

	private static readonly List<AudioSource> sfxSources = new List<AudioSource>();

	private static readonly Dictionary<uint, string> idToClip = new Dictionary<uint, string>();

	private static readonly Dictionary<string, AudioClip> clipCache = new Dictionary<string, AudioClip>();

	private static AudioMixerGroup musicGroup;

	private static AudioMixerGroup sfxGroup;

	private static AudioMixerGroup ambienceGroup;

	private static AudioMixerGroup voiceGroup;

	private static bool inited;

	public static bool MusicEnabled { get; private set; } = true;

	public static bool SfxEnabled { get; private set; } = true;

	public static void Init()
	{
		if (inited)
		{
			return;
		}
		inited = true;
		root = new GameObject("GameAudio");
		Object.DontDestroyOnLoad(root);

		AudioMixer mixer = GameRes.Load<AudioMixer>("res/local/sound/Master");
		if (mixer != null)
		{
			musicGroup = FindGroup(mixer, "Music");
			sfxGroup = FindGroup(mixer, "Regular") ?? FindGroup(mixer, "SFX");
			ambienceGroup = FindGroup(mixer, "Ambience");
			voiceGroup = FindGroup(mixer, "Voice");
		}

		musicSource = root.AddComponent<AudioSource>();
		musicSource.loop = true;
		musicSource.outputAudioMixerGroup = musicGroup;
		musicSource.volume = 0.7f;

		ambienceSource = root.AddComponent<AudioSource>();
		ambienceSource.loop = true;
		ambienceSource.outputAudioMixerGroup = ambienceGroup;
		ambienceSource.volume = 0.8f;

		RegisterEventIds();
	}

	private static AudioMixerGroup FindGroup(AudioMixer mixer, string name)
	{
		AudioMixerGroup[] groups = mixer.FindMatchingGroups(name);
		return groups != null && groups.Length > 0 ? groups[0] : null;
	}

	private static void RegisterEventIds()
	{
		uint id = 1u;
		FieldInfo[] fields = typeof(DLMJOHCOJKN).GetFields(BindingFlags.Public | BindingFlags.Static);
		foreach (FieldInfo f in fields)
		{
			string name = f.Name;
			int firstUnderscore = name.IndexOf('_');
			string clip = firstUnderscore >= 0 ? name.Substring(firstUnderscore + 1) : name;
			f.SetValue(null, id);
			idToClip[id] = clip;
			id++;
		}
	}

	private static AudioClip GetClip(string clipName)
	{
		if (string.IsNullOrEmpty(clipName))
		{
			return null;
		}
		if (clipCache.TryGetValue(clipName, out AudioClip cached))
		{
			return cached;
		}
		AudioClip clip = GameRes.LoadAudio(clipName);
		clipCache[clipName] = clip;
		return clip;
	}

	private static AudioSource GetFreeSfxSource()
	{
		for (int i = 0; i < sfxSources.Count; i++)
		{
			if (!sfxSources[i].isPlaying)
			{
				return sfxSources[i];
			}
		}
		AudioSource source = root.AddComponent<AudioSource>();
		source.playOnAwake = false;
		sfxSources.Add(source);
		return source;
	}

	public static void Play(uint eventId)
	{
		if (eventId == 0 || !idToClip.TryGetValue(eventId, out string clipName))
		{
			return;
		}
		Play(clipName);
	}

	public static void Play(string clipName)
	{
		if (!inited || !SfxEnabled)
		{
			return;
		}
		AudioClip clip = GetClip(clipName);
		if (clip == null)
		{
			return;
		}
		AudioSource source = GetFreeSfxSource();
		source.outputAudioMixerGroup = clipName.StartsWith("voice_") ? voiceGroup : sfxGroup;
		source.pitch = 1f;
		source.PlayOneShot(clip);
	}

	/// <summary>连续合成音调：三消游戏中的连击音高递增。semitones 为升高的半音数。</summary>
	public static void PlayPitched(string clipName, int semitones)
	{
		if (!inited || !SfxEnabled)
		{
			return;
		}
		AudioClip clip = GetClip(clipName);
		if (clip == null)
		{
			return;
		}
		AudioSource source = GetFreeSfxSource();
		source.outputAudioMixerGroup = sfxGroup;
		source.pitch = Mathf.Pow(2f, semitones / 12f);
		source.PlayOneShot(clip);
	}

	public static void PlayMusic(string clipName = "music")
	{
		if (!inited)
		{
			return;
		}
		AudioClip clip = GetClip(clipName);
		if (clip == null || (musicSource.clip == clip && musicSource.isPlaying))
		{
			return;
		}
		musicSource.clip = clip;
		if (MusicEnabled)
		{
			musicSource.Play();
		}
	}

	public static void StopMusic()
	{
		if (inited)
		{
			musicSource.Stop();
		}
	}

	public static void PlayAmbience(string clipName)
	{
		if (!inited)
		{
			return;
		}
		AudioClip clip = GetClip(clipName);
		if (clip == null || (ambienceSource.clip == clip && ambienceSource.isPlaying))
		{
			return;
		}
		ambienceSource.clip = clip;
		if (MusicEnabled)
		{
			ambienceSource.Play();
		}
	}

	public static void StopAmbience()
	{
		if (inited)
		{
			ambienceSource.Stop();
		}
	}

	public static void SetMusicEnabled(bool enabled)
	{
		MusicEnabled = enabled;
		if (!inited)
		{
			return;
		}
		if (enabled)
		{
			if (musicSource.clip != null && !musicSource.isPlaying)
			{
				musicSource.Play();
			}
			if (ambienceSource.clip != null && !ambienceSource.isPlaying)
			{
				ambienceSource.Play();
			}
		}
		else
		{
			musicSource.Pause();
			ambienceSource.Pause();
		}
	}

	public static void SetSfxEnabled(bool enabled)
	{
		SfxEnabled = enabled;
	}
}
