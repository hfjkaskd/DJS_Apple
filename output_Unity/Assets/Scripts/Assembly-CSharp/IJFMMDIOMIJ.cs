using System.Collections.Generic;

public class IJFMMDIOMIJ : global::IAJDGNOGFGO<PurchaseData, IJFMMDIOMIJ>
{
	public PurchaseData Data => null;

	public bool Inited { get; private set; }

	public List<FPHGLNCPHJP> GiftPackRewards { get; private set; }

	public List<COAEOCCIEAO> CoinPackRewards { get; private set; }

	public FPHGLNCPHJP LastGiftPackReward { get; private set; }

	public COAEOCCIEAO LastCoinPackReward { get; private set; }

	protected override string GetKey()
	{
		return null;
	}

	public override void Init()
	{
	}

	private void OnProcessPurchase(string InProductId)
	{
	}

	public bool HasRemoveAdsRight()
	{
		return false;
	}

	public void SetRemoveADState(bool InRemoveADState)
	{
	}

	private void BuyGiftPack(FPHGLNCPHJP InGiftPackReward)
	{
	}

	private void BuyCoinPack(COAEOCCIEAO InCoinPackReward)
	{
	}

	private void CleanAllTempProperties(params POJCEPBNNIP[] itemTypes)
	{
	}
}
