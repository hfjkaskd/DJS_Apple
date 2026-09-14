using System;
using System.Collections.Generic;
using System.IO;

namespace OCES.Audio
{
	[Serializable]
	public class AudioObjectConfig : FMMIFBOKDGM
	{
		private Dictionary<uint, AudioObject> m_audioObjectInfos;

		private List<AudioObject> m_audioObjectInfoList;

		private Dictionary<uint, Dictionary<int, uint>> m_switchMapping;

		public List<AudioObject> AudioObjectList()
		{
			return null;
		}

		public void DeSerialize(BinaryReader reader)
		{
		}

		public void Serialize(BinaryWriter writer)
		{
		}

		public AudioObject QueryById(uint id)
		{
			return null;
		}

		internal void PreParseSwitchMappings()
		{
		}

		public AudioObject GetMappingResult(uint switchContainerId, Enum enumState)
		{
			return null;
		}

		private Dictionary<int, uint> ParseSwitchMapping(AudioObject switchContainer)
		{
			return null;
		}

		internal AudioObject GetDefaultSwitchOrFallback(AudioObject switchContainer)
		{
			return null;
		}
	}
}
