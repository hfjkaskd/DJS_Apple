using System;
using System.Collections;
using System.Text;
using UnityEngine;

namespace FruitWithdrawal
{
    public enum WithdrawalScreen { Cash, Diamonds, Channel, Confirm, Conditions, Review, Complete, Rejected, Records, Rewards, Rules }

    [Serializable]
    public sealed class WithdrawalScreenDefinition
    {
        public WithdrawalScreen Screen;
        public string ResourcePath;
    }

    public sealed class WithdrawalHost : BaseUI
    {
        public const string Path = "withdrawal/WithdrawalHost";
        [SerializeField] public WithdrawalScreenDefinition[] Screens;
        [SerializeField] public RectTransform PageRoot;
        private static WithdrawalScreen requestedScreen;
        private static WithdrawalCurrency requestedCurrency;
        private WithdrawalScreen screen;
        private WithdrawalScreen rulesReturnScreen;
        private WithdrawalPageView page;
        private WithdrawalCurrency currency;
        private string selectedTier;
        private string selectedRequest;
        private string rulesRequest;
        private int selectedChannel;
        private string notice = "";
        private Coroutine ticker;
        private bool changingPage;
        private bool refreshing;
        private bool refreshPending;
        private bool handlingAction;
        private bool opened;
        private WithdrawalService Service => WithdrawalService.Instance;
        public override PAIEAGDLCBJ Layer => PAIEAGDLCBJ.Top;
        private WithdrawalScreen WalletScreen => currency == WithdrawalCurrency.Cash ? WithdrawalScreen.Cash : WithdrawalScreen.Diamonds;

        public static void Open(WithdrawalCurrency kind, bool rewards = false)
        {
            requestedCurrency = kind;
            requestedScreen = rewards ? WithdrawalScreen.Rewards : kind == WithdrawalCurrency.Cash ? WithdrawalScreen.Cash : WithdrawalScreen.Diamonds;
            if (MgrUI.Instance != null && !MCCIJBJGMCK.IsLock()) MgrUI.Instance.Open(Path);
        }

        protected override void Init() { }

        protected override void BeforeOpen()
        {
            notice = "";
            selectedRequest = null;
            rulesRequest = null;
            currency = requestedCurrency;
            EnsureTier();
            Service.Changed -= OnServiceChanged;
            opened = false;
            Service.Tick();
            opened = true;
            Service.Changed += OnServiceChanged;
            Show(requestedScreen);
        }

        protected override void AfterOpen() { ticker = StartCoroutine(TickWhileOpen()); }

        protected override void BeforeClose()
        {
            opened = false;
            refreshPending = false;
            Service.Changed -= OnServiceChanged;
            if (ticker != null) StopCoroutine(ticker);
            ticker = null;
        }

        private IEnumerator TickWhileOpen()
        {
            var wait = new WaitForSecondsRealtime(1);
            while (true) { yield return wait; if (!Service.Tick()) Refresh(); }
        }

        private void OnServiceChanged()
        {
            if (opened) Refresh();
        }

        private void Show(WithdrawalScreen next)
        {
            if (changingPage) return;
            changingPage = true;
            try {
                if (page != null) {
                    page.gameObject.SetActive(false);
                    Destroy(page.gameObject);
                }
                page = null;
                screen = next;
                if (Screens != null) for (int i = 0; i < Screens.Length; i++) {
                    if (Screens[i] == null || Screens[i].Screen != next) continue;
                    GameObject prefab = Resources.Load<GameObject>(Screens[i].ResourcePath);
                    if (prefab != null) {
                        GameObject instance = Instantiate(prefab, PageRoot, false);
                        page = instance.GetComponent<WithdrawalPageView>();
                        if (page != null) page.Bind(Handle);
                        else Destroy(instance);
                    }
                    break;
                }
            }
            finally { changingPage = false; }
            if (page == null) { Debug.LogError("[Withdrawal] Missing configured page."); return; }
            Refresh();
        }

