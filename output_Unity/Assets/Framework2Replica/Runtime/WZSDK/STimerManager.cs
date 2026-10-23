using System;
using System.Collections.Generic;

namespace WZSDK
{
	public class STimerManager : Singleton<STimerManager>, ILoad, IDispose
	{
		public LinkedList<STimer> runingSTimer;

		public LinkedList<STimer> pauseSTimer;

		public Stack<STimer> standbySTimer;

		public void Load()
		{
			runingSTimer = new LinkedList<STimer>();
			pauseSTimer = new LinkedList<STimer>();
			standbySTimer = new Stack<STimer>();
		}

		public STimer CreateSTimer(float targetTime, int loopCount = 0, bool ifscale = false, bool ifAutoPushPool = true, Action endAction = null, Action<float> updateAction = null, params timingActions[] timingActions)
		{
			STimer timer = standbySTimer.Count > 0 ? standbySTimer.Pop() : new STimer();
			timer.SetSTimeInfo(targetTime, loopCount, ifscale, ifAutoPushPool, endAction, updateAction, timingActions);
			timer.Start();
			return timer;
		}

		public STimer CreateSDelay(float targetTime, Action endAction)
		{
			return CreateSTimer(targetTime, 0, true, true, endAction);
		}

		public STimer CreateSUpdate(float targetTime, Action<float> updateAction)
		{
			return CreateSTimer(targetTime, 0, false, true, null, updateAction);
		}

		public void ClearSTimer()
		{
			runingSTimer.Clear();
			pauseSTimer.Clear();
			standbySTimer.Clear();
		}

		private void UpdateSTimer()
		{
			LinkedListNode<STimer> node = runingSTimer.Last;
			while (node != null)
			{
				LinkedListNode<STimer> previous = node.Previous;
				STimer timer = node.Value;
				if (timer == null)
				{
					runingSTimer.Remove(node);
				}
				else
				{
					switch (timer.STimerState)
					{
						case STimerState.Standby:
							standbySTimer.Push(timer);
							runingSTimer.Remove(node);
							break;
						case STimerState.Pause:
							if (!pauseSTimer.Contains(timer))
							{
								pauseSTimer.AddLast(timer);
							}
							runingSTimer.Remove(node);
							break;
						case STimerState.Running:
							timer.Update();
							break;
					}
				}
				node = previous;
			}
		}

		public void UpdateInstance()
		{
			UpdateSTimer();
		}

		public void Dispose()
		{
			ClearSTimer();
		}
	}
}
