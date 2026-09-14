using UnityEngine;

/// <summary>Connects the configured reward UI and ledger to the existing game lifecycle.</summary>
public static class HarvestGameplayBridge
{
    private const string RewardPage = "harvest/HarvestRewardsUI";
    private const string FirstTargetPromptKey = "harvest.first_target_prompt.v1";
    private static float refreshElapsed;
    private static double effectiveElapsed;
    private static bool paused;
    private static string lastShownOffer;

    public static void Initialize()
    {
        HarvestRewardService.Instance.Tick();
        refreshElapsed = 0;
        effectiveElapsed = 0;
        paused = false;
        lastShownOffer = null;
    }

    public static void Tick(float deltaTime)
    {
        if (paused || deltaTime <= 0) return;
        MgrUI manager = MgrUI.Instance;
        CorePlay.CorePlayUI playUI = CorePlay.CorePlayUI.Instance;
        bool playable = manager != null && playUI != null && playUI.IsOpening &&
            manager.GetTopUI() == playUI && !MCCIJBJGMCK.IsLock() &&
            JEFOMCDAPGK.Instance.CurMainLevelIndex > 1 &&
            !JEFOMCDAPGK.Instance.Data.isLevelFinished &&
            !JEFOMCDAPGK.Instance.MainLevelData.isLose;
        if (playable) effectiveElapsed += deltaTime;
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
        // Wait until the board is available, so rewards never interrupt a win or another popup.
        if (!playable) return;
        HarvestRewardOffer offer = service.State.PendingInGameReward;
        if (offer != null && offer.RewardId != lastShownOffer)
        {
            lastShownOffer = offer.RewardId;
            manager.Open(RewardPage);
            return;
        }
        if (service.State.ActiveRequest == null &&
            service.State.AvailableCents >= service.Config.ThresholdCents[0] &&
            !PlayerPrefs.HasKey(FirstTargetPromptKey))
        {
            manager.Open(RewardPage);
            if (manager.IsOpenning(RewardPage))
            {
                PlayerPrefs.SetInt(FirstTargetPromptKey, 1);
                PlayerPrefs.Save();
            }
        }
    }

    public static void SetPaused(bool value)
    {
        paused = value;
        if (value)
        {
            if (effectiveElapsed > 0)
                HarvestRewardService.Instance.AdvanceEffectivePlay(effectiveElapsed);
            effectiveElapsed = 0;
            HarvestRewardService.Instance.Flush();
        }
        else HarvestRewardService.Instance.Tick();
    }
}
