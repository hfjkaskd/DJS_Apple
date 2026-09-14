using System;
using System.Collections.Generic;

namespace Framework.Base.Res.AB.Core
{
	[Serializable]
	public class RemoteAssetsListConfig
	{
		public int ver;

		public int sver;

		public string headUrl;

		public List<InfoBundle> bundleList;

		public RemoteAssetsListConfig()
		{
		}

		public RemoteAssetsListConfig(string headUrl, int version, int sversion)
		{
		}
	}
}
