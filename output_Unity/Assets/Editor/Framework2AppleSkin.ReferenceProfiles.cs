using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static partial class Framework2AppleSkin
{
    static void WithdrawBoxes(GameObject root,Rect phone,Rect panel,Rect header,Rect top,Rect money,Rect amount,Rect caption,
        Rect track,Rect middle,Rect history,Rect objective,Rect primary,Rect lucky,Rect locked,Rect tips,Rect footer,Rect close)
    {
        RefPhone(phone.x,phone.y,phone.width,phone.height);
        RefAccountFrame(root,"Panel",panel.x,panel.y,panel.width,panel.height);
        Put(root,"Panel/AppleHeader",header);Put(root,"Panel/AppleHeaderText",new Rect(header.x+8,header.y+5,header.width-16,header.height-10),76);
        Put(root,"Panel/PanelA",top);Put(root,"Panel/PanelA/PanelD",top);
        Put(root,"Panel/PanelA/PanelD/PanelE",new Rect(top.x,top.y,top.width,track.y-top.y));
        Put(root,"Panel/PanelA/PanelD/PanelE/Image",money);Put(root,"Panel/PanelA/PanelD/PanelE/content",amount,106);
        Put(root,"Panel/PanelA/PanelD/PanelE/AccountVerified",caption,43);
        Put(root,"Panel/PanelA/PanelD/Process",track);
        AccProgress(root,"Panel/PanelA/PanelD/Process","Background","Fill","num",track.width*1200/phone.width,track.height*1200/phone.width);
        Put(root,"Panel/PanelB",middle);Put(root,"Panel/PanelB/RecordBtn",history);Put(root,"Panel/PanelB/withdrawProcess",objective,43);
        Put(root,"Panel/PanelB/WithDraw",primary);Put(root,"Panel/PanelC",lucky);Put(root,"Panel/PanelC/AppleLockedIcon",locked);
        Put(root,"Panel/PanelC/Tips",tips,49);Put(root,"Panel/AppleTestFooter",footer,33);Put(root,"Panel/close",close);
    }
    static void Put(GameObject root,string path,Rect r,int font=0)=>RefBox(root,path,r.x,r.y,r.width,r.height,font);
    static Rect R(float x,float y,float w,float h)=>new Rect(x,y,w,h);
    static void AuthorWithdrawProfiles(GameObject root)
    {
        Transform panel=root.transform.Find("Panel");
        NavNewText(panel,"AppleRemaining","",0,0,100,100,43,Brown);
        NavNewText(panel,"AppleOrdinaryCap","Simulation only. Ordinary rewards\ncap at $2,999.50.",0,0,100,100,40,Brown);
        var profiles=new List<Framework2AppleLayoutProfiles.Profile>();
        profiles.Add(WithdrawSnapshot(root,"default",false));
        Framework2ReferenceLayoutAudit.Variant("stage1");
        WithdrawBoxes(root,R(580,58,514,831),R(610,217,454,584),R(650,185,348,74),R(638,274,396,182),R(657,292,96,92),
            R(770,284,255,61),R(772,343,244,62),R(657,405,357,35),R(638,471,396,155),R(657,487,111,42),
            R(783,488,239,40),R(656,539,362,70),R(638,642,396,100),R(662,655,61,71),R(745,661,281,67),R(654,750,355,25),R(1007,209,58,62));
        profiles.Add(WithdrawSnapshot(root,"stage1",false));
        Framework2ReferenceLayoutAudit.Variant("stage2");
        WithdrawBoxes(root,R(1258,101,404,777),R(1288,267,360,490),R(1334,244,255,61),R(1302,316,321,158),R(1322,330,80,84),
            R(1420,322,192,57),R(1422,375,190,43),R(1317,428,289,33),R(1302,483,321,127),R(1318,494,96,35),
            R(1426,497,185,32),R(1317,542,290,57),R(1302,620,321,87),R(1323,632,49,62),R(1393,635,218,64),R(1322,720,289,27),R(1594,262,52,52));
        profiles.Add(WithdrawSnapshot(root,"stage2",false));
        foreach(string key in new[]{"late","unlocked"})
        {
            Framework2ReferenceLayoutAudit.Variant(key);
            float dx=key=="unlocked"?409:0;
            WithdrawBoxes(root,R(845+dx,81,393,793),R(867+dx,249,357,521),R(921+dx,224,242,59),R(889+dx,294,310,217),R(906+dx,310,73,76),
                R(994+dx,302,199,48),R(994+dx,348,196,42),R(899+dx,400,288,32),R(889+dx,511,310,112),R(899+dx,513,91,35),
                R(1004+dx,515,180,32),R(899+dx,555,289,56),R(889+dx,635,310,88),R(909+dx,647,56,66),R(986+dx,649,207,65),R(903+dx,729,284,25),R(1165+dx,242,52,52));
            RefBox(root,"Panel/AppleRemaining",909+dx,434,278,26,43);
            RefBox(root,"Panel/AppleOrdinaryCap",906+dx,471,280,34,40);
            RefBox(root,"Panel/PanelC/AppleWalletIcon",887+dx,625,155,115);
            RefBox(root,"Panel/PanelC/Lucky",1041+dx,625,154,28,43);
            RefBox(root,"Panel/PanelC/CurLuckyCoin",1041+dx,647,154,39,85);
            RefBox(root,"Panel/PanelC/CashOut",1034+dx,688,155,48);
            profiles.Add(WithdrawSnapshot(root,key,true));
        }
        Framework2ReferenceLayoutAudit.Variant(null);
        var layout=root.GetComponent<Framework2AppleLayoutProfiles>();if(layout==null)layout=root.AddComponent<Framework2AppleLayoutProfiles>();
        layout.Configure(profiles.ToArray());layout.Apply("default");
        var presenter=root.GetComponent<Framework2AppleSkinPresenter>();presenter.ConfigureLayout(layout);
        NavLiteral(root.transform.Find("Panel/PanelA/PanelD/PanelE/AccountVerified"),"Local simulation.\nDummy details saved locally.");
        presenter.ConfigureTaskLabels(root.transform.Find("Panel/PanelA/PanelD/PanelE/AccountVerified").GetComponent<Text>(),
            root.transform.Find("Panel/PanelB/withdrawProcess").GetComponent<Text>(),root.transform.Find("Panel/AppleRemaining").GetComponent<Text>());
    }
    static Framework2AppleLayoutProfiles.Profile WithdrawSnapshot(GameObject root,string key,bool late)
    {
        var profile=SnapshotPlain(root,key);
        profile.visibility=new[]{
            new Framework2AppleLayoutProfiles.Visibility {target=root.transform.Find("Panel/AppleRemaining").gameObject,visible=late},
            new Framework2AppleLayoutProfiles.Visibility {target=root.transform.Find("Panel/AppleOrdinaryCap").gameObject,visible=late},
            new Framework2AppleLayoutProfiles.Visibility {target=root.transform.Find("Panel/PanelB/withdrawProcess").gameObject,visible=!late}};
        return profile;
    }
    static Framework2AppleLayoutProfiles.Profile SnapshotPlain(GameObject root,string key)
    {
        var entries=new List<Framework2AppleLayoutProfiles.Entry>();
        foreach(RectTransform rect in root.GetComponentsInChildren<RectTransform>(true))
        {
            if(rect==root.transform)continue;
            Text text=rect.GetComponent<Text>();
            entries.Add(new Framework2AppleLayoutProfiles.Entry {target=rect,position=rect.anchoredPosition,size=rect.sizeDelta,
                anchorMin=rect.anchorMin,anchorMax=rect.anchorMax,pivot=rect.pivot,text=text,
                font=text!=null?text.fontSize:0,minFont=text!=null?text.resizeTextMinSize:0,maxFont=text!=null?text.resizeTextMaxSize:0});
        }
        return new Framework2AppleLayoutProfiles.Profile {key=key,entries=entries.ToArray(),visibility=new Framework2AppleLayoutProfiles.Visibility[0]};
    }
    static Framework2AppleLayoutProfiles.Profile Snapshot(GameObject root,string key,string[] optional,bool region)
    {
        var result=SnapshotPlain(root,key);
        var visibility=new List<Framework2AppleLayoutProfiles.Visibility>();
        foreach(string path in optional)
            visibility.Add(new Framework2AppleLayoutProfiles.Visibility {target=root.transform.Find(path).gameObject,visible=region});
        foreach(string path in new[]{"PanelA/AppleHeader","PanelA/AppleHeaderText","PanelA/AppleReferenceLeaf"})
            visibility.Add(new Framework2AppleLayoutProfiles.Visibility {target=root.transform.Find(path).gameObject,visible=!region});
        visibility.Add(new Framework2AppleLayoutProfiles.Visibility {target=root.transform.Find("PanelA/AppleAccountNotice").gameObject,visible=!region});
        result.visibility=visibility.ToArray();return result;
    }

    static void AuthorReferenceProfiles(GameObject root,string name)
    {
        if(name=="WithDraw"){AuthorWithdrawProfiles(root);return;}
        if(name!="WithDrawmethod")return;
        Transform panel=root.transform.Find("PanelA");
        NavDecor(panel,"AppleRegionalHero","bluebadge",0,0,100,100);
        NavDecor(root.transform.Find("PanelA/AppleRegionalHero"),"Bank","bank",0,0,155,155);
        NavNewText(panel,"AppleRegionalTitle","Test account",0,0,100,100,83,Brown);
        NavNewText(panel,"AppleRegionalDescription","Link a local test account.",0,0,100,100,43,Brown);
        NavDecor(panel,"AppleRegionalNotice","inset",0,0,100,100);
        NavNewText(root.transform.Find("PanelA/AppleRegionalNotice"),"Label","Use a local dummy account only.\nNo real transfer will be made.",0,0,700,150,41,new Color32(0,49,143,255));
        NavDecor(root.transform.Find("PanelA/AppleRegionalNotice"),"Info","bluebadge",-390,0,105,105);
        NavNewText(root.transform.Find("PanelA/AppleRegionalNotice/Info"),"Label","i",0,0,80,100,80,Color.white);
        string[] extras={"PanelA/AppleRegionalHero","PanelA/AppleRegionalTitle","PanelA/AppleRegionalDescription","PanelA/AppleRegionalNotice"};
        var profiles=new List<Framework2AppleLayoutProfiles.Profile>();
        profiles.Add(Snapshot(root,"default",extras,false));
        Framework2ReferenceLayoutAudit.Variant("invalid");
        RefPhone();
        RefBox(root,"PanelA/AppleAccountNotice",56,580,314,22,37);
        RefBox(root,"PanelA/Confirm",56,603,314,58);
        RefBox(root,"PanelE",56,548,314,24);
        RefBox(root,"PanelE/Image",56,548,314,24);
        RefBox(root,"PanelE/Image/content",56,548,314,24,38);
        profiles.Add(Snapshot(root,"invalid",extras,false));
        foreach(string region in new[]{"pix","phone"})
        {
            bool pix=region=="pix";
            Framework2ReferenceLayoutAudit.Variant(region);
            float left=pix?454:842;
            RefPhone(left,146,pix?378:378,722);
            RefBox(root,"PanelA",left+14,315,351,515);
            RefBox(root,"PanelA/PanelC",left+43,578,295,49);
            RefBox(root,"PanelA/close",left+315,291,53,53);
            RefBox(root,"PanelA/AppleRegionalHero",left+148,266,89,84);
            RefBox(root,"PanelA/AppleRegionalHero/Bank",left+168,280,51,54);
            RefBox(root,"PanelA/AppleRegionalTitle",left+46,353,295,38,80);
            RefBox(root,"PanelA/AppleRegionalDescription",left+36,387,312,29,43);
            RefBox(root,"PanelA/ApplePhoneMethod",left+146,425,89,98);
            RefBox(root,"PanelA/ApplePhoneMethod/Handset",left+178,436,28,37);
            RefBox(root,"PanelA/ApplePhoneMethod/Label",left+159,474,62,24,36);
            RefBox(root,"PanelA/ApplePhoneMethod/Selected",left+175,498,29,29);
            string p="PanelA/PanelC/BrazilPixControls";
            RefBox(root,p,left+35,425,311,102);
            RefBox(root,p+"/Title",left+34,387,312,29,43);
            root.transform.Find(p+"/Title").gameObject.SetActive(false);
            string[] names={"CpfCnpj","Phone","Email","Random"};
            for(int i=0;i<4;i++)
            {
                float x=left+35+i*80;
                RefBox(root,p+"/"+names[i],x,425,74,101);
                RefBox(root,p+"/"+names[i]+"/Label",x+2,456,70,42,32);
                RefBox(root,p+"/"+names[i]+"/Selection",x+25,499,24,24);
                var glyph=NavDecor(root.transform.Find(p+"/"+names[i]),"ApplePixGlyph","bank",0,0,100,100);
                Rect[] glyphs={R(512,438,28,32),R(596,438,21,32),R(668,441,40,29),R(748,441,34,29)};
                Rect icon=glyphs[i];
                RefRoundedIllustration(glyph,10,icon,1);
                RefBox(root,p+"/"+names[i]+"/ApplePixGlyph",x+(74-icon.width)*.5f,438,icon.width,icon.height);
            }
            RefBox(root,"PanelA/AppleInputLabel",left+43,544,290,29,54);
            RefBox(root,"PanelA/Panel/ques",left+309,544,29,29);
            RefBox(root,"PanelA/PanelC",left+43,578,295,49);
            foreach(string input in new[]{"Email","Name"})RefBox(root,"PanelA/PanelC/"+input,left+43,578,295,49);
            RefBox(root,"PanelA/PanelC/Email/AppleInputErrorBorder",left+41,576,299,53);
            RefBox(root,"PanelA/PanelC/phoneFront",left+43,578,61,49);
            RefBox(root,"PanelA/PanelC/phoneBehind",left+108,578,230,49);
            RefBox(root,"PanelA/AppleRegionalNotice",left+43,641,295,59);
            RefBox(root,"PanelA/AppleRegionalNotice/Info",left+55,651,35,35);
            RefBox(root,"PanelA/AppleRegionalNotice/Info/Label",left+55,650,35,37,73);
            RefBox(root,"PanelA/AppleRegionalNotice/Label",left+94,646,241,46,39);
            RefBox(root,"PanelA/AppleAccountNotice",left+43,641,295,59,37);
            RefBox(root,"PanelA/Confirm",left+46,713,287,60);
            RefBox(root,"PanelA/AppleTestFooter",left+43,784,295,24,32);
            RefBox(root,"PanelE",left+43,627,295,24);
            RefBox(root,"PanelE/Image",left+43,627,295,24);
            RefBox(root,"PanelE/Image/content",left+43,627,295,24,36);
            // The default single-line notice is replaced by the larger region inset.
            var snapshot=Snapshot(root,region,extras,true);
            var visible=new List<Framework2AppleLayoutProfiles.Visibility>(snapshot.visibility);
            visible.Add(new Framework2AppleLayoutProfiles.Visibility {target=root.transform.Find("PanelA/AppleAccountNotice").gameObject,visible=false});
            snapshot.visibility=visible.ToArray();profiles.Add(snapshot);
        }
        Framework2ReferenceLayoutAudit.Variant(null);
        var layout=root.GetComponent<Framework2AppleLayoutProfiles>();
        if(layout==null)layout=root.AddComponent<Framework2AppleLayoutProfiles>();
        layout.Configure(profiles.ToArray());layout.Apply("default");
        root.GetComponent<Framework2AppleSkinPresenter>().ConfigureLayout(layout);
        root.GetComponent<Framework2AppleSkinPresenter>().ConfigureRegionalDescription(root.transform.Find("PanelA/AppleRegionalDescription").GetComponent<Text>(),
            "Link a Pix account (local simulation).","Link a phone account (local simulation).");
    }
}
