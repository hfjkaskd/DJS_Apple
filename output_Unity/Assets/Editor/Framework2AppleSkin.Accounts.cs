using UnityEngine;
using UnityEngine.UI;

// Prefab authoring for the approved account, task and record boards (03-06, 10).
// All bound source names remain intact; source controllers still own values and actions.
public static partial class Framework2AppleSkin
{
    static readonly Color AccBrown = new Color32(91, 38, 18, 255);
    static readonly Color AccBlue = new Color32(0, 61, 178, 255);

    static void ApplyAccounts(GameObject root, string prefabName)
    {
        if (root.GetComponent<Framework2AppleSkinMarker>() == null) root.AddComponent<Framework2AppleSkinMarker>();
        switch (prefabName)
        {
            case "WithDraw": AccWithdraw(root); break;
            case "WithDrawmethod": AccMethod(root); break;
            case "WithDrawConfirm": AccConfirm(root); break;
            case "WithDrawQues": AccQuestion(root); break;
            case "WithDrawProcess1": AccProcess(root); break;
            case "WithDrawHistory": AccHistory(root); break;
            case "WithDrawContinue": AccContinue(root); break;
            case "PayPalErrorView": AccError(root); break;
            case "WithdrawView": AccDiamond(root); break;
            case "historyItem": AccHistoryItem(root, false); break;
            case "historyItemBlue": AccHistoryItem(root, true); break;
            case "WithDrawItem": AccTaskItem(root.transform, 0); break;
            case "priceItem": AccPriceItem(root); break;
        }
        AccBindPresentation(root, prefabName);
    }

    static void AccWithdraw(GameObject root)
    {
        var panel = AccShell(root, "Panel", 1120, 1430, "Withdraw", "close");
        AccHide(root, "pos");
        AccHide(root, "Panel/title"); // Source still refreshes its ordinal; the approved header is stable.
        var top = AccRect(root, "Panel/PanelA", 0, 365, 960, 430);
        AccArt(top, "inset");
        AccRect(root, "Panel/PanelA/PanelD", 0, 0, 960, 430);
        AccRect(root, "Panel/PanelA/PanelD/PanelE", 0, 70, 900, 260);
        AccArt(AccRect(root, "Panel/PanelA/PanelD/PanelE/Image", -315, 15, 230, 230), "money", true);
        AccStyle(root, "Panel/PanelA/PanelD/PanelE/content", 115, 65, 590, 145, 103, TextAnchor.MiddleLeft);
        AccStyle(root, "Panel/PanelA/PanelD/PanelE/AccountVerified", 110, -55, 570, 95, 38, TextAnchor.MiddleLeft);
        AccHide(root, "Panel/PanelA/PanelD/WithdrawableBalance");
        AccRect(root, "Panel/PanelA/PanelD/Process", 0, -145, 890, 88);
        AccProgress(root, "Panel/PanelA/PanelD/Process", "Background", "Fill", "num", 890, 88);

        var middle = AccRect(root, "Panel/PanelB", 0, -50, 960, 320);
        AccArt(middle, "inset");
        AccButton(root, "Panel/PanelB/RecordBtn", -320, 90, 260, 100, false, 42);
        AccStyle(root, "Panel/PanelB/withdrawProcess", 170, 88, 540, 118, 43, TextAnchor.MiddleCenter);
        AccButton(root, "Panel/PanelB/WithDraw", 0, -70, 900, 170, true, 66);

        var lucky = AccRect(root, "Panel/PanelC", 0, -425, 960, 300);
        AccArt(lucky, "inset");
        AccStyle(root, "Panel/PanelC/Tips", 120, 0, 650, 180, 50, TextAnchor.MiddleCenter);
        AccStyle(root, "Panel/PanelC/Lucky", 160, 95, 540, 60, 40);
        AccStyle(root, "Panel/PanelC/CurLuckyCoin", 180, 25, 530, 100, 80);
        AccButton(root, "Panel/PanelC/CashOut", 180, -80, 500, 126, true, 46);
        AccHide(root, "Panel/PanelC/ADD");
        AccHide(root, "Panel/PanelC/WithdrawAnytime");
        AccAddArt(lucky, "AppleLockedIcon", "lock", -320, 0, 180, 190);
        AccAddArt(lucky, "AppleWalletIcon", "wallet", -310, 0, 240, 240);
        // Runtime binds these two authored images to the same existing unlock condition.
        lucky.Find("AppleWalletIcon").gameObject.SetActive(false);

        AccToast(root, "PanelE", "Complete the current test task first.");
        AccFooter(panel, 1430);
    }

