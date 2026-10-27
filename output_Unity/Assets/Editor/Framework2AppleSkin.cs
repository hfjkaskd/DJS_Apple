using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>Author the approved layered skin into saved prefabs; never creates static UI at runtime.</summary>
public static partial class Framework2AppleSkin
{
    private const string Source = "Assets/Framework2Replica/Resources/Framework2Replica/Sources/ArrowOutRuntimeResources/";
    private const string Art = "Assets/Framework2AppleSkin/Resources/Framework2AppleSkin/";
    private const string Config = "Assets/Framework2Replica/Resources/Framework2Replica/ProductConfig.json";
    private const string ExpectedConfigHash = "74A89260E72FC83AEEE7E3D1BD5BC8CEEB6543AF08342CD19E1FC05359648537";
    private static readonly Dictionary<string, Sprite> SkinSprites = new Dictionary<string, Sprite>();
    private static Font cachedSkinFont;
    static Font SkinFont => cachedSkinFont != null ? cachedSkinFont :
        cachedSkinFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/ApkEarningReference/Visuals/Font/Vagron.ttf");
    static readonly Color Brown = new Color32(91, 38, 18, 255);
    static Sprite SkinSprite(string key)
    {
        if (!SkinSprites.TryGetValue(key, out Sprite value)) throw new InvalidOperationException("Missing skin sprite " + key);
        return value;
    }

