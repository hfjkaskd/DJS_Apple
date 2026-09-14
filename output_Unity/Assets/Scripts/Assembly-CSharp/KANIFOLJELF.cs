public class KANIFOLJELF : BIDJKEKHFDE
{
	public KANIFOLJELF(string unitId)
		: base(null)
	{
	}

	private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
	{
	}

	private void OnInterstitialLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
	{
	}

	private void OnInterstitialShownEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
	{
	}

	private void OnInterstitialAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
	{
	}

	private void OnInterstitialClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
	{
	}

	private void OnInterstitialDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
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
}
