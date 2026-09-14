using System;
using System.Runtime.CompilerServices;

public class HFAIEGDEABG
{
	private GPJMMMNFOBM curVAD_main;

	private GPJMMMNFOBM curVAD_sub;

	private readonly OGLPGNHCOGO mainMediType;

	private readonly OGLPGNHCOGO subMediType;

	private static bool BeginLoop;

	private bool lastValue;

	private float deltaTime;

	public static event Action<bool> OnADEvent
	{
		[CompilerGenerated]
		add
		{
		}
		[CompilerGenerated]
		remove
		{
		}
	}

	public HFAIEGDEABG(OGLPGNHCOGO mainType, string mainUnitId, OGLPGNHCOGO subType, string subUnitId)
	{
	}

	private static GPJMMMNFOBM CreateVAD(OGLPGNHCOGO mediType, string unitId)
	{
		return null;
	}

	private void CheckVadEvent(bool available)
	{
	}

	public bool HasVAD()
	{
		return false;
	}

	public static void BeginLoad()
	{
	}

	public static void StopLoad()
	{
	}

	public void ShowVAD(Action<string> beforeAction, Action<string, string> showedAction, Action<string, string> rewardAction, Action<string, string> clickAction, Action<bool> closeAction)
	{
	}

	private GPJMMMNFOBM GetBestVAD()
	{
		return null;
	}

	public void DoUpdate()
	{
	}

	private void CheckNextLoad(GPJMMMNFOBM curVAD, OGLPGNHCOGO mediType, float deltaTime)
	{
	}

	private double GetPeerValidEcpm(GPJMMMNFOBM targetVAD)
	{
		return 0.0;
	}

	private static bool HasComparableRevenue(GPJMMMNFOBM vad)
	{
		return false;
	}

	private float GetDelayInterval(GPJMMMNFOBM curVAD)
	{
		return 0f;
	}

	private void CheckOverTime(GPJMMMNFOBM curVAD, float deltaTime)
	{
	}

	private void CheckFixFake(GPJMMMNFOBM curVAD, float deltaTime)
	{
	}

	public void CheckFixOverTime()
	{
	}

	public void CheckForceFix()
	{
	}
}
