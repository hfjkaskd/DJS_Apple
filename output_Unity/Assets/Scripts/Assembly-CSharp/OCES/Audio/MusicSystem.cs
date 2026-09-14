using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace OCES.Audio
{
	internal class MusicSystem : MonoBehaviour
	{
		private AHCICLEHAHJ m_stateRouter;

		private AOPIEJBCGEH m_musicChannel;

		private OKDMGHGHAME m_ambienceChannel;

		internal IReadOnlyDictionary<Type, Enum> ActiveStates => null;

		internal event Action<uint> OnBeat
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

		internal event Action<uint> OnBar
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

		internal event Action<uint> OnGrid
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

		internal void Initialize(MusicSegmentConfig segments, MusicContainerConfig containers, MusicPathConfig musicPaths, AmbiencePathConfig ambiencePaths, MusicTransitionConfig musicTransitions, AmbienceTransitionConfig ambienceTransitions, AFLBDHLOMLA musicPool, AFLBDHLOMLA ambiencePool)
		{
		}

		internal void OnStateChanged<TEnum>(TEnum state) where TEnum : Enum
		{
		}
	}
}
