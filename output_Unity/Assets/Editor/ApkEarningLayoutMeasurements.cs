using System;
using System.Collections.Generic;
using System.IO;
using FruitApkEarning;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Read-only geometry evidence for the saved-prefab preview renderer. This does
/// not author a layout, assert expected positions, or run a player's workflow.
/// </summary>
public static class ApkEarningLayoutMeasurements
{
    private const string ReferenceRoot = "C:/Projects/DJS_Apple/apk-comparison/reference/";

    [Serializable] private sealed class Report
    {
        public string generatedUtc, page, state;
        public int width, height;
        public string origin = "top-left; pixels at the stated camera render size";
        public string scope = "Actual RectTransform corners after layout and camera rendering. Synthetic Unity preview data, not an installed-device screenshot. No expected current positions are embedded.";
        public string referenceCaveat = "Reference boxes are approximate visible painted boundaries in original 1080x1920 APK screenshots. Current boxes are full RectTransforms, including transparent sprite padding. These are different measurement types; small edge deltas are not automatically layout errors. Missing reference boxes are intentionally unmeasured.";
        public List<Entry> entries = new List<Entry>();
        public List<string> warnings = new List<string>();
    }

    [Serializable] private sealed class Entry
    {
        public string key, binding, kind, note;
        public float x, y, w, h;
        public bool active, enabled;
        public float fontSize;
        public string text;
        public List<ReferenceBox> reference = new List<ReferenceBox>();
    }

    [Serializable] private sealed class ReferenceBox
    {
        public string source, kind = "manual estimate of visible artwork; not a RectTransform", note;
        public float x, y, w, h, uncertaintyPx;
        public ReferenceBox(string source, float x, float y, float w, float h, float uncertainty, string note)
        {
            this.source = ReferenceRoot + source;
            this.x = x; this.y = y; this.w = w; this.h = h;
            uncertaintyPx = uncertainty; this.note = note;
        }
    }

    private sealed class Reader
    {
        private readonly ApkEarningPageView page;
        private readonly Camera camera;
        private readonly Vector2Int size;
        private readonly Report report;
        private readonly Vector3[] corners = new Vector3[4];

        public Reader(ApkEarningPageView page, Camera camera, Vector2Int size, Report report)
        { this.page = page; this.camera = camera; this.size = size; this.report = report; }

        public void Image(string key, string binding, string note = "")
        { Component(key, binding, page.GetImage(binding), "image", note); }
        public void Text(string key, string binding, string note = "")
        { Component(key, binding, page.GetText(binding), "text rectangle", note); }
        public void Button(string key, string binding, string note = "")
        { Component(key, binding, page.GetButton(binding), "button", note); }
        public void Input(string key, string binding, string note = "")
        { Component(key, binding, page.GetInput(binding), "input", note); }
        public void Object(string key, string binding, string note = "")
        {
            GameObject target = page.GetObject(binding);
            Rect(key, binding, target == null ? null : target.transform as RectTransform, null, "container", note);
        }

        public void Component(string key, string binding, Component target, string kind, string note = "")
        { Rect(key, binding, target == null ? null : target.transform as RectTransform, target, kind, note); }

        public void Rect(string key, string binding, RectTransform target, Component component, string kind, string note = "")
        {
            if (target == null)
            {
                report.warnings.Add(key + ": missing directly bound RectTransform " + binding);
                return;
            }
            target.GetWorldCorners(corners);
            float minX = float.PositiveInfinity, minY = float.PositiveInfinity;
            float maxX = float.NegativeInfinity, maxY = float.NegativeInfinity;
            for (int i = 0; i < corners.Length; i++)
            {
                Vector3 screen = camera.WorldToScreenPoint(corners[i]);
                minX = Mathf.Min(minX, screen.x); maxX = Mathf.Max(maxX, screen.x);
                minY = Mathf.Min(minY, screen.y); maxY = Mathf.Max(maxY, screen.y);
            }
            var entry = new Entry {
                key = key, binding = binding, kind = kind, note = note,
                x = Round(minX), y = Round(size.y - maxY), w = Round(maxX - minX), h = Round(maxY - minY),
                active = target.gameObject.activeInHierarchy,
                enabled = !(component is Behaviour behaviour) || behaviour.enabled
            };
            if (component is TMP_Text text)
            {
                entry.text = text.text;
                entry.fontSize = text.fontSize;
            }
            ReferenceBox reference = Reference(report.page, report.state, key);
            if (reference != null) entry.reference.Add(reference);
            report.entries.Add(entry);
        }