        private WithdrawalTierRule Tier
        {
            get {
                var rules = Service.Rules;
                if (rules == null || rules.Tiers == null) return null;
                for (int i = 0; i < rules.Tiers.Length; i++) if (rules.Tiers[i].Id == selectedTier) return rules.Tiers[i];
                return null;
            }
        }

        private void EnsureTier()
        {
            if (Tier != null && Tier.Currency == currency) return;
            selectedTier = null;
            if (Service.Rules == null) return;
            for (int i = 0; i < Service.Rules.Tiers.Length; i++)
                if (Service.Rules.Tiers[i].Currency == currency) { selectedTier = Service.Rules.Tiers[i].Id; return; }
        }

        private WithdrawalRequest FindRequest(string id)
        {
            if (Service.State == null || string.IsNullOrEmpty(id)) return null;
            for (int i = Service.State.Requests.Count - 1; i >= 0; i--)
                if (Service.State.Requests[i].RequestId == id) return Service.State.Requests[i];
            return null;
        }

        private WithdrawalRequest Request => !string.IsNullOrEmpty(selectedRequest) ? FindRequest(selectedRequest) : Service.State?.ActiveRequest;

        private WithdrawalChannelRule Channel
        {
            get {
                var channels = Service.Rules?.Channels;
                return channels != null && selectedChannel >= 0 && selectedChannel < channels.Length && selectedChannel < 2 ? channels[selectedChannel] : null;
            }
        }

        private void Refresh()
        {
            if (!opened) return;
            // Apply/claim can synchronously publish Changed. Render only after the action has selected its final page/request.
            if (handlingAction || refreshing || changingPage) { refreshPending = true; return; }
            if (page == null) return;
            refreshing = true;
            try {
                do {
                    refreshPending = false;
                    RenderPage();
                } while (refreshPending && opened && page != null);
            }
            finally { refreshing = false; }
        }

        private void RenderPage()
        {
            page.SetText("test", "TEST MODE • NO CASH VALUE");
            if (!Service.IsAvailable || Service.State == null) {
                page.SetText("notice", "Simulation unavailable. " + Service.LastError);
                page.SetButton(WithdrawalAction.Apply, false);
                page.SetButton(WithdrawalAction.Confirm, false);
                page.SetButton(WithdrawalAction.Claim, false);
                page.SetButton(WithdrawalAction.Cancel, false);
                page.SetButton(WithdrawalAction.CompleteTest, false);
                page.SetButton(WithdrawalAction.RejectTest, false);
                return;
            }
            page.SetText("notice", notice);
            page.SetText("cash", Money(Service.State.CashCents));
            page.SetText("diamonds", WithdrawalPresentation.Number(Service.State.DiamondUnits));
            long frozen = currency == WithdrawalCurrency.Cash ? Service.State.FrozenCashCents : Service.State.FrozenDiamondUnits;
            page.SetText("frozen", "In application: " + WithdrawalPresentation.Units(currency, frozen));
            if (screen == WithdrawalScreen.Cash || screen == WithdrawalScreen.Diamonds) RenderWallet();
            else if (screen == WithdrawalScreen.Channel) RenderChannel();
            else if (screen == WithdrawalScreen.Confirm) RenderConfirm();
            else if (screen == WithdrawalScreen.Rules) RenderRules();
            else if (screen == WithdrawalScreen.Records) RenderRecords();
            else if (screen == WithdrawalScreen.Rewards) RenderRewards();
            else RenderRequest();
        }

        private bool IsCompleted(string tierId)
        {
            for (int i = 0; i < Service.State.Requests.Count; i++)
                if (Service.State.Requests[i].TierId == tierId && Service.State.Requests[i].Status == WithdrawalRequestStatus.SimulatedCompleted) return true;
            return false;
        }

        private bool CanApply(WithdrawalTierRule tier)
        {
            return tier != null && Service.State.ActiveRequest == null && !IsCompleted(tier.Id)
                && WithdrawalPresentation.Balance(Service.State, tier.Currency) >= tier.RequiredBalanceUnits;
        }

