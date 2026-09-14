using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Project.Logic;
using UnityEngine;

public class DPANDGJGLLJ : global::IAJDGNOGFGO<AppServiceRecordData, DPANDGJGLLJ>
{
	private sealed class NKIEBJEJMEL : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public bool isStart;

		public DPANDGJGLLJ _003C_003E4__this;

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
		public NKIEBJEJMEL(int _003C_003E1__state)
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

	private bool isAppPause;

	private bool _isNewInstall;

	private string sessionId;

	private static string userGuid;

	private bool _installByUpdate;

	private string _lastCloseTag;

	private const string AgreePolicyKey = "agree_policy";

	private const string DeletePrivacyKey = "delete_privacy";

	private float sessionPurePlaySeconds;

	private bool checkViewVisible;

	private float lastCheckTime;

	private Coroutine checkPlaySecondsCoroutine;

	private AppServiceRecordData recordData => null;

	public AppServiceRecordData Data => null;

	public string SessionId
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public string UserGuid => null;

	public bool IsNewInstall => false;

	public bool InstallByUpdate => false;

	public int Version => 0;

	public int PreviousVersion => 0;

	public int InstallVersion => 0;

	public int AppOpenCount => 0;

	public int FristInstallTime => 0;

	public int TodayOpenCount => 0;

	public int LocalTodayOpenCount => 0;

	public int LastOpenTime => 0;

	public int LastInactiveSeconds => 0;

	public int AppLaunchCount => 0;

	public string LastCloseTag
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public int SessionPlaySeconds => 0;

	public int PureSessionPlaySeconds => 0;

	public int TotalPlaySeconds => 0;

	public int LastLoggedActSeconds => 0;

	public int LastLoggedContinueDaySinceInstall => 0;

	public int LastLoggedDaySinceInstall => 0;

	public int TodayLaunchCount => 0;

	public int TodayPlaySeconds => 0;

	public int TotalPurePlaySec => 0;

	public string SystemLanguage => null;

	public bool AutoTrack { get; set; }

	public int LastUpdateDialogShowTime
	{
		get
		{
			return 0;
		}
		set
		{
		}
	}

	public bool IsUserReturn => false;

	public int PlayDay => 0;

	public void CheckGenerateGUIDForNewUser()
	{
	}

	public override void Init()
	{
	}

	private void AppUpdateLogic(int lastVer, int nowVer)
	{
	}

	private void ProcessAfterLoadRecord()
	{
	}

	private void SetFirstLogMainLevelTime()
	{
	}

	public bool CheckCanLogMainLevel()
	{
		return false;
	}

	public AppServiceRecordData GetAppServiceData()
	{
		return null;
	}

	public void SetUserID(string strUserIDFromServer)
	{
	}

	public void SetDebugUser(bool isDebug)
	{
	}

	public void SetCountryName(string strCountryCode)
	{
	}

	private void InitRecord()
	{
	}

	private void AddOpenCount()
	{
	}

	private void EnterNewUtcDay()
	{
	}

	private void EnterNewLocalDay()
	{
	}

	private void AddLaunchCount()
	{
	}

	public void OnLoadingBarFinished()
	{
	}

	private void OnAppPauseOrResume(bool isPause)
	{
	}

	private void OnLocalDayChanged(int days)
	{
	}

	public void OnActHourLog()
	{
	}

	public void OnContinueDayLog()
	{
	}

	public void OnLoginDayLog()
	{
	}

	public string GetClientId()
	{
		return null;
	}

	public string GetVisitorId()
	{
		return null;
	}

	public DateTime InstallDate()
	{
		return default(DateTime);
	}

	public int DayCountSinceInstall()
	{
		return 0;
	}

	public void UpdateLastLaunchTime()
	{
	}

	public bool GetUserReturnStatus()
	{
		return false;
	}

	public static bool GetAgreePolicy()
	{
		return false;
	}

	public static void SetAgreePolicy()
	{
	}

	public static bool GetDeletePrivacy()
	{
		return false;
	}

	public static void SetDeletePrivacy()
	{
	}

	public void SetCheckViewVisible(bool isCheck)
	{
	}

	private bool IsViewVisible()
	{
		return false;
	}

	[IteratorStateMachine(typeof(NKIEBJEJMEL))]
	private IEnumerator CheckPlaySeconds(bool isStart)
	{
		return null;
	}

	public float GetSessionPurePlaySeconds()
	{
		return 0f;
	}

	public void SetAdjustAttributioninfo(string attributioninfo)
	{
	}

	public string GetAdjustAttributioninfo()
	{
		return null;
	}

	public void SetAlreadySendAdjustLogInfo(int alreadySendAdjustLogInfo)
	{
	}

	public int GetAlreadySendAdjustGTAInfo()
	{
		return 0;
	}

	protected override string GetKey()
	{
		return null;
	}
}
