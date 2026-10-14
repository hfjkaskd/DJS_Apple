using System;
using System.Collections.Generic;
using System.IO;
using FruitWithdrawal;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>Reference-board measurements in source pixels, baked into editable prefabs.</summary>
public static class WithdrawalReferenceUIAuthoring
{
    private const string Art = "Assets/WithdrawalReferenceArt/";
    private const string Pages = "Assets/Resources/WithdrawalImported/Panels/";
    private static readonly Color Ink = new Color32(83, 20, 7, 255);
    private static readonly Color Forest = new Color32(4, 67, 15, 255);
    private static readonly Color Ruby = new Color32(128, 22, 24, 255);
    private static readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
    private static readonly List<WithdrawalTextBinding> texts = new List<WithdrawalTextBinding>();
    private static readonly List<WithdrawalObjectBinding> objects = new List<WithdrawalObjectBinding>();
    private static readonly List<WithdrawalButtonBinding> buttons = new List<WithdrawalButtonBinding>();
    private static readonly List<WithdrawalProgressBinding> progress = new List<WithdrawalProgressBinding>();
    private static TMP_FontAsset body, display;
    private static Material whiteOutline;
    private static Rect screen;
    private static float scale;
    private static GameObject root;
    private static RectTransform content;
    private static WithdrawalPageView view;

    public static void BuildAndRender() { Build(); WithdrawalUIRenderValidation.Run(); }

    [MenuItem("Tools/Withdrawal/Build Measured Reference Interfaces")]
    public static void Build()
    {
        AssetDatabase.Refresh();
        sprites.Clear();
        // Rectangles below trim only the Unity Sprite region. Source PNG pixels are unchanged.
        Skin("PanelClean", R(83,69,943,1446), new Vector4(135,135,135,135));
        Skin("Title", R(178,144,1984,527), new Vector4(210,0,210,0));
        Skin("PrimaryButton", R(223,194,1950,529), new Vector4(180,160,180,160));
        Skin("SecondaryButton", R(241,385,1295,639), new Vector4(130,122,130,122));
        Skin("InsetCard", R(414,277,1245,673), new Vector4(98,98,98,98));
        Skin("SelectedCard", R(86,134,1521,846), new Vector4(126,126,126,126));
        Skin("CashBundle", R(149,116,1364,973));
        Skin("Gem", R(163,140,1254,981));
        Skin("CloseButton", R(69,67,1184,1169));
        Skin("CheckBadge", R(65,66,1189,1189));
        Skin("Wallet", R(173,92,1216,1098));
        foreach (string name in new[] { "Trophy", "Calendar", "Crown", "Clock", "GoldenSparkle", "ReturnArrow", "RewardBurst" }) SkinByAlpha(name);
        sprites["Bank"] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WithdrawalFruitArt/Bank.png");
        sprites["Hourglass"] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WithdrawalFruitArt/Hourglass.png");
        sprites["Star"] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/res/local/pops/rate/sprite/Star.asset");
        sprites["StepGreen"] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/res/local/common/sprite/btn/MidBtn_Green.asset");
        sprites["StepGray"] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/res/local/common/sprite/btn/MidBtn_Gray.asset");
        body = Font("BarlowCondensed-SemiBold", "ReferenceBody");
        display = Font("LilitaOne-Regular", "ReferenceDisplay");
        whiteOutline = AssetDatabase.LoadAssetAtPath<Material>(Art + "ReferenceWhiteOutline.mat");
        if (whiteOutline == null) { whiteOutline = new Material(display.material); AssetDatabase.CreateAsset(whiteOutline, Art + "ReferenceWhiteOutline.mat"); }
        whiteOutline.SetColor("_OutlineColor", new Color32(87,35,12,255)); whiteOutline.SetFloat("_OutlineWidth", .20f);
        whiteOutline.EnableKeyword("OUTLINE_ON");
        whiteOutline.SetFloat("_FaceDilate", .10f); EditorUtility.SetDirty(whiteOutline);
        Cash(); Diamonds(); Reward(); Channel(); Confirm(); Rules(); Conditions(); Review(); Completed(); Returned(); Records();
        Hud(); AssetDatabase.SaveAssets();
        Debug.Log("[WithdrawalReferenceUI] Authored all eleven pages from measured reference rectangles.");
    }

