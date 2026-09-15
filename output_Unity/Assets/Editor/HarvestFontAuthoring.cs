using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

/// <summary>
/// Explicit offline font authoring. Runtime uses the saved static font assets only.
/// Source fonts live under an Editor folder; no system font or dynamic runtime fallback is used.
/// </summary>
public static class HarvestFontAuthoring
{
    public const string FontDirectory = "Assets/res/local/harvest/fonts";
    public const string TitleAssetPath = FontDirectory + "/HarvestTitleRound SDF.asset";
    public const string BodyAssetPath = FontDirectory + "/HarvestBodyRound SDF.asset";
    public const string TitleOutlineMaterialPath = FontDirectory + "/HarvestTitleRound WhiteOutline.mat";
    public const string TitleGreenOutlineMaterialPath = FontDirectory + "/HarvestTitleRound GreenOutline.mat";
    public const string TitlePlainMaterialPath = FontDirectory + "/HarvestTitleRound Plain.mat";
    public const string BodyPlainMaterialPath = FontDirectory + "/HarvestBodyRound Plain.mat";
    private const string TitleSourcePath = FontDirectory + "/Editor/Source/LilitaOne-Regular.ttf";
    private const string BodySourcePath = FontDirectory + "/Editor/Source/HarvestBodyRound-ExtraBold.ttf";
    private const int SamplingPointSize = 90;
    private const int AtlasPadding = 8;
    private const int AtlasSize = 1024;
    // Both source cmaps have these glyphs. All ASCII punctuation is included by the range below.
    private const string ExtraCharacters = "\u00a2\u00a3\u00a5\u00a9\u00ae\u00b0\u00b1\u00b7\u00d7\u00f7\u2013\u2014\u2018\u2019\u201c\u201d\u2022\u2026\u20ac";

