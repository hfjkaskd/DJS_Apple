using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Apple.Framework2;
using CorePlay;
using FruitApkEarning;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FruitEarningVariants
{
    /// <summary>Test-only host. Static UI is authored in the saved controller prefab.</summary>
    public sealed class EarningVariantController : MonoBehaviour
    {
        public const string ResourcePath = "EarningVariants/EarningVariantController";
        private const string DisclosureKey = "Apple.Framework2.SimulationDisclosure.v1";
        public Transform RuntimeParent;
        public Button OpenSelectorButton, ChooseAButton, ChooseBButton, CloseSelectorButton;
        public Button RulesButton, CloseRulesButton, CompleteAdButton, CancelAdButton;
        public GameObject SelectorPanel, RulesPanel, AdPanel, ErrorPanel;
        public Text CurrentVariantText, AdPlacementText, ErrorText;
        public Button ErrorSwitchButton;
        public static EarningVariantController Instance { get; private set; }
        public AppleF2Runtime Runtime { get; private set; }
        public bool Ready { get; private set; }
        public string InitializationError { get; private set; }
        private bool switching, paused;
        private readonly CancellationTokenSource lifetime = new CancellationTokenSource();
        private Action<bool> adCompleted;
        private bool LocalPanel => switching || !Ready || SelectorPanel.activeSelf || RulesPanel.activeSelf || AdPanel.activeSelf || ErrorPanel.activeSelf;
        public static bool HasCommercialPanel => Instance != null && Instance.LocalPanel ||
            (EarningVariantSelection.IsB ? Instance != null && Instance.Runtime != null && Instance.Runtime.HasOpenPanels : ApkEarningHost.IsOpen);
        public static bool BlocksGameplay => HasCommercialPanel ||
            (EarningVariantSelection.IsB && (Instance == null || Instance.Runtime == null || !Instance.Runtime.Ready || Instance.Runtime.HasPendingGameplayPresentation));

        private void Awake()
        {
            Instance = this;
            OpenSelectorButton.onClick.AddListener(OpenSelector);
            ChooseAButton.onClick.AddListener(ChooseA);
            ChooseBButton.onClick.AddListener(ChooseB);
            CloseSelectorButton.onClick.AddListener(CloseSelector);
            RulesButton.onClick.AddListener(OpenRules);
            CloseRulesButton.onClick.AddListener(CloseRules);
            CompleteAdButton.onClick.AddListener(CompleteAd);
            CancelAdButton.onClick.AddListener(CancelAd);
            ErrorSwitchButton.onClick.AddListener(OpenSelector);
            SelectorPanel.SetActive(false); RulesPanel.SetActive(false); AdPanel.SetActive(false); ErrorPanel.SetActive(false);
            RefreshSelection();
        }
        public async Task InitializeAsync()
        {
            AppleF2Runtime created = null;
            try
            {
                if (EarningVariantSelection.IsB)
                {
                    // No SDK, networking or automatic ad completion is connected to this host.
                    var config = await AppleF2Runtime.LoadProductConfigAsync();
                    lifetime.Token.ThrowIfCancellationRequested();
                    created = await AppleF2Runtime.CreateAsync(RuntimeParent, config, ShowLocalAd, ShowLocalAd, IsGameplayActive,
                        storageNamespace: EarningVariantSelection.Framework2Namespace, cancellationToken: lifetime.Token);
                    if (this == null || switching || lifetime.IsCancellationRequested) { created.Dispose(); return; }
                    Runtime = created;
                    Runtime.HintGranted += GrantAppleHint;
                    AppleF2Runtime.OpenHostTerms = OpenRules;
                    AppleF2Runtime.OpenHostPrivacy = OpenRules;
                    var model = JEFOMCDAPGK.Instance;
                    if (model.CurMainLevelIndex != Runtime.LinearLevelNumber)
                    {
                        model.Data.mainLevelData = new LevelData();
                        model.Data.isLevelFinished = false;
                        model.SetWinCount(Runtime.LinearLevelNumber - 1);
                    }
                    if (PlayerPrefs.GetInt(EarningVariantSelection.PreferenceKey(DisclosureKey), 0) == 0) RulesPanel.SetActive(true);
                }
                Ready = true;
                RefreshSelection();
            }
            catch (OperationCanceledException) { created?.Dispose(); }
            catch (Exception exception)
            {
                created?.Dispose(); Runtime = null;
                if (this == null) return;
                InitializationError = exception.Message;
                ErrorText.text = "Framework 2 could not load.\n" + exception.Message + "\nYour A and B saves have been retained.";
                ErrorPanel.SetActive(true);
                RefreshSelection();
                Debug.LogException(exception);
            }
        }
        public void ShowSettlement(Action nextLevel)
        {
            if (Runtime == null || !Runtime.Ready) throw new InvalidOperationException("Framework 2 is not ready.");
            Runtime.OnWin(nextLevel);
        }
        private void GrantAppleHint(int count)
        {
            if (count > 0 && !switching)
                GNEJJHEDEBL.Instance.AddItem(POJCEPBNNIP.Magic, count, false, NMNPFKEJMHO.InLevel);
        }
        private bool IsGameplayActive()
        {
            return !paused && Ready && !BlocksGameplay && !MCCIJBJGMCK.IsLock() &&
                CorePlayUI.Instance != null && CorePlayUI.Instance.IsOpening &&
                MgrUI.Instance != null && MgrUI.Instance.GetTopUI() == CorePlayUI.Instance &&
                !JEFOMCDAPGK.Instance.Data.isLevelFinished;
        }
        private void Update() { if (Runtime != null) Runtime.Tick(IsGameplayActive() ? Time.deltaTime : 0f); }
        private void OnApplicationPause(bool value) { paused = value; if (value) Runtime?.Save(); }
        private void ShowLocalAd(string placement, Action<bool> completed)
        {
            if (switching || adCompleted != null) { completed?.Invoke(false); return; }
            adCompleted = completed;
            AdPlacementText.text = "LOCAL AD SIMULATION\n" + placement + "\nNo ad is requested, no revenue is generated.";
            AdPanel.SetActive(true);
        }
        private void CompleteAd() { ResolveAd(true); }
        private void CancelAd() { ResolveAd(false); }
        private void ResolveAd(bool success)
        {
            Action<bool> completed = adCompleted; adCompleted = null;
            AdPanel.SetActive(false);
            completed?.Invoke(success);
        }
        private void OpenSelector()
        {
            if (switching || (!Ready && string.IsNullOrEmpty(InitializationError))) return;
            RefreshSelection(); SelectorPanel.SetActive(true); SelectorPanel.transform.SetAsLastSibling();
        }
        private void CloseSelector() { SelectorPanel.SetActive(false); }
        private void OpenRules() { RulesPanel.SetActive(true); RulesPanel.transform.SetAsLastSibling(); }
        private void CloseRules()
        {
            RulesPanel.SetActive(false);
            if (EarningVariantSelection.IsB) { PlayerPrefs.SetInt(EarningVariantSelection.PreferenceKey(DisclosureKey), 1); PlayerPrefs.Save(); }
        }
        private void ChooseA() { Select(false); }
        private void ChooseB() { Select(true); }
        private void RefreshSelection()
        {
            CurrentVariantText.text = "Active: " + EarningVariantSelection.Label + "\nSeparate progress and balances. Switching reloads the saved game.";
            bool canSwitch = !switching && (Ready || !string.IsNullOrEmpty(InitializationError));
            OpenSelectorButton.interactable = canSwitch;
            ChooseAButton.interactable = canSwitch && EarningVariantSelection.IsB;
            ChooseBButton.interactable = canSwitch && !EarningVariantSelection.IsB;
        }
        private void Select(bool useB)
        {
            if (switching || (!Ready && string.IsNullOrEmpty(InitializationError)) || useB == EarningVariantSelection.IsB) return;
            StartCoroutine(SwitchRoutine(useB));
        }
        private IEnumerator SwitchRoutine(bool useB)
        {
            switching = true; RefreshSelection();
            // Freeze gameplay first. Keep the session selection unchanged while its models save.
            FruitWithdrawal.WithdrawalGameplayBridge.Flush();
            bool saved = EEEHAEKANLA.Instance.TrySaveToDisk() & BMNFNJFCPHG.Instance.TrySaveToDisk() &
                GNEJJHEDEBL.Instance.TrySaveToDisk() & JEFOMCDAPGK.Instance.TrySaveToDisk() &
                FPFGGCMEDND.Instance.TrySaveToDisk() & KDBNHIIFGLD.Instance.TrySaveToDisk();
            if (!saved || (!EarningVariantSelection.IsB && !string.IsNullOrEmpty(ApkEarningService.Instance.Error)))
            {
                switching = false;
                ErrorText.text = "Progress could not be saved. The active variant has been retained.\nFree storage and retry the switch.";
                ErrorPanel.SetActive(true); ErrorPanel.transform.SetAsLastSibling();
                RefreshSelection();
                yield break;
            }
            if (!EarningVariantSelection.IsB && !ApkEarningService.Instance.EndSession())
            {
                switching = false;
                ErrorText.text = "The current reward session could not be saved. The active variant has been retained.";
                ErrorPanel.SetActive(true); ErrorPanel.transform.SetAsLastSibling(); RefreshSelection();
                yield break;
            }
            Framework.Base.UtilModule.CoroutineManager.Instance.StopAllCoroutines();
            Timer.Instance.StopAllCoroutines();
            DG.Tweening.DOTween.KillAll(false);
            if (Runtime != null) Runtime.HintGranted -= GrantAppleHint;
            Runtime?.Dispose(); Runtime = null; adCompleted = null;
            EarningVariantSelection.SaveNextSelection(useB);
            yield return null; // Let old source singletons finish OnDestroy before any new B instance.
            AsyncOperation load = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
            while (!load.isDone) yield return null;
        }
        private void OnDestroy()
        {
            lifetime.Cancel();
            if (Runtime != null) Runtime.HintGranted -= GrantAppleHint;
            Runtime?.Dispose(); Runtime = null; adCompleted = null;
            if (Instance == this)
            {
                Instance = null;
                AppleF2Runtime.OpenHostTerms = null; AppleF2Runtime.OpenHostPrivacy = null;
            }
        }
    }
}
