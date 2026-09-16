using System;
using System.Collections;
using System.Globalization;
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
        private WithdrawalScreen screen;
        private WithdrawalPageView page;
        private WithdrawalCurrency currency;
        private string selectedTier;
        private string selectedRequest;
        private int selectedChannel;
        private string notice = "";
        private Coroutine ticker;
        private bool changingPage;
        private WithdrawalService Service => WithdrawalService.Instance;
        public override PAIEAGDLCBJ Layer => PAIEAGDLCBJ.Top;

        public static void Open(WithdrawalCurrency kind, bool rewards = false)
        {
            requestedScreen = rewards ? WithdrawalScreen.Rewards : kind == WithdrawalCurrency.Cash ? WithdrawalScreen.Cash : WithdrawalScreen.Diamonds;
            if (MgrUI.Instance != null && !MCCIJBJGMCK.IsLock()) MgrUI.Instance.Open(Path);
        }

        protected override void Init() { }

        protected override void BeforeOpen()
        {
            notice = "";
            selectedRequest = null;
            currency = requestedScreen == WithdrawalScreen.Diamonds ? WithdrawalCurrency.Diamonds : WithdrawalCurrency.Cash;
            EnsureTier();
            Service.Changed += Refresh;
            Show(requestedScreen);
        }

        protected override void AfterOpen() { ticker = StartCoroutine(TickWhileOpen()); }

        protected override void BeforeClose()
        {
            Service.Changed -= Refresh;
            if (ticker != null) StopCoroutine(ticker);
            ticker = null;
        }

        private IEnumerator TickWhileOpen()
        {
            var wait = new WaitForSecondsRealtime(1);
            while (true) { yield return wait; Service.Tick(); Refresh(); }
        }

        private void Show(WithdrawalScreen next)
        {
            if (changingPage) return;
            changingPage = true;
            if (page != null) Destroy(page.gameObject);
            page = null;
            screen = next;
            for (int i = 0; i < Screens.Length; i++)
            {
                if (Screens[i].Screen != next) continue;
                GameObject prefab = Resources.Load<GameObject>(Screens[i].ResourcePath);
                if (prefab != null)
                {
                    GameObject instance = Instantiate(prefab, PageRoot, false);
                    page = instance.GetComponent<WithdrawalPageView>();
                    if (page != null) page.Bind(Handle);
                }
                break;
            }
            changingPage = false;
            if (page == null) { Debug.LogError("[Withdrawal] Missing configured page."); return; }
            Refresh();
        }

        private WithdrawalTierRule Tier
        {
            get
            {
                var rules = Service.Rules;
                if (rules == null || rules.Tiers == null) return null;
                for (int i = 0; i < rules.Tiers.Length; i++) if (rules.Tiers[i].Id == selectedTier) return rules.Tiers[i];
                return null;
            }
        }

        private void EnsureTier()
        {
            if (Tier != null && Tier.Currency == currency) return;
            if (Service.Rules == null) return;
            for (int i = 0; i < Service.Rules.Tiers.Length; i++)
                if (Service.Rules.Tiers[i].Currency == currency) { selectedTier = Service.Rules.Tiers[i].Id; return; }
        }

        private WithdrawalRequest Request
        {
            get
            {
                if (Service.State == null) return null;
                if (!string.IsNullOrEmpty(selectedRequest))
                    for (int i = 0; i < Service.State.Requests.Count; i++)
                        if (Service.State.Requests[i].RequestId == selectedRequest) return Service.State.Requests[i];
                return Service.State.ActiveRequest;
            }
        }

        private void Refresh()
        {
            if (page == null || changingPage) return;
            page.SetText("test", "TEST MODE - NO REAL PAYOUT");
            if (!Service.IsAvailable || Service.State == null)
            {
                page.SetText("notice", "Simulation unavailable. " + Service.LastError);
                page.SetButton(WithdrawalAction.Apply, false);
                page.SetButton(WithdrawalAction.Confirm, false);
                page.SetButton(WithdrawalAction.Claim, false);
                return;
            }
            page.SetText("notice", notice);
            page.SetText("cash", Money(Service.State.CashCents));
            page.SetText("diamonds", Service.State.DiamondUnits.ToString("N0", CultureInfo.InvariantCulture));
            page.SetText("frozen", "In application: " + Money(Service.State.FrozenCashCents));
            if (screen == WithdrawalScreen.Cash || screen == WithdrawalScreen.Diamonds) RenderWallet();
            else if (screen == WithdrawalScreen.Channel)
            {
                page.SetText("amount", "Apply for " + Money(Tier.DisplayCents));
                for (int i = 0; i < Service.Rules.Channels.Length && i < 2; i++)
                    page.SetText("channel" + i, (selectedChannel == i ? "[Selected] " : "") + Service.Rules.Channels[i].Label);
            }
            else if (screen == WithdrawalScreen.Confirm)
            {
                page.SetText("amount", Money(Tier.DisplayCents));
                page.SetText("details", "SIMULATED APPLICATION\n\n" + Service.Rules.Channels[selectedChannel].Label + "\n\n" + Money(Tier.DisplayCents) + "\n\nNo payment account is collected.\nThe full conditions were shown before applying.");
            }
            else if (screen == WithdrawalScreen.Rules) RenderRules();
            else if (screen == WithdrawalScreen.Records) RenderRecords();
            else if (screen == WithdrawalScreen.Rewards) RenderRewards();
            else RenderRequest();
        }

        private void RenderWallet()
        {
            EnsureTier();
            var tier = Tier;
            int index = 0;
            for (int i = 0; i < Service.Rules.Tiers.Length; i++)
            {
                var item = Service.Rules.Tiers[i];
                if (item.Currency != currency) continue;
                page.SetText("tier" + index, Money(item.DisplayCents));
                page.SetVisible("selected" + index, item.Id == selectedTier);
                index++;
            }
            for (int i = 0; i < 4; i++) page.SetVisible("tier" + i, i < index);
            long balance = currency == WithdrawalCurrency.Cash ? Service.State.CashCents : Service.State.DiamondUnits;
            long remaining = Math.Max(0, tier.RequiredBalanceUnits - balance);
            page.SetText("amount", Money(tier.DisplayCents));
            page.SetText("balance", currency == WithdrawalCurrency.Cash ? Money(balance) : balance.ToString("N0", CultureInfo.InvariantCulture));
            page.SetText("target", remaining == 0 ? "Balance target reached" : currency == WithdrawalCurrency.Cash ? "Earn " + Money(remaining) + " more" : "Earn " + remaining.ToString("N0", CultureInfo.InvariantCulture) + " more diamonds");
            page.SetText("ratio", Math.Min(100, balance * 100 / tier.RequiredBalanceUnits) + "%");
            page.SetProgress((float)((double)balance / tier.RequiredBalanceUnits));
            page.SetText("apply", Service.State.ActiveRequest != null ? "View application" : "Apply " + Money(tier.DisplayCents));
            bool completed = false;
            for (int i = 0; i < Service.State.Requests.Count; i++)
                if (Service.State.Requests[i].TierId == tier.Id && Service.State.Requests[i].Status == WithdrawalRequestStatus.SimulatedCompleted) completed = true;
            if (completed && Service.State.ActiveRequest == null) page.SetText("apply", "Test already completed");
            page.SetButton(WithdrawalAction.Apply, Service.State.ActiveRequest != null || (!completed && remaining == 0));
            page.SetText("rewards", Service.State.PendingRewards.Count > 0 ? "Claim rewards (" + Service.State.PendingRewards.Count + ")" : "Reward center");
            string balanceHint = remaining == 0 ? "Balance target reached." : currency == WithdrawalCurrency.Cash
                ? "Earn " + Money(remaining) + " more to apply."
                : "Earn " + remaining.ToString("N0", CultureInfo.InvariantCulture) + " more diamonds.";
            page.SetText("summary", currency == WithdrawalCurrency.Cash ? balanceHint + "\nRead all conditions in Rules." : "Read all conditions in Rules before applying.");
        }

        private void RenderRules()
        {
            var tier = !string.IsNullOrEmpty(selectedRequest) && Request != null ? Request.Terms : Tier;
            var b = new StringBuilder();
            b.Append(Money(tier.DisplayCents)).Append(" - ALL CONDITIONS\n\n");
            b.Append("Balance required: ").Append(tier.Currency == WithdrawalCurrency.Cash ? Money(tier.RequiredBalanceUnits) : tier.RequiredBalanceUnits.ToString("N0", CultureInfo.InvariantCulture) + " diamonds").Append("\n");
            if (tier.RequiredPassLevels > 0) b.Append("Clear ").Append(tier.RequiredPassLevels).Append(" fruit levels.\n");
            if (tier.RequiredHiddenPassLevels > 0) b.Append("Clear fruit level ").Append(tier.RequiredHiddenPassLevels).Append(".\n");
            if (tier.RequiredStage2PassLevels > 0) b.Append("Clear fruit level ").Append(tier.RequiredStage2PassLevels).Append(".\n");
            if (tier.RequiredDailyWins > 0) b.Append("Clear ").Append(tier.RequiredDailyWins).Append(" levels in one UTC day.\n");
            if (tier.RequiredCheckInDays > 0) b.Append("Qualify on ").Append(tier.RequiredCheckInDays).Append(" separate UTC days (each needs ").Append(Service.Rules.DailyWinsForCheckIn).Append(" wins).\n");
            if (tier.RequiredUserLevel > 0) b.Append("Reach reward rank ").Append(tier.RequiredUserLevel).Append(".\n");
            b.Append("\nSimulated review: ").Append(tier.ReviewSeconds).Append(" seconds after conditions complete.\n\n");
            b.Append("One active application. Its amount is reserved. Cancelling or rejecting returns it.\n\nAll rewards and results are simulated, with no cash value.\n\nProgress is saved on this device. Review all conditions before applying.");
            page.SetText("rules", b.ToString());
        }

        private void RenderRewards()
        {
            long cash = 0, diamonds = 0;
            for (int i = 0; i < Service.State.PendingRewards.Count; i++) { cash += Service.State.PendingRewards[i].CashCents; diamonds += Service.State.PendingRewards[i].DiamondUnits; }
            page.SetText("rewardCash", "+" + Money(cash));
            page.SetText("rewardDiamonds", "+" + diamonds.ToString("N0", CultureInfo.InvariantCulture) + " diamonds");
            page.SetText("rewardDetail", cash > 0 || diamonds > 0 ? "Earned by your fruit matches and cleared levels." : "Match three fruits or clear a level to earn test rewards.");
            page.SetButton(WithdrawalAction.Claim, Service.State.PendingRewards.Count > 0);
        }

        private void RenderRecords()
        {
            int count = Service.State.Requests.Count;
            page.SetText("empty", count == 0 ? "No applications yet.\nChoose a target in your wallet." : "Your latest simulated applications");
            for (int i = 0; i < 8; i++)
            {
                page.SetVisible("record" + i, i < count);
                if (i >= count) continue;
                var r = Service.State.Requests[count - 1 - i];
                page.SetText("recordAmount" + i, Money(r.Terms.DisplayCents));
                page.SetText("recordStatus" + i, r.Status == WithdrawalRequestStatus.SimulatedCompleted ? "Test complete\nNo payment" : Status(r.Status));
                page.SetText("recordTime" + i, DateTimeOffset.FromUnixTimeSeconds(r.CreatedUtcSeconds).UtcDateTime.ToString("yyyy-MM-dd HH:mm 'UTC'", CultureInfo.InvariantCulture));
            }
        }

        private void RenderRequest()
        {
            var r = Request;
            if (r == null) { Show(WithdrawalScreen.Records); return; }
            var expected = ScreenFor(r.Status);
            if (screen != expected) { Show(expected); return; }
            page.SetText("amount", Money(r.Terms.DisplayCents));
            page.SetText("status", Status(r.Status));
            page.SetText("channel", r.ChannelLabel);
            page.SetText("requestId", "ID: " + r.RequestId.Substring(0, Math.Min(12, r.RequestId.Length)));
            page.SetButton(WithdrawalAction.Cancel, r.IsActive);
            page.SetButton(WithdrawalAction.CompleteTest, r.Status == WithdrawalRequestStatus.ReviewPending && Service.GetRemainingReviewSeconds() == 0);
            page.SetButton(WithdrawalAction.RejectTest, r.Status == WithdrawalRequestStatus.ReviewPending);
            if (r.Status == WithdrawalRequestStatus.ConditionsPending)
            {
                var conditions = Service.GetConditions(r.TierId);
                var b = new StringBuilder();
                int done = 0;
                for (int i = 0; i < conditions.Length; i++)
                {
                    var c = conditions[i];
                    b.Append(c.Complete ? "[OK] " : "[ ] ").Append(c.Label).Append("  ").Append(c.Current).Append(" / ").Append(c.Target).Append('\n');
                    if (c.Complete) done++;
                }
                page.SetText("conditions", b.ToString());
                page.SetText("step", "Complete the disclosed conditions");
                page.SetProgress(conditions.Length == 0 ? 1 : (float)done / conditions.Length);
            }
            else if (r.Status == WithdrawalRequestStatus.ReviewPending)
            {
                long seconds = Service.GetRemainingReviewSeconds();
                page.SetText("conditions", "Application saved locally.\nNo account information is needed.\n\n" + (seconds > 0 ? "Simulated review: " + seconds + "s remaining" : "Simulated review is ready.\nComplete the test to see its final record."));
                page.SetText("step", "SIMULATED REVIEW");
            }
            else
            {
                page.SetText("conditions", r.Status == WithdrawalRequestStatus.SimulatedCompleted ? "Simulation completed.\n\nNo money was sent.\nThis result is saved in Records." : "This test application was closed.\nThe reserved reward units were returned.\nYou can start another test.");
                page.SetText("step", Status(r.Status));
            }
        }

        private void Handle(WithdrawalAction action, int index)
        {
            notice = "";
            if (action == WithdrawalAction.Close) { MgrUI.Instance.Close(Path, false); return; }
            if (!Service.IsAvailable || Service.State == null) { Refresh(); return; }
            switch (action)
            {
                case WithdrawalAction.Cash: selectedRequest = null; currency = WithdrawalCurrency.Cash; EnsureTier(); Show(WithdrawalScreen.Cash); return;
                case WithdrawalAction.Diamonds: selectedRequest = null; currency = WithdrawalCurrency.Diamonds; EnsureTier(); Show(WithdrawalScreen.Diamonds); return;
                case WithdrawalAction.SelectTier:
                    if (screen == WithdrawalScreen.Channel) { selectedChannel = index; break; }
                    int j = 0;
                    for (int i = 0; i < Service.Rules.Tiers.Length; i++) if (Service.Rules.Tiers[i].Currency == currency) { if (j++ == index) { selectedTier = Service.Rules.Tiers[i].Id; break; } }
                    break;
                case WithdrawalAction.Apply:
                    if (Service.State.ActiveRequest != null) { SelectRequest(Service.State.ActiveRequest); Show(ScreenFor(Service.State.ActiveRequest.Status)); return; }
                    selectedChannel = 0; Show(WithdrawalScreen.Channel); return;
                case WithdrawalAction.Confirm:
                    if (screen == WithdrawalScreen.Channel) { Show(WithdrawalScreen.Confirm); return; }
                    if (Service.Apply(selectedTier, Service.Rules.Channels[selectedChannel].Id))
                    {
                        var created = Service.State.Requests[Service.State.Requests.Count - 1];
                        SelectRequest(created); Show(ScreenFor(created.Status)); return;
                    }
                    notice = Service.LastError; break;
                case WithdrawalAction.Rules: Show(WithdrawalScreen.Rules); return;
                case WithdrawalAction.Rewards: Show(WithdrawalScreen.Rewards); return;
                case WithdrawalAction.Records: Show(WithdrawalScreen.Records); return;
                case WithdrawalAction.Progress:
                    if (Request != null) SelectRequest(Request);
                    Show(Request == null ? WithdrawalScreen.Records : ScreenFor(Request.Status)); return;
                case WithdrawalAction.SelectRecord:
                    int recordIndex = Service.State.Requests.Count - 1 - index;
                    if (recordIndex >= 0) { SelectRequest(Service.State.Requests[recordIndex]); Show(ScreenFor(Request.Status)); }
                    return;
                case WithdrawalAction.Claim:
                    bool claimed = Service.ClaimAllRewards();
                    notice = claimed ? "Test rewards added to your wallet." : Service.IsAvailable ? "No new rewards to claim." : Service.LastError;
                    Show(currency == WithdrawalCurrency.Cash ? WithdrawalScreen.Cash : WithdrawalScreen.Diamonds); return;
                case WithdrawalAction.Cancel:
                    if (Request != null) { selectedRequest = Request.RequestId; Service.CancelRequest(Request.RequestId); }
                    break;
                case WithdrawalAction.CompleteTest:
                case WithdrawalAction.RejectTest:
                    if (Request != null) { selectedRequest = Request.RequestId; Service.ResolveSimulation(Request.RequestId, action == WithdrawalAction.CompleteTest); }
                    break;
                case WithdrawalAction.Back: selectedRequest = null; Show(currency == WithdrawalCurrency.Cash ? WithdrawalScreen.Cash : WithdrawalScreen.Diamonds); return;
            }
            Refresh();
        }

        private void SelectRequest(WithdrawalRequest request)
        {
            selectedRequest = request.RequestId;
            selectedTier = request.TierId;
            currency = request.Terms.Currency;
        }

        private static WithdrawalScreen ScreenFor(WithdrawalRequestStatus status)
        {
            if (status == WithdrawalRequestStatus.ConditionsPending) return WithdrawalScreen.Conditions;
            if (status == WithdrawalRequestStatus.ReviewPending) return WithdrawalScreen.Review;
            return status == WithdrawalRequestStatus.SimulatedCompleted ? WithdrawalScreen.Complete : WithdrawalScreen.Rejected;
        }
        public static string Money(long cents) { return "$" + (cents / 100m).ToString("N2", CultureInfo.InvariantCulture); }
        public static string Status(WithdrawalRequestStatus status)
        {
            switch (status)
            {
                case WithdrawalRequestStatus.ConditionsPending: return "Conditions in progress";
                case WithdrawalRequestStatus.ReviewPending: return "Simulated review";
                case WithdrawalRequestStatus.SimulatedCompleted: return "SIMULATED COMPLETE - NO PAYMENT";
                case WithdrawalRequestStatus.Rejected: return "Test rejected - units returned";
                default: return "Cancelled - units returned";
            }
        }
    }
}
