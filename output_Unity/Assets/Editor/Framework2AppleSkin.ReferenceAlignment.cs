using UnityEngine;
using UnityEngine.UI;

// Reference rectangles are measured on the approved boards, not guessed canvas units.
public static partial class Framework2AppleSkin
{
    static Rect referencePhone;
    static void RefPhone(float x=15, float y=100, float w=397, float h=770)
    { referencePhone = new Rect(x,y,w,h); }

    static void RefBox(GameObject root, string path, float x, float y, float w, float h, int font=0)
    {
        Transform target = root.transform.Find(path); if (target == null) return;
        Vector3 canvas = new Vector3(((x+w*.5f-referencePhone.x)/referencePhone.width-.5f)*1200,
            (.5f-(y+h*.5f-referencePhone.y)/referencePhone.height)*2132,0);
        Vector3 local = target.parent.InverseTransformPoint(root.transform.TransformPoint(canvas));
        NavRect(target,local.x,local.y,w/referencePhone.width*1200,h/referencePhone.height*2132);
        if(font>0 && target.GetComponent<Text>() is Text text)
        {
            text.fontSize=font; text.resizeTextMaxSize=font;
            text.resizeTextMinSize=Mathf.Max(20,font-4);
        }
    }

    static void RefLabel(GameObject root,string path,string value)
    { NavLiteral(root.transform.Find(path),value); }

    static void RefAccountFrame(GameObject root,string panel,float x,float y,float w,float h)
    {
        RefBox(root,panel,x,y,w,h);
        RefBox(root,panel+"/AppleHeader",x+w*.5f-137,y-25,274,60);
        RefBox(root,panel+"/AppleHeaderText",x+w*.5f-130,y-23,260,54,73);
        RefBox(root,panel+"/close",x+w-45,y-10,50,50);
        RefBox(root,panel+"/AppleTestFooter",x+26,y+h-39,w-52,22,35);
        AccAddArt(root.transform.Find(panel),"AppleReferenceLeaf","leaf",0,0,100,100);
        RefBox(root,panel+"/AppleReferenceLeaf",x+28,y-43,50,50);
        root.transform.Find(panel+"/AppleReferenceLeaf")?.SetAsLastSibling();
        root.transform.Find(panel+"/AppleHeader")?.SetAsLastSibling();
        root.transform.Find(panel+"/AppleHeaderText")?.SetAsLastSibling();
        root.transform.Find(panel+"/close")?.SetAsLastSibling();
    }

    static void RefRewardFrame(GameObject root,float x,float y,float w,float h)
    {
        RefBox(root,"SkinSurface",x,y,w,h);
        RefBox(root,"SkinHeading",x+w*.5f-137,y-26,274,60);
        RefBox(root,"SkinHeading/SkinHeadingText",x+w*.5f-128,y-23,256,52,74);
        RefBox(root,"SkinLeaf",x+28,y-44,50,50);
        RefBox(root,"SkinTestFooter",x+24,y+h-39,w-48,22,34);
    }

