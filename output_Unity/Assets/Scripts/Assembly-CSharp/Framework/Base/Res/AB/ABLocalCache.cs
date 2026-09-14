using System;

namespace Framework.Base.Res.AB
{
	[Serializable]
	internal class ABLocalCache
	{
		public string key;

		public int ver;

		public int sver;

		public ABLocalCache()
		{
		}

		public ABLocalCache(string key, int ver, int sver)
		{
		}
	}
}
