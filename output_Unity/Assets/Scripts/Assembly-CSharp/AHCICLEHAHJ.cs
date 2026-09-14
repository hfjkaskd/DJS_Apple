using System;
using System.Collections.Generic;
using OCES.Audio;

public class AHCICLEHAHJ
{
	private readonly Dictionary<Type, Enum> m_activeStates;

	private readonly MusicPathConfig m_musicPaths;

	private readonly AmbiencePathConfig m_ambiencePaths;

	public uint LastMusicPathId { get; private set; }

	public uint LastAmbiencePathId { get; private set; }

	internal IReadOnlyDictionary<Type, Enum> ActiveStates => null;

	public AHCICLEHAHJ(MusicPathConfig musicPaths, AmbiencePathConfig ambiencePaths)
	{
	}

	public void SetState<TEnum>(TEnum state, out uint musicContainerId, out uint ambienceContainerId) where TEnum : Enum
	{
		musicContainerId = default(uint);
		ambienceContainerId = default(uint);
	}

	private uint MatchBestPath<T>(List<T> paths, out uint matchedPathId) where T : GKDHHICNIBN
	{
		matchedPathId = default(uint);
		return 0u;
	}

	private bool PathMatches(string pathStr)
	{
		return false;
	}
}
