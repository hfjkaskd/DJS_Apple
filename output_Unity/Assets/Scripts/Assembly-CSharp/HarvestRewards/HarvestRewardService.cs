using System;
using System.IO;
using UnityEngine;

/// <summary>A real SDK adapter must report its original request ID only after a completed rewarded ad.</summary>
public interface IHarvestRewardedAdProvider
{
    bool IsRewardedAvailable(int rewardKind);
    void ShowRewarded(string requestId, int rewardKind, long displayedRewardCents, Action<string, bool> completion);
}

/// <summary>Shared Editor/Android/iOS simulation service. Does not instantiate UI or fake SDK success.</summary>
public sealed class HarvestRewardService
{
    private static HarvestRewardService instance;
    public static HarvestRewardService Instance { get { if (instance == null) instance = new HarvestRewardService(); return instance; } }
    public event Action Changed;
    public bool IsAvailable { get; private set; }
    public string LastError { get; private set; }
    public string LastAdError { get; private set; }
    public HarvestRewardSettings Config { get; private set; }
    public HarvestRewardState State { get { return engine != null ? engine.State : unavailableState; } }
    public string CurrentRunId { get { return engine == null ? null : engine.CurrentRunId; } }
    public bool HasInGameReward { get { return IsAvailable && engine.HasInGameReward; } }
    public bool HasWinReward { get { return IsAvailable && engine.HasWinReward; } }
    public bool CanClaimInGameBase { get { return IsAvailable && engine.CanClaimInGameBase; } }
    public bool CanClaimWinBase { get { return IsAvailable && engine.CanClaimWinBase; } }
    public bool IsRewardedAdPending { get { return IsAvailable && engine.HasPendingAd; } }
    private HarvestRewardEngine engine;
    private readonly HarvestRewardState unavailableState = new HarvestRewardState();
    private readonly string savePath;
    private IHarvestRewardedAdProvider adProvider;
    private double lastFlushRealtime;

    private HarvestRewardService()
    {
        savePath = Path.Combine(Application.persistentDataPath, "harvest_rewards_v1.bin");
        try
        {
            HarvestRewardConfig asset = Resources.Load<HarvestRewardConfig>("HarvestRewardConfig");
            if (asset == null) throw new InvalidOperationException("Resources/HarvestRewardConfig is required.");
            Config = asset.Settings;
            engine = new HarvestRewardEngine(Config, HarvestRewardStorage.Load(savePath));
            engine.Tick(UtcNow());
            engine.Changed += OnEngineChanged;
            IsAvailable = true;
            Flush();
        }
        catch (Exception exception) { Disable("丰收奖励暂不可用：配置或存档异常，请保留存档并联系支持。", exception); }
    }

    public string BeginRun(string runId = null)
    {
        if (!IsAvailable) return runId;
        return engine.BeginRun(runId, UtcNow());
    }

    public bool RecordTriple(string runId, int eventIndex, bool isTutorial, bool isFinalGroup = false)
    {
        return IsAvailable && engine.RecordTriple(runId, eventIndex, isTutorial, UtcNow(), isFinalGroup) && IsAvailable;
    }

    public bool RecordWin(string runId, bool isTutorial)
    {
        return IsAvailable && engine.RecordWin(runId, isTutorial, UtcNow()) && IsAvailable;
    }

    public bool ClaimInGameBase(string runId = null)
    {
        return IsAvailable && engine.ClaimInGameBase(runId, UtcNow()) && IsAvailable;
    }

    public bool ClaimWinBase(string runId = null)
    {
        return IsAvailable && engine.ClaimWinBase(runId, UtcNow()) && IsAvailable;
    }

    public long GetInGameRewardPreviewCents() { return IsAvailable ? engine.GetInGameRewardPreviewCents() : 0; }
    public long GetWinRewardPreviewCents() { return IsAvailable ? engine.GetWinRewardPreviewCents() : 0; }
    public long GetRemainingSeconds() { return IsAvailable ? engine.GetRemainingSeconds() : 0; }
    public bool TryApply(int tierIndex) { return IsAvailable && engine.TryApply(tierIndex, UtcNow()) && IsAvailable; }

    public void Tick()
    {
        if (!IsAvailable) return;
        engine.Tick(UtcNow());
        if (Time.realtimeSinceStartupAsDouble - lastFlushRealtime >= Config.SaveIntervalSeconds) Flush();
    }

    public void AdvanceEffectivePlay(double seconds)
    {
        if (IsAvailable) engine.AdvanceEffectivePlay(seconds, UtcNow());
    }

    public void Flush()
    {
        if (!IsAvailable) return;
        try
        {
            HarvestRewardStorage.Validate(State, Config);
            HarvestRewardStorage.Save(savePath, State);
            lastFlushRealtime = Time.realtimeSinceStartupAsDouble;
        }
        catch (Exception exception) { Disable("丰收奖励存档写入失败，已暂停奖励操作。请保留存档并重启后检查。", exception); }
    }

    public void SetRewardedAdProvider(IHarvestRewardedAdProvider provider)
    {
        if (engine != null) engine.CancelRewardedClaim();
        adProvider = provider;
    }

    public bool IsRewardedAvailable(int kind)
    {
        return IsAvailable && !engine.HasPendingAd && adProvider != null && adProvider.IsRewardedAvailable(kind);
    }

    public bool RequestRewardedReward(int kind)
    {
        if (!IsRewardedAvailable(kind)) { LastAdError = "暂无可用广告，可领取基础奖励。"; return false; }
        string requestId;
        long rewardCents;
        if (!engine.TryBeginRewardedClaim(kind, out requestId, out rewardCents)) return false;
        LastAdError = null;
        try { adProvider.ShowRewarded(requestId, kind, rewardCents, OnRewardedCompleted); }
        catch (Exception exception)
        {
            engine.CancelRewardedClaim();
            LastAdError = "广告未能开始，可领取基础奖励。";
            Debug.LogWarning("Harvest rewarded provider failed: " + exception.Message);
            PublishChanged();
            return false;
        }
        return true;
    }

    private void OnRewardedCompleted(string requestId, bool completed)
    {
        if (!IsAvailable) return;
        if (!engine.CompleteRewardedClaim(requestId, completed, UtcNow()))
        {
            if (!completed) LastAdError = "广告未完整观看，未增加奖励或成功次数。";
            PublishChanged();
        }
    }

    private void OnEngineChanged()
    {
        Flush();
        PublishChanged();
    }

    private void PublishChanged() { Action handler = Changed; if (handler != null) handler(); }

    private void Disable(string message, Exception exception)
    {
        IsAvailable = false;
        LastError = message;
        Debug.LogError(message + " " + exception.Message);
    }

    private static long UtcNow() { return DateTimeOffset.UtcNow.ToUnixTimeSeconds(); }
}
