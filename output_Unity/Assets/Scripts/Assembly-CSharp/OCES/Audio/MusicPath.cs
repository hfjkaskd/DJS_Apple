using System;
using System.IO;

namespace OCES.Audio
{
	[Serializable]
	public class MusicPath : FMMIFBOKDGM, GKDHHICNIBN
	{
		public uint Id { get; set; }

		public string Path { get; set; }

		public uint ContainerId { get; set; }

		public int Priority { get; set; }

		public void DeSerialize(BinaryReader reader)
		{
		}

		public void Serialize(BinaryWriter writer)
		{
		}
	}
}
