using System;
using System.Runtime.CompilerServices;

public class MFEGLMEDNLA
{
	private BIDJKEKHFDE curIAD_main;

	private BIDJKEKHFDE curIAD_sub;

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

	public MFEGLMEDNLA(OGLPGNHCOGO mainType, string mainUnitId, OGLPGNHCOGO subType, string subUnitId)
	{
	}

	private static BIDJKEKHFDE CreateIAD(OGLPGNHCOGO mediType, string unitId)
	{
		return null;
	}

	private void CheckIadEvent(bool available)
	{
	}

	public bool HasIAD()
	{
		return false;
	}

	public static void BeginLoad()
	{
	}

	public static void StopLoad()
	{
	}

	public void ShowIAD(Action<string> beforeAction, Action<string, string> showedAction, Action<string, string> clickAction, Action<bool> closeAction)
	{
	}

	private BIDJKEKHFDE GetBestIAD()
	{
		return null;
	}

	public void DoUpdate()
	{
	}

	private void CheckNextLoad(BIDJKEKHFDE curIAD, OGLPGNHCOGO mediType, float deltaTime)
	{
	}

	private double GetPeerValidEcpm(BIDJKEKHFDE targetIAD)
	{
		return 0.0;
	}

	private static bool HasComparableRevenue(BIDJKEKHFDE iad)
	{
		return false;
	}

	private float GetDelayInterval(BIDJKEKHFDE curIAD)
	{
		return 0f;
	}

	private void CheckOverTime(BIDJKEKHFDE curIAD, float deltaTime)
	{
	}

	private void CheckFixFake(BIDJKEKHFDE curIAD, float deltaTime)
	{
	}

	public void CheckFixOverTime()
	{
	}

	public void CheckForceFix()
	{
	}
}
