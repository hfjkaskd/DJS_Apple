using System;
using System.Collections.Generic;
using System.IO;

namespace OCES.Audio
{
	[Serializable]
	public class MusicPathConfig : FMMIFBOKDGM
	{
		private Dictionary<uint, MusicPath> m_musicPathInfos;

		private List<MusicPath> m_musicPathInfoList;

		public List<MusicPath> MusicPathList()
		{
			return null;
		}

		public void DeSerialize(BinaryReader reader)
		{
		}

		public void Serialize(BinaryWriter writer)
		{
		}

		public MusicPath QueryById(uint id)
		{
			return null;
		}
	}
}
