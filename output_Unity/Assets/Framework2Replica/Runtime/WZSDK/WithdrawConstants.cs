namespace WZSDK
{
	public static class WithdrawConstants
	{
		public const string CurrencyUsd = "USD";

		public const string WalletIncomeTypeNormal = "NORMAL";

		public const string WalletIncomeTypeLucky = "LUCKY";

		public const string WithdrawTypeFirstAd = "FIRST_AD";

		public const string WithdrawTypeWallet = "WALLET";

		public const string RecipientTypeEmail = "EMAIL";

		public const string RecipientTypePix = "PIX";

		public static string GetCurrentRecipientType()
		{
			switch (Framework2Host.PaymentChannel?.InputKind)
			{
				case WaterSort.WaterSortAccountInputKind.Pix: return RecipientTypePix;
				case WaterSort.WaterSortAccountInputKind.Phone: return "PHONE";
				case WaterSort.WaterSortAccountInputKind.Account: return "ACCOUNT";
				default: return RecipientTypeEmail;
			}
		}
	}
}
