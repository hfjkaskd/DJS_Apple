using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Apple.Framework2;
using CorePlay;
using FruitEarningVariants;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WZSDK;
using Object = UnityEngine.Object;

/// <summary>
/// Diagnostic renders of the actual saved Apple-skinned B prefabs running in Game.unity.
/// Fixtures are isolated from production progress; these are not earned states, phone screenshots,
/// full workflow tests, or pixel-parity evidence. No artwork is substituted during capture.
/// </summary>
[InitializeOnLoad]
public static class Framework2AppleSkinValidation
{
    private const string SessionPrefix = "Apple.F2.SkinCapture.";
    private const string ConfigPath = "Assets/Framework2Replica/Resources/Framework2Replica/ProductConfig.json";
    private const string ExpectedConfigHash = "74a89260e72fc83aeee7e3d1bd5bc8ceeb6543af08342cd19e1fc05359648537";
    private const string EvidenceRoot = "Docs/CommercializationAB/Framework2Replica/ReskinImplementation-20261023";
    private static readonly Stack<IEnumerator> Routines = new Stack<IEnumerator>();
    private static PlayerLoopSystem originalLoop;
    private static bool installed, started, finishing;
    private static int lastFrame = -1, productionSelection;
    private static string output;
    private static Report report;
    private static Dictionary<string, string> productionFiles;
    private static Transform shownRoot;
    private static AppleF2Runtime regionalRuntime;

    [Serializable] private sealed class CaptureRecord
    {
        public int approvalId;
        public string template, state, file, kind = "actual-runtime-prefab-instance", fixture, background;
        public double elapsedSettleSeconds;
        public Vector3 rootScale;
        public int activeButtons, totalButtons;
        public bool viewVisible;
        public List<string> visibleText = new List<string>();
        public List<string> buttonPaths = new List<string>();
        public string referenceAspectFile;
        public Rect referencePhone;
        public List<LayoutObservation> layout = new List<LayoutObservation>();
    }

    [Serializable] private sealed class LayoutObservation
    {
        public string path;
        public Rect expected, actual;
        public bool visible;
        public float maxEdgeError;
    }

    [Serializable] private sealed class Omission
    {
        public int approvalId;
        public string reason;
    }

    [Serializable] private sealed class Report
    {
        public string generatedUtc, profile, productConfigSha256;
        public string scope = "Actual Game.unity and production B UI APIs in an isolated QA save profile. " +
            "Every balance/stage/account/history setup is explicitly synthetic. No real advertisement, payment, or user account is used. " +
            "Each PNG is an offscreen 1080x1920 render of real live scene/prefab instances after animation settling, " +
            "with safe-area adapters temporarily paused by Framework2ActualCapture. Reusable control surfaces and sprite contours use the approved artwork; text, values, controls and the gameplay scene remain live. " +
            "This is visual coverage, not physical-device evidence or a full end-to-end playthrough. Original workflow tests remain separate.";
        public string approvalManifest = "../ReskinPreview-AllUI-20261023/coverage.json";
        public bool passed, productionFilesUnchanged, productionSelectionUnchanged;
        public int assertions, frames, prefabsChecked, serializedButtonsChecked, captures, activeTemplatesCaptured, hostPanelsCaptured;
        public List<CaptureRecord> screenshots = new List<CaptureRecord>();
        public List<Omission> notCaptured = new List<Omission>();
        public List<string> checks = new List<string>();
        public List<string> errors = new List<string>();
        public List<string> warnings = new List<string>();
    }

    static Framework2AppleSkinValidation()
    {
        if (SessionState.GetBool(SessionPrefix + "Active", false)) Attach();
    }

    /// <summary>Use -batchmode -executeMethod Framework2AppleSkinValidation.Run, without -quit.</summary>
    public static void Run()
    {
        if (!Application.isBatchMode || EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Use a separate batch Editor in Edit mode; this runner exits its process.");
        output = Environment.GetEnvironmentVariable("DJS_APPLE_SKIN_VALIDATION_OUTPUT");
        if (string.IsNullOrWhiteSpace(output)) output = Path.Combine(Application.dataPath, "../" + EvidenceRoot + "/ActualCaptures");
        output = Path.GetFullPath(output);
        Directory.CreateDirectory(output);
        SessionState.SetString(SessionPrefix + "Output", output);
        SessionState.SetString(SessionPrefix + "Profile", "f2-apple-skin-" + Guid.NewGuid().ToString("N"));
        SessionState.SetString(SessionPrefix + "Deadline", DateTime.UtcNow.AddMinutes(12).ToString("O"));
        SessionState.SetBool(SessionPrefix + "Active", true);
        Attach();
        try
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorApplication.EnterPlaymode();
        }
        catch (Exception exception) { Finish(exception); }
    }

    private static void Attach()
    {
        EditorApplication.update -= Watchdog;
        EditorApplication.update += Watchdog;
        EditorApplication.playModeStateChanged -= OnPlayMode;
        EditorApplication.playModeStateChanged += OnPlayMode;
        Application.logMessageReceived -= OnLog;
        Application.logMessageReceived += OnLog;
    }

    private static void Watchdog()
    {
        if (finishing || !SessionState.GetBool(SessionPrefix + "Active", false)) return;
        if (!DateTime.TryParse(SessionState.GetString(SessionPrefix + "Deadline", ""), null,
            DateTimeStyles.RoundtripKind, out DateTime deadline) || DateTime.UtcNow > deadline)
            Finish(new TimeoutException("Apple skin capture exceeded 12 minutes."));
    }