    static void ApplyReferenceAlignment(GameObject root,string name)
    {
        RefPhone();
        switch(name)
        {
            case "WithDrawmethod":
                RefAccountFrame(root,"PanelA",28,274,372,425);
                RefBox(root,"PanelA/PanelB",52,321,324,128);
                for(int i=0;i<3;i++)
                {
                    string p="PanelA/PanelB/Toggle"+(i==0?"A":i==1?"B":"C");
                    RefBox(root,p,52+i*111,321,101,128);
                    RefBox(root,p+"/Background",52+i*111,321,101,128);
                    RefWhole(root.transform.Find(p+"/Background").GetComponent<Image>(),new Rect(52+i*111,321,101,128),9);
                    root.transform.Find(p+"/AppleChannelLogo").gameObject.SetActive(false);
                    root.transform.Find(p+"/AppleChannelLabel").gameObject.SetActive(false);
                    RefBox(root,p+"/Background/Checkmark",91+i*111,418,24,24);
                    RefWhole(root.transform.Find(p+"/Background/Checkmark").GetComponent<Image>(),new Rect(313,417,24,24),12);
                }
                root.transform.Find("PanelA/Panel").gameObject.SetActive(true);
                root.transform.Find("PanelA/Panel/ques").gameObject.SetActive(true);
                root.transform.Find("PanelA/Panel/ques").GetComponent<Image>().enabled=true;
                foreach(Text text in root.transform.Find("PanelA/Panel").GetComponentsInChildren<Text>(true))
                    if(text.name!="AppleQuestionMark") text.enabled=false;
                RefBox(root,"PanelA/Panel/ques",338,457,34,34);
                RefWhole(root.transform.Find("PanelA/Panel/ques").GetComponent<Image>(),new Rect(338,457,34,34),17);
                root.transform.Find("PanelA/Panel/ques/AppleQuestionMark").gameObject.SetActive(false);
                RefBox(root,"PanelA/AppleInputLabel",56,457,270,38,57);
                RefBox(root,"PanelA/PanelC",56,495,314,53);
                RefBox(root,"PanelA/PanelC/Email",56,495,314,53);
                RefBox(root,"PanelA/PanelC/Email/AppleInputErrorBorder",54,493,318,57);
                RefBox(root,"PanelA/PanelC/Name",56,495,314,53);
                RefBox(root,"PanelA/PanelC/phoneFront",56,495,65,53);
                RefBox(root,"PanelA/PanelC/phoneBehind",124,495,246,53);
                RefBox(root,"PanelA/AppleAccountNotice",56,559,314,29,37);
                RefBox(root,"PanelA/Confirm",56,596,314,58);
                foreach(Text label in root.transform.Find("PanelA/Confirm").GetComponentsInChildren<Text>(true))
                    NavLiteral(label.transform,"Confirm");
                RefLabel(root,"PanelA/Confirm/Text","Confirm");
                RefBox(root,"PanelE",56,548,314,29);
                RefBox(root,"PanelE/Image",56,548,314,29);
                RefBox(root,"PanelE/Image/content",56,548,314,29,38);
                RefBox(root,"PanelA/ApplePhoneMethod",164,321,101,128);
                RefBox(root,"PanelA/PanelC/BrazilPixControls",56,321,314,128);
                string[] pixNames={"CpfCnpj","Phone","Email","Random"};
                for(int i=0;i<4;i++)
                {
                    string p="PanelA/PanelC/BrazilPixControls/"+pixNames[i];
                    RefBox(root,p,56+i*79,321,74,108);
                    RefBox(root,p+"/Label",58+i*79,352,70,36,32);
                    RefBox(root,p+"/Selection",81+i*79,398,24,24);
                }
                break;
            case "WithDrawConfirm":
                RefAccountFrame(root,"PanelA",28,274,372,425);
                RefBox(root,"PanelA/PanelC",56,326,314,183);
                RefBox(root,"PanelA/method",174,341,78,72);
                RefBox(root,"PanelA/PanelC/YourAccount",70,414,286,35,51);
                RefBox(root,"PanelA/PanelC/phone",68,450,290,40,61);
                RefBox(root,"PanelA/ReEnter",56,529,314,55);
                RefBox(root,"PanelA/Confirm",56,595,314,59);
                break;
            case "WithDrawQues":
                RefAccountFrame(root,"PanelA",28,274,372,420);
                RefBox(root,"PanelA/PanelC",56,334,314,104);
                RefBox(root,"PanelA/PanelC/content",56,334,314,104,51);
                RefBox(root,"PanelA/AppleInputLabel",56,449,280,40,54);
                RefBox(root,"PanelA/phone",56,491,314,55);
                RefBox(root,"PanelA/Confirm",56,578,314,60);
                break;
            case "WithDraw": AlignWithdraw(root); break;
            case "WithDrawProcess1": AlignProcess(root); break;
            case "WithDrawHistory":
                RefPhone(21,78,401,813);
                RefBox(root,"Panel",34,263,372,498);
                RefAccountFrame(root,"Panel/bg",34,263,372,498);
                RefBox(root,"Panel/PanelA",57,312,327,95);
                RefBox(root,"Panel/PanelA/AppleTotalLabel",85,315,270,27,46);
                RefBox(root,"Panel/PanelA/money",116,340,69,57);
                RefBox(root,"Panel/PanelA/Num",188,340,170,57,112);
                RefBox(root,"Panel/Scroll View",57,410,327,306);
                RefBox(root,"Panel/Scroll View/Viewport",60,415,321,296);
                RefBox(root,"Panel/AppleEmptyState",57,410,327,306);
                RefBox(root,"Panel/AppleEmptyState/Receipt",136,480,170,125);
                RefBox(root,"Panel/AppleEmptyState/Label",80,610,280,44,62);
                break;
            case "PayPalErrorView":
                RefPhone(846,78,388,813);
                RefAccountFrame(root,"PanelA",858,264,365,497);
                RefBox(root,"PanelA/PanelB",879,311,320,169);
                RefBox(root,"PanelA/PanelB/method",1010,314,60,60);
                RefBox(root,"PanelA/PanelB/CurLuckyCoin",901,378,280,64,114);
                RefBox(root,"PanelA/PanelC",891,440,300,36);
                RefBox(root,"PanelA/PanelC/email",891,440,300,36,52);
                RefBox(root,"PanelA/AppleErrorExplanation",891,481,298,88,49);
                RefBox(root,"PanelA/ReEnter",887,588,310,58);
                RefBox(root,"PanelA/Confirm",887,654,310,48);
                break;
            case "WithdrawMissionView": AlignMission(root); break;
            case "WithdrawFeedbackView":
                RefPhone(11,101,403,777);
                RefRewardFrame(root,34,285,363,394);
                RefBox(root,"SkinSavedInset",56,328,319,312);
                RefBox(root,"SkinSavedCash",87,345,110,100);
                RefBox(root,"SkinSavedCheck",155,399,44,44);
                RefBox(root,"SkinRewardAmount",201,359,165,64,107);
                RefBox(root,"RewardMessage/Panel/Top/Title",78,454,285,65,55);
                RefBox(root,"RewardMessage/Panel/Bottom/GenerateMoreBtn",66,535,300,55);
                RefBox(root,"RewardMessage/Panel/continuebtn",69,599,294,48);
                RefBox(root,"RewardMessage/Panel/bg/close",351,270,49,49);
                // Reference 15 has its local-test disclosure in the persistent footer.
                RefBox(root,"SkinTestFooter",70,651,292,18,25);
                break;
            case "SlotView": AlignSlot(root); break;
            case "ProgressView": AlignStageProgress(root); break;
            case "BloatView": AlignBonus(root); break;
            case "BonusWheelView": AlignWheel(root,true); break;
            case "LuckySpinView": AlignWheel(root,false); break;
            case "WithDrawSuc":
                RefPhone(1260,99,402,770); RefRewardFrame(root,1288,282,346,451);
                RefBox(root,"SkinCashPile",1313,345,294,163);
                RefBox(root,"PanelA/bg/content",1310,515,303,76,118);
                RefBox(root,"SkinBalanceInset",1320,595,282,50);
                RefBox(root,"SkinCurrentBalance",1329,600,265,40,54);
                RefBox(root,"PanelA/bg/Dec",1310,652,300,30,30);
                RefBox(root,"PanelA/bg/close",1589,269,49,49);
                break;
            case "BloatProgressView": AlignFinalProgress(root); break;
            case "RewardView": AlignSettlement(root,false); break;
            case "ChallangeSuccessView": AlignSettlement(root,true); break;
            case "LuckySuccessView": AlignLuckySuccess(root); break;
            case "GameBalancView": AlignWallet(root); break;
            case "GamePlayerView": AlignHud(root); break;
            case "SettingsView": AlignSettings(root); break;
            case "UserLevelView": AlignUserLevel(root); break;
            case "ReStartView": AlignRestart(root); break;
            case "WithdrawView":
                // Optional and regional pages retain the source entry/visibility policy.
                AlignOptionalFrame(root,"Plane",1120,1500); break;
            case "WithDrawContinue": AlignOptionalFrame(root,"Panel",1120,1470); break;
        }
    }

