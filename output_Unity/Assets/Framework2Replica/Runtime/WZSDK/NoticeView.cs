using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	public class NoticeView : BaseView
	{
		[CompilerGenerated]
		private sealed class _003CHideAfterDelay_003Ed__7 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public float delay;

			public NoticeView _003C_003E4__this;

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
			public _003CHideAfterDelay_003Ed__7(int _003C_003E1__state)
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

		public Text noticeText;

		public float duration = 2f;

		private Coroutine _hideCoroutine;

		public override void InitView()
		{
		}

		public override void Start()
		{
		}

		public override void Update()
		{
		}

		protected override void HandleViewArgs(object[] args)
		{
			if (args == null || args.Length == 0)
			{
				return;
			}

			object message = args[0];
			if (message == null)
			{
				return;
			}

			if (noticeText != null)
			{
				noticeText.text = message.ToString();
			}

			StopHideCoroutine();
			_hideCoroutine = StartCoroutine(HideAfterDelay(duration));
		}

		[IteratorStateMachine(typeof(_003CHideAfterDelay_003Ed__7))]
		private IEnumerator HideAfterDelay(float delay)
		{
			yield return new WaitForSeconds(delay);

			if (this != null && gameObject != null && UIManager.instance != null)
			{
				UIManager.instance.CloseView(EUIType.NoticeView);
			}

			_hideCoroutine = null;
		}

		private void OnDisable()
		{
			StopHideCoroutine();
		}

		private void OnDestroy()
		{
			StopHideCoroutine();
		}

		private void StopHideCoroutine()
		{
			if (_hideCoroutine == null)
			{
				return;
			}

			StopCoroutine(_hideCoroutine);
			_hideCoroutine = null;
		}
	}
}
