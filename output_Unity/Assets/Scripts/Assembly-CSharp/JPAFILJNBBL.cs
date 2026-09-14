using System.Collections.Generic;

public class JPAFILJNBBL
{
	private static List<JPAFILJNBBL> pool;

	private static List<KGLCJHCMEGJ> listPool;

	public string Name { get; private set; }

	public object Value { get; private set; }

	public bool locked { get; private set; }

	public JPAFILJNBBL(string name, string value)
	{
	}

	public JPAFILJNBBL(string name, long value)
	{
	}

	public JPAFILJNBBL(string name, int value)
	{
	}

	public JPAFILJNBBL(string name, double value)
	{
	}

	public JPAFILJNBBL(string name, bool value)
	{
	}

	public static JPAFILJNBBL GetMyAnaParam(string name, string value)
	{
		return null;
	}

	public static JPAFILJNBBL GetMyAnaParam(string name, long value)
	{
		return null;
	}

	public static JPAFILJNBBL GetMyAnaParam(string name, int value)
	{
		return null;
	}

	public static JPAFILJNBBL GetMyAnaParam(string name, double value)
	{
		return null;
	}

	public static JPAFILJNBBL GetMyAnaParam(string name, bool value)
	{
		return null;
	}

	public static void Recycle(JPAFILJNBBL myAnaParam)
	{
	}

	public static KGLCJHCMEGJ GetList()
	{
		return null;
	}

	public static void RecycleList(KGLCJHCMEGJ myAnaParams)
	{
	}

	public static void ClearList(KGLCJHCMEGJ myAnaParams)
	{
	}
}
