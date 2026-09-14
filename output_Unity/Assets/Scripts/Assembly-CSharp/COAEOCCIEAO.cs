using System.Collections.Generic;

/// <summary>金币包商品配置。</summary>
public class COAEOCCIEAO
{
	public string productID;

	public int coinCount;

	public COAEOCCIEAO(string InProductID, int InCoinCount)
	{
		productID = InProductID;
		coinCount = InCoinCount;
	}

	public static List<COAEOCCIEAO> GetCoinPackRewards()
	{
		return new List<COAEOCCIEAO>
		{
			new COAEOCCIEAO("coinpack_1", 300),
			new COAEOCCIEAO("coinpack_2", 800),
			new COAEOCCIEAO("coinpack_3", 1800),
			new COAEOCCIEAO("coinpack_4", 3800),
			new COAEOCCIEAO("coinpack_5", 8000),
			new COAEOCCIEAO("coinpack_6", 20000)
		};
	}
}
