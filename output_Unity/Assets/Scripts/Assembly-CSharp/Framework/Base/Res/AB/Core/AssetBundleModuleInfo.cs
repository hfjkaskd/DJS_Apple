using System;
using UnityEngine;

namespace Framework.Base.Res.AB.Core
{
	[Serializable]
	public class AssetBundleModuleInfo
	{
		[SerializeField]
		public string ModuleName;

		[SerializeField]
		public int Version;

		[SerializeField]
		public int RMListVersion;

		[SerializeField]
		public int SubVersion;
	}
}
