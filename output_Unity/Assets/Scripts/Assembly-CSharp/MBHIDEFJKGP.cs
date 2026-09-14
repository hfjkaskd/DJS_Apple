using System.Collections.Generic;
using OCES.Audio;

public class MBHIDEFJKGP
{
	private sealed class MJHOLFJKCBC
	{
		public Queue<int> recent;

		public HashSet<int> history;

		internal bool _003CPickShuffleIndex_003Eb__2(int i)
		{
			return false;
		}

		internal bool _003CPickShuffleIndex_003Eb__0(int i)
		{
			return false;
		}

		internal bool _003CPickShuffleIndex_003Eb__1(int i)
		{
			return false;
		}
	}

	private sealed class JLCHOGFEJBP
	{
		public Queue<int> recentPlayed;

		internal bool _003CPickNextRandomIndex_003Eb__0(int i)
		{
			return false;
		}
	}

	private readonly Dictionary<uint, HashSet<int>> m_randomPlayedHistories;

	private readonly Dictionary<uint, Queue<int>> m_randomRecentQueues;

	private readonly Dictionary<uint, int> m_sequenceNextIndex;

	public int PickShuffleIndex(AudioObject audioObject)
	{
		return 0;
	}

	public int PickNextRandomIndex(AudioObject audioObject, List<int> remainingPool, Queue<int> recentPlayed, int limitRepetition)
	{
		return 0;
	}

	public int GetNextSequenceIndex(AudioObject audioObject)
	{
		return 0;
	}

	public void ResetHistory(uint id)
	{
	}

	public int GetHistoryCount(uint id)
	{
		return 0;
	}
}
