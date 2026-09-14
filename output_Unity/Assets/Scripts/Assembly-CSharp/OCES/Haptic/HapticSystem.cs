using System;
using Lofelt.NiceVibrations;
using UnityEngine;

namespace OCES.Haptic
{
	public class HapticSystem : MonoBehaviour
	{
		private static class HMNEMFNJGAK
		{
			internal static T Load<T>(string tableName) where T : BAGHAGJNHAE, new()
			{
				return default(T);
			}
		}

		[NonSerialized]
		public bool IsHapticSupported;

		[NonSerialized]
		public bool IsMeetsAdvanceRequirements;

		[SerializeField]
		private HapticSettings hapticSettings;

		internal ResourceLoader ResourceLoader;

		private HapticObjectConfig m_hapticObjects;

		public static HapticSystem Instance { get; private set; }

		public void Play(uint hapticId, bool isDirectCall = true)
		{
		}

		public void Play(HapticObject hapticObject)
		{
		}

		public void Stop(uint? hapticId = null)
		{
		}

		public void EnableHaptic(bool enable)
		{
		}

		private void Awake()
		{
		}

		private static HapticClip GetHapticClip(HapticObject hapticObject)
		{
			return null;
		}
	}
}
