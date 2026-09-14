using System;
using System.Collections.Generic;
using System.IO;

namespace OCES.Audio
{
	[Serializable]
	public class MusicContainer : FMMIFBOKDGM
	{
		public uint Id { get; set; }

		public List<uint> Segments { get; set; }

		public CPMIHIDFCHA ContainerType { get; set; }

		public bool ContainerPlayMode { get; set; }

		public ushort StrategyParam { get; set; }

		public short LoopCount { get; set; }

		public float Bpm { get; set; }

		public string TimeSig { get; set; }

		public int Grid { get; set; }

		public void DeSerialize(BinaryReader reader)
		{
		}

		public void Serialize(BinaryWriter writer)
		{
		}
	}
}
