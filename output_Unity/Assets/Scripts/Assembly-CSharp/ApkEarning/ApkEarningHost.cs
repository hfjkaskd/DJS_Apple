using System;
using System.Collections;
using System.Globalization;
using TMPro;
using UnityEngine;

namespace FruitApkEarning
{
    /// <summary>Local APK flow; the original saved prefabs own all artwork and layout.</summary>
    public sealed class ApkEarningHost : BaseUI
    {
        public const string Path = "withdrawal/ApkEarningHost";
        public RectTransform PageRoot;
        public TMP_Text GlobalTestModeCaption;
        public float ReceiptSeconds = 2, MessageSeconds = 3, BonusesSeconds = 5.5f, LoadingSeconds = 1.5f;
        private static bool requestedDiamonds, requestedRewards, requestedAccountLevel;
        private static Action requestedCompletion;
        private Action completion;
        private ApkEarningPageView page;
        private string panel;
        private bool diamonds, busy, opened;
        private int tier, earlyOrdinal, channel;
        private ApkReward reward;
        private string accountName="Test user", accountEmail="test@example.com", accountNumber="0000000000", accountArea="+1", channelInfo="";
        private Coroutine sequence, noticeSequence;
        private ApkEarningPageView noticeView;
        private Sprite selectedDiamondOption, normalDiamondOption;
        private ApkEarningConfig config;
        private bool useOtherAccount;
        private long simulatedCashCents;
        private ApkEarningService Service => ApkEarningService.Instance;
        public override PAIEAGDLCBJ Layer => PAIEAGDLCBJ.Top;
        public static bool IsOpen => MgrUI.Instance != null && MgrUI.Instance.IsOpenning(Path);
        public static string Money(long cents) => "$"+(cents/100m).ToString("N2",CultureInfo.InvariantCulture);
        private static string SourceMoney(long cents) => "$"+(cents/100m).ToString(cents%100==0?"N0":"N2",CultureInfo.InvariantCulture);
        private static string DiamondMoney(long hundredths) => SourceMoney(ApkEarningPresentation.DiamondCashCents(hundredths));
        public static string DiamondText(long value) => (value/100m).ToString("0.##",CultureInfo.InvariantCulture);

