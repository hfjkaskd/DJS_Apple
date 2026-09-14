using Unity.Services.LevelPlay;
using UnityEngine;

public class LevelPlaySample : MonoBehaviour
{
	[SerializeField]
	private Texture2D lpLogo;

	private LevelPlayBannerAd bannerAd;

	private LevelPlayInterstitialAd interstitialAd;

	private LevelPlayRewardedAd rewardedVideoAd;

	private bool isAdsEnabled;

	public void Start()
	{
	}

	private void EnableAds()
	{
	}

	public void OnGUI()
	{
	}

	private void SdkInitializationCompletedEvent(LevelPlayConfiguration config)
	{
	}

	private void SdkInitializationFailedEvent(LevelPlayInitError error)
	{
	}

	private void RewardedVideoOnLoadedEvent(LevelPlayAdInfo adInfo)
	{
	}

	private void RewardedVideoOnAdLoadFailedEvent(LevelPlayAdError error)
	{
	}

	private void RewardedVideoOnAdDisplayedEvent(LevelPlayAdInfo adInfo)
	{
	}

	private void RewardedVideoOnAdDisplayedFailedEvent(LevelPlayAdDisplayInfoError error)
	{
	}

	private void RewardedVideoOnAdRewardedEvent(LevelPlayAdInfo adInfo, LevelPlayReward reward)
	{
	}

	private void RewardedVideoOnAdClickedEvent(LevelPlayAdInfo adInfo)
	{
	}

	private void RewardedVideoOnAdClosedEvent(LevelPlayAdInfo adInfo)
	{
	}

	private void RewardedVideoOnAdInfoChangedEvent(LevelPlayAdInfo adInfo)
	{
	}

	private void InterstitialOnAdLoadedEvent(LevelPlayAdInfo adInfo)
	{
	}

	private void InterstitialOnAdLoadFailedEvent(LevelPlayAdError error)
	{
	}

	private void InterstitialOnAdDisplayedEvent(LevelPlayAdInfo adInfo)
	{
	}

	private void InterstitialOnAdDisplayFailedEvent(LevelPlayAdDisplayInfoError infoError)
	{
	}

	private void InterstitialOnAdClickedEvent(LevelPlayAdInfo adInfo)
	{
	}

	private void InterstitialOnAdClosedEvent(LevelPlayAdInfo adInfo)
	{
	}

	private void InterstitialOnAdInfoChangedEvent(LevelPlayAdInfo adInfo)
	{
	}

	private void BannerOnAdLoadedEvent(LevelPlayAdInfo adInfo)
	{
	}

	private void BannerOnAdLoadFailedEvent(LevelPlayAdError error)
	{
	}

	private void BannerOnAdClickedEvent(LevelPlayAdInfo adInfo)
	{
	}

	private void BannerOnAdDisplayedEvent(LevelPlayAdInfo adInfo)
	{
	}

	private void BannerOnAdDisplayFailedEvent(LevelPlayAdDisplayInfoError adInfoError)
	{
	}

	private void BannerOnAdCollapsedEvent(LevelPlayAdInfo adInfo)
	{
	}

	private void BannerOnAdLeftApplicationEvent(LevelPlayAdInfo adInfo)
	{
	}

	private void BannerOnAdExpandedEvent(LevelPlayAdInfo adInfo)
	{
	}

	private void ImpressionDataReadyEvent(LevelPlayImpressionData impressionData)
	{
	}

	private void OnDisable()
	{
	}
}