    static void AccMethod(GameObject root)
    {
        var panel = AccShell(root, "PanelA", 1120, 1260, "Test account", "close");
        AccHide(root, "PanelA/Bg");
        AccHide(root, "PanelA/title");
        AccHide(root, "PanelA/method");
        var methods = AccRect(root, "PanelA/PanelB", 0, 295, 960, 360);
        AccClearImage(methods);
        string[] methodNames = { "ToggleA", "ToggleB", "ToggleC" };
        string[] methodLabels = { "Bank\n(Test)", "Card\n(Test)", "PayPal\n(Test)" };
        for (int i = 0; i < methodNames.Length; i++)
        {
            string path = "PanelA/PanelB/" + methodNames[i];
            var toggle = AccRect(root, path, -325 + i * 325, 0, 300, 360);
            toggle.gameObject.SetActive(true);
            AccClearImage(toggle);
            var bg = AccRect(root, path + "/Background", 0, 0, 300, 360);
            // Existing source channel artwork is preserved inside a newly authored card.
            var existingLogo = toggle.Find("AppleChannelLogo");
            Sprite channelSprite = existingLogo != null ? existingLogo.GetComponent<Image>().sprite :
                bg.GetComponent<Image>() != null ? bg.GetComponent<Image>().sprite : null;
            AccArt(bg, "inset");
            bg.GetComponent<Image>().color = i == 2 ? new Color(.91f, .98f, 1f) : new Color(.88f, .87f, .82f);
            var logo = AccAddArt(toggle, "AppleChannelLogo", null, 0, 80, 125, 115);
            if (logo != null) { logo.sprite = i == 0 ? SkinSprite("bank") : i == 1 ? SkinSprite("card") : channelSprite; logo.preserveAspect = true; }
            AccNewText(toggle, "AppleChannelLabel", methodLabels[i], 0, -50, 260, 100, 42, i == 2 ? AccBlue : new Color(.48f, .46f, .42f));
            var check = AccNode(bg, "Checkmark"); AccRect(check, 0, -145, 62, 62);
            check.gameObject.SetActive(true);
            AccArt(check, "check", true);
            var selector = toggle.GetComponent<Toggle>();
            if (selector != null)
            {
                var surface = bg.GetComponent<Image>(); surface.raycastTarget = true;
                selector.targetGraphic = surface; selector.graphic = check.GetComponent<Image>();
            }
        }
        AccRect(root, "PanelA/Panel", 0, -10, 960, 110);
        AccClearImage(root.transform.Find("PanelA/Panel"));
        AccButton(root, "PanelA/Panel/ques", 425, 0, 92, 92, true, 52);
        AccNewText(root.transform.Find("PanelA/Panel/ques"), "AppleQuestionMark", "?", 0, 0, 70, 70, 57, Color.white);
        AccNewText(panel, "AppleInputLabel", "Test email", -110, -10, 720, 76, 53, AccBrown, TextAnchor.MiddleLeft);
        var inputs = AccRect(root, "PanelA/PanelC", 0, -125, 960, 150);
        AccClearImage(inputs);
        AccInput(root, "PanelA/PanelC/Email", 0, 0, 940, 150);
        var errorBorder = AccAddArt(inputs.Find("Email"), "AppleInputErrorBorder", "inset", 0, 0, 950, 160);
        errorBorder.type = Image.Type.Sliced; errorBorder.preserveAspect = false;
        errorBorder.fillCenter = false; errorBorder.color = new Color(1f, .06f, .04f); errorBorder.enabled = false;
        AccInput(root, "PanelA/PanelC/Name", 0, 0, 940, 150);
        AccHide(root, "PanelA/PanelC/Name");
        AccRect(root, "PanelA/PanelC/phoneFront", -360, 0, 200, 150);
        AccInput(root, "PanelA/PanelC/phoneBehind", 115, 0, 710, 150);
        AccNewText(panel, "AppleAccountNotice", "Use fictitious details. No payment is sent.", 0, -285, 960, 65, 34, AccBrown);
        AccButton(root, "PanelA/Confirm", 0, -435, 960, 170, true, 69);
        var phoneMethod = AccNode(panel, "ApplePhoneMethod");
        AccRect(phoneMethod, 0, 295, 300, 360); AccArt(phoneMethod, "inset");
        var handset = AccAddArt(phoneMethod, "Handset", "phone", 0, 80, 110, 155);
        handset.type = Image.Type.Simple; handset.preserveAspect = true;
        AccNewText(phoneMethod, "Label", "Phone\n(Test)", 0, -65, 270, 100, 42, AccBlue);
        AccAddArt(phoneMethod, "Selected", "check", 0, -145, 62, 62);
        phoneMethod.gameObject.SetActive(false);

        var pix = AccRect(root, "PanelA/PanelC/BrazilPixControls", 0, 310, 960, 480);
        AccStyle(root, "PanelA/PanelC/BrazilPixControls/Title", 0, 190, 940, 75, 40);
        AccStyle(root, "PanelA/PanelC/BrazilPixControls/KeyLabel", -110, -170, 720, 70, 45, TextAnchor.MiddleLeft);
        AccHide(root, "PanelA/PanelC/BrazilPixControls/KeyLabel");
        string[] pixNames = { "CpfCnpj", "Phone", "Email", "Random" };
        for (int i = 0; i < pixNames.Length; i++)
        {
            string path = "PanelA/PanelC/BrazilPixControls/" + pixNames[i];
            var item = AccRect(root, path, -360 + i * 240, 20, 222, 270);
            AccArt(item, "inset");
            AccStyle(root, path + "/Label", 0, 5, 206, 85, 33);
            AccArt(AccRect(root, path + "/Selection", 0, -90, 65, 65), "check", true);
        }
        AccStyle(root, "PanelA/PanelC/BrazilPixControls/Error", 0, -425, 940, 68, 37, TextAnchor.MiddleLeft);
        AccToast(root, "PanelE", "Check dummy account format.");
        // This is a nonblocking field error placed at the same location as board 03.
        AccRect(root, "PanelE", 0, -270, 960, 75);
        AccRect(root, "PanelE/Image", 0, 0, 960, 75);
        AccRect(root, "PanelE/Image/content", 0, 0, 940, 70);
        AccFooter(panel, 1260);
    }