        public static void Open(bool diamonds=false,bool rewards=false) { Open(diamonds,rewards,null); }
        public static void Open(bool diamonds,bool rewards,Action finished)
        { OpenPage(diamonds,rewards,finished,false); }
        public static void OpenAccountLevel() { OpenPage(false,false,null,true); }
        private static void OpenPage(bool diamonds,bool rewards,Action finished,bool accountLevel)
        {
            var service=ApkEarningService.Instance;
            if(IsOpen||MgrUI.Instance==null||MCCIJBJGMCK.IsLock()||(!service.Available&&string.IsNullOrEmpty(service.Error)))return;
            requestedDiamonds=diamonds;requestedRewards=rewards;requestedCompletion=finished;requestedAccountLevel=accountLevel;
            MgrUI.Instance.Open(Path);
        }
        protected override void Init()
        {
            config=Resources.Load<ApkEarningConfig>("ApkEarningConfig");
            if(config==null)throw new InvalidOperationException("ApkEarningConfig is missing.");
        }
        protected override void BeforeOpen()
        {
            diamonds=requestedDiamonds; completion=requestedCompletion;requestedCompletion=null;
            earlyOrdinal=0;tier=0;channel=0;reward=null;opened=true;
            Service.Tick();Service.Changed+=OnChanged;
        }
        protected override void AfterOpen()
        {
            // BaseUI activates cached pages after BeforeOpen. Start sequences only
            // once this host is active, including resumed activation and guides.
            if(!opened||!IsOpening)return;
            if(!Service.Available){ShowRecovery(ShowInitialPage);return;}
            ShowInitialPage();
        }
        private void ShowInitialPage()
        {
            if(requestedAccountLevel){requestedAccountLevel=false;ShowAccountLevel();}
            else if(!Service.State.WelcomeShown&&Service.State.CurrentLevel==1)ShowWelcome();
            else if(requestedRewards&&Service.State.PendingRewards.Count>0)ShowReward();
            else if(Service.State.ActivationPending)StartActivation();
            else if(!Service.State.DiamondGuideShown&&Service.State.CurrentLevel>=Service.Rules.DiamondFirstLevel)StartDiamondGuide();
            else ShowWallet();
        }
        protected override void BeforeClose()
        {
            opened=false;Service.Changed-=OnChanged;
            if(sequence!=null)StopCoroutine(sequence);sequence=null;
            ClearNotice();
        }
        protected override void AfterClose(){var callback=completion;completion=null;callback?.Invoke();}
        private void OnChanged(){if(opened&&!busy)Refresh();}
        private void Close()
        {
            if(earlyOrdinal>0&&!ArchiveEarly(false,Close))return;
            if(Service.State.ActivationPending&&panel!="LocalEarningSequence"){StartActivation();return;}
            MgrUI.Instance.Close(Path,false);
        }
        private void StopSequence()
        {
            if(sequence!=null)StopCoroutine(sequence);
            sequence=null;
            if(page!=null)page.CancelPresentation();
        }
        private bool ArchiveEarly(bool submitted,Action recovered)
        {
            int ordinal=earlyOrdinal;busy=true;
            bool ok;
            try {ok=submitted?Service.SubmitEarlyRecord(ordinal):Service.ArchiveEarlyReward(ordinal);}
            finally{busy=false;}
            if(!ok){ShowRecovery(()=>{if(ArchiveEarly(submitted,recovered))recovered?.Invoke();});return false;}
            earlyOrdinal=0;return true;
        }
        private ApkEarningPageView Show(string name)
        {
            ClearNotice();
            if(page!=null){page.gameObject.SetActive(false);Destroy(page.gameObject);}
            panel=name;
            var prefab=Resources.Load<GameObject>("ApkEarningReference/Panels/"+name);
            if(prefab==null)throw new InvalidOperationException("Missing earning prefab "+name);
            page=Instantiate(prefab,PageRoot,false).GetComponent<ApkEarningPageView>();
            if(page==null)throw new InvalidOperationException("Unbound earning prefab "+name);
            page.RefreshLayout();
            page.Bind("Close",Close);page.Bind("CloseBtn",Close);page.Bind("close",Close);page.Bind("closeButton",Close);
            page.SetText("TestMode","TEST MODE · NO CASH PAYMENT");
            RefreshTestDisclosure();
            return page;
        }
        private void RefreshTestDisclosure()
        {
            var local=page.GetText("TestMode");
            var message=page.GetText("MessageDisclosure");
            bool visible=(local!=null&&local.isActiveAndEnabled)||(message!=null&&message.isActiveAndEnabled);
            if(GlobalTestModeCaption!=null)GlobalTestModeCaption.gameObject.SetActive(!visible);
        }
        private void ShowWallet()
        {
            StopSequence();
            if(!diamonds)
            {
                busy=true;bool saved;
                try{saved=Service.RefreshSimulatedStats(config.SimulatedAttemptsMinimum,config.SimulatedAttemptsMaximumExclusive);}
                finally{busy=false;}
                if(!saved){ShowRecovery(ShowWallet);return;}
                simulatedCashCents=UnityEngine.Random.Range(config.SimulatedCashMinimumCents,config.SimulatedCashMaximumCents);
            }
            // The record is persisted before its presentation. Resume it after a restart too.
            if(!diamonds&&earlyOrdinal==0)earlyOrdinal=FindUnarchivedOrdinal();
            Show(diamonds?"TXCoinPanel":"TXPanel");
            page.Bind("TXBtn",BeginWithdrawal);
            if(diamonds){
                if(!page.TintDiamondOptions){
                    selectedDiamondOption=Resources.Load<Sprite>("ApkEarningReference/Artwork/DiamondOptionSelected");
                    normalDiamondOption=Resources.Load<Sprite>("ApkEarningReference/Artwork/DiamondOptionNormal");
                }
                for(int i=0;i<4;i++){int selected=i;page.Bind("options["+i+"]",()=>{tier=selected;Refresh();});}
            }
            else {page.Bind("HistoryButton",ShowHistory);page.SetVisible("Level1",false);page.SetVisible("Level2",false);}
            Refresh();
        }
        private void ShowWelcome()
        {
            Show("TXRecordGuidePanel");
            page.Bind("startButton",()=>{
                busy=true;bool saved;try{saved=Service.CompleteWelcome();}finally{busy=false;}
                if(!saved){ShowRecovery(ShowWelcome);return;}
                if(Service.State.PendingRewards.Count>0)ShowReward();else Close();
            });
        }
        private int FindUnarchivedOrdinal()
        {
            int ordinal=0;
            foreach(var record in Service.State.Records)
                if(!record.Archived&&(ordinal==0||record.Ordinal<ordinal))ordinal=record.Ordinal;
            return ordinal;
        }
        private ApkRecord FindEarlyRecord()
        {
            foreach(var record in Service.State.Records)if(record.Ordinal==earlyOrdinal)return record;
            return null;
        }
        private void Refresh()
        {
            if(page==null||Service.State==null)return;
            if(!Service.Available){Notice("Local save unavailable. "+Service.Error);return;}
            var state=Service.State;
            if(panel=="TXPanel")
            {
                int target=state.HighestClearedLevel<Service.Rules.FirstCashGate?Service.Rules.FirstCashGate:Service.Rules.SecondCashGate;
                int shownLevel=state.CurrentLevel;
                long current=state.HighestClearedLevel,total=target;
                string hint="Clear level "+target+"\nto review withdrawal";
                if(state.HighestClearedLevel>=Service.Rules.SecondCashGate){
                    var requirements=Service.GetRequirements(false,0);
                    foreach(var item in requirements.Items)if(!item.Complete){current=item.Current;total=item.Target;hint=item.Known?RequirementName(item.Key):"Awaiting account update";break;}
                }
                var early=FindEarlyRecord();
                if(early!=null){current=1;total=1;hint="View your cash reward";}
                page.SetText("GoldCount",Money(early!=null?early.AmountCents:state.CashCents));
                page.SetText("Title","CASH WALLET");
                page.SetText("StageLabel","Level "+(early!=null?early.Ordinal:shownLevel));
                page.SetText("TXText","Withdraw");page.SetText("Tip",hint);page.SetText("labels[0].Text","Available balance");
                page.SetText("ProgressValue",total>0?Math.Min(current,total)+" / "+total:"--");
                page.SetText("ProgressTipValue",early!=null?string.Empty:total>0?Math.Max(0,total-current).ToString(CultureInfo.InvariantCulture):"--");
                page.SetProgress("Progress",total>0?(float)current/total:0);
                int players=(int)DateTime.Now.TimeOfDay.TotalMinutes*config.SimulatedPlayersPerMinute;
                int attempts=early!=null||state.CurrentLevel<=2?1:state.SimulatedDailyAttempts;
                long sample=early!=null?early.AmountCents:state.CurrentLevel<=2?
                    (state.CurrentLevel==1?Service.Rules.EarlyFirstCents:Service.Rules.EarlySecondCents):simulatedCashCents;
                page.SetText("Stats.Time","Simulated stats");
                page.SetText("Stats.PassCount",players.ToString("N0",CultureInfo.InvariantCulture));
                page.SetText("Stats.ChallengeTimes",attempts.ToString(CultureInfo.InvariantCulture));
                page.SetText("Stats.GoldGetCount",Money(sample));
            }
            else if(panel=="TXCoinPanel")
            {
                long required=Service.Rules.DiamondTargets[tier],value=Service.Rules.DiamondPayoutCents[tier];
                string amount=SourceMoney(value),remaining=DiamondMoney(Math.Max(0,required-state.Diamonds));
                page.SetText("CoinValue",DiamondText(state.Diamonds));page.SetText("GoldValue",DiamondMoney(state.Diamonds));
                page.SetText("TipValue","100=$1");page.SetText("TXTip",amount+" Withdrawal Instructions:");
                string emphasis=ColorUtility.ToHtmlStringRGB(page.EmphasisColor);
                page.SetText("IitemTip","Balance reaches <color=#"+emphasis+">"+amount+"</color> to withdraw all\nStill <color=#"+emphasis+">"+remaining+"</color> to go");
                page.SetText("TipBarValue",DiamondMoney(Math.Min(state.Diamonds,required))+"/"+amount);
                page.SetProgress("TipBar",(float)state.Diamonds/required);
                for(int i=0;i<4;i++){
                    page.SetText("amounts["+i+"]",SourceMoney(Service.Rules.DiamondPayoutCents[i]));page.SetVisible("selectionFrames["+i+"]",i==tier);
                    var option=page.GetButton("options["+i+"]");var art=option==null?null:option.targetGraphic as UnityEngine.UI.Image;
                    if(art!=null){
                        if(page.TintDiamondOptions)art.color=i==tier?page.SelectedOptionColor:page.NormalOptionColor;
                        else {var sprite=i==tier?selectedDiamondOption:normalDiamondOption;if(sprite!=null)art.sprite=sprite;}
                    }
                }
                page.SetText("labels[0].Text","My Diamonds");
            }
            else if(panel=="LocalWithdrawalConditions")RenderConditions();
        }
        private void BeginWithdrawal()
        {
            if(!Service.Available)return;
            if(!diamonds&&earlyOrdinal==0)earlyOrdinal=FindUnarchivedOrdinal();
            if(earlyOrdinal>0){sequence=StartCoroutine(LoadAccount());return;}
            ShowConditions();
        }
        private IEnumerator LoadAccount()
        {
            Show("TXGuideLoadingPanel");page.SetText("label","Preparing withdrawal request...");
            float elapsed=0;while(elapsed<LoadingSeconds){elapsed+=Time.unscaledDeltaTime;page.SetProgress("Progress",elapsed/LoadingSeconds);yield return null;}
            sequence=null;ShowAccount();
        }
        private void ShowConditions()
        {
            StopSequence();
            Show("LocalWithdrawalConditions");page.Bind("action",ConditionsAction);page.Bind("close",ShowWallet);RenderConditions();
        }
        private void RenderConditions()
        {
            var data=Service.GetRequirements(diamonds,tier);
            page.SetText("title","Withdrawal requirements");page.SetText("subtitle",diamonds?"Diamond withdrawal":"Cash withdrawal");
            page.SetText("notice",data.Eligible?"Requirements complete. Confirm your payment details.":"Complete the requirements below to continue.");
            page.SetText("actionCaption",data.Eligible?"Enter payment details":data.FirstIncomplete=="secondLevel"?"View next stage":"Continue playing");
            for(int i=0;i<7;i++)
            {
                string prefix="rows["+i+"]";bool exists=i<data.Items.Count;page.SetVisible(prefix+".Root",exists);if(!exists)continue;
                var row=data.Items[i];page.SetText(prefix+".Name",row.Key=="firstLevel"||row.Key=="secondLevel"?"Complete level "+row.Target:RequirementName(row.Key));
                page.SetText(prefix+".Value",!row.Known?"Awaiting account update":row.Waived?"Login complete":RequirementValue(row));
                page.SetText(prefix+".Status",row.Complete?"Complete":"In progress");
            }
        }
        public static string RequirementName(string key)
        {
            switch(key){case "firstLevel":return "First level target";case "secondLevel":return "Next level target";case "balance":return "Required balance";
            case "guide":return "Complete cash guidance";case "daily":return "Levels completed today";case "login":return "Login days";case "rank":return "Account level";default:return "Requirement";}
        }
        private string RequirementValue(ApkRequirement r)
        {
            if(r.Key=="balance")return diamonds?DiamondText(r.Current)+" / "+DiamondText(r.Target):Money(r.Current)+" / "+Money(r.Target);
            if(r.Key=="firstLevel"||r.Key=="secondLevel")return r.Current+" / "+r.Target+" boards";
            return r.Current+" / "+r.Target;
        }
        private void ConditionsAction()
        {
            var data=Service.GetRequirements(diamonds,tier);
            if(data.Eligible)ShowAccount();else if(data.FirstIncomplete=="secondLevel")ShowStage();else Close();
        }
        private void ShowStage() { ShowStage(false); }
        private void ShowStage(bool resume)
        {
            StopSequence();
            Show("TXProgress0Panel");
            page.SetText("labels[0].Text","Withdrawal progress");page.SetText("labels[1].Text","Back to withdrawal");
            page.SetText("labels[2].Text","Complete the next stage requirements");
            page.SetText("labels[3].Text","First level target completed");page.SetText("labels[4].Text","Next level target pending");
            page.SetText("labels[5].Text","Review remaining requirements before applying");
            page.SetText("Tip","Complete level <color=#FF0000>"+Service.Rules.SecondCashGate+"</color> to continue the next stage");
            page.SetText("Des","Complete the next level target to continue.");page.SetText("KeFu","Application not submitted");
            for(int i=1;i<=4;i++){
                string key="path:main/Step/"+i;
                page.SetVisible(key+"/Done",i==1);page.SetVisible(key+"/Loading",i>1);
                page.SetText(key+"/Content/Time",string.Empty);
            }
            page.Bind("SureBtn",ShowConditions);page.Bind("CloseBtn",ShowConditions);
            sequence=StartCoroutine(StageSequence(resume));
        }
        private IEnumerator StageSequence(bool resume)
        {
            var view=page;
            yield return view.PlayStageSteps(resume);
            sequence=null;
            if(opened&&page==view&&!resume)ShowRequirementsReview();
        }
        private void ShowRequirementsReview()
        {
            Show("TXTooFastHintPanel");
            page.SetText("labels[0].Text","Complete the next level target to continue. Your current progress is shown below.");
            page.SetText("labels[1].Text","Requirements review");page.SetText("labels[2].Text","Continue");
            int target=Service.Rules.SecondCashGate;
            page.SetText("LevelTip","Complete level "+target);
            page.SetText("LevelPregressValue",Service.State.HighestClearedLevel+" / "+target);
            page.SetProgress("LevelPregress",target>0?(float)Service.State.HighestClearedLevel/target:0);
            page.SetText("VideoTip","Test reward videos completed");
            page.SetText("VideoPregressValue",Service.State.RewardVideos+" / "+config.ReviewVideoReferenceCount);
            page.SetProgress("VideoPregress",config.ReviewVideoReferenceCount>0?(float)Service.State.RewardVideos/config.ReviewVideoReferenceCount:0);
            page.Bind("IKnowBtn",()=>ShowStage(true));
        }
        private void ShowAccountLevel()
        {
            StopSequence();Show("LocalUserLevel");
            page.SetText("Title","Account level");
            page.SetText("levelLabel","Level "+Service.State.AccountLevel);
            page.SetText("progressLabel","Awaiting account update");
            page.SetText("ProgressCaption","Account progress");
            page.SetText("Caption","Continue");
            page.SetProgress("progressFill",0);
            page.Bind("closeButton",()=>MgrUI.Instance.Close(Path,false));
            page.Bind("continueButton",()=>MgrUI.Instance.Close(Path,false));
        }
        private void CloseAccount()
        {
            if(earlyOrdinal>0)Close();else ShowWallet();
        }
        private void ShowAccount()
        {
            StopSequence();
            Show("TXUserInfoPanel");useOtherAccount=false;
            page.Bind("CloseBtn",CloseAccount);
            SetInput("InputName",accountName);SetInput("InputEmail",accountEmail);SetInput("InputAreaNumber",accountArea);SetInput("InputNumber",accountNumber);
            page.SetText("labels[0].Text","Use test account details. No payment is submitted.");
            page.SetText("labels[1].Text","Withdrawal account");page.SetText("labels[2].Text","Confirm");
            page.Bind("GetBtn",ConfirmAccount);page.Bind("ChannelInfoBtn",ShowChannelHelp);
            for(int i=0;i<3;i++){int selected=i;page.Bind("channels["+i+"].Button",()=>{ReadAccount();channel=selected;SelectChannel();});}
            SelectChannel();
        }
        private ApkPaymentChannel SelectedChannel=>config.PaymentChannels[channel];
        private void SelectChannel()
        {
            for(int i=0;i<3;i++){
                bool available=i<config.PaymentChannels.Length;
                var button=page.GetButton("channels["+i+"].Button");if(button!=null)button.gameObject.SetActive(available);
                page.SetVisible("channels["+i+"].Checkmark",available&&i==channel);
                if(available)SetSprite("channels["+i+"].Icon",config.PaymentChannels[i].SmallIconPath);
            }
            SetInputVisible("InputEmail",SelectedChannel.UsesEmail);
            SetInputVisible("InputAreaNumber",!SelectedChannel.UsesEmail);SetInputVisible("InputNumber",!SelectedChannel.UsesEmail);
        }
        private void SetSprite(string key,string path)
        {
            if(string.IsNullOrEmpty(path))return;
            var image=page.GetImage(key);var sprite=Resources.Load<Sprite>(path);if(image!=null&&sprite!=null)image.sprite=sprite;
        }
        private void SetInputVisible(string key,bool visible)
        {var input=page.GetInput(key);if(input!=null)input.gameObject.SetActive(visible);}
        private void SetInput(string key,string value){var input=page.GetInput(key);if(input!=null)input.SetTextWithoutNotify(value);}
        private string Input(string key){var input=page.GetInput(key);return input==null?"":input.text.Trim();}
        private void ConfirmAccount()
        {
            ReadAccount();
            string error=SelectedChannel.Validate(accountName,accountEmail,accountNumber);
            if(error.Length>0){page.SetText("labels[0].Text",error);return;}
            useOtherAccount=false;
            ShowConfirmation();
        }
        private void ReadAccount()
        {accountName=Input("InputName");accountEmail=Input("InputEmail");accountNumber=Input("InputNumber");accountArea=Input("InputAreaNumber");}
        private void ShowConfirmation()
        {
            Show("TXUserInfoSurePanel");page.SetText("labels[0].Text","Confirm account");page.SetText("labels[1].Text","Confirm");page.SetText("labels[2].Text","Re-enter");
            page.Bind("CloseBtn",CloseAccount);
            page.SetText("Tip",(useOtherAccount?channelInfo:SelectedChannel.FormatAccount(accountName,accountEmail,accountArea,accountNumber))+"\n\nTest request · no payment");
            SetSprite("Image",useOtherAccount?config.OtherChannelIconPath:SelectedChannel.LargeIconPath);
            page.Bind("ReEnterBtn",()=>{if(useOtherAccount)ShowChannelHelp();else ShowAccount();});page.Bind("GetBtn",()=>{if(earlyOrdinal>0)ShowVerification();else{ShowConditions();Notice("Withdrawal service is not connected. No withdrawal has been submitted.");}});
        }
        private void ShowChannelHelp()
        {
            if(panel=="TXUserInfoPanel")ReadAccount();
            Show("TXChannelPanel");
            page.SetText("labels[0].Text","Enter test payment account information. No payment is submitted.");
            page.SetText("labels[1].Text","Withdrawal account");page.SetText("labels[2].Text","Confirm");SetInput("InputInfo",channelInfo);
            page.Bind("CloseBtn",ShowAccount);page.Bind("SureBtn",()=>{
                channelInfo=Input("InputInfo");if(channelInfo.Length==0){page.SetText("labels[0].Text","Enter test payment account information to continue.");return;}
                useOtherAccount=true;ShowConfirmation();
            });
        }
        private void ShowVerification()
        {
            var record=FindEarlyRecord();if(record==null){ShowWallet();Notice("This reward is no longer available.");return;}
            Show("LocalWithdrawalVerification");page.SetText("Tip","Reward #"+earlyOrdinal);page.SetText("GoldCount",Money(record.AmountCents));
            page.SetText("labels[0].Text","Withdrawal request");page.SetText("labels[1].Text","Confirm withdrawal");
            page.SetText("labels[2].Text","Current status");page.SetText("labels[3].Text","Reward recorded");page.SetText("labels[4].Text","Account details entered");page.SetText("labels[5].Text","Ready to submit");
            page.Bind("CloseBtn",CloseAccount);
            page.Bind("SureBtn",()=>{if(page.VerificationReady&&ArchiveEarly(true,Close))Close();});
            sequence=StartCoroutine(VerificationSequence());
        }
        private IEnumerator VerificationSequence()
        {
            yield return page.PlayVerificationSteps();
            sequence=null;
        }
        private void ShowHistory()
        {
            StopSequence();
            Show("LocalCashHistory");page.SetText("titleLabel","Withdrawal History");page.SetText("balanceLabel",SourceMoney(Service.State.CashCents));
            page.Bind("closeButton",ShowWallet);
            var parent=page.GetObject("rowParent");if(parent==null)return;
            var prefab=Resources.Load<GameObject>("ApkEarningReference/Panels/LocalCashHistoryRow");
            if(prefab==null)throw new InvalidOperationException("Missing cash history row prefab.");
            foreach(var record in Service.State.Records)
            {
                var row=Instantiate(prefab,parent.transform,false).GetComponent<ApkEarningPageView>();int ordinal=record.Ordinal;
                long timestamp=record.Pending?record.SubmittedUtc:record.CreatedUtc;
                row.SetText("titleLabel","Cash Reward #"+ordinal);row.SetText("timeLabel",timestamp>0?DateTimeOffset.FromUnixTimeSeconds(timestamp).ToLocalTime().ToString("MM/dd/yyyy HH:mm",CultureInfo.InvariantCulture):string.Empty);
                row.SetText("amountLabel",Money(record.AmountCents));row.SetText("actionLabel","Withdraw");
                row.SetText("statusLabel","Pending - not paid");
                row.SetVisible("readyVisual",!record.Pending);row.SetVisible("pendingVisual",record.Pending);
                row.SetVisible("actionButton",!record.Pending);row.SetVisible("statusLabel",record.Pending);
                var button=row.GetButton("actionButton");if(button!=null){button.interactable=!record.Pending;button.gameObject.SetActive(!record.Pending);}
                row.Bind("actionButton",()=>{diamonds=false;earlyOrdinal=ordinal;sequence=StartCoroutine(LoadAccount());});
            }
            var current=Instantiate(prefab,parent.transform,false).GetComponent<ApkEarningPageView>();
            current.SetText("titleLabel","Cash Reward #3");current.SetText("timeLabel",string.Empty);
            current.SetText("amountLabel",Money(Service.State.CashCents));current.SetText("actionLabel","Withdraw");
            current.SetVisible("statusLabel",false);current.SetVisible("readyVisual",true);current.SetVisible("pendingVisual",false);
            current.Bind("actionButton",()=>{
                if(earlyOrdinal>0&&!ArchiveEarly(false,()=>{diamonds=false;ShowConditions();}))return;
                diamonds=false;ShowConditions();
            });
        }
        private void ShowReward()
        {
            if(Service.State.PendingRewards.Count==0){Close();return;}
            reward=Service.State.PendingRewards[0];Show(reward.Kind=="lucky"?"LocalLuckyReward":"LocalSuccess");
            page.SetText("labels[2].Text",reward.Kind=="lucky"?"Lucky Reward":"Level Complete");
            page.SetVisible("ad",reward.EarlyOrdinal==0);
            page.SetVisible("MoreGetBtn",true);page.SetVisible("GetBtn",reward.EarlyOrdinal==0);page.SetVisible("Close",false);
            page.SetText("moreLabel",reward.EarlyOrdinal>0?"Withdraw":"Claim "+Service.Rules.BonusMultiplier+"X");page.SetText("labels[1].Text","Claim");
            page.Bind("GetBtn",ClaimReward);page.Bind("MoreGetBtn",()=>{if(reward.EarlyOrdinal>0)ClaimReward();else ShowTestVideo();});
            var parent=page.GetObject("Items");
            if(parent!=null){if(reward.CashCents>0)AddRewardItem(parent.transform,false,reward.CashCents);if(reward.Diamonds>0)AddRewardItem(parent.transform,true,reward.Diamonds);}
        }
        private void AddRewardItem(Transform parent,bool diamond,long amount)
        {
            var prefab=Resources.Load<GameObject>("ApkEarningReference/Panels/"+(reward.Kind=="lucky"?"LocalLuckyItem":"Item_Big"));
            if(prefab==null)return;
            var item=Instantiate(prefab,parent,false).GetComponent<ApkEarningPageView>();
            item.SetText("count",diamond?DiamondText(amount):Money(amount));item.SetVisible("max",false);
            var icon=item.GetImage("icon");var art=Resources.Load<Sprite>("ApkEarningReference/Artwork/"+(diamond?"DiamondPile":"CashPile"));
            if(icon!=null&&art!=null){icon.sprite=art;icon.preserveAspect=true;}
        }
        private void ShowTestVideo()
        {
            if(reward==null||reward.EarlyOrdinal>0||busy)return;
            StopSequence();Show("LocalRewardVideoTest");
            page.SetText("title","Test reward video");
            page.SetText("actionCaption","Complete test video");page.SetText("cancelCaption","Cancel");
            var confirm=page.GetButton("retry");if(confirm!=null)confirm.interactable=false;
            page.Bind("close",CancelTestVideo);page.Bind("Close",CancelTestVideo);page.Bind("CloseBtn",CancelTestVideo);
            page.Bind("retry",()=>{if(confirm!=null&&confirm.interactable){StopSequence();ClaimReward(true);}});
            sequence=StartCoroutine(TestVideoCountdown());
        }
        private IEnumerator TestVideoCountdown()
        {
            var view=page;float remaining=Mathf.Max(0,config.TestVideoSeconds);int shown=-1;
            while(remaining>0&&page==view)
            {
                int seconds=Mathf.CeilToInt(remaining);
                if(seconds!=shown){shown=seconds;view.SetText("body","Local test video · "+seconds+"s\nNo ad network or payment is connected.\nComplete to collect "+Service.Rules.BonusMultiplier+"X, or cancel without changing your reward.");}
                remaining-=Time.unscaledDeltaTime;yield return null;
            }
            sequence=null;
            if(page!=view)yield break;
            view.SetText("body","Test video finished.\nTap Complete test video to collect "+Service.Rules.BonusMultiplier+"X.\nCancel keeps the normal reward available.");
            var button=view.GetButton("retry");if(button!=null)button.interactable=true;
        }
        private void CancelTestVideo()
        {
            StopSequence();
            if(reward!=null)Service.ClaimWithTestVideo(reward.Id,false);
            ShowReward();
        }
        private void ClaimReward() { ClaimReward(false); }
        private void ClaimReward(bool testVideoCompleted)
        {
            if(reward==null||busy)return;busy=true;bool claimed;
            try{claimed=testVideoCompleted?Service.ClaimWithTestVideo(reward.Id,true):Service.Claim(reward.Id);}finally{busy=false;}
            if(!claimed){
                if(Service.Available&&string.IsNullOrEmpty(Service.Error)){ShowReward();return;}
                ShowRecovery(()=>ClaimReward(testVideoCompleted));return;
            }
            long cash=checked(reward.CashCents*(testVideoCompleted?Service.Rules.BonusMultiplier:1));
            earlyOrdinal=reward.EarlyOrdinal;sequence=StartCoroutine(Receipt(cash));
        }
        private IEnumerator Receipt(long cash)
        {
            Show("LocalEarningSequence");SequenceMode(-1,true,false);page.SetText("receiptAmount",Money(cash));
            yield return page.PlayReceipt(ApkEarningTopView.CashReceiptTarget,ReceiptSeconds);sequence=null;
            if(earlyOrdinal>0){Show("TXGuideTargetCompletePanel");page.SetText("labels[0].Text","Withdraw");page.SetText("labels[1].Text","Withdrawal on Clear");page.SetText("tip",Money(cash));page.Bind("continueButton",()=>{diamonds=false;ShowWallet();});}
            else if(Service.State.PendingRewards.Count>0)ShowReward();else Close();
        }
        private void StartActivation()
        {
            if(sequence!=null)StopCoroutine(sequence);sequence=StartCoroutine(Activation());
        }
        private void StartDiamondGuide()
        {
            if(sequence!=null)StopCoroutine(sequence);
            sequence=StartCoroutine(DiamondGuide());
        }
        private IEnumerator DiamondGuide()
        {
            Show("LocalEarningSequence");SequenceMode(3,false,false);
            SetMessage(3,"Extra reward unlocked","Reward preview\n"+DiamondText(config.DiamondPreviewHundredths)+" diamonds");
            yield return new WaitForSecondsRealtime(Mathf.Max(0,config.DiamondPreviewSeconds));
            sequence=null;ShowDiamondGuide();
        }
        private void ShowDiamondGuide()
        {
            Show("LocalDiamondUnlockGuide");
            page.SetText("title","Diamond rewards unlocked");
            page.SetText("body","Diamond rewards are unlocked. Tap to continue playing.");
            page.SetText("diamondAmount",DiamondText(Service.State.Diamonds));
            page.Bind("continueButton",()=>{
                busy=true;bool saved;try{saved=Service.CompleteDiamondGuide();}finally{busy=false;}
                if(!saved){ShowRecovery(ShowDiamondGuide);return;}
                Close();
            });
        }
        private IEnumerator Activation()
        {
            Show("LocalEarningSequence");
            while(Service.State.ActivationStep<6)
            {
                int step=Service.State.ActivationStep;
                if(step==2)
                {
                    SequenceMode(-1,false,true);page.SetText("bonusesTitle","Your reward bonuses");
                    string[] captions={"Extra cash reward","Match three fruits to earn a reward","Test reward video bonus","Unlock diamond rewards","Withdrawal requirements"};
                    for(int i=0;i<captions.Length;i++)page.SetText("bonusCaptions["+i+"]",captions[i]);
                    yield return page.PlayBonusSequence(BonusesSeconds);
                }
                else if(step==3)
                {
                    busy=true;bool applied;try{applied=Service.ApplyActivationBalance();}finally{busy=false;}
                    if(!applied){sequence=null;ShowRecovery(StartActivation);yield break;}
                    SequenceMode(-1,true,false);page.SetText("receiptAmount",Money(Service.State.CashCents));
                    yield return page.PlayReceipt(ApkEarningTopView.CashReceiptTarget,ReceiptSeconds);
                }
                else
                {
                    int layout=step<2?step:step-2;
                    SequenceMode(layout,false,false);
                    if(step==0)SetMessage(layout,"Reward assessment","Your first two cash rewards are saved in withdrawal history.");
                    else if(step==1)SetMessage(layout,"Activating rewards","Preparing your cash rewards and level bonuses...");
                    else if(step==4)SetMessage(layout,"Rewards activated","Continue playing to collect cash and unlock more rewards.");
                    else SetMessage(layout,"Your withdrawal history","Your reward records are saved. Submitted requests remain pending; no payment has been made.");
                    yield return new WaitForSecondsRealtime(MessageSeconds);
                }
                busy=true;bool saved;try{saved=Service.SetActivationStep(step+1);}finally{busy=false;}
                if(!saved){sequence=null;ShowRecovery(StartActivation);yield break;}
            }
            busy=true;bool complete;try{complete=Service.CompleteActivation();}finally{busy=false;}
            sequence=null;
            if(!complete){ShowRecovery(StartActivation);yield break;}
            Close();
        }
        private void SequenceMode(int message,bool receipt,bool bonuses)
        {
            page.SetVisible("messageGroup",message>=0);page.SetVisible("receiptGroup",receipt);page.SetVisible("bonusesGroup",bonuses);
            for(int i=0;i<4;i++)page.SetVisible("messageLayouts["+i+"].Root",i==message);
            for(int i=0;i<5;i++)page.SetVisible("flightIcons["+i+"]",false);
            RefreshTestDisclosure();
        }
        private void SetMessage(int index,string title,string body){page.SetText("messageLayouts["+index+"].Title",title);page.SetText("messageLayouts["+index+"].Body",body);}
        private void ShowRecovery(Action retry)
        {
            Show("LocalEarningRecovery");
            page.SetText("title","Local save unavailable");
            page.SetText("body",Service.Error+"\nYour last saved rewards are preserved. Retry saving, or return to the game and recover them on restart.");
            page.Bind("retry",()=>{
                busy=true;bool ready;
                try{ready=Service.Init(JEFOMCDAPGK.Instance.CurMainLevelIndex);}finally{busy=false;}
                if(!ready){ShowRecovery(retry);return;}
                retry();
            });
            // Leaving does not claim, archive, submit or erase the failed transaction.
            page.Bind("close",()=>MgrUI.Instance.Close(Path,false));
            page.Bind("Close",()=>MgrUI.Instance.Close(Path,false));
        }
        private void Notice(string message)
        {
            if(page==null)return;
            if(string.IsNullOrEmpty(message))message="This action is unavailable. Please try again.";
            if(page.GetText("notice")!=null){page.SetText("notice",message);return;}
            // The APK's saved toast prefab keeps feedback visible even on reward pages with no Tip field.
            ClearNotice();
            var prefab=Resources.Load<GameObject>("ApkEarningReference/Panels/Tip");
            if(prefab==null){Debug.LogError(message);return;}
            noticeView=Instantiate(prefab,PageRoot,false).GetComponent<ApkEarningPageView>();
            noticeView.SetText("content",message);noticeView.RefreshLayout();
            noticeSequence=StartCoroutine(HideNotice());
        }
        private IEnumerator HideNotice()
        {
            yield return new WaitForSecondsRealtime(MessageSeconds);
            noticeSequence=null;if(noticeView!=null)Destroy(noticeView.gameObject);noticeView=null;
        }
        private void ClearNotice()
        {
            if(noticeSequence!=null)StopCoroutine(noticeSequence);noticeSequence=null;
            if(noticeView!=null){noticeView.gameObject.SetActive(false);Destroy(noticeView.gameObject);}noticeView=null;
        }
    }
}
