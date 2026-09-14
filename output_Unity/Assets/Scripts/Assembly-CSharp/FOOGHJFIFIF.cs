using System;
using System.Runtime.CompilerServices;

public abstract class FOOGHJFIFIF
{
	private class ANMAFOLHBKI : HOPKEMDHFIM.CPHPOKCFEPP
	{
		private FOOGHJFIFIF parent;

		public ANMAFOLHBKI(FOOGHJFIFIF parent)
		{
		}

		void HOPKEMDHFIM.CPHPOKCFEPP.onAdLoaded(string network, string msg)
		{
		}

		void HOPKEMDHFIM.CPHPOKCFEPP.onAdFailedToLoad(string msg)
		{
		}

		void HOPKEMDHFIM.CPHPOKCFEPP.onAdClicked(string network, string msg)
		{
		}
	}

	public JBKFEONJCPC.HBJFLCPJCAI.MLDICCENFAG loadStatus;

	public JBKFEONJCPC.HBJFLCPJCAI.IKJGIKKMBKK lifeStatus;

	public float lastCheckLoadTimer;

	public int continue_failed_count;

	public bool firstLoad;

	public Action<string, string> clickAction;

	public HOPKEMDHFIM.CPHPOKCFEPP badCallback;

	protected string unitID;

	protected PBJIHLKAFGG badSize;

	private string logContext;

	public event Action<JBKFEONJCPC.HBJFLCPJCAI.COMDLKINBAI> OnADEvent
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

	public abstract bool inner_auto_retry();

	public abstract bool inner_auto_show();

	public abstract void inner_load(string unitID);

	public abstract void inner_show(string unitID);

	public abstract void inner_hide(string unitID);

	public abstract void inner_destroy(string unitID);

	public abstract float inner_adaptive_height_after_init(PBJIHLKAFGG size);

	public FOOGHJFIFIF(string unit, PBJIHLKAFGG size)
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

	public bool isShowing()
	{
		return false;
	}

	public void load()
	{
	}

	public void show()
	{
	}

	public void hide()
	{
	}

	public void destroy()
	{
	}
}
