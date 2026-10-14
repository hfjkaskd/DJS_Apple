using System;
using System.IO;
using FruitWithdrawal;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>Editor authoring only: all static layout and styling are saved in the prefab.</summary>
public static class WithdrawalHudAuthoring
{
    private const string Path = "Assets/res/local/withdrawal/WithdrawalHud.prefab";
    private const string Art = "Assets/WithdrawalReferenceArt/";
    private static readonly Color Ink = new Color32(83, 34, 14, 255);
    private static TMP_FontAsset display, body;
    private static Material outlined;

    [MenuItem("Tools/Withdrawal/Build Top Currency HUD %&h")]
    public static void BuildAndValidate()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play before editing saved HUD assets.");
        Build();
        WithdrawalHudValidation.Run();
    }

    public static void Build()
    {
        display = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Art + "Fonts/ReferenceDisplay.asset");
        body = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Art + "Fonts/ReferenceBody.asset");
        outlined = AssetDatabase.LoadAssetAtPath<Material>(Art + "ReferenceWhiteOutline.mat");
        if (display == null || body == null || outlined == null) throw new InvalidOperationException("Build reference art/fonts before the currency HUD.");
        GameObject root = PrefabUtility.LoadPrefabContents(Path);
        try {
            root.SetActive(false);
            for (int i = root.transform.childCount - 1; i >= 0; i--) Object.DestroyImmediate(root.transform.GetChild(i).gameObject);
            var rect = (RectTransform)root.transform;
            root.layer = 5;
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, 1);
            rect.pivot = new Vector2(.5f, 1);
            rect.anchoredPosition = new Vector2(0, -24);
            rect.sizeDelta = new Vector2(1000, 174);
            rect.localScale = Vector3.one;
            var hud = root.GetComponent<WithdrawalHud>();
            Currency(root.transform, "Cash", -196, "CashBundle", "$0.00", out hud.CashButton, out hud.Cash);
            Currency(root.transform, "Diamonds", 196, "Gem", "0", out hud.DiamondButton, out hud.Diamonds);
            Text("TestLabel", root.transform, new Vector2(-135, -130), new Vector2(470, 38), "TEST MODE • NO CASH VALUE", 24, false).color = Ink;
            hud.RewardsButton = Action("Rewards", root.transform, new Vector2(260, -132), new Vector2(240, 48), "REWARDS", out hud.Rewards, false);
            hud.Rewards.fontSize = hud.Rewards.fontSizeMax = 25;
            hud.Rewards.fontSizeMin = 19;
            root.SetActive(true);
            PrefabUtility.SaveAsPrefabAsset(root, Path);
        } finally { PrefabUtility.UnloadPrefabContents(root); }
        AssetDatabase.SaveAssets();
        Debug.Log("[WithdrawalHudAuthoring] Saved the cash/diamond balance HUD. Gameplay installation is a bounded YAML patch.");
    }

    private static void Currency(Transform parent, string name, float x, string icon, string value, out Button button, out TMP_Text amount)
    {
        var card = Image(name + "Card", parent, new Vector2(x, -50), new Vector2(368, 100), "InsetCard", true);
        card.pixelsPerUnitMultiplier = 2.4f;
        var glyph = Image(name + "Icon", card.transform, new Vector2(-143, 0), new Vector2(66, 72), icon, false);
        glyph.preserveAspect = true;
        amount = Text(name + "Balance", card.transform, new Vector2(-28, 0), new Vector2(152, 76), value, 38, true);
        amount.fontSizeMin = 22;
        amount.enableWordWrapping = false;
        TMP_Text action;
        button = Action(name, card.transform, new Vector2(119, 0), new Vector2(118, 94), "Cash", out action, true);
        action.fontSize = action.fontSizeMax = 32;
        action.fontSizeMin = 26;
    }

    private static RectTransform Node(string name, Transform parent, Vector2 pos, Vector2 size)
    {
        var node = new GameObject(name, typeof(RectTransform)); node.layer = 5;
        var rect = (RectTransform)node.transform; rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
        if (parent.name == "WithdrawalHud") rect.anchorMin = rect.anchorMax = new Vector2(.5f, 1);
        rect.anchoredPosition = pos; rect.sizeDelta = size; return rect;
    }

    private static Sprite Sprite(string name)
    {
        foreach (Object item in AssetDatabase.LoadAllAssetsAtPath(Art + name + ".png")) if (item is Sprite sprite) return sprite;
        throw new InvalidOperationException("Missing HUD sprite: " + name);
    }

    private static Image Image(string name, Transform parent, Vector2 pos, Vector2 size, string art, bool sliced)
    {
        var image = Node(name, parent, pos, size).gameObject.AddComponent<Image>();
        image.sprite = Sprite(art); image.type = sliced ? UnityEngine.UI.Image.Type.Sliced : UnityEngine.UI.Image.Type.Simple;
        image.raycastTarget = false; return image;
    }

    private static TMP_Text Text(string name, Transform parent, Vector2 pos, Vector2 size, string value, float sizePx, bool headline)
    {
        var text = Node(name, parent, pos, size).gameObject.AddComponent<TextMeshProUGUI>();
        text.font = headline ? display : body; text.text = value; text.color = Ink;
        text.fontSize = text.fontSizeMax = sizePx; text.fontSizeMin = sizePx * .75f;
        text.enableAutoSizing = true; text.enableWordWrapping = false;
        text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false; return text;
    }

    private static Button Action(string name, Transform parent, Vector2 pos, Vector2 size, string label, out TMP_Text text, bool primary)
    {
        var image = Image(name, parent, pos, size, primary ? "PrimaryButton" : "SecondaryButton", true);
        image.pixelsPerUnitMultiplier = primary ? 2.5f : 3;
        image.raycastTarget = true;
        var button = image.gameObject.AddComponent<Button>(); button.targetGraphic = image;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        var colors = button.colors; colors.pressedColor = new Color(.86f,.93f,.84f); button.colors = colors;
        text = Text("Label", image.transform, Vector2.zero, size - new Vector2(14,10), label, primary ? 40 : 26, primary);
        if (primary) { text.color = Color.white; text.fontSharedMaterial = outlined; }
        return button;
    }
}
