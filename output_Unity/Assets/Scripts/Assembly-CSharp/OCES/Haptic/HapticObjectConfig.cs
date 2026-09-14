using System;
using System.Collections.Generic;
using System.IO;

namespace OCES.Haptic
{
	[Serializable]
	public class HapticObjectConfig : BAGHAGJNHAE
	{
		private Dictionary<uint, HapticObject> m_hapticObjectInfos;

		private List<HapticObject> m_hapticObjectInfoList;

		public List<HapticObject> HapticObjectList()
		{
			return null;
		}

		public void DeSerialize(BinaryReader reader)
		{
		}

		public void Serialize(BinaryWriter writer)
		{
		}

		public HapticObject QueryById(uint id)
		{
			return null;
		}
	}
}
