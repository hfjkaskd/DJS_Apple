using System.Collections.Generic;
using OCES.Audio;

public class FCDKOIDOEBC
{
	private readonly Dictionary<uint, int> m_volumeStepCounts;

	private readonly Dictionary<uint, double> m_volumeStepLastTime;

	internal float ResolveVolume(AudioObject audioObject, double time)
	{
		return 0f;
	}
}