    static void AlignWithdraw(GameObject root)
    {
        RefPhone(21,81,397,793); RefAccountFrame(root,"Panel",45,281,354,474);
        RefBox(root,"Panel/PanelA",65,326,311,165); RefBox(root,"Panel/PanelA/PanelD",65,326,311,165);
        RefBox(root,"Panel/PanelA/PanelD/PanelE",65,326,311,108);
        RefBox(root,"Panel/PanelA/PanelD/PanelE/Image",81,343,75,78);
        RefBox(root,"Panel/PanelA/PanelD/PanelE/content",176,334,194,48,106);
        RefBox(root,"Panel/PanelA/PanelD/PanelE/AccountVerified",176,383,194,46,43);
        RefBox(root,"Panel/PanelA/PanelD/Process",77,436,290,33);
        AccProgress(root,"Panel/PanelA/PanelD/Process","Background","Fill","num",877,89);
        RefBox(root,"Panel/PanelB",65,491,311,119);
        RefBox(root,"Panel/PanelB/RecordBtn",77,500,92,33);
        RefBox(root,"Panel/PanelB/withdrawProcess",173,493,190,46,43);
        RefBox(root,"Panel/PanelB/WithDraw",77,540,290,57);
        RefBox(root,"Panel/PanelC",65,622,311,89);
        RefBox(root,"Panel/PanelC/AppleLockedIcon",83,631,56,66);
        RefBox(root,"Panel/PanelC/AppleWalletIcon",67,623,132,104);
        RefBox(root,"Panel/PanelC/Tips",153,635,217,65,49);
        RefBox(root,"Panel/PanelC/Lucky",198,626,170,25,43);
        RefBox(root,"Panel/PanelC/CurLuckyCoin",198,649,170,33,74);
        RefBox(root,"Panel/PanelC/CashOut",205,683,163,42);
    }

