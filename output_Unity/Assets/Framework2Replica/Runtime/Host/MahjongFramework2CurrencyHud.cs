using UnityEngine;
using UnityEngine.UI;
using WZSDK;

namespace MahjongSlide.Withdrawal
{
    // Explicit bindings for the source HUD subtrees. GamePlayerView
    // still owns native currency, level, stage and lucky-progress presentation.
    [DisallowMultipleComponent]
    public sealed class MahjongFramework2CurrencyHud : MonoBehaviour
    {
        [SerializeField] private GamePlayerView sourceView;
        [SerializeField] private Text cashText;
        [SerializeField] private Text gemText;
        [SerializeField] private RectTransform cashTarget;
        [SerializeField] private RectTransform gemTarget;
        [SerializeField] private Button cashButton;
        [SerializeField] private Button gemButton;
        [SerializeField] private GameObject gemRoot;
        [SerializeField] private Button settingsButton;
        [SerializeField] private ProgressBindings progress;
        [SerializeField] private RectTransform reservedProgressRegion;
        [SerializeField, Min(0f)] private float gameplayTopPadding;
        [SerializeField, Min(0f)] private float secondaryCurrencyRowHeight;
        private readonly Vector3[] graphicCorners = new Vector3[4];
        private Canvas sourceCanvas;
        private Graphic[] headerGraphics;
        private RectTransform withdrawPromptRect;
        private Vector2 stagePosition;
        private Vector2 luckyPosition;
        private Vector2 withdrawPromptPosition;
        private bool secondaryCurrencyVisible;
        private readonly MahjongCashCounter cashCounter = new MahjongCashCounter();

        private void Awake()
        {
            sourceCanvas = GetComponentInParent<Canvas>();
            headerGraphics = GetComponentsInChildren<Graphic>(true);
            if (progress == null) return;
            if (progress.stageRoot != null) stagePosition = progress.stageRoot.anchoredPosition;
            if (progress.luckyRoot != null) luckyPosition = progress.luckyRoot.anchoredPosition;
            withdrawPromptRect = progress.withdrawPrompt != null ? progress.withdrawPrompt.transform as RectTransform : null;
            if (withdrawPromptRect != null) withdrawPromptPosition = withdrawPromptRect.anchoredPosition;
            RefreshCurrencyRows();
        }

        private void OnEnable()
        {
            cashCounter.Clear();
            if (GameManagerWZ.instance != null) RefreshCash(GameManagerWZ.instance.currentCoin);
        }

        private void OnDisable() { MahjongRewardAudio.CounterStopped(this); cashCounter.Clear(); }

        private void LateUpdate()
        {
            RefreshCurrencyRows();
            decimal previous = cashCounter.Displayed;
            string previousText = cashText == null ? null : cashText.text;
            if (cashCounter.Tick(Time.unscaledTime))
            {
                RenderCash();
                if (cashText != null && cashText.isActiveAndEnabled && cashText.text != previousText && cashCounter.Displayed > previous)
                    MahjongRewardAudio.CounterAdvanced(this);
            }
            if (!cashCounter.IsAnimating || cashText == null || !cashText.isActiveAndEnabled)
                MahjongRewardAudio.CounterStopped(this);
        }

        internal void RefreshCash(double amount)
        {
            if (cashText == null || double.IsNaN(amount) || double.IsInfinity(amount)) return;
            if (!isActiveAndEnabled) cashCounter.Clear();
            var settings = Framework2Host.Settings;
            cashCounter.SetBalance((decimal)amount,
                settings != null ? settings.hudBalanceAnimationSeconds : 0f, Time.unscaledTime);
            RenderCash();
        }

        internal MahjongCashCounter.Flight HoldCashFlight(float amount)
        {
            if (!isActiveAndEnabled || cashText == null) return null;
            var settings = Framework2Host.Settings;
            return settings == null ? null : cashCounter.Hold((decimal)amount,
                settings.hudBalanceFlightTimeoutSeconds, Time.unscaledTime, true);
        }

