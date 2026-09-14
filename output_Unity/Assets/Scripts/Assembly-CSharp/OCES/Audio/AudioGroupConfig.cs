using System;
using System.Collections.Generic;
using System.IO;

namespace OCES.Audio
{
	[Serializable]
	public class AudioGroupConfig : FMMIFBOKDGM
	{
		private Dictionary<uint, AudioGroup> m_audioGroupInfos;

		private List<AudioGroup> m_audioGroupInfoList;

		public List<AudioGroup> AudioGroupList()
		{
			return null;
		}

		public void DeSerialize(BinaryReader reader)
		{
		}

		public void Serialize(BinaryWriter writer)
		{
		}

		public AudioGroup QueryById(uint id)
		{
			return null;
		}
	}
}