    static void AlignProcess(GameObject root)
    {
        RefPhone(428,101,402,777); RefAccountFrame(root,"PanelA",448,267,363,482);
        RefBox(root,"PanelA/PanelB",470,311,318,87);
        RefBox(root,"PanelA/PanelB/money",493,319,74,75);
        RefBox(root,"PanelA/PanelB/content",582,319,193,47,105);
        RefBox(root,"PanelA/PanelB/AppleLocalCaption",570,369,210,30,37);
        RefBox(root,"PanelA/PanelC",477,402,303,210);
        RefBox(root,"PanelA/PanelC/StateRoot",477,402,303,210);
        for(int i=1;i<=5;i++)
        {
            string p="PanelA/PanelC/StateRoot/State"+i;
            RefBox(root,p,486,402+(i-1)*51,293,48);
            RefBox(root,p+"/AppleStateBackground",530,402+(i-1)*51,243,48);
            RefBox(root,p+"/Finsh",498,406+(i-1)*51,36,36);
            RefBox(root,p+"/wait",498,406+(i-1)*51,36,36);
            RefBox(root,p+"/content1",550,406+(i-1)*51,217,39,48);
        }
        RefBox(root,"PanelA/PanelC/Dec",476,620,305,70,44);
        RefBox(root,"PanelA/Continue",476,655,305,57);
        RefBox(root,"PanelA/AppleTestFooter",479,712,299,22,33);
    }

    static void AlignMission(GameObject root)
    {
        RefPhone(1250,78,401,813); RefRewardFrame(root,1268,267,367,519);
        RefBox(root,"SkinChest",1345,307,211,103);
        RefBox(root,"SkinTaskInset",1289,408,326,335);
        RefBox(root,"SkinMissionRewardLabel",1310,411,285,26,43);
        RefBox(root,"RewardMessage/Panel/Top/Bg2/CurCoin",1330,434,247,45,99);
        RefBox(root,"RewardMessage/Panel/Middle/BG",1340,500,225,18);
        for(int i=1;i<=3;i++)
        {
            string p="RewardMessage/Panel/Middle/ProgressStateRoot/ProgressState"+i;
            RefBox(root,p,1320+(i-1)*104,480,60,60);
            RefBox(root,p+"/Finish",1320+(i-1)*104,480,60,60);
            RefBox(root,p+"/Unfinished",1320+(i-1)*104,480,60,60);
            RefBox(root,p+"/Count",1325+(i-1)*104,487,50,45,72);
        }
        RefBox(root,"SkinMissionTrack",1296,544,313,26);
        RefBox(root,"SkinMissionFill",1299,547,307,20);
        RefBox(root,"SkinMissionCount",1306,544,293,26,43);
        RefBox(root,"RewardMessage/Panel/Bottom/Dec",1300,578,300,65,45);
        RefBox(root,"RewardMessage/Panel/Middle/QueueSize",1300,637,300,15,24);
        RefBox(root,"RewardMessage/Panel/Bottom/GenerateMoreBtn",1297,651,312,48);
        RefBox(root,"RewardMessage/continuebtn",1297,706,312,38);
        RefBox(root,"RewardMessage/close",1589,254,52,52);
    }

    static void AlignSlot(GameObject root)
    {
        RefPhone(11,99,402,770); RefRewardFrame(root,35,267,360,466);
        RefBox(root,"LotteryPanel/Image (2)",63,320,305,115);
        string[] columns={"Panel (3)","Panel (2)","Panel (1)","Panel"};
        for(int i=0;i<4;i++) RefBox(root,"LotteryPanel/Image (2)/"+columns[i],75+i*70,331,63,92);
        RefBox(root,"SkinCashPile",72,448,287,88);
        RefBox(root,"SkinPossibleLabel",70,539,292,30,49);
        RefBox(root,"SkinRewardAmount",80,568,270,49,108);
        RefBox(root,"ClickBtn",63,624,305,57);
        RefBox(root,"CloseBtn",348,250,48,48);
    }

