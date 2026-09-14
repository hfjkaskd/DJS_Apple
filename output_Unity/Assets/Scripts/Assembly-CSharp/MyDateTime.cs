using System;

[Serializable]
public class MyDateTime
{
	public long time;

	private static readonly DateTime NoneTime;

	public MyDateTime(int year, int month, int day, int hour = 0, int minute = 0, int second = 0)
	{
	}

	public MyDateTime(DateTime dateTime)
	{
	}

	public MyDateTime(long time)
	{
	}

	public void Update(DateTime dateTime)
	{
	}

	public DateTime ConvertToDateTime()
	{
		return default(DateTime);
	}

	public bool Equals(MyDateTime other)
	{
		return false;
	}
}