        public void Children(string parentBinding, string prefix, bool history)
        {
            GameObject parent = page.GetObject(parentBinding);
            if (parent == null)
            {
                report.warnings.Add(prefix + ": missing directly bound parent " + parentBinding);
                return;
            }
            // These are the known item/record instances beneath the serialized
            // container, not an arbitrary hierarchy or name search.
            for (int i = 0; i < parent.transform.childCount; i++)
            {
                Transform child = parent.transform.GetChild(i);
                ApkEarningPageView childPage = child.GetComponent<ApkEarningPageView>();
                if (childPage == null) continue;
                string key = prefix + "[" + i + "]";
                Rect(key, parentBinding + " child " + i, child as RectTransform, null, "instantiated row/item", "Actual direct child of the saved container; visibility reflects the captured state.");
                if (history)
                {
                    ChildText(childPage, key, "titleLabel"); ChildText(childPage, key, "timeLabel");
                    ChildText(childPage, key, "amountLabel"); ChildText(childPage, key, "statusLabel");
                    Component(key + ".action", parentBinding + " child " + i + " / actionButton", childPage.GetButton("actionButton"), "button");
                }
                else
                {
                    Component(key + ".icon", parentBinding + " child " + i + " / icon", childPage.GetImage("icon"), "image");
                    ChildText(childPage, key, "count");
                }
            }
        }

        private void ChildText(ApkEarningPageView child, string key, string binding)
        { Component(key + "." + binding, key + " / " + binding, child.GetText(binding), "text rectangle"); }
    }

    public static void Capture(ApkEarningPageView page, string name, string state,
        Camera camera, Vector2Int size, string outputDirectory)
    {
        if (size.x != 1080 || size.y != 1920 || !Representative(name, state)) return;
        if (page == null || camera == null) throw new ArgumentNullException(page == null ? nameof(page) : nameof(camera));
        if (string.IsNullOrEmpty(outputDirectory)) throw new ArgumentException("Measurement output directory is required.", nameof(outputDirectory));
        var report = new Report {
            generatedUtc = DateTime.UtcNow.ToString("O"), page = name,
            state = string.IsNullOrEmpty(state) ? "representative" : state,
            width = size.x, height = size.y
        };
        var reader = new Reader(page, camera, size, report);
        switch (name)
        {
            case "TXPanel": Cash(reader); break;
            case "TXCoinPanel": Diamond(reader); break;
            case "TXUserInfoPanel": Account(reader); break;
            case "LocalCashHistory": History(reader); break;
            case "LocalWithdrawalConditions": Conditions(reader); break;
            case "LocalSuccess": Reward(reader); break;
        }
        Directory.CreateDirectory(outputDirectory);
        string suffix = string.IsNullOrEmpty(state) ? "" : "--" + state;
        string path = Path.Combine(outputDirectory, "measurements-" + name + suffix + "-1080x1920.json");
        File.WriteAllText(path, JsonUtility.ToJson(report, true));
    }

    private static bool Representative(string name, string state)
    {
        if (!string.IsNullOrEmpty(state)) return name == "LocalWithdrawalConditions" && state == "diamond";
        return name == "TXPanel" || name == "TXCoinPanel" || name == "TXUserInfoPanel" ||
            name == "LocalCashHistory" || name == "LocalWithdrawalConditions" || name == "LocalSuccess";
    }