    static void AlignStageProgress(GameObject root)
    {
        RefPhone(580,58,514,831); RefRewardFrame(root,610,308,444,492);
        RefBox(root,"SkinHeading",659,321,361,67);
        RefBox(root,"SkinHeading/SkinHeadingText",670,327,340,54,85);
        RefBox(root,"SkinCashPile",647,203,381,128);
        RefBox(root,"SkinAmountInset",641,399,397,108);
        RefBox(root,"SkinRewardAmount",667,405,345,64,125);
        RefBox(root,"SkinAwardLabel",660,469,355,33,54);
        RefBox(root,"SkinTargetInset",641,517,397,161);
        RefBox(root,"SkinTargetLabel",668,522,345,35,53);
        RefBox(root,"ProgressRoot/BG/Progress/Count",667,554,344,33,54);
        RefBox(root,"ProgressRoot/BG",657,594,307,35);
        RefBox(root,"SkinProgressPercent",973,594,55,35,54);
        RefBox(root,"dec",662,639,365,31,43);
        RefBox(root,"bg/Panel/content",662,639,365,31,43);
        RefBox(root,"continuebtn",645,679,389,65);
        RefBox(root,"bg/close",1016,289,61,61);
    }

    static void AlignBonus(GameObject root)
    {
        RefPhone(428,99,400,770); RefRewardFrame(root,447,267,362,466);
        RefBox(root,"SkinCashPile",497,322,254,112);
        RefBox(root,"SkinIncreaseArrow",649,319,78,98);
        root.transform.Find("SkinIncreaseArrow").localRotation=Quaternion.Euler(0,0,-135);
        RefBox(root,"SkinTargetLabel",480,432,297,29,49);
        RefBox(root,"SkinTargetAmount",480,457,297,49,109);
        RefBox(root,"bg/Panel/Dec",480,505,297,25,46);
        RefBox(root,"SkinCurrentInset",482,535,294,78);
        RefBox(root,"SkinCurrentLabel",493,541,272,28,45);
        RefBox(root,"SkinBalanceMoney",507,572,60,39);
        RefBox(root,"SkinCurrentBalance",581,571,177,39,80);
        RefBox(root,"bg/ClickBtn",474,624,310,57);
        RefBox(root,"bg/close",761,250,48,48);
    }

    static void AlignWheel(GameObject root,bool bonus)
    {
        if(bonus)
        {
            RefPhone(844,99,402,770); RefRewardFrame(root,866,267,359,466);
            RefBox(root,"Plane/Turntable",880,301,333,329);
            RefBox(root,"Plane/Turntable/Table",880,301,333,329);
            RefBox(root,"Plane/Turntable/Pointer",1025,288,38,50);
            RefBox(root,"Plane/LotteryBtn",985,407,120,120);
            RefBox(root,"SkinWheelBalanceInset",896,630,301,55);
            RefBox(root,"SkinLocalWheelLabel",901,634,292,25,44);
            RefBox(root,"SkinCurrentBalance",901,656,292,25,44);
            RefBox(root,"ExitBtn",1178,250,48,48);
            for(int i=0;i<8;i++)
            {
                var rt=root.transform.Find("Plane/Turntable/Table/SkinBonusCash"+i) as RectTransform;
                if(rt!=null) { rt.anchoredPosition*=.99f; rt.sizeDelta=new Vector2(174,154); }
            }
        }
        else
        {
            RefPhone(1253,106,404,763); RefRewardFrame(root,1267,284,379,554);
            RefBox(root,"Plane/Turntable",1279,323,356,356);
            RefBox(root,"Plane/Turntable/Table",1279,323,356,356);
            RefBox(root,"Plane/Turntable/Table/SkinWheelDisc",1279,323,356,356);
            RefBox(root,"Plane/Turntable/Pointer",1435,312,41,53);
            RefBox(root,"Plane/LotteryBtn",1318,696,279,66);
            RefBox(root,"SkinLocalWheelLabel",1304,773,305,25,40);
            RefBox(root,"ExitBtn",1594,268,52,52);
        }
    }

    static void AlignFinalProgress(GameObject root)
    {
        RefPhone(13,106,409,763); RefRewardFrame(root,27,342,380,485);
        RefBox(root,"SkinCurrentInset",51,391,335,111);
        RefBox(root,"SkinCashIcon",66,405,80,82);
        RefBox(root,"SkinCurrentBalance",157,400,226,57,113);
        RefBox(root,"SkinSimulationLabel",159,451,220,27,45);
        RefBox(root,"SkinLocalDetails",159,477,220,24,37);
        RefBox(root,"ProgressRoot/BG",56,512,264,36);
        RefBox(root,"ProgressRoot/OverallProgress",331,512,52,36);
        RefBox(root,"SkinRemainingInset",51,568,335,88);
        RefBox(root,"SkinRemainingMoney",69,589,73,57);
        RefBox(root,"SkinRemainingLabel",150,576,226,30,48);
        RefBox(root,"SkinRemainingAmount",150,605,226,42,92);
        RefBox(root,"bg/ClickBtn",51,669,337,65);
        RefBox(root,"bg/Panel/Dec",60,747,317,34,38);
        RefBox(root,"bg/close",366,333,50,50);
    }

