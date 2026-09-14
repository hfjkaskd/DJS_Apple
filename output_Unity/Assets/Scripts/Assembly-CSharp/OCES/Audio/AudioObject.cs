using System;
using System.Collections.Generic;
using System.IO;

namespace OCES.Audio
{
	[Serializable]
	public class AudioObject : FMMIFBOKDGM
	{
		public uint Id { get; set; }

		public string CueName { get; set; }

		public List<string> Name { get; set; }

		public string Comment { get; set; }

		public float InitialDelay { get; set; }

		public short LoopCount { get; set; }

		public OCCNJHKFEEE MixingType { get; set; }

		public uint Group { get; set; }

		public ushort ThrottleCount { get; set; }

		public ushort MinInterval { get; set; }

		public byte Priority { get; set; }

		public OBHAFELJJIN KillMode { get; set; }

		public uint PitchStepThreshold { get; set; }

		public sbyte PitchStep { get; set; }

		public byte PitchStepLimit { get; set; }

		public uint Haptic { get; set; }

		public CPMIHIDFCHA ContainerType { get; set; }

		public bool ContainerPlayMode { get; set; }

		public byte LimitRepetition { get; set; }

		public bool RandomType { get; set; }

		public uint VolumeStepThreshold { get; set; }

		public int Volume { get; set; }

		public int VolumeStep { get; set; }

		public uint SwitchGroupId { get; set; }

		public uint DefaultSwitchId { get; set; }

		public void DeSerialize(BinaryReader reader)
		{
		}

		public void Serialize(BinaryWriter writer)
		{
		}
	}
}
