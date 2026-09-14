using Unity.Services.LevelPlay;

public class DKKBLKEMDDP : GPJMMMNFOBM
{
	private LevelPlayRewardedAd rewardedAd;

	private bool currentAdUsesBidFloor;

	public DKKBLKEMDDP(string unitId)
		: base(null)
	{
	}

	private void OnRewardedVideoLoadedEvent(LevelPlayAdInfo adInfo)
	{
	}

	private void OnRewardedVideoLoadFailedEvent(LevelPlayAdError errorInfo)
	{
	}

	private void OnRewardedVideoImpressionTrackedEvent(LevelPlayAdInfo adInfo)
	{
	}

	private void OnRewardedVideoFailedToPlayEvent(LevelPlayAdDisplayInfoError errorInfo)
	{
	}

	private void OnRewardedVideoClickedEvent(LevelPlayAdInfo adInfo)
	{
	}

	private void OnRewardedVideoClosedEvent(LevelPlayAdInfo adInfo)
	{
	}

	private void OnRewardedVideoReceivedRewardEvent(LevelPlayAdInfo adInfo, LevelPlayReward reward)
	{
	}

	private void OnRewardedVideoInfoChangedEvent(LevelPlayAdInfo adInfo)
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

	public override bool auto_load()
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
