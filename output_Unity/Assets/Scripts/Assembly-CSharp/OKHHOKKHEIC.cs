using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class OKHHOKKHEIC
{
	private sealed class DONDJPEEOBM<T> : IEnumerator<object>, IEnumerator, IDisposable where T : UnityEngine.Object
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public OKHHOKKHEIC _003C_003E4__this;

		public Action<T> InCallback;

		public string InName;

		private AssetBundleRequest _003CassetBundleRequest_003E5__2;

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
		public DONDJPEEOBM(int _003C_003E1__state)
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

	private sealed class JBDPPLMLOPI : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public OKHHOKKHEIC _003C_003E4__this;

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
		public JBDPPLMLOPI(int _003C_003E1__state)
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

	private sealed class PALPBNEJBIH : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public OKHHOKKHEIC _003C_003E4__this;

		private AssetBundleCreateRequest _003CassetBundleCreateRequest_003E5__2;

		private HashSet<OKHHOKKHEIC>.Enumerator _003C_003E7__wrap2;

		private OKHHOKKHEIC _003Citem_003E5__4;

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
		public PALPBNEJBIH(int _003C_003E1__state)
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

		private void _003C_003Em__Finally1()
		{
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
		}
	}

	private static global::IAMOPBKAHDO<OKHHOKKHEIC> pool;

	private AssetBundle ab;

	private HashSet<OKHHOKKHEIC> dependencies;

	private bool IsLocal;

	private static AssetBundleManifest bundleManifest;

	internal string Key { get; private set; }

	internal string Address { get; private set; }

	public int ReferencesCount { get; private set; }

	public int manuallyLoadCount { get; set; }

	public bool IsLoading { get; private set; }

	public int LoadingAssetsCount { get; private set; }

	internal bool IsInMemory => false;

	internal static event Action<string> ReleaseAbFromMemory
	{
		[CompilerGenerated]
		add
		{
		}
		[CompilerGenerated]
		remove
		{
		}
	}

	private static OKHHOKKHEIC Create()
	{
		return null;
	}

	public static OKHHOKKHEIC Get(string InKey, string InAddress, bool InIsLocal)
	{
		return null;
	}

	private OKHHOKKHEIC()
	{
	}

	private void Initialize(string InKey, string InAddress, bool InIsLocal)
	{
	}

	internal T LoadAssetSync<T>(string InName) where T : UnityEngine.Object
	{
		return null;
	}

	internal void LoadAssetAsync<T>(string InName, Action<T> InCallback) where T : UnityEngine.Object
	{
	}

	private void ResetGoShader(GameObject InGo)
	{
	}

	[IteratorStateMachine(typeof(DONDJPEEOBM<>))]
	private IEnumerator LoadAssetAsyncCor<T>(string InName, Action<T> InCallback) where T : UnityEngine.Object
	{
		return null;
	}

	private void LoadAssetBundleSync()
	{
	}

	private void LoadAssetBundleAsync()
	{
	}

	[IteratorStateMachine(typeof(JBDPPLMLOPI))]
	private IEnumerator LoadAssetBundleCor()
	{
		return null;
	}

	[IteratorStateMachine(typeof(PALPBNEJBIH))]
	private IEnumerator SingleAssetBundleAsync()
	{
		return null;
	}

	private bool CheckBaseReleaseCondition()
	{
		return false;
	}

	private void DoRelease()
	{
	}

	internal bool TryRelease(bool withForce)
	{
		return false;
	}

	private void GetDependencies()
	{
	}

	private static string[] GetKeyDependencies(string InABName)
	{
		return null;
	}

	private static void LoadMainifest()
	{
	}

	private static string GetManifestPath()
	{
		return null;
	}
}