    [MenuItem("Tools/Harvest/Bake Static Fonts")]
    public static void BuildAll()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        TMP_FontAsset title = BakeFont(TitleSourcePath, TitleAssetPath, "Harvest Title Round", SamplingPointSize, AtlasPadding, AtlasSize, AtlasSize);
        TMP_FontAsset body = BakeFont(BodySourcePath, BodyAssetPath, "Harvest Body Round", SamplingPointSize, AtlasPadding, AtlasSize, AtlasSize);
        Material titleOutline = SaveMaterialPreset(title, TitleOutlineMaterialPath, new Color32(103, 49, 17, 255), 0.23f);
        Material titleGreenOutline = SaveMaterialPreset(title, TitleGreenOutlineMaterialPath, new Color32(28, 116, 31, 255), 0.20f);
        Material titlePlain = SaveMaterialPreset(title, TitlePlainMaterialPath, Color.clear, 0f);
        Material bodyPlain = SaveMaterialPreset(body, BodyPlainMaterialPath, Color.clear, 0f);
        AssetDatabase.SaveAssets();
        WriteBindingManifest(title, body, titleOutline, titleGreenOutline, titlePlain, bodyPlain);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        Debug.Log("HARVEST_FONTS_OK: two static 1024x1024 Alpha8 atlases, ASCII and UI punctuation including bullet, four shared material presets; no runtime font source reference.");
    }

    /// <summary>Parameters are authoring choices, not runtime configuration or initialization.</summary>
    public static TMP_FontAsset BakeFont(string sourcePath, string assetPath, string familyName,
        int pointSize, int padding, int atlasWidth, int atlasHeight)
    {
        if (!assetPath.StartsWith(FontDirectory + "/", StringComparison.Ordinal) ||
            pointSize < 1 || padding < 1 || atlasWidth < 64 || atlasHeight < 64)
            throw new ArgumentException("Invalid harvest font authoring parameters.");
        Font source = AssetDatabase.LoadAssetAtPath<Font>(sourcePath);
        if (source == null) throw new InvalidOperationException("Missing licensed harvest font source: " + sourcePath);

        TMP_FontAsset baked = null;
        Texture2D atlas = null;
        Material material = null;
        bool persisted = false;
        try
        {
            // TMP's API requires Dynamic while adding characters. Only the final Static asset is saved.
            baked = TMP_FontAsset.CreateFontAsset(source, pointSize, padding, GlyphRenderMode.SDFAA,
                atlasWidth, atlasHeight, AtlasPopulationMode.Dynamic, false);
            if (baked == null) throw new InvalidOperationException("TMP could not load the harvest font face: " + sourcePath);
            atlas = baked.atlasTextures[0];
            material = baked.material;
            baked.name = Path.GetFileNameWithoutExtension(assetPath);
            uint[] characters = GetRequiredCharacters();
            uint[] missing;
            if (!baked.TryAddCharacters(characters, out missing, true))
                throw new InvalidOperationException("Harvest atlas is incomplete; adjust its authoring dimensions or source. Missing: " + DescribeMissing(missing));
            if (baked.atlasTextures == null || baked.atlasTextures.Length != 1 || baked.atlasTextureCount != 1)
                throw new InvalidOperationException("Harvest fonts must fit one static atlas per family.");

            atlas = baked.atlasTextures[0];
            material = baked.material;
            if (atlas == null || material == null || atlas.width != atlasWidth || atlas.height != atlasHeight || atlas.format != TextureFormat.Alpha8)
                throw new InvalidOperationException("Unexpected harvest font atlas or material.");
            atlas.name = baked.name + " Atlas";
            atlas.filterMode = FilterMode.Bilinear;
            atlas.wrapMode = TextureWrapMode.Clamp;
            atlas.anisoLevel = 0;
            material.name = baked.name + " Atlas Material";
            ConfigurePlainMaterial(material, atlas, padding);
            // The source is already a real ExtraBold face; faux bold would make counters muddy.
            baked.normalStyle = 0f;
            baked.boldStyle = 0f;
            baked.fallbackFontAssetTable = new List<TMP_FontAsset>();
            baked.isMultiAtlasTexturesEnabled = false;
            var face = baked.faceInfo;
            // Keep derived font names separate from the source's Reserved Font Name.
            face.familyName = familyName;
            baked.faceInfo = face;
            baked.atlasPopulationMode = AtlasPopulationMode.Static;
            if (baked.sourceFontFile != null)
                throw new InvalidOperationException("The static harvest font retained a runtime font source reference.");
            baked.ReadFontAssetDefinition();
            for (int i = 0; i < characters.Length; i++)
                if (!baked.HasCharacter((int)characters[i])) throw new InvalidOperationException("Missing baked glyph U+" + characters[i].ToString("X4", CultureInfo.InvariantCulture));

            TMP_FontAsset existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(baked, assetPath);
                AssetDatabase.AddObjectToAsset(atlas, baked);
                AssetDatabase.AddObjectToAsset(material, baked);
                persisted = true;
                EditorUtility.SetDirty(baked);
                EditorUtility.SetDirty(atlas);
                EditorUtility.SetDirty(material);
                return baked;
            }

            // Preserve the GUID and local sub-asset IDs when re-baking an already referenced font.
            if (existing.atlasTextures == null || existing.atlasTextures.Length != 1 || existing.material == null)
                throw new InvalidOperationException("Existing harvest font has an unexpected asset structure: " + assetPath);
            Texture2D existingAtlas = existing.atlasTextures[0];
            Material existingMaterial = existing.material;
            if (existingAtlas == null || AssetDatabase.GetAssetPath(existingAtlas) != assetPath || AssetDatabase.GetAssetPath(existingMaterial) != assetPath)
                throw new InvalidOperationException("Existing harvest font atlas/material must be its own sub-assets.");
            EditorUtility.CopySerialized(atlas, existingAtlas);
            existingMaterial.CopyPropertiesFromMaterial(material);
            existingMaterial.mainTexture = existingAtlas;
            EditorUtility.CopySerialized(baked, existing);
            existing.atlasTextures = new[] { existingAtlas };
            existing.material = existingMaterial;
            existing.ReadFontAssetDefinition();
            EditorUtility.SetDirty(existingAtlas);
            EditorUtility.SetDirty(existingMaterial);
            EditorUtility.SetDirty(existing);
            return existing;
        }
        finally
        {
            if (!persisted)
            {
                if (material != null) UnityEngine.Object.DestroyImmediate(material);
                if (atlas != null) UnityEngine.Object.DestroyImmediate(atlas);
                if (baked != null) UnityEngine.Object.DestroyImmediate(baked);
            }
        }
    }

    public static uint[] GetRequiredCharacters()
    {
        var points = new List<uint>(114);
        for (uint code = 32; code <= 126; code++) points.Add(code);
        for (int i = 0; i < ExtraCharacters.Length; i++) points.Add(ExtraCharacters[i]);
        return points.ToArray();
    }

    private static void ConfigurePlainMaterial(Material material, Texture2D atlas, int padding)
    {
        material.mainTexture = atlas;
        material.SetColor(ShaderUtilities.ID_FaceColor, Color.white);
        material.SetColor(ShaderUtilities.ID_OutlineColor, Color.clear);
        material.SetFloat(ShaderUtilities.ID_OutlineWidth, 0f);
        material.SetFloat(ShaderUtilities.ID_FaceDilate, 0f);
        material.SetFloat(ShaderUtilities.ID_WeightNormal, 0f);
        material.SetFloat(ShaderUtilities.ID_WeightBold, 0f);
        material.SetFloat(ShaderUtilities.ID_GradientScale, padding + 1);
        material.SetFloat(ShaderUtilities.ID_TextureWidth, atlas.width);
        material.SetFloat(ShaderUtilities.ID_TextureHeight, atlas.height);
        material.DisableKeyword("OUTLINE_ON");
        material.DisableKeyword("UNDERLAY_ON");
        material.DisableKeyword("UNDERLAY_INNER");
        material.DisableKeyword("GLOW_ON");
    }

    private static Material SaveMaterialPreset(TMP_FontAsset font, string path, Color outlineColor, float outlineWidth)
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(font.material);
            material.name = Path.GetFileNameWithoutExtension(path);
            AssetDatabase.CreateAsset(material, path);
        }
        else material.CopyPropertiesFromMaterial(font.material);
        material.SetColor(ShaderUtilities.ID_OutlineColor, outlineColor);
        material.SetFloat(ShaderUtilities.ID_OutlineWidth, outlineWidth);
        // The mobile SDF fragment shader only draws outlines in this shader variant.
        if (outlineWidth > 0f && outlineColor.a > 0f) material.EnableKeyword("OUTLINE_ON");
        else material.DisableKeyword("OUTLINE_ON");
        EditorUtility.SetDirty(material);
        return material;
    }

    private static string DescribeMissing(uint[] missing)
    {
        if (missing == null || missing.Length == 0) return "atlas packing failed";
        var result = new StringBuilder();
        for (int i = 0; i < missing.Length; i++)
        {
            if (i > 0) result.Append(", ");
            result.Append("U+").Append(missing[i].ToString("X4", CultureInfo.InvariantCulture));
        }
        return result.ToString();
    }

    private static void WriteBindingManifest(TMP_FontAsset title, TMP_FontAsset body, Material titleOutline, Material titleGreenOutline, Material titlePlain, Material bodyPlain)
    {
        var manifest = new StringBuilder("{\n");
        AppendBinding(manifest, "titleFont", title, true);
        AppendBinding(manifest, "bodyFont", body, true);
        AppendBinding(manifest, "titleOutlineMaterial", titleOutline, true);
        AppendBinding(manifest, "titleGreenOutlineMaterial", titleGreenOutline, true);
        AppendBinding(manifest, "titlePlainMaterial", titlePlain, true);
        AppendBinding(manifest, "bodyPlainMaterial", bodyPlain, false);
        manifest.Append("}\n");
        File.WriteAllText(FontDirectory + "/font-bindings.json", manifest.ToString(), new UTF8Encoding(false));
    }

    private static void AppendBinding(StringBuilder text, string key, UnityEngine.Object asset, bool trailingComma)
    {
        string guid;
        long fileId;
        if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out guid, out fileId))
            throw new InvalidOperationException("Harvest font binding is not persistent: " + key);
        text.Append("  \"").Append(key).Append("\": {\"guid\": \"").Append(guid)
            .Append("\", \"fileID\": ").Append(fileId.ToString(CultureInfo.InvariantCulture)).Append("}")
            .Append(trailingComma ? ",\n" : "\n");
    }
}
