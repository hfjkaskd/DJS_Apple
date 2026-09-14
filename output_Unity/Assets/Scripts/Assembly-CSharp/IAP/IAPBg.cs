using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

namespace IAP
{
	public class IAPBg : MonoBehaviour
	{
		private sealed class JOKHGLLDJOO : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public IAPBg _003C_003E4__this;

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
			public JOKHGLLDJOO(int _003C_003E1__state)
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
		private Image m_MidIcon;

		private bool isOpening;

		private const float SWITCH_ICON_TIME = 1f;

		private const int ELETYPE_MIN = 0;

		private const int ELETYPE_MAX = 66;

		private int nowShowEle;

		public void Show()
		{
		}

		[IteratorStateMachine(typeof(JOKHGLLDJOO))]
		private IEnumerator StartIdleAni()
		{
			return null;
		}

		public void Close(bool success, PurchaseFailureReason reason)
		{
		}

		private int GetNextShowEleType()
		{
			return 0;
		}
	}
}
