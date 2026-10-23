using System;
using FruitApkEarning;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static partial class ApkEarningReferenceLayoutAuthoring
{
    private const string MessageArtworkPath = "Assets/Resources/OrchardEarning/Messages/OrchardMessageBoard.png";
    private const string MessageResourcePath = "OrchardEarning/Messages/OrchardMessageBoard";

    [MenuItem("Tools/Apk Earning/Apply Illustrated Message Cards")]
    public static void ApplyMessageCards()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play before authoring message prefabs.");
        ImportMessageArtwork();
        Edit("LocalEarningSequence", FollowupSequence);
        AssetDatabase.SaveAssets();
    }

    public static void ApplyValidateMessageCards()
    {
        ApplyMessageCards();
        ApkEarningValidation.ValidateMessageLayouts();
        ApkEarningFlowAnimationValidation.Run();
    }

    private static void ImportMessageArtwork()
    {
        AssetDatabase.ImportAsset(MessageArtworkPath, ImportAssetOptions.ForceSynchronousImport);
        var importer = AssetImporter.GetAtPath(MessageArtworkPath) as TextureImporter;
        if (importer == null) throw new InvalidOperationException("Approved message artwork is missing.");
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.isReadable = false;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.maxTextureSize = 2048;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.CompressedHQ;
        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);
        importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings {
            name = "Android", overridden = true, maxTextureSize = 2048,
            format = TextureImporterFormat.ASTC_6x6, compressionQuality = 80
        });
        importer.SaveAndReimport();
    }

    private static void IllustratedMessageCards(ApkEarningPageView view)
    {
        if (AssetDatabase.LoadAssetAtPath<Sprite>(MessageArtworkPath) == null)
            throw new InvalidOperationException("Run Apply Illustrated Message Cards to import the approved sprite.");
        string[] names = { "Assessment", "Activating", "Activated", "Summary" };
        view.ResourceImages = new ApkEarningPageView.ResourceImageSlot[names.Length];
        for (int i = 0; i < names.Length; i++)
        {
            string prefix = "Message/" + names[i];
            Image image = view.GetImage("messageLayouts[" + i + "].Artwork");
            if (image == null) throw new InvalidOperationException("Missing message Artwork binding.");
            FollowupPosition(view, image.rectTransform, new Rect(10, 590, 1060, 1060f * 948 / 1659), new Vector2(.5f, .5f));
            // One shared sprite is loaded only when this page opens, not through
            // four strong serialized texture references in the page prefab.
            image.overrideSprite = null;
            image.sprite = null;
            image.enabled = true;
            image.type = Image.Type.Simple;
            image.preserveAspect = true;
            image.color = Color.white;
            image.raycastTarget = false;
            image.transform.SetAsFirstSibling();
            foreach (Shadow shadow in image.GetComponents<Shadow>()) shadow.enabled = false;
            view.ResourceImages[i] = new ApkEarningPageView.ResourceImageSlot { Target = image, Path = MessageResourcePath };
            TMP_Text title = view.GetText("messageLayouts[" + i + "].Title");
            TMP_Text body = view.GetText("messageLayouts[" + i + "].Body");
            title.gameObject.SetActive(i != 0);
            if (i == 0)
                MessageText(view, body, new Rect(105, 920, 870, 140), 54);
            else
            {
                MessageText(view, title, new Rect(105, 908, 870, 56), 52);
                MessageText(view, body, new Rect(110, 974, 860, 108), i == 3 ? 38 : 42);
            }
            Transform oldYou = FollowupNode(view, prefix).Find("You");
            if (oldYou != null) oldYou.gameObject.SetActive(false);
        }

        // Saved under Message: it is visible for all four explanations and
        // hidden together with that group during receipt and bonus animation.
        RectTransform message = FollowupNode(view, "Message");
        Transform existing = message.Find("TestDisclosure");
        TMP_Text disclosure;
        if (existing == null)
        {
            disclosure = UnityEngine.Object.Instantiate(view.GetText("messageLayouts[0].Body"), message, false);
            disclosure.name = "TestDisclosure";
        }
        else disclosure = existing.GetComponent<TMP_Text>();
        MessageText(view, disclosure, new Rect(180, 1090, 720, 26), 22);
        disclosure.text = "TEST MODE · NO CASH PAYMENT";
        disclosure.color = new Color32(139, 98, 58, 255);
        var slots = new System.Collections.Generic.List<ApkEarningPageView.TextSlot>(view.Texts);
        slots.RemoveAll(slot => slot.Key == "MessageDisclosure");
        slots.Add(new ApkEarningPageView.TextSlot { Key = "MessageDisclosure", Target = disclosure });
        view.Texts = slots.ToArray();
        Image shade = view.GetImage("$root");
        if (shade != null) shade.color = new Color(0, 0, 0, .4f);
    }

    private static void MessageText(ApkEarningPageView view, TMP_Text text, Rect rect, float size)
    {
        if (text == null) throw new InvalidOperationException("Message text binding is missing.");
        FollowupPosition(view, text.rectTransform, rect, new Vector2(.5f, .5f));
        text.gameObject.SetActive(true);
        text.enabled = true;
        text.enableAutoSizing = true;
        text.enableWordWrapping = true;
        text.overflowMode = TextOverflowModes.Overflow;
        text.margin = Vector4.zero;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        text.fontStyle = FontStyles.Normal;
        text.color = new Color32(93, 43, 17, 255);
        SetFontSize(text, size);
        text.transform.SetAsLastSibling();
    }
}