        private void RenderWallet()
        {
            EnsureTier();
            var tier = Tier;
            int index = 0;
            for (int i = 0; i < Service.Rules.Tiers.Length && index < 4; i++) {
                var item = Service.Rules.Tiers[i];
                if (item.Currency != currency) continue;
                page.SetText("tier" + index, WithdrawalPresentation.TierAmount(item.DisplayCents));
                page.SetText("tierCost" + index, WithdrawalPresentation.Number(item.RequiredBalanceUnits));
                page.SetVisible("selected" + index, item.Id == selectedTier);
                page.SetVisible("selectedAmount" + index, item.Id == selectedTier);
                index++;
            }
            for (int i = 0; i < 4; i++) {
                page.SetVisible("tier" + i, i < index);
                if (i >= index) {
                    page.SetVisible("selected" + i, false);
                    page.SetVisible("selectedAmount" + i, false);
                }
            }
            if (tier == null) { page.SetButton(WithdrawalAction.Apply, false); return; }
            long balance = WithdrawalPresentation.Balance(Service.State, currency);
            long remaining = Math.Max(0, tier.RequiredBalanceUnits - balance);
            page.SetText("amount", Money(tier.DisplayCents));
            page.SetText("balance", currency == WithdrawalCurrency.Cash ? Money(balance) : WithdrawalPresentation.Number(balance));
            page.SetText("target", remaining == 0 ? "Balance target reached" : "Earn " + WithdrawalPresentation.Units(currency, remaining) + " more");
            page.SetText("ratio", WithdrawalPresentation.Percentage(balance, tier.RequiredBalanceUnits));
            page.SetProgress(WithdrawalPresentation.Ratio(balance, tier.RequiredBalanceUnits));
            bool active = Service.State.ActiveRequest != null;
            bool completed = IsCompleted(tier.Id);
            page.SetText("apply", active ? "VIEW APPLICATION" : completed ? "TEST COMPLETED" : remaining > 0 ? "KEEP HARVESTING" : "CHOOSE CHANNEL");
            page.SetButton(WithdrawalAction.Apply, active || CanApply(tier));
            page.SetText("rewards", Service.State.PendingRewards.Count > 0 ? "Rewards (" + Service.State.PendingRewards.Count + ")" : "Rewards");
            page.SetText("summary", WithdrawalPresentation.WalletSummary(tier));
        }

        private void RenderChannel()
        {
            var tier = Tier;
            page.SetText("amount", tier == null ? "Choose a target" : Money(tier.DisplayCents));
            for (int i = 0; i < 2; i++) {
                bool exists = i < Service.Rules.Channels.Length;
                page.SetText("channel" + i, exists ? WithdrawalPresentation.ChannelLabel(Service.Rules.Channels[i].Label) : "Unavailable");
                page.SetVisible("channelSelected" + i, exists && selectedChannel == i);
                page.SetButton(WithdrawalAction.SelectTier, exists, i);
            }
            page.SetButton(WithdrawalAction.Confirm, Channel != null && CanApply(tier));
        }

        private void RenderConfirm()
        {
            var tier = Tier;
            var channel = Channel;
            if (tier == null) { page.SetButton(WithdrawalAction.Confirm, false); return; }
            long balance = WithdrawalPresentation.Balance(Service.State, tier.Currency);
            page.SetText("amount", Money(tier.DisplayCents));
            page.SetText("channel", channel == null ? "Select a test channel" : WithdrawalPresentation.ChannelLabel(channel.Label));
            page.SetText("reserve", WithdrawalPresentation.Units(tier.Currency, tier.RequiredBalanceUnits));
            page.SetText("afterBalance", balance >= tier.RequiredBalanceUnits ? WithdrawalPresentation.Units(tier.Currency, balance - tier.RequiredBalanceUnits) : "Insufficient balance");
            page.SetText("hint", "Your test balance will be reserved.");
            page.SetText("details", "SIMULATED APPLICATION\n" + (channel == null ? "" : WithdrawalPresentation.ChannelLabel(channel.Label))
                + "\nReserve " + WithdrawalPresentation.Units(tier.Currency, tier.RequiredBalanceUnits)
                + "\nView all conditions before confirming.\nNo account information or real payment.");
            page.SetButton(WithdrawalAction.Confirm, channel != null && CanApply(tier));
        }

