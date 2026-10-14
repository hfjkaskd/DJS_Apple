using System;
using System.Collections.Generic;
using FruitWithdrawal;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

internal static class WithdrawalPresentationTests
{
    private static int passed;
    private static WithdrawalPageView page;
    private static WithdrawalScreen shown;
    private static int created;
    private static WithdrawalHost host;
    private static WithdrawalHud hud;

    private static void Main()
    {
        Run("Canvas reference framing preserves full-screen center and caches lookup", CanvasFrame);
        Run("Canvas artwork uses minimal safe-area shift and extreme-height fit", CanvasSafeArea);
        Run("Grouped progress preserves actual rank and condition requirements", GroupedConditions);
        Run("Short rule rows and wallet summaries retain configured requirements", ShortRules);
        Run("Sliced progress keeps rounded caps and hides empty fill", SlicedProgress);
        Run("Published rules compact and preserve request terms", Rules);
        Run("Countdown and large-balance percentage boundaries", Formatting);
        Run("Reward preview matches actual batch claim", Rewards);
        Run("Closing rewards preserves pending items and source wallet", RewardsReopen);
        Run("Wallet-channel-confirm and source-aware Rules return", Navigation);
        Run("Synchronous Apply Changed renders one final page", ApplyRefresh);
        Run("Zero-target conditions are omitted and named progress follows rows", Conditions);
        Run("Historical request terms do not replace wallet selection", History);
        Run("Review completion while reading Rules returns to terminal result", ReviewReturn);
        Run("Cancel Changed retains result and shows refunded balance", Cancel);
        Run("HUD cash, diamond and rewards Buttons open their actual destinations", HudNavigation);
        Run("HUD updates both currencies once when claiming a reward batch", HudClaim);
        Run("HUD applies and refunds only the selected currency", HudReserveAndRefund);
        Run("HUD repeated Bind and Unbind preserve one callback and current balances", HudLifecycle);
        Run("HUD unavailable state clears values and disables all three Buttons", HudUnavailable);
        Console.WriteLine(passed+" presentation checks passed.");
    }
    private static void Run(string name,Action test) { test(); passed++; Console.WriteLine("PASS "+name); }
    private static void Eq<T>(T expected,T actual) { if(!EqualityComparer<T>.Default.Equals(expected,actual)) throw new Exception("Expected "+expected+"; got "+actual); }
    private static void True(bool value) { if(!value)throw new Exception("Assertion failed"); }
    private static WithdrawalService Setup(WithdrawalState state=null)
    {
        if(hud!=null) { hud.Unbind(); hud=null; }
        if(host!=null)host.TestClose();
        var service=new WithdrawalService(WithdrawalRules.CreateDefault(),state??new WithdrawalState { CashCents=100000,DiamondUnits=100000 });
        host=new WithdrawalHost { PageRoot=new RectTransform(),Screens=new WithdrawalScreenDefinition[11] };
        Resources.Pages.Clear(); created=0;
        for(int i=0;i<11;i++) {
            var screen=(WithdrawalScreen)i;
            string path="test-page-"+i;
            host.Screens[i]=new WithdrawalScreenDefinition { Screen=screen,ResourcePath=path };
            Resources.Pages.Add(path,new GameObject { Factory=()=>MakePage(screen) });
        }
        MgrUI.Instance.OnClose=()=>host.TestClose();
        WithdrawalHost.Open(WithdrawalCurrency.Cash);
        host.TestOpen();
        return service;
    }
    private static GameObject MakePage(WithdrawalScreen screen)
    {
        page=new WithdrawalPageView(); shown=screen; created++;
        var texts=new List<WithdrawalTextBinding>();
        var names=new List<string> { "test","notice","cash","diamonds","frozen","balance","amount","target","ratio","apply","rewards","summary","channel","reserve","afterBalance","hint","details","ruleFootnote","rules","rewardCash","rewardDiamonds","afterCash","afterDiamonds","rewardDetail","empty","status","requestId","conditions","step","countdown","reviewHint","resultTitle","resultDetail","milestones","reserved","daily","days","rank","dailyPercent","daysPercent" };
        for(int i=0;i<8;i++) foreach(string prefix in new[]{"tier","tierCost","channel","rules","cond","recordAmount","recordStatus","recordTime","ruleSummary"})names.Add(prefix+i);
        foreach(string name in names)texts.Add(new WithdrawalTextBinding { Key=name,Text=new TMP_Text() });
        page.Texts=texts.ToArray();
        var objects=new List<WithdrawalObjectBinding>();
        for(int i=0;i<8;i++)foreach(string prefix in new[]{"tier","selected","selectedAmount","channelSelected","rule","condition","done","record","recordComplete","recordOther","levelDone","levelPending","ruleSummaryRow"})objects.Add(new WithdrawalObjectBinding { Key=prefix+i,Object=new GameObject() });
        foreach(string name in new[]{"levelsRow","dailyRow","daysRow","rankRow","rankDone"})objects.Add(new WithdrawalObjectBinding { Key=name,Object=new GameObject() });
        page.Objects=objects.ToArray();
        var buttons=new List<WithdrawalButtonBinding>();
        for(int a=0;a<=15;a++)for(int i=0;i<8;i++)buttons.Add(new WithdrawalButtonBinding { Action=(WithdrawalAction)a,Index=i,Button=new Button() });
        page.Buttons=buttons.ToArray();
        page.ProgressFill=new Image();page.Progress=new WithdrawalProgressBinding[9];
        for(int i=0;i<7;i++)page.Progress[i]=new WithdrawalProgressBinding { Key="condition"+i,Fill=new Image() };
        page.Progress[7]=new WithdrawalProgressBinding { Key="daily",Fill=new Image() };
        page.Progress[8]=new WithdrawalProgressBinding { Key="days",Fill=new Image() };
        page.gameObject.Component=page;
        return page.gameObject;
    }
    private static void Click(WithdrawalAction action,int index=0)
    {
        foreach(var b in page.Buttons)if(b.Action==action&&b.Index==index){True(b.Button.interactable);b.Button.Press();return;}
        throw new Exception("Missing action");
    }
    private static string Text(string key) { foreach(var b in page.Texts)if(b.Key==key)return b.Text.text;throw new Exception(key); }
    private static bool Visible(string key) { foreach(var b in page.Objects)if(b.Key==key)return b.Object.activeSelf;throw new Exception(key); }
    private static void ApplySelected()
    {
        Click(WithdrawalAction.Apply);Eq(WithdrawalScreen.Channel,shown);
        Click(WithdrawalAction.Confirm);Eq(WithdrawalScreen.Confirm,shown);
        Click(WithdrawalAction.Confirm);
    }
    private static void Near(float expected,float actual) { if(Math.Abs(expected-actual)>0.002f)throw new Exception("Expected approx "+expected+"; got "+actual); }
    private static WithdrawalPageView Frame(float canvasHeight,float safeHeight,float rootY,Rect bounds)
    {
        var canvas=new Canvas();canvas.rootCanvas=canvas;
        var canvasRect=(RectTransform)canvas.transform;canvasRect.rect=new Rect(-540,-canvasHeight/2,1080,canvasHeight);
        var root=new RectTransform { parent=canvasRect,rect=new Rect(-540,-safeHeight/2,1080,safeHeight),localPosition=new Vector3(0,rootY,0) };
        var content=new RectTransform { parent=root };
        var view=new WithdrawalPageView { transform=root,Content=content,ParentCanvas=canvas,UseCanvasFrame=true,DesignSize=new Vector2(1080,2251.8f),ArtworkBounds=bounds };
        view.RefreshLayout();return view;
    }
    private static void CanvasFrame()
    {
        var view=Frame(2400,2305,47.5f,new Rect(-500,-800,1000,1600));
        Near(1,view.Content.localScale.x);Near(-47.5f,view.Content.localPosition.y);Near(0,view.Content.position.y);
        Eq(1,view.ParentLookupCount);view.RefreshLayout();Eq(1,view.ParentLookupCount);
        view.UseCanvasFrame=false;view.DesignSize=new Vector2(1080,2600);view.DesignCenter=new Vector2(10,20);view.RefreshLayout();
        Near(2305f/2600,view.Content.localScale.x);Near(-20*2305f/2600,view.Content.anchoredPosition.y);
    }
    private static void CanvasSafeArea()
    {
        var bounds=new Rect(-500,-1000,1000,2200);
        var view=Frame(2400,2200,50,bounds);Near(1,view.Content.localScale.x);Near(-100,view.Content.localPosition.y);
        Near(-1100,view.Content.localPosition.y+bounds.yMin*view.Content.localScale.y);
        Near(1100,view.Content.localPosition.y+bounds.yMax*view.Content.localScale.y);
        view=Frame(1920,1800,0,bounds);Near(1800f/2200,view.Content.localScale.x);
        Near(-900,view.Content.localPosition.y+bounds.yMin*view.Content.localScale.y);Near(900,view.Content.localPosition.y+bounds.yMax*view.Content.localScale.y);
        view=Frame(3000,2900,20,new Rect(-500,-800,1000,1600));Near(1,view.Content.localScale.x);Near(-20,view.Content.localPosition.y);
    }
    private static void GroupedConditions()
    {
        var service=Setup(new WithdrawalState { CashCents=100000,TotalWins=48,HighestClearedLevel=51,TodayWins=8,QualifiedCheckInDays=4,CurrentUtcDay=1,LastQualifiedUtcDay=0 });
        Click(WithdrawalAction.SelectTier,2);ApplySelected();
        Eq("Level milestones 2 / 22 / 51",Text("milestones"));Eq("Today: 8 / 10 wins",Text("daily"));Eq("Qualified days: 4 / 5",Text("days"));Eq("Reward rank: 49 / 31",Text("rank"));
        Eq("80%",Text("dailyPercent"));Eq("80%",Text("daysPercent"));Near(0.8f,page.Progress[7].Fill.fillAmount);Near(0.8f,page.Progress[8].Fill.fillAmount);
        True(Visible("levelsRow"));True(Visible("dailyRow"));True(Visible("daysRow"));True(Visible("rankRow"));True(Visible("rankDone"));
        for(int i=0;i<3;i++){True(Visible("levelDone"+i));True(!Visible("levelPending"+i));}
        Eq("Reserved: $500.00",Text("reserved"));
        service=Setup();ApplySelected();True(Visible("levelsRow"));True(Visible("levelPending0"));True(!Visible("levelPending1"));True(!Visible("dailyRow"));True(!Visible("daysRow"));True(!Visible("rankRow"));
    }
    private static void ShortRules()
    {
        Setup();Click(WithdrawalAction.SelectTier,2);Eq("Review all conditions before applying",Text("summary"));Eq("In application: $0.00",Text("frozen"));
        Click(WithdrawalAction.Rules);Eq("Balance: $500.00",Text("ruleSummary0"));Eq("Level milestones 2 / 22 / 51",Text("ruleSummary1"));
        Eq("Daily wins: 10",Text("ruleSummary2"));Eq("Qualified days: 5",Text("ruleSummary3"));Eq("Reward rank: 31",Text("ruleSummary4"));Eq("Simulated review: 60 seconds",Text("ruleSummary5"));
        Eq(3,Text("ruleFootnote").Split('\n').Length);True(Text("ruleFootnote").Contains("10 wins"));
        Click(WithdrawalAction.Back);Click(WithdrawalAction.Diamonds);Eq("5 qualified days - Rank 31 - Daily 10 wins",Text("summary"));
        Click(WithdrawalAction.SelectTier,3);Eq("15 qualified days - Rank 34 - Daily 10 wins",Text("summary"));
        Click(WithdrawalAction.Cash);Click(WithdrawalAction.SelectTier,0);Click(WithdrawalAction.Rules);True(!Visible("ruleSummaryRow2"));True(!Visible("ruleSummaryRow3"));True(!Visible("ruleSummaryRow4"));
        Eq("Oct 09",WithdrawalPresentation.RecordDate(new DateTimeOffset(2026,10,9,12,0,0,TimeSpan.Zero).ToUnixTimeSeconds()));Eq("Test complete - No payment",WithdrawalPresentation.RecordStatus(WithdrawalRequestStatus.SimulatedCompleted));
    }
    private static void SlicedProgress()
    {
        var view=new WithdrawalPageView();var image=new Image { type=Image.Type.Sliced };
        view.ProgressFill=image;view.Progress=new[]{new WithdrawalProgressBinding { Key="condition0",Fill=image }};
        view.SetProgress(0.35f);Eq(0.35f,image.rectTransform.anchorMax.x);Eq(1f,image.rectTransform.anchorMax.y);True(image.enabled);
        Eq(0f,image.rectTransform.offsetMin.x);Eq(0f,image.rectTransform.offsetMax.x);
        view.SetProgress("condition0",0);Eq(0f,image.rectTransform.anchorMax.x);True(!image.enabled);
        view.SetProgress("condition0",2);Eq(1f,image.rectTransform.anchorMax.x);True(image.enabled);
        image.type=Image.Type.Filled;view.SetProgress("condition0",0.75f);Eq(0.75f,image.fillAmount);
    }
    private static void Rules()
    {
        var rules=WithdrawalRules.CreateDefault();var tier=rules.Tiers[2];var snapshot=tier.Copy();
        var rows=WithdrawalPresentation.RuleRows(snapshot,rules);Eq(8,rows.Length);True(rows[0].Contains("2 / 22 / 51"));
        tier.RequiredPassLevels=99;Eq(rows[0],WithdrawalPresentation.RuleRows(snapshot,rules)[0]);
        Eq(4,WithdrawalPresentation.RuleRows(rules.Tiers[0],rules).Length);
        Eq("Test wallet",WithdrawalPresentation.ChannelLabel(rules.Channels[0].Label));
    }
    private static void Formatting()
    {
        Eq("00:42",WithdrawalPresentation.Countdown(42));Eq("00:00",WithdrawalPresentation.Countdown(-1));
        Eq("01:00:00",WithdrawalPresentation.Countdown(3600));Eq("1d 01:00:00",WithdrawalPresentation.Countdown(90000));
        Eq("100%",WithdrawalPresentation.Percentage(long.MaxValue,long.MaxValue));Eq("50%",WithdrawalPresentation.Percentage(long.MaxValue/2,long.MaxValue));
        Eq(1f,WithdrawalPresentation.Ratio(50,0));Eq("$0.40",WithdrawalPresentation.Money(40));
        Eq("$500",WithdrawalPresentation.TierAmount(50000));Eq("$1,000",WithdrawalPresentation.TierAmount(100000));Eq("$0.40",WithdrawalPresentation.TierAmount(40));Eq("$0.60",WithdrawalPresentation.TierAmount(60));
    }
    private static void Rewards()
    {
        var state=new WithdrawalState { CashCents=200,DiamondUnits=100 };
        state.PendingRewards.Add(new WithdrawalReward { RewardId="one",RunId="run",CashCents=10,DiamondUnits=3 });
        state.PendingRewards.Add(new WithdrawalReward { RewardId="two",RunId="run",CashCents=100,DiamondUnits=7 });
        var service=Setup(state);Eq("Rewards (2)",Text("rewards"));Click(WithdrawalAction.Rewards);
        Eq("$3.10",Text("afterCash"));Eq("110",Text("afterDiamonds"));Eq("$1.10",Text("rewardCash"));
        int before=created;Click(WithdrawalAction.Claim);
        Eq(before+1,created);Eq(310L,service.State.CashCents);Eq(110L,service.State.DiamondUnits);Eq(0,service.State.PendingRewards.Count);
    }
    private static void RewardsReopen()
    {
        var state=new WithdrawalState { CashCents=200,DiamondUnits=100 };
        state.PendingRewards.Add(new WithdrawalReward { RewardId="saved",RunId="run",CashCents=10,DiamondUnits=3 });
        var service=Setup(state);Click(WithdrawalAction.Diamonds);Eq("50,000",Text("tierCost0"));Eq("KEEP HARVESTING",Text("apply"));Click(WithdrawalAction.Rewards);
        Click(WithdrawalAction.Close);Eq(1,service.State.PendingRewards.Count);
        WithdrawalHost.Open(WithdrawalCurrency.Diamonds,true);host.TestOpen();Eq(WithdrawalScreen.Rewards,shown);
        Eq("$2.10",Text("afterCash"));Click(WithdrawalAction.Claim);Eq(WithdrawalScreen.Diamonds,shown);
        Eq(103L,service.State.DiamondUnits);Eq(0,service.State.PendingRewards.Count);
    }
    private static void Navigation()
    {
        Setup();Eq("TEST MODE • NO CASH VALUE",Text("test"));Eq("$0.40",Text("tier0"));Eq("$500",Text("tier2"));True(Visible("selectedAmount0"));True(!Visible("selectedAmount2"));True(!Visible("selectedAmount3"));Click(WithdrawalAction.SelectTier,2);True(!Visible("selectedAmount0"));True(Visible("selectedAmount2"));Eq("CHOOSE CHANNEL",Text("apply"));Click(WithdrawalAction.Rules);Eq(WithdrawalScreen.Rules,shown);
        True(Text("rules0").Contains("2 / 22 / 51"));Click(WithdrawalAction.Back);Eq(WithdrawalScreen.Cash,shown);Eq("$500.00",Text("amount"));
        Click(WithdrawalAction.Apply);Eq("$500.00",Text("amount"));Click(WithdrawalAction.SelectTier,1);True(Visible("channelSelected1"));True(!Visible("channelSelected0"));
        Click(WithdrawalAction.Confirm);Eq("Test bank",Text("channel"));Eq("$500.00",Text("reserve"));Eq("$500.00",Text("afterBalance"));Eq("Your test balance will be reserved.",Text("hint"));
        Click(WithdrawalAction.Rules);Click(WithdrawalAction.Back);Eq(WithdrawalScreen.Confirm,shown);Eq("Test bank",Text("channel"));
        Click(WithdrawalAction.Back);Eq(WithdrawalScreen.Channel,shown);True(Visible("channelSelected1"));
    }
    private static void ApplyRefresh()
    {
        var service=Setup();Click(WithdrawalAction.SelectTier,2);Click(WithdrawalAction.Apply);Click(WithdrawalAction.Confirm);
        int before=created;Click(WithdrawalAction.Confirm);Eq(before+1,created);Eq(WithdrawalScreen.Conditions,shown);
        Eq(50000L,service.State.FrozenCashCents);Eq(50000L,service.State.CashCents);True(Text("cond3").StartsWith("Reserved:"));
        True(Visible("done3"));Eq(1f,page.Progress[3].Fill.fillAmount);
    }
    private static void Conditions()
    {
        var service=Setup();ApplySelected();Eq(WithdrawalScreen.Conditions,shown);
        True(Visible("condition0"));True(Visible("condition1"));True(!Visible("condition2"));
        Eq("Clear fruit level: 0 / 1",Text("cond0"));Eq("Reserved: $0.40 / $0.40",Text("cond1"));
        Eq("1 / 2 CONDITIONS COMPLETE",Text("step"));Eq(0.5f,page.ProgressFill.fillAmount);Eq(0f,page.Progress[2].Fill.fillAmount);
    }
    private static void History()
    {
        var service=Setup(new WithdrawalState { CashCents=100000,DiamondUnits=100000,HighestClearedLevel=1 });
        True(service.Apply("cash-040","test-wallet"));service.Utc+=60;True(service.Tick());True(service.Apply("diamond-500","test-bank"));
        Click(WithdrawalAction.SelectTier,2);Click(WithdrawalAction.Records);
        True(Visible("recordOther0"));True(!Visible("recordComplete0"));True(Visible("recordComplete1"));True(!Visible("recordOther1"));True(!Visible("recordComplete2"));True(!Visible("recordOther2"));
        Click(WithdrawalAction.SelectRecord,1);
        Eq(WithdrawalScreen.Complete,shown);Eq("$0.40",Text("amount"));Eq("$999.60",Text("afterBalance"));
        Click(WithdrawalAction.Rules);True(Text("rules0").Contains("milestones: 1"));True(!Visible("rule4"));
        Click(WithdrawalAction.Back);Eq(WithdrawalScreen.Complete,shown);Click(WithdrawalAction.Back);Eq(WithdrawalScreen.Cash,shown);Eq("$500.00",Text("amount"));
        Click(WithdrawalAction.Apply);Eq(WithdrawalScreen.Conditions,shown);Eq("$500.00",Text("amount"));True(Text("cond0").Contains("diamonds"));
        Click(WithdrawalAction.Back);Eq(WithdrawalScreen.Cash,shown);True(Visible("selected2"));
    }
    private static void ReviewReturn()
    {
        var service=Setup(new WithdrawalState { CashCents=100000,HighestClearedLevel=1 });ApplySelected();Eq(WithdrawalScreen.Review,shown);Eq("01:00",Text("countdown"));Eq("All conditions completed.\nReview completes automatically.",Text("reviewHint"));
        Click(WithdrawalAction.Rules);service.Utc+=60;True(service.Tick());Eq(WithdrawalScreen.Rules,shown);
        Click(WithdrawalAction.Back);Eq(WithdrawalScreen.Complete,shown);Eq("Simulation completed",Text("resultTitle"));Eq("No real money was sent",Text("resultDetail"));
    }
    private static void Cancel()
    {
        var service=Setup();ApplySelected();int before=created;Click(WithdrawalAction.Cancel);
        Eq(before+1,created);Eq(WithdrawalScreen.Rejected,shown);Eq("Test request closed",Text("resultTitle"));Eq("$1,000.00",Text("afterBalance"));Eq(0L,service.State.FrozenCashCents);
    }
    private static WithdrawalHud MakeHud()
    {
        hud=new WithdrawalHud {
            CashButton=new Button(),DiamondButton=new Button(),RewardsButton=new Button(),
            Cash=new TMP_Text(),Diamonds=new TMP_Text(),Rewards=new TMP_Text()
        };
        hud.Bind();return hud;
    }
    private static void OpenFromHud(Button button,WithdrawalScreen expected)
    {
        host.TestClose();
        int before=MgrUI.Instance.OpenCount;
        button.Press();
        Eq(before+1,MgrUI.Instance.OpenCount);Eq(WithdrawalHost.Path,MgrUI.Instance.LastOpenedPath);
        host.TestOpen();Eq(expected,shown);
    }
    private static WithdrawalState WithHudRewards(long cash=100000,long diamonds=100000)
    {
        var state=new WithdrawalState { CashCents=cash,DiamondUnits=diamonds };
        state.PendingRewards.Add(new WithdrawalReward { RewardId="hud-one",RunId="hud-run",CashCents=10,DiamondUnits=3 });
        state.PendingRewards.Add(new WithdrawalReward { RewardId="hud-two",RunId="hud-run",CashCents=100,DiamondUnits=7 });
        return state;
    }
    private static void HudNavigation()
    {
        Setup(WithHudRewards());MakeHud();
        OpenFromHud(hud.CashButton,WithdrawalScreen.Cash);Eq("$0.40",Text("tier0"));
        OpenFromHud(hud.DiamondButton,WithdrawalScreen.Diamonds);Eq("50,000",Text("tierCost0"));
        OpenFromHud(hud.RewardsButton,WithdrawalScreen.Rewards);Eq("$1.10",Text("rewardCash"));
    }
    private static void HudClaim()
    {
        var service=Setup(WithHudRewards(200,100));MakeHud();
        Eq("$2.00",hud.Cash.text);Eq("100",hud.Diamonds.text);Eq("Claim (2)",hud.Rewards.text);
        int cashWrites=hud.Cash.WriteCount,diamondWrites=hud.Diamonds.WriteCount;
        True(service.ClaimAllRewards());
        Eq("$3.10",hud.Cash.text);Eq("110",hud.Diamonds.text);Eq("Rewards",hud.Rewards.text);
        Eq(cashWrites+1,hud.Cash.WriteCount);Eq(diamondWrites+1,hud.Diamonds.WriteCount);
        True(!service.ClaimAllRewards());Eq(cashWrites+1,hud.Cash.WriteCount);
    }
    private static void HudReserveAndRefund()
    {
        var service=Setup();MakeHud();
        True(service.Apply("cash-main","test-wallet"));
        Eq(50000L,service.State.FrozenCashCents);Eq("$500.00",hud.Cash.text);Eq("100,000",hud.Diamonds.text);
        True(service.CancelRequest(service.State.ActiveRequest.RequestId));
        Eq(0L,service.State.FrozenCashCents);Eq("$1,000.00",hud.Cash.text);Eq("100,000",hud.Diamonds.text);
        True(service.Apply("diamond-500","test-bank"));
        Eq(50000L,service.State.FrozenDiamondUnits);Eq("$1,000.00",hud.Cash.text);Eq("50,000",hud.Diamonds.text);
        True(service.CancelRequest(service.State.ActiveRequest.RequestId));
        Eq(0L,service.State.FrozenDiamondUnits);Eq("$1,000.00",hud.Cash.text);Eq("100,000",hud.Diamonds.text);
    }
    private static void HudLifecycle()
    {
        var service=Setup(WithHudRewards());MakeHud();hud.Bind();hud.Bind();
        OpenFromHud(hud.CashButton,WithdrawalScreen.Cash);
        OpenFromHud(hud.DiamondButton,WithdrawalScreen.Diamonds);
        OpenFromHud(hud.RewardsButton,WithdrawalScreen.Rewards);
        int cashWrites=hud.Cash.WriteCount,diamondWrites=hud.Diamonds.WriteCount;
        True(service.ClaimAllRewards());Eq(cashWrites+1,hud.Cash.WriteCount);Eq(diamondWrites+1,hud.Diamonds.WriteCount);
        hud.Unbind();hud.Unbind();
        cashWrites=hud.Cash.WriteCount;diamondWrites=hud.Diamonds.WriteCount;
        int opens=MgrUI.Instance.OpenCount;
        hud.CashButton.Press();hud.DiamondButton.Press();hud.RewardsButton.Press();Eq(opens,MgrUI.Instance.OpenCount);
        True(service.Apply("cash-main","test-wallet"));
        Eq(cashWrites,hud.Cash.WriteCount);Eq(diamondWrites,hud.Diamonds.WriteCount);Eq("$1,001.10",hud.Cash.text);
        hud.Bind();Eq("$501.10",hud.Cash.text);Eq("100,010",hud.Diamonds.text);
        cashWrites=hud.Cash.WriteCount;
        True(service.CancelRequest(service.State.ActiveRequest.RequestId));Eq("$1,001.10",hud.Cash.text);Eq(cashWrites+1,hud.Cash.WriteCount);
        OpenFromHud(hud.DiamondButton,WithdrawalScreen.Diamonds);
    }
    private static void HudUnavailable()
    {
        var service=Setup(WithHudRewards());MakeHud();service.SetAvailable(false);
        Eq("--",hud.Cash.text);Eq("--",hud.Diamonds.text);Eq("Unavailable",hud.Rewards.text);
        True(!hud.CashButton.interactable);True(!hud.DiamondButton.interactable);True(!hud.RewardsButton.interactable);
        int opens=MgrUI.Instance.OpenCount;
        hud.CashButton.Press();hud.DiamondButton.Press();hud.RewardsButton.Press();Eq(opens,MgrUI.Instance.OpenCount);
        service.SetAvailable(true);
        Eq("$1,000.00",hud.Cash.text);Eq("100,000",hud.Diamonds.text);Eq("Claim (2)",hud.Rewards.text);
        True(hud.CashButton.interactable);True(hud.DiamondButton.interactable);True(hud.RewardsButton.interactable);
        OpenFromHud(hud.CashButton,WithdrawalScreen.Cash);
    }
}
