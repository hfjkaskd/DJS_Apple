using System;
using System.Collections.Generic;
using UnityEngine;

namespace WZSDK
{
	public class STimer
	{
		public float nowTime;

		public float targetTime;

		public Action endAction;

		public Action<float> updateAction;

		public List<timingActions> timingActions;

		public List<timingActions> onceActions;

		public bool iftiming;

		public int loopCount;

		public int currentlc;

		public bool ifscale;

		public bool ifAutoPushPool;

		private STimerState sTimerState;

		private bool ifClose;

		public STimerState STimerState => sTimerState;

		public STimer()
		{
			timingActions = new List<timingActions>();
			onceActions = new List<timingActions>();
		}

		public void SetSTimeInfo(float targetTime, int loopCount = 0, bool ifscale = false, bool ifAutoPushPool = true, Action endAction = null, Action<float> updateAction = null, params timingActions[] timingActions)
		{
			ifClose = false;
			nowTime = 0f;
			this.targetTime = targetTime;
			this.loopCount = loopCount;
			currentlc = loopCount;
			this.ifscale = ifscale;
			this.ifAutoPushPool = ifAutoPushPool;
			this.endAction = endAction;
			this.updateAction = updateAction;

			this.timingActions.Clear();
			if (timingActions != null && timingActions.Length > 0)
			{
				this.timingActions.AddRange(timingActions);
			}
			onceActions.Clear();

			if (this.timingActions.Count > 0)
			{
				this.updateAction += HandleTimingActions;
			}
		}

		public void Start()
		{
			STimerManager manager = STimerManager.Instance;
			if (sTimerState != STimerState.Running)
			{
				manager.runingSTimer.AddLast(this);
				manager.pauseSTimer.Remove(this);
			}

			iftiming = true;
			sTimerState = STimerState.Running;
		}

		public void Pause()
		{
			iftiming = false;
			sTimerState = STimerState.Pause;
		}

		public void Stop()
		{
			ReStart();
			iftiming = false;
		}

		public void ReStart()
		{
			nowTime = 0f;
			currentlc = loopCount;
			timingActions.AddRange(onceActions);
			onceActions.Clear();
			Start();
		}

		public void Close()
		{
			if (!ifAutoPushPool)
			{
				ReStart();
				iftiming = false;
				return;
			}

			iftiming = false;
			ifClose = true;
			onceActions.Clear();
			sTimerState = STimerState.Standby;
		}

		public void Update()
		{
			if (!iftiming)
			{
				return;
			}

			if (nowTime <= targetTime)
			{
				updateAction?.Invoke(nowTime);
				nowTime += ifscale ? Time.deltaTime : Time.unscaledDeltaTime;
				return;
			}

			nowTime = targetTime;
			updateAction?.Invoke(nowTime);
			endAction?.Invoke();
			if (currentlc == 0)
			{
				Close();
			}
			else if (currentlc < 1)
			{
				ReStart();
			}
			else
			{
				nowTime = 0f;
				currentlc--;
			}
		}

		private bool CheckClose()
		{
			if (ifClose)
			{
				Debug.LogError("STimer is closed.");
			}
			return ifClose;
		}

		private void HandleTimingActions(float currentTime)
		{
			for (int i = timingActions.Count - 1; i >= 0; i--)
			{
				timingActions action = timingActions[i];
				switch (action.clockActionType)
				{
					case ClockActionType.Once:
						if (action.timing <= currentTime)
						{
							action.clockAction?.Invoke(action.timing);
							onceActions.Add(action);
							timingActions.RemoveAt(i);
						}
						break;
					case ClockActionType.Before:
						if (currentTime <= action.timing)
						{
							action.clockAction?.Invoke(currentTime);
						}
						break;
					case ClockActionType.After:
						if (action.timing <= currentTime)
						{
							action.clockAction?.Invoke(currentTime);
						}
						break;
				}
			}
		}
	}
}
