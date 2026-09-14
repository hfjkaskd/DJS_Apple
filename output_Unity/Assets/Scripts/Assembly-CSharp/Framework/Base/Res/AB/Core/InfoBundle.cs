using System;

namespace Framework.Base.Res.AB.Core
{
	[Serializable]
	public class InfoBundle
	{
		public string Key;

		public string DownloadKey;

		public int Local;

		public int Ver;

		public int Sver;

		public string Crc;

		public int Size;

		public bool IsLocal => false;

		public InfoBundle(string InKey, string InDownloadKey, int IsLocal, int InVer, int InSver, string InCrc, int InSize)
		{
		}

		public InfoBundle Copy()
		{
			return null;
		}
	}
}
