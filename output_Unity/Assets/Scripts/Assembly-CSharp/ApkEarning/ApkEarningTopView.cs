using System;
using System.Collections;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FruitApkEarning
{
    /// <summary>Refreshes the APK's saved top prefab; static layout stays in the prefab.</summary>
    public sealed class ApkEarningTopView : MonoBehaviour
    {
        [Serializable] public sealed class LevelDisplay
        {
            public int level, showLevel, totalRound, round, subTotalRound, subRound;
        }
        [Serializable] public sealed class DotSlot { public GameObject Root, Left, Right; public Image Icon; }
        public ApkEarningPageView View;
        public bool CompactHud;
        public Sprite NormalMilestone, PassedMilestone, ActiveMilestone;
        public Sprite PendingDot, PassedDot;
        public LevelDisplay[] Levels = Array.Empty<LevelDisplay>();
        public DotSlot[] SubRoundDots = Array.Empty<DotSlot>();
        public RectTransform LevelSettingsRow, GameplayViewport;
        public float ReferenceWidth = 1080, ReferenceSettingsY = -520, ReferenceViewportTop = 600;
        public Vector2 ReferencePosition = new Vector2(0, 182);
        [Min(.1f)] public float RewardPollSeconds = .5f;
        [Min(.1f)] public float BlinkSeconds = 1;
        [Range(0, 1)] public float BlinkAlpha = .5f;
        private ApkEarningService service;
        private CorePlay.CorePlayUI owner;
        private Coroutine polling;
        private string lastAutomaticReward;
        private bool bound;
        private Image blinking;
        private float blinkStart;
        private bool fittingLayout;
        public static Transform CashReceiptTarget { get; private set; }

        private void OnEnable()
        {
            if (View == null) View = GetComponent<ApkEarningPageView>();
            owner = GetComponentInParent<CorePlay.CorePlayUI>();
            service = ApkEarningService.Instance;
            var cashTarget = View.GetButton("OriginalGoldItem.click");
            CashReceiptTarget = cashTarget == null ? null : cashTarget.transform;
            service.Changed += Refresh;
            View.Bind("OriginalGoldItem.click", OpenCash);
            View.Bind("OriginalCoinItem.click", OpenDiamonds);
            View.Bind("CompactTaskView", OpenCash);
            for (int i = 0; i < 5; i++) View.Bind("MilestoneButton[" + i + "]", OpenCash);
            bound = true; lastAutomaticReward = null;
            RefreshLayout();
            Refresh();
            polling = StartCoroutine(WatchPendingRewards());
        }
        private void OnDisable()
        {
            var cashTarget = View == null ? null : View.GetButton("OriginalGoldItem.click");
            if (cashTarget != null && CashReceiptTarget == cashTarget.transform) CashReceiptTarget = null;
            if (bound && service != null) service.Changed -= Refresh;
            bound = false;
            if (View != null) View.Unbind();
            if (polling != null) StopCoroutine(polling);
            polling = null;
            blinking = null;
        }
        private void OnRectTransformDimensionsChange() { RefreshLayout(); }
        public void RefreshLayout()
        {
            if (fittingLayout || ReferenceWidth <= 0) return;
            RectTransform rect = transform as RectTransform;
            RectTransform parent = transform.parent as RectTransform;
            if (rect == null || parent == null || parent.rect.width <= 0) return;
            fittingLayout = true;
            try
            {
                float scale = parent.rect.width / ReferenceWidth;
                rect.sizeDelta = new Vector2(ReferenceWidth - parent.rect.width, rect.sizeDelta.y);
                rect.localScale = Vector3.one * scale; rect.anchoredPosition = ReferencePosition * scale;
                if (LevelSettingsRow != null)
                {
                    LevelSettingsRow.localScale = Vector3.one * scale;
                    LevelSettingsRow.anchoredPosition = new Vector2(0, ReferenceSettingsY * scale);
                }
                if (GameplayViewport != null)
                    GameplayViewport.offsetMax = new Vector2(GameplayViewport.offsetMax.x, -ReferenceViewportTop * scale);
            }
            finally { fittingLayout = false; }
        }
        private bool CanOpen()
        {
            return owner != null && owner.IsOpening && MgrUI.Instance != null &&
                   MgrUI.Instance.GetTopUI() == owner && !MCCIJBJGMCK.IsLock() && !ApkEarningHost.IsOpen;
        }
        private void OpenCash() { if (CanOpen()) ApkEarningHost.Open(false, false); }
        private void OpenDiamonds() { if (CanOpen()) ApkEarningHost.Open(true, false); }
        private IEnumerator WatchPendingRewards()
        {
            var delay = new WaitForSecondsRealtime(Mathf.Max(.1f, RewardPollSeconds));
            while (true)
            {
                yield return delay;
                if (!service.Available || service.State == null ||
                    JEFOMCDAPGK.Instance.Data.isLevelFinished || !CanOpen()) continue;
                ApkEarningState state = service.State;
                // Level rewards belong to the settlement queue, which also owns
                // the callback that starts the next board after the page closes.
                if (state.PendingRewards.Count > 0 && state.PendingRewards[0].Kind != "lucky") continue;
                string pending = !state.WelcomeShown && state.CurrentLevel == 1 ? "welcome" : state.ActivationPending ? "activation" : state.PendingRewards.Count > 0 ? state.PendingRewards[0].Id :
                    !state.DiamondGuideShown && state.CurrentLevel >= service.Rules.DiamondFirstLevel ? "diamond-guide" : null;
                if (pending == null || pending == lastAutomaticReward) continue;
                lastAutomaticReward = pending;
                ApkEarningHost.Open(false, true);
            }
        }
        public void Refresh()
        {
            if (View == null || service == null) return;
            bool ready = service.Available && service.State != null;
            Button cash = View.GetButton("OriginalGoldItem.click"), diamond = View.GetButton("OriginalCoinItem.click");
            Button task = View.GetButton("CompactTaskView");
            if (cash != null) cash.interactable = ready || !string.IsNullOrEmpty(service.Error);
            if (diamond != null) diamond.interactable = ready;
            if (task != null) task.interactable = ready || !string.IsNullOrEmpty(service.Error);
            if (!ready)
            {
                View.SetText("OriginalGoldItem.value", "--"); View.SetText("OriginalCoinItem.value", "--");
                if (CompactHud)
                {
                    blinking = null;
                    View.SetText("CompactTaskLabel", "Local rewards unavailable");
                    View.SetText("CompactTaskRatio", "--");
                    View.SetProgress("CompactTaskProgress", 0);
                }
                else
                {
                    View.SetText("OriginalGoldItem.goldHintText", "Local rewards unavailable");
                    View.SetText("OriginalCoinItem.hintText", "Local rewards unavailable");
                }
                return;
            }
            ApkEarningState state = service.State;
            bool showDiamonds = state.DiamondGuideShown || state.CurrentLevel >= service.Rules.DiamondFirstLevel;
            View.SetVisible("CoinItem", CompactHud || showDiamonds);
            if (diamond != null) diamond.interactable = showDiamonds;
            View.SetText("OriginalGoldItem.value", Money(state.CashCents));
            View.SetText("OriginalCoinItem.value", (state.Diamonds / (decimal)ApkEarningRules.DiamondScale).ToString("0.##", CultureInfo.InvariantCulture));
            if (CompactHud)
            {
                RefreshCompactTask(state);
                return;
            }
            View.SetText("OriginalGoldItem.goldHintText", state.CurrentLevel <= service.Rules.FirstCashGate ?
                "<color=#FF0000>Clear this level</color> Withdraw all" : "Clear level <color=#FF0000>" + DisplayLevel(service.Rules.SecondCashGate) + "</color> Withdraw all");
            long remaining = Math.Max(0, service.Rules.DiamondTargets[0] - state.Diamonds);
            View.SetText("OriginalCoinItem.hintText", "Earn <color=#FF0000>" + Money((remaining + 99) / 100) +
                "</color> more Withdraw <color=#FF0000>" + Money(service.Rules.DiamondPayoutCents[0], true) + "</color>");
            View.SetVisible("GoldItem/Hint", state.CurrentLevel <= service.Rules.SecondCashGate);
            View.SetVisible("CoinItem/Hint", remaining > 0);
            RefreshMilestones(state);
        }
        private void RefreshCompactTask(ApkEarningState state)
        {
            blinking = null;
            // The wallet resumes an unarchived early reward before its next target.
            foreach (ApkRecord record in state.Records)
            {
                if (record.Archived) continue;
                View.SetText("CompactTaskLabel", "Review cash reward");
                View.SetText("CompactTaskRatio", "1 / 1");
                View.SetProgress("CompactTaskProgress", 1);
                return;
            }
            ApkRequirements requirements = service.GetRequirements(false, 0);
            ApkRequirement next = null;
            foreach (ApkRequirement item in requirements.Items)
                if (!item.Complete) { next = item; break; }
            if (next == null)
            {
                View.SetText("CompactTaskLabel", "Ready to review");
                View.SetText("CompactTaskRatio", "Complete");
                View.SetProgress("CompactTaskProgress", 1);
                return;
            }
            if (!next.Known || next.Target <= 0)
            {
                View.SetText("CompactTaskLabel", "Awaiting account update");
                View.SetText("CompactTaskRatio", "--");
                View.SetProgress("CompactTaskProgress", 0);
                return;
            }

            long current = Math.Max(0, Math.Min(next.Current, next.Target));
            long target = next.Target;
            string label = RequirementName(next.Key);
            if (next.Key == "firstLevel" || next.Key == "secondLevel")
            {
                // The fruit game's header counts individual boards. Keep the
                // wallet and task on that same scale without changing eligibility.
                label = "Clear level " + target.ToString(CultureInfo.InvariantCulture);
            }
            string ratio = next.Key == "balance" ? Money(current) + " / " + Money(target) :
                current.ToString(CultureInfo.InvariantCulture) + " / " + target.ToString(CultureInfo.InvariantCulture);
            View.SetText("CompactTaskLabel", label);
            View.SetText("CompactTaskRatio", ratio);
            View.SetProgress("CompactTaskProgress", target > 0 ? (float)current / target : 0);
        }
        private void RefreshMilestones(ApkEarningState state)
        {
            bool first = state.CurrentLevel <= service.Rules.FirstCashGate;
            bool milestones = state.CurrentLevel <= service.Rules.SecondCashGate;
            View.SetVisible("HideLevel", milestones); View.SetVisible("TX_JD", !milestones);
            LevelDisplay display = FindLevel(state.CurrentLevel);
            int shown = display == null ? state.CurrentLevel : display.showLevel;
            if (milestones)
            {
                float fill = first ? Mathf.Clamp01((shown - 1) / 4f) : shown < 20 ? Mathf.Clamp01((shown - 5) / 20f) : .75f + Mathf.Clamp01((shown - 20) / 10f) * .25f;
                View.SetProgress("HideLevel/Progress/Bar", fill);
                for (int i = 0; i < 5; i++)
                {
                    int marker = first ? i + 1 : i == 4 ? 30 : (i + 1) * 5;
                    string path = "HideLevel/Levels/" + (i + 1);
                    View.SetText(path + "/Level", marker.ToString(CultureInfo.InvariantCulture));
                    View.SetVisible(path + "/Get", first ? i == 4 || (marker >= state.CurrentLevel && i != 2) : i == 4);
                    if (i == 4) continue; // The APK's last marker keeps its crown artwork.
                    Image background = View.GetImage(path + "/BG");
                    if (background != null)
                    {
                        int next = first ? marker + 1 : i == 3 ? 30 : marker + 5;
                        background.sprite = shown >= next ? PassedMilestone : shown < marker ? NormalMilestone : ActiveMilestone;
                    }
                }
                bool round4 = display != null && shown == 4 && display.round > 1;
                bool round5 = display != null && (shown == 5 || (!first && shown == 30)) && display.round > 1;
                View.SetVisible("HideLevel/Levels/4/Round", round4); View.SetVisible("HideLevel/Levels/5/Round", round5);
                if (display != null)
                {
                    string rounds = display.round + "/" + display.totalRound;
                    View.SetText("HideLevel/Levels/4/Round/Value", rounds);
                    View.SetText("HideLevel/Levels/5/Round/Value", rounds);
                }
                int count = display == null ? 0 : display.subTotalRound;
                View.SetVisible("HideLevel/SubRounds", count > 0);
                blinking = null;
                for (int i = 0; i < SubRoundDots.Length; i++)
                {
                    DotSlot dot = SubRoundDots[i];
                    dot.Root.SetActive(i < count);
                    if (i >= count) continue;
                    dot.Left.SetActive(i > 0); dot.Right.SetActive(i < count - 1);
                    dot.Icon.sprite = i < display.subRound ? PassedDot : PendingDot;
                    dot.Icon.color = Color.white;
                    if (i == display.subRound - 1) blinking = dot.Icon;
                }
                blinkStart = Time.unscaledTime;
            }
            else
            {
                ApkRequirements requirements = service.GetRequirements(false, 0);
                ApkRequirement next = null;
                foreach (ApkRequirement item in requirements.Items) if (!item.Complete) { next = item; break; }
                View.SetText("OriginalRewardProgressView.goldValue", "Withdraw All");
                View.SetText("OriginalRewardProgressView.tip", next == null ? "Requirements complete" : !next.Known ? "Awaiting account update" : RequirementName(next.Key));
                View.SetText("OriginalRewardProgressView.progressValue", next == null ? "Complete" : !next.Known ? "--" : next.Current + "/" + next.Target);
                View.SetProgress("OriginalRewardProgressView.bar", next == null ? 1 : !next.Known || next.Target <= 0 ? 0 : (float)next.Current / next.Target);
            }
        }
        private void Update()
        {
            if (blinking == null || !blinking.gameObject.activeInHierarchy) return;
            float t = Mathf.PingPong(Time.unscaledTime - blinkStart, BlinkSeconds) / BlinkSeconds;
            Color color = blinking.color; color.a = Mathf.Lerp(1, BlinkAlpha, 1 - (1 - t) * (1 - t)); blinking.color = color;
        }
        private LevelDisplay FindLevel(int level)
        {
            foreach (LevelDisplay item in Levels) if (item.level == level) return item;
            return null;
        }
        private int DisplayLevel(int actual)
        { LevelDisplay item = FindLevel(actual); return item == null ? actual : item.showLevel; }
        private static string RequirementName(string key)
        {
            switch (key)
            {
                case "balance": return "Required balance";
                case "guide": return "Complete cash guidance";
                case "daily": return "Levels completed today";
                case "login": return "Login days";
                case "rank": return "Account level";
                default: return "Withdrawal requirements";
            }
        }
        private static string Money(long cents, bool whole = false)
        { return "$" + (cents / 100m).ToString(whole ? "#,0.##" : "#,0.00", CultureInfo.InvariantCulture); }
    }
}