    static void AccConfirm(GameObject root)
    {
        var panel = AccShell(root, "PanelA", 1120, 1260, "Test account", "close");
        AccHide(root, "PanelA/Bg");
        AccHide(root, "PanelA/title");
        var card = AccRect(root, "PanelA/PanelC", 0, 180, 960, 540);
        AccArt(card, "inset");
        AccHide(root, "PanelA/PanelC/LevelProgressTxt");
        AccRect(root, "PanelA/method", 0, 310, 220, 210);
        AccStyle(root, "PanelA/PanelC/YourAccount", 0, -30, 880, 85, 50);
        AccStyle(root, "PanelA/PanelC/phone", 0, -155, 910, 130, 57);
        var phone = root.transform.Find("PanelA/PanelC/phone").GetComponent<Text>();
        if (phone != null) phone.color = AccBlue;
        AccButton(root, "PanelA/ReEnter", 0, -230, 960, 156, false, 64);
        AccButton(root, "PanelA/Confirm", 0, -425, 960, 174, true, 70);
        AccFooter(panel, 1260);
    }

    static void AccQuestion(GameObject root)
    {
        var panel = AccShell(root, "PanelA", 1120, 1240, "Test account method", "close");
        AccHide(root, "PanelA/title");
        AccRect(root, "PanelA/PanelC", 0, 260, 930, 330);
        AccClearImage(root.transform.Find("PanelA/PanelC"));
        AccStyle(root, "PanelA/PanelC/content", 0, 0, 910, 325, 48);
        AccNewText(panel, "AppleInputLabel", "phone/email", 0, -30, 940, 85, 53, AccBrown, TextAnchor.MiddleLeft);
        AccInput(root, "PanelA/phone", 0, -170, 960, 156);
        AccButton(root, "PanelA/Confirm", 0, -430, 960, 176, true, 69);
        AccFooter(panel, 1240);
    }

    static void AccProcess(GameObject root)
    {
        var panel = AccShell(root, "PanelA", 1120, 1440, "Saving test request", "close");
        AccHide(root, "Panel");
        var total = AccRect(root, "PanelA/PanelB", 0, 430, 960, 240);
        AccArt(total, "inset");
        AccArt(AccRect(root, "PanelA/PanelB/money", -320, 0, 210, 210), "money", true);
        AccStyle(root, "PanelA/PanelB/content", 120, 30, 600, 155, 109, TextAnchor.MiddleLeft);
        AccNewText(total, "AppleLocalCaption", "Local simulation", 115, -70, 600, 60, 34, AccBrown, TextAnchor.MiddleLeft);
        var statuses = AccRect(root, "PanelA/PanelC", 0, -20, 960, 580);
        AccClearImage(statuses);
        AccHide(root, "PanelA/PanelC/Current");
        AccRect(root, "PanelA/PanelC/StateRoot", 0, 0, 960, 540);
        for (int i = 1; i <= 5; i++)
        {
            string p = "PanelA/PanelC/StateRoot/State" + i;
            var row = AccRect(root, p, 0, 220 - (i - 1) * 115, 940, 100);
            AccClearImage(row);
            var bg = AccAddArt(row, "AppleStateBackground", "inset", 70, 0, 800, 100);
            bg.color = new Color(.91f, .97f, .85f, 1);
            bg.transform.SetAsFirstSibling();
            AccArt(AccRect(root, p + "/Finsh", -390, 0, 94, 94), "check", true);
            AccRect(root, p + "/wait", -390, 0, 94, 94);
            if (row.Find("content1") == null)
                AccNewText(row, "content1", "", 80, 0, 760, 90, 42, AccBrown, TextAnchor.MiddleLeft);
            AccStyle(root, p + "/content1", 80, 0, 760, 90, 42, TextAnchor.MiddleLeft);
            var line = row.Find("line");
            if (line != null)
            {
                // State4/5's legacy line was a text duplicate, not a connector.
                if (line.GetComponent<Text>() != null) AccText(line.GetComponent<Text>(), 1, TextAnchor.MiddleCenter, new Color(0, 0, 0, 0));
                else { AccRect(line, -390, -58, 8, 35); var im = line.GetComponent<Image>(); if (im != null) im.color = new Color(.77f, .67f, .48f); }
            }
        }
        AccStyle(root, "PanelA/PanelC/Dec", 0, -360, 930, 100, 37);
        AccButton(root, "PanelA/Continue", 0, -525, 960, 165, true, 64);
        AccFooter(panel, 1440);
    }

