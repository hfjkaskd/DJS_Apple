using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;

namespace OCES
{
	public class ResourceLoader : MonoBehaviour
	{
		private sealed class JIKPKIAJEML<T> where T : UnityEngine.Object
		{
			public Action<T> onComplete;

			public ResourceLoader _003C_003E4__this;

			public string path;

			internal void _003CLoadAsync_003Eb__0(UnityEngine.Object obj)
			{
			}

			internal void _003CLoadAsync_003Eb__2(UnityEngine.Object obj)
			{
			}

			internal void _003CLoadAsync_003Eb__1(T asset)
			{
			}
		}

		private sealed class IDOGLGPOHHG<T> : IEnumerator<object>, IEnumerator, IDisposable where T : UnityEngine.Object
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public string path;

			public ResourceLoader _003C_003E4__this;

			private ResourceRequest _003Crequest_003E5__2;

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
			public IDOGLGPOHHG(int _003C_003E1__state)
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

		private readonly Dictionary<string, UnityEngine.Object> m_cachedObjects;

		private readonly Dictionary<string, List<Action<UnityEngine.Object>>> m_pendingCallbacks;

		private EKJOAPFBDNI m_assetProvider;

		public void SetProvider(EKJOAPFBDNI assetProvider)
		{
		}

		[CanBeNull]
		internal T LoadSync<T>(string path) where T : UnityEngine.Object
		{
			return null;
		}

		internal void LoadAsync<T>(string path, Action<T> onComplete) where T : UnityEngine.Object
		{
		}

		[IteratorStateMachine(typeof(IDOGLGPOHHG<>))]
		private IEnumerator FallbackLoadAsyncCoroutine<T>(string path) where T : UnityEngine.Object
		{
			return null;
		}

		private void OnAsyncAssetLoaded(string path, UnityEngine.Object asset)
		{
		}

		internal void PreloadAsync<T>(string[] paths)
		{
		}

		internal void Release<T>(string path)
		{
		}

		internal void ReleaseUnused()
		{
		}
	}
}
