using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	public class GameView : BaseView
	{
		[Serializable]
		public class LevelMilestone
		{
			public int startLevel;

			public int endLevel;

			public int[] displayLevels;
		}

		[Header("Text")]
		public Text levelTxt;

		public Text level1Txt;

		public Text coinTxt;

		public Text gemTxt;

		public Text remainUndoCount;

		public Text AddBottleTxtCount;

		public Text ClearCountTxtCount;

		public Text tipsTxt;

		[Header("Icons")]
		public Transform coinIconInBoard;

		public Transform CapRoot;

		public GameObject undoRWIcon;

		public GameObject addRwIcon;

		public GameObject clearRWIcon;

		public GameObject promptIcon;

		public GameObject guidIcon;

		[Header("UI Elements")]
		public GameObject booster;

		public GameObject boosterTut;

		public GameObject boosterMask;

		public GameObject tips;

		public GameObject MoneyRoot;

		public GameObject WithdrawTop;

		public GameObject MoneyIcon;

		public GameObject IAAMoneyIcon;

		public GameObject LV;

		public GameObject LV1;

		[Header("Buttons")]
		public Button replayBtn;

		public Button Clear;

		public Button UndoBtn;

		public Button AddBottle;

		public Button Setting;

		public Button CoinBoard;

		public Button GemBoard;

		[Header("Settings")]
		public bool unlockHintView;

		public float barrageStartDelay;

		public float barrageInterval;

		private bool _isEventsBound;

		private float barrageTimer;

		private bool isBarrageActive;

		private Coroutine firstBarrageCoroutine;

		public GameObject Ng1;

		public GameObject Ng2;

		public GameObject Ng3;

		public GameObject Ng4;

		public GameObject Ng5;

		public GameObject Arrow1;

		public GameObject Arrow2;

		public GameObject Arrow3;

		public GameObject Arrow4;

		public GameObject StageIcon3;

		public GameObject WithdrawPrompt;

		public Slider Process;

		public Text content;

		public Text num;

		[Header("Barrage")]
		public TextMarquee barrage;

		private List<LevelMilestone> milestones;

		private GameManagerWZ GM => GameManagerWZ.instance;

		public GameView()
		{
			barrageStartDelay = 6f;
			barrageInterval = 20f;
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
			FacadeGamePlayExtend.GetFlyObjGoldHandle += GetFlyObjGold;
			GenerateMilestones();
			RefreshAllData();
			BindEvents();
			StartBarrage();
		}

		public override void Update()
		{
		}

		private void StartBarrage()
		{
			if (barrage == null || GM == null)
			{
				return;
			}
			if (GM.currentLv >= 3 && !GameDefines.ifIAA)
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
			return false;
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
			string play = row.TryGetValue("play", out playValue)
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
			if (GM == null)
			{
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
			if (CoinBoard != null)
			{
				CoinBoard.gameObject.SetActive(false);
			}
		}

		public void RefreshStageUI()
		{
			RefreshAllData();
		}

		internal override void RefreshRuntimeGmPresentation()
		{
			RefreshStageUI();
			if (barrage != null && barrage.gameObject.activeInHierarchy)
			{
				ShowBarrage();
			}
		}

		private void ResolveReferencesIfNeeded()
		{
			levelTxt = levelTxt != null
				? levelTxt
				: FindText("Area/TopButtons/LV/levelText");
			level1Txt = level1Txt != null
				? level1Txt
				: FindText("Area/TopButtons/LV1/levelText");
			coinTxt = coinTxt != null
				? coinTxt
				: FindText("Area/TopButtons/CurMoney/Bg1/CoinTxt");
			gemTxt = gemTxt != null
				? gemTxt
				: FindText("Area/TopButtons/GemMoney/Bg1/GemTxt");
			remainUndoCount = remainUndoCount != null
				? remainUndoCount
				: FindText("Area/BoosterButtons/UndoBtn/UndoRWIcon/Undocount");
			AddBottleTxtCount = AddBottleTxtCount != null
				? AddBottleTxtCount
				: FindText("Area/BoosterButtons/AddBottleBtn/AddBottleRWIcon/Addcount");
			ClearCountTxtCount = ClearCountTxtCount != null
				? ClearCountTxtCount
				: FindText("Area/BoosterButtons/ClearBtn/ClearRWIcon/Clearcount");
			tipsTxt = tipsTxt != null
				? tipsTxt
				: FindText("Area/TopButtons/Tips/tipsTxt");

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
			booster = booster != null
				? booster
				: FindObject("Area/BoosterButtons");
			tips = tips != null
				? tips
				: FindObject("Area/TopButtons/Tips");
			MoneyRoot = MoneyRoot != null
				? MoneyRoot
				: FindObject("Area/TopButtons/GemMoney");
			WithdrawTop = WithdrawTop != null
				? WithdrawTop
				: FindObject("Area/TopButtons/WithdrawTop");
			MoneyIcon = MoneyIcon != null
				? MoneyIcon
				: FindObject(
					"Area/TopButtons/CurMoney/Bg1/MoneyIcon/MoneyIcon");
			LV = LV != null
				? LV
				: FindObject("Area/TopButtons/LV");
			LV1 = LV1 != null
				? LV1
				: FindObject("Area/TopButtons/LV1");

			replayBtn = replayBtn != null
				? replayBtn
				: FindButton("Area/TopButtons/Replay");
			Clear = Clear != null
				? Clear
				: FindButton("Area/BoosterButtons/ClearBtn");
			UndoBtn = UndoBtn != null
				? UndoBtn
				: FindButton("Area/BoosterButtons/UndoBtn");
			AddBottle = AddBottle != null
				? AddBottle
				: FindButton("Area/BoosterButtons/AddBottleBtn");
			Setting = Setting != null
				? Setting
				: FindButton("Area/TopButtons/Setting");
			CoinBoard = CoinBoard != null
				? CoinBoard
				: FindButton("Area/TopButtons/CurMoney/CoinBtn");
			GemBoard = GemBoard != null
				? GemBoard
				: FindButton("Area/TopButtons/GemMoney/GemBtn");

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
				: FindSlider("Area/TopButtons/WithdrawPrompt/bg/Process");
			content = content != null
				? content
				: FindText("Area/TopButtons/WithdrawPrompt/bg/content");
			num = num != null
				? num
				: FindText(
					"Area/TopButtons/WithdrawPrompt/bg/Process/Fill Area/num");

			if (barrage == null)
			{
				GameObject barrageObject =
					FindObject("Area/MarquBG/Marque");
				barrage = barrageObject != null
					? barrageObject.GetComponent<TextMarquee>()
					: FindChildUtility.FindChild<TextMarquee>(
						gameObject, "Marque");
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

		private Slider FindSlider(string path)
		{
			GameObject target = FindObject(path);
			return target != null ? target.GetComponent<Slider>() : null;
		}

		public void GetLocation()
		{
			if (promptIcon == null)
			{
				promptIcon = GameObject.Find(
					"Canvas-UIManger/GamePlayUI/Footer/btnHint");
			}
			if (guidIcon == null)
			{
				guidIcon = GameObject.Find(
					"Canvas-UIManger/GamePlayUI/Footer/btnLines");
			}
		}

		private void RefreshTipsByStageAndLevel()
		{
			if (GM == null || tipsTxt == null)
			{
				return;
			}
			if (tips != null)
			{
				tips.SetActive(false);
			}

			int currentLevel = GM.currentLv;
			int currentStage = GM.currentStage;
			string objectiveKey = null;
			if (currentStage == 0)
			{
				objectiveKey = GetLevelObjectivesByStage(1);
			}
			else if (currentStage == 1)
			{
				objectiveKey = GetLevelObjectivesByStage(2);
			}
			else if (currentStage >= 2 &&
				currentLevel >= 3 &&
				currentLevel < GameDefines.SmallLevelGroupEnd)
			{
				objectiveKey = GetLevelObjectivesByStage(3);
			}

			LocalizationManager localization = LocalizationManager.Instance;
			if (string.IsNullOrEmpty(objectiveKey))
			{
				string format = localization != null
					? localization.GetText("1029")
					: "Clear Level{0}<color=#E7534B> Withdraw all</color>";
				tipsTxt.text = string.Format(format, currentLevel);
			}
			else
			{
				tipsTxt.text = localization != null
					? localization.GetText(objectiveKey)
					: objectiveKey;
			}
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

			object objective;
			if (!stageData.TryGetValue(
					"LevelObjectives", out objective))
			{
				return null;
			}
			string objectiveKey = objective as string;
			return string.IsNullOrEmpty(objectiveKey)
				? null
				: objectiveKey;
		}

		private void RefreshLevelInfo()
		{
			if (GM == null)
			{
				return;
			}

			LocalizationManager localization = LocalizationManager.Instance;
			string format = localization != null
				? localization.GetText("1004")
				: "Level {0}";
			string level = string.Format(format, GM.currentLv);
			if (levelTxt != null)
			{
				levelTxt.text = level;
			}
			if (level1Txt != null)
			{
				level1Txt.text = level;
			}
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
			ShowWithdrawTop();
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
			if (curLevel > GameDefines.SmallLevelGroupEnd)
			{
				HideWithdrawTop();
			}
			else
			{
				ShowWithdrawTop();
			}
		}

		private void SetLevelItem(
			GameObject itemObj, int level, int currentLevel)
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

		private void RefreshCoinInfo()
		{
			if (coinTxt == null || GM == null)
			{
				return;
			}
			coinTxt.text = FacadePayTypeExtend.RegionalChangeHandle != null
				? FacadePayTypeExtend.RegionalChangeHandle(GM.currentCoin)
				: GM.FormatRegionalValue(GM.currentCoin);
		}

		private void RefreshGemInfo()
		{
			if (gemTxt != null && GM != null)
			{
				gemTxt.text = GM.currentGems.ToString();
			}
		}

		private Dictionary<ERewardType, Vector3> GetFlyObjGold()
		{
			Vector3 defaultPosition = transform.position;
			Dictionary<ERewardType, Vector3> targets =
				new Dictionary<ERewardType, Vector3>();
			targets.Add(
				ERewardType.Money,
				coinTxt != null
					? coinTxt.transform.position
					: defaultPosition);
			targets.Add(
				ERewardType.Gem,
				gemTxt != null
					? gemTxt.transform.position
					: defaultPosition);
			targets.Add(
				ERewardType.AddBottle,
				AddBottleTxtCount != null
					? AddBottleTxtCount.transform.position
					: defaultPosition);
			targets.Add(
				ERewardType.Clear,
				ClearCountTxtCount != null
					? ClearCountTxtCount.transform.position
					: defaultPosition);
			targets.Add(
				ERewardType.Undo,
				remainUndoCount != null
					? remainUndoCount.transform.position
					: defaultPosition);
			targets.Add(
				ERewardType.Prompt,
				promptIcon != null
					? promptIcon.transform.position
					: defaultPosition);
			targets.Add(
				ERewardType.Guid,
				guidIcon != null
					? guidIcon.transform.position
					: defaultPosition);
			return targets;
		}

		public void RefreshUndoInfo()
		{
			if (GM == null || remainUndoCount == null)
			{
				return;
			}
			int count = GM.currentUndo;
			if (undoRWIcon != null)
			{
				undoRWIcon.SetActive(count > 0);
			}
			if (count >= 1)
			{
				remainUndoCount.text = count.ToString();
			}
		}

		public void RefreshClearInfo()
		{
			if (GM == null || ClearCountTxtCount == null)
			{
				return;
			}
			int count = GM.currentClear;
			if (clearRWIcon != null)
			{
				clearRWIcon.SetActive(count > 0);
			}
			if (count >= 1)
			{
				ClearCountTxtCount.text = count.ToString();
			}
		}

		public void RefreshBottleCount()
		{
			if (GM == null || AddBottleTxtCount == null)
			{
				return;
			}
			int count = GM.currentAddBottleCount;
			if (addRwIcon != null)
			{
				addRwIcon.SetActive(count > 0);
			}
			if (count >= 1)
			{
				AddBottleTxtCount.text = count.ToString();
			}
		}

		private void UpdateMoneyDisplay()
		{
			bool iaa = GameDefines.ifIAA;
			if (IAAMoneyIcon != null)
			{
				IAAMoneyIcon.SetActive(iaa);
			}
			if (GM == null)
			{
				return;
			}
			if (GM.currentLv > GameDefines.SmallLevelGroupEnd)
			{
				HideWithdrawTop();
			}
			else if (WithdrawTop != null)
			{
				WithdrawTop.SetActive(!iaa);
			}
		}

		private void HideWithdrawTop()
		{
			if (WithdrawTop != null)
			{
				WithdrawTop.SetActive(false);
			}
			if (LV1 != null)
			{
				LV1.SetActive(true);
			}
			if (LV != null)
			{
				LV.SetActive(false);
			}
			if (tips != null)
			{
				tips.SetActive(false);
			}
		}

		public void ShowWithdrawTop()
		{
			if (GM == null)
			{
				HideWithdrawTop();
				return;
			}
			if (GM.currentLv > GameDefines.SmallLevelGroupEnd)
			{
				HideWithdrawTop();
				return;
			}

			bool visible = !GameDefines.ifIAA;
			if (WithdrawTop != null)
			{
				WithdrawTop.SetActive(visible);
			}
			if (tips != null)
			{
				tips.SetActive(visible);
			}
		}

		public void UpdateWithdrawPrompt()
		{
			if (GM == null ||
				WithdrawPrompt == null ||
				StageIcon3 == null ||
				Process == null ||
				content == null ||
				num == null)
			{
				return;
			}

			if (GM.currentLv <= GameDefines.SmallLevelGroupEnd ||
				(GM.currentStage != 4 && GM.currentStage != 5))
			{
				WithdrawPrompt.SetActive(false);
				StageIcon3.SetActive(false);
				return;
			}

			WithdrawPrompt.SetActive(true);
			StageIcon3.SetActive(false);
			LocalizationManager localization = LocalizationManager.Instance;
			if (GM.currentStage == 4)
			{
				float target = GetStageTargetAsFloat(4);
				float remaining = target - GM.currentCoin;
				string format = localization != null
					? localization.GetText("2097")
					: "Earn {0} more to withdraw {1}";
				content.text = string.Format(
					format,
					GM.FormatRegionalValue(
						(double)Mathf.Max(remaining, 0f)),
					GM.FormatRegionalValue((double)target));
				Process.value = target > 0f
					? Mathf.Clamp01(GM.currentCoin / target)
					: 0f;
				num.text =
					GM.FormatRegionalValue(GM.GetCoin()) + "/" +
					GM.FormatRegionalValue((double)target);
				return;
			}

			int targetCount = GetStageTargetAsInt(5);
			int currentCount = GM.stage5CurrentCycle;
			int remainingCount = targetCount - currentCount;
			string stage5Text = localization != null
				? localization.GetText("2098")
				: "Check in for {0} more days withdraw all cash";
			content.text = remainingCount > 0
				? string.Format(stage5Text, remainingCount)
				: stage5Text;
			Process.value = remainingCount > 0 && targetCount > 0
				? Mathf.Clamp01(
					currentCount / (float)targetCount)
				: 1f;
			num.text =
				currentCount.ToString() + "/" +
				targetCount.ToString();
		}

		private float GetStageTargetAsFloat(int stageId)
		{
			Dictionary<string, object> stageData =
				GM != null ? GM.GetStageData(stageId) : null;
			object value;
			float target;
			return stageData != null &&
				stageData.TryGetValue("Value", out value) &&
				float.TryParse(
					value as string,
					NumberStyles.Float,
					CultureInfo.InvariantCulture,
					out target)
					? target
					: 0f;
		}

		private int GetStageTargetAsInt(int stageId)
		{
			Dictionary<string, object> stageData =
				GM != null ? GM.GetStageData(stageId) : null;
			object value;
			int target;
			return stageData != null &&
				stageData.TryGetValue("Value", out value) &&
				int.TryParse(
					value as string,
					NumberStyles.Integer,
					CultureInfo.InvariantCulture,
					out target)
					? target
					: 0;
		}

		private void BindEvents()
		{
			if (_isEventsBound)
			{
				RemoveEvents();
			}
			if (replayBtn != null)
			{
				replayBtn.onClick.AddListener(OnReplayClick);
			}
			if (Clear != null)
			{
				Clear.onClick.AddListener(OnClearClick);
			}
			if (UndoBtn != null)
			{
				UndoBtn.onClick.AddListener(OnUndoClick);
			}
			if (AddBottle != null)
			{
				AddBottle.onClick.AddListener(OnAddBottleClick);
			}
			if (Setting != null)
			{
				Setting.onClick.AddListener(OnSettingClick);
			}
			if (CoinBoard != null)
			{
				CoinBoard.onClick.AddListener(OnBoardClick);
			}
			if (GemBoard != null)
			{
				GemBoard.onClick.AddListener(() =>
					UIManager.instance.ShowView(EUIType.WithdrawView));
			}
			_isEventsBound = true;
		}

		private void RemoveEvents()
		{
			FacadeGamePlayExtend.GetFlyObjGoldHandle -= GetFlyObjGold;
			if (replayBtn != null)
			{
				replayBtn.onClick.RemoveAllListeners();
			}
			if (Clear != null)
			{
				Clear.onClick.RemoveAllListeners();
			}
			if (UndoBtn != null)
			{
				UndoBtn.onClick.RemoveAllListeners();
			}
			if (AddBottle != null)
			{
				AddBottle.onClick.RemoveAllListeners();
			}
			if (Setting != null)
			{
				Setting.onClick.RemoveAllListeners();
			}
			if (CoinBoard != null)
			{
				CoinBoard.onClick.RemoveAllListeners();
			}
			if (GemBoard != null)
			{
				GemBoard.onClick.RemoveAllListeners();
			}
			_isEventsBound = false;
		}

		private void PlayClick()
		{
		}

		private void OnReplayClick()
		{
			if (UIManager.instance != null)
			{
				UIManager.instance.ShowView(EUIType.ReStartView);
			}
		}

		private void OnSettingClick()
		{
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
				CloseType = 1
			};
			int stage = GM.currentStage;
			if (stage == 3)
			{
				data.Num = 3;
				data.Special = 1;
			}
			else
			{
				if (stage < 4)
				{
					stage++;
				}
				data.Num = stage;
			}
			if (GameApp.viewManager != null)
			{
				GameApp.viewManager.Open(ViewType.WithDraw, data);
			}
		}

		private void OnClearClick()
		{
		}

		private void OnUndoClick()
		{
		}

		private void OnAddBottleClick()
		{
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
		}

		public void ShowWithdraw()
		{
			UpdateMoneyDisplay();
		}

		private void OnDestroy()
		{
			if (_isEventsBound)
			{
				RemoveEvents();
			}
			StopBarrage();
		}
	}
}