        private void RenderRules()
        {
            var request = FindRequest(rulesRequest);
            var tier = request != null ? request.Terms : Tier;
            if (tier == null) return;
            string[] rows = WithdrawalPresentation.RuleRows(tier, Service.Rules);
            page.SetText("amount", Money(tier.DisplayCents));
            var legacy = new StringBuilder(Money(tier.DisplayCents) + " - ALL CONDITIONS\n\n");
            for (int i = 0; i < 8; i++) {
                bool visible = i < rows.Length;
                page.SetVisible("rule" + i, visible);
                page.SetText("rules" + i, visible ? rows[i] : "");
                if (visible) legacy.Append(rows[i]).Append('\n');
            }
            string[] summaryRows = WithdrawalPresentation.RuleSummaryRows(tier);
            for (int i = 0; i < summaryRows.Length; i++) {
                page.SetText("ruleSummary" + i, summaryRows[i]);
                page.SetVisible("ruleSummaryRow" + i, summaryRows[i].Length > 0);
            }
            string footnote = WithdrawalPresentation.RuleFootnote(tier, Service.Rules);
            page.SetText("ruleFootnote", footnote);
            legacy.Append('\n').Append(footnote);
            page.SetText("rules", legacy.ToString());
        }

        private void RenderRewards()
        {
            long cash, diamonds;
            WithdrawalPresentation.RewardTotals(Service.State, out cash, out diamonds);
            page.SetText("rewardCash", Money(cash));
            page.SetText("rewardDiamonds", "+" + WithdrawalPresentation.Number(diamonds) + " diamonds");
            page.SetText("afterCash", Money(checked(Service.State.CashCents + cash)));
            page.SetText("afterDiamonds", WithdrawalPresentation.Number(checked(Service.State.DiamondUnits + diamonds)));
            page.SetText("rewardDetail", cash > 0 || diamonds > 0 ? "Fruit matches + level clear" : "Match three fruits or clear a level to earn test rewards.");
            page.SetButton(WithdrawalAction.Claim, Service.State.PendingRewards.Count > 0);
        }

        private void RenderRecords()
        {
            int count = Service.State.Requests.Count;
            page.SetText("empty", count == 0 ? "No applications yet. Choose a target." : "Your latest simulated applications");
            for (int i = 0; i < 8; i++) {
                page.SetVisible("record" + i, i < count);
                page.SetVisible("recordComplete" + i, false);
                page.SetVisible("recordOther" + i, false);
                if (i >= count) continue;
                var r = Service.State.Requests[count - 1 - i];
                bool completed = r.Status == WithdrawalRequestStatus.SimulatedCompleted;
                page.SetVisible("recordComplete" + i, completed);
                page.SetVisible("recordOther" + i, !completed);
                page.SetText("recordAmount" + i, Money(r.Terms.DisplayCents));
                page.SetText("recordStatus" + i, WithdrawalPresentation.RecordStatus(r.Status));
                page.SetText("recordTime" + i, WithdrawalPresentation.RecordDate(r.CreatedUtcSeconds));
            }
        }

