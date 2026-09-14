using IAP;
using UnityEngine.Purchasing;

public class HBEFMOOIDDG : global::IAJDGNOGFGO<IapVerifySaveData, HBEFMOOIDDG>
{
	private sealed class CFIOFCLHMKC
	{
		public HBEFMOOIDDG _003C_003E4__this;

		public IapTransactionData InTransactionData;

		public string encryptText;

		internal void _003CTryIapTransactionVerifyRequest_003Eb__0(long code, string text)
		{
		}
	}

	private const string JsonKeyPayload = "Payload";

	private const string JsonKeyJson = "json";

	private const string JsonKeyPurchaseToken = "purchaseToken";

	private const string JsonKeyOrderId = "orderId";

	private const string ResponseKeyStatus = "status";

	private const string ResponseKeyIsValid = "isValid";

	private const string ResponseKeyTransactionID = "transactionID";

	private const string IAP_TRANSACTION_ENCRYPT_KEY = "c92cWRxPfHn1sQUxeU60";

	public bool IsCheatUser => false;

	protected override string GetKey()
	{
		return null;
	}

	public override void Init()
	{
	}

	public void Add(string purchaseId, Product product)
	{
	}

	private void CheckAll()
	{
	}

	private void TryIapTransactionVerifyRequest(IapTransactionData InTransactionData)
	{
	}

	private string EncryptWithXORAndBase64(string input, string key)
	{
		return null;
	}

	private void OnTransactionVerifyFinish(IapTransactionData InTransactionData, string InTransId, string InEncryptedOrderInfo, bool InIsValid, int InStatus)
	{
	}

	private void LogPrice(string purchaseId)
	{
	}
}