    private static void Cash(Reader r)
    {
        r.Image("mainCard", "main/Image"); r.Image("titlePlate", "main/Title1"); r.Text("title", "StageLabel");
        r.Image("amountCard", "main/Image (1)"); r.Text("amountCaption", "labels[0].Text");
        r.Image("cashIcon", "goldImage"); r.Text("amount", "GoldCount");
        r.Image("progressTrack", "main/Progress"); r.Image("progressFill", "Progress"); r.Text("progressText", "ProgressValue");
        r.Text("requirement", "Tip"); r.Button("action", "TXBtn"); r.Text("actionCaption", "TXText");
        r.Image("stats", "main/PlayerInfoPos/PlayerInfo/Image"); r.Text("statsTitle", "Stats.Time");
        r.Text("statsPlayersCaption", "Stats.PlayersCaption"); r.Text("statsPlayers", "Stats.PassCount");
        r.Text("statsAttemptsCaption", "Stats.AttemptsCaption"); r.Text("statsAttempts", "Stats.ChallengeTimes");
        r.Text("statsAmountCaption", "Stats.AmountCaption"); r.Text("statsAmount", "Stats.GoldGetCount");
        r.Button("history", "HistoryButton"); r.Text("historyCaption", "HistoryCaption"); r.Button("close", "Close");
    }

    private static void Diamond(Reader r)
    {
        r.Image("mainCard", "bg"); r.Image("titlePlate", "Title"); r.Text("title", "labels[0].Text");
        r.Image("balanceCard", "main/Image (1)"); r.Image("diamondIcon", "CoinIcon"); r.Text("balance", "CoinValue");
        r.Image("amountCard", "main/Image (3)"); r.Image("cashIcon", "main/Image"); r.Text("amount", "GoldValue");
        r.Text("conversion", "TipValue"); r.Image("tierCard", "main/Image (2)"); r.Text("tierCaption", "labels[1].Text");
        for (int i = 0; i < 4; i++)
        {
            r.Button("tier[" + i + "]", "options[" + i + "]");
            r.Text("tier[" + i + "].amount", "amounts[" + i + "]");
        }
        r.Image("conditions", "TXProgress"); r.Text("conditionsTitle", "TXTip"); r.Text("conditionsText", "IitemTip");
        r.Image("progressTrack", "main/TXProgress/Bar"); r.Image("progressFill", "TipBar"); r.Text("progressText", "TipBarValue");
        r.Button("action", "TXBtn"); r.Text("actionCaption", "labels[2].Text"); r.Button("close", "Close");
    }

    private static void Account(Reader r)
    {
        r.Image("mainCard", "main/Image"); r.Image("titlePlate", "main/Title"); r.Text("title", "labels[1].Text");
        for (int i = 0; i < 3; i++)
        {
            r.Button("channel[" + i + "]", "channels[" + i + "].Button");
            r.Image("channel[" + i + "].icon", "channels[" + i + "].Icon");
        }
        r.Text("instructions", "labels[0].Text"); r.Button("help", "ChannelInfoBtn");
        r.Input("name", "InputName"); r.Input("email", "InputEmail", "Inactive in phone-entry state; retained for channel-specific form coverage.");
        r.Input("dialCode", "InputAreaNumber"); r.Input("phone", "InputNumber");
        r.Button("action", "GetBtn"); r.Text("actionCaption", "labels[2].Text"); r.Button("close", "CloseBtn");
    }

    private static void History(Reader r)
    {
        r.Image("mainCard", "Card"); r.Image("titlePlate", "Heading"); r.Text("title", "titleLabel");
        r.Image("balanceCard", "Balance"); r.Image("cashIcon", "Bank"); r.Text("balanceCaption", "Caption"); r.Text("balance", "balanceLabel");
        r.Object("rows", "rowParent"); r.Children("rowParent", "row", true); r.Button("close", "closeButton");
    }