        private void RenderRequest()
        {
            var r = Request;
            if (r == null) { Show(WithdrawalScreen.Records); return; }
            // Retain the ID before Tick transitions this request out of ActiveRequest.
            selectedRequest = r.RequestId;
            var expected = ScreenFor(r.Status);
            if (screen != expected) { Show(expected); return; }
            page.SetText("amount", Money(r.Terms.DisplayCents));
            page.SetText("status", Status(r.Status));
            page.SetText("channel", WithdrawalPresentation.ChannelLabel(r.ChannelLabel));
            page.SetText("requestId", "ID: " + r.RequestId);
            page.SetButton(WithdrawalAction.Cancel, r.IsActive);
            page.SetButton(WithdrawalAction.CompleteTest, r.Status == WithdrawalRequestStatus.ReviewPending && Service.GetRemainingReviewSeconds() == 0);
            page.SetButton(WithdrawalAction.RejectTest, r.Status == WithdrawalRequestStatus.ReviewPending);
            if (r.Status == WithdrawalRequestStatus.ConditionsPending) RenderConditions(r);
            else if (r.Status == WithdrawalRequestStatus.ReviewPending) {
                long seconds = Service.GetRemainingReviewSeconds();
                page.SetText("countdown", WithdrawalPresentation.Countdown(seconds));
                page.SetText("reviewHint", "All conditions completed.\nReview completes automatically.");
                page.SetText("conditions", "Application saved locally.\nNo account information is needed.\n\nSimulated review: " + WithdrawalPresentation.Countdown(seconds));
                page.SetText("step", "SIMULATED REVIEW");
                page.SetProgress(WithdrawalPresentation.Ratio(r.Terms.ReviewSeconds - seconds, r.Terms.ReviewSeconds));
            }
            else {
                bool complete = r.Status == WithdrawalRequestStatus.SimulatedCompleted;
                string title = complete ? "Simulation completed" : "Test request closed";
                string detail = complete ? "No real money was sent" : "Reserved units returned to wallet";
                page.SetText("resultTitle", title);
                page.SetText("resultDetail", detail);
                page.SetText("afterBalance", WithdrawalPresentation.Units(r.Terms.Currency, WithdrawalPresentation.Balance(Service.State, r.Terms.Currency)));
                page.SetText("conditions", detail);
                page.SetText("step", title);
            }
        }

        private void RenderConditions(WithdrawalRequest request)
        {
            var conditions = Service.GetConditions(request.TierId);
            RenderGroupedConditions(request, conditions);
            var legacy = new StringBuilder();
            int visible = 0, done = 0;
            for (int i = 0; i < conditions.Length && visible < 7; i++) {
                var c = conditions[i];
                if (c.Target == 0) continue;
                string text = WithdrawalPresentation.ConditionText(c, request.Terms.Currency);
                page.SetVisible("condition" + visible, true);
                page.SetVisible("done" + visible, c.Complete);
                page.SetText("cond" + visible, text);
                page.SetProgress("condition" + visible, c.Complete ? 1f : WithdrawalPresentation.Ratio(c.Current, c.Target));
                legacy.Append(c.Complete ? "[OK] " : "[ ] ").Append(text).Append('\n');
                visible++;
                if (c.Complete) done++;
            }
            for (int i = visible; i < 7; i++) {
                page.SetVisible("condition" + i, false);
                page.SetVisible("done" + i, false);
                page.SetText("cond" + i, "");
                page.SetProgress("condition" + i, 0);
            }
            page.SetText("conditions", legacy.ToString());
            page.SetText("step", done + " / " + visible + " CONDITIONS COMPLETE");
            page.SetText("summary", "Earlier recorded progress counts. Keep playing to finish the remaining goals.");
            page.SetProgress(visible == 0 ? 1f : (float)done / visible);
        }

