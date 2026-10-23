using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using WaterSort;

/// <summary>
/// Updates the additional authored skin widgets from the source controller's state.
/// This component does not advance tasks, award currency, submit requests, or create UI.
/// </summary>
[DisallowMultipleComponent]
public sealed class Framework2AppleSkinPresenter : MonoBehaviour
{
    [Serializable]
    public sealed class Bindings
    {
        public Text Header;
        public string SavingTitle;
        public string RecordedTitle;
        public string FailedTitle;
        public string ReviewTitle;
        public GameObject LockedIcon;
        public GameObject WalletIcon;
        public GameObject LuckyTitle;
        public GameObject StandardMethods;
        public GameObject PixMethods;
        public GameObject PhoneMethod;
        public Text InputLabel;
        public string EmailLabel;
        public string PhoneLabel;
        public string PixLabel;
        public Image InputErrorBorder;
        public GameObject EmptyHistory;
        public Text[] GemCostLabels;
        public Image TaskBackground;
        public Image TaskBadge;
        public Text TaskNumber;
        public Color TaskNormalTint = Color.white;
        public Color TaskCompletedTint = Color.white;
    }

    [SerializeField] private Bindings bindings = new Bindings();

    // Called by the editor authoring pass; all references are then stored in the prefab.
    public void Configure(Bindings value)
    {
        bindings = value ?? new Bindings();
    }

    public void SetLuckyUnlocked(bool unlocked)
    {
        SetVisible(bindings.LockedIcon, !unlocked);
        SetVisible(bindings.WalletIcon, unlocked);
        SetVisible(bindings.LuckyTitle, unlocked);
    }

    public void SetAccountChannel(WaterSortAccountInputKind inputKind)
    {
        bool pix = inputKind == WaterSortAccountInputKind.Pix;
        bool phone = inputKind == WaterSortAccountInputKind.Phone;
        SetVisible(bindings.StandardMethods, !pix && !phone);
        SetVisible(bindings.PixMethods, pix);
        SetVisible(bindings.PhoneMethod, phone);
        if (bindings.InputLabel != null)
            bindings.InputLabel.text = pix ? bindings.PixLabel : phone ? bindings.PhoneLabel : bindings.EmailLabel;
    }

    public void SetAccountError(bool invalid)
    {
        if (bindings.InputErrorBorder != null)
            bindings.InputErrorBorder.enabled = invalid;
    }

    public void SetHistoryEmpty(bool empty) => SetVisible(bindings.EmptyHistory, empty);

    public void SetVerificationState(bool complete, bool failed = false, bool reviewOnly = false)
    {
        if (bindings.Header == null) return;
        bindings.Header.text = failed ? bindings.FailedTitle : reviewOnly ? bindings.ReviewTitle : complete ? bindings.RecordedTitle : bindings.SavingTitle;
    }

    public void SetGemCosts(int[] costs)
    {
        if (bindings.GemCostLabels == null || costs == null) return;
        int count = Mathf.Min(bindings.GemCostLabels.Length, costs.Length);
        for (int i = 0; i < count; i++)
            if (bindings.GemCostLabels[i] != null)
                bindings.GemCostLabels[i].text = costs[i].ToString("N0", CultureInfo.CurrentCulture);
    }

    public void SetTaskComplete(bool complete)
    {
        if (bindings.TaskBackground != null)
            bindings.TaskBackground.color = complete ? bindings.TaskCompletedTint : bindings.TaskNormalTint;
        if (bindings.TaskBadge != null)
            bindings.TaskBadge.sprite = Framework2AppleSkinAssets.LoadSprite(complete ? "check" : "goldbadge");
        if (bindings.TaskNumber != null) SetVisible(bindings.TaskNumber.gameObject, !complete);
    }

    private static void SetVisible(GameObject target, bool visible)
    {
        if (target != null && target.activeSelf != visible) target.SetActive(visible);
    }
}
