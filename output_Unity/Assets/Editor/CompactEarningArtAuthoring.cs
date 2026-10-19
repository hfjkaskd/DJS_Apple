using System;
using System.Collections.Generic;
using FruitApkEarning;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>Applies the approved cash-wallet visual language to the saved flow prefabs.</summary>
public static class CompactEarningArtAuthoring
{
    public const string ThemePath = "Assets/OrchardEarningArt/CompactEarningTheme.asset";
    const string Art = CompactEarningLayout.ArtFolder;

    [MenuItem("Tools/Apk Earning/Apply Complete Compact Flow and Validate %#&b")]
    public static void ApplyAndValidate()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play before authoring.");
        if (AssetDatabase.LoadAssetAtPath<CompactEarningTheme>(ThemePath) == null)
        {
            var theme = ScriptableObject.CreateInstance<CompactEarningTheme>();
            Seed(theme);
            AssetDatabase.CreateAsset(theme, ThemePath);
        }
        ApplyIfConfigured();
        ApkEarningValidation.RenderFixtures();
    }

    [MenuItem("Tools/Apk Earning/Restore Compact Layout Presets and Validate %#&n")]
    public static void RestoreLayoutPresets()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play before authoring.");
        var theme=AssetDatabase.LoadAssetAtPath<CompactEarningTheme>(ThemePath);
        if(theme==null){ApplyAndValidate();return;}
        theme.Pages.Clear();Seed(theme);EditorUtility.SetDirty(theme);AssetDatabase.SaveAssets();
        ApplyIfConfigured();ApkEarningValidation.RenderFixtures();
    }
    static void Seed(CompactEarningTheme theme)
    {CompactWalletLayouts.Seed(theme);CompactAccountLayouts.Seed(theme);CompactRewardLayouts.Seed(theme);}

    public static void ApplyIfConfigured()
    {
        var theme = AssetDatabase.LoadAssetAtPath<CompactEarningTheme>(ThemePath);
        if (theme == null || !theme.Enabled) return;
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play before authoring.");
        var font = Require<TMP_FontAsset>(theme.FontPath);
        var plain = Require<Material>(theme.PlainMaterialPath);
        var outlined = Require<Material>(theme.OutlinedMaterialPath);
        var oldTheme = Require<OrchardEarningTheme>(OrchardEarningArtAuthoring.ThemePath);
        var sources = new Dictionary<string,string>(StringComparer.Ordinal);
        foreach (var binding in oldTheme.AuthoredImages) sources[binding.PrefabPath+"|"+binding.HierarchyIndices] = binding.SourcePath;
        var ordered = new List<CompactEarningTheme.Page>(theme.Pages);
        // The result page inherits this base; author the base before its variant.
        int baseIndex = ordered.FindIndex(p => p.Name == "SuccessPanel");
        if (baseIndex >= 0) { var first = ordered[baseIndex]; ordered.RemoveAt(baseIndex); ordered.Insert(0,first); }
        foreach (var page in ordered)
        {
            string path = theme.PanelFolder + page.Name + ".prefab";
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var view = root.GetComponent<ApkEarningPageView>();
                if (view == null) throw new InvalidOperationException("Unbound earning page: "+path);
                ApplySharedArt(root, view, path, theme, sources, font, plain, outlined);
                if(page.Name=="SuccessPanel"||page.Name=="LocalSuccess"||page.Name=="LocalLuckyReward")
                {
                    // Background and content must share the production responsive root.
                    var background=view.GetImage("bg");
                    background.transform.SetParent(view.ArtworkRoot,false);
                    background.transform.SetAsFirstSibling();
                    foreach(string key in new[]{"GetBtn","MoreGetBtn","Close"})
                        AddObjectAlias(view,key,view.GetButton(key).gameObject);
                }
                if(page.Name=="LocalCashHistoryRow")
                {
                    AddObjectAlias(view,"statusLabel",view.GetText("statusLabel").gameObject);
                    AddObjectAlias(view,"actionButton",view.GetButton("actionButton").gameObject);
                }
                foreach (string key in page.DisableLayouts)
                {
                    var rect = Resolve(view,key);
                    if (rect == null) throw new InvalidOperationException(path+" missing layout "+key);
                    foreach (LayoutGroup group in rect.GetComponents<LayoutGroup>()) group.enabled=false;
                    foreach (ContentSizeFitter fitter in rect.GetComponents<ContentSizeFitter>()) fitter.enabled=false;
                }
                foreach (var rule in page.Elements)
                {
                    var rect = Resolve(view, rule.Target);
                    if (rect == null) throw new InvalidOperationException(path+" missing target "+rule.Target);
                    if (!page.PreserveActiveTargets.Contains(rule.Target)) rect.gameObject.SetActive(rule.Active);
                    if (rule.SetRect) SetRect(rect, view.ArtworkRoot, rule.Position, rule.Size);
                    if (rule.StyleImage)
                    {
                        var image = rect.GetComponent<Image>();
                        if (image == null) throw new InvalidOperationException(path+" missing Image "+rule.Target);
                        image.enabled=rule.ImageEnabled;
                        if (!string.IsNullOrEmpty(rule.SpritePath)) image.sprite=SpriteAt(rule.SpritePath);
                        image.type=rule.ImageType; image.pixelsPerUnitMultiplier=rule.PixelsPerUnit;
                        image.preserveAspect=rule.PreserveAspect; image.color=rule.ImageColor;
                    }
                    if (rule.StyleText)
                    {
                        var text=rect.GetComponent<TMP_Text>();
                        if (text == null) throw new InvalidOperationException(path+" missing Text "+rule.Target);
                        StyleText(text,font,rule.Outlined?outlined:plain,rule.FontSize,rule.TextColor);
                        text.fontStyle=rule.FontStyle; text.enableWordWrapping=rule.Wrap;
                        if (!string.IsNullOrEmpty(rule.Text)) text.text=rule.Text;
                    }
                }
                foreach (var rule in page.RowLayouts)
                {
                    var rect=Resolve(view,rule.Target);
                    var group=rect.GetComponent<HorizontalOrVerticalLayoutGroup>();
                    if(group==null)throw new InvalidOperationException(path+" missing row layout "+rule.Target);
                    group.enabled=true; group.spacing=rule.Spacing; group.childAlignment=rule.Alignment;
                    group.childControlWidth=group.childControlHeight=rule.ControlSize;
                    group.childForceExpandWidth=group.childForceExpandHeight=false;
                    if(rule.ItemSize!=Vector2.zero)foreach(RectTransform child in rect)
                    {
                        var layout=child.GetComponent<LayoutElement>();
                        if(layout==null)layout=child.gameObject.AddComponent<LayoutElement>();
                        layout.preferredWidth=rule.ItemSize.x;layout.preferredHeight=rule.ItemSize.y;
                    }
                }
                foreach(string key in page.SendToBackTargets) Resolve(view,key).SetAsFirstSibling();
                if(page.AddTestCaption) AddTestCaption(view,page,theme,font,plain);
                ConfigureDiamondOptions(view, page.Name, theme);
                // Some imported buttons use their visible TMP label as their target graphic.
                foreach(var button in root.GetComponentsInChildren<Button>(true))
                    if(button.targetGraphic!=null)button.targetGraphic.raycastTarget=true;
                RoundedEarningProgressAuthoring.Apply(root, true);
                PrefabUtility.SaveAsPrefabAsset(root,path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        AssetDatabase.SaveAssets();
        Debug.Log("[CompactEarningArt] Saved "+ordered.Count+" flow prefabs using the approved orchard assets.");
    }

    static void ApplySharedArt(GameObject root, ApkEarningPageView view, string path, CompactEarningTheme theme,
        Dictionary<string,string> sources,TMP_FontAsset font,Material plain,Material outlined)
    {
        foreach(var image in root.GetComponentsInChildren<Image>(true))
        {
            foreach(Shadow shadow in image.GetComponents<Shadow>())shadow.enabled=false;
            if(image.name=="OrchardAppleAccent")
            {
                image.sprite=SpriteAt(Art+"CashWalletApple.png");image.type=Image.Type.Simple;image.preserveAspect=true;
                var rect=image.rectTransform;rect.anchorMin=rect.anchorMax=new Vector2(0,.5f);
                rect.anchoredPosition=new Vector2(-8,45);rect.sizeDelta=new Vector2(100,125);continue;
            }
            if(image.sprite==null)continue;
            string spritePath=AssetDatabase.GetAssetPath(image.sprite);
            sources.TryGetValue(path+"|"+HierarchyIndices(root.transform,image.transform),out string source);
            string name=string.IsNullOrEmpty(source)?string.Empty:System.IO.Path.GetFileNameWithoutExtension(source);
            string replacement=null;float ppu=theme.InsetPPU;Image.Type type=Image.Type.Sliced;
            if(name=="close"||name=="aux-blue-close"||spritePath.EndsWith("PopClose_top.asset"))
            {replacement="CashWalletClose";type=Image.Type.Simple;}
            else if(spritePath.EndsWith("OrchardWoodTitle.png")) {replacement="CashWalletHeader";ppu=theme.HeaderPPU;}
            else if(spritePath.EndsWith("OrchardPrimaryButton.png")||spritePath.EndsWith("OrchardSelectedButton.png"))
            {replacement="CashWalletButton";ppu=theme.ButtonPPU;}
            else if(spritePath.EndsWith("OrchardPanel.png")||name=="record-card"||name=="cream-card")
            {
                bool main=name=="panel"||name=="panel-heading"||name=="background"||name=="diamond-panel"||name=="wood-card"||name=="aux-steps-panel";
                replacement=main?"CashWalletPanel":"CashWalletInset";ppu=main?theme.PanelPPU:theme.InsetPPU;
            }
            if(replacement!=null){image.sprite=SpriteAt(Art+replacement+".png");image.type=type;image.color=Color.white;image.pixelsPerUnitMultiplier=ppu;image.preserveAspect=type==Image.Type.Simple;}
        }
        foreach(var text in root.GetComponentsInChildren<TMP_Text>(true))
        {
            bool button=text.GetComponentInParent<Button>()!=null;
            bool title=false;
            for(var node=text.transform.parent;node!=null&&node!=root.transform;node=node.parent)
            {
                var image=node.GetComponent<Image>();
                if(image!=null&&image.sprite!=null&&AssetDatabase.GetAssetPath(image.sprite)==Art+"CashWalletHeader.png") {title=true;break;}
            }
            float size=Mathf.Min(text.fontSize,title?64:button?52:theme.BodyFontSize);
            StyleText(text,font,button||title?outlined:plain,size,button||title?new Color32(255,255,244,255):theme.Ink);
        }
        foreach(var input in root.GetComponentsInChildren<TMP_InputField>(true))
        {input.fontAsset=font;input.pointSize=theme.BodyFontSize;}
        foreach(var slot in view.Images)if(slot.Key=="backdrop"||slot.Key=="$root")
            if(slot.Target!=null&&slot.Target.sprite==null)slot.Target.color=theme.Scrim;
    }
    static void StyleText(TMP_Text text,TMP_FontAsset font,Material material,float size,Color color)
    {
        text.font=font;text.fontSharedMaterial=material;text.color=color;text.enableVertexGradient=false;
        text.fontStyle=FontStyles.Normal;text.fontWeight=FontWeight.Regular;
        text.fontSize=text.fontSizeMax=Mathf.Max(20,size);text.fontSizeMin=Mathf.Max(18,size*.75f);text.enableAutoSizing=true;
        text.margin=Vector4.zero;text.overflowMode=TextOverflowModes.Ellipsis;text.raycastTarget=false;
    }
    static void AddTestCaption(ApkEarningPageView view,CompactEarningTheme.Page page,CompactEarningTheme theme,TMP_FontAsset font,Material material)
    {
        var inherited=view.ArtworkRoot.Find("CompactTestMode");
        var text=inherited==null?view.GetText("TestMode"):inherited.GetComponent<TMP_Text>();
        if(text!=null)
        {
            foreach(var slot in view.Texts)if(slot.Key=="TestMode")slot.Target=text;
            foreach(var candidate in view.ArtworkRoot.GetComponentsInChildren<TMP_Text>(true))
                if(candidate!=text&&candidate.name=="CompactTestMode")Object.DestroyImmediate(candidate.gameObject);
        }
        if(text==null)
        {
            var go=new GameObject("CompactTestMode",typeof(RectTransform),typeof(CanvasRenderer),typeof(TextMeshProUGUI));
            go.layer=view.gameObject.layer;go.transform.SetParent(view.ArtworkRoot,false);text=go.GetComponent<TextMeshProUGUI>();
        }
        if(view.GetText("TestMode")==null)
        {var slots=new List<ApkEarningPageView.TextSlot>(view.Texts);slots.Add(new ApkEarningPageView.TextSlot{Key="TestMode",Target=text});view.Texts=slots.ToArray();}
        text.text="TEST MODE · NO CASH PAYMENT";text.alignment=TextAlignmentOptions.Center;text.enableWordWrapping=false;
        StyleText(text,font,material,theme.TestFontSize,theme.Ink);
        SetRect(text.rectTransform,view.ArtworkRoot,page.TestPosition,new Vector2(page.TestWidth,40));text.transform.SetAsLastSibling();
    }
    static void ConfigureDiamondOptions(ApkEarningPageView view,string name,CompactEarningTheme theme)
    {
        if(name!="TXCoinPanel")return;
        view.TintDiamondOptions=true;view.SelectedOptionColor=theme.SelectedOptionColor;view.NormalOptionColor=theme.NormalOptionColor;
        for(int i=0;i<4;i++)
        {
            var button=view.GetButton("options["+i+"]");var image=button.targetGraphic as Image;
            image.sprite=SpriteAt(Art+"CashWalletInset.png");image.type=Image.Type.Sliced;image.pixelsPerUnitMultiplier=theme.InsetPPU;
            image.color=i==0?view.SelectedOptionColor:view.NormalOptionColor;
        }
    }
    static void AddObjectAlias(ApkEarningPageView view,string key,GameObject target)
    {
        if(view.GetObject(key)!=null)return;
        var slots=new List<ApkEarningPageView.ObjectSlot>(view.Objects);
        slots.Add(new ApkEarningPageView.ObjectSlot{Key=key,Target=target});view.Objects=slots.ToArray();
    }
    public static RectTransform Resolve(ApkEarningPageView view,string selector)
    {
        string key=selector.Substring(2);
        switch(selector.Substring(0,2))
        {
            case "t:":return view.GetText(key)?.rectTransform;
            case "l:":return view.GetText(key)?.transform.parent as RectTransform;
            case "i:":return view.GetImage(key)?.rectTransform;
            case "b:":return view.GetButton(key)?.transform as RectTransform;
            case "o:":return view.GetObject(key)?.transform as RectTransform;
            case "u:":return view.GetInput(key)?.transform as RectTransform;
            case "p:":return view.transform.Find(key) as RectTransform;
            default:throw new InvalidOperationException("Unknown selector "+selector);
        }
    }
    static void SetRect(RectTransform rect,RectTransform artwork,Vector2 position,Vector2 size)
    {
        rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(.5f,.5f);rect.localScale=Vector3.one;rect.localRotation=Quaternion.identity;rect.sizeDelta=size;
        if(rect==artwork)rect.anchoredPosition=position;else rect.position=artwork.TransformPoint(position);
    }
    static string HierarchyIndices(Transform root,Transform target)
    {string result="";for(var node=target;node!=null&&node!=root;node=node.parent)result=node.GetSiblingIndex()+"/"+result;return result;}
    static Sprite SpriteAt(string path)
    {foreach(Object asset in AssetDatabase.LoadAllAssetsAtPath(path))if(asset is Sprite sprite)return sprite;throw new InvalidOperationException("Missing sprite "+path);}
    static T Require<T>(string path) where T:Object
    {var asset=AssetDatabase.LoadAssetAtPath<T>(path);if(asset==null)throw new InvalidOperationException("Missing asset "+path);return asset;}
}