    static void AccHistory(GameObject root)
    {
        var panel = AccRect(root, "Panel", 0, -35, 1120, 1460);
        AccClearImage(panel);
        var bg = AccRect(root, "Panel/bg", 0, 0, 1120, 1460);
        AccArt(bg, "panel");
        AccHide(root, "Panel/bg/title");
        AccHeader(bg, 1120, 1460, "Local test records", "close");
        AccRect(root, "Panel/PanelA", 0, 450, 960, 280);
        AccArt(root.transform.Find("Panel/PanelA"), "inset");
        AccNewText(root.transform.Find("Panel/PanelA"), "AppleTotalLabel", "Total (local simulation)", 0, 90, 900, 70, 43, AccBrown);
        AccArt(AccRect(root, "Panel/PanelA/money", -230, -30, 190, 175), "money", true);
        AccStyle(root, "Panel/PanelA/Num", 145, -20, 570, 150, 117);
        var scroll = AccRect(root, "Panel/Scroll View", 0, -155, 960, 870);
        AccArt(scroll, "inset");
        var viewport = AccRect(root, "Panel/Scroll View/Viewport", 0, 0, 930, 840);
        var dragSurface = viewport.GetComponent<Image>();
        if (dragSurface == null) dragSurface = viewport.gameObject.AddComponent<Image>();
        dragSurface.color = Color.clear; dragSurface.raycastTarget = true;
        var viewportMask = viewport.GetComponent<Mask>();
        if (viewportMask != null) { viewportMask.showMaskGraphic = false; dragSurface.color = Color.white; }
        var content = root.transform.Find("Panel/Scroll View/Viewport/Content") as RectTransform;
        if (content != null)
        {
            content.anchorMin = new Vector2(0, 1); content.anchorMax = Vector2.one;
            content.pivot = new Vector2(.5f, 1); content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero; content.localScale = Vector3.one;
            var vertical = content.GetComponent<VerticalLayoutGroup>();
            if (vertical == null) vertical = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vertical.enabled = true; vertical.spacing = 26; vertical.padding = new RectOffset(12, 12, 18, 18);
            vertical.childControlHeight = true; vertical.childControlWidth = true;
            vertical.childForceExpandHeight = false; vertical.childForceExpandWidth = true;
            var fitter = content.GetComponent<ContentSizeFitter>();
            if (fitter == null) fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.enabled = true; fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        }
        AccHide(root, "Panel/Scroll View/Scrollbar Horizontal");
        AccHide(root, "Panel/Button");
        var empty = AccNode(panel, "AppleEmptyState");
        AccRect(empty, 0, -155, 900, 750);
        var oldReceiptText = empty.Find("Receipt")?.GetComponent<Text>();
        if (oldReceiptText != null) Object.DestroyImmediate(oldReceiptText);
        AccAddArt(empty, "Receipt", "receipt", 0, 95, 420, 350);
        AccNewText(empty, "Label", "No records yet.", 0, -150, 800, 100, 63, AccBrown);
        AccFooter(panel, 1460);
    }

    static void AccHistoryItem(GameObject root, bool wallet)
    {
        AccRect(root.transform, 0, 0, 900, 350);
        var layout = root.GetComponent<LayoutElement>();
        if (layout == null) layout = root.AddComponent<LayoutElement>();
        layout.preferredHeight = 350; layout.minHeight = 350;
        AccArt(root.transform, "inset");
        AccRect(root, "BG", 0, 0, 900, 350);
        AccClearImage(root.transform.Find("BG"));
        var color = wallet ? new Color(.88f, .96f, 1f) : new Color(.91f, 1f, .83f);
        var image = root.GetComponent<Image>(); image.color = color;
        var button = root.GetComponent<Button>(); if (button != null) button.targetGraphic = image;
        AccAddArt(root.transform, "AppleRecordIcon", wallet ? "wallet" : "money", -325, 15, 210, 230);
        AccStyle(root, "ContentB", -30, 100, 360, 100, 76, TextAnchor.MiddleLeft);
        AccStyle(root, "titleB", 250, 106, 310, 80, 39);
        AccStyle(root, "ContentA", 100, 0, 590, 65, 38, TextAnchor.MiddleLeft);
        AccStyle(root, "titleA", 100, -60, 590, 65, 35, TextAnchor.MiddleLeft);
        AccStyle(root, "Accountbalance", 100, -125, 590, 65, 36, TextAnchor.MiddleLeft);
        AccLiteral(root, "Accountbalance", "Saved on this device.");
        AccHide(root, "Cash");
    }

