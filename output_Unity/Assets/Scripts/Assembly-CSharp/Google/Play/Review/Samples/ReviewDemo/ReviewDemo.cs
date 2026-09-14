using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Google.Play.Common;
using UnityEngine;
using UnityEngine.UI;

namespace Google.Play.Review.Samples.ReviewDemo
{
	public class ReviewDemo : MonoBehaviour
	{
		[CompilerGenerated]
		private sealed class _003CAllInOneFlowCoroutine_003Ed__14 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public ReviewDemo _003C_003E4__this;

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
			public _003CAllInOneFlowCoroutine_003Ed__14(int _003C_003E1__state)
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

		[CompilerGenerated]
		private sealed class _003CLaunchFlowCoroutine_003Ed__13 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public ReviewDemo _003C_003E4__this;

			private PlayAsyncOperation<VoidResult, ReviewErrorCode> _003ClaunchFlowOperation_003E5__2;

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
			public _003CLaunchFlowCoroutine_003Ed__13(int _003C_003E1__state)
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

		[CompilerGenerated]
		private sealed class _003CRequestFlowCoroutine_003Ed__12 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public ReviewDemo _003C_003E4__this;

			public bool isStepRequest;

			private Stopwatch _003CstopWatch_003E5__2;

			private PlayAsyncOperation<PlayReviewInfo, ReviewErrorCode> _003CrequestFlowOperation_003E5__3;

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
			public _003CRequestFlowCoroutine_003Ed__12(int _003C_003E1__state)
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

		private static PlayReviewInfo _playReviewInfo;

		private ReviewManager _reviewManager;

		private Dictionary<KeyCode, Action> _keyMappings;

		public Text statusText;

		public Button requestFlowButton;

		public Button launchFlowButton;

		public Button allInOneButton;

		private void Start()
		{
		}

		private void Update()
		{
		}

		private void RequestFlowClick()
		{
		}

		private void LaunchFlowClick()
		{
		}

		private void AllInOneFlowClick()
		{
		}

		[IteratorStateMachine(typeof(_003CRequestFlowCoroutine_003Ed__12))]
		private IEnumerator RequestFlowCoroutine(bool isStepRequest)
		{
			return null;
		}

		[IteratorStateMachine(typeof(_003CLaunchFlowCoroutine_003Ed__13))]
		private IEnumerator LaunchFlowCoroutine()
		{
			return null;
		}

		[IteratorStateMachine(typeof(_003CAllInOneFlowCoroutine_003Ed__14))]
		private IEnumerator AllInOneFlowCoroutine()
		{
			return null;
		}

		private void ResetDisplay(string errorText)
		{
		}
	}
}
