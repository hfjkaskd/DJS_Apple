using System;
using System.IO;

namespace OCES.Audio
{
	[Serializable]
	public class MusicSegment : FMMIFBOKDGM
	{
		public uint Id { get; set; }

		public string Name { get; set; }

		public double StartOffset { get; set; }

		public double EndOffset { get; set; }

		public bool IsOffBeat { get; set; }

		public void DeSerialize(BinaryReader reader)
		{
		}

		public void Serialize(BinaryWriter writer)
		{
		}
	}
}