    static void AccError(GameObject root)
    {
        var panel = AccShell(root, "PanelA", 1120, 1380, "Test request failed", "close");
        AccHide(root, "PanelA/title");
        AccRect(root, "PanelA/PanelB", 0, 250, 960, 500);
        AccArt(root.transform.Find("PanelA/PanelB"), "inset");
        AccArt(AccRect(root, "PanelA/PanelB/method", 0, 140, 170, 170), "error", true);
        AccStyle(root, "PanelA/PanelB/CurLuckyCoin", 0, -20, 880, 180, 130);
        AccHide(root, "PanelA/PanelB/content");
        AccHide(root, "PanelA/PanelB/YourAccount");
        AccRect(root, "PanelA/PanelC", 0, -20, 940, 100);
        AccClearImage(root.transform.Find("PanelA/PanelC"));
        AccStyle(root, "PanelA/PanelC/email", 0, 0, 920, 90, 49);
        AccNewText(panel, "AppleErrorExplanation", "Check the dummy account format and submit the local test again.", 0, -200, 900, 220, 48, AccBrown);
        AccButton(root, "PanelA/ReEnter", 0, -370, 960, 165, true, 65);
        AccButton(root, "PanelA/Confirm", 0, -535, 960, 130, false, 55);
        AccFooter(panel, 1380);
    }

    static void AccContinue(GameObject root)
    {
        var panel = AccShell(root, "PanelA", 1120, 1430, "Withdrawal", null);
        AccHide(root, "Image");
        AccHide(root, "PanelA/title");
        AccStyle(root, "PanelA/ContentA", 0, 425, 960, 230, 44);
        AccHide(root, "PanelA/ContentB");
        var tasks = AccRect(root, "PanelA/PanelC", 0, -5, 960, 690);
        AccClearImage(tasks);
        string[] names = { "WithDrawItem", "WithDrawItem1", "WithDrawItem2" };
        for (int i = 0; i < names.Length; i++)
        {
            var item = tasks.Find(names[i]);
            AccTaskItem(item, i + 1);
            AccRect(item, 0, 230 - 230 * i, 960, 205);
        }
        AccButton(root, "PanelA/Continue", 0, -490, 960, 180, true, 70);
        AccFooter(panel, 1430);
    }

    static void AccTaskItem(Transform item, int number)
    {
        if (item == null) return;
        AccRect(item, 0, 0, 960, 205);
        AccArt(item, "inset");
        var head = item.Find("headPic");
        AccRect(head, -360, 0, 140, 140);
        AccArt(head, number == 1 ? "check" : "goldbadge", true);
        var name = head != null ? head.Find("name") : null;
        if (name != null) name.gameObject.SetActive(false);
        if (number > 0)
        {
            var numberText = AccNewText(head, "AppleTaskNumber", number.ToString(), 0, 0, 120, 120, 67, AccBrown);
            numberText.gameObject.SetActive(number != 1);
        }
        var content = item.Find("PanelA");
        AccRect(content, 100, 0, 700, 180);
        AccClearImage(content);
        AccStyle(item.gameObject, "PanelA/TextUp", 0, 43, 650, 90, 62, TextAnchor.MiddleLeft);
        AccStyle(item.gameObject, "PanelA/TextDown", 0, -50, 650, 90, 41, TextAnchor.MiddleLeft);
        AccBindTaskPresentation(item.gameObject);
    }

