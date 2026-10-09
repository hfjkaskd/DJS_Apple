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

    private static void Main()
    {
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
        Console.WriteLine(passed+" presentation checks passed.");
    }
    private static void Run(string name,Action test) { test(); passed++; Console.WriteLine("PASS "+name); }
    private static void Eq<T>(T expected,T actual) { if(!EqualityComparer<T>.Default.Equals(expected,actual)) throw new Exception("Expected "+expected+"; got "+actual); }
    private static void True(bool value) { if(!value)throw new Exception("Assertion failed"); }
    private static WithdrawalService Setup(WithdrawalState state=null)
    {
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
        var names=new List<string> { "test","notice","cash","diamonds","frozen","balance","amount","target","ratio","apply","rewards","summary","channel","reserve","afterBalance","hint","details","ruleFootnote","rules","rewardCash","rewardDiamonds","afterCash","afterDiamonds","rewardDetail","empty","status","requestId","conditions","step","countdown","reviewHint","resultTitle","resultDetail" };
        for(int i=0;i<8;i++) foreach(string prefix in new[]{"tier","tierCost","channel","rules","cond","recordAmount","recordStatus","recordTime"})names.Add(prefix+i);
        foreach(string name in names)texts.Add(new WithdrawalTextBinding { Key=name,Text=new TMP_Text() });
        page.Texts=texts.ToArray();
        var objects=new List<WithdrawalObjectBinding>();
        for(int i=0;i<8;i++)foreach(string prefix in new[]{"tier","selected","channelSelected","rule","condition","done","record"})objects.Add(new WithdrawalObjectBinding { Key=prefix+i,Object=new GameObject() });
        page.Objects=objects.ToArray();
        var buttons=new List<WithdrawalButtonBinding>();
        for(int a=0;a<=15;a++)for(int i=0;i<8;i++)buttons.Add(new WithdrawalButtonBinding { Action=(WithdrawalAction)a,Index=i,Button=new Button() });
        page.Buttons=buttons.ToArray();
        page.ProgressFill=new Image();page.Progress=new WithdrawalProgressBinding[7];
        for(int i=0;i<7;i++)page.Progress[i]=new WithdrawalProgressBinding { Key="condition"+i,Fill=new Image() };
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
    }
    private static void Rewards()
    {
        var state=new WithdrawalState { CashCents=200,DiamondUnits=100 };
        state.PendingRewards.Add(new WithdrawalReward { RewardId="one",RunId="run",CashCents=10,DiamondUnits=3 });
        state.PendingRewards.Add(new WithdrawalReward { RewardId="two",RunId="run",CashCents=100,DiamondUnits=7 });
        var service=Setup(state);Eq("Rewards (2)",Text("rewards"));Click(WithdrawalAction.Rewards);
        Eq("$3.10",Text("afterCash"));Eq("110",Text("afterDiamonds"));Eq("+$1.10",Text("rewardCash"));
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
        Setup();Click(WithdrawalAction.SelectTier,2);Eq("CHOOSE CHANNEL",Text("apply"));Click(WithdrawalAction.Rules);Eq(WithdrawalScreen.Rules,shown);
        True(Text("rules0").Contains("2 / 22 / 51"));Click(WithdrawalAction.Back);Eq(WithdrawalScreen.Cash,shown);Eq("$500.00",Text("amount"));
        Click(WithdrawalAction.Apply);Eq("$500.00",Text("amount"));Click(WithdrawalAction.SelectTier,1);True(Visible("channelSelected1"));True(!Visible("channelSelected0"));
        Click(WithdrawalAction.Confirm);Eq("Test bank",Text("channel"));Eq("$500.00",Text("reserve"));Eq("$500.00",Text("afterBalance"));
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
        Click(WithdrawalAction.SelectTier,2);Click(WithdrawalAction.Records);Click(WithdrawalAction.SelectRecord,1);
        Eq(WithdrawalScreen.Complete,shown);Eq("$0.40",Text("amount"));Eq("$999.60",Text("afterBalance"));
        Click(WithdrawalAction.Rules);True(Text("rules0").Contains("milestones: 1"));True(!Visible("rule4"));
        Click(WithdrawalAction.Back);Eq(WithdrawalScreen.Complete,shown);Click(WithdrawalAction.Back);Eq(WithdrawalScreen.Cash,shown);Eq("$500.00",Text("amount"));
        Click(WithdrawalAction.Apply);Eq(WithdrawalScreen.Conditions,shown);Eq("$500.00",Text("amount"));True(Text("cond0").Contains("diamonds"));
        Click(WithdrawalAction.Back);Eq(WithdrawalScreen.Cash,shown);True(Visible("selected2"));
    }
    private static void ReviewReturn()
    {
        var service=Setup(new WithdrawalState { CashCents=100000,HighestClearedLevel=1 });ApplySelected();Eq(WithdrawalScreen.Review,shown);Eq("01:00",Text("countdown"));
        Click(WithdrawalAction.Rules);service.Utc+=60;True(service.Tick());Eq(WithdrawalScreen.Rules,shown);
        Click(WithdrawalAction.Back);Eq(WithdrawalScreen.Complete,shown);Eq("TEST COMPLETE",Text("resultTitle"));True(Text("resultDetail").Contains("No money was sent"));
    }
    private static void Cancel()
    {
        var service=Setup();ApplySelected();int before=created;Click(WithdrawalAction.Cancel);
        Eq(before+1,created);Eq(WithdrawalScreen.Rejected,shown);Eq("TEST CANCELLED",Text("resultTitle"));Eq("$1,000.00",Text("afterBalance"));Eq(0L,service.State.FrozenCashCents);
    }
}
