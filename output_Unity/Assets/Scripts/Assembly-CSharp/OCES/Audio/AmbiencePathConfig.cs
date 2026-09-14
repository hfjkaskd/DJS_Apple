using System;
using System.Collections.Generic;
using System.IO;

namespace OCES.Audio
{
	[Serializable]
	public class AmbiencePathConfig : FMMIFBOKDGM
	{
		private Dictionary<uint, AmbiencePath> m_ambiencePathInfos;

		private List<AmbiencePath> m_ambiencePathInfoList;

		public List<AmbiencePath> AmbiencePathList()
		{
			return null;
		}

		public void DeSerialize(BinaryReader reader)
		{
		}

		public void Serialize(BinaryWriter writer)
		{
		}

		public AmbiencePath QueryById(uint id)
		{
			return null;
		}
	}
}
