using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	public class GamePlayerView : BaseView
	{
		[SerializeField] private MahjongSlide.Withdrawal.MahjongFramework2CurrencyHud currencyHud;
		private bool nativeProgressReferencesResolved;
		[Serializable]
		private class LevelMilestone
		{
			public int startLevel;

			public int endLevel;

			public int[] displayLevels;
		}

		private const int VisibleStageCount = 3;

		private const int LuckyVisibleCount = 7;

		private const int Stage3Id = 3;

		private const int Stage4Id = 4;

		private const int Stage5Id = 5;

		private const int AdMissionType = 4;

		private const int DefaultAdMissionTarget = 3;

		private const float DeferredStage3ProgressAnimationSpeed = 0.5f;

		[Header("Text")]
		private Text levelTxt;

		private Text _coinTxt;

		private Text _gemTxt;

		private Text remainUndoCount;

		private Text AddBottleTxtCount;

		private Text ClearCountTxtCount;

		private Text tipsTxt;

		private Text OverallCount;

		private Text Stage3Dec;

		private Text ProgressTxT;

		private Text LuckyCurCoin;

		[Header("Icons")]
		private Transform coinIconInBoard;

		private GameObject undoRWIcon;

		private GameObject addRwIcon;

		private GameObject clearRWIcon;

		[Header("UI Elements")]
		private GameObject booster;

		private GameObject tips;

		private GameObject MoneyRoot;

		private GameObject WithdrawTop;

		private GameObject MoneyIcon;

		private RectTransform Stage3Root;

		private RectTransform ProgressStateRoot;

		private RectTransform LuckyRoot;

		private RectTransform LuckyRootProgressStateRoot;

		private Image Progress;

		private Image Fill;

		private Image LuckyProgress;

		private GameObject LV;

		[Header("Buttons")]
		private Button replayBtn;

		private Button Clear;

		private Button UndoBtn;

		private Button AddBottle;

		private Button Setting;

		public Button CoinBoard;

		public Button GemBoard;

		private Button SlotBtn;

		private float barrageStartDelay;

		private float barrageInterval;

		private bool _isEventsBound;

		private float barrageTimer;

		private bool isBarrageActive;

		private Coroutine firstBarrageCoroutine;

		private GameObject Ng1;

		private GameObject Ng2;

		private GameObject Ng3;

		private GameObject Ng4;

		private GameObject Ng5;

		private GameObject Arrow1;

		private GameObject Arrow2;

		private GameObject Arrow3;

		private GameObject Arrow4;

		private GameObject StageIcon3;

		private GameObject WithdrawPrompt;

		private Image Process;

		private Text content;

		private Text num;

		[Header("Barrage")]
		private TextMarquee barrage;

		private List<LevelMilestone> milestones;

		private readonly WithdrawMissionStateItem[] progressStates;

		private readonly LuckyItemProgress[] luckyProgressStates;

		private bool hasDisplayedStage3ProgressValue;

		private bool hasDeferredStage3ProgressUpdate;

		private float displayedStage3ProgressValue;

		private float deferredStage3TargetProgressValue;

		private int displayedStage3StageId;

		private int deferredStage3StageId;

		private bool displayedStage3ShowProgressStates;

		private bool deferredStage3ShowProgressStates;

		private int displayedStage3ProgressStateTargetCount;

		private int deferredStage3ProgressStateTargetCount;
		private string stageMissionDescription;

		private GameObject promptIcon;

		private GameObject guidIcon;

		private GameManagerWZ GM => GameManagerWZ.instance;

		public Text coinTxt => _coinTxt;

		public Text gemTxt => _gemTxt;

		public GamePlayerView()
		{
			barrageStartDelay = 6f;
			barrageInterval = 20f;
			progressStates = new WithdrawMissionStateItem[VisibleStageCount];
			luckyProgressStates = new LuckyItemProgress[LuckyVisibleCount];
		}

		public override void Start()
		{
			ResolveReferencesIfNeeded();
			if (barrage != null)
			{
				barrage.gameObject.SetActive(false);
			}
		}

		public override void InitView()
		{
			ResolveReferencesIfNeeded();
			if (currencyHud != null)
			{
				GenerateMilestones();
				RefreshAllData();
				BindEvents();
				return;
			}
			FacadeGamePlayExtend.GetFlyObjGoldHandle -= GetFlyObjGold;
			FacadeGamePlayExtend.GetFlyObjGoldHandle += GetFlyObjGold;
			GenerateMilestones();
			RefreshAllData();
			BindEvents();
			StartBarrage();
		}

		public void GetLocation()
		{
			if (currencyHud != null) return;
			promptIcon = GameObject.Find(
				"Canvas-UIManger/GamePlayUI/Footer/btnHint");
			guidIcon = GameObject.Find(
				"Canvas-UIManger/GamePlayUI/Footer/btnLines");
		}

		public override void Update()
		{
			UpdateDeferredStage3ProgressAnimation();
			if (currencyHud != null) return;
			if (!isBarrageActive || barrage == null)
			{
				return;
			}
			if (IsGamePaused())
			{
				if (!barrage.IsPaused())
				{
					barrage.Pause();
				}
				return;
			}
			if (barrage.IsPaused())
			{
				barrage.Resume();
				barrageTimer = 0f;
			}
			if (!barrage.gameObject.activeInHierarchy)
			{
				barrageTimer += Time.deltaTime;
				if (barrageTimer >= barrageInterval)
				{
					barrageTimer = 0f;
					ShowBarrage();
				}
			}
		}

		private void StartBarrage()
		{
			if (barrage == null || GM == null)
			{
				return;
			}
			if (GM.currentLv >= 3)
			{
				isBarrageActive = true;
				barrageTimer = 0f;
				CancelFirstBarrageCoroutine();
				firstBarrageCoroutine =
					StartCoroutine(ShowFirstBarrageAfterDelay());
			}
		}

		private IEnumerator ShowFirstBarrageAfterDelay()
		{
			yield return new WaitForSeconds(barrageStartDelay);
			firstBarrageCoroutine = null;
			ShowFirstBarrage();
		}

		private void ShowFirstBarrage()
		{
			if (isBarrageActive && barrage != null)
			{
				ShowBarrage();
				barrageTimer = 0f;
			}
		}

		private void StopBarrage()
		{
			isBarrageActive = false;
			CancelFirstBarrageCoroutine();
			if (barrage != null)
			{
				barrage.Pause();
				barrage.gameObject.SetActive(false);
			}
		}

		private void CancelFirstBarrageCoroutine()
		{
			if (firstBarrageCoroutine == null)
			{
				return;
			}
			StopCoroutine(firstBarrageCoroutine);
			firstBarrageCoroutine = null;
		}

		private void ShowBarrage()
		{
			if (!isBarrageActive || barrage == null)
			{
				return;
			}

			string firstBarrage = GetBarrageText();
			string secondBarrage = GetBarrageText();
			barrage.onScrollComplete = null;
			barrage.onScrollComplete = () => barrageTimer = 0f;
			barrage.SetMarqueeText(firstBarrage, secondBarrage);
			barrage.gameObject.SetActive(true);
		}

		private bool IsGamePaused()
		{
			return GM != null &&
				GM.currentState == GameManagerWZ.GAME_STATE.FINISH;
		}

		private string GetBarrageText()
		{
			UserDataManager userData = UserDataManager.Instance;
			List<Dictionary<string, object>> barrageData =
				userData != null ? userData.getAllBarrageData() : null;
			if (barrageData == null || barrageData.Count == 0)
			{
				return string.Empty;
			}

			Dictionary<string, object> row =
				barrageData[UnityEngine.Random.Range(0, barrageData.Count)];
			if (row == null)
			{
				return string.Empty;
			}
			object playValue;
			object stageValue;
			string play =
				row.TryGetValue("play", out playValue)
					? playValue as string ?? string.Empty
					: string.Empty;
			int stage;
			if (!row.TryGetValue("Stage", out stageValue) ||
				!int.TryParse(
					stageValue as string,
					NumberStyles.Integer,
					CultureInfo.InvariantCulture,
					out stage))
			{
				stage = 0;
			}

			LocalizationManager localization = LocalizationManager.Instance;
			string format = localization != null
				? localization.GetText("2102")
				: "{0} just passed stage {1}";
			format = format.Replace("S{1}", "{1}");
			string formattedAmount = GM != null
				? GM.FormatRegionalValue((double)stage)
				: stage.ToString(CultureInfo.CurrentCulture);
			return string.Format(format, play, formattedAmount);
		}

		private int GetTotalLevels()
		{
			return GameDefines.SmallLevelGroupEnd - 1;
		}

		private void GenerateMilestones()
		{
			milestones = new List<LevelMilestone>
			{
				new LevelMilestone
				{
					startLevel = 1,
					endLevel = 4,
					displayLevels = new[] { 1, 2, 3, 4, 8 }
				},
				new LevelMilestone
				{
					startLevel = 5,
					endLevel = 8,
					displayLevels = new[] { 4, 5, 6, 7, 8 }
				}
			};
		}

		private void RefreshAllData()
		{
			if (currencyHud != null)
			{
				RefreshLevelInfo();
				RefreshCoinInfo();
				RefreshGemInfo();
				RefreshProgressInfo();
				RefreshTipsByStageAndLevel();
				UpdateMoneyDisplay();
				UpdateWithdrawPrompt();
				RefreshLuckyProgress();
				return;
			}
			GetLocation();
			RefreshLevelInfo();
			RefreshCoinInfo();
			RefreshGemInfo();
			RefreshUndoInfo();
			RefreshBottleCount();
			RefreshClearInfo();
			RefreshProgressInfo();
			RefreshTipsByStageAndLevel();
			UpdateMoneyDisplay();
			UpdateWithdrawPrompt();
			RefreshLuckyProgress();
		}

		public void RefreshStageUI()
		{
			RefreshAllData();
			if (GM != null) Framework2BusinessAnalytics.TaskPresentation("gameplay_hud", GM.currentStage, WithdrawalTaskPresentation.Read(GM, GM.currentStage), (WithdrawPrompt != null && WithdrawPrompt.activeInHierarchy) || (Stage3Root != null && Stage3Root.gameObject.activeInHierarchy));
		}

		internal override void RefreshRuntimeGmPresentation()
		{
			RefreshStageUI();
			if (barrage != null && barrage.gameObject.activeInHierarchy)
			{
				ShowBarrage();
			}
		}

		private void RefreshTipsByStageAndLevel()
		{
			if (GM == null || tipsTxt == null)
			{
				return;
			}

			int currentLevel = GM.currentLv;
			string objectives =
				GetLevelObjectivesByStage(GM.currentStage);
			if (tips != null)
			{
				tips.SetActive(false);
			}
			tipsTxt.text = !string.IsNullOrEmpty(objectives)
				? objectives
				: string.Format(
					GetText(
						"1029",
						"Clear Level{0}<color=#E7534B> Withdraw all</color>"),
					currentLevel);
		}

		private string GetLevelObjectivesByStage(int stageId)
		{
			if (GM == null)
			{
				return null;
			}

			Dictionary<string, object> stageData =
				GM.GetStageData(stageId);
			if (stageData == null)
			{
				return null;
			}

			int targetValue = 0;
			object value;
			if (stageData.TryGetValue("Value", out value))
			{
				int.TryParse(
					value as string,
					NumberStyles.Integer,
					CultureInfo.InvariantCulture,
					out targetValue);
			}

			object objective;
			if (stageData.TryGetValue(
					"LevelObjectives", out objective) &&
				objective is string)
			{
				string objectiveKey = (string)objective;
				if (!string.IsNullOrEmpty(objectiveKey))
				{
					return string.Format(
						GetText(objectiveKey, objectiveKey),
						targetValue);
				}
			}

			int type;
			if (!GM.TryGetStageRequirement(stageId, out type, out targetValue)) return null;
			switch (type)
			{
				case 1:
					return string.Format(
						GetText("1029", "Clear Level{0}<color=#E7534B> Withdraw all</color>"),
						targetValue);
				case 2:
					return string.Format(
						GetText("2097", "Earn {0} more to withdraw {1}"),
						GM.FormatRegionalValue(Math.Max(targetValue - GM.GetCoin(), 0d)),
						GM.FormatRegionalValue((double)targetValue));
				case 3:
					return string.Format(
						GetText("2098", "Check in for {0} more days withdraw all cash"),
						Math.Max(targetValue - GM.stage5CurrentCycle, 0));
				case AdMissionType:
					return string.Format(
						GetText("2148", "Watch {0} ads to withdraw all cash"),
						targetValue);
				default:
					return null;
			}
		}

		private void ResolveReferencesIfNeeded()
		{
			if (currencyHud != null)
			{
				_coinTxt = currencyHud.CashText;
				_gemTxt = currencyHud.GemText;
				CoinBoard = currencyHud.CashButton;
				GemBoard = currencyHud.GemButton;
				MoneyRoot = currencyHud.GemRoot;
				Setting = currencyHud.SettingsButton;
				ResolveNativeProgressReferences();
				return;
			}
			levelTxt = levelTxt != null ? levelTxt : FindText("Area/TopButtons/LV/levelText");
			_coinTxt = _coinTxt != null ? _coinTxt : FindText("Area/TopButtons/CurMoney/Bg1/CoinTxt");
			_gemTxt = _gemTxt != null ? _gemTxt : FindText("Area/TopButtons/GemMoney/Bg1/GemTxt");
			remainUndoCount = remainUndoCount != null
				? remainUndoCount
				: FindText("Area/BoosterButtons/UndoBtn/UndoRWIcon/Undocount");
			AddBottleTxtCount = AddBottleTxtCount != null
				? AddBottleTxtCount
				: FindText("Area/BoosterButtons/AddBottleBtn/AddBottleRWIcon/Addcount");
			ClearCountTxtCount = ClearCountTxtCount != null
				? ClearCountTxtCount
				: FindText("Area/BoosterButtons/ClearBtn/ClearRWIcon/Clearcount");
			tipsTxt = tipsTxt != null ? tipsTxt : FindText("Area/TopButtons/Tips/tipsTxt");
			OverallCount = OverallCount != null
				? OverallCount
				: FindText("Area/TopButtons/Stage3Root/OverallProgress/OverallCount");
			Stage3Dec = Stage3Dec != null
				? Stage3Dec
				: FindText("Area/TopButtons/Stage3Root/bg/Stage3Dec");
			ProgressTxT = ProgressTxT != null
				? ProgressTxT
				: FindText("Area/TopButtons/Stage3Root/BG/ProgressTxT");
			LuckyCurCoin = LuckyCurCoin != null
				? LuckyCurCoin
				: FindText("Area/TopButtons/LuckyRoot/bg/CurCoin");

			GameObject coinIconObject =
				FindObject("Area/TopButtons/CurMoney/Bg1/MoneyIcon");
			coinIconInBoard = coinIconInBoard != null
				? coinIconInBoard
				: coinIconObject != null ? coinIconObject.transform : null;
			undoRWIcon = undoRWIcon != null
				? undoRWIcon
				: FindObject("Area/BoosterButtons/UndoBtn/UndoRWIcon");
			addRwIcon = addRwIcon != null
				? addRwIcon
				: FindObject("Area/BoosterButtons/AddBottleBtn/AddBottleRWIcon");
			clearRWIcon = clearRWIcon != null
				? clearRWIcon
				: FindObject("Area/BoosterButtons/ClearBtn/ClearRWIcon");
			booster = booster != null ? booster : FindObject("Area/BoosterButtons");
			tips = tips != null ? tips : FindObject("Area/TopButtons/Tips");
			MoneyRoot = MoneyRoot != null
				? MoneyRoot
				: FindObject("Area/TopButtons/GemMoney");
			WithdrawTop = WithdrawTop != null
				? WithdrawTop
				: FindObject("Area/TopButtons/WithdrawTop");
			MoneyIcon = MoneyIcon != null
				? MoneyIcon
				: FindObject("Area/TopButtons/CurMoney/Bg1/MoneyIcon/MoneyIcon");
			if (Stage3Root == null)
			{
				GameObject target =
					FindObject("Area/TopButtons/Stage3Root");
				Stage3Root = target != null
					? target.GetComponent<RectTransform>()
					: null;
			}
			if (ProgressStateRoot == null)
			{
				GameObject target =
					FindObject("Area/TopButtons/Stage3Root/ProgressStateRoot");
				ProgressStateRoot = target != null
					? target.GetComponent<RectTransform>()
					: null;
			}
			if (LuckyRoot == null)
			{
				GameObject target =
					FindObject("Area/TopButtons/LuckyRoot");
				LuckyRoot = target != null
					? target.GetComponent<RectTransform>()
					: null;
			}
			if (LuckyRootProgressStateRoot == null)
			{
				GameObject target =
					FindObject(
						"Area/TopButtons/LuckyRoot/LuckyRootProgressStateRoot");
				LuckyRootProgressStateRoot = target != null
					? target.GetComponent<RectTransform>()
					: null;
			}
			Progress = Progress != null
				? Progress
				: FindImage("Area/TopButtons/Stage3Root/BG/Progress");
			Fill = Fill != null
				? Fill
				: FindImage("Area/TopButtons/Stage3Root/OverallProgress/Fill");
			LuckyProgress = LuckyProgress != null
				? LuckyProgress
				: FindImage("Area/TopButtons/LuckyRoot/BG/LuckyProgress");
			LV = LV != null ? LV : FindObject("Area/TopButtons/LV");

			replayBtn = replayBtn != null ? replayBtn : FindButton("Area/TopButtons/Replay");
			Clear = Clear != null ? Clear : FindButton("Area/BoosterButtons/ClearBtn");
			UndoBtn = UndoBtn != null ? UndoBtn : FindButton("Area/BoosterButtons/UndoBtn");
			AddBottle = AddBottle != null
				? AddBottle
				: FindButton("Area/BoosterButtons/AddBottleBtn");
			Setting = Setting != null ? Setting : FindButton("Area/TopButtons/Setting");
			CoinBoard = CoinBoard != null
				? CoinBoard
				: FindButton("Area/TopButtons/CurMoney/CoinBtn");
			GemBoard = GemBoard != null
				? GemBoard
				: FindButton("Area/TopButtons/GemMoney/GemBtn");
			SlotBtn = SlotBtn != null ? SlotBtn : FindButton("Area/TopButtons/SlotBtn");

			Ng1 = Ng1 != null
				? Ng1
				: FindObject("Area/TopButtons/WithdrawTop/bg/NG1");
			Ng2 = Ng2 != null
				? Ng2
				: FindObject("Area/TopButtons/WithdrawTop/bg/NG2");
			Ng3 = Ng3 != null
				? Ng3
				: FindObject("Area/TopButtons/WithdrawTop/bg/NG3");
			Ng4 = Ng4 != null
				? Ng4
				: FindObject("Area/TopButtons/WithdrawTop/bg/NG4");
			Ng5 = Ng5 != null
				? Ng5
				: FindObject("Area/TopButtons/WithdrawTop/bg/NG5");
			Arrow1 = Arrow1 != null
				? Arrow1
				: FindObject("Area/TopButtons/WithdrawTop/bg/Arrow1");
			Arrow2 = Arrow2 != null
				? Arrow2
				: FindObject("Area/TopButtons/WithdrawTop/bg/Arrow2");
			Arrow3 = Arrow3 != null
				? Arrow3
				: FindObject("Area/TopButtons/WithdrawTop/bg/Arrow3");
			Arrow4 = Arrow4 != null
				? Arrow4
				: FindObject("Area/TopButtons/WithdrawTop/bg/Arrow4");
			StageIcon3 = StageIcon3 != null
				? StageIcon3
				: FindObject("Area/TopButtons/WithdrawPrompt/bg/icon");
			WithdrawPrompt = WithdrawPrompt != null
				? WithdrawPrompt
				: FindObject("Area/TopButtons/WithdrawPrompt");
			Process = Process != null
				? Process
				: FindImage(
					"Area/TopButtons/WithdrawPrompt/bg/Process/Fill Area/Fill");
			content = content != null
				? content
				: FindText("Area/TopButtons/WithdrawPrompt/bg/content");
			num = num != null
				? num
				: FindText(
					"Area/TopButtons/WithdrawPrompt/bg/Process/Fill Area/num");

			if (barrage == null)
			{
				GameObject barrageObject = FindObject("Area/MarquBG/Marque");
				barrage = barrageObject != null
					? barrageObject.GetComponent<TextMarquee>()
					: FindChildUtility.FindChild<TextMarquee>(gameObject, "Marque");
			}
			promptIcon = promptIcon != null
				? promptIcon
				: FindChildUtility.FindChildIncludeInactive(gameObject, "promptIcon");
			guidIcon = guidIcon != null
				? guidIcon
				: FindChildUtility.FindChildIncludeInactive(gameObject, "guidIcon");

			if (ProgressStateRoot != null)
			{
				for (int i = 0;
					i < VisibleStageCount &&
					i < ProgressStateRoot.childCount;
					i++)
				{
					progressStates[i] = GetOrAddStateItem(
						ProgressStateRoot.GetChild(i).gameObject);
				}
			}
			if (LuckyRootProgressStateRoot != null)
			{
				for (int i = 0;
					i < LuckyVisibleCount &&
					i < LuckyRootProgressStateRoot.childCount;
					i++)
				{
					luckyProgressStates[i] = GetOrAddLuckyItemProgress(
						LuckyRootProgressStateRoot.GetChild(i).gameObject);
				}
			}
		}

		private void ResolveNativeProgressReferences()
		{
			if (nativeProgressReferencesResolved || currencyHud.Progress == null) return;
			var bindings = currencyHud.Progress;
			LV = bindings.levelRoot;
			levelTxt = bindings.levelText;
			tips = bindings.tipsRoot;
			tipsTxt = bindings.tipsText;
			WithdrawTop = bindings.withdrawTop;
			Ng1 = bindings.levelMarkers[0];
			Ng2 = bindings.levelMarkers[1];
			Ng3 = bindings.levelMarkers[2];
			Ng4 = bindings.levelMarkers[3];
			Ng5 = bindings.levelMarkers[4];
			Arrow1 = bindings.levelArrows[0];
			Arrow2 = bindings.levelArrows[1];
			Arrow3 = bindings.levelArrows[2];
			Arrow4 = bindings.levelArrows[3];
			WithdrawPrompt = bindings.withdrawPrompt;
			StageIcon3 = bindings.stageIcon;
			Process = bindings.withdrawalFill;
			content = bindings.withdrawalDescription;
			num = bindings.withdrawalCount;
			Stage3Root = bindings.stageRoot;
			ProgressStateRoot = bindings.stageStateRoot;
			Stage3Dec = bindings.stageDescription;
			ProgressTxT = bindings.stageProgressText;
			OverallCount = bindings.stageOverallCount;
			Progress = bindings.stageFill;
			Fill = bindings.stageOverallFill;
			LuckyRoot = bindings.luckyRoot;
			LuckyRootProgressStateRoot = bindings.luckyStateRoot;
			LuckyProgress = bindings.luckyFill;
			LuckyCurCoin = bindings.luckyCurrentCoin;
			Array.Copy(bindings.stageStates, progressStates,
				Math.Min(bindings.stageStates.Length, progressStates.Length));
			Array.Copy(bindings.luckyStates, luckyProgressStates,
				Math.Min(bindings.luckyStates.Length, luckyProgressStates.Length));
			nativeProgressReferencesResolved = true;
		}

		private void RefreshLevelInfo()
		{
			if (levelTxt == null || GM == null)
			{
				return;
			}
			string format = GetText("1004", "Level {0}");
			try
			{
				levelTxt.text = string.Format(format, GM.currentLv);
			}
			catch (FormatException)
			{
				levelTxt.text = "Level " + GM.currentLv;
			}
		}

		private GameObject FindObject(string path)
		{
			return FindChildUtility.FindChildByPath(gameObject, path);
		}

		private Text FindText(string path)
		{
			GameObject target = FindObject(path);
			return target != null ? target.GetComponent<Text>() : null;
		}

		private Button FindButton(string path)
		{
			GameObject target = FindObject(path);
			return target != null ? target.GetComponent<Button>() : null;
		}

		private Image FindImage(string path)
		{
			GameObject target = FindObject(path);
			return target != null ? target.GetComponent<Image>() : null;
		}

		private void RefreshProgressInfo()
		{
			if (GM == null)
			{
				return;
			}

			int currentLevel = GM.currentLv;
			int totalLevels = GetTotalLevels();
			if (currentLevel > totalLevels)
			{
				ShowPostGameProgress(currentLevel);
				return;
			}

			int[] levels = GetLevelsToShow(currentLevel);
			SetLevelItem(Ng1, levels[0], currentLevel);
			SetLevelItem(Ng2, levels[1], currentLevel);
			SetLevelItem(Ng3, levels[2], currentLevel);
			SetLevelItem(Ng4, levels[3], currentLevel);
			SetLevelItem(Ng5, levels[4], currentLevel);
			SetArrowPosition(currentLevel);
			HideWithdrawTop();
		}

		private int analyticsLuckyLevel = -1;
		private bool analyticsLuckyVisible;

		private void RefreshLuckyProgress()
		{
			if (LuckyRoot == null)
			{
				return;
			}

			if (GM == null)
			{
				LuckyRoot.gameObject.SetActive(false);
				if (LuckyProgress != null)
				{
					LuckyProgress.fillAmount = 0f;
				}
				return;
			}

			int currentLevel = GM.currentLv;
			bool isUnlocked =
				currentLevel >= GameDefines.LuckyWalletUnlockLevel;
			LuckyRoot.gameObject.SetActive(isUnlocked);
			if (analyticsLuckyLevel != currentLevel || analyticsLuckyVisible != isUnlocked)
			{
				analyticsLuckyLevel = currentLevel;
				analyticsLuckyVisible = isUnlocked;
				MahjongProductAnalytics.Track("f2_lucky_progress_presentation", "level", currentLevel, "visible", isUnlocked, "unlock_level", GameDefines.LuckyWalletUnlockLevel, "group_start", GetLuckyGroupStartLevel(currentLevel), "visible_count", LuckyVisibleCount);
			}
			if (!isUnlocked)
			{
				if (LuckyProgress != null)
				{
					LuckyProgress.fillAmount = 0f;
				}
				return;
			}

			int groupStartLevel =
				GetLuckyGroupStartLevel(currentLevel);
			int completedInGroup =
				Mathf.Clamp(currentLevel - groupStartLevel, 0, LuckyVisibleCount);
			if (LuckyProgress != null)
			{
				LuckyProgress.fillAmount =
					completedInGroup / (float)LuckyVisibleCount;
			}

			for (int i = 0; i < luckyProgressStates.Length; i++)
			{
				int level = groupStartLevel + i;
				UserDataManager dataManager =
					UserDataManager.Instance;
				bool isLuckyLevel =
					dataManager != null &&
					dataManager.ShouldShowLuckyRewardIcon(level);
				LuckyItemProgress progressState =
					luckyProgressStates[i];
				if (progressState != null)
				{
					progressState.SetState(
						level,
						level < currentLevel,
						isLuckyLevel);
					progressState.SetLuckyRewardIconVisible(
						isLuckyLevel);
				}
			}
		}

		private int GetLuckyGroupStartLevel(int currentLevel)
		{
			int unlockLevel =
				GameDefines.LuckyWalletUnlockLevel;
			if (currentLevel > unlockLevel)
			{
				return unlockLevel +
					((currentLevel - unlockLevel) /
						LuckyVisibleCount) *
					LuckyVisibleCount;
			}
			return unlockLevel;
		}

		private void ShowPostGameProgress(int curLevel)
		{
			int totalLevels = GetTotalLevels();
			SetLevelItem(Ng1, totalLevels, curLevel);
			SetLevelItem(Ng2, totalLevels, curLevel);
			SetLevelItem(Ng3, totalLevels, curLevel);
			SetLevelItem(Ng4, totalLevels, curLevel);
			SetLevelItem(Ng5, curLevel, curLevel);
			HideAllArrows();
			if (Arrow4 != null)
			{
				Arrow4.SetActive(true);
			}
			HideWithdrawTop();
		}

		private void SetLevelItem(GameObject itemObj, int level, int currentLevel)
		{
			if (itemObj == null)
			{
				return;
			}

			LevelItem item = itemObj.GetComponent<LevelItem>();
			if (item != null)
			{
				item.SetLevel(level, currentLevel);
			}
		}

		private void SetArrowPosition(int curLevel)
		{
			HideAllArrows();
			if (curLevel == 1)
			{
				return;
			}

			int arrowIndex = curLevel <= 4
				? curLevel - 2
				: (curLevel - 1) & 3;
			GameObject arrow = null;
			switch (arrowIndex)
			{
				case 0:
					arrow = Arrow1;
					break;
				case 1:
					arrow = Arrow2;
					break;
				case 2:
					arrow = Arrow3;
					break;
				case 3:
					arrow = Arrow4;
					break;
			}
			if (arrow != null)
			{
				arrow.SetActive(true);
			}
		}

		private void HideAllArrows()
		{
			if (Arrow1 != null)
			{
				Arrow1.SetActive(false);
			}
			if (Arrow2 != null)
			{
				Arrow2.SetActive(false);
			}
			if (Arrow3 != null)
			{
				Arrow3.SetActive(false);
			}
			if (Arrow4 != null)
			{
				Arrow4.SetActive(false);
			}
		}

		private int[] GetLevelsToShow(int curLevel)
		{
			if (milestones != null)
			{
				for (int i = 0; i < milestones.Count; i++)
				{
					LevelMilestone milestone = milestones[i];
					if (milestone != null &&
						curLevel >= milestone.startLevel &&
						curLevel <= milestone.endLevel)
					{
						return milestone.displayLevels;
					}
				}
			}
			return GenerateDefaultLevels(curLevel);
		}

		private int[] GenerateDefaultLevels(int curLevel)
		{
			int totalLevels = GetTotalLevels();
			int startLevel = Mathf.Max(curLevel - 2, 1);
			int endLevel = Mathf.Min(startLevel + 4, totalLevels);
			if (endLevel - startLevel < 4)
			{
				startLevel = endLevel - 4;
			}
			startLevel = Mathf.Max(startLevel, 1);
			return new[]
			{
				startLevel,
				startLevel + 1,
				startLevel + 2,
				startLevel + 3,
				endLevel
			};
		}

		public void RefreshCoinInfo()
		{
			if (_coinTxt == null || GM == null)
			{
				return;
			}
			if (currencyHud != null)
			{
				currencyHud.RefreshCash(GM.currentCoin);
				return;
			}
			_coinTxt.text = FacadePayTypeExtend.RegionalChangeHandle != null
				? FacadePayTypeExtend.RegionalChangeHandle(GM.currentCoin)
				: GM.FormatRegionalValue(GM.currentCoin);
		}

		internal MahjongSlide.Withdrawal.MahjongCashCounter.Flight HoldHostCashFlight(float amount)
		{
			return currencyHud != null ? currencyHud.HoldCashFlight(amount) : null;
		}

		private void RefreshGemInfo()
		{
			if (_gemTxt != null && GM != null)
			{
				_gemTxt.text = GM.currentGems.ToString();
			}
		}

		internal void ShowGemCurrencyForGuide()
		{
			if (MoneyRoot != null) MoneyRoot.SetActive(true);
		}

		private Dictionary<ERewardType, Vector3> GetFlyObjGold()
		{
			Dictionary<ERewardType, Vector3> targets =
				new Dictionary<ERewardType, Vector3>();
			if (_coinTxt != null)
			{
				targets[ERewardType.Money] = _coinTxt.transform.position;
			}
			if (_gemTxt != null)
			{
				targets[ERewardType.Gem] = _gemTxt.transform.position;
			}
			if (AddBottleTxtCount != null)
			{
				targets[ERewardType.AddBottle] = AddBottleTxtCount.transform.position;
			}
			if (ClearCountTxtCount != null)
			{
				targets[ERewardType.Clear] = ClearCountTxtCount.transform.position;
			}
			if (remainUndoCount != null)
			{
				targets[ERewardType.Undo] = remainUndoCount.transform.position;
			}
			if (promptIcon != null)
			{
				targets[ERewardType.Prompt] = promptIcon.transform.position;
			}
			if (guidIcon != null)
			{
				targets[ERewardType.Guid] = guidIcon.transform.position;
			}
			return targets;
		}

		public void RefreshUndoInfo()
		{
			ResolveReferencesIfNeeded();
			if (GM == null || remainUndoCount == null)
			{
				return;
			}
			int count = GM.currentUndo;
			if (undoRWIcon != null)
			{
				undoRWIcon.SetActive(count > 0);
			}
			if (count > 0)
			{
				remainUndoCount.text = count.ToString();
			}
		}

		public void RefreshClearInfo()
		{
			ResolveReferencesIfNeeded();
			if (GM == null || ClearCountTxtCount == null)
			{
				return;
			}
			int count = GM.currentClear;
			if (clearRWIcon != null)
			{
				clearRWIcon.SetActive(count > 0);
			}
			if (count > 0)
			{
				ClearCountTxtCount.text = count.ToString();
			}
		}

		public void RefreshBottleCount()
		{
			ResolveReferencesIfNeeded();
			if (GM == null || AddBottleTxtCount == null)
			{
				return;
			}
			int count = GM.currentAddBottleCount;
			if (addRwIcon != null)
			{
				addRwIcon.SetActive(count > 0);
			}
			if (count > 0)
			{
				AddBottleTxtCount.text = count.ToString();
			}
		}

		private void UpdateMoneyDisplay()
		{
			if (LV != null)
			{
				LV.SetActive(true);
			}
			HideWithdrawTop();
		}

		private void HideWithdrawTop()
		{
			if (WithdrawTop != null)
			{
				WithdrawTop.SetActive(false);
			}
			if (tips != null)
			{
				tips.SetActive(false);
			}
		}

		private void ShowWithdrawTop()
		{
			HideWithdrawTop();
		}

		public void UpdateWithdrawPrompt()
		{
			if (GM == null)
			{
				return;
			}
			if (ShouldPreviewStage4PromptWhileWithdrawFeedbackVisible() &&
				TryRefreshStage45Prompt(Stage4Id))
			{
				return;
			}

			int stageId = GM.currentStage;
			if ((stageId == Stage4Id || stageId == Stage5Id) &&
				TryRefreshStage45Prompt(stageId))
			{
				return;
			}
			if (stageId == Stage3Id)
			{
				RefreshStage3Prompt();
				return;
			}

			SetStage3RootState(false);

			int type;
			int targetValue;
			if (!GM.TryGetStageRequirement(
					stageId, out type, out targetValue) ||
				targetValue < 1 ||
				(type != 2 && type != 3))
			{
				if (WithdrawPrompt != null)
				{
					WithdrawPrompt.SetActive(false);
				}
				if (StageIcon3 != null)
				{
					StageIcon3.SetActive(false);
				}
				if (LV != null)
				{
					LV.SetActive(true);
				}
				return;
			}

			if (WithdrawPrompt != null)
			{
				WithdrawPrompt.SetActive(true);
			}
			if (StageIcon3 != null)
			{
				StageIcon3.SetActive(false);
			}
			if (LV != null)
			{
				LV.SetActive(true);
			}

			if (type == 2)
			{
				float remaining = targetValue - GM.currentCoin;
				if (content != null)
				{
					content.text = string.Format(
						GetText(
							"2097",
							"Earn {0} more to withdraw {1}"),
						GM.FormatRegionalValue(
							(double)Mathf.Max(remaining, 0f)),
						GM.FormatRegionalValue((double)targetValue));
				}
				if (Process != null)
				{
					Process.fillAmount =
						Mathf.Clamp01(GM.currentCoin / targetValue);
				}
				if (num != null)
				{
					num.text =
						GM.FormatRegionalValue(GM.GetCoin()) + "/" +
						GM.FormatRegionalValue((double)targetValue);
				}
				return;
			}

			int currentCount = GM.stage5CurrentCycle;
			int remainingCount = targetValue - currentCount;
			if (content != null)
			{
				content.text = string.Format(
					GetText(
						"2098",
						"Check in for {0} more days withdraw all cash"),
					remainingCount < 1 ? 0 : remainingCount);
			}
			if (Process != null)
			{
				Process.fillAmount = remainingCount < 1
					? 1f
					: Mathf.Clamp01(
						currentCount / (float)targetValue);
			}
			if (num != null)
			{
				num.text =
					currentCount.ToString() + "/" +
					targetValue.ToString();
			}
		}

		private void RefreshStage3Prompt()
		{
			SetStage3RootState(true);
			SetStage3ProgressStateRootVisible(true);
			if (StageIcon3 != null)
			{
				StageIcon3.SetActive(false);
			}
			if (LV != null)
			{
				LV.SetActive(true);
			}

			int targetValue =
				GetStageTarget(Stage3Id, DefaultAdMissionTarget);
			int completedCount =
				Mathf.Max(GM.currentStageAdWatchCount, 0);
			if (ProgressTxT != null)
			{
				ProgressTxT.text =
					completedCount.ToString() + "/" +
					targetValue.ToString();
			}
			float progress = Mathf.Clamp01(
				Mathf.Min(completedCount, targetValue) /
				(float)Mathf.Max(targetValue, 1));
			RefreshStage3ProgressVisuals(
				Stage3Id, progress, true, targetValue);
		}

		private bool TryRefreshStage45Prompt(int stageId)
		{
			int type;
			int targetValue;
			if (GM == null ||
				!GM.TryGetStageRequirement(
					stageId, out type, out targetValue) ||
				targetValue < 1 ||
				(type != 2 && type != 3))
			{
				return false;
			}

			SetStage3RootState(true);
			SetStage3ProgressStateRootVisible(false);
			if (StageIcon3 != null)
			{
				StageIcon3.SetActive(false);
			}
			if (LV != null)
			{
				LV.SetActive(true);
			}

			float progress;
			if (type == 2)
			{
				progress =
					Mathf.Clamp01(GM.currentCoin / targetValue);
				if (ProgressTxT != null)
				{
					ProgressTxT.text = string.Format(
						"{0}/{1}",
						GM.FormatRegionalValue(GM.GetCoin()),
						GM.FormatRegionalValue((double)targetValue));
				}
			}
			else
			{
				int currentCount = GM.stage5CurrentCycle;
				progress = Mathf.Clamp01(
					currentCount / (float)targetValue);
				if (ProgressTxT != null)
				{
					ProgressTxT.text = string.Format(
						"{0}/{1}", currentCount, targetValue);
				}
			}

			RefreshStage3ProgressVisuals(
				stageId, progress, false, 0);
			return true;
		}

		private bool ShouldPreviewStage4PromptWhileWithdrawFeedbackVisible()
		{
			return GM != null &&
				GM.currentStage == Stage3Id &&
				GM.IsStageCompleted(Stage3Id) &&
				UIManager.instance != null &&
				UIManager.instance.HasActiveView(
					EUIType.WithdrawFeedbackView);
		}

		private void SetStage3RootState(bool isVisible)
		{
			if (Stage3Root != null)
			{
				Stage3Root.gameObject.SetActive(isVisible);
			}
			if (!isVisible)
			{
				ResetDeferredStage3ProgressState();
				return;
			}
			if (WithdrawPrompt != null)
			{
				WithdrawPrompt.SetActive(false);
			}
		}

		private void SetStage3ProgressStateRootVisible(bool isVisible)
		{
			if (ProgressStateRoot != null)
			{
				ProgressStateRoot.gameObject.SetActive(isVisible);
			}
		}

		private void SetStage3ProgressState(int completedCount, int targetCount)
		{
			if (progressStates == null)
			{
				return;
			}
			int completed = Mathf.Clamp(
				completedCount, 0, progressStates.Length);
			for (int i = 0; i < progressStates.Length; i++)
			{
				WithdrawMissionStateItem state = progressStates[i];
				if (state != null)
				{
					state.SetState(i + 1, i < completed);
				}
			}
		}

		private void RefreshStage3ProgressVisuals(int stageId, float progressValue, bool showProgressStates, int progressStateTargetCount)
		{
			int normalizedStageId = Mathf.Max(stageId, 0);
			stageMissionDescription = GetLevelObjectivesByStage(normalizedStageId) ?? string.Empty;
			int normalizedTargetCount =
				Mathf.Max(progressStateTargetCount, 0);
			float targetProgress = Mathf.Clamp01(progressValue);
			deferredStage3StageId = normalizedStageId;
			deferredStage3ShowProgressStates = showProgressStates;
			deferredStage3ProgressStateTargetCount =
				normalizedTargetCount;
			deferredStage3TargetProgressValue = targetProgress;

			if (!hasDisplayedStage3ProgressValue)
			{
				ApplyStage3ProgressVisualState(
					normalizedStageId,
					targetProgress,
					showProgressStates,
					normalizedTargetCount);
				RememberDisplayedStage3ProgressState(
					normalizedStageId,
					targetProgress,
					showProgressStates,
					normalizedTargetCount);
				hasDeferredStage3ProgressUpdate = false;
				return;
			}

			bool stateShapeChanged =
				displayedStage3StageId != normalizedStageId ||
				displayedStage3ShowProgressStates !=
					showProgressStates ||
				displayedStage3ProgressStateTargetCount !=
					normalizedTargetCount;
			if (stateShapeChanged)
			{
				hasDeferredStage3ProgressUpdate = false;
				ApplyStage3ProgressVisualState(
					normalizedStageId,
					targetProgress,
					showProgressStates,
					normalizedTargetCount);
				RememberDisplayedStage3ProgressState(
					normalizedStageId,
					targetProgress,
					showProgressStates,
					normalizedTargetCount);
				return;
			}

			if (ShouldDeferStage3ProgressUpdate())
			{
				hasDeferredStage3ProgressUpdate =
					!Mathf.Approximately(
						displayedStage3ProgressValue,
						targetProgress);
				return;
			}
			if (hasDeferredStage3ProgressUpdate)
			{
				return;
			}

			ApplyStage3ProgressVisualState(
				normalizedStageId,
				targetProgress,
				showProgressStates,
				normalizedTargetCount);
			RememberDisplayedStage3ProgressState(
				normalizedStageId,
				targetProgress,
				showProgressStates,
				normalizedTargetCount);
		}

		private void UpdateDeferredStage3ProgressAnimation()
		{
			if (!hasDisplayedStage3ProgressValue ||
				!hasDeferredStage3ProgressUpdate)
			{
				return;
			}
			if (Stage3Root == null ||
				!Stage3Root.gameObject.activeInHierarchy)
			{
				ResetDeferredStage3ProgressState();
				return;
			}
			if (ShouldDeferStage3ProgressUpdate())
			{
				return;
			}

			displayedStage3ProgressValue = Mathf.MoveTowards(
				displayedStage3ProgressValue,
				deferredStage3TargetProgressValue,
				Time.unscaledDeltaTime *
					DeferredStage3ProgressAnimationSpeed);
			ApplyStage3ProgressVisualState(
				deferredStage3StageId,
				displayedStage3ProgressValue,
				deferredStage3ShowProgressStates,
				deferredStage3ProgressStateTargetCount);
			if (!Mathf.Approximately(
					displayedStage3ProgressValue,
					deferredStage3TargetProgressValue))
			{
				return;
			}

			displayedStage3ProgressValue =
				deferredStage3TargetProgressValue;
			ApplyStage3ProgressVisualState(
				deferredStage3StageId,
				displayedStage3ProgressValue,
				deferredStage3ShowProgressStates,
				deferredStage3ProgressStateTargetCount);
			RememberDisplayedStage3ProgressState(
				deferredStage3StageId,
				displayedStage3ProgressValue,
				deferredStage3ShowProgressStates,
				deferredStage3ProgressStateTargetCount);
			hasDeferredStage3ProgressUpdate = false;
		}

		private void ApplyStage3ProgressVisualState(int stageId, float progressValue, bool showProgressStates, int progressStateTargetCount)
		{
			float progress = Mathf.Clamp01(progressValue);
			if (Stage3Dec != null)
			{
				// Use the stage being displayed, including the source stage-4
				// preview, rather than showing one generic percentage task forever.
				Stage3Dec.text = stageMissionDescription;
			}
			if (Progress != null)
			{
				Progress.fillAmount = progress;
			}
			if (Fill != null)
			{
				Fill.fillAmount = progress;
			}
			if (OverallCount != null)
			{
				OverallCount.text = string.Format(
					"{0}%", Mathf.RoundToInt(progress * 100f));
			}
			if (showProgressStates)
			{
				SetStage3ProgressState(
					GetDisplayedStage3CompletedCount(
						progress,
						progressStateTargetCount),
					progressStateTargetCount);
			}
		}

		private int GetDisplayedStage3CompletedCount(float progressValue, int targetCount)
		{
			if (targetCount <= 0)
			{
				return 0;
			}
			return Mathf.Clamp(
				Mathf.FloorToInt(
					Mathf.Clamp01(progressValue) *
						targetCount +
					0.0001f),
				0,
				targetCount);
		}

		private bool ShouldDeferStage3ProgressUpdate()
		{
			return UIManager.instance != null &&
				!UIManager.instance.IsGameplayOnlyUIActive();
		}

		private void RememberDisplayedStage3ProgressState(int stageId, float progressValue, bool showProgressStates, int progressStateTargetCount)
		{
			displayedStage3StageId = Mathf.Max(stageId, 0);
			displayedStage3ShowProgressStates =
				showProgressStates;
			displayedStage3ProgressStateTargetCount =
				Mathf.Max(progressStateTargetCount, 0);
			hasDisplayedStage3ProgressValue = true;
			displayedStage3ProgressValue =
				Mathf.Clamp01(progressValue);
		}

		private void ResetDeferredStage3ProgressState()
		{
			hasDisplayedStage3ProgressValue = false;
			hasDeferredStage3ProgressUpdate = false;
			displayedStage3ProgressValue = 0f;
			deferredStage3TargetProgressValue = 0f;
			displayedStage3StageId = 0;
			deferredStage3StageId = 0;
			displayedStage3ShowProgressStates = false;
			deferredStage3ShowProgressStates = false;
			displayedStage3ProgressStateTargetCount = 0;
			deferredStage3ProgressStateTargetCount = 0;
		}

		private int GetStageTarget(int stageId, int fallbackValue)
		{
			int type;
			int value;
			if (GM == null ||
				!GM.TryGetStageRequirement(
					stageId, out type, out value) ||
				type != AdMissionType ||
				value < 1)
			{
				return fallbackValue;
			}
			return value;
		}

		private string GetText(string key, string fallback)
		{
			if (LocalizationManager.Instance == null)
			{
				return fallback;
			}
			string localized = LocalizationManager.Instance.GetText(key);
			return string.IsNullOrEmpty(localized) || localized == key
				? fallback
				: localized;
		}

		private WithdrawMissionStateItem GetOrAddStateItem(GameObject target)
		{
			if (target == null)
			{
				return null;
			}
			WithdrawMissionStateItem item =
				target.GetComponent<WithdrawMissionStateItem>();
			if (item == null)
			{
				item =
					target.AddComponent<WithdrawMissionStateItem>();
			}
			return item;
		}

		private LuckyItemProgress GetOrAddLuckyItemProgress(GameObject target)
		{
			if (target == null)
			{
				return null;
			}
			LuckyItemProgress item =
				target.GetComponent<LuckyItemProgress>();
			if (item == null)
			{
				item = target.AddComponent<LuckyItemProgress>();
			}
			if (item != null)
			{
				item.Initialize();
			}
			return item;
		}

		private void BindEvents()
		{
			if (_isEventsBound)
			{
				RemoveEvents();
			}
			replayBtn?.onClick.AddListener(OnReplayClick);
			Clear?.onClick.AddListener(OnClearClick);
			UndoBtn?.onClick.AddListener(OnUndoClick);
			AddBottle?.onClick.AddListener(OnAddBottleClick);
			Setting?.onClick.AddListener(OnSettingClick);
			CoinBoard?.onClick.AddListener(OnBoardClick);
			GemBoard?.onClick.AddListener(OnGemBoardClick);
			SlotBtn?.onClick.AddListener(() =>
				UIManager.instance?.ShowView(EUIType.SlotView));
			_isEventsBound = true;
		}

		private void RemoveEvents()
		{
			FacadeGamePlayExtend.GetFlyObjGoldHandle -= GetFlyObjGold;
			replayBtn?.onClick.RemoveAllListeners();
			Clear?.onClick.RemoveAllListeners();
			UndoBtn?.onClick.RemoveAllListeners();
			AddBottle?.onClick.RemoveAllListeners();
			Setting?.onClick.RemoveAllListeners();
			CoinBoard?.onClick.RemoveListener(OnBoardClick);
			GemBoard?.onClick.RemoveListener(OnGemBoardClick);
			SlotBtn?.onClick.RemoveAllListeners();
			_isEventsBound = false;
		}

		private void PlayClick()
		{
			SoundManager.Instance?.PlayUIClickSFX();
		}

		private void OnReplayClick()
		{
			PlayClick();
			UIManager.instance?.ShowView(EUIType.ReStartView);
		}

		private void OnSettingClick()
		{
			PlayClick();
			if (Apple.Framework2.AppleF2Runtime.OpenHostSettings != null) Apple.Framework2.AppleF2Runtime.OpenHostSettings();
			else UIManager.instance?.ShowView(EUIType.SettingsView);
		}

		private void OnBoardClick()
		{
			if (GM == null)
			{
				return;
			}
			DataModel data = new DataModel
			{
				Level = GM.currentLv - 1,
				Money = GM.GetCoin(),
				CloseType = 1,
				Num = GM.currentStage < 2 ? 1 : GM.currentStage
			};
			GameApp.viewManager?.Open(ViewType.WithDraw, data);
		}

		private void OnGemBoardClick()
		{
			UIManager.instance?.ShowView(EUIType.WithdrawView);
		}

		private void OnClearClick()
		{
			if (GM == null)
			{
				return;
			}
			if (GM.currentClear > 0)
			{
				GM.ProcessClear();
			}
			else
			{
				UIManager.instance?.ShowView(EUIType.PropView, 2);
			}
		}

		private void OnUndoClick()
		{
			if (GM == null)
			{
				return;
			}
			if (GM.currentUndo > 0)
			{
				GM.UseUndo();
			}
			else
			{
				UIManager.instance?.ShowView(EUIType.PropView, 3);
			}
		}

		private void OnAddBottleClick()
		{
			if (GM == null || GM.levelGen == null)
			{
				return;
			}
			if (GM.currentAddBottleCount < 1)
			{
				UIManager.instance?.ShowView(EUIType.PropView, 1);
			}
		}

		public void ShowBooster()
		{
			if (booster != null)
			{
				booster.SetActive(false);
			}
		}

		public void HideBooster()
		{
			if (booster != null)
			{
				booster.SetActive(false);
			}
		}

		public void HideWithdraw()
		{
			UpdateMoneyDisplay();
			if (WithdrawTop != null)
			{
				WithdrawTop.SetActive(false);
			}
			if (MoneyRoot != null)
			{
				MoneyRoot.SetActive(false);
			}
			if (Stage3Root != null)
			{
				Stage3Root.gameObject.SetActive(false);
			}
		}

		public void ShowWithdraw()
		{
			UpdateMoneyDisplay();
			UpdateWithdrawPrompt();
		}

		private void OnDestroy()
		{
			RemoveEvents();
			StopBarrage();
		}
	}
}
