using System;
using System.Collections.Generic;
using System.IO;

namespace OCES.Audio
{
	[Serializable]
	public class AmbienceTransitionConfig : FMMIFBOKDGM
	{
		private Dictionary<uint, AmbienceTransition> m_ambienceTransitionInfos;

		private List<AmbienceTransition> m_ambienceTransitionInfoList;

		public List<AmbienceTransition> AmbienceTransitionList()
		{
			return null;
		}

		public void DeSerialize(BinaryReader reader)
		{
		}

		public void Serialize(BinaryWriter writer)
		{
		}

		public AmbienceTransition QueryById(uint id)
		{
			return null;
		}
	}
}
