using System;
using FruitApkEarning;
using Module.Setting;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Checks the saved flow additions without rebuilding the approved artwork.</summary>
public static class ApkEarningFlowPrefabValidation
{
    private const string Panels = "Assets/ApkEarningReference/Resources/ApkEarningReference/Panels/";
    private const string Settings = "Assets/res/local/pops/setting/GameSettingUI.prefab";

    [MenuItem("Tools/Apk Earning/Validate Flow Additions")]
    public static void Validate()
    {
        var settingsPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Settings);
        Require(settingsPrefab != null, "Missing settings prefab.");
        var settings = settingsPrefab.GetComponent<GameSettingUI>();
        Require(settings != null, "Missing GameSettingUI component.");
        var serialized = new SerializedObject(settings);
        var property = serialized.FindProperty("m_AccountLevelBtn");
        var accountButton = property == null ? null : property.objectReferenceValue as Button;
        CheckButton(accountButton, "Settings account level");
        Require(accountButton.gameObject.activeSelf, "Account level entry must be active.");
        Require(accountButton.GetComponent<InputMono>() == null,
            "Account level entry must not retain the old pointer handler.");
        var label = accountButton.GetComponentInChildren<TMP_Text>();
        Require(label != null && label.font != null && label.text.Contains("Account"),
            "Account level entry requires its saved label and font.");

        var rank = Page("LocalUserLevel");
        Texts(rank, "Title", "levelLabel", "progressLabel", "ProgressCaption", "Caption");
        CheckButton(rank.GetButton("closeButton"), "Account level close");
        CheckButton(rank.GetButton("continueButton"), "Account level continue");
        Require(rank.GetImage("progressFill") != null, "Missing account level progress.");

        var review = Page("TXTooFastHintPanel");
        Texts(review, "labels[0].Text", "labels[1].Text", "labels[2].Text",
            "LevelTip", "VideoTip", "LevelPregressValue", "VideoPregressValue");
        CheckButton(review.GetButton("IKnowBtn"), "Review continue");
        Require(review.GetImage("LevelPregress") != null && review.GetImage("VideoPregress") != null,
            "Missing review progress bars.");

        CheckSteps(Page("LocalWithdrawalVerification"), 3);
        CheckSteps(Page("TXProgress0Panel"), 4, 2);
        var sequence = Page("LocalEarningSequence");
        for (int i = 0; i < 5; i++)
        {
            Require(sequence.GetObject("bonusRows[" + i + "]") != null, "Missing saved bonus row " + i);
            Require(sequence.GetObject("flightIcons[" + i + "]") != null, "Missing saved receipt icon " + i);
        }
        var video = Page("LocalRewardVideoTest");
        Texts(video, "title", "body", "actionCaption", "cancelCaption");
        CheckButton(video.GetButton("retry"), "Test video complete");
        CheckButton(video.GetButton("close"), "Test video cancel");
        Debug.Log("[ApkEarningFlowPrefabValidation] Saved settings, account level, review, step and sequence bindings passed.");
    }

    private static ApkEarningPageView Page(string name)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Panels + name + ".prefab");
        Require(prefab != null, "Missing page " + name);
        var view = prefab.GetComponent<ApkEarningPageView>();
        Require(view != null, "Missing page bindings " + name);
        return view;
    }

    private static void Texts(ApkEarningPageView view, params string[] keys)
    {
        foreach (string key in keys)
        {
            var text = view.GetText(key);
            Require(text != null && text.font != null, view.name + ": missing text/font " + key);
        }
    }

    private static void CheckSteps(ApkEarningPageView view, int count, int doneCount = 3)
    {
        CheckButton(view.GetButton("SureBtn"), view.name + " confirm");
        for (int i = 1; i <= count; i++)
        {
            string prefix = "path:main/Step/" + i;
            if (i <= doneCount) Require(view.GetObject(prefix + "/Done") != null, view.name + ": missing done step " + i);
            Require(view.GetObject(prefix + "/Loading") != null, view.name + ": missing loading step " + i);
        }
    }

    private static void CheckButton(Button button, string context)
    {
        Require(button != null, context + ": missing standard Button.");
        Require(button.targetGraphic != null && button.targetGraphic.transform.IsChildOf(button.transform),
            context + ": button must own its visual.");
        Require(button.onClick.GetPersistentEventCount() == 0, context + ": events must be bound by code.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
