using System;
using System.Collections.Generic;

namespace CorePlay
{
	[Serializable]
	public class Data
	{
		public bool hasEnterLevel;

		public int mainLevelWinCnt;

		public LevelData mainLevelData;

		public bool isLevelFinished;

		public bool hasShownWillFillTip;

		public bool hasShowFreeRevive;

		public bool hasInitV3;

		public List<int> hadShownNewItemPop;

		public int levelTipNextContentType;

		public int[] levelTipTextSeq;

		public bool hasTreeDownloadSucceededOnce;
	}
}
