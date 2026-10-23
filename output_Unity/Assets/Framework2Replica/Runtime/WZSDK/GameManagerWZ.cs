using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace WZSDK
{
	public partial class GameManagerWZ : MonoBehaviour
	{
		public enum GAME_STATE
		{
			WAIT = 0,
			PLAYING = 1,
			FINISH = 2
		}

		[CompilerGenerated]
		private sealed class _003CProcessUIQueue_003Ed__154 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public GameManagerWZ _003C_003E4__this;

			private EUIType _003CnextUI_003E5__2;

			object IEnumerator<object>.Current
			{
				[DebuggerHidden]
				get
				{
					return null;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					return null;
				}
			}

			[DebuggerHidden]
			public _003CProcessUIQueue_003Ed__154(int _003C_003E1__state)
			{
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
			}

			private bool MoveNext()
			{
				return false;
			}

			bool IEnumerator.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				return this.MoveNext();
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
			}
		}

		[CompilerGenerated]
		private sealed class _003CShowFinishLevelIE_003Ed__153 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public GameManagerWZ _003C_003E4__this;

			object IEnumerator<object>.Current
			{
				[DebuggerHidden]
				get
				{
					return null;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					return null;
				}
			}

			[DebuggerHidden]
			public _003CShowFinishLevelIE_003Ed__153(int _003C_003E1__state)
			{
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
			}

			private bool MoveNext()
			{
				return false;
			}

			bool IEnumerator.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				return this.MoveNext();
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
			}
		}

		[CompilerGenerated]
		private sealed class _003CShowRewardViewDelayed_003Ed__144 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public float delay;

			public GameManagerWZ _003C_003E4__this;

			object IEnumerator<object>.Current
			{
				[DebuggerHidden]
				get
				{
					return null;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					return null;
				}
			}

			[DebuggerHidden]
			public _003CShowRewardViewDelayed_003Ed__144(int _003C_003E1__state)
			{
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
			}

			private bool MoveNext()
			{
				return false;
			}

			bool IEnumerator.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				return this.MoveNext();
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
			}
		}

		[CompilerGenerated]
		private sealed class _003CShowSuccessViewCoroutine_003Ed__163 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public GameManagerWZ _003C_003E4__this;

			public EUIType successViewType;

			object IEnumerator<object>.Current
			{
				[DebuggerHidden]
				get
				{
					return null;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					return null;
				}
			}

			[DebuggerHidden]
			public _003CShowSuccessViewCoroutine_003Ed__163(int _003C_003E1__state)
			{
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
			}

			private bool MoveNext()
			{
				return false;
			}

			bool IEnumerator.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				return this.MoveNext();
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
			}
		}

		public LevelGenerator levelGen;

		private Tutorial tutorial;

		public UIManager uiManager;

		public GameObject LocalContentErrorTip;

		public GameObject bg;

		public GameObject heartParent;

		public GameSceneWZ GameScene;

		public string userName;

		public string userID;

		public int userLevel;

		public int userExp;

		[HideInInspector]
		public float currentCoin;

		[HideInInspector]
		public float currentGems;

		[HideInInspector]
		public int currentUndo;

		[HideInInspector]
		public int currentClear;

		[HideInInspector]
		public int currentAddBottleCount;

		[HideInInspector]
		public int currentLv;

		[HideInInspector]
		public int currentStage;

		[HideInInspector]
		public int maxUnlockedStage;

		[HideInInspector]
		public int currentStageAdWatchCount;

		[HideInInspector]
		public int currentStageAdWatchOwnerStage;

		[HideInInspector]
		public int currentBottleFull;

		[HideInInspector]
		public int completedAnimationBottles;

		private int lastCompletedLevel;

		public int stage5CurrentCycle;

		public bool useCustomIAAValue;

		public bool ifIAA;

		public int decimals = 2;

		public string mark = "$";

		private int currentEliminationCount;

		private int totalEliminationCount;

		private STimer pouringTimer;

		private bool isPouringTimerActive;

		private STimer levelTimer;

		private bool isLevelTimerActive;

		private bool canTriggerReward;

		private bool isLevelTimerPausedByUI;

		private Queue<EUIType> pendingUIQueue = new Queue<EUIType>();

		private bool isProcessingUIQueue;

		private bool isLevelCompletionFlowRunning;

		private bool isProcessingStage4SlotFlow;

		private Task startupTask;

		private bool startupCompleted;

		private LoadingView activeLoadingView;

		private Action<int, int> OnStageUpdated;

		private Action OnCoinChanged;

		private Action OnLevelChanged;

		private Action OnLoginDayChanged;

		private Action OnAdWatchCountChanged;

		[HideInInspector]
		public GAME_STATE currentState;

		public static GameManagerWZ instance;

		private const int Stage3Id = 3;

		private const int Stage4Id = 4;

		private const int Stage5Id = 5;

		private static int NoCoinRewardEndLevel => Framework2Host.Settings.economy.noCoinRewardEndLevel;

		private static float Stage3AdRewardAmount => Framework2Host.Settings.economy.stage3AdRewardAmount;

		private static float Stage4SlotRewardAmount => Framework2Host.Settings.economy.stage4SlotRewardAmount;

		private static float NormalCoinHardCap => Framework2Host.Settings.economy.normalCoinHardCap;

		public static float Stage5CompensationTargetCoin => Framework2Host.Settings.economy.stage5CompensationTargetCoin;

		private const int ActiveStageSystemVersion = 2;

		private const string AutoNextStageField = "AutoNextStage";

		private void Awake()
		{
			instance = this;

		}

		private void Start()
		{
			// The Mahjong host initializes only the imported withdrawal subsystem.
		}

		public void StartGame()
		{
			if (startupCompleted ||
				(startupTask != null && !startupTask.IsCompleted))
			{
				return;
			}
			SetLocalContentErrorTipVisible(false);
			startupTask = RunStartupAsync();
		}

		private async Task RunStartupAsync()
		{
			await InitializeForMahjongAsync();
		}

		private void ApplyWhitePackageMode()
		{
			ifIAA = Framework2Host.Settings.iaaMode;
			GameDefines.ifIAA = ifIAA;
		}

		private void Update()
		{
			if (!startupCompleted) return;
			ManageTimerBasedOnUI();
			if (Framework2Host.GameplayActive == null || !Framework2Host.GameplayActive()) PauseLevelTimerByUI();
			STimerManager.Instance.UpdateInstance();
		}

		private void OnDestroy()
		{
			UnregisterStageEvents();
			if (instance == this)
			{
				ClearFacadeUserHandles();
				instance = null;
			}
		}

		private void SetLocalContentErrorTipVisible(bool visible)
		{
			if (LocalContentErrorTip != null)
			{
				LocalContentErrorTip.SetActive(visible);
			}
		}

		private int GetTotalLevels()
		{
			return Framework2Host.Settings.maximumProgressLevel;
		}

		public void CheckIfAllAnimationsCompleted()
		{
			if (isLevelCompletionFlowRunning ||
				currentState == GAME_STATE.FINISH)
			{
				return;
			}

			isLevelCompletionFlowRunning = true;
			currentState = GAME_STATE.FINISH;
			StopLevelTimer();
			pouringTimer?.Close();
			canTriggerReward = false;
			ShowFinishLevel();
		}

		public void OnPourCompleteForLuckyReward()
		{
			if (!GameDefines.EnableLuckyRewardOnPourComplete ||
				currentState == GAME_STATE.FINISH)
			{
				return;
			}

			UIManager manager = UIManager.instance;
			if (manager == null ||
				manager.HasActiveView(EUIType.RewardView) ||
				HasActiveSuccessView())
			{
				return;
			}

			if (currentLv > GameDefines.CloseInt)
			{
				CheckPourCompleteForReward();
			}
		}

		public void AddEliminationCount(int count = 1)
		{
			if (count < 1)
			{
				return;
			}
			currentEliminationCount += count;
			totalEliminationCount += count;
			SaveEliminationCount();
			CheckLuckySpinCondition();
		}

		public void AddExp(int value)
		{
			if (value < 1)
			{
				return;
			}

			UserDataManager dataManager = UserDataManager.Instance;
			if (dataManager == null)
			{
				return;
			}
			dataManager.AddExp(value);
			userExp = dataManager.GetExp();
			userLevel = dataManager.GetUserLevel();
		}

		private void InitGame()
		{
			currentState = GAME_STATE.WAIT;
			currentBottleFull = 0;
			completedAnimationBottles = 0;
			SetFirstData();
			GetCurrentLevel();
			LoadEliminationCount();
			LoadStageData();
			LoadStageAdWatchData();
			LoadStage5CycleData();
			CheckDailyLogin();
			RegisterStageEvents();
			RegisterFacadeUserHandles();
			FacadePayTypeExtend.RegionalChangeHandle += RegionalChange;
			OnCoinChanged?.Invoke();
			Framework2Host.CurrencyChanged();
			CheckCoinStageProgress();
		}

		private async Task PreloadCompletionViewsAsync(UIManager manager)
		{
			if (manager == null)
			{
				return;
			}

			Task challengeTask = TryPreloadCompletionViewAsync(
				manager,
				EUIType.ChallangeSuccessView);
			if (GameDefines.ifIAA)
			{
				await challengeTask;
				return;
			}

			await Task.WhenAll(
				challengeTask,
				TryPreloadCompletionViewAsync(
					manager,
					EUIType.LuckySuccessView));
		}

		private async Task TryPreloadCompletionViewAsync(
			UIManager manager,
			EUIType viewType)
		{
			try
			{
				GameObject prefab = await manager.LoadPrefabAsync(viewType);
				if (manager == null || prefab == null)
				{
					Debug.LogWarning(
						"Completion view preload was skipped: " +
						EUITypeContract.GetStableName(viewType) + ".");
					return;
				}

				await manager.PreloadViewsAsync(viewType, 1);
			}
			catch (Exception exception)
			{
				Debug.LogWarning(
					"Completion view preload failed for " +
					EUITypeContract.GetStableName(viewType) + ": " +
					exception.Message);
			}
		}

		private async Task InitializeGameUiAsync(UIManager manager)
		{
			var tutorialPrefab = await manager.LoadPrefabAsync(EUIType.Tutorial);
			if (this == null || manager == null) return;
			if (tutorialPrefab == null)
				throw new InvalidOperationException("Framework 2 tutorial prefab could not be loaded.");
			var effects = await manager.ShowViewAsync(EUIType.UIEffectView);
			if (this == null || manager == null) return;
			if (effects == null)
				throw new InvalidOperationException("Framework 2 UI effects prefab could not be loaded.");
			StopLevelTimer();
			currentState = GAME_STATE.PLAYING;
			StartLevelTimer();
		}

		private void SetFirstData()
		{
			if (!PlayerPrefs.HasKey(FacadePlayerPrefExtend.start))
			{
				PlayerPrefs.SetInt(FacadePlayerPrefExtend.start, 1);
				PlayerPrefs.SetFloat(FacadePlayerPrefExtend.coin, Framework2Host.Settings.economy.initialCoin);
				PlayerPrefs.SetFloat(FacadePlayerPrefExtend.gems, Framework2Host.Settings.economy.initialGems);
				PlayerPrefs.SetInt(FacadePlayerPrefExtend.AddBottleCount, Framework2Host.Settings.economy.initialAddBottle);
				PlayerPrefs.SetInt(FacadePlayerPrefExtend.ClearColorCount, Framework2Host.Settings.economy.initialClear);
				PlayerPrefs.SetInt(FacadePlayerPrefExtend.undo, Framework2Host.Settings.economy.initialUndo);
				PlayerPrefs.SetInt(FacadePlayerPrefExtend.currentEliminationCount, 0);
				PlayerPrefs.SetInt(FacadePlayerPrefExtend.totalEliminationCount, 0);
				PlayerPrefs.SetInt(FacadePlayerPrefExtend.AddDay, 0);
				PlayerPrefs.SetInt(FacadePlayerPrefExtend.StageAdWatchCount, 0);
				PlayerPrefs.SetInt(FacadePlayerPrefExtend.StageAdWatchOwnerStage, 0);
			}

			UserDataManager dataManager = UserDataManager.Instance;
			userName = dataManager.GetUserName();
			userID = dataManager.GetUserID();
			userLevel = dataManager.GetUserLevel();
			userExp = dataManager.GetExp();
			currentCoin = NormalizeRegularCoin(PlayerPrefs.GetFloat(FacadePlayerPrefExtend.coin, Framework2Host.Settings.economy.initialCoin));
			currentGems = PlayerPrefs.GetFloat(FacadePlayerPrefExtend.gems, Framework2Host.Settings.economy.initialGems);
			currentUndo = PlayerPrefs.GetInt(FacadePlayerPrefExtend.undo, Framework2Host.Settings.economy.initialUndo);
			currentClear = PlayerPrefs.GetInt(FacadePlayerPrefExtend.ClearColorCount, Framework2Host.Settings.economy.initialClear);
			currentAddBottleCount = PlayerPrefs.GetInt(FacadePlayerPrefExtend.AddBottleCount, Framework2Host.Settings.economy.initialAddBottle);
		}

		private void GetCurrentLevel()
		{
			currentLv = PlayerPrefs.GetInt(FacadePlayerPrefExtend.currentLevel, 1);
		}

		private void LoadStageData()
		{
			MigrateStageProgressIfNeeded();
			int defaultStage = GetStageData(1) != null ? 1 : 0;
			currentStage = PlayerPrefs.GetInt(
				FacadePlayerPrefExtend.Stage, defaultStage);
			if (currentStage < 1 ||
				GetStageData(currentStage) == null)
			{
				currentStage = defaultStage;
			}
			maxUnlockedStage = Mathf.Max(
				PlayerPrefs.GetInt(
					FacadePlayerPrefExtend.MaxUnlockedStage,
					currentStage),
				currentStage);
			SaveCurrentStage();
			SaveMaxUnlockedStage();
			PlayerPrefs.Save();
		}

		private void LoadStage5CycleData()
		{
			int type;
			int value;
			if (!TryGetStageRequirement(
					currentStage, out type, out value) ||
				type != 3)
			{
				stage5CurrentCycle = 0;
				return;
			}
			stage5CurrentCycle = PlayerPrefs.GetInt(
				FacadePlayerPrefExtend.Stage5_CurrentCycle, 0);
			PlayerPrefs.SetInt(
				FacadePlayerPrefExtend.AddDay,
				stage5CurrentCycle);
		}

		private void LoadEliminationCount()
		{
			currentEliminationCount = PlayerPrefs.GetInt(
				FacadePlayerPrefExtend.currentEliminationCount, 0);
			totalEliminationCount = PlayerPrefs.GetInt(
				FacadePlayerPrefExtend.totalEliminationCount, 0);
		}

		private void RegisterStageEvents()
		{
			OnStageUpdated += OnStageChanged;
			OnCoinChanged += OnCoinResourceChanged;
			OnLoginDayChanged += OnLoginDayProgressChanged;
			OnAdWatchCountChanged += OnAdWatchProgressChanged;
		}

		private void UnregisterStageEvents()
		{
			OnStageUpdated -= OnStageChanged;
			OnCoinChanged -= OnCoinResourceChanged;
			OnLoginDayChanged -= OnLoginDayProgressChanged;
			OnAdWatchCountChanged -= OnAdWatchProgressChanged;
			FacadePayTypeExtend.RegionalChangeHandle -= RegionalChange;
		}

		private void MigrateStageProgressIfNeeded()
		{
			if (PlayerPrefs.GetInt(
					FacadePlayerPrefExtend.StageSystemVersion, 0) >=
				ActiveStageSystemVersion)
			{
				return;
			}

			int maxStage = GetMaxConfiguredStageId();
			if (maxStage <= 0)
			{
				return;
			}

			int legacyStage = PlayerPrefs.GetInt(
				FacadePlayerPrefExtend.Stage, 0);
			int legacyMax = PlayerPrefs.GetInt(
				FacadePlayerPrefExtend.MaxUnlockedStage,
				legacyStage);
			int migratedStage = Mathf.Clamp(
				NormalizeLegacyStage(legacyStage), 1, maxStage);
			int migratedMax = Mathf.Max(
				migratedStage,
				Mathf.Clamp(
					NormalizeLegacyStage(legacyMax),
					1,
					maxStage));
			PlayerPrefs.SetInt(
				FacadePlayerPrefExtend.Stage, migratedStage);
			PlayerPrefs.SetInt(
				FacadePlayerPrefExtend.MaxUnlockedStage,
				migratedMax);
			PlayerPrefs.SetInt(
				FacadePlayerPrefExtend.StageSystemVersion,
				ActiveStageSystemVersion);
			PlayerPrefs.Save();
		}

		public void SaveCurrentStage()
		{
			PlayerPrefs.SetInt(
				FacadePlayerPrefExtend.Stage, currentStage);
		}

		public void SaveMaxUnlockedStage()
		{
			PlayerPrefs.SetInt(
				FacadePlayerPrefExtend.MaxUnlockedStage,
				maxUnlockedStage);
		}

		internal void SetStageForTesting(int stage)
		{
			int previous = currentStage;
			currentStage = Mathf.Clamp(stage, 1, 5);
			maxUnlockedStage = Mathf.Max(maxUnlockedStage, currentStage);
			SaveCurrentStage();
			SaveMaxUnlockedStage();
			OnStageChanged(currentStage, previous);
			PlayerPrefs.Save();
		}

		private void TrackStageAnalytics()
		{
			try
			{
				int type, target;
				if (!TryGetStageRequirement(currentStage, out type, out target)) return;
				double progress = type == 1 ? Math.Max(0, currentLv - 1) : type == 2 ? currentCoin : type == 3 ? stage5CurrentCycle : type == 4 ? currentStageAdWatchCount : 0;
				MahjongProductAnalytics.SetContext("f2_stage", currentStage);
				MahjongProductAnalytics.State("f2_stage_snapshot", "f2_stage", "stage", currentStage, "task_type", type, "target", target, "progress", progress, "complete", progress >= target, "auto_advance", ShouldAutoEnterNextStage(currentStage), "level", currentLv, "iaa_mode", GameDefines.ifIAA);
			}
			catch { /* Analytics must not affect task progression. */ }
		}

		public Dictionary<string, object> GetStageData(int stageId)
		{
			UserDataManager dataManager = UserDataManager.Instance;
			return dataManager != null
				? dataManager.getStageData(stageId)
				: null;
		}

		private int NormalizeLegacyStage(int legacyStage)
		{
			if (legacyStage < 1)
			{
				return 1;
			}
			return legacyStage < 3
				? legacyStage + 1
				: legacyStage;
		}

		private int GetMaxConfiguredStageId()
		{
			int stageId = 1;
			while (GetStageData(stageId) != null)
			{
				stageId++;
			}
			return stageId - 1;
		}

		public bool TryGetStageRequirement(int stageId, out int type, out int value)
		{
			type = 0;
			value = 0;
			Dictionary<string, object> stageData =
				GetStageData(stageId);
			if (stageData == null ||
				!stageData.ContainsKey("Type") ||
				!stageData.ContainsKey("Value"))
			{
				return false;
			}
			return int.TryParse(
					stageData["Type"] as string,
					out type) &&
				int.TryParse(
					stageData["Value"] as string,
					out value);
		}

		public bool TryGetCurrentStageProgressForTesting(
			out int type,
			out int target,
			out double current,
			out bool autoNext)
		{
			current = 0.0;
			autoNext = ShouldAutoEnterNextStage(currentStage);
			if (!TryGetStageRequirement(currentStage, out type, out target))
			{
				return false;
			}

			switch (type)
			{
				case 1:
					current = currentLv;
					break;
				case 2:
					current = currentCoin;
					break;
				case 3:
					current = stage5CurrentCycle;
					break;
				case 4:
					current = currentStageAdWatchCount;
					break;
				default:
					return false;
			}
			return true;
		}

		public bool CompleteCurrentCoinStageForTesting()
		{
			int type;
			int target;
			if (GameDefines.ifIAA ||
				!TryGetStageRequirement(currentStage, out type, out target) ||
				type != 2)
			{
				return false;
			}

			// The production path deliberately caps ordinary cash at 2999.5.
			// This diagnostic-only entry point persists the configured stage target
			// long enough for the normal stage event chain to validate the 3000 case.
			currentCoin = Mathf.Max(currentCoin, target);
			PlayerPrefs.SetFloat(FacadePlayerPrefExtend.coin, currentCoin);
			PlayerPrefs.Save();
			UpdateCoinUI();
			OnCoinChanged?.Invoke();
			Framework2Host.CurrencyChanged();
			return true;
		}

		public bool ShouldAutoEnterNextStage(int stageId)
		{
			Dictionary<string, object> stageData =
				GetStageData(stageId);
			if (stageData == null ||
				!stageData.ContainsKey(AutoNextStageField))
			{
				return true;
			}
			string raw = stageData[AutoNextStageField] as string;
			if (string.IsNullOrEmpty(raw))
			{
				return true;
			}
			bool booleanValue;
			if (bool.TryParse(raw, out booleanValue))
			{
				return booleanValue;
			}
			int integerValue;
			return !int.TryParse(raw, out integerValue) ||
				integerValue != 0;
		}

		private bool IsLoginStage(int stageId)
		{
			int type;
			int value;
			return TryGetStageRequirement(
					stageId, out type, out value) &&
				type == 3;
		}

		private bool IsCoinStage(int stageId)
		{
			int type;
			int value;
			return TryGetStageRequirement(
					stageId, out type, out value) &&
				type == 2;
		}

		private bool IsAdWatchStage(int stageId)
		{
			int type;
			int value;
			return TryGetStageRequirement(
					stageId, out type, out value) &&
				type == 4;
		}

		private void TryAutoEnterCompletedStage(int stageId)
		{
			if (stageId > 0 &&
				!GameDefines.ifIAA &&
				currentStage == stageId &&
				ShouldAutoEnterNextStage(stageId) &&
				IsStageCompleted(stageId))
			{
				TryEnterNextStage();
			}
		}

		private void HandleCompletedStage(int completedStage)
		{
			MahjongProductAnalytics.Track("f2_stage_completed", "stage", completedStage, "level", currentLv, "balance", currentCoin);
			double coinBeforeReward = Math.Round(
				currentCoin,
				decimals,
				MidpointRounding.ToEven);

			if (completedStage == 1)
			{
				ConsumeLegacyStageReward(completedStage);
				SavePendingHostStageTutorial(completedStage);
				DataModel dataModel = new DataModel
				{
					HistoryId = string.Empty,
					Money = coinBeforeReward,
					Level = currentLv - 1,
					Num = 1,
					CloseType = 1
				};
				if (currentStage <= 2)
				{
					hostTutorialPresentation = Tutorial.OpenWithdrawTutorialAsync(dataModel);
				}
				return;
			}

			if (completedStage != 2)
			{
				return;
			}

			if (!ConsumeLegacyStageReward(completedStage))
			{
				AddCoin(GameDefines.AddCoin, currentLv);
				double coinAfterReward = GetCoin();
				GameScene?.setHistory(coinAfterReward, 2);
				WithdrawApiService.EnsureQueuedFirstAdOrder(coinAfterReward);
			}

			SavePendingHostStageTutorial(completedStage);
			hostTutorialPresentation = Tutorial.ShowStepAsync(Tutorial.TYPE.TYPE3, 3);
		}

		private void RefreshStageUI()
		{
			TrackStageAnalytics();
			UIManager manager =
				UIManager.instance != null ? UIManager.instance : uiManager;
			if (manager == null)
			{
				return;
			}
			if (GameDefines.ifIAA)
			{
				manager.GetView<GameView>(
					EUIType.GameView)?.RefreshStageUI();
			}
			else
			{
				manager.GetView<GamePlayerView>(
					EUIType.GamePlayerView)?.RefreshStageUI();
			}
		}

		public bool IsStageCompleted(int stageId)
		{
			int type;
			int value;
			if (!TryGetStageRequirement(
					stageId, out type, out value))
			{
				return false;
			}
			switch (type)
			{
				case 1:
					return currentLv > value;
				case 2:
					return currentCoin >= value;
				case 3:
					return stage5CurrentCycle >= value;
				case 4:
					return currentStageAdWatchCount >= value;
				default:
					return false;
			}
		}

		public bool TryEnterNextStage()
		{
			MahjongProductAnalytics.Track("f2_stage_advance_check", "stage", currentStage, "complete", IsStageCompleted(currentStage), "iaa_mode", GameDefines.ifIAA);
			if (GameDefines.ifIAA ||
				!IsStageCompleted(currentStage))
			{
				return false;
			}

			int completedStage = currentStage;
			int nextStage = completedStage + 1;
			if (GetStageData(nextStage) == null)
			{
				return false;
			}

			MahjongProductAnalytics.Track("f2_stage_advanced", "previous_stage", completedStage, "stage", nextStage, "balance", currentCoin, "level", currentLv);
			currentStage = nextStage;
			if (maxUnlockedStage < nextStage)
			{
				maxUnlockedStage = nextStage;
				SaveMaxUnlockedStage();
			}
			SaveCurrentStage();
			PlayerPrefs.Save();
			OnStageUpdated?.Invoke(currentStage, completedStage);
			HandleCompletedStage(completedStage);
			return true;
		}

		private void CheckCoinStageProgress()
		{
			while (IsCoinStage(currentStage) &&
				ShouldAutoEnterNextStage(currentStage) &&
				IsStageCompleted(currentStage))
			{
				if (!TryEnterNextStage())
				{
					break;
				}
			}
		}

		private void CheckDeferredStageProgressOnLevelComplete(int stageId)
		{
			int type;
			int value;
			if (stageId <= 0 ||
				GameDefines.ifIAA ||
				currentStage != stageId ||
				!TryGetStageRequirement(
					stageId, out type, out value) ||
				type != 1 ||
				!ShouldAutoEnterNextStage(stageId) ||
				!IsStageCompleted(stageId) ||
				!TryEnterNextStage())
			{
				return;
			}
			CheckCoinStageProgress();
		}

		private void OnStageChanged(int newStage, int oldStage)
		{
			Debug.Log(string.Format(
				"Stage changed from {0} to {1}",
				oldStage,
				newStage));

			if (IsAdWatchStage(newStage))
			{
				ResetStageAdWatchProgress(newStage);
			}
			else if (IsAdWatchStage(oldStage))
			{
				ResetStageAdWatchProgress();
			}

			if (IsLoginStage(newStage))
			{
				stage5CurrentCycle = 0;
				SaveStage5CycleData();
			}
			else if (IsLoginStage(oldStage))
			{
				stage5CurrentCycle = 0;
				PlayerPrefs.SetInt(FacadePlayerPrefExtend.AddDay, 0);
				PlayerPrefs.DeleteKey(
					FacadePlayerPrefExtend.Stage5_StartDay);
				PlayerPrefs.DeleteKey(
					FacadePlayerPrefExtend.Stage5_CurrentCycle);
				PlayerPrefs.Save();
			}

			RefreshStageUI();
			TryShowStage4TargetWithdrawView(newStage);
			TryShowStage5CompensationLuckySpin(newStage);
			CheckCoinStageProgress();
		}

		private bool HasCompletedStage5CompensationLuckySpin()
		{
			return PlayerPrefs.GetInt(
				FacadePlayerPrefExtend.Stage5LuckySpinCompleted,
				0) != 0;
		}

		private void MarkStage5CompensationLuckySpinCompleted()
		{
			MahjongProductAnalytics.Track("f2_compensation_completed", "balance", currentCoin, "target", Stage5CompensationTargetCoin, "stage", currentStage);
			PlayerPrefs.SetInt(
				FacadePlayerPrefExtend.Stage5LuckySpinCompleted,
				1);
			PlayerPrefs.Save();
		}

		private void TryShowStage5CompensationLuckySpin(int stageId)
		{
			UIManager manager = UIManager.instance;
			if (stageId != Stage5Id ||
				GameDefines.ifIAA ||
				currentStage != Stage5Id ||
				manager == null ||
				isProcessingStage4SlotFlow ||
				HasCompletedStage5CompensationLuckySpin() ||
				manager.HasActiveView(EUIType.BloatView) ||
				manager.HasActiveView(EUIType.LuckySpinView) ||
				manager.HasActiveView(EUIType.BonusWheelView))
			{
				return;
			}

			float rewardAmount = Mathf.Max(
				0f,
				Stage5CompensationTargetCoin - currentCoin);
			MahjongProductAnalytics.Track("f2_compensation_decision", "target", Stage5CompensationTargetCoin, "balance", currentCoin, "amount", rewardAmount, "required", rewardAmount > 0f);
			if (rewardAmount <= 0f)
			{
				MarkStage5CompensationLuckySpinCompleted();
				return;
			}

			double coinBeforeReward = Math.Round(
				currentCoin, decimals);
			LuckySpinOpenData openData =
				new LuckySpinOpenData
				{
					ForceMoneyReward = true,
					ForcedMoneyAmount = rewardAmount,
					MoneyRewardTarget =
						LuckySpinMoneyRewardTarget.CurrentCoin,
					IsMandatory = true,
					AutoStartSpin = true,
					FixedBonusWheelAngleIndex = 1,
					OnRewardComplete = () =>
					{
						MarkStage5CompensationLuckySpinCompleted();
						OpenStage5CompensationSuccessView(
							coinBeforeReward);
					}
				};
			MahjongProductAnalytics.Track("f2_compensation_requested", "amount", rewardAmount, "wallet", "main_cash", "fixed_index", 1, "auto_spin", true);
			manager.ShowView(EUIType.BloatView, openData);
		}

		private void OpenStage5CompensationSuccessView(double coinBeforeReward)
		{
			if (GameApp.viewManager == null)
			{
				return;
			}
			DataModel dataModel = new DataModel
			{
				Level = Math.Max(0, currentLv - 1),
				Num = Stage5Id,
				Money = coinBeforeReward,
				CloseType = 1,
				Special =
					DataModel.SpecialRewardAlreadyGranted
			};
			GameApp.viewManager.Open(
				ViewType.WithDrawSuc, dataModel);
		}

		private void TryShowStage4TargetWithdrawView(int stageId)
		{
			UIManager manager = UIManager.instance;
			if (stageId != Stage4Id ||
				GameDefines.ifIAA ||
				manager == null ||
				IsStageCompleted(Stage4Id) ||
				PlayerPrefs.GetInt(
					FacadePlayerPrefExtend.HasShownStage4, 0) != 0 ||
				manager.HasActiveView(EUIType.TargetWithdrawView) ||
				manager.HasActiveView(EUIType.SlotView))
			{
				return;
			}

			PlayerPrefs.SetInt(
				FacadePlayerPrefExtend.HasShownStage4, 1);
			PlayerPrefs.Save();
			isProcessingStage4SlotFlow = true;
			MahjongProductAnalytics.Track("f2_stage4_slot_requested", "stage", stageId, "amount", Stage4SlotRewardAmount, "balance", currentCoin);
			manager.ShowView(
				EUIType.SlotView,
				Stage4SlotRewardAmount,
				(Action)(() =>
				{
					if (UIManager.instance != null)
					{
						UIManager.instance.ShowView(
							EUIType.ProgressView,
							Stage4SlotRewardAmount);
					}
				}));
		}

		public void BeginStage4SlotFlow()
		{
			isProcessingStage4SlotFlow = true;
		}

		public void CompleteStage4SlotFlow()
		{
			MahjongProductAnalytics.Track("f2_stage4_flow_completed", "stage", currentStage, "balance", currentCoin);
			isProcessingStage4SlotFlow = false;
			if (currentStage == Stage5Id)
			{
				TryShowStage5CompensationLuckySpin(Stage5Id);
			}
		}

		private void OnCoinResourceChanged()
		{
			CheckCoinStageProgress();
			RefreshStageUI();
		}

		private void OnLoginDayProgressChanged()
		{
			int type;
			int value;
			if (TryGetStageRequirement(
					currentStage, out type, out value) &&
				type == 3)
			{
				TryAutoEnterCompletedStage(currentStage);
			}
			RefreshStageUI();
		}

		private void OnAdWatchProgressChanged()
		{
			int type;
			int value;
			if (TryGetStageRequirement(
					currentStage, out type, out value) &&
				type == 4)
			{
				TryAutoEnterCompletedStage(currentStage);
			}
			RefreshStageUI();
		}

		public bool TrySyncStageProgress(int stageId)
		{
			return !GameDefines.ifIAA &&
				currentStage == stageId &&
				IsStageCompleted(stageId) &&
				TryEnterNextStage();
		}

		private void SaveStage5CycleData()
		{
			int type;
			int value;
			if (!TryGetStageRequirement(
					currentStage, out type, out value) ||
				type != 3)
			{
				return;
			}
			PlayerPrefs.SetInt(
				FacadePlayerPrefExtend.Stage5_CurrentCycle,
				stage5CurrentCycle);
			PlayerPrefs.SetInt(
				FacadePlayerPrefExtend.AddDay,
				stage5CurrentCycle);
			PlayerPrefs.Save();
		}

		private void CheckStage5Progress()
		{
			int type;
			int value;
			if (TryGetStageRequirement(
					currentStage, out type, out value) &&
				type == 3)
			{
				stage5CurrentCycle++;
				SaveStage5CycleData();
				OnLoginDayChanged?.Invoke();
			}
		}

		private void CheckDailyLogin()
		{
			string today = Framework2Host.LocalNow.ToString(
				"yyyy-MM-dd");
			string lastLogin = PlayerPrefs.GetString(
				FacadePlayerPrefExtend.LastLoginDateKey,
				string.Empty);
			if (lastLogin == today)
			{
				return;
			}
			OnDailyLogin();
			PlayerPrefs.SetString(
				FacadePlayerPrefExtend.LastLoginDateKey,
				today);
			PlayerPrefs.Save();
		}

		public void OnDailyLogin()
		{
			int type;
			int value;
			if (TryGetStageRequirement(
					currentStage, out type, out value) &&
				type == 3)
			{
				CheckStage5Progress();
			}
		}

		public void RefreshDailyLoginForTesting()
		{
			CheckDailyLogin();
		}

		public void ResetDailyLoginClockForTesting()
		{
			PlayerPrefs.SetString(
				FacadePlayerPrefExtend.LastLoginDateKey,
				Framework2Host.LocalNow.ToString("yyyy-MM-dd"));
			PlayerPrefs.Save();
			RefreshStageUI();
		}

		public void SaveLoginDay(int day)
		{
			stage5CurrentCycle = Mathf.Max(0, day);
			PlayerPrefs.SetInt(
				FacadePlayerPrefExtend.AddDay,
				stage5CurrentCycle);
			PlayerPrefs.SetInt(
				FacadePlayerPrefExtend.Stage5_CurrentCycle,
				stage5CurrentCycle);
			PlayerPrefs.Save();
		}

		public int getLoginDay()
		{
			return stage5CurrentCycle;
		}

		private void LoadStageAdWatchData()
		{
			currentStageAdWatchCount = PlayerPrefs.GetInt(
				FacadePlayerPrefExtend.StageAdWatchCount, 0);
			currentStageAdWatchOwnerStage = PlayerPrefs.GetInt(
				FacadePlayerPrefExtend.StageAdWatchOwnerStage,
				0);
			EnsureStageAdWatchProgressState();
		}

		private void EnsureStageAdWatchProgressState()
		{
			int type;
			int value;
			if (!TryGetStageRequirement(
					currentStage, out type, out value) ||
				type != 4)
			{
				if (currentStageAdWatchCount == 0 &&
					currentStageAdWatchOwnerStage == 0)
				{
					return;
				}
				currentStageAdWatchCount = 0;
				currentStageAdWatchOwnerStage = 0;
			}
			else
			{
				if (currentStageAdWatchOwnerStage == currentStage)
				{
					return;
				}
				currentStageAdWatchCount = 0;
				currentStageAdWatchOwnerStage = currentStage;
			}
			SaveStageAdWatchData();
		}

		public void RecordStageAdWatch(int count = 1)
		{
			RecordStageAdWatch(count, true);
		}

		internal void RecordStageAdWatchWithoutReward(int count = 1)
		{
			RecordStageAdWatch(count, false);
		}

		private void RecordStageAdWatch(int count, bool grantReward)
		{
			MahjongProductAnalytics.Track("f2_stage_ad_eligibility", "stage", currentStage, "count", count, "grant_reward", grantReward, "owner_stage", currentStageAdWatchOwnerStage, "eligible", count > 0 && IsAdWatchStage(currentStage));
			if (count <= 0)
			{
				return;
			}
			int type;
			int value;
			if (!TryGetStageRequirement(
					currentStage, out type, out value) ||
				type != 4)
			{
				return;
			}
			if (currentStageAdWatchOwnerStage != currentStage)
			{
				currentStageAdWatchCount = 0;
				currentStageAdWatchOwnerStage = currentStage;
				SaveStageAdWatchData();
			}
			int previousCount = currentStageAdWatchCount;
			currentStageAdWatchCount += count;
			MahjongProductAnalytics.Track("f2_stage_ad_progress", "stage", currentStage, "previous", previousCount, "current", currentStageAdWatchCount, "target", value, "grant_reward", grantReward);
			SaveStageAdWatchData();
			if (grantReward)
			{
				ApplyStageAdWatchReward(
					previousCount,
					currentStageAdWatchCount);
			}
			OnAdWatchCountChanged?.Invoke();
		}

		public int GetStageAdWatchCount()
		{
			return currentStageAdWatchCount;
		}

		public bool SetStageAdWatchProgressForTesting(int count)
		{
			int type;
			int target;
			if (!TryGetStageRequirement(
					currentStage, out type, out target) ||
				type != 4)
			{
				return false;
			}

			int normalized = Mathf.Clamp(count, 0, Mathf.Max(0, target));
			if (currentStageAdWatchOwnerStage != currentStage)
			{
				currentStageAdWatchCount = 0;
				currentStageAdWatchOwnerStage = currentStage;
			}
			if (normalized > currentStageAdWatchCount)
			{
				RecordStageAdWatch(normalized - currentStageAdWatchCount);
				return true;
			}

			currentStageAdWatchCount = normalized;
			SaveStageAdWatchData();
			OnAdWatchCountChanged?.Invoke();
			RefreshStageUI();
			return true;
		}

		public void ResetStage4SlotForTesting()
		{
			PlayerPrefs.DeleteKey(FacadePlayerPrefExtend.HasShownStage4);
			PlayerPrefs.Save();
		}

		public void ResetStage5CompensationForTesting()
		{
			PlayerPrefs.DeleteKey(
				FacadePlayerPrefExtend.Stage5LuckySpinCompleted);
			PlayerPrefs.Save();
		}

		private void ResetStageAdWatchProgress(int ownerStage = 0)
		{
			currentStageAdWatchCount = 0;
			currentStageAdWatchOwnerStage = ownerStage;
			SaveStageAdWatchData();
		}

		private void SaveStageAdWatchData()
		{
			PlayerPrefs.SetInt(
				FacadePlayerPrefExtend.StageAdWatchCount,
				currentStageAdWatchCount);
			PlayerPrefs.SetInt(
				FacadePlayerPrefExtend.StageAdWatchOwnerStage,
				currentStageAdWatchOwnerStage);
			PlayerPrefs.Save();
		}

		private void ApplyStageAdWatchReward(int previousCount, int newCount)
		{
			if (currentStage != Stage3Id)
			{
				return;
			}

			int rewardLimit = Mathf.Max(
				0,
				GetStageAdRewardTarget() - 1);
			int clampedPrevious = Mathf.Clamp(
				previousCount, 0, rewardLimit);
			int clampedNew = Mathf.Clamp(
				newCount, 0, rewardLimit);
			int rewardedViews = clampedNew - clampedPrevious;
			if (rewardedViews <= 0)
			{
				return;
			}

			double rewardAmount =
				rewardedViews * Stage3AdRewardAmount;
			MahjongProductAnalytics.Track("f2_stage_ad_reward", "stage", currentStage, "previous_count", previousCount, "new_count", newCount, "reward_limit", rewardLimit, "amount", rewardAmount);
			AddCoin((float)rewardAmount, currentLv);
			AddRewardToPendingStage2History(rewardAmount);
		}

		private int GetStageAdRewardTarget()
		{
			int type;
			int value;
			return TryGetStageRequirement(
					Stage3Id, out type, out value) &&
				type == 4 &&
				value > 0
				? value
				: 3;
		}

		private void AddRewardToPendingStage2History(double rewardAmount)
		{
			if (rewardAmount <= 0.0)
			{
				return;
			}

			List<HistoryModel> history =
				SPlayerPrefs.GetListTem<HistoryModel>(
					FacadePlayerPrefExtend.History);
			for (int i = history.Count - 1; i >= 0; i--)
			{
				HistoryModel item = history[i];
				if (item == null ||
					item.Stage != 2 ||
					item.Status == 1)
				{
					continue;
				}

				MahjongProductAnalytics.Track("f2_queued_amount_changed", "stage", item.Stage, "before", item.Money, "delta", rewardAmount, "wallet_credit", false);
				item.Money = Math.Round(
					item.Money + rewardAmount,
					decimals,
					MidpointRounding.ToEven);
				SPlayerPrefs.SetListTem(
					FacadePlayerPrefExtend.History,
					history);
				WithdrawApiService.EnsureQueuedFirstAdOrder(
					item.Money);
				return;
			}
		}

		private void ResetLevelTimerState()
		{
			StopLevelTimer();
			isLevelTimerActive = false;
			canTriggerReward = false;
			isLevelTimerPausedByUI = false;
		}

		public void StartLevelTimer()
		{
			if (Framework2Host.Settings.useMahjongInGameRewardTrigger) return;
			if (currentLv < Framework2Host.Settings.timedRewardStartLevel || currentState == GAME_STATE.FINISH)
			{
				return;
			}

			StopLevelTimer();
			isLevelTimerActive = true;
			canTriggerReward = false;
			isLevelTimerPausedByUI = false;
			float interval = GetLevelTimerInterval(currentLv);
			MahjongProductAnalytics.Track("f2_reward_timer_started", "level", currentLv, "interval_seconds", interval, "trigger_rule", "timer_then_pair");
			levelTimer = STimerManager.Instance.CreateSTimer(
				interval,
				0,
				true,
				false,
				OnLevelTimerComplete,
				GetTime);
		}

		private float GetLevelTimerInterval(int level)
		{
			if (GameDefines.LevelTimerConfig != null && GameDefines.LevelTimerConfig.Count > 0)
			{
				List<int> levels = new List<int>(GameDefines.LevelTimerConfig.Keys);
				levels.Sort();
				for (int i = 0; i < levels.Count; i++)
				{
					if (levels[i] >= level)
					{
						GameDefines.TimerReward_Interval = GameDefines.LevelTimerConfig[levels[i]];
						return GameDefines.TimerReward_Interval;
					}
				}

				GameDefines.TimerReward_Interval = GameDefines.LevelTimerConfig[levels[levels.Count - 1]];
			}
			return GameDefines.TimerReward_Interval;
		}

		public void GetTime(float time)
		{
		}

		public void StopLevelTimer()
		{
			levelTimer?.Close();
			levelTimer = null;
			isLevelTimerActive = false;
			isLevelTimerPausedByUI = false;
		}

		public void PauseLevelTimerByUI()
		{
			if (levelTimer != null && isLevelTimerActive && !isLevelTimerPausedByUI)
			{
				levelTimer.Pause();
				isLevelTimerPausedByUI = true;
			}
		}

		public void ResumeLevelTimerByUI()
		{
			if (levelTimer != null && isLevelTimerActive && isLevelTimerPausedByUI)
			{
				levelTimer.Start();
				isLevelTimerPausedByUI = false;
			}
		}

		private void OnLevelTimerComplete()
		{
			MahjongProductAnalytics.Track("f2_reward_timer_ready", "level", currentLv, "stage", currentStage);
			isLevelTimerPausedByUI = false;
			isLevelTimerActive = false;
			canTriggerReward = true;
		}

		public bool ArmTimedRewardForTesting()
		{
			if (currentLv <= GameDefines.CloseInt ||
				currentState == GAME_STATE.FINISH)
			{
				return false;
			}

			StopLevelTimer();
			OnLevelTimerComplete();
			return true;
		}

		public bool IsTimedRewardReadyForTesting()
		{
			return canTriggerReward;
		}

		public bool IsTimedRewardTimerActiveForTesting()
		{
			return isLevelTimerActive;
		}

		public float GetTimedRewardIntervalForTesting()
		{
			return GetLevelTimerInterval(currentLv);
		}

		public void CheckPourCompleteForReward()
		{
			if (Framework2Host.Settings.useMahjongInGameRewardTrigger) return;
			if (currentLv <= GameDefines.CloseInt ||
				currentState == GAME_STATE.FINISH)
			{
				return;
			}

			UIManager manager = UIManager.instance;
			if (manager == null ||
				manager.HasActiveView(EUIType.RewardView) ||
				HasActiveSuccessView() ||
				!canTriggerReward)
			{
				return;
			}

			StartCoroutine(ShowRewardViewDelayed(Framework2Host.Settings.inGameRewardTriggerDelaySeconds));
		}

		[IteratorStateMachine(typeof(_003CShowRewardViewDelayed_003Ed__144))]
		private IEnumerator ShowRewardViewDelayed(float delay)
		{
			hostPendingPairRewards++;
			string analyticsOpportunity = MahjongProductAnalytics.Begin("f2_pair_reward", "level", currentLv, "delay_seconds", delay, "pairs", totalEliminationCount);
			try
			{
			int rewardLevel = currentLv;
			yield return new WaitForSeconds(delay);
			if (currentState == GAME_STATE.FINISH ||
				currentLv <= GameDefines.CloseInt || currentLv != rewardLevel || !canTriggerReward)
			{
				MahjongProductAnalytics.Track("f2_pair_reward_cancelled", "operation_id", analyticsOpportunity, "reason", "level_or_reward_state_changed", "level", currentLv);
				yield break;
			}

			UIManager manager = UIManager.instance;
			if (manager == null ||
				manager.HasActiveView(EUIType.RewardView) ||
				HasActiveSuccessView() || !manager.IsGameplayOnlyUIActive())
			{
				MahjongProductAnalytics.Track("f2_pair_reward_cancelled", "operation_id", analyticsOpportunity, "reason", "blocking_view");
				yield break;
			}
			MahjongProductAnalytics.Track("f2_pair_reward_present_request", "operation_id", analyticsOpportunity, "level", currentLv);
			manager.ShowView(EUIType.RewardView);
			}
			finally { hostPendingPairRewards--; }
		}

		public void RestartLevelTimer()
		{
			ResetLevelTimerState();
			StartLevelTimer();
		}

		private void ManageTimerBasedOnUI()
		{
			if (Framework2Host.Settings.useMahjongInGameRewardTrigger) return;
			if (currentLv <= GameDefines.CloseInt || currentState == GAME_STATE.FINISH)
			{
				return;
			}

			UIManager manager = UIManager.instance != null ? UIManager.instance : uiManager;
			if (manager == null)
			{
				return;
			}

			if (!manager.IsGameplayOnlyUIActive())
			{
				if (isLevelTimerActive && !isLevelTimerPausedByUI)
				{
					PauseLevelTimerByUI();
				}
			}
			else if (isLevelTimerPausedByUI)
			{
				ResumeLevelTimerByUI();
			}
			else if (!isLevelTimerActive && levelTimer == null && !canTriggerReward)
			{
				StartLevelTimer();
			}
		}

		public void ResetPouringTimerState()
		{
			pouringTimer?.Close();
			isPouringTimerActive = false;
		}

		public void StopPouringTimer()
		{
			pouringTimer?.Close();
		}

		public void PausePouringTimer()
		{
			if (pouringTimer != null && isPouringTimerActive)
			{
				pouringTimer.Pause();
			}
		}

		public void ResumePouringTimer()
		{
			if (pouringTimer != null && isPouringTimerActive)
			{
				pouringTimer.Start();
			}
		}

		public void ResetPouringTimer()
		{
			pouringTimer?.Close();
			isPouringTimerActive = false;
		}

		public void ShowFinishLevel()
		{
			if (currentState == GAME_STATE.FINISH)
			{
				StartCoroutine(ShowFinishLevelIE());
			}
		}

		[IteratorStateMachine(typeof(_003CShowFinishLevelIE_003Ed__153))]
		private IEnumerator ShowFinishLevelIE()
		{
			return ShowFinishLevelRoutine();
		}

		[IteratorStateMachine(typeof(_003CProcessUIQueue_003Ed__154))]
		private IEnumerator ProcessUIQueue()
		{
			return ProcessUIQueueRoutine();
		}

		private void PrepareLevelCompletion()
		{
			int completedStage = currentStage;
			lastCompletedLevel = currentLv;
			if (currentLv < GetTotalLevels())
			{
				currentLv++;
				PlayerPrefs.SetInt(FacadePlayerPrefExtend.currentLevel, currentLv);
			}
			else
			{
				currentLv = PlayerPrefs.GetInt(FacadePlayerPrefExtend.currentLevel);
			}
			OnLevelChanged?.Invoke();
			OnCoinChanged?.Invoke();
			Framework2Host.CurrencyChanged();
			CheckDeferredStageProgressOnLevelComplete(completedStage);
		}

		private bool HasActiveSuccessView()
		{
			UIManager manager = UIManager.instance != null ? UIManager.instance : uiManager;
			return manager != null &&
				(manager.HasActiveView(EUIType.ChallangeSuccessView) ||
				 manager.HasActiveView(EUIType.LuckySuccessView));
		}

		private bool IsCompletionFlowBlockedBySpecialView()
		{
			UIManager manager = UIManager.instance != null ? UIManager.instance : uiManager;
			bool uiBlocked = manager != null &&
				(manager.HasActiveView(EUIType.LuckySpinView) ||
				 manager.HasActiveView(EUIType.BonusWheelView) ||
				 manager.HasActiveView(EUIType.BloatView));
			bool legacyBlocked = GameApp.viewManager != null &&
				GameApp.viewManager.IsOpen((int)ViewType.WithDrawSuc);
			return uiBlocked || legacyBlocked;
		}

		private int GetLastCompletedLevel()
		{
			if (lastCompletedLevel > 0)
			{
				return lastCompletedLevel;
			}
			return Mathf.Max(1, currentLv - 1);
		}

		private bool IsLuckySuccessLevel(int completedLevel)
		{
			if (completedLevel <= 0)
			{
				return false;
			}
			UserDataManager dataManager = UserDataManager.Instance;
			return dataManager != null && dataManager.IsLuckyRewardLevel(completedLevel);
		}

		private EUIType GetSuccessViewTypeForCompletedLevel(int completedLevel)
		{
			if (GameDefines.ifIAA)
			{
				return EUIType.ChallangeSuccessView;
			}
			return IsLuckySuccessLevel(completedLevel)
				? EUIType.LuckySuccessView
				: EUIType.ChallangeSuccessView;
		}

		private void ShowSuccessView()
		{
			ShowSuccessView(GetSuccessViewTypeForCompletedLevel(GetLastCompletedLevel()));
		}

		private void ShowSuccessView(EUIType successViewType)
		{
			StartCoroutine(ShowSuccessViewCoroutine(successViewType));
		}

		[IteratorStateMachine(typeof(_003CShowSuccessViewCoroutine_003Ed__163))]
		private IEnumerator ShowSuccessViewCoroutine(EUIType successViewType)
		{
			return ShowSuccessViewRoutine(successViewType);
		}

		private IEnumerator ShowFinishLevelRoutine()
		{
			PrepareLevelCompletion();

			UIManager manager = UIManager.instance != null ? UIManager.instance : uiManager;
			if (manager == null)
			{
				RecoverFromMissingCompletionView(
					GetSuccessViewTypeForCompletedLevel(
						GetLastCompletedLevel()));
				yield break;
			}

			if (!manager.HasActiveView(EUIType.RewardView))
			{
				ShowSuccessView();
				yield break;
			}

			if (pendingUIQueue == null)
			{
				pendingUIQueue = new Queue<EUIType>();
			}
			pendingUIQueue.Enqueue(GetSuccessViewTypeForCompletedLevel(GetLastCompletedLevel()));
			if (!isProcessingUIQueue)
			{
				StartCoroutine(ProcessUIQueue());
			}
		}

		private IEnumerator ProcessUIQueueRoutine()
		{
			if (isProcessingUIQueue)
			{
				yield break;
			}

			isProcessingUIQueue = true;
			UIManager manager = UIManager.instance != null ? UIManager.instance : uiManager;
			try
			{
				while (manager != null && pendingUIQueue != null &&
					pendingUIQueue.Count > 0)
				{
					EUIType nextUI = pendingUIQueue.Peek();
					while (manager.HasActiveView(EUIType.RewardView) ||
						HasActiveSuccessView())
					{
						yield return new WaitForSecondsRealtime(0.1f);
					}

					yield return null;
					pendingUIQueue.Dequeue();
					if (nextUI != EUIType.LuckySuccessView &&
						nextUI != EUIType.ChallangeSuccessView)
					{
						continue;
					}

					// Run the show coroutine inline. The old fire-and-forget path then
					// waited forever for a view that could have failed to load.
					yield return ShowSuccessViewCoroutine(nextUI);
					if (!manager.HasActiveView(nextUI))
					{
						continue;
					}

					while (manager.HasActiveView(nextUI))
					{
						yield return new WaitForSecondsRealtime(0.1f);
					}
				}
			}
			finally
			{
				isProcessingUIQueue = false;
			}
		}

		private IEnumerator ShowSuccessViewRoutine(EUIType successViewType)
		{
			if (HasActiveSuccessView())
			{
				yield break;
			}

			// Let the just-updated HUD/layout settle for one frame, then show the
			// already-preloaded completion view. The previous one-second fixed wait
			// produced the empty screen seen after every completed level.
			yield return null;
			while (IsCompletionFlowBlockedBySpecialView())
			{
				yield return new WaitForSecondsRealtime(0.1f);
			}
			if (HasActiveSuccessView())
			{
				yield break;
			}

			int completedLevel = GetLastCompletedLevel();
			if (!GameDefines.ifIAA && currentLv <= GameDefines.CloseInt)
			{
				yield break;
			}

			UIManager manager = UIManager.instance != null ? UIManager.instance : uiManager;
			if (manager == null)
			{
				RecoverFromMissingCompletionView(successViewType);
				yield break;
			}

			Task<BaseView> showTask = manager.ShowViewAsync(
				successViewType,
				completedLevel);
			float showDeadline = Time.realtimeSinceStartup + 15f;
			while (!showTask.IsCompleted)
			{
				if (Time.realtimeSinceStartup >= showDeadline)
				{
					manager.CloseView(successViewType);
					Debug.LogError(
						"Timed out while loading completion view " +
						EUITypeContract.GetStableName(successViewType) + ".");
					RecoverFromMissingCompletionView(successViewType);
					yield break;
				}
				yield return null;
			}

			BaseView shownView = null;
			if (!showTask.IsCanceled && !showTask.IsFaulted)
			{
				shownView = showTask.Result;
			}
			if (shownView == null || !manager.HasActiveView(successViewType))
			{
				Debug.LogError(
					"Completion view failed to open: " +
					EUITypeContract.GetStableName(successViewType) + ".");
				RecoverFromMissingCompletionView(successViewType);
			}
		}

		private void RecoverFromMissingCompletionView(EUIType successViewType)
		{
			if (currentState != GAME_STATE.FINISH ||
				HasActiveSuccessView() ||
				IsCompletionFlowBlockedBySpecialView())
			{
				return;
			}

			Debug.LogWarning(
				"Skipping unavailable completion view " +
				EUITypeContract.GetStableName(successViewType) +
				" so the level flow cannot remain locked.");
			NextLevel();
		}

		private void UpdateBoosterState()
		{
			UIManager manager = UIManager.instance != null ? UIManager.instance : uiManager;
			if (manager == null)
			{
				StopLevelTimer();
				isLevelTimerActive = false;
				canTriggerReward = false;
				isLevelTimerPausedByUI = false;
				return;
			}

			if (GameDefines.ifIAA)
			{
				GameView view = manager.GetView<GameView>(EUIType.GameView);
				if (view != null)
				{
					if (currentLv == 1)
					{
						view.HideBooster();
						view.HideWithdraw();
					}
					else
					{
						view.ShowBooster();
						view.ShowWithdraw();
					}
				}
			}
			else
			{
				GamePlayerView view = manager.GetView<GamePlayerView>(EUIType.GamePlayerView);
				if (view != null)
				{
					if (currentLv == 1)
					{
						view.HideBooster();
						view.HideWithdraw();
					}
					else
					{
						view.ShowBooster();
						view.ShowWithdraw();
					}
				}
			}
		}

		public void OnHintBottleAdded()
		{
			if (currentAddBottleCount > 0)
			{
				currentAddBottleCount--;
				SaveAddBottleData();
				RefreshBottleCountUI();
			}
		}

		public void RewardHintBottle(int amount = 1)
		{
			currentAddBottleCount += amount;
			SaveAddBottleData();
			RefreshBottleCountUI();
		}

		public void ProcessClear()
		{
			if (currentClear > 0)
			{
				currentClear--;
				SaveClearData();
				RefreshClearInfoUI();
			}
		}

		public void RewardClear(int amount = 1)
		{
			currentClear += amount;
			SaveClearData();
			RefreshClearInfoUI();
		}

		public void RewardUndo(int amount = 1)
		{
			currentUndo += amount;
			SaveUndoData();
			RefreshUndoInfoUI();
		}

		public bool UseUndo()
		{
			if (currentUndo > 0)
			{
				currentUndo--;
				SaveUndoData();
				RefreshUndoInfoUI();
				return true;
			}

			string message = "No operation steps to undo";
			if (LocalizationManager.Instance != null)
			{
				string localized = LocalizationManager.Instance.GetText("1034");
				if (!string.IsNullOrEmpty(localized) && localized != "1034")
				{
					message = localized;
				}
			}
			UIManager.instance?.ShowView(EUIType.NoticeView, message);
			return false;
		}

		public bool UseClear()
		{
			bool canUse = currentClear > 0;
			if (canUse)
			{
				currentClear--;
				SaveClearData();
				RefreshClearInfoUI();
			}
			return canUse;
		}

		private void RefreshBottleCountUI()
		{
			UIManager manager = UIManager.instance != null ? UIManager.instance : uiManager;
			if (manager == null)
			{
				return;
			}
			if (GameDefines.ifIAA)
			{
				manager.GetView<GameView>(EUIType.GameView)?.RefreshBottleCount();
			}
			else
			{
				manager.GetView<GamePlayerView>(EUIType.GamePlayerView)?.RefreshBottleCount();
			}
		}

		private void RefreshClearInfoUI()
		{
			UIManager manager = UIManager.instance != null ? UIManager.instance : uiManager;
			if (manager == null)
			{
				return;
			}
			if (GameDefines.ifIAA)
			{
				manager.GetView<GameView>(EUIType.GameView)?.RefreshClearInfo();
			}
			else
			{
				manager.GetView<GamePlayerView>(EUIType.GamePlayerView)?.RefreshClearInfo();
			}
		}

		private void RefreshUndoInfoUI()
		{
			UIManager manager = UIManager.instance != null ? UIManager.instance : uiManager;
			if (manager == null)
			{
				return;
			}
			if (GameDefines.ifIAA)
			{
				manager.GetView<GameView>(EUIType.GameView)?.RefreshUndoInfo();
			}
			else
			{
				manager.GetView<GamePlayerView>(EUIType.GamePlayerView)?.RefreshUndoInfo();
			}
		}

		public void AddUndo()
		{
		}

		public void ProcessUndo()
		{
		}

		private void SaveEliminationCount()
		{
			PlayerPrefs.SetInt(
				FacadePlayerPrefExtend.currentEliminationCount,
				currentEliminationCount);
			PlayerPrefs.SetInt(
				FacadePlayerPrefExtend.totalEliminationCount,
				totalEliminationCount);
			PlayerPrefs.Save();
		}

		private void CheckLuckySpinCondition()
		{
			if (currentEliminationCount >=
				GameDefines.LuckySpin_Elimination_Interval)
			{
				TriggerLuckySpin();
			}
		}

		private void TriggerLuckySpin()
		{
			MahjongProductAnalytics.Track("f2_lucky_spin_threshold", "pairs", currentEliminationCount, "total_pairs", totalEliminationCount, "threshold", GameDefines.LuckySpin_Elimination_Interval, "level", currentLv);
			UIManager manager = UIManager.instance;
			if (manager == null)
			{
				currentEliminationCount = 0;
				SaveEliminationCount();
				return;
			}
			if (manager.HasActiveView(EUIType.RewardView))
			{
				MahjongProductAnalytics.Track("f2_lucky_spin_blocked", "reason", "in_game_reward_active");
				return;
			}

			MahjongProductAnalytics.Track("f2_lucky_spin_requested", "pairs_before_reset", currentEliminationCount, "pairs_after_reset", 0);
			currentEliminationCount = 0;
			SaveEliminationCount();
			FacadeEffectExtend.PlayCongratulationEffectHandle?.Invoke();
			manager.ShowView(EUIType.LuckySpinView);
		}

		public void ResetEliminationCount()
		{
			currentEliminationCount = 0;
			SaveEliminationCount();
		}

		public int GetEliminationCountForTesting()
		{
			return currentEliminationCount;
		}

		public void AddCoin(float moreCoin)
		{
			AddCoin(moreCoin, currentLv);
		}

		public void AddCoin(float moreCoin, int rewardLevel)
		{
			if (moreCoin == 0f ||
				(moreCoin > 0f && !CanGrantCoinRewardForLevel(rewardLevel)))
			{
				MahjongProductAnalytics.Track("f2_coin_reward_suppressed", "requested_amount", moreCoin, "reward_level", rewardLevel, "reason", moreCoin == 0f ? "zero_amount" : "early_level_rule");
				return;
			}
			float analyticsBefore = currentCoin;
			currentCoin = NormalizeRegularCoin(currentCoin + moreCoin);
			MahjongProductAnalytics.Track("f2_resource_change", "resource", "main_cash", "before", analyticsBefore, "requested_delta", moreCoin, "actual_delta", currentCoin - analyticsBefore, "after", currentCoin, "reward_level", rewardLevel, "operation", "add");
			SaveCoinData();
			UpdateCoinUI();
			OnCoinChanged?.Invoke();
			Framework2Host.CurrencyChanged();
		}

		public void AddCoinTem(float moreCoin)
		{
			float analyticsBefore = currentCoin;
			currentCoin = NormalizeRegularCoin(
				currentCoin + moreCoin);
			MahjongProductAnalytics.Track("f2_resource_change", "resource", "main_cash", "before", analyticsBefore, "requested_delta", moreCoin, "actual_delta", currentCoin - analyticsBefore, "after", currentCoin, "operation", "direct_add");
			SaveCoinData();
			UpdateCoinUI();
			OnCoinChanged?.Invoke();
			Framework2Host.CurrencyChanged();
		}

		public void AddGems(float addGems)
		{
			if (addGems <= 0f)
			{
				return;
			}
			MahjongProductAnalytics.Track("f2_resource_change", "resource", "gem", "before", currentGems, "actual_delta", addGems, "after", currentGems + addGems, "operation", "add");
			currentGems += addGems;
			SaveGemsData();
			UpdateGemUI();
		}

		public void SubCoin(int subCoin)
		{
			if (subCoin <= 0)
			{
				return;
			}
			float analyticsBefore = currentCoin;
			currentCoin = NormalizeRegularCoin(
				currentCoin - subCoin);
			MahjongProductAnalytics.Track("f2_resource_change", "resource", "main_cash", "before", analyticsBefore, "requested_delta", -subCoin, "actual_delta", currentCoin - analyticsBefore, "after", currentCoin, "operation", "spend");
			SaveCoinData();
			UpdateCoinUI();
			OnCoinChanged?.Invoke();
			Framework2Host.CurrencyChanged();
		}

		public void SubGem(int subGem)
		{
			if (subGem <= 0)
			{
				return;
			}
			MahjongProductAnalytics.Track("f2_resource_change", "resource", "gem", "before", currentGems, "actual_delta", -subGem, "after", currentGems - subGem, "operation", "spend");
			currentGems -= subGem;
			SaveGemsData();
			UpdateGemUI();
		}

		public void UpdateCoinUI()
		{
			UIManager manager =
				uiManager != null ? uiManager : UIManager.instance;
			if (manager == null)
			{
				return;
			}

			if (GameDefines.ifIAA)
			{
				GameView view =
					manager.GetView<GameView>(EUIType.GameView);
				if (view == null)
				{
					return;
				}
				if (view.coinTxt != null)
				{
					view.coinTxt.text = RegionalChange(currentCoin);
				}
				view.UpdateWithdrawPrompt();
				return;
			}

			GamePlayerView playerView =
				manager.GetView<GamePlayerView>(
					EUIType.GamePlayerView);
			if (playerView == null)
			{
				return;
			}
			playerView.RefreshCoinInfo();
			playerView.UpdateWithdrawPrompt();
		}

		public void UpdateGemUI()
		{
			UIManager manager =
				uiManager != null ? uiManager : UIManager.instance;
			if (manager == null)
			{
				return;
			}

			if (GameDefines.ifIAA)
			{
				GameView view =
					manager.GetView<GameView>(EUIType.GameView);
				if (view != null && view.gemTxt != null)
				{
					view.gemTxt.text = currentGems.ToString();
				}
				return;
			}

			GamePlayerView playerView =
				manager.GetView<GamePlayerView>(
					EUIType.GamePlayerView);
			if (playerView != null && playerView.gemTxt != null)
			{
				playerView.gemTxt.text = currentGems.ToString();
			}
		}

		private string RegionalChange(float value)
		{
			return FormatRegionalValue((double)value);
		}

		internal string FormatRegionalValue(float value)
		{
			return FormatRegionalValue((double)value);
		}

		internal string FormatRegionalValue(double value)
		{
			if (double.IsNaN(value) || double.IsInfinity(value)) value = 0d;
			if (GameDefines.ifIAA) return Math.Ceiling(value).ToString(CultureInfo.InvariantCulture);
			return WaterSort.WaterSortCurrencyService.FormatBaseAmount(
				Framework2Host.Currency, (decimal)value);
		}

		internal string FormatRegionalNumber(float value) => FormatRegionalNumber((double)value);

		internal string FormatRegionalNumber(double value)
		{
			if (double.IsNaN(value) || double.IsInfinity(value)) value = 0d;
			if (GameDefines.ifIAA) return Math.Ceiling(value).ToString(CultureInfo.InvariantCulture);
			return WaterSort.WaterSortCurrencyService.FormatBaseAmount(
				Framework2Host.Currency, (decimal)value, false);
		}

		public double GetCoin()
		{
			return Math.Round(currentCoin, decimals, MidpointRounding.ToEven);
		}

		public bool CanGrantCoinRewardForCurrentLevel()
		{
			return CanGrantCoinRewardForLevel(currentLv);
		}

		public bool CanGrantCoinRewardForLevel(int level)
		{
			return (level < 1 || level > NoCoinRewardEndLevel) || GameDefines.ifIAA;
		}

		public float GetCoinRewardAmount(int gearType, int rewardLevel = -1)
		{
			if (rewardLevel < 1)
			{
				rewardLevel = currentLv;
			}
			return CanGrantCoinRewardForLevel(rewardLevel)
				? getResourceNum(1, gearType, 0)
				: 0f;
		}

		private float NormalizeRegularCoin(float coinValue)
		{
			return GameDefines.ifIAA ? coinValue : Mathf.Min(coinValue, NormalCoinHardCap);
		}

		public void SaveCoinData()
		{
			TrackStageAnalytics();
			currentCoin = NormalizeRegularCoin(currentCoin);
			PlayerPrefs.SetFloat(FacadePlayerPrefExtend.coin, currentCoin);
		}

		public float getCoinData()
		{
			return NormalizeRegularCoin(PlayerPrefs.GetFloat(FacadePlayerPrefExtend.coin));
		}

		public int getLevel()
		{
			return PlayerPrefs.GetInt(FacadePlayerPrefExtend.currentLevel);
		}

		public void SaveGemsData()
		{
			PlayerPrefs.SetFloat(
				FacadePlayerPrefExtend.gems, currentGems);
		}

		public void SaveUndoData()
		{
			PlayerPrefs.SetInt(FacadePlayerPrefExtend.undo, currentUndo);
		}

		public void SaveClearData()
		{
			PlayerPrefs.SetInt(FacadePlayerPrefExtend.ClearColorCount, currentClear);
		}

		public void SaveAddBottleData()
		{
			PlayerPrefs.SetInt(FacadePlayerPrefExtend.AddBottleCount, currentAddBottleCount);
		}

		public void SavePlayerName(string name)
		{
			PlayerPrefs.SetString(FacadePlayerPrefExtend.AddName, name);
		}

		public string getPlayerName()
		{
			return PlayerPrefs.GetString(FacadePlayerPrefExtend.AddName);
		}

		public void SavePlayerPhone(string phone)
		{
			PlayerPrefs.SetString(FacadePlayerPrefExtend.AddPhone, phone);
		}

		public string getPlayerPhone()
		{
			return PlayerPrefs.GetString(FacadePlayerPrefExtend.AddPhone);
		}

		public void SavePlayerEmail(string email)
		{
			PlayerPrefs.SetString(RegionalReceiverKey, email);
		}

		public string getPlayerEmail()
		{
			return PlayerPrefs.GetString(RegionalReceiverKey);
		}

		// Preserve the original US account key; regional providers never reuse another provider's account.
		private string RegionalReceiverKey => (Framework2Host.Currency?.Id ?? "US") == "US"
			? FacadePlayerPrefExtend.AddEmail
			: FacadePlayerPrefExtend.AddEmail + "." + Framework2Host.Currency.Id + "." +
				Framework2Host.Currency.PrimaryChannel.ToString(CultureInfo.InvariantCulture);

		public void SavePlayerIndex(int index)
		{
			PlayerPrefs.SetInt(FacadePlayerPrefExtend.AddIndex, index);
		}

		public int getPlayerIndex()
		{
			return PlayerPrefs.GetInt(FacadePlayerPrefExtend.AddIndex);
		}

		public int getPlayerStage()
		{
			return currentStage;
		}

		private void RegisterFacadeUserHandles()
		{
			UserWalletData cachedWallet =
				WithdrawApiService.GetCachedWallet();
			if (cachedWallet != null)
			{
				GameDefines.LuckyChestPlaceholderCoin =
					(float)Math.Max(cachedWallet.Balance, 0d);
			}

			FacadeUserExtend.GetUserCoinHandle = () => currentCoin;
			FacadeUserExtend.SetUserCoinHandle = value =>
			{
				currentCoin = NormalizeRegularCoin(value);
				SaveCoinData();
				UpdateCoinUI();
				OnCoinChanged?.Invoke();
			Framework2Host.CurrencyChanged();
			};
			FacadeUserExtend.AddUserCoinHandle = AddCoin;
			FacadeUserExtend.GetWithdrawalCoinHandle =
				() => GameDefines.LuckyChestPlaceholderCoin;
			FacadeUserExtend.AddWithdrawalCoinHandle =
				value => WithdrawApiService.AddWalletIncome(value);

			FacadeUserExtend.GetAddSpacePropNumHandle = () => currentAddBottleCount;
			FacadeUserExtend.SetAddSpacePropCountHandle = value =>
			{
				currentAddBottleCount = Mathf.Max(0, value);
				SaveAddBottleData();
				RefreshBottleCountUI();
			};
			FacadeUserExtend.AddAddSpacePropNumHandle = RewardHintBottle;

			FacadeUserExtend.GetClearPropNumHandle = () => currentClear;
			FacadeUserExtend.SetClearPropNumHandle = value =>
			{
				currentClear = Mathf.Max(0, value);
				SaveClearData();
				RefreshClearInfoUI();
			};
			FacadeUserExtend.AddClearPropNumHandle = RewardClear;

			FacadeUserExtend.GetHammerPropNumHandle = () => currentUndo;
			FacadeUserExtend.SetHammerPropNumHandle = value =>
			{
				currentUndo = Mathf.Max(0, value);
				SaveUndoData();
				RefreshUndoInfoUI();
			};
			FacadeUserExtend.AddHammerPropNumHandle = RewardUndo;
		}

		private void ClearFacadeUserHandles()
		{
			FacadeUserExtend.GetUserCoinHandle = null;
			FacadeUserExtend.SetUserCoinHandle = null;
			FacadeUserExtend.AddUserCoinHandle = null;
			FacadeUserExtend.GetWithdrawalCoinHandle = null;
			FacadeUserExtend.AddWithdrawalCoinHandle = null;
			FacadeUserExtend.GetAddSpacePropNumHandle = null;
			FacadeUserExtend.SetAddSpacePropCountHandle = null;
			FacadeUserExtend.AddAddSpacePropNumHandle = null;
			FacadeUserExtend.GetClearPropNumHandle = null;
			FacadeUserExtend.SetClearPropNumHandle = null;
			FacadeUserExtend.AddClearPropNumHandle = null;
			FacadeUserExtend.GetHammerPropNumHandle = null;
			FacadeUserExtend.SetHammerPropNumHandle = null;
			FacadeUserExtend.AddHammerPropNumHandle = null;
		}

		public void ReplayGame()
		{
			ResetHostRound();
			Framework2Host.NextRound();
		}

		public void NextLevel()
		{
			// Completing the source input/confirmation flow also completes its
			// target highlight, including clicks on the real button rather than
			// the guide proxy. Do not leave an orphan guide blocking the host.
			if (HasPendingHostStageTutorial) Tutorial.CancelPendingPresentation();
			// Only the host may release the board after the complete source chain,
			// including the wheel queued by settlement on the following frame.
			if (Framework2Host.Runtime == null || !Framework2Host.Runtime.OwnsCompletionFlow)
				ResetHostRound();
			Framework2Host.NextRound();
		}

		private void RefreshGameplayViewLocation()
		{
			UIManager manager = UIManager.instance != null ? UIManager.instance : uiManager;
			if (manager == null)
			{
				return;
			}
			if (GameDefines.ifIAA)
			{
				manager.GetView<GameView>(EUIType.GameView)?.GetLocation();
			}
			else
			{
				manager.GetView<GamePlayerView>(EUIType.GamePlayerView)?.GetLocation();
			}
		}

		public float getResourceNum(int type, int gearType, int gemType)
		{
			try
			{
				List<Dictionary<string, object>> rows = type == 1
					? UserDataManager.Instance.getAllGearData()
					: UserDataManager.Instance.getAllGemData();
				float currency = type == 1 ? currentCoin : currentGems;
				Dictionary<string, object> range = FindCurrencyRange(rows, currency);
				if (range == null)
				{
					return 0f;
				}

				if (type == 1)
				{
					if (gearType == 1)
					{
						return RandomRangeFromRow(range, "LuckMin", "LuckMax");
					}
					if (gearType == 2)
					{
						return RandomRangeFromRow(range, "EliminateMin", "EliminateMax");
					}
					return ParseResourceValue(range, "TurntableMax");
				}

				return gemType == 1
					? RandomRangeFromRow(range, "LuckMin", "LuckMax")
					: ParseResourceValue(range, "TurntableMin");
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				return NormalizeRegularCoin(PlayerPrefs.GetFloat(
					FacadePlayerPrefExtend.coin, 0f));
			}
		}

		private static Dictionary<string, object> FindCurrencyRange(
			List<Dictionary<string, object>> rows,
			float currency)
		{
			if (rows == null)
			{
				return null;
			}
			foreach (Dictionary<string, object> row in rows)
			{
				int minimum;
				int maximum;
				if (!TryParseResourceInt(row, "CurrencyMin", out minimum) ||
					!TryParseResourceInt(row, "CurrencyMax", out maximum))
				{
					// Gear/Gem CSV files preserve their original second
					// descriptive row. It is metadata, not a currency range.
					continue;
				}
				if (currency > minimum && currency <= maximum)
				{
					return row;
				}
			}
			return null;
		}

		private static bool TryParseResourceInt(
			Dictionary<string, object> row,
			string key,
			out int value)
		{
			value = 0;
			object raw;
			return row != null &&
				row.TryGetValue(key, out raw) &&
				int.TryParse(
					raw as string,
					System.Globalization.NumberStyles.Integer,
					System.Globalization.CultureInfo.InvariantCulture,
					out value);
		}

		private static float ParseResourceValue(
			Dictionary<string, object> row,
			string key)
		{
			object raw;
			if (row == null || !row.TryGetValue(key, out raw))
			{
				throw new KeyNotFoundException(key);
			}
			return float.Parse(raw as string,
				System.Globalization.NumberStyles.Float,
				System.Globalization.CultureInfo.InvariantCulture);
		}

		private static float RandomRangeFromRow(
			Dictionary<string, object> row,
			string firstKey,
			string secondKey)
		{
			return UnityEngine.Random.Range(
				ParseResourceValue(row, firstKey),
				ParseResourceValue(row, secondKey));
		}
	}
}