        private void RenderGroupedConditions(WithdrawalRequest request, WithdrawalConditionProgress[] conditions)
        {
            string levels = WithdrawalPresentation.Milestones(request.Terms);
            page.SetText("milestones", "Level milestones " + levels);
            page.SetVisible("levelsRow", levels.Length > 0);
            page.SetText("reserved", "Reserved: " + WithdrawalPresentation.Units(request.Terms.Currency, request.Terms.RequiredBalanceUnits));
            for (int i = 0; i < 3; i++) {
                page.SetVisible("levelDone" + i, false);
                page.SetVisible("levelPending" + i, false);
            }
            page.SetVisible("dailyRow", false);
            page.SetVisible("daysRow", false);
            page.SetVisible("rankRow", false);
            page.SetVisible("rankDone", false);
            page.SetText("daily", "");
            page.SetText("days", "");
            page.SetText("rank", "");
            page.SetText("dailyPercent", "");
            page.SetText("daysPercent", "");
            page.SetProgress("daily", 0);
            page.SetProgress("days", 0);
            for (int i = 0; i < conditions.Length; i++) {
                var condition = conditions[i];
                if (condition.Target == 0) continue;
                switch (condition.Kind) {
                    case WithdrawalConditionKind.PassLevel:
                    case WithdrawalConditionKind.PassHideLevel:
                    case WithdrawalConditionKind.PassStage2Level:
                        int levelIndex = (int)condition.Kind - 1;
                        page.SetVisible("levelDone" + levelIndex, condition.Complete);
                        page.SetVisible("levelPending" + levelIndex, !condition.Complete);
                        break;
                    case WithdrawalConditionKind.LoginPassLevel:
                        RenderGroupedProgress("daily", condition);
                        break;
                    case WithdrawalConditionKind.LoginDay:
                        RenderGroupedProgress("days", condition);
                        break;
                    case WithdrawalConditionKind.UserLevel:
                        page.SetVisible("rankRow", true);
                        page.SetText("rank", WithdrawalPresentation.GroupedProgress(condition));
                        page.SetVisible("rankDone", condition.Complete);
                        break;
                }
            }
        }

        private void RenderGroupedProgress(string key, WithdrawalConditionProgress condition)
        {
            page.SetVisible(key + "Row", true);
            page.SetText(key, WithdrawalPresentation.GroupedProgress(condition));
            page.SetText(key + "Percent", WithdrawalPresentation.Percentage(condition.Current, condition.Target));
            page.SetProgress(key, condition.Complete ? 1f : WithdrawalPresentation.Ratio(condition.Current, condition.Target));
        }

        private void Handle(WithdrawalAction action, int index)
        {
            if (handlingAction) return;
            handlingAction = true;
            try { HandleAction(action, index); }
            finally {
                handlingAction = false;
                if (opened) Refresh();
            }
        }

