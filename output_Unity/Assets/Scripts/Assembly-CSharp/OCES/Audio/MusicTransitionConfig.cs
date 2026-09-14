using System;
using System.Collections.Generic;
using System.IO;

namespace OCES.Audio
{
	[Serializable]
	public class MusicTransitionConfig : FMMIFBOKDGM
	{
		private Dictionary<uint, MusicTransition> m_musicTransitionInfos;

		private List<MusicTransition> m_musicTransitionInfoList;

		public List<MusicTransition> MusicTransitionList()
		{
			return null;
		}

		public void DeSerialize(BinaryReader reader)
		{
		}

		public void Serialize(BinaryWriter writer)
		{
		}

		public MusicTransition QueryById(uint id)
		{
			return null;
		}
	}
}
