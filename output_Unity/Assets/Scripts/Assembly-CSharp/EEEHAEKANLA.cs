using System;
using UnityEngine;

/// <summary>全局存档模型：注册时间、版本、登录时间。</summary>
public class EEEHAEKANLA : global::IAJDGNOGFGO<GlobalModelData, EEEHAEKANLA>
{
	public const string FORMAT = "yyyy-MM-dd HH:mm:ss";

	public GlobalModelData Data => data;

	public bool IsFirstColdStart { get; private set; }

	protected override string GetKey()
	{
		return "GlobalModel";
	}

	public override void Init()
	{
		base.Init();
		IsFirstColdStart = isFirstInit;
		AddLogTime();
	}

	protected override void AfterFirstInitData()
	{
		DateTime now = DateTime.Now;
		data.registTime = now.ToString(FORMAT);
		data.registTimeUtc8 = now.ToUniversalTime().AddHours(8.0).ToString(FORMAT);
		data.logTime = now.Ticks;
		int version = 0;
		string[] parts = Application.version.Split('.');
		foreach (string p in parts)
		{
			if (int.TryParse(p, out int v))
			{
				version = version * 100 + v;
			}
		}
		data.registVersion = version;
	}

	public DateTime GetRegistTime()
	{
		if (DateTime.TryParseExact(data.registTime, FORMAT, null, System.Globalization.DateTimeStyles.None, out DateTime t))
		{
			return t;
		}
		return DateTime.Now;
	}

	public DateTime GetRegistTimeUtc8()
	{
		if (DateTime.TryParseExact(data.registTimeUtc8, FORMAT, null, System.Globalization.DateTimeStyles.None, out DateTime t))
		{
			return t;
		}
		return DateTime.UtcNow.AddHours(8.0);
	}

	public void AddLogTime()
	{
		data.logTime = DateTime.Now.Ticks;
		SaveData();
	}

	public int GetUserRegisterVersion()
	{
		return data.registVersion;
	}

	public void SetUserRegisterVersionBackDoor(int InVersion)
	{
		data.registVersion = InVersion;
		SaveData();
	}

	public void SetRegistTimeBackDoor(DateTime inTime)
	{
		data.registTime = inTime.ToString(FORMAT);
		SaveData();
	}
}
