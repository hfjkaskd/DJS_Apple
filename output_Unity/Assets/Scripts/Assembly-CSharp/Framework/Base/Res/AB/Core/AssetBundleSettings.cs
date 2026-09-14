using System.Collections.Generic;
using UnityEngine;

namespace Framework.Base.Res.AB.Core
{
	public class AssetBundleSettings : ScriptableObject
	{
		[SerializeField]
		private bool formalServer;

		[SerializeField]
		private HLIIKICGAOH simulator;

		[SerializeField]
		private List<AssetBundleModuleInfo> moduleInfo;

		private static AssetBundleSettings settings;

		private static AssetBundleSettingsInfo settingInfo;

		public static bool IsFormalServer => false;

		public static bool NeedCompareToLastVersion { get; private set; }

		public static string AddressResUpdateLink => null;

		public static string RootServerUrl => null;

		public static global::JFDHADOAJPO<AssetBundleModuleInfo> ModuleInfos { get; private set; }

		public static AssetBundleModuleInfo QueryModuleVersion(string InModule)
		{
			return null;
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		public static void Initialize()
		{
		}

		public static void LoadSetting()
		{
		}
	}
}
