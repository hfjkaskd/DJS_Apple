using System.Collections.Generic;
using UnityEngine;

internal class FPPAMLCHNKE
{
	internal Coroutine Coroutine;

	internal bool Cancelled;

	internal float TargetVolume;

	internal List<AudioSource> ActiveSources;

	internal List<FPPAMLCHNKE> ChildHandles;

	internal void CollectActiveSources(List<AudioSource> result)
	{
	}

	internal AudioSource GetFirstLeafSource()
	{
		return null;
	}
}
