using System.Collections.Generic;

public class OLMGKOFLIDM
{
	public List<IGOBDCAPLDG> items = new List<IGOBDCAPLDG>();

	public OLMGKOFLIDM Add(POJCEPBNNIP itemType, int count)
	{
		items.Add(new IGOBDCAPLDG(itemType, count));
		return this;
	}
}
