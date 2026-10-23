using System;
using System.Collections.Generic;
using UnityEngine;

namespace WZSDK
{
	public static class FacadeGamePlayExtend
	{
		public static Action<int> OnCountMoveChangeHandle;

		public static Action<int> OnCountRollbackChangeHandle;

		public static Action<int> OnCountRefreshChangeHandle;

		public static Action OnAddSpacePassHandle;

		public static Action OnMinuteChangeHandle;

		public static Func<RectTransform> GetGamePlayMoneyRectHandle;

		public static Func<RectTransform> GetGamePlayDiamondRectHandle;

		public static Func<RectTransform> GetGamePlayMovePropRectHandle;

		public static Func<RectTransform> GetGamePlayRollbackPropRectHandle;

		public static Func<RectTransform> GetGamePlayRefreshPropRectHandle;

		public static Action<int, bool> ChangeCountBoolHandle;

		public static Action OnMoneyFlyFinishHandle;

		public static Action OnDiamondFlyFinishHandle;

		public static Action ShowCompleteMaskHandle;

		public static Action OpenWidthdrawTipHandle;

		public static Action<int> CreateLevelHandle;

		public static Action OnTouchStartHandle;

		public static Action<Collision2D> OnBeginContactHandle;

		public static Action Func_INCREASEHandle;

		public static Action<bool> Func_CLEARHandle;

		public static Action Func_HAMMERHandle;

		public static Action RePlayHandle;

		public static Action<int> SavePaintingDataHandle;

		public static Action<float> SetCurMoneyShowHandle;

		public static Action<float> SetBarValueHandle;

		public static Func<Transform> GetSaoBaHandle;

		public static Func<RectTransform> GetTargetsNodeHandle;

		public static Func<RectTransform> GetLevelRootHandle;

		public static Func<Transform> GetHideTmpNodeHandle;

		public static Func<Transform> GetTmpNodeHandle;

		public static Func<Transform> GetCMDialogTextTransHandle;

		public static Action<int> SetBATCountShowHandle;

		public static Action<int> SetBCTCountShowHandle;

		public static Action<int> SetBRBCountShowHandle;

		public static Action<bool> SetBlockLayerActiveHandle;

		public static Func<Dictionary<ERewardType, Vector3>> GetFlyObjGoldHandle;

		public static Action<bool> SetCurLevelTextHandle;

		public static Action SetWithdrawalTipHandle;

		public static Action CheckWLProgressActiveHandle;

		public static Action<bool> SetCanClickCashBtnHandle;

		public static Func<RectTransform> GetGridContainerHandle;

		public static Func<int, int, Vector3> GetCellPositionHandle;

		public static Func<int, Action<Vector2Int, bool>, (float duration, bool isFinished)> DrawWithColorHandle;

		public static Action<int> FixHistoryHandle;

		public static Action<int> DrawQuickHandle;

		public static Action ClearAllCellsHandle;

		public static Action OnUnlockTmpHandle;

		public static Func<int, string> GetLevelTxtHandle;

		public static Func<int, bool> GetIsInSmallLevelGroupHandle;

		public static Action ReSetRefuseCountHandle;

		public static Func<bool> GetIsVIPHandle;

		public static Action<bool> SetIsVIPHandle;

		public static Func<bool> GetFirstAddHandle;

		public static Action<bool> SetFirstAddHandle;

		public static Action<int> CheckGuideHandle;

		public static Action<bool> SetGetBrb_SpineBgState;

		public static Func<int, List<Vector3>> GetSpoolPos;

		public static Func<List<Transform>> GetLockSpool;

		public static Action ReSetLockSpool;

		public static Func<string> GetLevelProgressText;

		public static Func<string> GetCurMoneyTextText;

		public static Func<string> GetCMDialogTextText;
	}
}
