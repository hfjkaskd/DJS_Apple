using UnityEngine;

namespace OCES.Haptic
{
	public class HapticSettings : ScriptableObject
	{
		[Tooltip("触感配置文件")]
		public string hapticConfigPath;

		[Tooltip("触感资源文件（.haptic）")]
		public string hapticResourcePath;

		public static HapticSettings Instance { get; internal set; }
	}
}