    static void AlignSettlement(GameObject root,bool challenge)
    {
        if(challenge) RefPhone(1115,58,523,831); else RefPhone(436,106,397,763);
        float x=challenge?1153:455,y=challenge?303:321,w=challenge?452:360,h=challenge?510:504;
        RefRewardFrame(root,x,y,w,h);
        string p="Plane/RewardRoot/Reward1/Reward1";
        RefBox(root,p+"/MoneyIcon",x+40,y+(challenge?-110:54),w-80,challenge?150:143);
        RefBox(root,p+"/IAAMoneyIcon",x+40,y+54,w-80,143);
        RefBox(root,p+"/count",x+26,y+(challenge?104:185),w-52,challenge?90:63,124);
        RefBox(root,"SkinLocalLabel",x+35,y+243,w-70,31,48);
        RefBox(root,"SkinLocalDetails",x+35,y+272,w-70,29,42);
        if(challenge)
        {
            RefBox(root,"SkinHeading",x+56,y+28,w-112,63);
            RefBox(root,"SkinHeading/SkinHeadingText",x+66,y+32,w-132,55,83);
            for(int i=1;i<=6;i++)
            {
                string step=i==6?"ProgressRoot/ProgressState6":"ProgressRoot/ProgressStateRoot/ProgressState"+i;
                RefBox(root,step,x+43+(i-1)*64,y+199,55,55);
                RefBox(root,step+"/Finish",x+43+(i-1)*64,y+199,55,55);
                RefBox(root,step+"/Unfinished",x+43+(i-1)*64,y+199,55,55);
                RefBox(root,step+"/Count",x+48+(i-1)*64,y+205,45,43,61);
            }
            RefBox(root,"ProgressRoot/BG",x+65,y+221,338,17);
            RefBox(root,"ProgressRoot/Dec",x+38,y+269,w-76,32,39);
        }
        RefBox(root,"Plane/AdBtn",x+24,y+(challenge?311:302),w-48,challenge?77:70);
        RefBox(root,"Plane/AdBtn/icon",x+73,y+(challenge?322:315),58,47);
        RefBox(root,"Plane/AdBtn/Text",x+138,y+(challenge?316:308),w-171,58,79);
        RefBox(root,"Plane/OnlyBtn",x+24,y+(challenge?397:381),w-48,challenge?59:63);
    }

    static void AlignLuckySuccess(GameObject root)
    {
        RefPhone(847,106,392,763); RefRewardFrame(root,863,321,363,504);
        RefBox(root,"SkinLuckyWallet",892,366,306,164);
        RefBox(root,"Plane/CurrentLuckyCoinTxT",894,521,301,58,121);
        RefBox(root,"Plane/CardBg/Lucky",894,576,301,31,47);
        RefBox(root,"Plane/CardBg/WithdrawAnytime",894,606,301,27,40);
        RefBox(root,"Plane/AdBtn",887,639,315,69);
        RefBox(root,"Plane/AdBtn/icon",937,650,57,43);
        RefBox(root,"Plane/AdBtn/Text",999,643,192,58,79);
        RefBox(root,"Plane/AbandonBtn",889,716,311,54);
    }

    static void AlignWallet(GameObject root)
    {
        RefPhone(17,101,399,769); RefRewardFrame(root,30,253,376,504);
        RefBox(root,"SkinWalletBalanceInset",57,299,323,108);
        RefBox(root,"SkinWalletMoney",73,310,91,86);
        RefBox(root,"Plane/Bg2/CurrrCoin",187,303,185,56,116);
        RefBox(root,"Plane/Bg2/AddCoin ",187,357,185,43,45);
        RefBox(root,"Plane/With",57,416,323,63);
        RefBox(root,"Plane/Bg/SpecialRoot/Special1",58,496,153,131);
        RefBox(root,"Plane/Bg/SpecialRoot/Special2",224,496,155,131);
        RefBox(root,"Plane/Bg/SpecialRoot/Special1/SkinSpecialCash",78,509,114,85);
        RefBox(root,"Plane/Bg/SpecialRoot/Special2/SkinSpecialWheel",253,502,96,96);
        RefBox(root,"Plane/Bg/SpecialRoot/Special1/SpecialTxt1",64,594,142,27,49);
        RefBox(root,"Plane/Bg/SpecialRoot/Special2/SpecialTxt2",229,594,142,27,49);
        RefBox(root,"Plane/Bg/Content",59,646,321,25,40);
        for(int i=1;i<=4;i++) RefBox(root,"Plane/LevelProgressTxt/Dec"+i,59,669+(i-1)*18,321,23,37);
        RefBox(root,"Plane/ExitBtn",357,247,51,51);
    }

