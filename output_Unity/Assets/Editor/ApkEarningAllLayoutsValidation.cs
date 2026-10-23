using System;
using System.Collections.Generic;
using System.IO;
using FruitApkEarning;
using UnityEditor;
using UnityEngine;

/// <summary>Full reachable earning UI coverage; fixture state never invokes the service.</summary>
public static partial class ApkEarningValidation
{
    private static readonly string[] AllFlowPages = {
        "TXPanel", "TXCoinPanel", "LocalWithdrawalConditions", "LocalCashHistory", "LocalEarningSequence",
        "LocalLuckyReward", "TXUserInfoPanel", "TXUserInfoSurePanel", "LocalSuccess", "LocalEarningRecovery",
        "TXRecordGuidePanel", "TXGuideLoadingPanel", "TXProgress0Panel", "TXChannelPanel",
        "LocalWithdrawalVerification", "TXGuideTargetCompletePanel", "LocalDiamondUnlockGuide",
        "LocalUserLevel", "TXTooFastHintPanel", "LocalRewardVideoTest", "Tip"
    };

    [MenuItem("Tools/Apk Earning/Validate Every Flow Layout")]
    public static void ValidateAllFlowLayouts()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play before validating saved layouts.");
        string directory = Environment.GetEnvironmentVariable("DJS_APPLE_ALL_LAYOUT_OUTPUT");
        if (string.IsNullOrWhiteSpace(directory)) directory = "E:/CodexCacheBackups/DJS_Apple-all-layout-20261022/after";
        Directory.CreateDirectory(directory);
        var report = new Report {
            generatedUtc = DateTime.UtcNow.ToString("O"),
            scope = "All 20 reachable earning pages, Tip toast and actual compact gameplay HUD. " +
                "Every listed state renders at 1080x1920, 1080x2400 and 720x1600. PNGs capture all states at 1080x1920 and HUD at every size. " +
                "All saved Panels prefabs and the real HUD/host are audited. Four unreachable source pages receive separate default renders only. " +
                "Synthetic fixture data, existing PageView layout, saved prefabs and real camera rendering; no account service, player save, Host navigation or payment call. " +
                "These previews are not phone screenshots or proof of pixel-identical APK reproduction. Runtime animation needs the separate Play-mode report."
        };
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { PanelFolder.TrimEnd('/') }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AuditPrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path), path, report);
        }
        AuditPrefab(AssetDatabase.LoadAssetAtPath<GameObject>(CorePath), CorePath, report, false, true);
        AuditPrefab(AssetDatabase.LoadAssetAtPath<GameObject>(TopPath), TopPath, report);
        AuditPrefab(AssetDatabase.LoadAssetAtPath<GameObject>(HostPath), HostPath, report, false);
        foreach (Vector2Int size in new[] { new Vector2Int(1080, 1920), new Vector2Int(1080, 2400), new Vector2Int(720, 1600) })
        {
            RenderFixture("GameplayHUD", CorePath, size, true, report, "", true, false, directory);
            foreach (string page in AllFlowPages)
                foreach (string state in AllFlowStates(page))
                    RenderFixture(page, PanelFolder + page + ".prefab", size, false, report, state,
                        size.x == 1080 && size.y == 1920, page != "Tip", directory);
        }
        foreach (string retained in new[] { "NewbieGuidePanel", "RewardPanel", "SuccessPanel", "TXLevelPanel" })
            RenderFixture(retained, PanelFolder + retained + ".prefab", new Vector2Int(1080, 1920), false,
                report, "retained-source", true, false, directory);
        File.WriteAllText(Path.Combine(directory, "all-flow-layout-validation.json"), JsonUtility.ToJson(report, true));
        if (report.errors.Count > 0)
            throw new InvalidOperationException("Full earning UI validation found " + report.errors.Count + " errors. See " + directory);
        Debug.Log("APK_EARNING_ALL_LAYOUT_PASS reachableSurfaces=22 audits=" + report.pages.Count +
            " renders=" + report.renders.Count + " pngs=" + report.renders.FindAll(item => !string.IsNullOrEmpty(item.image)).Count);
    }

    private static string[] AllFlowStates(string page)
    {
        switch (page)
        {
            case "TXPanel": return new[] { "", "early-reward" };
            case "TXCoinPanel": return new[] { "", "tier-1500", "tier-2000", "tier-1000", "target-reached" };
            case "LocalCashHistory": return new[] { "", "no-prior-records" };
            case "TXUserInfoPanel": return new[] { "", "email", "zelle", "invalid" };
            case "TXUserInfoSurePanel": return new[] { "", "phone", "other" };
            case "TXChannelPanel": return new[] { "", "invalid" };
            case "LocalWithdrawalVerification": return new[] { "", "checking" };
            case "TXProgress0Panel": return new[] { "", "reviewing" };
            case "LocalEarningSequence": return new[] { "", "activating", "activated", "summary", "receipt", "bonuses", "diamond-preview" };
            case "Tip": return new[] { "", "service-unavailable" };
            default: return FixtureStates(page);
        }
    }

    private static void ApplyAllFlowVariant(ApkEarningPageView page, string name, string state)
    {
        if (name == "TXRecordGuidePanel") return; // Intro uses its actual saved text and five milestones.
        if (name == "LocalEarningSequence" && state == "diamond-preview")
        {
            for (int i = 0; i < 4; i++) page.SetVisible("messageLayouts[" + i + "].Root", i == 3);
            page.SetText("messageLayouts[3].Title", "Extra reward unlocked");
            page.SetText("messageLayouts[3].Body", "Reward preview\n30,000 diamonds");
        }
        if (name == "TXPanel" && state == "early-reward")
        {
            page.SetText("GoldCount", "$1.50"); page.SetText("StageLabel", "Level 1");
            page.SetText("ProgressValue", "1 / 1"); page.SetProgress("Progress", 1);
            page.SetText("Tip", "Cash reward ready to review");
        }
        if (name == "TXCoinPanel" && state.StartsWith("tier-", StringComparison.Ordinal))
        {
            int index = state == "tier-1000" ? 1 : state == "tier-1500" ? 2 : 3;
            string amount = index == 1 ? "$1,000" : index == 2 ? "$1,500" : "$2,000";
            page.SetText("TXTip", amount + " Withdrawal Instructions:");
            float target = index == 1 ? 1000 : index == 2 ? 1500 : 2000;
            page.SetText("IitemTip", "Balance reaches " + amount + " to withdraw all\nStill $" +
                (target - 3.55f).ToString("N2", System.Globalization.CultureInfo.InvariantCulture) + " to go");
            page.SetText("TipBarValue", "$3.55 / " + amount);
            page.SetProgress("TipBar", 355.11f / (target * 100));
            for (int i = 0; i < 4; i++)
            {
                page.SetVisible("selectionFrames[" + i + "]", i == index);
                var button = page.GetButton("options[" + i + "]");
                if (button != null && button.targetGraphic != null)
                    button.targetGraphic.color = i == index ? page.SelectedOptionColor : page.NormalOptionColor;
            }
        }
        if (name == "LocalCashHistory" && state == "no-prior-records")
        {
            var parent = page.GetObject("rowParent");
            if (parent != null)
                // Runtime still shows the current reward when no prior requests exist.
                for (int i = parent.transform.childCount - 2; i >= 0; i--)
                    UnityEngine.Object.DestroyImmediate(parent.transform.GetChild(i).gameObject);
        }
        if (name == "TXUserInfoPanel" && state == "zelle")
            for (int i = 0; i < 3; i++) page.SetVisible("channels[" + i + "].Checkmark", i == 1);
        if (name == "TXUserInfoPanel" && state == "invalid")
            page.SetText("labels[0].Text", "Enter a test phone number with at least five digits.");
        if (name == "TXChannelPanel" && state == "invalid")
        {
            PreviewInput(page, "InputInfo", string.Empty);
            page.SetText("labels[0].Text", "Enter test payment account information to continue.");
        }
        if (name == "TXUserInfoSurePanel" && state == "phone")
        {
            PreviewSprite(page, "Image", "VenmoLarge");
            page.SetText("Tip", "Preview Player\n+1 2025550123\n\nTest request · no payment");
        }
        if (name == "TXUserInfoSurePanel" && state == "other")
        {
            PreviewSprite(page, "Image", "OtherChannel");
            page.SetText("Tip", "Test account\npreview@example.com\n\nTest request · no payment");
        }
        if (name == "LocalWithdrawalVerification" && state == "checking")
        {
            for (int i = 1; i <= 3; i++)
            {
                page.SetVisible("path:main/Step/" + i + "/Done", i == 1);
                page.SetVisible("path:main/Step/" + i + "/Loading", i > 1);
            }
            var action = page.GetButton("SureBtn"); if (action != null) action.interactable = false;
        }
        if (name == "TXProgress0Panel" && state == "reviewing")
        {
            page.SetVisible("path:main/Step/2/Done", false);
            page.SetVisible("path:main/Step/2/Loading", true);
        }
        if (name == "Tip" && state == "service-unavailable")
            page.SetText("content", "Withdrawal service is not connected. No withdrawal has been submitted.");
    }
}
