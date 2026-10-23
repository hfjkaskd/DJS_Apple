using System;
using UnityEngine;

namespace WZSDK
{
	public static class FacadeEffectExtend
	{
		public static Action<ERewardItemStruct[], Action> PlayGetRewardEffectHandle;

		public static Action PlayCongratulationEffectHandle;

		public static Action<Transform, int, float, Action, ERewardType> PlayFlyMoneyHandle;

		public static Action<float, ERewardType> PlayFlyMoneyTipHandle;
	}
}