    static void AlignHud(GameObject root)
    {
        const string p="Area/TopButtons/";
        NavRect(root,p+"LV",-444,72,170,170);
        NavRect(root,p+"LV/NavLevelNumber",0,0,140,140);
        NavRect(root,p+"CurMoney",35,72,729,144);
        NavRect(root,p+"CurMoney/Bg1",0,0,729,144);
        NavImage(root.transform.Find(p+"CurMoney/Bg1"),"panel",true);
        NavRect(root,p+"CurMoney/Bg1/MoneyIcon",-280,0,124,124);
        NavRect(root,p+"CurMoney/Bg1/MoneyIcon/MoneyIcon",0,0,124,124);
        NavRect(root,p+"CurMoney/Bg1/CoinTxt",-50,0,340,100);
        NavRect(root,p+"CurMoney/CoinBtn",255,0,220,134);
        NavImage(root.transform.Find(p+"CurMoney/CoinBtn"),"title",true);
        NavDecor(root.transform.Find(p+"LV"),"NavLevelLeaf","leaf",-51,62,69,65);
        NavRect(root,p+"Setting",503,72,160,160);
        var presenter=root.GetComponent<Framework2AppleNavigationPresenter>();
        presenter.PrimaryTaskPosition=new Vector2(0,-79); presenter.SecondaryTaskPosition=new Vector2(0,-225);
        foreach(string n in new[]{"NavEarlyTaskRoot","NavAdTaskRoot","NavBalanceTaskRoot"})
        {NavRect(root,p+n,0,-79,1050,94);NavImage(root.transform.Find(p+n),"panel",true);}
        NavRect(root,p+"NavEarlyTaskRoot/NavTrack",-335,0,275,66);
        NavRect(root,p+"NavEarlyTaskRoot/NavFill",-335,0,257,48);
        NavRect(root,p+"NavEarlyTaskRoot/NavObjective",125,0,630,78);
        NavRect(root,p+"NavBalanceTaskRoot/NavTrack",-345,0,285,66);
        NavRect(root,p+"NavBalanceTaskRoot/NavFill",-345,0,267,48);
        NavRect(root,p+"NavBalanceTaskRoot/NavBalanceCount",92,0,590,74);
        NavRect(root,p+"NavBalanceTaskRoot/NavPercent",457,0,125,74);
    }

    static void AlignSettings(GameObject root)
    {
        RefPhone(18,98,418,770);
        RefBox(root,"Plane",37,263,381,524); RefBox(root,"Plane/Bg",37,263,381,524);
        RefBox(root,"Plane/NavTitleBacking",100,237,260,64); RefBox(root,"Plane/Title",109,241,242,55,80);
        RefBox(root,"Plane/NavLeaf",81,222,51,51); RefBox(root,"Plane/ExitBtn",368,256,52,52);
        RefBox(root,"Plane/Bg2",61,313,333,159);
        RefBox(root,"Plane/Bg2/PlayerIcon",75,326,96,103);
        RefBox(root,"Plane/Bg2/UserNameText",187,324,200,35,62);
        RefBox(root,"Plane/Bg2/UserIDText",187,356,200,25,41);
        RefBox(root,"Plane/Bg2/UserLv",187,380,80,27,44);
        RefBox(root,"Plane/Bg2/UserLevelBtn",189,412,197,51);
        RefBox(root,"Plane/Sound",63,480,329,128);
        RefBox(root,"Plane/Vibration",63,545,329,64); NavClearGraphic(root.transform.Find("Plane/Vibration"));
        RefBox(root,"Plane/Sound/Text",139,493,130,44,58);
        RefBox(root,"Plane/Vibration/Text",139,552,140,44,54);
        NavDecor(root.transform.Find("Plane/Sound"),"NavSoundIcon","video",0,0,100,100);
        NavDecor(root.transform.Find("Plane/Vibration"),"NavVibrationIcon","video",0,0,100,100);
        RefBox(root,"Plane/Sound/NavSoundIcon",83,497,42,37);
        RefBox(root,"Plane/Vibration/NavVibrationIcon",85,554,38,44);
        RefBox(root,"Plane/Sound/s_Toggle",284,506,87,43);
        RefBox(root,"Plane/Vibration/v_Toggle",284,565,87,43);
        RefBox(root,"Plane/Bg2/PrivacyPolicyBtn",63,620,329,51);
        RefBox(root,"Plane/Bg2/TermsofUseBtn",63,682,329,50);
        foreach(string buttonName in new[]{"PrivacyPolicyBtn","TermsofUseBtn"})
        {
            Transform button=root.transform.Find("Plane/Bg2/"+buttonName);
            button.Find("NavArrow").gameObject.SetActive(false);
            Text arrow=NavNewText(button,"NavReferenceArrow","›",420,0,70,90,80,Brown);
            foreach(Text text in button.GetComponentsInChildren<Text>(true))
                if(text!=arrow){NavRect(text.transform,-70,0,760,110);text.alignment=TextAnchor.MiddleLeft;}
        }
        RefBox(root,"Plane/NavTestFooter",69,745,317,23,34);
    }

