public class HMBIFJPBNHN : GPJMMMNFOBM
{
	public HMBIFJPBNHN(string unitId)
		: base(null)
	{
	}

	private void OnRewardedVideoLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
	{
	}

	private void OnRewardedVideoLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
	{
	}

	private void OnRewardedVideoImpressionTrackedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
	{
	}

	private void OnRewardedVideoFailedToPlayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
	{
	}

	private void OnRewardedVideoClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
	{
	}

	private void OnRewardedVideoClosedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
	{
	}

	private void OnRewardedVideoReceivedRewardEvent(string adUnitId, MaxSdkBase.Reward reward, MaxSdkBase.AdInfo adInfo)
	{
	}

	public override bool inner_isReady(string unitId)
	{
		return false;
	}

	public override void inner_load(string unitId, double bidFloorEcpm)
	{
	}

	public override void inner_show(string unitId)
	{
	}

	public override void inner_destroy()
	{
	}

	public override bool inner_reusable()
	{
		return false;
	}

	public override bool auto_load()
	{
		return false;
	}
}