    static void AccDiamond(GameObject root)
    {
        AccRect(root, "Plane", 0, 0, 1200, 2132);
        var panel = AccShell(root, "Plane/Bg", 1120, 1460, "Diamond exchange", "ExitBtn");
        AccHide(root, "Plane/Bg/TopTitle");
        AccRect(root, "Plane/Bg/Bg2", 0, 470, 960, 130);
        AccClearImage(root.transform.Find("Plane/Bg/Bg2"));
        AccStyle(root, "Plane/Bg/Bg2/CurGem", 50, 0, 350, 100, 66);
        var gems = root.transform.Find("Plane/Bg/Bg2").GetComponentsInChildren<Image>(true);
        foreach (Image gem in gems) if (gem.name == "GemIcon") AccRect(gem.transform, -170, 0, 100, 100);
        AccHide(root, "Plane/Bg/Bg2/CashCount");
        AccStyle(root, "Plane/Bg/Bg2/mark", 0, -780, 960, 70, 38);
        AccRect(root, "Plane/Bg/WithdrawalTask", 0, 70, 960, 620);
        AccClearImage(root.transform.Find("Plane/Bg/WithdrawalTask"));
        AccHide(root, "Plane/Bg/WithdrawalTask/WithdrawalTaskTitle");
        var group = AccRect(root, "Plane/Bg/WithdrawalTask/WTGroup", 0, 0, 960, 620);
        for (int i = 0; i < 4; i++)
        {
            var item = group.Find("WTToggleItem" + (i + 1));
            AccRect(item, i % 2 == 0 ? -245 : 245, i < 2 ? 165 : -165, 465, 300);
            AccArt(item.Find("Bg"), "inset"); AccRect(item.Find("Bg"), 0, 0, 465, 300);
            AccRect(item.Find("Checkmark"), 0, 0, 465, 300);
            AccArt(item.Find("Checkmark"), "inset");
            if (item.Find("Checkmark")?.GetComponent<Image>() is Image selected) selected.color = new Color(.82f, .98f, 1f, .65f);
            var selector = item.GetComponent<Toggle>();
            if (selector != null)
            {
                var surface = item.Find("Bg").GetComponent<Image>(); surface.raycastTarget = true;
                selector.targetGraphic = surface; selector.graphic = item.Find("Checkmark").GetComponent<Image>();
            }
            AccStyle(item.gameObject, "WTTI2Label", 50, -65, 340, 100, 53);
            AccAddArt(item, "AppleExchangeMoney", "money", -150, -65, 100, 100);
            AccAddArt(item, "AppleExchangeDiamond", "diamond", -150, 65, 100, 100);
            // These labels are tied to the existing serialized source exchange-cost array by the host binder.
            AccNewText(item, "AppleGemCost", "", 50, 65, 340, 100, 53, AccBrown);
        }
        AccHide(root, "Plane/Bg/icon");
        var bottom = AccRect(root, "Plane/Bg/BottomBg", 0, -410, 960, 100);
        AccClearImage(bottom);
        AccHide(root, "Plane/Bg/BottomBg/Withdrawal");
        AccHide(root, "Plane/Bg/BottomBg/juli");
        AccRect(root, "Plane/Bg/BottomBg/Process", 0, 0, 960, 100);
        AccProgress(root, "Plane/Bg/BottomBg/Process", "Background", "Fill Area/Fill", "num", 960, 100);
        AccRect(root, "Plane/Bg/BottomBg/Process/Fill Area", 0, 0, 940, 80);
        AccButton(root, "Plane/Bg/Withdraw", 0, -550, 960, 150, true, 65);
        AccFooter(panel, 1460);
    }

    static void AccPriceItem(GameObject root)
    {
        AccRect(root.transform, 0, 0, 300, 260);
        AccArt(root.transform, "inset");
        AccStyle(root, "textUp", 0, 65, 280, 90, 31);
        AccStyle(root, "TextDown", 0, -50, 280, 100, 52);
    }

    static void AccBindPresentation(GameObject root, string prefabName)
    {
        if (prefabName == "WithDrawItem") return; // Bound independently for embedded and standalone items.
        var refs = new Framework2AppleSkinPresenter.Bindings();
        switch (prefabName)
        {
            case "WithDraw":
                refs.LockedIcon = AccObject(root, "Panel/PanelC/AppleLockedIcon");
                refs.WalletIcon = AccObject(root, "Panel/PanelC/AppleWalletIcon");
                refs.LuckyTitle = AccObject(root, "Panel/PanelC/Lucky");
                break;
            case "WithDrawmethod":
                refs.StandardMethods = AccObject(root, "PanelA/PanelB");
                refs.PixMethods = AccObject(root, "PanelA/PanelC/BrazilPixControls");
                refs.PhoneMethod = AccObject(root, "PanelA/ApplePhoneMethod");
                refs.InputLabel = AccComponent<Text>(root, "PanelA/AppleInputLabel");
                refs.EmailLabel = "Test email"; refs.PhoneLabel = "Test phone"; refs.PixLabel = "Dummy Pix key";
                refs.InputErrorBorder = AccComponent<Image>(root, "PanelA/PanelC/Email/AppleInputErrorBorder");
                break;
            case "WithDrawProcess1":
                refs.Header = AccComponent<Text>(root, "PanelA/AppleHeaderText");
                refs.SavingTitle = "Saving test request"; refs.RecordedTitle = "Recorded locally";
                refs.FailedTitle = "Test request failed"; refs.ReviewTitle = "Withdraw";
                break;
            case "WithDrawHistory": refs.EmptyHistory = AccObject(root, "Panel/AppleEmptyState"); break;
            case "WithdrawView":
                refs.GemCostLabels = new Text[4];
                for (int i = 0; i < refs.GemCostLabels.Length; i++)
                    refs.GemCostLabels[i] = AccComponent<Text>(root, "Plane/Bg/WithdrawalTask/WTGroup/WTToggleItem" + (i + 1) + "/AppleGemCost");
                break;
            default: return;
        }
        var presenter = root.GetComponent<Framework2AppleSkinPresenter>();
        if (presenter == null) presenter = root.AddComponent<Framework2AppleSkinPresenter>();
        presenter.Configure(refs);
    }

