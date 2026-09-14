using System;
using System.Collections.Generic;

namespace IAP
{
	[Serializable]
	public class IapVerifySaveData
	{
		public List<IapTransactionData> transactionList;

		public bool isCheatUser;
	}
}
