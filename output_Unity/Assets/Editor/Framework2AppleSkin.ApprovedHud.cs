using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static partial class Framework2AppleSkin
{
    static readonly Dictionary<string, Sprite> approvedHud = new Dictionary<string, Sprite>();

    // Build small, reusable UI sprites from the approved board. Live amounts,
    // level numbers and progress are deliberately removed from the chrome.
    // This is an asset-authoring operation, never a runtime texture readback.
    static void ImportApprovedHud()
    {
        const string path = Art + "HudApproved.png";
        var board = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        board.LoadImage(File.ReadAllBytes("Docs/CommercializationAB/Framework2Replica/ReskinPreview-AllUI-20261023/board-01.png"));
        var atlas = new Texture2D(1024, 256, TextureFormat.RGBA32, false);
        atlas.SetPixels32(new Color32[1024 * 256]);
        var slices = new List<SpriteMetaData>();
        int cursorX = 2, cursorY = 2, rowHeight = 0;
        Action<string, RectInt, float, Vector4, Func<int, int, Color>> add = (name, crop, radius, border, sample) =>
        {
            if (cursorX + crop.width + 2 > atlas.width) { cursorX = 2; cursorY += rowHeight + 4; rowHeight = 0; }
            for (int y = 0; y < crop.height; y++)
            for (int x = 0; x < crop.width; x++)
            {
                Color color = sample != null ? sample(x, y) : HudPixel(board, crop.x + x, crop.y + y);
                float dx = Mathf.Max(Mathf.Abs(x + .5f - crop.width * .5f) - (crop.width * .5f - radius), 0);
                float dy = Mathf.Max(Mathf.Abs(y + .5f - crop.height * .5f) - (crop.height * .5f - radius), 0);
                if (radius > 0) color.a *= Mathf.Clamp01(radius + .5f - Mathf.Sqrt(dx * dx + dy * dy));
                atlas.SetPixel(cursorX + x, cursorY + crop.height - 1 - y, color);
            }
            slices.Add(new SpriteMetaData { name = name, rect = new Rect(cursorX, cursorY, crop.width, crop.height),
                pivot = Vector2.one * .5f, alignment = (int)SpriteAlignment.Center, border = border });
            cursorX += crop.width + 4; rowHeight = Mathf.Max(rowHeight, crop.height);
        };

        add("hud-panel", new RectInt(144, 72, 321, 66), 33, new Vector4(33, 25, 33, 25), (x, y) =>
        {
            int bx=144+x,by=72+y;
            Color c = HudPixel(board, bx, by);
            // Remove only the icon ink, preserving the curved frame beneath it.
            if (bx >= 162 && bx < 212 && by >= 82 && by < 128 && c.g >= c.r - .06f)
                return HudPixel(board, 217, by);
            if (bx >= 228 && bx < 339 && by >= 90 && by < 122) return HudPixel(board, 217, by);
            return c;
        });
        add("hud-task", new RectInt(69, 147, 462, 49), 24.5f, new Vector4(25, 18, 25, 18), (x, y) =>
            x >= 16 && x < 448 && y >= 10 && y < 37 ? HudPixel(board, 435, 147 + y) : HudPixel(board, 69 + x, 147 + y));
        add("hud-cash", new RectInt(356, 72, 109, 66), 33, Vector4.zero, null);
        add("hud-settings", new RectInt(474, 72, 68, 67), 33.5f, Vector4.zero, null);
        add("hud-level", new RectInt(57, 67, 78, 77), 38.5f, Vector4.zero, (x, y) =>
        {
            int bx = x + 57, by = y + 67;
            if (bx >= 82 && bx <= 110 && by >= 86 && by <= 122)
            {
                Color left = HudPixel(board, 80, by), right = HudPixel(board, 112, by);
                if (left.r > .3f) left = right;
                return Color.Lerp(left, right, (bx - 80) / 32f);
            }
            return HudPixel(board, bx, by);
        });
        add("hud-money", new RectInt(162, 82, 50, 46), 0, Vector4.zero, (x, y) =>
        {
            Color c = HudPixel(board, 162 + x, 82 + y);
            // Cream lies outside the green silhouette; the dollar highlights are neutral.
            c.a = Mathf.Clamp01((c.g - c.r + .025f) * 35f);
            return c;
        });
        add("hud-leaf", new RectInt(49, 61, 38, 35), 0, Vector4.zero, (x, y) =>
        {
            Color c = HudPixel(board, 49 + x, 61 + y);
            c.a = Mathf.Clamp01((c.g - Mathf.Max(c.r, c.b) + .018f) * 40f);
            return c;
        });
        add("hud-track", new RectInt(86, 158, 119, 26), 13, new Vector4(13, 12, 13, 12), (x, y) =>
            HudPixel(board, x < 33 ? 204 - x : 86 + x, 158 + y));
        add("hud-fill", new RectInt(86, 158, 26, 26), 8, new Vector4(12, 12, 12, 12), null);
        atlas.Apply(false);
        string evidence = Environment.GetEnvironmentVariable("DJS_APPLE_SKIN_VALIDATION_OUTPUT");
        if (!string.IsNullOrEmpty(evidence))
        {
            Directory.CreateDirectory(evidence);
            var crop = new Texture2D(525, 145, TextureFormat.RGBA32, false);
            crop.SetPixels(board.GetPixels(33, board.height - 58 - 145, 525, 145)); crop.Apply(false);
            File.WriteAllBytes(Path.Combine(evidence, "approved-hud.png"), crop.EncodeToPNG());
            Object.DestroyImmediate(crop);
        }
        File.WriteAllBytes(path, atlas.EncodeToPNG());
        Object.DestroyImmediate(board); Object.DestroyImmediate(atlas);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritesheet = slices.ToArray(); importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true; importer.maxTextureSize = 1024;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.filterMode = FilterMode.Bilinear; importer.wrapMode = TextureWrapMode.Clamp;
        importer.SaveAndReimport(); approvedHud.Clear();
        foreach (Object item in AssetDatabase.LoadAllAssetsAtPath(path))
            if (item is Sprite sprite) approvedHud.Add(sprite.name, sprite);
    }

    static Color HudPixel(Texture2D board, int x, int y) => board.GetPixel(x, board.height - 1 - y);

    static void HudSprite(GameObject root, string path, string key, bool sliced = false)
    {
        Image image = root.transform.Find(path).GetComponent<Image>();
        var surface = image.GetComponent<Framework2ReferenceSurface>();
        if (surface != null) Object.DestroyImmediate(surface);
        var cutout = image.GetComponent<Framework2ReferenceCutout>();
        if (cutout != null) Object.DestroyImmediate(cutout);
        image.overrideSprite = null; image.sprite = approvedHud[key]; image.color = Color.white;
        image.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
        image.preserveAspect = false;
        image.pixelsPerUnitMultiplier = sliced ? image.sprite.rect.height / image.rectTransform.rect.height / image.pixelsPerUnit : 1;
    }

    static void HudFont(GameObject root, string path, int size, float tracking = 0)
    {
        Text text = root.transform.Find(path).GetComponent<Text>();
        text.font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Framework2Replica/Resources/Framework2Replica/External/Assets/Font/Roboto-Bold.ttf");
        text.fontSize = text.resizeTextMaxSize = size;
        text.resizeTextMinSize = Mathf.RoundToInt(size * .75f);
        text.fontStyle = FontStyle.Normal;
        var spacing = text.GetComponent<Framework2AppleTextTracking>();
        if (tracking != 0 || spacing != null)
        {
            if (spacing == null) spacing = text.gameObject.AddComponent<Framework2AppleTextTracking>();
            spacing.Configure(tracking);
        }
    }

    static void ApplyApprovedHudArtwork(GameObject root)
    {
        const string p = "Area/TopButtons/";
        HudSprite(root, p + "LV", "hud-level");
        HudSprite(root, p + "LV/NavLevelLeaf", "hud-leaf");
        HudSprite(root, p + "CurMoney/Bg1", "hud-panel");
        HudSprite(root, p + "CurMoney/Bg1/MoneyIcon/MoneyIcon", "hud-money");
        HudSprite(root, p + "CurMoney/CoinBtn", "hud-cash");
        root.transform.Find(p + "CurMoney/CoinBtn/Text").gameObject.SetActive(false);
        HudSprite(root, p + "Setting", "hud-settings");
        HudFont(root, p + "LV/NavLevelNumber", 90);
        HudFont(root, p + "CurMoney/Bg1/CoinTxt", 64);
        foreach (string task in new[] { "NavEarlyTaskRoot", "NavAdTaskRoot", "NavBalanceTaskRoot" })
        {
            HudSprite(root, p + task, "hud-task");
            HudSprite(root, p + task + "/NavTrack", "hud-track", true);
            HudSprite(root, p + task + "/NavFill", "hud-fill");
            Image fill = root.transform.Find(p + task + "/NavFill").GetComponent<Image>();
            fill.type = Image.Type.Filled; fill.fillMethod = Image.FillMethod.Horizontal; fill.fillOrigin = 0;
            var round = fill.GetComponent<Framework2AppleRoundedFill>();
            if (round == null) round = fill.gameObject.AddComponent<Framework2AppleRoundedFill>();
            round.ConfigureMinimumCap(true);
        }
        HudFont(root, p + "NavBalanceTaskRoot/NavBalanceCount", 46, .7f);
        HudFont(root, p + "NavBalanceTaskRoot/NavPercent", 45);
        HudFont(root, p + "NavEarlyTaskRoot/NavObjective", 45);
        HudFont(root, p + "NavAdTaskRoot/NavCount", 45);
        HudFont(root, p + "NavAdTaskRoot/NavObjective", 43);
        HudNumerals(root,p+"CurMoney/Bg1/CoinTxt",amountGlyphs,1200f/525,new Vector2(0,1200f/525));
        HudNumerals(root,p+"NavBalanceTaskRoot/NavBalanceCount",countGlyphs,1200f/525,new Vector2(0,-1));
        HudNumerals(root,p+"NavBalanceTaskRoot/NavPercent",countGlyphs,1200f/525,new Vector2(0,-1));
        HudNumerals(root,p+"LV/NavLevelNumber",levelGlyphs,1200f/525,Vector2.zero);
        NavButtonTarget(root.transform.Find(p + "CurMoney/CoinBtn"));
        NavButtonTarget(root.transform.Find(p + "Setting"));
    }

    public static void ApplyHudAndCapture()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play before authoring.");
        string originalHost = "Assets/Resources/EarningVariants/EarningVariantController.prefab";
        string originalHash = Hash(originalHost);
        if (Hash(Config) != ExpectedConfigHash) throw new InvalidOperationException("Source product rules changed.");
        ImportArt(); ImportReferenceArt(); ImportApprovedHud(); ImportApprovedHudGlyphs(); Framework2ReferenceLayoutAudit.BeginExisting();
        string path = FindPrefab("GamePlayerView");
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            var rect = (RectTransform)root.transform; Vector2 saved = rect.sizeDelta;
            rect.sizeDelta = new Vector2(1200, 2132);
            root.GetComponent<Framework2AppleSkinMarker>().RestoreAuthoringFonts();
            NavHud(root); AlignHud(root); FinalizePrefab(root); RefFinish(root);
            ApplyApprovedHudArtwork(root);
            rect.sizeDelta = saved; PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        Framework2ReferenceLayoutAudit.Save();
        if (Hash(originalHost) != originalHash || Hash(Config) != ExpectedConfigHash)
            throw new InvalidOperationException("A prefab or source rules changed.");
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        Debug.Log("APPLE_APPROVED_HUD_AUTHORING_PASS: A and ProductConfig unchanged.");
        Framework2AppleSkinValidation.Run();
    }
}
