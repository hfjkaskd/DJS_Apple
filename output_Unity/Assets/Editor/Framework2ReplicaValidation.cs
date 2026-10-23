using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using MahjongSlide.Withdrawal;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Read-only audit of the port's saved assets and the live source's business data.
/// Does not run a host, change player data, simulate a payment, or mutate prefabs.
/// </summary>
public static class Framework2ReplicaValidation
{
    private const string ResourceRoot = "Assets/Framework2Replica/Resources/Framework2Replica/";
    private const string EvidenceRoot = "Docs/CommercializationAB/Framework2Replica/";
    private const string ExpectedConfigSha256 = "74a89260e72fc83aeee7e3d1bd5bc8ceeb6543af08342cd19e1fc05359648537";

    [Serializable] private sealed class AddressEntry { public string address, path, subAsset; }
    [Serializable] private sealed class AddressMap { public AddressEntry[] entries; }
    [Serializable] private sealed class LayoutBaseline { public string sourceCommit; public PrefabBaseline[] prefabs; }
    [Serializable] private sealed class PrefabBaseline { public string path, sourceSha256; public RectBaseline[] rects; }
    [Serializable] private sealed class RectBaseline
    {
        public string fileId;
        public float[] m_LocalPosition, m_LocalRotation, m_LocalScale, m_AnchorMin,
            m_AnchorMax, m_AnchoredPosition, m_SizeDelta, m_Pivot;
    }
    [Serializable] private sealed class Report
    {
        public string generatedUtc, sourceCommit, configSha256;
        public string layoutComparison = "Saved Unity YAML vectors by RectTransform file ID; never compare Canvas-derived runtime localPosition.";
        public string scope = "Saved Framework2 prefabs, resource address closure, live-source business config and serialized layout parity. " +
            "Does not execute gameplay, ads or payment. Local withdrawal state is not payment evidence. " +
            "Separate PlayMode/device verification is required for live AB switching and input/settlement routing.";
        public int addressCount, prefabCount, buttonCount, rectCount, layoutRectChecks, guidReferenceChecks, configAssertions;
        public List<string> errors = new List<string>();
        public List<string> warnings = new List<string>();
    }

