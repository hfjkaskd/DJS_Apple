using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

public class PFLHDLKKJNL : global::FOLJNEPEKCA<PFLHDLKKJNL>, IDetailedStoreListener, IStoreListener
{
	public const string FRUIT_REMOVE_ADS = "fruit_remove_ads";

	public const string FRUIT_COINPACK_1 = "fruit_coinpack_1";

	public const string FRUIT_COINPACK_2 = "fruit_coinpack_2";

	public const string FRUIT_COINPACK_3 = "fruit_coinpack_3";

	public const string FRUIT_COINPACK_4 = "fruit_coinpack_4";

	public const string FRUIT_COINPACK_5 = "fruit_coinpack_5";

	public const string FRUIT_COINPACK_6 = "fruit_coinpack_6";

	public const string FRUIT_BUNDLE_DELUXE = "fruit_bundle_deluxe";

	public const string FRUIT_BUNDLE_MEGA = "fruit_bundle_mega";

	public const string FRUIT_BUNDLE_ULTIMATE = "fruit_bundle_ultimate";

	public const string FRUIT_BUNDLE_SUPREME = "fruit_bundle_supreme";

	public static bool PurchaseSimulation;

	private IStoreController storeController;

	private IGooglePlayStoreExtensions googlePlayStoreExtensions;

	public static FCCMMLGODPL lastPurchaseActScene;

	public bool Ready { get; private set; }

	public void Init()
	{
	}

	public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
	{
	}

	public void OnInitializeFailed(InitializationFailureReason error)
	{
	}

	public void OnInitializeFailed(InitializationFailureReason error, string message)
	{
	}

	public void StartPurchase(string InId, FCCMMLGODPL InPurchaseActScene)
	{
	}

	public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
	{
	}

	public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
	{
	}

	public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
	{
		return default(PurchaseProcessingResult);
	}

	private void SimulationPurchaseSuccess(string InProductId)
	{
	}

	public Product GetProductById(string id)
	{
		return null;
	}

	public void Restore()
	{
	}

	public string GetPurchaseReason(string InProductID)
	{
		return null;
	}
}
