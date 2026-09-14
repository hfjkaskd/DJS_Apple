using System;

namespace Project.Logic
{
	[Serializable]
	public class AppServiceRecordData
	{
		public int installVer;

		public int prevVer;

		public int ver;

		public int installUtcTime;

		public int lastPurchasedUtcTime;

		public int appLaunchUtcTime;

		public int lastAppLaunchUtcTime;

		public string userID;

		public string clientID;

		public string visitorId;

		public string countryName;

		public int debugUser;

		public int openCount;

		public int todayOpenCount;

		public int localTodayOpenCount;

		public int launchCount;

		public int sessionBegUtcTime;

		public int sessionEndUtcTime;

		public int dlyOpenUtcTime;

		public int dlyOpenLocalTime;

		public int todayPlaySec;

		public int todayLaunchCount;

		public int totalPlaySec;

		public int lastLoggedActSec;

		public int startLogMainLevelSec;

		public int availableLogTime;

		public float totalPurePlaySec;

		public int lastUpdateDialogShowTime;

		public string adjustAttributionInfo;

		public int alreadySendAdjustLogInfo;

		public int playDay;

		public int lastLoggedContinueDaySinceInstall;

		public int lastLoggedDaySinceInstall;

		public string userGuid;
	}
}
