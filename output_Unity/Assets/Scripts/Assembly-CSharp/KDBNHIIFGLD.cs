public class KDBNHIIFGLD : global::IAJDGNOGFGO<GKLEFBIMILH, KDBNHIIFGLD>
{
	public int InsertFinishedCount => data.insertFinishedCount;

	public override void Init()
	{
		base.Init();
	}

	public void UpdateRegisterTime()
	{
		data.registerTime = (int)(System.DateTime.UtcNow - new System.DateTime(1970, 1, 1)).TotalSeconds;
		SaveData();
	}

	public void AddInsertFinishedCount()
	{
		data.insertFinishedCount++;
		SaveData();
	}

	protected override string GetKey()
	{
		return "MiscModel";
	}
}
