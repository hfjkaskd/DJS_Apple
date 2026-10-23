using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using WZSDK;

/// <summary>
/// Read-only presentation binding for the authored Apple B HUD and user-level badge.
/// Original controllers remain the sole owners of progression, balances, ad counts and reward rules.
/// </summary>
[DisallowMultipleComponent]
[DefaultExecutionOrder(1100)]
public sealed class Framework2AppleNavigationPresenter : MonoBehaviour
{
    public Text LevelValue, UserLevelValue;
    public GameObject SourceStageRoot, SecondaryCurrencyRoot;
    public Image SourceProgress;
    public Text SourceProgressText, SourcePercentText;
    public GameObject EarlyTaskRoot, AdTaskRoot, BalanceTaskRoot;
    public Image EarlyFill, AdFill, BalanceFill;
    public Text EarlyTaskText, AdCountText, BalanceCountText, BalancePercentText;
    public Vector2 PrimaryTaskPosition, SecondaryTaskPosition;
    [SerializeField, Min(.02f)] private float refreshInterval = .1f;
    private float nextRefresh;
    private int lastLevel = -1, lastUserLevel = -1, lastObjectiveTarget = -1;
    private bool lastSecondaryVisible;

    private void OnEnable()
    {
        nextRefresh = 0f;
        lastLevel = lastUserLevel = lastObjectiveTarget = -1;
        lastSecondaryVisible = false;
    }

    private void LateUpdate()
    {
        if (Time.unscaledTime < nextRefresh) return;
        nextRefresh = Time.unscaledTime + refreshInterval;
        GameManagerWZ manager = GameManagerWZ.instance;
        if (manager == null) return;
        if (LevelValue != null && lastLevel != manager.currentLv)
        {
            lastLevel = manager.currentLv;
            LevelValue.text = lastLevel.ToString(CultureInfo.InvariantCulture);
        }
        if (UserLevelValue != null && lastUserLevel != manager.userLevel)
        {
            lastUserLevel = manager.userLevel;
            UserLevelValue.text = (lastUserLevel + 1).ToString(CultureInfo.InvariantCulture);
        }
        if (EarlyTaskRoot == null) return;

        bool early = manager.currentStage <= 2 && manager.TryGetStageRequirement(manager.currentStage, out int earlyType, out int earlyTarget) && earlyType == 1;
        SetVisible(EarlyTaskRoot, early);
        if (early)
        {
            manager.TryGetStageRequirement(manager.currentStage, out _, out int target);
            if (EarlyTaskText != null && target != lastObjectiveTarget)
            {
                lastObjectiveTarget = target;
                string format = LocalizationManager.Instance != null && LocalizationManager.Instance.HasText("3101")
                    ? LocalizationManager.Instance.GetText("3101") : "Clear Level {0}";
                EarlyTaskText.text = string.Format(CultureInfo.CurrentCulture, format, target);
            }
            if (EarlyFill != null) EarlyFill.fillAmount = target <= 0 ? 0f : Mathf.Clamp01((manager.currentLv - 1f) / target);
        }
        bool nativeVisible = SourceStageRoot != null && SourceStageRoot.activeInHierarchy;
        bool feedbackPreviewsNextBalance = manager.currentStage == 3 && manager.IsStageCompleted(3) &&
            UIManager.instance != null && UIManager.instance.HasActiveView(EUIType.WithdrawFeedbackView);
        bool ads = !early && nativeVisible && manager.currentStage == 3 && !feedbackPreviewsNextBalance;
        bool balance = !early && nativeVisible && !ads;
        SetVisible(AdTaskRoot, ads);
        SetVisible(BalanceTaskRoot, balance);
        // Mirror the original delayed fill/counter. Never recompute an earlier reward arrival.
        if (SourceProgress != null)
        {
            if (ads && AdFill != null) AdFill.fillAmount = SourceProgress.fillAmount;
            if (balance && BalanceFill != null) BalanceFill.fillAmount = SourceProgress.fillAmount;
        }
        if (SourceProgressText != null)
        {
            if (ads) CopyText(AdCountText, SourceProgressText.text);
            if (balance) CopyText(BalanceCountText, SourceProgressText.text);
        }
        if (balance && SourcePercentText != null) CopyText(BalancePercentText, SourcePercentText.text);
        bool secondary = SecondaryCurrencyRoot != null && SecondaryCurrencyRoot.activeSelf;
        if (secondary != lastSecondaryVisible)
        {
            lastSecondaryVisible = secondary;
            Vector2 position = secondary ? SecondaryTaskPosition : PrimaryTaskPosition;
            SetTaskPosition(EarlyTaskRoot, position); SetTaskPosition(AdTaskRoot, position); SetTaskPosition(BalanceTaskRoot, position);
        }
    }

    private static void CopyText(Text target, string value)
    {
        if (target != null && target.text != value) target.text = value;
    }

    private static void SetVisible(GameObject target, bool value)
    {
        if (target != null && target.activeSelf != value) target.SetActive(value);
    }

    private static void SetTaskPosition(GameObject target, Vector2 value)
    {
        if (target != null && target.transform is RectTransform rect) rect.anchoredPosition = value;
    }
}
