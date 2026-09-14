using System;
using System.IO;

namespace OCES.Haptic
{
	[Serializable]
	public class HapticObject : BAGHAGJNHAE
	{
		public uint Id { get; set; }

		public DICHDJBKCPD Type { get; set; }

		public float Amplitude { get; set; }

		public float Frequency { get; set; }

		public float Duration { get; set; }

		public string Payload { get; set; }

		public string FallbackPreset { get; set; }

		public void DeSerialize(BinaryReader reader)
		{
		}

		public void Serialize(BinaryWriter writer)
		{
		}
	}
}
