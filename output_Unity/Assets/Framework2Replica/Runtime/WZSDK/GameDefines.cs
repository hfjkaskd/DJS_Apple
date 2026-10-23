using System.Collections.Generic;
using UnityEngine;

namespace WZSDK
{
	public abstract class GameDefines
	{
		public static bool EnableWithdrawMissionFirstAdTestButton;

		public static bool EnableWithdrawHistoryAllStatusClick;

		public static float WheelReward
		{
			get => Framework2Host.Settings.economy.wheelReward;
			set => Framework2Host.Settings.economy.wheelReward = value;
		}

		public static bool ifIAA = true;

		public static float Elimination_Gem_Coefficient
		{
			get => Framework2Host.Settings.economy.eliminationGemCoefficient;
			set => Framework2Host.Settings.economy.eliminationGemCoefficient = value;
		}

		public const int Elimination_FlyMoneyCount = 8;

		public static int LuckyWalletUnlockLevel
		{
			get => Framework2Host.Settings.economy.luckyWalletUnlockLevel;
			set => Framework2Host.Settings.economy.luckyWalletUnlockLevel = value;
		}

		public static readonly List<int> ConfiguredLuckyRewardLevels =
			new List<int>();

		public static float LuckyChestPlaceholderCoin = 0f;

		public static float AddCoin
		{
			get => Framework2Host.Settings.economy.stageCompletionCoin;
			set => Framework2Host.Settings.economy.stageCompletionCoin = value;
		}

		public const float GRE_StayTime = 1f;

		public const int GRE_FlyMoneyCount = 8;

		public const float LTE_MoveTime = 0.5f;

		public const float LTE_StayTime = 2f;

		public const float LTE_GoAwayTime = 0.5f;

		public const float CE_MoveTime = 0.2f;

		public const float CE_StayTime = 2f;

		public static readonly Vector2 CE_Content_attemptTimes = new Vector2(1f, 2f);

		public const string nameString = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

		public const float FlyProp_MoveTime = 0.5f;

		public const float FlyMoney_MoveTime = 0.5f;

		public const float FlyMoney_DelayTime = 0.5f;

		public const float FlyMoney_IntervalTime = 0.1f;

		public const float FlyMoney_RandomSpawnDist = 100f;

		public const float FlyMoneyTip_DelayTime = 1f;

		public const float FlyMoneyTip_MoveTime = 2f;

		public const float FlyMoneyTip_MoveDist = 60f;

		public static readonly Vector2 TargetArriveTime = new Vector2(1f, 2f);

		public const float ShowAnimTime = 0.25f;

		public const float HideAnimTime = 0.25f;

		public const string ERIAAMoneyIconPath = "UI/LuckySpinIcons/icon_qianbi_IAA";

		public const string ERAddSpaceIconPath = "UI/FuncIcon/Bottle";

		public const string ERClearIconPath = "UI/FuncIcon/icon_mofab";

		public const string ERHammerIconPath = "UI/FuncIcon/icon_shuaxin";

		public const string ERAddPromptIconPath = "UINew/dengpao";

		public const string ERAddGuidIconPath = "UINew/chizi";

		public const string Default_SingleGem = "UI/Money/gem";

		public const string Default_SingDuileGem = "UI/Money/zuanshi_image";

		public const string LSIAAMoneyIconPath = "UI/LuckySpinIcons/icon_qianbi_IAA";

		public const string LSLuckyWalletIconPath = "UI/PayType/Icon/paypal_s";

		public static Dictionary<int, float> LevelTimerConfig = new Dictionary<int, float>();

		public static Dictionary<int, int> LevelMapConfig = new Dictionary<int, int>();

		public static int LuckySpin_Elimination_Interval
		{
			get => Framework2Host.Settings.sourceDefaults.LuckySpin_Elimination_Interval;
			set => Framework2Host.Settings.sourceDefaults.LuckySpin_Elimination_Interval = value;
		}

		public static float TimerReward_Interval
		{
			get => Framework2Host.Settings.sourceDefaults.TimerReward_Interval;
			set => Framework2Host.Settings.sourceDefaults.TimerReward_Interval = value;
		}

		public static bool EnableLuckyRewardOnPourComplete
		{
			get => Framework2Host.Settings.sourceDefaults.EnableLuckyRewardOnPourComplete;
			set => Framework2Host.Settings.sourceDefaults.EnableLuckyRewardOnPourComplete = value;
		}

		public static List<float> LS_Angles = new List<float>
		{
			33f,
			95f,
			153f,
			210f,
			266f,
			327f
		};

		public static List<float> BonusWheel_Angles = new List<float>
		{
			180f,
			135f,
			90f,
			45f,
			0f,
			-45f,
			90f,
			-135f
		};

		public const int LS_rotateCount = 6;

		public const float LS_rotateTime = 2f;

		public static int EXP_Single
		{
			get => Framework2Host.Settings.economy.experiencePerElimination;
			set => Framework2Host.Settings.economy.experiencePerElimination = value;
		}

		public const int SmallLevelGroupStart = 9;

		public const int SmallLevelGroupEnd = 17;

		public static int CloseInt
		{
			get => Framework2Host.Settings.sourceDefaults.CloseInt;
			set => Framework2Host.Settings.sourceDefaults.CloseInt = value;
		}

		public static float Elimination_Money =>
			GameManagerWZ.instance != null
				? GameManagerWZ.instance.getResourceNum(1, 2, 0)
				: Framework2Host.Settings.economy.missingManagerEliminationCoin;

		public static float IAA_Elimination_Money =>
			GameManagerWZ.instance != null
				? GameManagerWZ.instance.getResourceNum(1, 2, 0)
				: Framework2Host.Settings.economy.missingManagerIaaEliminationCoin;

		public static void ApplyConfiguredLuckyRewardLevels(
			IEnumerable<int> levels)
		{
			ConfiguredLuckyRewardLevels.Clear();
			if (levels == null)
			{
				return;
			}
			HashSet<int> uniqueLevels = new HashSet<int>();
			foreach (int level in levels)
			{
				if (level > 0 && uniqueLevels.Add(level))
				{
					ConfiguredLuckyRewardLevels.Add(level);
				}
			}
			ConfiguredLuckyRewardLevels.Sort();
		}
	}
}
