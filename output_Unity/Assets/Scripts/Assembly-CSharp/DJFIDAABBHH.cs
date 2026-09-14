using Unity.Services.LevelPlay;

public class DJFIDAABBHH : BIDJKEKHFDE
{
	private LevelPlayInterstitialAd interstitialAd;

	private bool currentAdUsesBidFloor;

	public DJFIDAABBHH(string unitId)
		: base(null)
	{
	}

	private void OnInterstitialLoadedEvent(LevelPlayAdInfo adInfo)
	{
	}

	private void OnInterstitialLoadFailedEvent(LevelPlayAdError errorInfo)
	{
	}

	private void OnInterstitialShownEvent(LevelPlayAdInfo adInfo)
	{
	}

	private void OnInterstitialAdFailedToDisplayEvent(LevelPlayAdDisplayInfoError errorInfo)
	{
	}

	private void OnInterstitialClickedEvent(LevelPlayAdInfo adInfo)
	{
	}

	private void OnInterstitialDismissedEvent(LevelPlayAdInfo adInfo)
	{
	}

	private void OnInterstitialInfoChangedEvent(LevelPlayAdInfo adInfo)
	{
	}

	public override bool inner_isReady(string unitID)
	{
		return false;
	}

	public override void inner_load(string unitId, double bidFloorEcpm)
	{
	}

	public override void inner_show(string unitID)
	{
	}

	public override void inner_destroy()
	{
	}

	public override bool inner_reusable()
	{
		return false;
	}

	protected override bool inner_support_bid_floor()
	{
		return false;
	}

	private void CreateAd(string unitId, double bidFloorEcpm)
	{
	}

	private static bool ShouldUseBidFloor(double bidFloorEcpm)
	{
		return false;
	}
}
