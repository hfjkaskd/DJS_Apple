using System.Globalization;

namespace WZSDK
{
	public enum EUIType
	{
		ENone = 0,
		GameView = 1,
		Tutorial = 2,
		SettingsView = 3,
		ProfileView = 4,
		FinishView = 5,
		WarningView = 6,
		UserLevelView = 7,
		ReStartView = 8,
		ChallangeSuccessView = 9,
		UIEffectView = 10,
		LuckySpinView = 11,
		PropView = 12,
		RewardView = 13,
		NewGamePlay = 14,
		LoadingView = 15,
		NoticeView = 16,
		GamePlayerView = 17,
		WithdrawView = 18,
		WelcomeView = 19,
		FailView = 20,
		SlotView = 21,
		WithdrawMissionView = 22,
		TargetWithdrawView = 23,
		GameBalancView = 24,
		LuckyWalletView = 25,
		LuckySuccessView = 26,
		PayPalErrorView = 27,
		BloatView = 28,
		BloatProgressView = 29,
		BonusWheelView = 30,
		LuckyTips = 31,
		WithdrawFeedbackView = 32,
		ProgressView = 33,
		ToUView = 34
	}

	internal static class EUITypeContract
	{
		public static string GetStableName(EUIType value)
		{
			switch (value)
			{
				case EUIType.ENone: return "ENone";
				case EUIType.GameView: return "GameView";
				case EUIType.Tutorial: return "Tutorial";
				case EUIType.SettingsView: return "SettingsView";
				case EUIType.ProfileView: return "ProfileView";
				case EUIType.FinishView: return "FinishView";
				case EUIType.WarningView: return "WarningView";
				case EUIType.UserLevelView: return "UserLevelView";
				case EUIType.ReStartView: return "ReStartView";
				case EUIType.ChallangeSuccessView: return "ChallangeSuccessView";
				case EUIType.UIEffectView: return "UIEffectView";
				case EUIType.LuckySpinView: return "LuckySpinView";
				case EUIType.PropView: return "PropView";
				case EUIType.RewardView: return "RewardView";
				case EUIType.NewGamePlay: return "NewGamePlay";
				case EUIType.LoadingView: return "LoadingView";
				case EUIType.NoticeView: return "NoticeView";
				case EUIType.GamePlayerView: return "GamePlayerView";
				case EUIType.WithdrawView: return "WithdrawView";
				case EUIType.WelcomeView: return "WelcomeView";
				case EUIType.FailView: return "FailView";
				case EUIType.SlotView: return "SlotView";
				case EUIType.WithdrawMissionView: return "WithdrawMissionView";
				case EUIType.TargetWithdrawView: return "TargetWithdrawView";
				case EUIType.GameBalancView: return "GameBalancView";
				case EUIType.LuckyWalletView: return "LuckyWalletView";
				case EUIType.LuckySuccessView: return "LuckySuccessView";
				case EUIType.PayPalErrorView: return "PayPalErrorView";
				case EUIType.BloatView: return "BloatView";
				case EUIType.BloatProgressView: return "BloatProgressView";
				case EUIType.BonusWheelView: return "BonusWheelView";
				case EUIType.LuckyTips: return "LuckyTips";
				case EUIType.WithdrawFeedbackView: return "WithdrawFeedbackView";
				case EUIType.ProgressView: return "ProgressView";
				case EUIType.ToUView: return "ToUView";
				default:
					return "Unknown(" +
						((int)value).ToString(CultureInfo.InvariantCulture) + ")";
			}
		}
	}
}