        private void RenderCash()
        {
            if (cashText == null || !cashCounter.Initialized || GameManagerWZ.instance == null) return;
            double amount = (double)cashCounter.Displayed;
            string formatted = FacadePayTypeExtend.RegionalChangeHandle != null
                ? FacadePayTypeExtend.RegionalChangeHandle((float)amount)
                : GameManagerWZ.instance.FormatRegionalValue(amount);
            if (cashText.text != formatted) cashText.text = formatted;
        }

        private void RefreshCurrencyRows()
        {
            bool visible = gemRoot != null && gemRoot.activeSelf;
            if (secondaryCurrencyVisible == visible || progress == null) return;
            secondaryCurrencyVisible = visible;
            Vector2 offset = visible ? Vector2.down * secondaryCurrencyRowHeight : Vector2.zero;
            if (progress.stageRoot != null) progress.stageRoot.anchoredPosition = stagePosition + offset;
            if (progress.luckyRoot != null) progress.luckyRoot.anchoredPosition = luckyPosition + offset;
            if (withdrawPromptRect != null) withdrawPromptRect.anchoredPosition = withdrawPromptPosition + offset;
        }

        // Keep the source TopButtons region reserved even before its stage bar
        // unlocks. Visible rows extending below it can only enlarge this inset.
        public float GetGameplayTopInset(float canvasHeight)
        {
            if (sourceCanvas == null || headerGraphics == null || Screen.height <= 0) return 0f;
            RefreshCurrencyRows();
            Camera camera = sourceCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : sourceCanvas.worldCamera;
            float bottom = Screen.height;
            if (reservedProgressRegion != null)
            {
                reservedProgressRegion.GetWorldCorners(graphicCorners);
                bottom = Mathf.Min(bottom, RectTransformUtility.WorldToScreenPoint(camera, graphicCorners[0]).y);
            }
            for (int i = 0; i < headerGraphics.Length; i++)
            {
                Graphic graphic = headerGraphics[i];
                if (graphic == null || !graphic.isActiveAndEnabled || graphic.color.a <= 0f) continue;
                graphic.rectTransform.GetWorldCorners(graphicCorners);
                bottom = Mathf.Min(bottom, RectTransformUtility.WorldToScreenPoint(camera, graphicCorners[0]).y);
            }
            var canvasRect = sourceCanvas.transform as RectTransform;
            float paddingScale = canvasRect != null && canvasRect.rect.height > 0f ? canvasHeight / canvasRect.rect.height : 1f;
            return Mathf.Clamp01(1f - bottom / Screen.height) * canvasHeight + gameplayTopPadding * paddingScale;
        }

        [System.Serializable]
        public sealed class ProgressBindings
        {
            public GameObject levelRoot;
            public Text levelText;
            public GameObject tipsRoot;
            public Text tipsText;
            public GameObject withdrawTop;
            public GameObject[] levelMarkers;
            public GameObject[] levelArrows;
            public GameObject withdrawPrompt;
            public GameObject stageIcon;
            public Image withdrawalFill;
            public Text withdrawalDescription;
            public Text withdrawalCount;
            public RectTransform stageRoot;
            public RectTransform stageStateRoot;
            public Text stageDescription;
            public Text stageProgressText;
            public Text stageOverallCount;
            public Image stageFill;
            public Image stageOverallFill;
            public WithdrawMissionStateItem[] stageStates;
            public RectTransform luckyRoot;
            public RectTransform luckyStateRoot;
            public Image luckyFill;
            public Text luckyCurrentCoin;
            public LuckyItemProgress[] luckyStates;
        }

        public Text CashText => cashText;
        public Text GemText => gemText;
        public RectTransform CashTarget => cashTarget;
        public RectTransform GemTarget => gemTarget;
        public Button CashButton => cashButton;
        public Button GemButton => gemButton;
        public GameObject GemRoot => gemRoot;
        public Button SettingsButton => settingsButton;
        public ProgressBindings Progress => progress;
        public bool HasRequiredBindings => sourceView != null && cashText != null &&
            gemText != null && cashTarget != null && gemTarget != null &&
            cashButton != null && gemButton != null && gemRoot != null && settingsButton != null &&
            reservedProgressRegion != null && progress != null && progress.levelRoot != null && progress.levelText != null &&
            progress.withdrawPrompt != null && progress.stageRoot != null &&
            progress.stageStates != null && progress.luckyRoot != null &&
            progress.luckyStates != null;
    }
}
