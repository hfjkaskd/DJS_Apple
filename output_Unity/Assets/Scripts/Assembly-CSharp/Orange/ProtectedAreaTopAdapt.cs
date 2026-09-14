using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Orange
{
	public class ProtectedAreaTopAdapt : MyMonoBehaviour
	{
		private sealed class NNNNDIPKMNG : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public ProtectedAreaTopAdapt _003C_003E4__this;

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
			public NNNNDIPKMNG(int _003C_003E1__state)
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
		private float m_SelfAeraHeight;

		private bool IsReversed;

		private void Start()
		{
			Do();
		}

		/// <summary>
		/// 顶栏穿过刘海区的适配。项目内唯一挂载点（ShopUI/Top）的预制体已烘焙
		/// 适配后数值（高 396 = 设计 296 + 顶部安全区 100，posY -100），与录屏设备
		/// 运行时布局一致，故这里不再叠加，避免二次偏移。
		/// </summary>
		private void Do()
		{
			if (IsReversed)
			{
				return;
			}
			IsReversed = true;
		}

		[IteratorStateMachine(typeof(NNNNDIPKMNG))]
		private IEnumerator Wait()
		{
			return null;
		}
	}
}