    static void AccBindTaskPresentation(GameObject root)
    {
        var presenter = root.GetComponent<Framework2AppleSkinPresenter>();
        if (presenter == null) presenter = root.AddComponent<Framework2AppleSkinPresenter>();
        presenter.Configure(new Framework2AppleSkinPresenter.Bindings
        {
            TaskBackground = root.GetComponent<Image>(), TaskBadge = AccComponent<Image>(root, "headPic"),
            TaskNumber = AccComponent<Text>(root, "headPic/AppleTaskNumber"),
            TaskNormalTint = Color.white, TaskCompletedTint = new Color(.85f, 1f, .77f)
        });
    }

    static GameObject AccObject(GameObject root, string path)
    {
        var child = root.transform.Find(path); return child != null ? child.gameObject : null;
    }

    static T AccComponent<T>(GameObject root, string path) where T : Component
    {
        var child = root.transform.Find(path); return child != null ? child.GetComponent<T>() : null;
    }

    static RectTransform AccShell(GameObject root, string path, float width, float height, string title, string close)
    {
        var panel = AccRect(root, path, 0, -35, width, height);
        AccArt(panel, "panel");
        AccHeader(panel, width, height, title, close);
        return panel;
    }

    static void AccHeader(Transform panel, float width, float height, string title, string close)
    {
        AccAddArt(panel, "AppleHeader", "title", -20, height * .5f - 12, 820, 168);
        AccNewText(panel, "AppleHeaderText", title, -20, height * .5f - 10, 770, 140, title.Length > 19 ? 62 : 76, Color.white);
        var heading = panel.Find("AppleHeaderText").GetComponent<Text>();
        var outline = heading.GetComponent<Outline>();
        if (outline == null) outline = heading.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(.06f, .18f, .55f); outline.effectDistance = new Vector2(2, -2);
        if (close != null && panel.Find(close) != null)
        {
            var button = panel.Find(close);
            AccRect(button, width * .5f - 64, height * .5f - 45, 146, 146);
            AccArt(button, "close", true);
            button.SetAsLastSibling();
        }
    }

    static void AccFooter(Transform panel, float height) =>
        AccNewText(panel, "AppleTestFooter", "TEST MODE · NO CASH PAYMENT", 0, -height * .5f + 58, 990, 64, 33, AccBrown);

    static RectTransform AccRect(GameObject root, string path, float x, float y, float w, float h) =>
        AccRect(root.transform.Find(path), x, y, w, h);

    static RectTransform AccRect(Transform t, float x, float y, float w, float h)
    {
        if (t == null) return null;
        var rect = t as RectTransform;
        if (rect == null) return null;
        foreach (var layout in t.GetComponents<LayoutGroup>()) layout.enabled = false;
        var fitter = t.GetComponent<ContentSizeFitter>(); if (fitter != null) fitter.enabled = false;
        rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
        rect.pivot = new Vector2(.5f, .5f); rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(w, h); rect.localScale = Vector3.one;
        return rect;
    }

