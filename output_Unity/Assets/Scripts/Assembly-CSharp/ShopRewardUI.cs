using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ShopRewardUI : BaseUI
{
	private sealed class PHFAHBFLLHG
	{
		public ShopRewardItem item;

		internal void _003COnClaimBtnClick_003Eb__1()
		{
		}
	}

	private sealed class LJHLONILCFF : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public ShopRewardUI _003C_003E4__this;

		private FPHGLNCPHJP _003CgiftReward_003E5__2;

		private COAEOCCIEAO _003CcoinReward_003E5__3;

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
		public LJHLONILCFF(int _003C_003E1__state)
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

	[SerializeField]
	private Animation m_ShopRewardUIAni;

	[SerializeField]
	private string m_DisappearAniName;

	[SerializeField]
	private GameObject m_ClaimBtn;

	[SerializeField]
	private Transform m_ClaimBtnPos4Item;

	[SerializeField]
	private Transform m_ClaimBtnPosCoin;

	[SerializeField]
	private Transform m_CoinPos;

	[SerializeField]
	private List<Transform> m_ItemPosList;

	[SerializeField]
	private float m_SingleRewardDelay;

	[SerializeField]
	private float m_FourRewardsDelay;

	[SerializeField]
	private float m_PerRewardsGap;

	[SerializeField]
	private float m_DelayForEffectTime;

	private const int FLY_COIN_NUM = 20;

	private static readonly float[] FlyDirections;

	private const float FLY_SAFETY_TIMEOUT = 5f;

	private List<ShopRewardItem> rewardItems;

	private bool isGiftPack;

	private int flyCompletedCount;

	private bool isCleaning;

	public override PAIEAGDLCBJ Layer => default(PAIEAGDLCBJ);

	protected override void Init()
	{
	}

	protected override void BeforeOpen()
	{
	}

	private void Clear()
	{
	}

	[IteratorStateMachine(typeof(LJHLONILCFF))]
	private IEnumerator SetupRewardUI()
	{
		return null;
	}

	private void CreateItem(POJCEPBNNIP itemType, int count, Transform parent)
	{
	}

	private void OnClaimBtnClick(GameObject go)
	{
	}

	private Vector3 GetFlyTargetPos(POJCEPBNNIP itemType)
	{
		return default(Vector3);
	}

	private void OnSingleFlyComplete()
	{
	}

	private void CleanAndClose()
	{
	}
}
