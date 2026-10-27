using UnityEngine;
using UnityEngine.UI;

public static partial class Framework2AppleSkin
{
    const string HostModal="TestOverlay/ModalRoot/";
    static void RefHostFrame(GameObject root,string page,float x,float y,float w,float h)
    {
        string card=HostModal+page+"/Card";
        RefBox(root,card,x,y,w,h);
        RefBox(root,card+"/NavTitleBacking",x+w*.5f-132,y-25,264,59);
        RefBox(root,card+"/Title",x+w*.5f-124,y-23,248,52,73);
        RefBox(root,card+"/NavReferenceLeaf",x+31,y-42,49,49);
        RefBox(root,card+"/NavTestFooter",x+30,y+h-39,w-60,22,34);
        if(root.transform.Find(card+"/Close")!=null)RefBox(root,card+"/Close",x+w-45,y-11,50,50);
    }
    static void AlignHostPages(GameObject root)
    {
        // Overlay canvases exported from a scene serialize a zero local scale;
        // Unity assigns their live screen scale later. Author in canvas units.
        root.transform.Find("TestOverlay").localScale=Vector3.one;
        root.transform.Find("TestOverlay").GetComponent<CanvasScaler>().referenceResolution=new Vector2(1200,2132);
        RefPhone(12,92,405,782);
        RefHostFrame(root,"SelectorPanel",33,296,367,435);
        string p=HostModal+"SelectorPanel/Card/";
        NavImage(root.transform.Find(p+"ChooseA"),"inset",true);
        NavImage(root.transform.Find(p+"ChooseB"),"inset",true);
        RefBox(root,p+"ChooseA",57,345,320,99);
        RefBox(root,p+"ChooseB",57,456,320,99);
        RefBox(root,p+"ChooseA/Label",207,369,162,51,49);
        RefBox(root,p+"ChooseB/Label",207,480,162,51,49);
        NavFormat(root.transform.Find(p+"ChooseA/Label").GetComponent<Text>(),49,Brown);
        NavFormat(root.transform.Find(p+"ChooseB/Label").GetComponent<Text>(),49,Brown);
        NavHide(root,p+"ChooseB/NavSelected");
        foreach(string choice in new[]{"ChooseA","ChooseB"})
        {
            Transform parent=root.transform.Find(p+choice);
            var thumb=NavDecor(parent,"NavGameThumbnail","inset",0,0,100,100);
            RefBox(root,p+choice+"/NavGameThumbnail",113,choice=="ChooseA"?353:464,78,82);
            RefCutout(thumb,11,new Rect(113,353,78,82),new[]{P(122,353),P(182,353),P(191,362),P(191,426),P(182,435),P(122,435),P(113,426),P(113,362)});
            var radio=NavDecor(parent,"NavEmptyRadio","inset",0,0,100,100);
            RefBox(root,p+choice+"/NavEmptyRadio",71,choice=="ChooseA"?377:488,34,34);
            RefEllipse(radio,11,new Rect(71,377,34,34));
            NavDecor(parent,"NavChecked","check",0,0,100,100);
            RefBox(root,p+choice+"/NavChecked",71,choice=="ChooseA"?377:488,34,34);
        }
        root.transform.Find(p+"CurrentVariant").GetComponent<Text>().enabled=false;
        root.transform.Find(p+"SelectionNote").GetComponent<Text>().enabled=false;
        var note=NavNewText(root.transform.Find(p.TrimEnd('/')),"NavSelectionDescription","Separate progress and balances.\nSwitching reloads the saved game.",0,0,100,100,43,Brown);
        RefBox(root,p+note.name,58,571,320,46,43);
        RectTransform confirm=NavNode(root.transform.Find(p.TrimEnd('/')),"NavConfirmSwitch");
        if(confirm.GetComponent<Button>()==null)confirm.gameObject.AddComponent<Button>();
        NavImage(confirm,"button",true);NavButtonTarget(confirm);
        NavNewText(confirm,"Label","Confirm and switch",0,0,890,125,62,Color.white);
        RefBox(root,p+"NavConfirmSwitch",63,626,310,54);
        var selector=root.GetComponent<Framework2AppleVariantSelector>();
        if(selector==null)selector=root.AddComponent<Framework2AppleVariantSelector>();
        selector.Configure(confirm.GetComponent<Button>(),root.transform.Find(p+"ChooseA").GetComponent<Image>(),root.transform.Find(p+"ChooseB").GetComponent<Image>(),root.transform.Find(p+"ChooseA/NavChecked").gameObject,root.transform.Find(p+"ChooseB/NavChecked").gameObject);

        RefPhone(434,92,397,782);RefHostFrame(root,"RulesPanel",454,239,362,546);
        p=HostModal+"RulesPanel/Card/";
        root.transform.Find(p+"RulesBody").GetComponent<Text>().enabled=false;
        string[] titles={"Complete Level 1","Complete Level 2","Complete 3 simulated\nad checks","Reach $200","Reach $3,000"};
        string[] details={"Reach the target to clear Level 1.","Reach the target to clear Level 2.","Finish 3 local ad simulations.","Accumulate $200 in the game.","Accumulate $3,000 in the game."};
        float[] ys={285,338,391,459,512};
        for(int i=0;i<5;i++)
        {
            Transform card=root.transform.Find(p.TrimEnd('/'));
            NavDecor(card,"NavRuleBadge"+i,"secondary",0,0,100,100);
            NavNewText(card,"NavRuleNumber"+i,(i+1).ToString(),0,0,100,100,61,Brown);
            NavNewText(card,"NavRuleTitle"+i,titles[i],0,0,100,100,48,Brown);
            NavNewText(card,"NavRuleDetail"+i,details[i],0,0,100,100,38,Brown);
            RefBox(root,p+"NavRuleBadge"+i,482,ys[i],36,37);
            RefBox(root,p+"NavRuleNumber"+i,482,ys[i],36,37,61);
            RefBox(root,p+"NavRuleTitle"+i,539,ys[i]-1,246,i==2?45:27,48);
            RefBox(root,p+"NavRuleDetail"+i,539,ys[i]+(i==2?42:24),246,23,37);
            root.transform.Find(p+"NavRuleTitle"+i).GetComponent<Text>().alignment=TextAnchor.UpperLeft;
            root.transform.Find(p+"NavRuleDetail"+i).GetComponent<Text>().alignment=TextAnchor.UpperLeft;
        }
        NavDecor(root.transform.Find(p.TrimEnd('/')),"NavRuleWarning","inset",0,0,100,100);
        RefBox(root,p+"NavRuleWarning",476,566,314,81);
        NavNewText(root.transform.Find(p.TrimEnd('/')),"NavRuleWarningText","Ordinary rewards stop at\n$2,999.50. They cannot\ncomplete the $3,000 target.",0,0,100,100,40,Brown);
        RefBox(root,p+"NavRuleWarningText",552,573,229,68,39);
        NavDecor(root.transform.Find(p.TrimEnd('/')),"NavRuleWarningIcon","error",0,0,100,100);
        RefBox(root,p+"NavRuleWarningIcon",491,580,51,51);
        NavNewText(root.transform.Find(p.TrimEnd('/')),"NavRuleLocal","Local simulation. No payment, real ads or revenue.",0,0,100,100,33,Brown);
        RefBox(root,p+"NavRuleLocal",480,653,310,24,33);
        RefBox(root,p+"CloseRules",479,685,310,54);
        RefBox(root,p+"NavTestFooter",480,744,310,22,30);

        RefPhone(846,92,394,782);RefHostFrame(root,"AdPanel",865,309,355,412);
        p=HostModal+"AdPanel/Card/";
        RefBox(root,p+"NavVideo",919,360,249,144);
        RefRoundedIllustration(root.transform.Find(p+"NavVideo").GetComponent<Image>(),11,new Rect(919,360,249,144),18);
        root.transform.Find(p+"AdPlacement").GetComponent<Text>().enabled=false;
        NavNewText(root.transform.Find(p.TrimEnd('/')),"NavLocalAdCaption","No ad is requested.\nNo revenue is generated.",0,0,100,100,47,Brown);
        RefBox(root,p+"NavLocalAdCaption",920,512,245,46,47);
        RefBox(root,p+"CompleteAd",898,567,292,49);
        RefBox(root,p+"CancelAd",899,625,290,43);

        RefPhone(1254,92,405,782);RefHostFrame(root,"ErrorPanel",1276,319,361,368);
        p=HostModal+"ErrorPanel/Card/";
        RefBox(root,p+"NavError",1405,368,103,97);
        RefCutout(root.transform.Find(p+"NavError").GetComponent<Image>(),11,new Rect(1405,368,103,97),new[]{P(1448,371),P(1464,371),P(1505,439),P(1506,452),P(1497,462),P(1415,462),P(1405,453),P(1405,442)});
        RefBox(root,p+"ErrorText",1303,475,310,77,47);
        RefBox(root,p+"ErrorSwitch",1305,575,308,58);
    }
}