        private void HandleAction(WithdrawalAction action, int index)
        {
            notice = "";
            if (action == WithdrawalAction.Close) { MgrUI.Instance.Close(Path, false); return; }
            if (action == WithdrawalAction.Back) {
                if (screen == WithdrawalScreen.Rules) {
                    selectedRequest = rulesRequest;
                    Show(rulesReturnScreen);
                } else if (screen == WithdrawalScreen.Confirm) Show(WithdrawalScreen.Channel);
                else OpenWallet();
                return;
            }
            if (!Service.IsAvailable || Service.State == null) return;
            switch (action) {
                case WithdrawalAction.Cash:
                    currency = WithdrawalCurrency.Cash; OpenWallet(); return;
                case WithdrawalAction.Diamonds:
                    currency = WithdrawalCurrency.Diamonds; OpenWallet(); return;
                case WithdrawalAction.SelectTier:
                    if (screen == WithdrawalScreen.Channel) {
                        if (index >= 0 && index < 2 && index < Service.Rules.Channels.Length) selectedChannel = index;
                        return;
                    }
                    if (screen != WithdrawalScreen.Cash && screen != WithdrawalScreen.Diamonds) return;
                    int j = 0;
                    for (int i = 0; i < Service.Rules.Tiers.Length; i++) if (Service.Rules.Tiers[i].Currency == currency) {
                        if (j++ == index) { selectedTier = Service.Rules.Tiers[i].Id; selectedRequest = null; break; }
                    }
                    return;
                case WithdrawalAction.Apply:
                    if (Service.State.ActiveRequest != null) { SelectRequest(Service.State.ActiveRequest); Show(ScreenFor(Request.Status)); return; }
                    if (!CanApply(Tier)) { notice = "Choose an available target and claim enough test rewards."; return; }
                    selectedRequest = null;
                    selectedChannel = 0;
                    Show(WithdrawalScreen.Channel); return;
                case WithdrawalAction.Confirm:
                    if (screen == WithdrawalScreen.Channel) { if (CanApply(Tier) && Channel != null) Show(WithdrawalScreen.Confirm); return; }
                    if (screen != WithdrawalScreen.Confirm || Tier == null || Channel == null) return;
                    if (Service.Apply(selectedTier, Channel.Id)) {
                        var created = Service.State.Requests[Service.State.Requests.Count - 1];
                        SelectRequest(created); Show(ScreenFor(created.Status)); return;
                    }
                    notice = Service.LastError; return;
                case WithdrawalAction.Rules:
                    if (screen == WithdrawalScreen.Rules) return;
                    rulesReturnScreen = screen;
                    rulesRequest = IsRequestScreen(screen) ? Request?.RequestId : null;
                    Show(WithdrawalScreen.Rules); return;
                case WithdrawalAction.Rewards: selectedRequest = null; Show(WithdrawalScreen.Rewards); return;
                case WithdrawalAction.Records: selectedRequest = null; Show(WithdrawalScreen.Records); return;
                case WithdrawalAction.Progress:
                    var active = Service.State.ActiveRequest;
                    if (active == null) Show(WithdrawalScreen.Records);
                    else { SelectRequest(active); Show(ScreenFor(active.Status)); }
                    return;
                case WithdrawalAction.SelectRecord:
                    int recordIndex = Service.State.Requests.Count - 1 - index;
                    if (index >= 0 && index < 8 && recordIndex >= 0 && recordIndex < Service.State.Requests.Count) {
                        SelectRequest(Service.State.Requests[recordIndex]); Show(ScreenFor(Request.Status));
                    }
                    return;
                case WithdrawalAction.Claim:
                    if (screen != WithdrawalScreen.Rewards) return;
                    bool claimed = Service.ClaimAllRewards();
                    notice = claimed ? "Test rewards added to your wallet." : Service.IsAvailable ? "No new rewards to claim." : Service.LastError;
                    OpenWallet(); return;
                case WithdrawalAction.Cancel:
                    var cancel = Request;
                    if (cancel != null && cancel.IsActive) {
                        SelectRequest(cancel);
                        if (!Service.CancelRequest(cancel.RequestId)) notice = Service.LastError;
                    }
                    return;
                case WithdrawalAction.CompleteTest:
                case WithdrawalAction.RejectTest:
                    var resolve = Request;
                    if (resolve != null && resolve.IsActive) {
                        SelectRequest(resolve);
                        if (!Service.ResolveSimulation(resolve.RequestId, action == WithdrawalAction.CompleteTest)) notice = Service.LastError;
                    }
                    return;
            }
        }

        private void OpenWallet()
        {
            selectedRequest = null;
            rulesRequest = null;
            EnsureTier();
            Show(WalletScreen);
        }
        private void SelectRequest(WithdrawalRequest request)
        {
            selectedRequest = request.RequestId;
            // Wallet selection and currency survive viewing another application's history.
        }
        private static bool IsRequestScreen(WithdrawalScreen value)
        {
            return value == WithdrawalScreen.Conditions || value == WithdrawalScreen.Review || value == WithdrawalScreen.Complete || value == WithdrawalScreen.Rejected;
        }
        private static WithdrawalScreen ScreenFor(WithdrawalRequestStatus status)
        {
            if (status == WithdrawalRequestStatus.ConditionsPending) return WithdrawalScreen.Conditions;
            if (status == WithdrawalRequestStatus.ReviewPending) return WithdrawalScreen.Review;
            return status == WithdrawalRequestStatus.SimulatedCompleted ? WithdrawalScreen.Complete : WithdrawalScreen.Rejected;
        }
        public static string Money(long cents) { return WithdrawalPresentation.Money(cents); }
        public static string Status(WithdrawalRequestStatus status)
        {
            switch (status) {
                case WithdrawalRequestStatus.ConditionsPending: return "Conditions in progress";
                case WithdrawalRequestStatus.ReviewPending: return "Simulated review";
                case WithdrawalRequestStatus.SimulatedCompleted: return "SIMULATED COMPLETE - NO PAYMENT";
                case WithdrawalRequestStatus.Rejected: return "Test rejected - units returned";
                default: return "Cancelled - units returned";
            }
        }
    }
}
