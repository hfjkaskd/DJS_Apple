using DG.Tweening;
using UnityEngine;

namespace OCES.Audio
{
	public class AudioExtendSettings : ScriptableObject
	{
		[Header("Local")]
		[Tooltip("Resources 子目录：音频配置文件（.bytes）")]
		public string audioConfigPath;

		[Tooltip("Resources 子目录：音频资源文件（.wav/.ogg）")]
		public string audioResourcePath;

		[Tooltip("Resources 路径：AudioMixer 资产")]
		public string audioMixerPath;

		[Header("AudioMixer Groups")]
		public string sfxGroupPath;

		public string voiceGroupPath;

		public string accentSfxGroupPath;

		public string musicGroupPath;

		public string ambienceGroupPath;

		[Header("Lowpass Filter")]
		public string lowpassParamName;

		public float lowpassEnabledCutoff;

		public float lowpassDisabledCutoff;

		public float lowpassTweenDuration;

		[Header("Audio Import Settings")]
		public AudioCompressionFormat compressionFormat;

		public uint sfxSampleRate;

		public uint musicSampleRate;

		public float musicQuality;

		public float sfxQuality;

		public float decompressThreshold;

		public float streamingThreshold;

		[Header("Fade")]
		public Ease defaultFadeOutEase;

		public Ease defaultFadeInEase;

		public static AudioExtendSettings Instance { get; internal set; }

		public string FullAudioResourcePath => null;

		public string FullAudioConfigPath => null;
	}
}
