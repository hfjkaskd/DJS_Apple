using UnityEngine;

/// <summary>Records effective play and presents small rewards only at safe gameplay boundaries.</summary>
public static class HarvestGameplayBridge
{
    private const string WelcomePromptKey = "harvest.welcome_card.v1";
    private static float refreshElapsed;
    private static double effectiveElapsed;
    private static bool paused;
    private static string lastShownInGameOffer;
    private static string lastShownWinOffer;

    public static void Initialize()
    {
        HarvestRewardService.Instance.Tick();
        refreshElapsed = 0;
        effectiveElapsed = 0;
        paused = false;
        lastShownInGameOffer = null;
        lastShownWinOffer = null;
    }

    public static void Tick(float deltaTime)
    {
        if (paused || deltaTime <= 0) return;
        MgrUI manager = MgrUI.Instance;
        CorePlay.CorePlayUI playUI = CorePlay.CorePlayUI.Instance;
        BaseUI top = manager != null ? manager.GetTopUI() : null;
        bool boardReady = manager != null && playUI != null && playUI.IsOpening && top == playUI
            && !MCCIJBJGMCK.IsLock() && !JEFOMCDAPGK.Instance.Data.isLevelFinished
            && !JEFOMCDAPGK.Instance.MainLevelData.isLose;
        bool countsEffectivePlay = boardReady && JEFOMCDAPGK.Instance.CurMainLevelIndex > 1;
        if (countsEffectivePlay) effectiveElapsed += deltaTime;
        refreshElapsed += deltaTime;
        if (refreshElapsed < 1f) return;
        refreshElapsed = 0f;
        HarvestRewardService service = HarvestRewardService.Instance;
        service.Tick();
        if (!service.IsAvailable)
        {
            effectiveElapsed = 0;
            return;
        }
        if (effectiveElapsed > 0)
        {
            service.AdvanceEffectivePlay(effectiveElapsed);
            effectiveElapsed = 0;
        }
        if (manager == null || MCCIJBJGMCK.IsLock() || service.IsRewardedAdPending) return;
        WinUI win = top as WinUI;
        bool winReady = win != null && win.IsHarvestRewardPromptReady;
        if (!boardReady && !winReady) return;

        // The tutorial gift is already credited by the domain engine; this card only acknowledges it.
        if (service.State.TutorialGiftClaimed && !PlayerPrefs.HasKey(WelcomePromptKey))
        {
            if (HarvestRewardPopupUI.OpenWelcome())
            {
                PlayerPrefs.SetInt(WelcomePromptKey, 1);
                PlayerPrefs.Save();
            }
            return;
        }
        if (winReady)
        {
            HarvestRewardOffer reward = service.State.PendingWinReward;
            if (reward != null && reward.RewardId != lastShownWinOffer && HarvestRewardPopupUI.OpenWin())
                lastShownWinOffer = reward.RewardId;
            return;
        }
        HarvestRewardOffer offer = service.State.PendingInGameReward;
        if (offer != null && offer.RewardId != lastShownInGameOffer && HarvestRewardPopupUI.OpenInGame())
            lastShownInGameOffer = offer.RewardId;
        // Thresholds and task progress stay in the HUD; the wallet only opens on a Button click.
    }

    public static void SetPaused(bool value)
    {
        paused = value;
        if (value)
        {
            if (effectiveElapsed > 0) HarvestRewardService.Instance.AdvanceEffectivePlay(effectiveElapsed);
            effectiveElapsed = 0;
            HarvestRewardService.Instance.Flush();
        }
        else HarvestRewardService.Instance.Tick();
    }
}
