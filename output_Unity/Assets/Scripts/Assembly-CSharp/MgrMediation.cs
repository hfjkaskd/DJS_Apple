using System;
using UnityEngine;

public class MgrMediation : MonoBehaviour
{
	private enum FIADGKCOMLI
	{
		None = 0,
		Loading = 1,
		Success = 2
	}

	private class NADFPHNMODF
	{
		public OGLPGNHCOGO Type;

		public BaseSDK TargetSDK;

		public FIADGKCOMLI Status;

		public int ContinueFailedCount;

		public float LastCheckRetryTimer;

		public bool FailedReported;

		public readonly string Name;

		public bool Enabled => false;

		public NADFPHNMODF(string name)
		{
		}

		public void Reset(OGLPGNHCOGO type)
		{
		}

		public void Disable()
		{
		}
	}

	private sealed class LDMBBLMPFNB
	{
		public MgrMediation _003C_003E4__this;

		public NADFPHNMODF lane;

		internal void _003CDoInit_003Eb__0(bool initSuccess, string errorMsg)
		{
		}
	}

	private static MgrMediation _instance;

	private bool Begin_Loop;

	private bool inner_user_deny;

	private Action inner_success_callback;

	private bool any_ready_reported;

	private readonly NADFPHNMODF mainLane;

	private readonly NADFPHNMODF subLane;

	public static MgrMediation Instance => null;

	private static string GetLogContext(NADFPHNMODF lane)
	{
		return null;
	}

	private void OnInitCallback(NADFPHNMODF lane, bool initSuccess, string errorMsg)
	{
	}

	public void Init(OGLPGNHCOGO mainMediType, OGLPGNHCOGO subMediType, bool user_deny, Action success_callback)
	{
	}

	private void DoInit(NADFPHNMODF lane)
	{
	}

	public void configSDK(bool user_deny)
	{
	}

	public bool IsMediationReady(OGLPGNHCOGO mediType)
	{
		return false;
	}

	private void UpdateLane(NADFPHNMODF lane)
	{
	}

	private void Update()
	{
	}
}
