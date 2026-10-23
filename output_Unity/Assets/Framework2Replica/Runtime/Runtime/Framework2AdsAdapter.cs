using System;

namespace WZSDK
{
    internal sealed class Framework2AdsAdapter
    {
        private static readonly Framework2AdsAdapter instance = new Framework2AdsAdapter();
        internal static Framework2AdsAdapter GetOrCreate() => instance;
        internal bool IsInterstitialReady => Framework2Host.Interstitial != null;
        internal bool IsRewardVideoReady => Framework2Host.Rewarded != null;
        internal void ShowRewardVideo(string placement, Action<bool> completed)
        {
            if (Framework2Host.Rewarded != null) Framework2Host.Rewarded(placement, completed);
            else completed?.Invoke(false);
        }
        internal void ShowInterstitial(string placement, Action<bool> completed)
        {
            if (Framework2Host.Interstitial != null) Framework2Host.Interstitial(placement, completed);
            else completed?.Invoke(false);
        }
    }
}