    static void AlignUserLevel(GameObject root)
    {
        RefPhone(454,98,381,770);
        RefBox(root,"Plane",472,265,344,491); RefBox(root,"Plane/Bg",472,265,344,491);
        RefBox(root,"Plane/NavTitleBacking",519,242,253,62); RefBox(root,"Plane/Title",527,246,237,54,79);
        RefBox(root,"Plane/NavLeaf",500,225,50,50); RefBox(root,"Plane/ExitBtn",775,255,50,50);
        RefBox(root,"Plane/Bg2",495,313,301,223);
        RefBox(root,"Plane/NavCurrentLevelLabel",512,321,269,39,65);
        RefBox(root,"Plane/NavLevelBadge",584,366,120,118);
        RefBox(root,"Plane/NavUserLevelNumber",598,375,92,98,159);
        RefBox(root,"Plane/LevelProgress",507,491,215,33);
        RefBox(root,"Plane/LevelProgress/LevelProgressText",734,491,57,33,56);
        RefBox(root,"Plane/Text",515,547,263,69,58);
        RefBox(root,"Plane/KeepPlayingBtn",501,630,289,61);
        RefBox(root,"Plane/NavTestFooter",504,710,283,24,34);
    }

    static void AlignRestart(GameObject root)
    {
        RefPhone(852,98,383,770);
        RefBox(root,"Plane",874,342,327,320); RefBox(root,"Plane/Bg",874,342,327,320);
        RefBox(root,"Plane/NavTitleBacking",932,318,211,56); RefBox(root,"Plane/Title",938,323,199,46,74);
        RefBox(root,"Plane/NavLeaf",911,302,46,46); RefBox(root,"Plane/ExitBtn",1154,331,49,49);
        RefBox(root,"Plane/Bg2",895,383,283,219);
        RefBox(root,"Plane/LevelProgress",919,394,235,62,60);
        RefBox(root,"Plane/ReStartBtn",939,470,228,60);
        RefBox(root,"Plane/ContinueBtn",906,540,260,53);
        RefBox(root,"Plane/NavTestFooter",905,612,264,24,34);
    }

    static void AlignOptionalFrame(GameObject root,string path,float w,float h)
    {
        Transform panel=root.transform.Find(path); if(panel==null)return;
        NavRect(panel,0,-20,w,h);
    }

    static void AlignReferenceHost(GameObject root)
    {
        // Shared host decoration/footer; individual panels are authored separately.
        const string p="TestOverlay/ModalRoot/";
        foreach(string page in new[]{"SelectorPanel","RulesPanel","AdPanel","ErrorPanel"})
        {
            Transform card=root.transform.Find(p+page+"/Card");
            if(card==null)continue;
            NavDecor(card,"NavReferenceLeaf","leaf",-350,(card as RectTransform).rect.height*.5f+25,130,120);
        }
        Transform footer=root.transform.Find("TestOverlay/PersistentTestDisclosure");
        RectTransform rect=(RectTransform)footer;
        rect.anchorMin=rect.anchorMax=new Vector2(.5f,0); rect.pivot=new Vector2(.5f,0);
        rect.anchoredPosition=new Vector2(0,22); rect.sizeDelta=new Vector2(720,77);
        NavRect(root,"TestOverlay/PersistentTestDisclosure/Disclosure",0,0,680,60);
        NavRect(root,"TestOverlay/PersistentTestDisclosure/OpenSelector",-464,0,135,75);
        NavRect(root,"TestOverlay/PersistentTestDisclosure/Rules",464,0,135,75);
    }
}
