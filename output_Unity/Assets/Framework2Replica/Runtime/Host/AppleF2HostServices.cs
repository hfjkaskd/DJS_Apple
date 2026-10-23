using System;
using UnityEngine;

// Original telemetry decision points use an optional Apple sink, without the Mahjong SDK.
internal static class MahjongProductAnalytics
{
    public static void Track(string name, params object[] values) => Apple.Framework2.AppleF2Runtime.Analytics?.Invoke(name, values);
    public static void State(string name, string scope, params object[] values) => Track(name, values);
    public static string Begin(string name, params object[] values) { string id = Guid.NewGuid().ToString("N"); Track(name, values); return id; }
    public static string Id(string value) => value ?? string.Empty;
    public static void SetContext(string name, object value) => Track("context", name, value);
    public static void GuideHidden(string name) => Track("guide_hidden", name);
    public static void GuideShow(string name, int step) => Track("guide_show", name, step);
    public static void GuideStepComplete(string name, int step) => Track("guide_step_complete", name, step);
    public static void GuideEnd(string name) => Track("guide_end", name);
}
internal static class MahjongAnalyticsView
{
    public static void Observe(GameObject view, string name) => MahjongProductAnalytics.Track("view_open", name);
    public static void Closed(GameObject view, string reason) => MahjongProductAnalytics.Track("view_close", view != null ? view.name : string.Empty, reason);
    public static void Snapshot(GameObject view, string reason) { }
}
namespace MahjongSlide.Withdrawal
{
    // Imported reward SFX keep their source SoundManager; Apple owns background music.
    public static class MahjongRewardAudio
    {
        public static void PopupOpened() => WZSDK.SoundManager.Instance?.PlayUIClickSFX();
        public static void FlightStarted() => WZSDK.SoundManager.Instance?.PlayCollectCoinSFX();
        public static void CashArrived() => WZSDK.SoundManager.Instance?.PlayCollectCoinSFX();
        public static void CounterAdvanced(object owner) { }
        public static void CounterStopped(object owner) { }
    }
}
