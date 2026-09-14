using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AFLBDHLOMLA
{
	private readonly Transform m_root;

	private readonly Queue<GameObject> m_audioSourcePool;

	private int m_counter;

	private AudioMixerGroup m_mixerGroup;

	public AFLBDHLOMLA(Transform root, AudioMixerGroup mixerGroup = null)
	{
	}

	public AudioSource AcquireAudioSource()
	{
		return null;
	}

	public void ReturnToPool(GameObject go)
	{
	}
}