    private static void OnPlayMode(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(SessionPrefix + "Active", false)) InstallLoop();
        if (started && !finishing && state == PlayModeStateChange.ExitingPlayMode)
            Finish(new InvalidOperationException("Play mode exited before captures completed."));
    }

    private static void InstallLoop()
    {
        if (installed) return;
        originalLoop = PlayerLoop.GetCurrentPlayerLoop();
        var loop = originalLoop;
        for (int i = 0; i < loop.subSystemList.Length; i++)
        {
            if (loop.subSystemList[i].type != typeof(UnityEngine.PlayerLoop.Update)) continue;
            var update = loop.subSystemList[i];
            var systems = update.subSystemList;
            Array.Resize(ref systems, systems.Length + 1);
            systems[systems.Length - 1] = new PlayerLoopSystem { type = typeof(Framework2AppleSkinValidation), updateDelegate = Tick };
            update.subSystemList = systems;
            loop.subSystemList[i] = update;
            PlayerLoop.SetPlayerLoop(loop);
            installed = true;
            return;
        }
        Finish(new InvalidOperationException("Player Update loop was not found."));
    }

    private static void Tick()
    {
        if (finishing || !EditorApplication.isPlaying || lastFrame == Time.frameCount) return;
        try
        {
            lastFrame = Time.frameCount;
            if (!started)
            {
                started = true;
                EnsureReport();
                Require(EarningVariantController.Instance == null && string.IsNullOrEmpty(SceneManager.GetActiveScene().path),
                    "QA storage is selected before any gameplay host exists.");
                productionFiles = HashProductionFiles();
                productionSelection = PlayerPrefs.GetInt("Apple.EarningVariant.v1", -1);
                EarningVariantSelection.ConfigureStorageProfile(report.profile);
                EarningVariantSelection.SaveNextSelection(true);
                Require(EarningVariantSelection.Framework2Namespace.Contains(report.profile), "B uses isolated QA storage.");
                Screen.SetResolution(1080, 1920, FullScreenMode.Windowed);
                ValidateSavedAssets();
                Routines.Push(Cases());
            }
            report.frames++;
            regionalRuntime?.Tick(0f);
            while (Routines.Count > 0)
            {
                var current = Routines.Peek();
                if (!current.MoveNext()) { (current as IDisposable)?.Dispose(); Routines.Pop(); continue; }
                if (current.Current is IEnumerator nested) { Routines.Push(nested); continue; }
                if (current.Current != null) throw new InvalidOperationException("Unsupported capture yield.");
                return;
            }
            Finish(null);
        }
        catch (Exception exception) { Finish(exception); }
    }

    private static IEnumerator Cases()
    {
        var load = SceneManager.LoadSceneAsync("Assets/Game.unity", LoadSceneMode.Single);
        while (!load.isDone) yield return null;
        yield return Until(() => EarningVariantController.Instance != null && EarningVariantController.Instance.Ready &&
            EarningVariantController.Instance.Runtime != null && EarningVariantController.Instance.Runtime.Ready &&
            CorePlayUI.Instance != null && CorePlayUI.Instance.IsOpening && CorePlayUI.Instance.m_BgImg.sprite != null,
            "B game scene, original fruit background, and runtime", 60);
        var host = EarningVariantController.Instance;
        // Use the actual fourth gameplay board for visual comparisons. The prior
        // fixture changed only B's commercial level and left the first tutorial
        // backdrop and locked tools behind it. This is isolated QA progress.
        JEFOMCDAPGK.Instance.SetWinCount(3);
        JEFOMCDAPGK.Instance.MarkNewItemPopShown(POJCEPBNNIP.Undo);
        JEFOMCDAPGK.Instance.MarkNewItemPopShown(POJCEPBNNIP.Shuffle);
        JEFOMCDAPGK.Instance.MarkNewItemPopShown(POJCEPBNNIP.Magic);
        JEFOMCDAPGK.Instance.MarkNewItemPopShown(POJCEPBNNIP.Extra);
        JEFOMCDAPGK.Instance.MainLevelData.Reset(false);
        bool boardReady=false;
        Framework.Base.UtilModule.CoroutineManager.Instance.StartCor(EDLHEMMBABM.Instance.ParseConfig(
            PLMIHDHFAAL.Instance.GetMainLevelConfig(),false,()=>boardReady=true));
        yield return Until(()=>boardReady,"native fourth gameplay board",30);
        CorePlayUI.Instance.UpdateBehavior();
        // Capture the requested commercial template without the native first-board tutorial overlay.
        // Gameplay itself is unchanged; the fixture does not claim a completed real playthrough.
        if (NewPlayerGuider.Instance != null) NewPlayerGuider.Instance.gameObject.SetActive(false);
        if (host.RulesPanel.activeSelf) Click(host.CloseRulesButton, "acknowledge local simulation rules");
        Require(EarningVariantSelection.IsB, "The current test session is B.");
        yield return Seconds(3);
        CheckRuntimeRules();

        yield return Prepare(5, 5, 123.45);
        shownRoot = UIManager.instance.GetView<GamePlayerView>(EUIType.GamePlayerView).transform;
        yield return Capture(1, "GamePlayerView", "late cash HUD", "stage 5 / level 5 / balance 123.45");
        yield return Legacy(2, "WithDraw", "stage1", ViewType.WithDraw, 1, 2, 0, Model(1, 0));
        yield return Legacy(18, "WithDraw", "stage2", ViewType.WithDraw, 2, 2, .1, Model(2, .1));
        yield return Legacy(19, "WithDraw", "stage3 advertisement progress", ViewType.WithDraw, 3, 3, .2, Model(3, .2), 1);
        yield return Legacy(20, "WithDraw", "stage4 balance task", ViewType.WithDraw, 4, 4, 50, Model(4, 50));
        yield return Legacy(21, "WithDraw", "stage5 balance task", ViewType.WithDraw, 5, 5, 1234.5, Model(5, 1234.5));
        yield return Legacy(22, "WithDraw", "Lucky wallet unlocked", ViewType.WithDraw, 5, 6, 2700, Model(5, 2700));

        yield return Legacy(7, "WithDrawmethod", "default email", ViewType.WithDrawMethod, 2, 2, .1, Model(1, .1));
        var email = FindInput(shownRoot, "Email");
        Require(email != null, "The receiver form retains its standard InputField.");
        email.text = "test@";
        Click(FindButton(shownRoot, "Confirm"), "invalid dummy receiver submission");
        yield return Capture(8, "WithDrawmethod", "invalid input", "QA dummy input test@; source Confirm handler", .8);
        yield return Legacy(9, "WithDrawConfirm", "confirmation", ViewType.WithDrawConfirm, 2, 2, .1, Model(1, .1));
        yield return Legacy(10, "WithDrawQues", "help", ViewType.WithDrawQues, 2, 2, .1, Model(1, .1));

        yield return Prepare(3, 3, .1);
        WithdrawApiService.ClearWalletAndOrdersForTesting();
        SPlayerPrefs.SetListTem(FacadePlayerPrefExtend.History, new List<HistoryModel>());
        yield return OpenLegacy(ViewType.WithDrawHistory, Model(3, .1));
        yield return Capture(11, "WithDrawHistory", "empty", "empty local QA history");
        yield return Prepare(3, 3, .1);
        WithdrawApiService.EnsureQueuedFirstAdOrder(.1);
        WithdrawApiService.CreateQueuedWalletOrder(.3, "test@example.invalid");
        Require(WithdrawApiService.GetCachedOrders().Count == 2, "History fixture contains two local queued orders.");
        yield return OpenLegacy(ViewType.WithDrawHistory, Model(3, .1));
        yield return Capture(12, "WithDrawHistory", "populated", "two explicitly synthetic pending local history rows, never paid");
        yield return Modern(13, "PayPalErrorView", "account condition error", EUIType.PayPalErrorView, 3, 3, .3, Model(3, .3));

        yield return Prepare(3, 4, .3, 3);
        yield return OpenModern(EUIType.WithdrawMissionView);
        yield return Capture(3, "WithdrawMissionView", "3/3", "QA stage 3 / three simulated ad completions / balance .30");
        yield return Prepare(3, 4, .2, 1);
        yield return OpenModern(EUIType.WithdrawMissionView);
        yield return Capture(14, "WithdrawMissionView", "in progress", "QA stage 3 / one simulated ad completion");
        yield return Modern(15, "WithdrawFeedbackView", "local review", EUIType.WithdrawFeedbackView, 3, 4, .3);

        yield return Prepare(3, 3, .1);
        yield return OpenLegacy(ViewType.WithDrawProcess1, Model(3, .1));
        yield return Capture(16, "WithDrawProcess1", "verification running", "QA local verification, intermediate source timer state", .8);
        yield return Capture(4, "WithDrawProcess1", "ordinary queue", "same QA local verification after original timers settle", 6);
        yield return Legacy(17, "WithDrawProcess1", "late-stage verification", ViewType.WithDrawProcess1, 5, 5, 2999.5, Model(5, 2999.5), 0, 6);

        yield return Modern(23, "SlotView", "stage4 slot", EUIType.SlotView, 4, 4, .3, 50f);
        yield return Modern(5, "ProgressView", "50/200", EUIType.ProgressView, 4, 4, 50, 50f);
        yield return Modern(24, "BloatView", "compensation offer", EUIType.BloatView, 5, 5, 2500);
        yield return Modern(25, "BonusWheelView", "compensation wheel", EUIType.BonusWheelView, 5, 5, 2500);
        var compensation = Model(5, 2500); compensation.Special = DataModel.SpecialRewardAlreadyGranted;
        yield return Legacy(26, "WithDrawSuc", "compensation result", ViewType.WithDrawSuc, 5, 5, 2700, compensation);
        yield return Modern(27, "BloatProgressView", "compensation progress", EUIType.BloatProgressView, 5, 5, 2700, 3000f);
        yield return Modern(28, "RewardView", "in-game reward", EUIType.RewardView, 5, 5, 123.45);
        yield return Modern(6, "ChallangeSuccessView", "normal settlement", EUIType.ChallangeSuccessView, 5, 5, 123.45, 5);
        yield return Modern(29, "LuckySuccessView", "lucky settlement", EUIType.LuckySuccessView, 5, 5, 123.45, 1);
        yield return Modern(30, "LuckySpinView", "lucky wheel", EUIType.LuckySpinView, 5, 5, 123.45);
        yield return Modern(31, "GameBalancView", "lucky wallet", EUIType.GameBalancView, 5, 5, 123.45);
        yield return Prepare(5, 5, 123.45);
        WithdrawApiService.ClearWalletAndOrdersForTesting();
        yield return OpenModern(EUIType.GameBalancView);
        Click(FindButton(shownRoot,"With"),"source insufficient-wallet notice");
        yield return Until(()=>UIManager.instance.HasActiveView(EUIType.NoticeView),"source insufficient notice");
        shownRoot=UIManager.instance.GetView<NoticeView>(EUIType.NoticeView).transform;
        yield return Capture(32, "NoticeView", "amount notice", "Original wallet withdrawal Button, with an explicitly empty QA wallet; original configured minimum and automatic dismissal duration", .75);
        yield return Modern(43, "SettingsView", "settings", EUIType.SettingsView, 5, 5, 123.45);
        yield return Modern(44, "UserLevelView", "player level", EUIType.UserLevelView, 5, 5, 123.45);

        yield return Prepare(1, 1, 0);
        var tutorial = Tutorial.ShowStepAsync(Tutorial.TYPE.TYPE3, 3);
        yield return Await(tutorial, "source cash entry tutorial");
        Require(tutorial.Result && Tutorial.instance != null, "Source tutorial uses its authored target and standard Button.");
        shownRoot = Tutorial.instance.transform;
        yield return Capture(33, "GamePlayerView + Tutorial", "initial guide", "source TYPE3 step 3; QA stage 1 / level 1");
        yield return Prepare(3, 4, .1, 1);
        shownRoot = UIManager.instance.GetView<GamePlayerView>(EUIType.GamePlayerView).transform;
        yield return Capture(34, "GamePlayerView", "stage3 hud", "QA stage 3 / level 4 / one of three simulated ad callbacks; no extra currency path enabled");
        yield return Modern(35, "WithdrawView", "reserved gem exchange", EUIType.WithdrawView, 5, 5, 123.45);
        yield return Legacy(38, "WithDrawContinue", "reserved continued tasks", ViewType.WithDrawContinue, 5, 5, 123.45, Model(3, 123.45));
        yield return Modern(45, "ReStartView", "reserved restart confirmation", EUIType.ReStartView, 5, 5, 123.45);

        yield return Prepare(5, 5, 123.45);
        Click(host.OpenSelectorButton, "open A/B selector");
        shownRoot = host.SelectorPanel.transform;
        yield return Capture(39, "SelectorPanel", "variant selector", "actual host OpenSelectorButton handler");
        Click(host.CloseSelectorButton, "close A/B selector");
        Require(!host.SelectorPanel.activeSelf, "Runtime-bound selector Close button hides its panel.");
        Click(host.RulesButton, "open rules");
        shownRoot = host.RulesPanel.transform;
        yield return Capture(40, "RulesPanel", "full local test rules", "actual host RulesButton handler");
        Click(host.CloseRulesButton, "close rules");
        Require(!host.RulesPanel.activeSelf, "Runtime-bound rules Close button hides its panel.");
        bool adFinished = false, adResult = true;
        AdsControl.Instance.ShowRewardedAd("skin-capture-local-only", success => { adFinished = true; adResult = success; }, false);
        yield return Until(() => host.AdPanel.activeSelf, "actual local ad panel");
        shownRoot = host.AdPanel.transform;
        yield return Capture(41, "AdPanel", "local advertisement simulation", "real host ad callback request; no SDK or network");
        Click(host.CancelAdButton, "cancel test advertisement");
        Require(adFinished && !adResult && !host.AdPanel.activeSelf, "Runtime-bound ad cancellation returns false exactly through its public callback.");
        host.ErrorText.text = "QA capture fixture: load unavailable.\nA and B saves are retained.\nNo real initialization failure was induced.";
        host.ErrorPanel.SetActive(true);
        shownRoot = host.ErrorPanel.transform;
        yield return Capture(42, "ErrorPanel", "error presentation fixture", "actual saved ErrorPanel manually activated; clearly synthetic QA error, not a reproduced failure");
        host.ErrorPanel.SetActive(false);

        // A separate facade/profile is required because the region delegate belongs to runtime initialization.
        // The production host is disposed only after all host assertions/captures; no production profile is selected.
        host.Runtime.Dispose();
        yield return Seconds(.5);
        yield return Regional(36, "BR", "Brazil conditional");
        yield return Regional(37, "ID", "phone conditional");
        report.notCaptured.Add(new Omission { approvalId = 46, reason = "Shared-components presentation board is not a standalone runtime page. Components are present within their actual owning pages, but no fabricated component montage is counted as a runtime screen." });
        report.activeTemplatesCaptured = CountActiveTemplates();
        report.hostPanelsCaptured = 4;
        foreach(var capture in report.screenshots)
            foreach(var box in capture.layout)
                if(box.visible)Require(box.maxEdgeError<=.5f,"Live layout within half a reference pixel: "+capture.approvalId+"/"+box.path+" ("+box.maxEdgeError+")");
        Require(report.activeTemplatesCaptured == 24, "All 24 current business templates have an actual runtime capture.");
        Require(HashFile(ConfigPath) == ExpectedConfigHash, "Original business ProductConfig remains byte-identical after all fixtures.");
        WriteIndex();
    }

    private static IEnumerator Regional(int approvalId, string region, string state)
    {
        var config = AppleF2Runtime.LoadProductConfigAsync();
        yield return Await(config, "original regional configuration");
        var create = AppleF2Runtime.CreateAsync(EarningVariantController.Instance.RuntimeParent, config.Result,
            (_, callback) => callback(false), (_, callback) => callback(false), () => false,
            () => new DateTime(2026, 10, 23, 12, 0, 0), () => region,
            "DJS.Apple.SkinCapture." + report.profile + "." + region + ".");
        yield return Await(create, "regional source facade initialization");
        regionalRuntime = create.Result;
        yield return Prepare(2, 2, .1);
        var data = Model(1, .1);
        if (region == "ID") data.Email = string.Empty;
        yield return OpenLegacy(ViewType.WithDrawMethod, data);
        if (region == "BR")
        {
            Transform pixEmail = shownRoot.Find("PanelA/PanelC/BrazilPixControls/Email");
            Toggle toggle = pixEmail == null ? null : pixEmail.GetComponent<Toggle>();
            Require(toggle != null, "Original Pix email selector is present.");
            toggle.isOn = true;
            FindInput(shownRoot, "Email").text = "test@example.invalid";
        }
        yield return Capture(approvalId, "WithDrawmethod", state,
            "Separate isolated " + region + " runtime facade initialized through its public region delegate. Same saved prefab; dummy fields only.");
        report.screenshots[report.screenshots.Count - 1].kind = "actual-regional-runtime-fixture-on-game-scene";
        regionalRuntime.Dispose(); regionalRuntime = null;
        yield return Seconds(.5);
    }

    private static DataModel Model(int number, double money) => new DataModel
    {
        Num = number, Level = number, Money = money, Index = 3, Email = "test@example.invalid",
        Phone = "", Name = "QA Test", CloseWithoutAdvance = true, SubmitOnOpen = false
    };

    private static IEnumerator Prepare(int stage, int level, double money, int ads = 0)
    {
        Tutorial.CancelPendingPresentation();
        UIManager.instance.CloseAllViews();
        yield return Seconds(.35);
        if (NewPlayerGuider.Instance != null) NewPlayerGuider.Instance.gameObject.SetActive(false);
        GameManagerWZ gm = GameManagerWZ.instance;
        gm.CompleteHostStageTutorial();
        gm.currentStage = stage; gm.maxUnlockedStage = stage;
        gm.SaveCurrentStage(); gm.SaveMaxUnlockedStage();
        gm.SetHostGameplayLevel(level);
        gm.currentCoin = (float)money; gm.SaveCoinData();
        gm.SavePlayerEmail("test@example.invalid");
        if (SPlayerPrefs.GetListTem<HistoryModel>(FacadePlayerPrefExtend.History) == null)
            SPlayerPrefs.SetListTem(FacadePlayerPrefExtend.History, new List<HistoryModel>());
        if (stage == 3) Require(gm.SetStageAdWatchProgressForTesting(ads), "Explicit QA ad progress seeded through source test API.");
        var hud = UIManager.instance.ShowViewAsync(EUIType.GamePlayerView);
        yield return Await(hud, "source HUD");
        shownRoot = hud.Result.transform;
        yield return Seconds(.2);
    }

    private static IEnumerator Legacy(int id, string template, string state, ViewType type, int stage, int level,
        double money, DataModel data, int ads = 0, double settle = 2.8)
    {
        yield return Prepare(stage, level, money, ads);
        yield return OpenLegacy(type, data);
        yield return Capture(id, template, state, "QA fixture: stage " + stage + ", level " + level + ", balance " + money.ToString(CultureInfo.InvariantCulture), settle);
    }

    private static IEnumerator Modern(int id, string template, string state, EUIType type, int stage, int level, double money, params object[] args)
    {
        yield return Prepare(stage, level, money);
        yield return OpenModern(type, args);
        yield return Capture(id, template, state, "QA fixture: stage " + stage + ", level " + level + ", balance " + money.ToString(CultureInfo.InvariantCulture));
    }

    private static IEnumerator OpenLegacy(ViewType type, params object[] args)
    {
        var task = GameApp.viewManager.OpenAsync(type, args);
        yield return Await(task, "legacy source page");
        var component = task.Result as Component;
        Require(component != null, "Legacy source API returned an actual component.");
        shownRoot = component.transform;
    }

    private static IEnumerator OpenModern(EUIType type, params object[] args)
    {
        var task = UIManager.instance.ShowViewAsync(type, args);
        yield return Await(task, "source page");
        Require(task.Result != null, "Source UI API returned an actual view.");
        shownRoot = task.Result.transform;
    }

    private static IEnumerator Capture(int id, string template, string state, string fixture, double settle = 2.8)
    {
        double beginning = Time.realtimeSinceStartupAsDouble;
        yield return Seconds(settle);
        Require(shownRoot != null && shownRoot.gameObject.activeInHierarchy, "Capture target remains visible: " + template + " / " + state);
        Require(Object.FindObjectOfType<NewItemPop>()==null,"No unrelated native unlock modal covers the requested UI.");
        if(template=="WithDrawmethod")
            Require(shownRoot.Find("PanelA/Panel/ques").gameObject.activeInHierarchy,"Account help remains visible and reachable.");
        if(template=="WithDrawConfirm")
        {
            var icon=shownRoot.Find("PanelA/method");
            Require(icon.gameObject.activeInHierarchy && icon.GetComponent<Image>().enabled && icon.GetSiblingIndex()==icon.parent.childCount-1,
                "Confirmation provider icon is enabled above the account card.");
        }
        if(id==39)
        {
            RectTransform a=shownRoot.Find("Card/ChooseA") as RectTransform;
            RectTransform b=shownRoot.Find("Card/ChooseB") as RectTransform;
            RectTransform confirm=shownRoot.Find("Card/NavConfirmSwitch") as RectTransform;
            Require(a.position.y>b.position.y && b.position.y>confirm.position.y,
                "Selector choices and confirmation occupy separate vertical rows.");
            Require(a.anchoredPosition.sqrMagnitude>1 && confirm.anchoredPosition.sqrMagnitude>1,
                "Saved host layout is expressed in nonzero canvas coordinates.");
            RectTransform overlay=EarningVariantController.Instance.transform.Find("TestOverlay") as RectTransform;
            RectTransform close=shownRoot.Find("Card/Close") as RectTransform;
            Bounds closeBounds=RectTransformUtility.CalculateRelativeRectTransformBounds(overlay,close);
            Require(closeBounds.max.x<=overlay.rect.xMax+1 && closeBounds.min.x>=overlay.rect.xMin-1,
                "Selector close button stays inside its live canvas viewport.");
        }
        if (shownRoot.GetComponent<BaseView>() != null)
            Require((shownRoot.localScale - Vector3.one).sqrMagnitude < .001f,
                "Source opening tween reached its final scale before capture: " + template);
        string file = id.ToString("00", CultureInfo.InvariantCulture) + "-" + template.Replace(" + ", "-") + ".png";
        var record = new CaptureRecord { approvalId = id, template = template, state = state, fixture = fixture, file = file,
            rootScale = shownRoot.localScale, elapsedSettleSeconds = Time.realtimeSinceStartupAsDouble - beginning,
            viewVisible = shownRoot.gameObject.activeInHierarchy };
        var bg = CorePlayUI.Instance.m_BgImg.sprite;
        record.background = bg == null ? "<missing>" : bg.name;
        foreach (Text text in shownRoot.GetComponentsInChildren<Text>())
            if (text.isActiveAndEnabled && !string.IsNullOrWhiteSpace(text.text)) record.visibleText.Add(text.text);
        foreach (Button button in shownRoot.GetComponentsInChildren<Button>(true))
        {
            record.totalButtons++;
            if (button.isActiveAndEnabled) record.activeButtons++;
            record.buttonPaths.Add(PathOf(button.transform, shownRoot));
            Require(button.onClick.GetPersistentEventCount() == 0, "Button events remain runtime-bound: " + template + "/" + button.name);
        }
        Action fit = () =>
        {
            if (regionalRuntime != null) return; // The disposed host cannot supply this separate regional facade's HUD inset.
            var ui = CorePlayUI.Instance;
            if (ui == null) return;
            ui.GetComponent<EarningVariantGameplayView>()?.RefreshLayout();
            ui.FitGameField();
            Tutorial.instance?.RefreshPresentationLayout();
        };
        if (!Framework2ActualCapture.TryCapturePortrait(Path.Combine(output, file), out string error, fit, fit))
            throw new InvalidOperationException("Actual-instance capture failed: " + error);
        var layouts=Framework2ReferenceLayoutAudit.Read();
        string suffix=id==2?"#stage1":id==8?"#invalid":id==18?"#stage2":id==21?"#late":id==22?"#unlocked":id==36?"#pix":id==37?"#phone":"";
        string layoutKey=template+suffix;
        var page=layouts.pages.Find(item=>item.template==layoutKey);
        if(page!=null)
        {
            record.referencePhone=page.phone;
            record.referenceAspectFile="ReferenceAspect/"+file;
            int width=Mathf.RoundToInt(page.phone.width*2),height=Mathf.RoundToInt(page.phone.height*2);
            Action<Camera> inspect=camera=>
            {
                Vector3[] corners=new Vector3[4];
                foreach(var box in page.boxes)
                {
                    var rect=shownRoot.Find(box.path) as RectTransform;
                    if(rect==null)throw new InvalidOperationException("Live layout target missing: "+template+"/"+box.path);
                    rect.GetWorldCorners(corners);
                    Vector2 lo=RectTransformUtility.WorldToScreenPoint(camera,corners[0]);
                    Vector2 hi=RectTransformUtility.WorldToScreenPoint(camera,corners[2]);
                    float scale=width/page.phone.width;
                    Rect actual=new Rect(page.phone.x+lo.x/scale,page.phone.y+(height-hi.y)/scale,
                        (hi.x-lo.x)/scale,(hi.y-lo.y)/scale);
                    float delta=Mathf.Max(Mathf.Abs(actual.xMin-box.reference.xMin),Mathf.Abs(actual.xMax-box.reference.xMax),
                        Mathf.Abs(actual.yMin-box.reference.yMin),Mathf.Abs(actual.yMax-box.reference.yMax));
                    record.layout.Add(new LayoutObservation {path=box.path,expected=box.reference,actual=actual,
                        visible=rect.gameObject.activeInHierarchy,maxEdgeError=delta});
                }
            };
            if(!Framework2ActualCapture.TryCapturePortrait(Path.Combine(output,record.referenceAspectFile),out error,fit,fit,width,height,inspect))
                throw new InvalidOperationException("Reference-aspect live capture failed: "+error);
        }
        report.screenshots.Add(record); report.captures++;
        WriteReport();
        Debug.Log("APPLE_SKIN_CAPTURE " + id + " " + template + " " + state);
        yield return null;
    }

    private static void CheckRuntimeRules()
    {
        int[] types = { 1, 1, 4, 2, 2 }, targets = { 1, 2, 3, 200, 3000 };
        for (int i = 0; i < 5; i++)
        {
            Require(GameManagerWZ.instance.TryGetStageRequirement(i + 1, out int type, out int target) && type == types[i] && target == targets[i],
                "Original stage " + (i + 1) + " type/target preserved.");
            Require(GameManagerWZ.instance.ShouldAutoEnterNextStage(i + 1) == (i != 2), "Original stage advancement contract preserved.");
        }
    }

    private static void ValidateSavedAssets()
    {
        report.productConfigSha256 = HashFile(ConfigPath);
        Require(report.productConfigSha256 == ExpectedConfigHash, "ProductConfig hash matches the original Framework 2 import exactly.");
        string[] roots = { "Assets/Framework2Replica/Resources/Framework2Replica", "Assets/Resources/EarningVariants" };
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", roots))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Require(prefab != null, "Saved prefab loads: " + path);
            report.prefabsChecked++;
            foreach (Transform node in prefab.GetComponentsInChildren<Transform>(true))
                Require(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(node.gameObject) == 0, "No missing MonoBehaviour: " + path + "/" + PathOf(node, prefab.transform));
            foreach (Button button in prefab.GetComponentsInChildren<Button>(true))
            {
                report.serializedButtonsChecked++;
                Require(button.onClick.GetPersistentEventCount() == 0, "No Inspector-wired Button events: " + path + "/" + button.name);
                if (button.targetGraphic == null)
                    report.warnings.Add("Standard Button has no assigned visual target: " + path + "/" + PathOf(button.transform, prefab.transform));
            }
            foreach (Component component in prefab.GetComponentsInChildren<Component>(true))
            {
                if (component == null) continue;
                var serialized = new SerializedObject(component);
                var property = serialized.GetIterator();
                while (property.NextVisible(true))
                    if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue == null &&
                        property.objectReferenceInstanceIDValue != 0)
                        Require(false, "Broken serialized object reference: " + path + "/" + component.name + "/" + property.propertyPath);
            }
        }
        // These paths are directly used by original OnAwake/runtime binding and may not be renamed by the skin.
        RequirePath("WithDraw", "Panel/PanelB/WithDraw", typeof(Button));
        RequirePath("WithDraw", "Panel/PanelA/PanelD/Process/Fill", typeof(Image));
        RequirePath("WithDrawmethod", "PanelA/PanelC/Email", typeof(InputField));
        RequirePath("WithDrawmethod", "PanelA/Confirm", typeof(Button));
        RequirePath("WithDrawConfirm", "PanelA/Confirm", typeof(Button));
        RequirePath("WithDrawQues", "PanelA/Confirm", typeof(Button));
        RequirePath("WithDrawProcess1", "PanelA/Continue", typeof(Button));
        RequirePath("WithDrawSuc", "PanelA/bg/close", typeof(Button));
    }

    private static void RequirePath(string name, string child, Type componentType)
    {
        string path = "Assets/Framework2Replica/Resources/Framework2Replica/Sources/ArrowOutRuntimeResources/prefab/" + name + ".prefab";
        var root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        Transform target = root == null ? null : root.transform.Find(child);
        Require(target != null && target.GetComponent(componentType) != null, "Original runtime key path/component preserved: " + name + "/" + child);
    }

    private static int CountActiveTemplates()
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        var excluded = new HashSet<string>(StringComparer.Ordinal) { "SelectorPanel", "RulesPanel", "AdPanel", "ErrorPanel", "WithdrawView", "WithDrawContinue", "ReStartView", "GamePlayerView + Tutorial" };
        foreach (var capture in report.screenshots) if (!excluded.Contains(capture.template)) names.Add(capture.template);
        return names.Count;
    }

    private static InputField FindInput(Transform root, string name)
    {
        foreach (InputField field in root.GetComponentsInChildren<InputField>(true)) if (field.name == name) return field;
        return null;
    }

    private static Button FindButton(Transform root, string name)
    {
        foreach (Button button in root.GetComponentsInChildren<Button>(true)) if (button.name == name) return button;
        throw new InvalidOperationException("Missing standard Button: " + name);
    }

    private static void Click(Button button, string purpose)
    {
        Require(button != null && button.isActiveAndEnabled && button.IsInteractable(), "Actionable standard Button: " + purpose);
        button.onClick.Invoke();
    }

    private static IEnumerator Seconds(double seconds)
    {
        double deadline = Time.realtimeSinceStartupAsDouble + seconds;
        while (Time.realtimeSinceStartupAsDouble < deadline) yield return null;
    }

    private static IEnumerator Until(Func<bool> ready, string operation, double seconds = 20)
    {
        double deadline = Time.realtimeSinceStartupAsDouble + seconds;
        while (!ready())
        {
            if (Time.realtimeSinceStartupAsDouble > deadline) throw new TimeoutException(operation);
            yield return null;
        }
    }

    private static IEnumerator Await(Task task, string operation)
    {
        yield return Until(() => task.IsCompleted, operation, 40);
        if (task.IsFaulted) throw new InvalidOperationException(operation, task.Exception);
        if (task.IsCanceled) throw new OperationCanceledException(operation);
    }

    private static string PathOf(Transform value, Transform root)
    {
        string path = value.name;
        while (value.parent != null && value != root && value.parent != root) { value = value.parent; path = value.name + "/" + path; }
        return path;
    }

    private static string HashFile(string path)
    {
        using (var algorithm = SHA256.Create())
        using (var stream = File.OpenRead(path)) return BitConverter.ToString(algorithm.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
    }

    private static Dictionary<string, string> HashProductionFiles()
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        if (!Directory.Exists(Application.persistentDataPath)) return values;
        foreach (string path in Directory.GetFiles(Application.persistentDataPath, "*", SearchOption.TopDirectoryOnly))
        {
            string extension = Path.GetExtension(path);
            if (extension == ".json" || extension == ".dat" || extension == ".bak") values[Path.GetFileName(path)] = HashFile(path);
        }
        return values;
    }

    private static bool SameHashes(Dictionary<string, string> a, Dictionary<string, string> b)
    {
        if (a == null || b == null || a.Count != b.Count) return false;
        foreach (var pair in a) if (!b.TryGetValue(pair.Key, out string value) || value != pair.Value) return false;
        return true;
    }

    private static void Require(bool condition, string message)
    {
        EnsureReport(); report.assertions++;
        if (!condition) throw new InvalidOperationException(message);
        report.checks.Add(message);
    }

    private static void EnsureReport()
    {
        if (report == null) report = new Report { generatedUtc = DateTime.UtcNow.ToString("O"), profile = SessionState.GetString(SessionPrefix + "Profile", "") };
        if (string.IsNullOrEmpty(output)) output = SessionState.GetString(SessionPrefix + "Output", "");
    }

    private static void OnLog(string message, string stack, LogType type)
    {
        if (finishing || !SessionState.GetBool(SessionPrefix + "Active", false)) return;
        EnsureReport();
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) report.errors.Add(message + "\n" + stack);
        else if (type == LogType.Warning) report.warnings.Add(message);
    }

    private static void WriteReport()
    {
        Directory.CreateDirectory(output);
        File.WriteAllText(Path.Combine(output, "capture-manifest.json"), JsonUtility.ToJson(report, true), new UTF8Encoding(false));
    }

    private static void WriteIndex()
    {
        var html = new StringBuilder("<!doctype html><meta charset='utf-8'><title>Framework 2 Apple skin actual runtime captures</title><style>body{background:#eee7db;color:#462d1d;font:16px system-ui;margin:28px}main{display:grid;grid-template-columns:repeat(auto-fit,minmax(250px,1fr));gap:24px}article{background:#fffaf0;padding:16px;border-radius:16px}img{width:100%;height:auto}small{display:block;overflow-wrap:anywhere}h1{font-size:28px}</style><h1>Framework 2 · Actual Unity runtime captures</h1><p>Actual saved prefab instances in Game.unity. Synthetic QA states; no live advertisement or payment. Offscreen 1080 × 1920, not physical-device or pixel-parity evidence.</p><main>");
        foreach (var capture in report.screenshots)
            html.Append("<article><h2>").Append(capture.approvalId.ToString("00")).Append(" · ").Append(capture.template)
                .Append("</h2><small>").Append(capture.state).Append("</small><a href='").Append(capture.file).Append("'><img src='")
                .Append(capture.file).Append("'></a><small>").Append(capture.fixture).Append("</small></article>");
        html.Append("</main><h2>Not captured in this run</h2><ul>");
        foreach (var omission in report.notCaptured) html.Append("<li>").Append(omission.approvalId).Append(": ").Append(omission.reason).Append("</li>");
        html.Append("</ul><p><a href='capture-manifest.json'>Full manifest, observations and limitations</a></p>");
        File.WriteAllText(Path.Combine(output, "index.html"), html.ToString(), new UTF8Encoding(false));
    }

    private static void Finish(Exception exception)
    {
        if (finishing) return;
        finishing = true; EnsureReport();
        if (exception != null) report.errors.Add(exception.ToString());
        regionalRuntime?.Dispose(); regionalRuntime = null;
        try
        {
            report.productionFilesUnchanged = SameHashes(productionFiles, HashProductionFiles());
            report.productionSelectionUnchanged = PlayerPrefs.GetInt("Apple.EarningVariant.v1", -1) == productionSelection;
            if (!report.productionFilesUnchanged) report.errors.Add("Production file hashes were not identical before/after QA.");
            if (!report.productionSelectionUnchanged) report.errors.Add("Production variant selection changed during QA.");
        }
        catch (Exception e) { report.errors.Add(e.ToString()); }
        report.passed = report.errors.Count == 0 && report.activeTemplatesCaptured == 24 && report.hostPanelsCaptured == 4;
        for (int id = 1; id <= 46; id++)
        {
            bool accountedFor = false;
            foreach (var capture in report.screenshots) if (capture.approvalId == id) { accountedFor = true; break; }
            if (!accountedFor) foreach (var missing in report.notCaptured) if (missing.approvalId == id) { accountedFor = true; break; }
            if (!accountedFor) report.notCaptured.Add(new Omission { approvalId = id, reason = "Run ended before this approval state was captured. No completeness claim is made for this state." });
        }
        SessionState.SetBool(SessionPrefix + "Active", false);
        EditorApplication.update -= Watchdog;
        EditorApplication.playModeStateChanged -= OnPlayMode;
        Application.logMessageReceived -= OnLog;
        if (installed) PlayerLoop.SetPlayerLoop(originalLoop);
        WriteReport(); WriteIndex();
        Debug.Log("APPLE_SKIN_VALIDATION " + (report.passed ? "PASS" : "FAIL") + " captures=" + report.captures + " " + output);
        EditorApplication.Exit(report.passed ? 0 : 1);
    }
}
