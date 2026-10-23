using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

/// <summary>Rebinds recovered script references to the equivalent installed DLL scripts.</summary>
public static class Framework2ReplicaAssetPreparation
{
    private const string ResourceRoot = "Assets/Framework2Replica/Resources/Framework2Replica";
    [Serializable] private sealed class AddressEntry { public string address, path, subAsset; }
    [Serializable] private sealed class AddressCatalog { public AddressEntry[] entries; }

    private sealed class ScriptBinding
    {
        public string SourceGuid;
        public string DllPath;
        public Type ComponentType;
        public string TargetGuid;
        public long TargetFileId;
        public int References;
    }

    [MenuItem("Tools/Framework 2 Replica/Prepare Imported DLL References")]
    public static void Prepare()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || BuildPipeline.isBuildingPlayer)
            throw new InvalidOperationException("Stop Play Mode and any active build before preparing imported assets.");

        var bindings = new[]
        {
            new ScriptBinding { SourceGuid = "ff326197253ae4cec0b46632391386ec", DllPath = "Assets/Plugins/spine-unity.dll", ComponentType = typeof(Spine.Unity.SkeletonGraphic) },
            new ScriptBinding { SourceGuid = "aecae2a74e033ea0f3031a0758bd00be", DllPath = "Assets/Plugins/spine-unity.dll", ComponentType = typeof(Spine.Unity.SkeletonDataAsset) },
            new ScriptBinding { SourceGuid = "fb6a9381bd1153e90fb3841230adf0c3", DllPath = "Assets/Plugins/spine-unity.dll", ComponentType = typeof(Spine.Unity.SpineAtlasAsset) },
            new ScriptBinding { SourceGuid = "a811bde74b26b53498b4f6d872b09b6d", DllPath = "Assets/Plugins/DOTween.dll", ComponentType = typeof(DG.Tweening.Core.DOTweenSettings) }
        };

        foreach (var binding in bindings)
        {
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(binding.DllPath))
            {
                var script = asset as MonoScript;
                if (script == null || script.GetClass() != binding.ComponentType) continue;
                if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(script, out binding.TargetGuid, out binding.TargetFileId))
                    throw new InvalidOperationException("Cannot resolve installed script ID: " + binding.ComponentType.FullName);
                break;
            }
            if (string.IsNullOrEmpty(binding.TargetGuid))
                throw new InvalidOperationException("Installed DLL script not found: " + binding.ComponentType.FullName);
        }

        var changed = new List<string>();
        foreach (string path in Directory.GetFiles(ResourceRoot, "*", SearchOption.AllDirectories))
        {
            string extension = Path.GetExtension(path);
            if (extension != ".prefab" && extension != ".asset") continue;
            string original = File.ReadAllText(path);
            string updated = original;
            foreach (var binding in bindings)
            {
                string pattern = @"m_Script: \{fileID: -?\d+, guid: " + binding.SourceGuid + @", type: 3\}";
                updated = Regex.Replace(updated, pattern, match =>
                {
                    binding.References++;
                    return "m_Script: {fileID: " + binding.TargetFileId + ", guid: " + binding.TargetGuid + ", type: 3}";
                });
            }
            if (updated == original) continue;
            File.WriteAllText(path, updated);
            changed.Add(path);
        }

        foreach (string path in changed)
            AssetDatabase.ImportAsset(path.Replace('\\', '/'), ImportAssetOptions.ForceSynchronousImport);
        NormalizeSingleSpriteAddresses();
        AssetDatabase.SaveAssets();
        foreach (var binding in bindings)
            Debug.Log("FRAMEWORK2_SCRIPT_BINDING " + binding.ComponentType.FullName + " " + binding.SourceGuid + " -> " + binding.TargetGuid + ":" + binding.TargetFileId + " references=" + binding.References);
        Debug.Log("FRAMEWORK2_ASSET_PREPARE_PASS changed=" + changed.Count);
    }

    /// <summary>Single-image extraction kept legacy atlas labels, but Unity names a Single Sprite after its texture.</summary>
    public static void NormalizeSingleSpriteAddresses()
    {
        string catalogPath = ResourceRoot + "/AddressMap.json";
        var catalog = JsonUtility.FromJson<AddressCatalog>(File.ReadAllText(catalogPath));
        var singleNames = new Dictionary<string, string>(StringComparer.Ordinal);
        int corrected = 0;
        foreach (var entry in catalog.entries)
        {
            if (string.IsNullOrEmpty(entry.subAsset)) continue;
            if (!singleNames.TryGetValue(entry.path, out string actualName))
            {
                const string prefix = "Framework2Replica/";
                if (!entry.path.StartsWith(prefix, StringComparison.Ordinal))
                    throw new InvalidOperationException("Unexpected Framework2 resource path: " + entry.path);
                string texturePath = ResourceRoot + "/" + entry.path.Substring(prefix.Length) + ".png";
                var importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
                if (importer == null || importer.spriteImportMode != SpriteImportMode.Single)
                    actualName = string.Empty; // Multi-Sprite sources continue to use the original exact selector.
                else
                {
                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);
                    if (sprite == null) throw new InvalidOperationException("Single Sprite failed to import: " + texturePath);
                    actualName = sprite.name;
                }
                singleNames.Add(entry.path, actualName);
            }
            if (string.IsNullOrEmpty(actualName) || entry.subAsset == actualName) continue;
            Debug.Log("FRAMEWORK2_SINGLE_SPRITE_SELECTOR " + entry.address + " [" + entry.subAsset + "] -> [" + actualName + "] sameTexture=" + entry.path);
            entry.subAsset = actualName;
            corrected++;
        }
        if (corrected > 0)
        {
            File.WriteAllText(catalogPath, JsonUtility.ToJson(catalog, true));
            AssetDatabase.ImportAsset(catalogPath, ImportAssetOptions.ForceSynchronousImport);
        }
        Debug.Log("FRAMEWORK2_SINGLE_SPRITE_SELECTORS_PASS corrected=" + corrected);
    }
}
