using System;
using System.Collections.Generic;
using System.IO;

namespace OCES.Audio
{
	[Serializable]
	public class MusicSegmentConfig : FMMIFBOKDGM
	{
		private Dictionary<uint, MusicSegment> m_musicSegmentInfos;

		private List<MusicSegment> m_musicSegmentInfoList;

		public List<MusicSegment> MusicSegmentList()
		{
			return null;
		}

		public void DeSerialize(BinaryReader reader)
		{
		}

		public void Serialize(BinaryWriter writer)
		{
		}

		public MusicSegment QueryById(uint id)
		{
			return null;
		}

		public void Validate()
		{
		}
	}
}