    [MenuItem("Tools/Framework 2 Replica/Validate Saved Assets And Source Parity")]
    public static void ValidateAssetsAndConfig()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play before the read-only Framework2 saved-asset audit.");
        var report = new Report { generatedUtc = DateTime.UtcNow.ToString("O") };
        AuditConfig(report);
        AuditAddresses(report);
        AuditPrefabs(report);
        AuditLayoutBaseline(report);
        AuditSerializedGuids(report);
        string output = Environment.GetEnvironmentVariable("DJS_APPLE_F2_VALIDATION_OUTPUT");
        if (string.IsNullOrWhiteSpace(output)) output = EvidenceRoot + "Validation";
        Directory.CreateDirectory(output);
        File.WriteAllText(Path.Combine(output, "saved-assets-validation.json"), JsonUtility.ToJson(report, true), new UTF8Encoding(false));
        if (report.errors.Count != 0)
            throw new InvalidOperationException("FRAMEWORK2_REPLICA_ASSETS_FAIL " + report.errors.Count + " errors: " + string.Join(" | ", report.errors));
        Debug.Log("FRAMEWORK2_REPLICA_ASSETS_PASS addresses=" + report.addressCount + " prefabs=" + report.prefabCount +
            " layoutRects=" + report.layoutRectChecks + " buttons=" + report.buttonCount + " configAssertions=" + report.configAssertions);
    }

    private static void AuditConfig(Report report)
    {
        string path = ResourceRoot + "ProductConfig.json";
        if (!File.Exists(path)) { report.errors.Add("Missing live source product: " + path); return; }
        using (var hash = SHA256.Create())
            report.configSha256 = BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(path))).Replace("-", "").ToLowerInvariant();
        Check(report.configSha256 == ExpectedConfigSha256, "Live source product SHA256 changed.", report);
        var product = JsonUtility.FromJson<Framework2ProductConfig>(File.ReadAllText(path));
        if (product == null || product.economy == null) { report.errors.Add("Live product did not deserialize."); return; }
        try { product.economy.ValidateOrThrow(); } catch (Exception e) { report.errors.Add("Live product validation: " + e.Message); }
        var economy = product.economy;
        int[] types = { 1, 1, 4, 2, 2 }, values = { 1, 2, 3, 200, 3000 }, auto = { 1, 1, 0, 1, 1 };
        Check(economy.stages != null && economy.stages.Length == 5, "Five source stages required.", report);
        if (economy.stages != null && economy.stages.Length == 5)
            for (int i = 0; i < 5; i++)
            {
                var stage = economy.stages[i];
                Check(stage.Id == i + 1 && stage.Stage == i + 1 && stage.Type == types[i] && stage.Value == values[i] && stage.AutoNextStage == auto[i],
                    "Source stage mismatch at " + (i + 1), report);
            }
        Near(economy.normalCoinHardCap, 2999.5f, "normalCoinHardCap", report);
        Near(economy.initialCoin, 0f, "initialCoin", report);
        Near(economy.initialGems, 0f, "initialGems", report);
        Check(economy.noCoinRewardEndLevel == 2, "First two levels must not grant ordinary cash.", report);
        Near(economy.stage3AdRewardAmount, .1f, "stage3AdRewardAmount", report);
        Near(economy.stage4SlotRewardAmount, 50f, "stage4SlotRewardAmount", report);
        Near(economy.stage5CompensationTargetCoin, 2700f, "stage5CompensationTargetCoin", report);
        Near(economy.rewards.settlementNonAdDivisor, 10f, "settlementNonAdDivisor", report);
        Near(economy.rewards.inGameNonAdDivisor, 10f, "inGameNonAdDivisor", report);
        Near(economy.rewards.settlementNonAdButtonDelaySeconds, 1f, "settlementNonAdButtonDelaySeconds", report);
        Near(economy.rewards.inGameNonAdButtonDelaySeconds, 1f, "inGameNonAdButtonDelaySeconds", report);
        Check(economy.rewards.luckySpinAdTarget == 6, "Lucky spin advertisement target must remain six.", report);
        Near(economy.rewards.minimumCashOutAmount, .1f, "minimumCashOutAmount", report);
        Near(economy.rewards.cashOutHandlingFee, .2f, "cashOutHandlingFee", report);
        Near(economy.rewards.feeFreeCashOutThreshold, 2f, "feeFreeCashOutThreshold", report);
        int[] exchange = { 500, 1000, 1500, 2000 };
        Check(economy.rewards.gemExchangeCosts.Length == 4 && economy.rewards.gemExchangeCashAmounts.Length == 4,
            "Four source diamond exchange tiers required.", report);
        for (int i = 0; i < Math.Min(4, economy.rewards.gemExchangeCosts.Length); i++)
            Check(economy.rewards.gemExchangeCosts[i] == exchange[i] && economy.rewards.gemExchangeCashAmounts[i] == exchange[i],
                "Diamond exchange tier mismatch at " + i, report);
        Check(product.refusalsBeforeAd == new Vector2Int(3, 4), "Refusal counter range mismatch.", report);
        Check(product.refuseInterstitialLevelRange == new Vector2Int(4, 10), "Refusal interstitial level range mismatch.", report);
        Check(product.refuseInterstitialPercent == 50, "Refusal interstitial percentage mismatch.", report);
        Check(product.timedRewardStartLevel == 4 && !product.useMahjongInGameRewardTrigger && !product.iaaMode,
            "Source trigger/economy mode mismatch.", report);
        Check(product.sourceDefaults.EnableLuckyRewardOnPourComplete && product.sourceDefaults.LuckySpin_Elimination_Interval == 40,
            "Source matching reward cadence mismatch.", report);
        Near(product.sourceDefaults.TimerReward_Interval, 30f, "TimerReward_Interval", report);
        Check(product.currencyDatabase != null && product.currencyDatabase.DefaultProfile != null && product.currencyDatabase.DefaultProfile.PrimaryChannel == 1,
            "US payment-channel packed array did not deserialize as [1].", report);
        Check(product.currencyDatabase != null && product.currencyDatabase.Resolve("KR").PrimaryChannel == 23,
            "KR payment-channel packed array did not deserialize as [23].", report);
    }

    private static void AuditAddresses(Report report)
    {
        string path = ResourceRoot + "AddressMap.json";
        if (!File.Exists(path)) { report.errors.Add("Missing F2 resource address map."); return; }
        var map = JsonUtility.FromJson<AddressMap>(File.ReadAllText(path));
        var addresses = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (map == null || map.entries == null) { report.errors.Add("Invalid resource map."); return; }
        foreach (var entry in map.entries)
        {
            report.addressCount++;
            if (entry == null || string.IsNullOrWhiteSpace(entry.address) || string.IsNullOrWhiteSpace(entry.path))
            { report.errors.Add("Empty resource map entry."); continue; }
            if (!addresses.Add(entry.address)) report.errors.Add("Duplicate address: " + entry.address);
            if (!entry.path.StartsWith("Framework2Replica/", StringComparison.Ordinal)) report.errors.Add("Address leaves B resource scope: " + entry.address);
            var asset = Resources.Load<UnityEngine.Object>(entry.path);
            if (asset == null) report.errors.Add("Unresolvable source address: " + entry.address + " -> " + entry.path);
            if (!string.IsNullOrEmpty(entry.subAsset))
            {
                bool found = false;
                foreach (Sprite sprite in Resources.LoadAll<Sprite>(entry.path))
                    if (string.Equals(sprite.name, entry.subAsset, StringComparison.Ordinal)) { found = true; break; }
                if (!found) report.errors.Add("Missing exact source Sprite subasset: " + entry.address + " -> " + entry.path + " [" + entry.subAsset + "]");
            }
        }
    }

    private static void AuditPrefabs(Report report)
    {
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { ResourceRoot.TrimEnd('/') }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) { report.errors.Add("Prefab cannot load: " + path); continue; }
            report.prefabCount++;
            foreach (Transform child in prefab.GetComponentsInChildren<Transform>(true))
            {
                if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject) != 0)
                    report.errors.Add("Missing script: " + path + " / " + child.name);
            }
            foreach (Component component in prefab.GetComponentsInChildren<Component>(true))
            {
                if (component == null) continue;
                using (var serialized = new SerializedObject(component))
                {
                    var property = serialized.GetIterator();
                    while (property.Next(true))
                        if (property.propertyType == SerializedPropertyType.ObjectReference &&
                            property.objectReferenceValue == null && property.objectReferenceInstanceIDValue != 0)
                            report.errors.Add("Broken serialized reference: " + path + " / " + component.name + " / " + property.propertyPath);
                }
            }
            foreach (Button button in prefab.GetComponentsInChildren<Button>(true))
            {
                report.buttonCount++;
                if (button.onClick.GetPersistentEventCount() != 0)
                    report.errors.Add("Inspector-bound Unity event must be replaced by code binding: " + path + " / " + button.name);
                if (button.targetGraphic != null && !button.targetGraphic.transform.IsChildOf(button.transform))
                    report.warnings.Add("Button targetGraphic is outside Button subtree: " + path + " / " + button.name);
            }
            foreach (RectTransform rect in prefab.GetComponentsInChildren<RectTransform>(true))
            {
                report.rectCount++;
                if (!Finite(rect.anchoredPosition) || !Finite(rect.sizeDelta) || !Finite(rect.anchorMin) || !Finite(rect.anchorMax))
                    report.errors.Add("Non-finite UI geometry: " + path + " / " + rect.name);
            }
        }
        if (report.prefabCount == 0) report.errors.Add("No Framework2 prefabs were imported.");
    }

    private static void AuditLayoutBaseline(Report report)
    {
        string path = EvidenceRoot + "source-layout-fingerprint.json";
        if (!File.Exists(path)) { report.errors.Add("Missing source layout baseline."); return; }
        var baseline = JsonUtility.FromJson<LayoutBaseline>(File.ReadAllText(path));
        report.sourceCommit = baseline.sourceCommit;
        foreach (var expectedPrefab in baseline.prefabs)
        {
            string target = ResourceRoot + expectedPrefab.path;
            if (!File.Exists(target)) { report.errors.Add("Source prefab not retained: " + expectedPrefab.path); continue; }
            // RectTransform.localPosition is recalculated by Unity from the current
            // Canvas size and anchors during asset import/loading. Both baselines
            // here must use saved YAML; otherwise identical serialized prefabs can
            // report different derived positions in different Editor windows.
            var actual = ReadSavedRects(File.ReadAllText(target), expectedPrefab.path, report);
            if (actual.Count != expectedPrefab.rects.Length)
                report.errors.Add("Source serialized RectTransform count differs: " + expectedPrefab.path);
            foreach (var expected in expectedPrefab.rects)
            {
                if (!actual.TryGetValue(expected.fileId, out RectBaseline rect))
                { report.errors.Add("Source UI element missing: " + expectedPrefab.path + " / " + expected.fileId); continue; }
                report.layoutRectChecks++;
                string label = expectedPrefab.path + " / " + expected.fileId;
                Compare(rect.m_LocalPosition, expected.m_LocalPosition, label + " localPosition", report);
                Compare(rect.m_LocalRotation, expected.m_LocalRotation, label + " localRotation", report);
                Compare(rect.m_LocalScale, expected.m_LocalScale, label + " localScale", report);
                Compare(rect.m_AnchorMin, expected.m_AnchorMin, label + " anchorMin", report);
                Compare(rect.m_AnchorMax, expected.m_AnchorMax, label + " anchorMax", report);
                Compare(rect.m_AnchoredPosition, expected.m_AnchoredPosition, label + " anchoredPosition", report);
                Compare(rect.m_SizeDelta, expected.m_SizeDelta, label + " sizeDelta", report);
                Compare(rect.m_Pivot, expected.m_Pivot, label + " pivot", report);
            }
        }
    }

    private static Dictionary<string, RectBaseline> ReadSavedRects(string yaml, string path, Report report)
    {
        var result = new Dictionary<string, RectBaseline>(StringComparer.Ordinal);
        foreach (Match document in Regex.Matches(yaml,
            @"(?ms)^--- !u!224 &(?<id>-?\d+)[^\r\n]*\r?\n(?<body>.*?)(?=^--- !u!|\z)", RegexOptions.CultureInvariant))
        {
            string id = document.Groups["id"].Value;
            string body = document.Groups["body"].Value;
            string label = path + " / " + id;
            var rect = new RectBaseline
            {
                fileId = id,
                m_LocalPosition = ReadSavedVector(body, "m_LocalPosition", 3, label, report),
                m_LocalRotation = ReadSavedVector(body, "m_LocalRotation", 4, label, report),
                m_LocalScale = ReadSavedVector(body, "m_LocalScale", 3, label, report),
                m_AnchorMin = ReadSavedVector(body, "m_AnchorMin", 2, label, report),
                m_AnchorMax = ReadSavedVector(body, "m_AnchorMax", 2, label, report),
                m_AnchoredPosition = ReadSavedVector(body, "m_AnchoredPosition", 2, label, report),
                m_SizeDelta = ReadSavedVector(body, "m_SizeDelta", 2, label, report),
                m_Pivot = ReadSavedVector(body, "m_Pivot", 2, label, report)
            };
            if (result.ContainsKey(id)) report.errors.Add("Duplicate serialized RectTransform file ID: " + label);
            else result.Add(id, rect);
        }
        return result;
    }

    private static float[] ReadSavedVector(string body, string field, int size, string label, Report report)
    {
        Match value = Regex.Match(body, @"(?m)^  " + field + @": \{([^}\r\n]+)\}", RegexOptions.CultureInvariant);
        string[] parts = value.Success ? value.Groups[1].Value.Split(',') : new string[0];
        var values = new float[size];
        string[] axes = { "x", "y", "z", "w" };
        if (parts.Length == size)
        {
            bool valid = true;
            for (int i = 0; i < size; i++)
            {
                string[] component = parts[i].Split(':');
                valid &= component.Length == 2 && component[0].Trim() == axes[i] &&
                    float.TryParse(component[1], System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out values[i]) &&
                    !float.IsNaN(values[i]) && !float.IsInfinity(values[i]);
            }
            if (valid) return values;
        }
        report.errors.Add("Invalid saved RectTransform vector: " + label + " / " + field);
        return null;
    }

    private static void AuditSerializedGuids(Report report)
    {
        var found = new HashSet<string>(StringComparer.Ordinal);
        var pattern = new Regex(@"guid: ([0-9a-f]{32})", RegexOptions.CultureInvariant);
        foreach (string path in Directory.GetFiles(ResourceRoot, "*", SearchOption.AllDirectories))
        {
            string extension = Path.GetExtension(path);
            if (extension != ".prefab" && extension != ".asset" && extension != ".mat" && extension != ".controller" && extension != ".anim") continue;
            foreach (Match match in pattern.Matches(File.ReadAllText(path)))
            {
                string guid = match.Groups[1].Value;
                if (guid.StartsWith("0000000000000000", StringComparison.Ordinal) || !found.Add(guid)) continue;
                report.guidReferenceChecks++;
                if (string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(guid)))
                    report.errors.Add("Unresolved asset GUID " + guid + " used by " + path);
            }
        }
    }

    private static void Check(bool condition, string error, Report report) { report.configAssertions++; if (!condition) report.errors.Add(error); }
    private static void Near(double value, double expected, string field, Report report) { Check(Math.Abs(value - expected) < .0001d, field + " differs from live source.", report); }
    private static bool Finite(Vector2 value) => !float.IsNaN(value.x) && !float.IsInfinity(value.x) && !float.IsNaN(value.y) && !float.IsInfinity(value.y);
    private static void Compare(float[] value, float[] expected, string label, Report report)
    {
        if (expected == null || value == null || value.Length != expected.Length) { report.errors.Add("Invalid layout baseline: " + label); return; }
        for (int i = 0; i < value.Length; i++)
            if (Mathf.Abs(value[i] - expected[i]) > .00001f) { report.errors.Add("Source layout differs: " + label); return; }
    }
}
