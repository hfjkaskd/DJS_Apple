using System;
using System.IO;

namespace OCES.Audio
{
	[Serializable]
	public class MusicTransition : FMMIFBOKDGM, HLCOONGMLLE
	{
		public uint Id { get; set; }

		public int SourceContainerID { get; set; }

		public int DestinationContainerID { get; set; }

		public float FadeOutTime { get; set; }

		public float FadeOutOffset { get; set; }

		public float FadeInTime { get; set; }

		public float FadeInOffset { get; set; }

		public AFHILMODOLO AlignMode { get; set; }

		public FHNEJGHAMPH SyncPoint { get; set; }

		public JMALCJMFBGD SyncSegment { get; set; }

		public void DeSerialize(BinaryReader reader)
		{
		}

		public void Serialize(BinaryWriter writer)
		{
		}
	}
}