    static RectTransform AccNode(Transform parent, string name)
    {
        var t = parent.Find(name) as RectTransform;
        if (t != null) return t;
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false); return (RectTransform)go.transform;
    }

    static void AccStyle(GameObject root, string path, float x, float y, float w, float h, int size,
        TextAnchor alignment = TextAnchor.MiddleCenter)
    {
        var rect = AccRect(root, path, x, y, w, h);
        if (rect != null) AccText(rect.GetComponent<Text>(), size, alignment, AccBrown);
    }

    static void AccText(Text text, int size, TextAnchor alignment, Color color)
    {
        if (text == null) return;
        text.font = SkinFont;
        text.fontSize = size; text.resizeTextForBestFit = true;
        text.resizeTextMinSize = Mathf.Max(20, size - 12); text.resizeTextMaxSize = size;
        text.alignment = alignment; text.color = color; text.fontStyle = FontStyle.Bold;
        text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Truncate;
        text.raycastTarget = false;
        foreach (var shadow in text.GetComponents<Shadow>()) shadow.enabled = false;
    }

    static Text AccNewText(Transform parent, string name, string value, float x, float y, float w, float h,
        int size, Color color, TextAnchor alignment = TextAnchor.MiddleCenter)
    {
        var rect = AccNode(parent, name); AccRect(rect, x, y, w, h);
        var text = rect.GetComponent<Text>(); if (text == null) text = rect.gameObject.AddComponent<Text>();
        text.font = SkinFont;
        text.text = value; AccText(text, size, alignment, color); return text;
    }

    static void AccLiteral(GameObject root, string path, string value)
    {
        var t = root.transform.Find(path); if (t == null) return;
        var localized = t.GetComponent<WZSDK.LocalizedText>();
        if (localized != null) Object.DestroyImmediate(localized);
        var text = t.GetComponent<Text>(); if (text != null) text.text = value;
    }

    static void AccButton(GameObject root, string path, float x, float y, float w, float h, bool primary, int size)
    {
        var rect = AccRect(root, path, x, y, w, h); if (rect == null) return;
        AccArt(rect, primary ? "button" : "secondary");
        var button = rect.GetComponent<Button>();
        if (button != null)
        {
            button.targetGraphic = rect.GetComponent<Image>();
            var colors = button.colors; colors.normalColor = Color.white;
            colors.highlightedColor = Color.white; colors.pressedColor = new Color(.86f, .9f, 1f);
            colors.disabledColor = new Color(.63f, .65f, .66f); button.colors = colors;
        }
        foreach (var text in rect.GetComponentsInChildren<Text>(true))
        {
            AccRect(text.transform, 0, 0, w - 70, h - 25);
            AccText(text, size, TextAnchor.MiddleCenter, primary ? Color.white : AccBrown);
            if (primary)
            {
                var outline = text.GetComponent<Outline>(); if (outline == null) outline = text.gameObject.AddComponent<Outline>();
                outline.enabled = true; outline.effectColor = new Color(.05f, .18f, .55f); outline.effectDistance = new Vector2(2, -2);
            }
        }
    }

    static void AccInput(GameObject root, string path, float x, float y, float w, float h)
    {
        var rect = AccRect(root, path, x, y, w, h); if (rect == null) return;
        AccArt(rect, "inset");
        var input = rect.GetComponent<InputField>();
        if (input == null) return;
        input.targetGraphic = rect.GetComponent<Image>();
        foreach (var text in rect.GetComponentsInChildren<Text>(true))
        {
            AccRect(text.transform, 0, 0, w - 70, h - 26);
            AccText(text, 48, TextAnchor.MiddleLeft, text == input.placeholder ? new Color(.56f, .55f, .52f) : AccBrown);
        }
    }

    static void AccProgress(GameObject root, string path, string trackName, string fillName, string labelName, float w, float h)
    {
        var track = AccRect(root, path + "/" + trackName, 0, 0, w, h); AccArt(track, "track");
        var fill = AccRect(root, path + "/" + fillName, 0, 0, w - 24, h - 24); AccArt(fill, "fill");
        if (fill != null && fill.GetComponent<Image>() is Image image)
        {
            image.type = Image.Type.Filled; image.fillMethod = Image.FillMethod.Horizontal;
            image.fillOrigin = 0; image.fillClockwise = true;
        }
        AccStyle(root, path + "/" + labelName, 0, 0, w - 55, h - 12, 45);
    }

    static void AccToast(GameObject root, string path, string fallback)
    {
        var box = AccRect(root, path, 0, -80, 1000, 150); if (box == null) return;
        AccClearImage(box);
        var background = AccRect(root, path + "/Image", 0, 0, 1000, 150); AccArt(background, "inset");
        AccStyle(root, path + "/Image/content", 0, 0, 950, 130, 42);
        box.gameObject.SetActive(false);
    }

    static void AccHide(GameObject root, string path)
    {
        var t = root.transform.Find(path); if (t != null) t.gameObject.SetActive(false);
    }

    static void AccClearImage(Transform t)
    {
        if (t == null) return;
        var image = t.GetComponent<Image>(); if (image != null) { image.color = Color.clear; image.raycastTarget = false; }
    }

    static Image AccAddArt(Transform parent, string name, string key, float x, float y, float w, float h)
    {
        var rect = AccNode(parent, name); AccRect(rect, x, y, w, h); AccArt(rect, key, true);
        return rect.GetComponent<Image>();
    }

    static void AccArt(Transform t, string key, bool simple = false)
    {
        if (t == null) return;
        var image = t.GetComponent<Image>(); if (image == null) image = t.gameObject.AddComponent<Image>();
        if (key != null) image.sprite = AccSprite(key);
        image.color = Color.white; image.type = simple ? Image.Type.Simple : Image.Type.Sliced;
        image.preserveAspect = simple; image.raycastTarget = t.GetComponent<Selectable>() != null;
        image.pixelsPerUnitMultiplier = 1;
    }

    static Sprite AccSprite(string key)
    {
        return SkinSprite(key);
    }
}
