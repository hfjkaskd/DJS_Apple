using System;
using System.IO;

namespace OCES.Audio
{
	[Serializable]
	public class AudioGroup : FMMIFBOKDGM
	{
		public uint Id { get; set; }

		public string Comment { get; set; }

		public ushort GroupThrottleCount { get; set; }

		public OBHAFELJJIN KillMode { get; set; }

		public void DeSerialize(BinaryReader reader)
		{
		}

		public void Serialize(BinaryWriter writer)
		{
		}
	}
}
