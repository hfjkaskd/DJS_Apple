using System;
using System.Runtime.CompilerServices;

public abstract class BIDJKEKHFDE
{
	private class OCHGEBFAFJM : HOPKEMDHFIM.PNCNLFMLLPF
	{
		private BIDJKEKHFDE parent;

		public OCHGEBFAFJM(BIDJKEKHFDE parent)
		{
		}

		void HOPKEMDHFIM.PNCNLFMLLPF.onAdLoaded(string network, double revenue, string msg)
		{
		}

		void HOPKEMDHFIM.PNCNLFMLLPF.onAdFailedToLoad(string msg)
		{
		}

		void HOPKEMDHFIM.PNCNLFMLLPF.onAdShowed(string network, string msg)
		{
		}

		void HOPKEMDHFIM.PNCNLFMLLPF.onAdFailedToShow(string network, int error_code, string msg)
		{
		}

		void HOPKEMDHFIM.PNCNLFMLLPF.onAdClosed(string network)
		{
		}

		void HOPKEMDHFIM.PNCNLFMLLPF.onAdClicked(string network, string msg)
		{
		}

		void HOPKEMDHFIM.PNCNLFMLLPF.onAdInfoChanged(string network, double revenue)
		{
		}

		private void closeLogic(bool normal_close)
		{
		}
	}

	public JBKFEONJCPC.EALPAFECMGG adStatus;

	public float loadedTime;

	public float showBeginTime;

	public float lastCheckLoadTimer;

	public float lastCheckOverTimer;

	public float lastCheckFakeTimer;

	private Action<string, string> showedAction;

	private Action<string, string> clickAction;

	private Action<bool> closeAction;

	public int continue_failed_count;

	public bool firstLoad;

	public HOPKEMDHFIM.PNCNLFMLLPF iadCallback;

	public string unitID;

	private string curNetwork;

	public double curRevenue;

	private double loadBidFloorEcpm;

	private string logContext;

	public event Action<bool> OnADEvent
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

	public abstract bool inner_isReady(string unitID);

	public abstract void inner_load(string unitId, double bidFloorEcpm);

	public abstract void inner_show(string unitID);

	public abstract void inner_destroy();

	public abstract bool inner_reusable();

	protected virtual bool inner_support_bid_floor()
	{
		return false;
	}

	public BIDJKEKHFDE(string id)
	{
	}

	public void SetLogContext(string context)
	{
	}

	private string GetLogContext()
	{
		return null;
	}

	private void Log(string msg)
	{
	}

	private void LogError(string msg)
	{
	}

	public bool isValid()
	{
		return false;
	}

	public void load(double bidFloorEcpm)
	{
	}

	private static bool IsValidBidFloorEcpm(double bidFloorEcpm)
	{
		return false;
	}

	private double CorrectRevenueByBidFloor(double revenue)
	{
		return 0.0;
	}

	private static bool IsMeaningfullyBelowFloor(double revenue, double floorRevenue)
	{
		return false;
	}

	private static string GetShortfallLog(string adType, double revenue, double floorRevenue)
	{
		return null;
	}

	public void show(Action<string> beforeAction, Action<string, string> showedAction, Action<string, string> clickAction, Action<bool> closeAction)
	{
	}

	public void checkDestroy(bool InForceDestroy)
	{
	}

	public void checkFixShowing()
	{
	}

	public void checkFixOverTime()
	{
	}

	public void checkFakeIAD()
	{
	}
}
