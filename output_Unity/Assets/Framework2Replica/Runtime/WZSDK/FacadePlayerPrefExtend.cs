namespace WZSDK
{
	public abstract class FacadePlayerPrefExtend
	{
		public static string MusicKey = GetKey("music");

		public static string SoundKey = GetKey("sound");

		public static string HapticKey = GetKey("haptic");

		public static string LegacyMusicKey = GetKey("Music");

		public static string LegacySoundKey = GetKey("Sound");

		public static string LegacyHapticKey = GetKey("Haptic");

		public static string ifIAA = GetKey("ifIAA");

		public static string start = GetKey("start");

		public static string userLevel => GetKey("userLevel");

		public static string userExp => GetKey("userExp");

		public static string userName => GetKey("userName");

		public static string userID => GetKey("userID");

		public static string SuserID => GetKey("SuserID");

		public static string withdrawalCoin => GetKey("withdrawalCoin");

		public static string currentLevel => GetKey("currentLevel");

		public static string restartNumber => GetKey("restartNumber");

		public static string useBottles => GetKey("useBottles");

		public static string currentEliminationCount => GetKey("currentEliminationCount");

		public static string totalEliminationCount => GetKey("totalEliminationCount");

		public static string coin => GetKey("coin");

		public static string gems => GetKey("gems");

		public static string undo => GetKey("undo");

		public static string AddBottleCount => GetKey("AddBottleCount");

		public static string AddName => GetKey("AddName");

		public static string AddDay => GetKey("AddDay");

		public static string StageAdWatchCount => GetKey("StageAdWatchCount");

		public static string StageAdWatchOwnerStage => GetKey("StageAdWatchOwnerStage");

		public static string AddPhone => GetKey("AddPhone");

		public static string AddEmail => GetKey("AddEmail");

		public static string AddIndex => GetKey("AddIndex");

		public static string History => GetKey("History");

		public static string WalletBalanceCache => GetKey("WalletBalanceCache");

		public static string WalletWithdrawOrdersCache => GetKey("WalletWithdrawOrdersCache");

		public static string Stage => GetKey("Stage");

		public static string ClearColorCount => GetKey("ClearColorCount");

		public static string Stage5_StartDay => GetKey("Stage5_StartDay");

		public static string Stage5_CurrentCycle => GetKey("Stage5_CurrentCycle");

		public static string currentPalette => GetKey("currentPalette");

		public static string music => GetKey("music");

		public static string haptic => GetKey("haptic");

		public static string LevelTutorialShown => GetKey("LevelTutorialShown");

		public static string Level5ZoomOnboardingShown => GetKey("Level5ZoomOnboardingShown");

		public static string MaxUnlockedStage => GetKey("MaxUnlockedStage");

		public static string StageSystemVersion => GetKey("StageSystemVersion");

		public static string LastLoginDateKey => GetKey("LastLoginDate");

		public static string LastLoginTimestampKey => GetKey("LastLoginTimestamp");

		public static string LoginDayCountKey => GetKey("LoginDayCount");

		public static string LoginDayHistoryKey => GetKey("LoginDayHistory");

		public static string HasShownRewardAtLevel3 => GetKey("HasShownRewardAtLevel3");

		public static string HasShownRewardAtLevel9 => GetKey("HasShownRewardAtLevel9");

		public static string HasShownLuckyWalletAtLevel => GetKey("HasShownLuckyWalletAtLevel");

		public static string HasShownStage4 => GetKey("HasShownStage4");

		public static string Stage5LuckySpinCompleted => GetKey("Stage5LuckySpinCompleted");

		public static string LuckySpinWalletRewardClaimed => GetKey("LuckySpinWalletRewardClaimed");

		public static string WithDrawExecuted3 => GetKey("WithDrawExecuted3");

		private static string GetKey(string baseKey)
		{
			return GameDefines.ifIAA ? "IAA_" + baseKey : baseKey;
		}
	}
}