    private static void Conditions(Reader r)
    {
        r.Image("mainCard", "main/Image"); r.Image("titlePlate", "Title1"); r.Image("inset", "main/Image (1)");
        r.Text("title", "title"); r.Text("subtitle", "subtitle"); r.Text("notice", "notice"); r.Object("rows", "Requirements");
        for (int i = 0; i < 7; i++)
        {
            string key = "rows[" + i + "]";
            r.Object("row[" + i + "]", key + ".Root", "Inactive rows are still measured and explicitly flagged inactive.");
            r.Text("row[" + i + "].name", key + ".Name"); r.Text("row[" + i + "].value", key + ".Value");
            r.Text("row[" + i + "].status", key + ".Status");
        }
        r.Button("action", "action"); r.Text("actionCaption", "actionCaption"); r.Button("close", "close");
    }

    private static void Reward(Reader r)
    {
        r.Image("titlePlate", "main/Title"); r.Text("title", "labels[2].Text"); r.Object("items", "Items");
        r.Children("Items", "item", false);
        r.Button("more", "MoreGetBtn", "Ordinary settlement: Claim 10X; reference has one cash reward, current fixture has cash plus diamonds.");
        r.Text("moreCaption", "moreLabel"); r.Button("get", "GetBtn"); r.Text("getCaption", "labels[1].Text");
    }

    private static float Round(float value) { return (float)Math.Round(value, 2); }

    private static ReferenceBox Reference(string page, string state, string key)
    {
        if (page == "TXPanel")
        {
            const string source = "APK-cash-wallet.png";
            switch (key)
            {
                case "mainCard": return Box(source, 49, 214, 982, 1032, 5);
                case "titlePlate": return Box(source, 155, 123, 770, 161, 5);
                case "amountCard": return Box(source, 115, 292, 850, 328);
                case "progressTrack": return Box(source, 120, 714, 840, 73);
                case "action": return Box(source, 206, 998, 668, 192);
                case "stats": return Box(source, 58, 1256, 964, 330);
                case "history": return Box(source, 33, 1610, 1013, 119);
            }
        }
        else if (page == "TXCoinPanel")
        {
            const string source = "APK-diamond-wallet.png";
            switch (key)
            {
                case "balanceCard": return Box(source, 17, 268, 1048, 108, 5);
                case "amountCard": return Box(source, 97, 390, 885, 271);
                case "tierCard": return Box(source, 68, 800, 945, 587);
                case "tier[0]": return Box(source, 126, 884, 366, 218);
                case "tier[1]": return Box(source, 583, 884, 365, 219);
                case "tier[2]": return Box(source, 126, 1140, 366, 221);
                case "tier[3]": return Box(source, 583, 1140, 365, 221);
                case "conditions": return Box(source, 62, 1396, 946, 278);
                case "action": return Box(source, 158, 1684, 764, 171);
            }
        }
        else if (page == "LocalWithdrawalConditions" && state == "diamond")
        {
            const string source = "APK-diamond-requirements.png";
            switch (key)
            {
                case "mainCard": return Box(source, 49, 214, 981, 1030);
                case "titlePlate": return Box(source, 155, 124, 768, 160);
                case "inset": return Box(source, 64, 322, 952, 910, 5);
                case "row[0]": return Box(source, 120, 442, 840, 76);
                case "row[1]": return Box(source, 120, 524, 840, 76);
                case "row[2]": return Box(source, 120, 606, 840, 76);
                case "row[3]": return Box(source, 120, 688, 840, 76);
                case "action": return Box(source, 182, 1067, 716, 138);
            }
            // The reference shows four rows. Do not compare the seven-row
            // production container's reserved height with this visible area.
        }
        // The remaining pages are measured without guessed reference-control
        // rectangles. Their screenshot evidence can be annotated separately.
        return null;
    }

    private static ReferenceBox Box(string source, float x, float y, float w, float h, float uncertainty = 3)
    { return new ReferenceBox(source, x, y, w, h, uncertainty, "Measured from the original full 1080x1920 PNG; visible edge, not its original Unity component bounds."); }
}
