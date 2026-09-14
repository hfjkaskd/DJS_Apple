using AmazonAds;
using UnityEngine;
using UnityEngine.UI;

public class AmazonIronSourceDemo : MonoBehaviour
{
	private const string isKey = "1396b4e61";

	private const string placementName = "YOUR_PLACEMENT_NAME";

	private const string appId = "7873ab072f0647b8837748312c7b8b5a";

	private const string amazonBannerSlotId = "ed3b9f16-4497-4001-be7d-2e8ca679ee73";

	private const string amzonInterstitialSlotId = "394133e6-27fe-477d-816b-4a00cdaa54b6";

	private const string amazonInterstitialVideoSlotId = "b9f9a2aa-72d8-4cb3-83db-949ebb53836f";

	private const string amazonRewardedVideoSlotId = "1ed9fa0b-3cf0-4326-8c35-c0e9ddcdb765";

	public Button isInitializedBut;

	private APSBannerAdRequest bannerAdRequest;

	private APSInterstitialAdRequest interstitialAdRequest;

	private APSVideoAdRequest videoAdRequest;

	private static bool isManualRewardedVideo;

	public static bool getManualRewardedVideoInitialization()
	{
		return false;
	}

	public static void setManualRewardedVideoInitialization(bool flag)
	{
	}

	public void Initialize()
	{
	}

	public void ManualRewardedVideoInitialize()
	{
	}

	public void goBack()
	{
	}

	private void RewardedVideoCallback()
	{
	}

	private void AmazonInitialize()
	{
	}

	public void IsInitialized()
	{
	}

	public void ShowBanner()
	{
	}

	public void ShowSkAdNBanner()
	{
	}

	public void ShowInterstitial()
	{
	}

	public void ShowInterstitialVideo()
	{
	}

	public void ShowRewardedVideo()
	{
	}

	private void BannerAdLoadedEvent(IronSourceAdInfo bannerAdInfo)
	{
	}

	private void BannerAdLoadFailedEvent(IronSourceError error)
	{
	}

	private void BannerAdClickedEvent(IronSourceAdInfo bannerAdClickInfo)
	{
	}

	private void BannerAdScreenPresentedEvent(IronSourceAdInfo bannerAdScreenPresentInfo)
	{
	}

	private void BannerAdScreenDismissedEvent(IronSourceAdInfo bannerAdScreenDismissedInfo)
	{
	}

	private void BannerAdLeftApplicationEvent(IronSourceAdInfo bannerAdLeftApplicationInfo)
	{
	}

	private void InterstitialAdLoadFailedEvent(IronSourceError error)
	{
	}

	private void InterstitialAdShowFailedEvent(IronSourceError error, IronSourceAdInfo adShowFailedInfo)
	{
	}

	private void InterstitialAdClickedEvent(IronSourceAdInfo adClickEventInfo)
	{
	}

	private void InterstitialAdClosedEvent(IronSourceAdInfo adClosedEventInfo)
	{
	}

	private void InterstitialAdReadyEvent(IronSourceAdInfo adReadyEventInfo)
	{
	}

	private void InterstitialAdOpenedEvent(IronSourceAdInfo adOpenEventInfo)
	{
	}

	private void InterstitialAdShowSucceededEvent(IronSourceAdInfo adShowSucceededEventInfo)
	{
	}

	private void RewardedAdReadyEvent()
	{
	}

	private void RewardedVideoOnAdAvailable(IronSourceAdInfo adInfo)
	{
	}

	private void RewardedVideoOnAdUnavailable()
	{
	}

	private void RewardedVideoOnAdOpenedEvent(IronSourceAdInfo adInfo)
	{
	}

	private void RewardedVideoOnAdClosedEvent(IronSourceAdInfo adInfo)
	{
	}

	private void RewardedVideoOnAdRewardedEvent(IronSourcePlacement placement, IronSourceAdInfo adInfo)
	{
	}

	private void RewardedVideoOnAdShowFailedEvent(IronSourceError error, IronSourceAdInfo adInfo)
	{
	}

	private void RewardedVideoOnAdLoadFailedEvent(IronSourceError error)
	{
	}

	private void RewardedVideoOnAdClickedEvent(IronSourcePlacement placement, IronSourceAdInfo adInfo)
	{
	}
}
