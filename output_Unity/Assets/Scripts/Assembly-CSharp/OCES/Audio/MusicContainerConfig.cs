using System;
using System.Collections.Generic;
using System.IO;

namespace OCES.Audio
{
	[Serializable]
	public class MusicContainerConfig : FMMIFBOKDGM
	{
		private Dictionary<uint, MusicContainer> m_musicContainerInfos;

		private List<MusicContainer> m_musicContainerInfoList;

		public List<MusicContainer> MusicContainerList()
		{
			return null;
		}

		public void DeSerialize(BinaryReader reader)
		{
		}

		public void Serialize(BinaryWriter writer)
		{
		}

		public MusicContainer QueryById(uint id)
		{
			return null;
		}

		public static int GetBeatsPerBar(string timeSig)
		{
			return 0;
		}

		public void Validate(MusicSegmentConfig segmentConfig)
		{
		}
	}
}
