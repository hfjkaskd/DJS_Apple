using System;
using com.unity3d.mediation;

public class MgrLevelPlay : BaseSDK
{
	private sealed class AEHANGCHAGE
	{
		public MgrLevelPlay _003C_003E4__this;

		public Action<bool, string> callback;

		internal void _003CConfig_003Eb__0(LevelPlayConfiguration obj)
		{
		}

		internal void _003CConfig_003Eb__1(LevelPlayInitError obj)
		{
		}
	}

	private static MgrLevelPlay _instance;

	private bool configed;

	public static MgrLevelPlay Instance => null;

	public override void ConfigSDK(bool user_deny)
	{
	}

	public override void Config(string sdkKey, bool user_deny, Action<bool, string> callback)
	{
	}

	private void InitAmazon()
	{
	}

	public override void Init(string sdkKey, bool user_deny, Action<bool, string> callback)
	{
	}

	private void configGDPR(bool user_deny)
	{
	}

	private void configCCPA(bool user_deny)
	{
	}

	private void configCOPPA()
	{
	}

	private void configSubADS(bool user_deny)
	{
	}

	private void configPauseGameForiOS()
	{
	}

	private void configPrivacyForiOS()
	{
	}

	private void listenImpression()
	{
	}

	private void OnApplicationPause(bool isPaused)
	{
	}
}