    private static Sprite Skin(string name, Rect topLeft, Vector4 border = default)
    {
        string path = Art + name + ".png";
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) throw new InvalidOperationException("Missing reference art: " + path);
        importer.GetSourceTextureWidthAndHeight(out int width, out int height);
        importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.isReadable = false;
        importer.maxTextureSize = name=="PanelClean" ? 2048
            : name=="GoldenSparkle" ? 128
            : name=="Trophy" || name=="Calendar" || name=="Crown" || name=="Clock" || name=="CloseButton" || name=="CheckBadge" ? 256
            : name=="CashBundle" || name=="Gem" || name=="Wallet" || name=="ReturnArrow" ? 512 : 1024;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.spritesheet = new[] { new SpriteMetaData { name = name, rect = new Rect(topLeft.x, height - topLeft.yMax, topLeft.width, topLeft.height), pivot = new Vector2(.5f,.5f), alignment = 9, border = border } };
        importer.SaveAndReimport();
        foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(path)) if (asset is Sprite sprite) { sprites[name] = sprite; return sprite; }
        throw new InvalidOperationException("Sprite import failed: " + name);
    }

    private static void SkinByAlpha(string name)
    {
        string path = Art + name + ".png";
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) throw new InvalidOperationException("Missing reference icon: " + path);
        importer.isReadable = true; importer.maxTextureSize = 2048; importer.SaveAndReimport();
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        Color32[] pixels = texture.GetPixels32();
        int minX = texture.width, minY = texture.height, maxX = 0, maxY = 0;
        for (int y = 0; y < texture.height; y++) for (int x = 0; x < texture.width; x++) if (pixels[y * texture.width + x].a > 16) {
            minX = Math.Min(minX,x); minY = Math.Min(minY,y); maxX = Math.Max(maxX,x); maxY = Math.Max(maxY,y);
        }
        importer.GetSourceTextureWidthAndHeight(out int sourceW, out int sourceH);
        float sx = (float)sourceW / texture.width, sy = (float)sourceH / texture.height;
        Skin(name, R(Math.Max(0,minX-3)*sx, Math.Max(0,texture.height-maxY-4)*sy, Math.Min(texture.width,maxX+4)*sx, Math.Min(texture.height,texture.height-minY+3)*sy));
    }

    private static TMP_FontAsset Font(string file, string name)
    {
        string path = Art + "Fonts/" + name + ".asset";
        TMP_FontAsset asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
        if (asset != null) return asset;
        var source = AssetDatabase.LoadAssetAtPath<UnityEngine.Font>(Art + "Fonts/" + file + ".ttf");
        asset = TMP_FontAsset.CreateFontAsset(source, 80, 10, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, false);
        asset.name = name;
        string alphabet = ""; for (int i = 32; i <= 126; i++) alphabet += (char)i;
        asset.TryAddCharacters(alphabet + "\u2022\u00d7", out string missing);
        if (!string.IsNullOrEmpty(missing)) Debug.LogWarning("[WithdrawalReferenceUI] Font optional glyphs unavailable: " + missing);
        asset.atlasPopulationMode = AtlasPopulationMode.Static;
        AssetDatabase.CreateAsset(asset, path);
        foreach (Texture2D atlas in asset.atlasTextures) AssetDatabase.AddObjectToAsset(atlas, asset);
        AssetDatabase.AddObjectToAsset(asset.material, asset);
        EditorUtility.SetDirty(asset); return asset;
    }

    private static Rect R(float l, float t, float r, float b) { return new Rect(l,t,r-l,b-t); }
    private static Rect Local(float l, float t, float r, float b) { return R(screen.x+l,screen.y+t,screen.x+r,screen.y+b); }
    private static Rect Pad(Rect r, float p) { return new Rect(r.x+p,r.y+p,r.width-p*2,r.height-p*2); }

    private static void Begin(string name, string title, Rect frame, Rect panel, Rect header, Rect close)
    {
        screen = frame; scale = 1080f / screen.width;
        texts.Clear(); objects.Clear(); buttons.Clear(); progress.Clear();
        root = new GameObject(name, typeof(RectTransform), typeof(WithdrawalPageView));
        var rt = (RectTransform)root.transform; rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero;
        view = root.GetComponent<WithdrawalPageView>();
        var shadeGo = new GameObject("ModalShade",typeof(RectTransform),typeof(Image),typeof(Orange.ProtectedAreaAdaptReverse));
        shadeGo.transform.SetParent(root.transform,false);
        var shade = shadeGo.GetComponent<Image>(); shade.color = new Color(.07f,.10f,.16f,.46f);
        var shadeRect = shade.rectTransform; shadeRect.anchorMin=Vector2.zero; shadeRect.anchorMax=Vector2.one; shadeRect.sizeDelta=Vector2.zero;
        var mainGo = new GameObject("Content",typeof(RectTransform)); mainGo.transform.SetParent(root.transform,false); content=(RectTransform)mainGo.transform;
        content.sizeDelta = new Vector2(1080, screen.height * scale);
        view.Content=content; view.DesignSize=content.sizeDelta; view.DesignCenter=Vector2.zero; view.UseCanvasFrame=true;
        float top=Math.Min(header.y,close.y), bottom=panel.yMax;
        float left=Math.Min(panel.x,Math.Min(header.x,close.x)), right=Math.Max(panel.xMax,Math.Max(header.xMax,close.xMax));
        view.ArtworkBounds = new Rect((left-screen.center.x)*scale,(screen.center.y-bottom)*scale,(right-left)*scale,(bottom-top)*scale);
        ArtImage("Panel",panel,"PanelClean",sliced:true, pixelsPerUnit: 1.3f);
        ArtImage("TitlePlate",header,"Title");
        var titleText=Text("Title",Pad(header,10),title,32,headline:true); titleText.color=Color.white; titleText.fontSharedMaterial=whiteOutline; Condense(titleText,.94f);
        Button("Close",close,null,WithdrawalAction.Close,"CloseButton");
        Text("test",R(panel.x+22,header.yMax+1,panel.xMax-22,header.yMax+22),"TEST MODE \u2022 NO CASH VALUE",12.5f,key:"test");
        Text("notice",R(panel.x+16,panel.yMax-20,panel.xMax-16,panel.yMax-4),"",11,key:"notice");
    }

    private static void End()
    {
        view.Texts=texts.ToArray(); view.Objects=objects.ToArray(); view.Buttons=buttons.ToArray(); view.Progress=progress.ToArray();
        PrefabUtility.SaveAsPrefabAsset(root,Pages+root.name+".prefab"); Object.DestroyImmediate(root);
    }

    private static RectTransform Node(string name, Rect bounds, Transform parent=null)
    {
        var go=new GameObject(name,typeof(RectTransform)); go.layer=5;
        var rt=(RectTransform)go.transform; rt.SetParent(content,false); rt.anchorMin=rt.anchorMax=new Vector2(.5f,.5f);
        rt.anchoredPosition=new Vector2((bounds.center.x-screen.center.x)*scale,(screen.center.y-bounds.center.y)*scale);
        rt.sizeDelta=bounds.size*scale;
        if(parent!=null && parent!=content) rt.SetParent(parent,true);
        return rt;
    }

    private static Image ArtImage(string name, Rect bounds, string art, Transform parent=null, bool sliced=false, float pixelsPerUnit=1)
    {
        var image=Node(name,bounds,parent).gameObject.AddComponent<Image>(); image.sprite=sprites[art]; image.raycastTarget=false;
        image.type=sliced?Image.Type.Sliced:Image.Type.Simple; image.preserveAspect=!sliced && art!="Title";
        image.pixelsPerUnitMultiplier=pixelsPerUnit; return image;
    }
    private static Image Card(string name,Rect bounds,Transform parent=null,bool selected=false)
    { return ArtImage(name,bounds,selected?"SelectedCard":"InsetCard",parent,true,selected?2.7f:1.5f); }
    private static TMP_Text Text(string name,Rect bounds,string value,float size,bool headline=false,string key=null,Transform parent=null,TextAlignmentOptions align=TextAlignmentOptions.Center)
    {
        var t=Node(name,bounds,parent).gameObject.AddComponent<TextMeshProUGUI>(); t.font=headline?display:body;
        t.text=value; t.fontSize=t.fontSizeMax=size*scale; t.fontSizeMin=size*scale*.8f; t.enableAutoSizing=true;
        t.alignment=align; t.color=Ink; t.enableWordWrapping=true; t.raycastTarget=false;
        if(headline) Condense(t,.9f);
        if(key!=null) texts.Add(new WithdrawalTextBinding{Key=key,Text=t}); return t;
    }
    private static void Condense(TMP_Text text,float widthScale)
    {
        RectTransform rect=text.rectTransform;
        float previousScale=rect.localScale.x;
        rect.sizeDelta=new Vector2(rect.sizeDelta.x*previousScale/widthScale,rect.sizeDelta.y);
        rect.localScale=new Vector3(widthScale,1,1);
    }
    private static void Obj(GameObject obj,string key) { objects.Add(new WithdrawalObjectBinding{Key=key,Object=obj}); }
    private static Button Button(string name,Rect bounds,string label,WithdrawalAction action,string skin="SecondaryButton",int index=0,string key=null)
    {
        var image=ArtImage(name,bounds,skin,sliced:skin!="CloseButton",pixelsPerUnit:skin=="PrimaryButton"?1f:1.45f);
        // These measured fixed-aspect faces retain their complete bevel and glossy capsule silhouette.
        if(skin=="PrimaryButton") { image.type=Image.Type.Simple; image.preserveAspect=false; }
        image.raycastTarget=true; var button=image.gameObject.AddComponent<Button>(); button.targetGraphic=image;
        button.navigation=new Navigation{mode=Navigation.Mode.None}; var colors=button.colors;
        colors.pressedColor=new Color(.88f,.91f,.84f); colors.disabledColor=new Color(.7f,.72f,.64f,.85f); button.colors=colors;
        if(label!=null) {
            bool primary=skin=="PrimaryButton"; var t=Text("Label",Pad(bounds,4),label,primary?25:20,primary,key,image.transform);
            if(primary){ t.color=Color.white; t.fontSharedMaterial=whiteOutline; Condense(t,.84f); }
        }
        buttons.Add(new WithdrawalButtonBinding{Button=button,Action=action,Index=index}); return button;
    }
    private static void Hero(Rect icon, Rect amount,string key="amount",float size=58)
    { ArtImage("CashIcon",icon,"CashBundle"); Text(key,amount,"$500.00",size,true,key).color=Forest; }
    private static Image Bar(string name,Rect bounds,string key=null)
    {
        var bg=Card(name,bounds); bg.color=new Color32(217,205,180,255);
        var track=Node("FillTrack",Pad(bounds,1.5f),bg.transform);
        var fill=ArtImage("Fill",Pad(bounds,1.5f),"PrimaryButton",track,true,6);
        fill.rectTransform.anchorMin=Vector2.zero; fill.rectTransform.anchorMax=Vector2.one;
        fill.rectTransform.offsetMin=fill.rectTransform.offsetMax=Vector2.zero;
        if(key!=null) progress.Add(new WithdrawalProgressBinding{Key=key,Fill=fill}); return fill;
    }

    private static void Tabs(Rect cash,Rect diamonds,bool selectedDiamond)
    {
        var cashTab=Button("CashTab",cash,"Cash",WithdrawalAction.Cash,selectedDiamond?"SecondaryButton":"PrimaryButton");
        var diamondTab=Button("DiamondTab",diamonds,"Diamonds",WithdrawalAction.Diamonds,selectedDiamond?"PrimaryButton":"SecondaryButton");
        foreach(var tab in new[]{cashTab,diamondTab}) { var label=tab.GetComponentInChildren<TMP_Text>(); label.fontSize=label.fontSizeMax=20*scale; label.fontSizeMin=18*scale; }
    }
    private static void WalletFooter(float x,float y)
    {
        Button("Rules",R(x,y,x+94,y+42),"Rules",WithdrawalAction.Rules);
        Button("Rewards",R(x+103,y,x+201,y+42),"Rewards",WithdrawalAction.Rewards);
        Button("Records",R(x+211,y,x+311,y+42),"Records",WithdrawalAction.Records);
    }
    private static void Tier(Rect bounds,int i,bool diamond)
    {
        var button=Button("Tier"+i,bounds,null,WithdrawalAction.SelectTier,"InsetCard",i);
        button.GetComponent<Image>().pixelsPerUnitMultiplier=1.5f; Obj(button.gameObject,"tier"+i);
        var selected=Card("Selected",bounds,button.transform,true); Obj(selected.gameObject,"selected"+i); selected.gameObject.SetActive(i==0);
        float amountY=diamond?bounds.y+10:bounds.y+15;
        Rect amountRect=R(bounds.x+6,amountY,bounds.xMax-6,amountY+30);
        Text("Amount",amountRect,"$500",diamond?25:26,true,"tier"+i,button.transform).color=Ink;
        var activeAmount=Text("SelectedAmount",amountRect,"$500",diamond?25:26,true,"tier"+i,button.transform); activeAmount.color=Forest; Obj(activeAmount.gameObject,"selectedAmount"+i); activeAmount.gameObject.SetActive(i==0);
        if(diamond) {
            ArtImage("Gem",R(bounds.x+26,bounds.y+45,bounds.x+59,bounds.y+74),"Gem",button.transform);
            Text("Cost",R(bounds.x+58,bounds.y+44,bounds.xMax-12,bounds.y+77),"50,000",24,true,"tierCost"+i,button.transform).color=Ruby;
        } else ArtImage("Cash",R(bounds.x+30,bounds.y+50,bounds.xMax-26,bounds.yMax-9),"CashBundle",button.transform);
        ArtImage("Check",R(bounds.xMax-21,bounds.y-7,bounds.xMax+10,bounds.y+24),"CheckBadge",selected.transform);
    }
    private static void Cash()
    {
        Begin("TXPanel","CASH WALLET",R(844,9,1244,843),R(858,158,1230,761),R(908,124,1183,187),R(1183,138,1233,188));
        Tabs(R(891,217,1043,261),R(1048,217,1201,261),false);
        Hero(R(901,285,985,360),R(995,280,1193,340),"balance",59);
        Text("Caption",R(997,338,1190,363),"Available balance",18.5f).color=Forest;
        Card("Frozen",R(896,372,1193,408)); Text("FrozenLabel",R(903,374,1188,405),"In application: $0.00",16.5f,key:"frozen");
        Tier(R(882,424,985,522),0,false); Tier(R(991,424,1095,522),1,false); Tier(R(1099,424,1209,522),2,false);
        view.ProgressFill=Bar("BalanceProgress",R(887,534,1201,559));
        Text("Target",R(900,561,1190,587),"Balance target reached",20,true,"target").color=Forest;
        Text("RulesHint",R(938,591,1185,620),"Review all conditions before applying",16,key:"summary");
        Text("Info",R(918,592,937,616),"!",20,true);
        Button("Apply",R(889,630,1201,686),"CHOOSE CHANNEL",WithdrawalAction.Apply,"PrimaryButton",key:"apply");
        WalletFooter(889,694); End();
    }
    private static void Diamonds()
    {
        Begin("TXCoinPanel","DIAMOND WALLET",R(1259,9,1659,843),R(1272,158,1646,761),R(1323,123,1598,187),R(1598,138,1648,188));
        Tabs(R(1303,217,1457,262),R(1463,217,1618,262),true);
        ArtImage("GemHero",R(1342,285,1415,348),"Gem"); var balance=Text("Balance",R(1420,283,1604,343),"50,600",59,true,"balance"); balance.color=Ruby; Condense(balance,.85f);
        Text("Rate",R(1320,344,1601,375),"100 diamonds = 1 test dollar",18.5f);
        Tier(R(1298,384,1455,476),0,true); Tier(R(1464,384,1621,476),1,true);
        Tier(R(1298,484,1455,575),2,true); Tier(R(1464,484,1621,575),3,true);
        Card("ConditionsSummary",R(1304,586,1614,622)); Text("Summary",R(1311,588,1607,620),"5 qualified days - Rank 31 - Daily 10 wins",16,key:"summary");
        Button("Apply",R(1304,630,1614,686),"CHOOSE CHANNEL",WithdrawalAction.Apply,"PrimaryButton",key:"apply");
        WalletFooter(1303,694); End();
    }
    private static void Reward()
    {
        Begin("RewardGetPanel","HARVEST REWARD",R(428,9,829,843),R(447,195,811,730),R(493,163,765,224),R(765,164,816,215));
        ArtImage("RewardBurst",R(493,257,770,405),"RewardBurst");
        ArtImage("CashBack",R(555,272,656,338),"CashBundle");
        ArtImage("CashFront",R(506,307,670,392),"CashBundle"); ArtImage("Gem",R(670,313,750,389),"Gem");
        Sparkles(new[]{new Vector2(518,294),new Vector2(665,277),new Vector2(749,293),new Vector2(752,395),new Vector2(514,397)});
        Text("RewardCash",R(515,386,748,443),"$1.30",59,true,"rewardCash").color=Forest;
        Text("RewardDiamonds",R(493,438,767,479),"+1,300 diamonds",34,true,"rewardDiamonds").color=Ruby;
        Text("Source",R(497,477,763,505),"Fruit matches + level clear",18,key:"rewardDetail");
        Card("AfterClaim",R(474,511,783,584));
        Text("CashCaption",R(486,518,625,544),"Cash after claim:",14);
        Text("DiamondCaption",R(635,518,774,544),"Diamonds after claim:",14);
        Text("AfterCash",R(489,540,625,574),"$500.00",25,true,"afterCash").color=Forest;
        ArtImage("AfterGem",R(649,544,675,568),"Gem"); Text("AfterDiamonds",R(676,540,774,574),"50,600",23,true,"afterDiamonds").color=Ruby;
        Button("Claim",R(480,596,778,651),"CLAIM REWARDS",WithdrawalAction.Claim,"PrimaryButton");
        Button("Back",R(493,660,765,704),"Back to game",WithdrawalAction.Close); End();
    }
    private static void Channel()
    {
        Begin("TXChannelPanel","CHOOSE CHANNEL",R(12,9,413,844),R(27,181,400,735),R(77,143,351,211),R(353,156,403,207));
        Card("AmountCard",R(56,246,372,348)); Hero(R(79,260,177,335),R(188,264,359,328));
        for(int i=0;i<2;i++) {
            Rect r=i==0?R(55,364,374,468):R(55,480,371,576);
            var b=Button("Channel"+i,r,null,WithdrawalAction.SelectTier,"InsetCard",i); b.GetComponent<Image>().pixelsPerUnitMultiplier=1.5f;
            var selected=Card("Selected",r,b.transform,true); Obj(selected.gameObject,"channelSelected"+i);
            ArtImage("Icon",R(r.x+18,r.y+15,r.x+94,r.yMax-12),i==0?"Wallet":"Bank",b.transform);
            Text("Name",R(r.x+107,r.y+15,r.xMax-23,r.y+54),i==0?"Test wallet":"Test bank",28,true,"channel"+i,b.transform,TextAlignmentOptions.MidlineLeft);
            Text("Hint",R(r.x+108,r.y+54,r.xMax-15,r.yMax-11),"No account details required",18,parent:b.transform,align:TextAlignmentOptions.MidlineLeft);
            ArtImage("Check",R(r.xMax-33,r.y-5,r.xMax+1,r.y+29),"CheckBadge",selected.transform);
        }
        Button("Continue",R(58,594,370,650),"CONTINUE",WithdrawalAction.Confirm,"PrimaryButton");
        Button("Back",R(71,659,356,705),"Back to wallet",WithdrawalAction.Back); End();
    }
    private static void Confirm()
    {
        Begin("TXUserInfoSurePanel","CONFIRM REQUEST",R(428,9,829,844),R(441,178,815,735),R(490,142,767,209),R(768,155,817,206));
        Card("Amount",R(468,241,789,332)); Hero(R(495,251,585,328),R(594,254,777,318));
        Card("Details",R(468,343,789,470));
        Pair("Channel:","channel",R(484,351,776,386),"Test wallet"); Pair("Reserve amount:","reserve",R(484,392,776,427),"$500.00"); Pair("Available after submit:","afterBalance",R(484,432,776,465),"$0.00");
        Card("Hint",R(468,480,789,528)); Text("HintText",R(493,486,777,524),"Your test balance will be reserved.",18,key:"hint");
        Button("Rules",R(475,535,784,579),"View all conditions",WithdrawalAction.Rules);
        Button("Submit",R(473,592,784,649),"SUBMIT TEST",WithdrawalAction.Confirm,"PrimaryButton");
        Button("Back",R(487,658,770,704),"Back",WithdrawalAction.Back); End();
    }
    private static void Pair(string label,string key,Rect r,string sample)
    {
        Text(key+"Label",R(r.x,r.y,r.x+r.width*.65f,r.yMax),label,19,align:TextAlignmentOptions.MidlineLeft);
        Text(key,R(r.x+r.width*.53f,r.y,r.xMax,r.yMax),sample,22,key:key,align:TextAlignmentOptions.MidlineRight);
    }
    private static void Rules()
    {
        Begin("RulesPanel","WITHDRAWAL RULES",R(843,9,1244,844),R(856,158,1230,739),R(906,121,1183,188),R(1183,138,1233,188));
        Card("Amount",R(881,214,1206,295)); Hero(R(908,222,997,288),R(1010,222,1194,288));
        string[] icons={"CashBundle","Star","Trophy","Calendar","Crown","Clock"};
        float[] top={305,347,389,432,475,518},bottom={345,387,430,473,515,562};
        for(int i=0;i<6;i++) {
            Rect r=R(882,top[i],1206,bottom[i]); var card=Card("Rule"+i,r); Obj(card.gameObject,"ruleSummaryRow"+i);
            ArtImage("Icon",R(889,top[i]+4,923,bottom[i]-3),icons[i],card.transform);
            Text("RuleSummary",R(934,top[i]+3,1196,bottom[i]-3),"Published condition",20,key:"ruleSummary"+i,parent:card.transform,align:TextAlignmentOptions.MidlineLeft);
        }
        Card("Footnotes",R(882,566,1206,648)); Text("Notes",R(902,570,1194,643),"Each qualified day needs 10 wins.\nDays do not need to be consecutive.\nCancel or reject: reserved units return.",18,key:"ruleFootnote",align:TextAlignmentOptions.MidlineLeft);
        Button("Back",R(885,657,1206,714),"BACK TO WALLET",WithdrawalAction.Back,"PrimaryButton"); End();
    }
    private static void Steps(float x,float y,float gap,int active,bool allDone=false)
    {
        var track=Card("JourneyLine",R(x,y-2,x+gap*2,y+3)); track.color=new Color32(176,176,168,255);
        string[] labels={"Conditions","Review","Result"};
        for(int i=0;i<3;i++) {
            Rect icon=R(x+i*gap-19,y-19,x+i*gap+19,y+19);
            if(i<active||allDone) ArtImage("Done"+i,icon,"CheckBadge");
            else {
                var image=ArtImage("Phase"+i,icon,i==active && i!=1?"StepGreen":"StepGray");
                if(i==active&&i==1) image.color=new Color32(255,160,42,255);
                var number=Text("Number"+i,icon,(i+1).ToString(),27,true); number.color=Color.white; number.fontSharedMaterial=whiteOutline;
            }
            Text("StepLabel"+i,R(x+i*gap-50,y+20,x+i*gap+50,y+45),labels[i],19).color=i==active||allDone?Forest:new Color32(111,106,102,255);
        }
    }
    private static void Conditions()
    {
        Begin("TXProgress0Panel","REQUEST PROGRESS",R(1258,9,1659,844),R(1272,157,1646,740),R(1321,120,1598,186),R(1598,135,1649,186));
        Card("Amount",R(1297,214,1620,289)); Hero(R(1316,222,1390,282),R(1401,226,1527,272),size:39);
        Text("ReservedLabel",R(1526,227,1610,273),"reserved",24);
        Steps(1351,317,111,0);
        var levels=Card("Levels",R(1297,368,1621,411)); Obj(levels.gameObject,"levelsRow");
        Text("Milestones",R(1308,372,1505,407),"Level milestones 2 / 22 / 51",19,key:"milestones",parent:levels.transform,align:TextAlignmentOptions.MidlineLeft);
        for(int i=0;i<3;i++) {
            Rect r=R(1508+i*36,375,1537+i*36,404);
            Obj(ArtImage("Pending"+i,r,"StepGray",levels.transform).gameObject,"levelPending"+i);
            Obj(ArtImage("Done"+i,r,"CheckBadge",levels.transform).gameObject,"levelDone"+i);
        }
        TaskRow("daily",R(1297,413,1621,474),"Today: 8 / 10 wins"); TaskRow("days",R(1297,476,1621,535),"Qualified days: 4 / 5");
        var rank=Card("Rank",R(1297,536,1621,576)); Obj(rank.gameObject,"rankRow");
        Text("Rank",R(1309,538,1575,574),"Reward rank: 49 / 31",20,key:"rank",parent:rank.transform,align:TextAlignmentOptions.MidlineLeft);
        Obj(ArtImage("RankDone",R(1580,542,1609,571),"CheckBadge",rank.transform).gameObject,"rankDone");
        Card("Hint",R(1297,578,1621,612)); Text("Hint",R(1329,580,1613,611),"Previous progress counts toward this request.",17);
        Button("Play",R(1296,616,1622,668),"KEEP PLAYING",WithdrawalAction.Close,"PrimaryButton");
        Button("Rules",R(1303,673,1455,716),"Rules",WithdrawalAction.Rules);
        Button("Cancel",R(1464,673,1617,716),"Cancel request",WithdrawalAction.Cancel); End();
    }
    private static void TaskRow(string key,Rect r,string sample)
    {
        var card=Card(key+"Card",r); Obj(card.gameObject,key+"Row");
        Text(key,R(r.x+11,r.y+3,r.xMax-12,r.y+30),sample,20,key:key,parent:card.transform,align:TextAlignmentOptions.MidlineLeft);
        Image fill=Bar(key+"Progress",R(r.x+11,r.y+33,r.xMax-51,r.yMax-9),key); fill.transform.parent.parent.SetParent(card.transform,true);
        Text(key+"Percent",R(r.xMax-47,r.y+27,r.xMax-9,r.yMax-6),"80%",19,key:key+"Percent",parent:card.transform);
    }
    private static void Review()
    {
        Begin("TXProgress1Panel","SIMULATED REVIEW",R(12,8,413,856),R(26,163,402,764),R(75,128,354,196),R(354,143,405,194));
        Card("Amount",R(56,224,372,319)); Hero(R(81,232,176,306),R(184,238,359,303)); Steps(110,349,103.5f,1);
        Card("Timer",R(56,399,372,563)); ArtImage("Hourglass",R(147,404,276,515),"Hourglass");
        Text("Time",R(126,509,301,562),"00:42",54,true,"countdown");
        Text("Hint",R(65,563,364,613),"All conditions completed.\nReview completes automatically.",21,key:"reviewHint");
        Button("Game",R(56,620,373,676),"BACK TO GAME",WithdrawalAction.Close,"PrimaryButton");
        Button("Records",R(61,681,205,724),"Records",WithdrawalAction.Records);
        Button("Cancel",R(212,681,369,724),"Cancel request",WithdrawalAction.Cancel);
        Text("NoAccount",R(82,729,345,748),"No payment account connected.",16); End();
    }
    private static void Completed()
    {
        Begin("TXProgress2Panel","TEST COMPLETE",R(427,8,828,856),R(441,163,816,749),R(491,128,767,196),R(770,143,820,194));
        ArtImage("Cash",R(558,233,692,336),"CashBundle"); ArtImage("Check",R(631,274,709,343),"CheckBadge");
        Sparkles(new[]{new Vector2(510,241),new Vector2(550,252),new Vector2(753,242),new Vector2(750,342),new Vector2(498,361),new Vector2(507,286),new Vector2(722,287)});
        Text("Amount",R(503,339,752,401),"$500.00",60,true,"amount").color=Forest;
        Card("Result",R(471,411,786,494)); Text("ResultTitle",R(482,422,775,453),"Simulation completed",28,true,"resultTitle");
        Text("ResultDetail",R(482,454,775,483),"No real money was sent.",20,key:"resultDetail");
        Text("Saved",R(488,495,773,525),"Result saved in Records",19); Steps(524,549,104.5f,2,true);
        Button("Records",R(473,613,785,671),"VIEW RECORDS",WithdrawalAction.Records,"PrimaryButton");
        Button("Back",R(480,675,778,722),"Back to wallet",WithdrawalAction.Back); End();
    }
    private static void Returned()
    {
        Begin("TXProgress3Panel","UNITS RETURNED",R(843,8,1244,856),R(857,163,1232,749),R(906,128,1183,196),R(1185,142,1235,193));
        Card("AmountCard",R(886,224,1203,397)); ArtImage("Cash",R(970,238,1103,335),"CashBundle");
        // A standard icon image has no click handler; the actual navigation stays on Buttons.
        ArtImage("ReturnMark",R(1061,264,1140,331),"ReturnArrow");
        Text("Amount",R(919,332,1170,392),"$500.00",58,true,"amount").color=Forest;
        Card("Result",R(887,402,1204,482)); Text("Title",R(898,411,1193,445),"Test request closed",27,true,"resultTitle");
        Text("Detail",R(898,445,1193,474),"Reserved units returned to wallet.",19,key:"resultDetail");
        Card("Balance",R(887,491,1204,540)); Pair("Available balance:","afterBalance",R(900,496,1192,535),"$500.00");
        Card("Hint",R(887,547,1204,604)); Text("Hint",R(925,552,1189,599),"You can try another request\nat any time.",20,align:TextAlignmentOptions.MidlineLeft);
        Button("TryAgain",R(887,611,1204,670),"TRY ANOTHER REQUEST",WithdrawalAction.Back,"PrimaryButton");
        Button("Records",R(895,674,1195,721),"Records",WithdrawalAction.Records); End();
    }
    private static void Records()
    {
        Begin("TXRecordPanel","REQUEST RECORDS",R(1258,8,1659,856),R(1272,163,1648,746),R(1322,127,1600,196),R(1600,142,1651,193));
        Card("HeaderCard",R(1298,225,1621,313)); ArtImage("Cash",R(1316,233,1393,302),"CashBundle");
        Text("Header",R(1404,237,1611,271),"Test applications",26,true);
        Text("Empty",R(1404,269,1611,300),"Your recent test requests.",18,key:"empty");
        Rect viewportBounds=R(1296,319,1624,630); var viewport=Node("Viewport",viewportBounds);
        var maskImage=viewport.gameObject.AddComponent<Image>(); maskImage.color=new Color(1,1,1,.001f); viewport.gameObject.AddComponent<RectMask2D>();
        var list=Node("RecordsContent",R(1296,319,1624,953),viewport);
        list.anchorMin=list.anchorMax=new Vector2(.5f,1); list.pivot=new Vector2(.5f,1); list.anchoredPosition=Vector2.zero;
        var scroll=viewport.gameObject.AddComponent<ScrollRect>(); scroll.viewport=viewport; scroll.content=list; scroll.horizontal=false; scroll.movementType=ScrollRect.MovementType.Clamped; scroll.scrollSensitivity=65;
        for(int i=0;i<8;i++) {
            Rect r=R(1298,321+i*79,1621,389+i*79); var b=Button("Record"+i,r,null,WithdrawalAction.SelectRecord,"InsetCard",i); b.GetComponent<Image>().pixelsPerUnitMultiplier=1.5f;
            b.transform.SetParent(list,true); Obj(b.gameObject,"record"+i);
            Text("Date",R(r.x+12,r.y+5,r.x+94,r.y+27),"Oct 09",17,key:"recordTime"+i,parent:b.transform,align:TextAlignmentOptions.MidlineLeft);
            var amount=Text("Amount",R(r.x+11,r.y+29,r.x+107,r.y+57),"$500.00",25,true,"recordAmount"+i,b.transform,TextAlignmentOptions.MidlineLeft); amount.color=Forest; amount.fontSizeMin=17*scale;
            Text("Status",R(r.x+127,r.y+15,r.xMax-24,r.y+52),"Test complete - No payment",16,key:"recordStatus"+i,parent:b.transform,align:TextAlignmentOptions.MidlineLeft);
            var completedDot=Text("CompleteDot",R(r.x+107,r.y+20,r.x+122,r.y+46),"\u2022",26,true,parent:b.transform); completedDot.color=new Color32(46,187,39,255); Obj(completedDot.gameObject,"recordComplete"+i);
            var otherDot=Text("OtherDot",R(r.x+107,r.y+20,r.x+122,r.y+46),"\u2022",26,true,parent:b.transform); otherDot.color=new Color32(231,143,31,255); Obj(otherDot.gameObject,"recordOther"+i);
            Text("Chevron",R(r.xMax-20,r.y+12,r.xMax-5,r.y+54),">",29,parent:b.transform);
        }
        Button("Back",R(1298,640,1623,698),"BACK TO WALLET",WithdrawalAction.Back,"PrimaryButton"); End();
    }
    private static void Sparkles(Vector2[] positions)
    {
        for(int i=0;i<positions.Length;i++) { float s=i%2==0?21:14; ArtImage("Sparkle"+i,R(positions[i].x-s/2,positions[i].y-s/2,positions[i].x+s/2,positions[i].y+s/2),"GoldenSparkle"); }
    }
    private static void Hud()
    {
        WithdrawalHudAuthoring.Build();
    }
}
