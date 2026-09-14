using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using ABTest.Core;
using Framework.ABTest.Core;

public static class LFFDOIDEPMD
{
	private class FBIONIHJLEM : IBridgeInterface
	{
		private sealed class FCNNCHHPHGM
		{
			public Action<long> InOnFail;

			internal void _003CPostEventFunc_003Eb__0(long InLong)
			{
			}
		}

		private sealed class APNKIMCGKBG
		{
			public Action<string> InOnSuccess;

			public Action<long> InOnFail;

			internal void _003CPostHttpFunc_003Eb__0(long l, string s)
			{
			}

			internal void _003CPostHttpFunc_003Eb__1(long l)
			{
			}
		}

		public int GetAppVersionFunc()
		{
			return 0;
		}

		public int GetConditionProcessFunc(string InLayerKey)
		{
			return 0;
		}

		public ABTestTagEnums.CountryTag GetCountryTagFunc()
		{
			return default(ABTestTagEnums.CountryTag);
		}

		public string GetDeviceRegionCode()
		{
			return null;
		}

		public int GetEffectiveProcessFunc(string InLayerKey)
		{
			return 0;
		}

		public string GetEventPostStrFunc(string InEventName, List<KeyValuePair<string, string>> InEventParams)
		{
			return null;
		}

		public int GetInstallVersionFunc()
		{
			return 0;
		}

		public int GetMainLevelIndexFunc()
		{
			return 0;
		}

		public string GetMCCFunc()
		{
			return null;
		}

		public string GetMonoTsParams()
		{
			return null;
		}

		public string GetUserIdFunc()
		{
			return null;
		}

		public int GetUserTypeSafeFunc()
		{
			return 0;
		}

		public bool IsLogEvent()
		{
			return false;
		}

		public bool IsTestMode()
		{
			return false;
		}

		public void PostEventFunc(string InData, Action<long> InOnFail)
		{
		}

		public void PostHttpFunc(string InLink, string InData, Action<string> InOnSuccess, Action<long> InOnFail)
		{
		}

		public bool UseTestLink()
		{
			return false;
		}
	}

	private sealed class HPDPIOCNMAB : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return null;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return null;
			}
		}

		[DebuggerHidden]
		public HPDPIOCNMAB(int _003C_003E1__state)
		{
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
		}

		private bool MoveNext()
		{
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
		}
	}

	private static ABTestTagEnums.CountryTag backDoorCountryTag;

	private static int backDoorUserTag;

	private static string backDoorAbMcc;

	public static bool IfUseBackDoorForABgroup;

	public static bool IfUseBackDoorRandomUserId;

	public static string randomDeviceInfo;

	public const float WAIT_SYNC_EXP_TIME = 2f;

	private static bool inited;

	private static readonly List<(string, string)> syncNames;

	private static readonly List<string> needLevelProgressLayer;

	private static Dictionary<string, int> ExpCanQueryGroupDic;

	private static Dictionary<string, int> ExpGroupDic;

	private const int FIRST_QUERY_COUNT = 3;

	public static bool UseBackDoorGroup;

	private static readonly List<(string, string)> NeedCheckGroupExps;

	private static readonly List<int> ToRemoveGroupIndexs;

	public static int ADBugFix104 => 0;

	public static int NewBeginner104 => 0;

	public static int MoreTree104 => 0;

	public static int Encourage104 => 0;

	public static int NewItemGuider104 => 0;

	public static int AddToy105 => 0;

	public static int DownloadTree105 => 0;

	public static int BeforeLevelTips105 => 0;

	public static int Shuffle105 => 0;

	public static void SetCountryTagByBackDoor(ABTestTagEnums.CountryTag InBackDoorCountryTag)
	{
	}

	public static void SetUserTagByBackDoor(int InBackDoorUserTag)
	{
	}

	public static void SetAbMccByBackDoor(string InAbMcc)
	{
	}

	public static void Init()
	{
	}

	[IteratorStateMachine(typeof(HPDPIOCNMAB))]
	private static IEnumerator InitCoroutine()
	{
		return null;
	}

	private static bool HasSyncNotGrouped()
	{
		return false;
	}

	private static void RefreshExpQuery()
	{
	}

	public static void SetForBackDoor(string InABExpName)
	{
	}

	private static int GetExpGroup(string InBackDoorSaveKey, string InABLayer, string InABExpName)
	{
		return 0;
	}

	private static int GetExpGroupWhenDeveloping(string InBackDoorSaveKey, string InABLayer, string InABExpName, bool InIsV2AB = false)
	{
		return 0;
	}

	public static string GetDeviceIdHash()
	{
		return null;
	}

	private static int QueryGroupByLayer(string InLayerKey, string InExperimentKey)
	{
		return 0;
	}

	public static void TryCheckCanQueryGroup()
	{
	}

	public static int GetConditionProcessFunc(string InLayerKey)
	{
		return 0;
	}

	public static int GetEffectiveProcessFunc(string InLayerKey)
	{
		return 0;
	}
}
