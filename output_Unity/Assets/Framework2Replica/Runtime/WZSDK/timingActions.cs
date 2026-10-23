using System;

namespace WZSDK
{
	public struct timingActions
	{
		public float timing;

		public Action<float> clockAction;

		public ClockActionType clockActionType;
	}
}