    [MenuItem("Tools/Framework 2/Apply Approved Apple Skin")]
    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play before authoring.");
        if (Hash(Config) != ExpectedConfigHash) throw new InvalidOperationException("Source product rules changed before skin authoring.");
        string originalHost = "Assets/Resources/EarningVariants/EarningVariantController.prefab";
        string originalHash = Hash(originalHost);
        ImportArt(); ImportReferenceArt();
        string[] names = {
            "GamePlayerView", "WithDraw", "WithDrawmethod", "WithDrawConfirm", "WithDrawQues", "WithDrawProcess1",
            "WithDrawHistory", "PayPalErrorView", "WithdrawMissionView", "WithdrawFeedbackView", "SlotView", "ProgressView",
            "BloatView", "BonusWheelView", "WithDrawSuc", "BloatProgressView", "RewardView", "ChallangeSuccessView",
            "LuckySuccessView", "LuckySpinView", "GameBalancView", "NoticeView", "SettingsView", "UserLevelView",
            "WithdrawView", "WithDrawContinue", "ReStartView", "Tutorial", "UIEffectView", "WithDrawItem",
            "historyItem", "historyItemBlue", "priceItem", "LocalContentErrorTip"
        };
        var paths = new List<string>();
        foreach (string name in names)
        {
            string path = FindPrefab(name);
            if (path == null) throw new InvalidOperationException("Source prefab missing: " + name);
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                foreach (var surface in root.GetComponentsInChildren<Framework2ReferenceSurface>(true)) Object.DestroyImmediate(surface);
                foreach (var cutout in root.GetComponentsInChildren<Framework2ReferenceCutout>(true)) Object.DestroyImmediate(cutout);
                if (root.GetComponent<Framework2AppleSkinMarker>() == null) root.AddComponent<Framework2AppleSkinMarker>();
                foreach (Text text in root.GetComponentsInChildren<Text>(true))
                { text.font = SkinFont; text.raycastTarget = false; }
                ApplyAccounts(root, name); ApplyRewards(root, name); ApplyNavigation(root, name);
                ApplyReferenceAlignment(root, name);
                FinalizePrefab(root); RefFinish(root); AlignReferenceIllustrations(root,name);
                PrefabUtility.SaveAsPrefabAsset(root, path);
                paths.Add(path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        string bHost = "Assets/Resources/EarningVariants/EarningVariantControllerApple.prefab";
        GameObject host = PrefabUtility.LoadPrefabContents(originalHost);
        try
        {
            ApplyHost(host); AlignReferenceHost(host); AlignHostPages(host); FinalizePrefab(host); RefFinish(host);
            PrefabUtility.SaveAsPrefabAsset(host, bHost); paths.Add(bHost);
        }
        finally { PrefabUtility.UnloadPrefabContents(host); }
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        if (Hash(originalHost) != originalHash || Hash(Config) != ExpectedConfigHash)
            throw new InvalidOperationException("A prefab or source rules were unexpectedly changed.");
        string report = "Docs/CommercializationAB/Framework2Replica/ReskinImplementation-20261023";
        Directory.CreateDirectory(report);
        File.WriteAllText(report + "/authored-prefabs.txt", string.Join("\n", paths) + "\nProductConfig SHA256: " + Hash(Config) + "\nA host prefab unchanged: " + originalHash);
        Debug.Log("APPLE_SKIN_AUTHORING_PASS " + paths.Count + " prefabs. Source rules and A host unchanged.");
    }

    public static void ApplyAndCapture()
    {
        Apply();
        Framework2AppleSkinValidation.Run();
    }

    private static string FindPrefab(string name)
    {
        string[] candidates = Directory.GetFiles(Source, name + ".prefab", SearchOption.AllDirectories);
        return candidates.Length == 1 ? candidates[0].Replace('\\', '/') : null;
    }

    private static void FinalizePrefab(GameObject root)
    {
        // Exported source UI contains extreme historical Z offsets (over 10,000).
        // They clip otherwise valid labels in the camera canvas. All authored UI
        // is a two-dimensional hierarchy; keep its original XY anchors and order.
        foreach (RectTransform rect in root.GetComponentsInChildren<RectTransform>(true))
        {
            Vector3 position = rect.anchoredPosition3D;
            rect.anchoredPosition3D = new Vector3(position.x, position.y, 0);
        }
        // Source controllers select the channel explicitly. Inactive regional groups
        // must tolerate their children not yet being enabled during Instantiate.
        foreach (ToggleGroup group in root.GetComponentsInChildren<ToggleGroup>(true))
        {
            group.allowSwitchOff = true;
            if (group.GetComponent<Framework2AppleToggleGroupLifecycle>() == null)
                group.gameObject.AddComponent<Framework2AppleToggleGroupLifecycle>();
        }
        foreach (Image image in root.GetComponentsInChildren<Image>(true))
        {
            if (image.sprite == null || !AssetDatabase.GetAssetPath(image.sprite).StartsWith(Art, StringComparison.Ordinal)) continue;
            var localized = image.GetComponent<WZSDK.LocalizedImage>();
            if (localized != null) Object.DestroyImmediate(localized);
            switch (image.sprite.name)
            {
                case "panel": case "inset": case "title": case "button": case "secondary": case "selected":
                    image.type = Image.Type.Sliced; image.preserveAspect = false; break;
            }
            if (image.sprite.name == "fill" && image.type == Image.Type.Filled && image.fillMethod == Image.FillMethod.Horizontal)
            {
                if (image.GetComponent<Framework2AppleRoundedFill>() == null) image.gameObject.AddComponent<Framework2AppleRoundedFill>();
            }
            if (image.sprite.name == "track" && image.rectTransform.rect.height > 0)
            {
                image.type = Image.Type.Sliced;
                image.preserveAspect = false;
                image.pixelsPerUnitMultiplier = image.sprite.rect.height / image.rectTransform.rect.height / image.pixelsPerUnit;
            }
        }
        foreach (Button button in root.GetComponentsInChildren<Button>(true))
        {
            // Every visible action owns its standard Button and its graphic.
            if (button.targetGraphic != null && button.targetGraphic.gameObject == button.gameObject) continue;
            var graphic = button.GetComponent<Graphic>();
            if (graphic == null)
            {
                Image surface = button.gameObject.AddComponent<Image>();
                surface.sprite = SkinSprite("button"); surface.type = Image.Type.Sliced; surface.color = Color.white;
                graphic = surface;
            }
            button.targetGraphic = graphic;
            graphic.raycastTarget = true;
        }
        foreach (Text text in root.GetComponentsInChildren<Text>(true))
            if (text.color == Color.white && text.fontSize >= 40)
            {
                Outline edge = text.GetComponent<Outline>(); if (edge == null) edge = text.gameObject.AddComponent<Outline>();
                edge.enabled = true; edge.effectColor = new Color(.04f, .16f, .4f, .85f); edge.effectDistance = new Vector2(1.5f, -1.5f);
            }
        root.transform.Find("SkinHeading")?.SetAsLastSibling();
        root.transform.Find("SkinLeaf")?.SetAsLastSibling();
    }

    private static string Hash(string path)
    { using (var hash = SHA256.Create()) return BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(path))).Replace("-", ""); }

    private struct Slice
    {
        public string Name; public Rect Rect; public Vector4 Border;
        public Slice(string name, float x, float y, float width, float height, float border = 0)
        { Name = name; Rect = new Rect(x, y, width, height); Border = new Vector4(border, border, border, border); }
        public Slice Caps(float horizontal, float vertical)
        { Border = new Vector4(horizontal, vertical, horizontal, vertical); return this; }
    }

    private static void ImportArt()
    {
        AssetDatabase.Refresh(); SkinSprites.Clear();
        // Coordinates normalized to the inspected 1280-pixel preview, origin upper left.
        ImportAtlas("Chrome", 1254, 1254, new [] {
            new Slice("panel", 29, 82, 396, 376, 78), new Slice("inset", 479, 125, 297, 297, 45),
            new Slice("button", 827, 205, 405, 152).Caps(85, 45), new Slice("title", 28, 606, 451, 144).Caps(75, 44),
            new Slice("secondary", 499, 616, 388, 125).Caps(65, 38), new Slice("selected", 935, 548, 244, 243, 47),
            new Slice("close", 73, 931, 242, 240), new Slice("track", 380, 1001, 443, 102).Caps(53, 37),
            new Slice("fill", 843, 1008, 385, 87).Caps(44, 30)
        });
        ImportAtlas("Icons", 1254, 1254, new [] {
            new Slice("money", 24, 99, 266, 231), new Slice("cashpile", 304, 60, 335, 275),
            new Slice("wallet", 655, 30, 279, 309), new Slice("chest", 960, 25, 288, 318),
            new Slice("lock", 32, 337, 267, 310), new Slice("check", 329, 358, 292, 280),
            new Slice("error", 635, 358, 296, 281), new Slice("diamond", 948, 370, 298, 265),
            new Slice("bluebadge", 12, 650, 298, 287), new Slice("greenbadge", 314, 649, 306, 289),
            new Slice("goldbadge", 630, 649, 300, 289), new Slice("leaf", 960, 674, 288, 243),
            new Slice("pointer", 36, 938, 249, 288), new Slice("arrow", 314, 987, 302, 237),
            new Slice("video", 637, 987, 297, 225), new Slice("burst", 918, 927, 336, 327)
        });
        ImportAtlas("Wheels", 1792, 896, new [] {
            new Slice("wheel", 38, 32, 824, 818), new Slice("wheel8", 908, 32, 838, 818)
        });
        ImportAtlas("Receipt", 1, 1, new [] { new Slice("receipt", 0, 0, 1, 1) });
        ImportAtlas("Methods", 2048, 683, new [] {
            new Slice("bank", 45, 110, 505, 490), new Slice("card", 590, 180, 500, 390),
            new Slice("phone", 1130, 80, 360, 530), new Slice("knob", 1570, 150, 440, 440)
        });
    }

    private static void ImportAtlas(string name, float referenceWidth, float referenceHeight, Slice[] slices)
    {
        string path = Art + name + ".png";
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) throw new FileNotFoundException("Skin atlas missing", path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.isReadable = false;
        importer.wrapMode = TextureWrapMode.Clamp; importer.filterMode = FilterMode.Bilinear;
        importer.maxTextureSize = 2048; importer.textureCompression = TextureImporterCompression.CompressedHQ;
        importer.spritePixelsPerUnit = 100;
        importer.GetSourceTextureWidthAndHeight(out int width, out int height);
        var data = new SpriteMetaData[slices.Length];
        for (int i = 0; i < slices.Length; i++)
        {
            Slice s = slices[i]; float sx = width / referenceWidth, sy = height / referenceHeight;
            data[i] = new SpriteMetaData { name = s.Name, alignment = (int)SpriteAlignment.Center, pivot = new Vector2(.5f, .5f),
                rect = new Rect(s.Rect.x * sx, height - (s.Rect.y + s.Rect.height) * sy, s.Rect.width * sx, s.Rect.height * sy),
                border = new Vector4(s.Border.x * sx, s.Border.y * sy, s.Border.z * sx, s.Border.w * sy) };
        }
        importer.spritesheet = data;
        importer.SaveAndReimport();
        foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
            if (asset is Sprite sprite) SkinSprites[sprite.name] = sprite;
    }
}
