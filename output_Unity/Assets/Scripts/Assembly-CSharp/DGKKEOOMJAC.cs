using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Notifications.Android;

public class DGKKEOOMJAC
{
	private sealed class EHDOMOGEMJG : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		private PermissionRequest _003Crequest_003E5__2;

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
		public EHDOMOGEMJG(int _003C_003E1__state)
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

	private const string ChannelId = "1107";

	private const string ChannelName = "Triple Harvest Core";

	private const string ChannelDes = "Game Notification Core Function";

	private const string GroupName = "Triple Harvest";

	private AndroidNotificationChannel channel;

	private bool channelReady;

	private int id;

	public void CancelAllNotifications()
	{
	}

	private void InitChannel()
	{
	}

	public void SendOne(KGMKANCOBFP one)
	{
	}

	private void SendDailyRepeat(KGMKANCOBFP one)
	{
	}

	private void SendDelay(KGMKANCOBFP one)
	{
	}

	private void SendImmediately(KGMKANCOBFP one)
	{
	}

	[IteratorStateMachine(typeof(EHDOMOGEMJG))]
	public static IEnumerator RequestNotificationPermission()
	{
		return null;
	}

	public static bool CheckNotifyIsAllowed()
	{
		return false;
	}

	public static bool IsAndroid13OrNewer()
	{
		return false;
	}

	public static void JumpToAppNotifyPermissionRequest()
	{
	}
}
