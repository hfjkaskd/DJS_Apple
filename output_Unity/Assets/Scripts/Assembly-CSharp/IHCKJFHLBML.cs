using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class IHCKJFHLBML : global::FOLJNEPEKCA<IHCKJFHLBML>
{
	private sealed class PGFFACJLCNF
	{
		public IHCKJFHLBML _003C_003E4__this;

		public string p;

		public Action<AssetBundle, string> action;

		public string path;

		public string persistentPath;

		public bool InAllowOld;

		public Action<string> fail;

		public ResServerAssetData d;

		public Action<AssetBundle> _003C_003E9__2;

		internal void _003CLoadABAsync_003Eb__0(AssetBundle ab)
		{
		}

		internal void _003CLoadABAsync_003Eb__2(AssetBundle ab2)
		{
		}

		internal void _003CLoadABAsync_003Eb__4(AssetBundle ab)
		{
		}
	}

	private sealed class HJKJHAMNNEP
	{
		public string oldPath;

		public PGFFACJLCNF CS_0024_003C_003E8__locals1;

		internal void _003CLoadABAsync_003Eb__1(AssetBundle ab0)
		{
		}
	}

	private sealed class LLAFPLPNPGB
	{
		public string oldPath;

		public PGFFACJLCNF CS_0024_003C_003E8__locals2;

		internal void _003CLoadABAsync_003Eb__3(AssetBundle ab0)
		{
		}
	}

	private sealed class HFENJDOMJCP
	{
		public IHCKJFHLBML _003C_003E4__this;

		public string path;

		public Action<AssetBundle, string> action;

		public Action<string> fail;

		internal void _003CDownload_003Eg__Fail_007C1()
		{
		}
	}

	private sealed class IAAMJNAHIIK
	{
		[StructLayout((LayoutKind)3)]
		private struct _003C_003CDownload_003Eg__DataLoaded_007C0_003Ed : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncVoidMethodBuilder _003C_003Et__builder;

			public IAAMJNAHIIK _003C_003E4__this;

			public byte[] bytes;

			private string _003CsavePath_003E5__2;

			private TaskAwaiter _003C_003Eu__1;

			private void MoveNext()
			{
			}

			void IAsyncStateMachine.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				this.MoveNext();
			}

			[DebuggerHidden]
			private void SetStateMachine(IAsyncStateMachine stateMachine)
			{
			}

			void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
			{
				//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
				this.SetStateMachine(stateMachine);
			}
		}

		public string p;

		public ResServerAssetData d;

		public HFENJDOMJCP CS_0024_003C_003E8__locals1;

		[AsyncStateMachine(typeof(_003C_003CDownload_003Eg__DataLoaded_007C0_003Ed))]
		internal void _003CDownload_003Eg__DataLoaded_007C0(byte[] bytes)
		{
		}
	}

	private sealed class MCMEAACCDHM : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public string path;

		public Action<AssetBundle> action;

		private AssetBundleCreateRequest _003CassetBundleCreateRequest_003E5__2;

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
		public MCMEAACCDHM(int _003C_003E1__state)
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

	public readonly Dictionary<string, ResServerAssetData> pathDataDic;

	private readonly Dictionary<string, AssetBundle> abDic;

	public void Init(string json)
	{
	}

	public void ApplyLocalOverrides(string localJson)
	{
	}

	public bool IsAvailable(string path, out bool hasUpdate, bool InAllowOld = false)
	{
		hasUpdate = default(bool);
		return false;
	}

	public void LoadAB(string path, Action<AssetBundle, string> action, Action<string> fail = null, bool InAllowOld = false)
	{
	}

	public void LoadABAsync(string path, Action<AssetBundle, string> action, Action<string> fail = null, bool InAllowOld = false)
	{
	}

	[IteratorStateMachine(typeof(MCMEAACCDHM))]
	private IEnumerator LoadFromFileAsync(string path, Action<AssetBundle> action)
	{
		return null;
	}

	public void Download(string path, Action<AssetBundle, string> action, Action<string> fail = null, Action<float, float> progressAction = null, bool immediately = true)
	{
	}

	private static string TryFindPersistentBundle(string path)
	{
		return null;
	}

	public void Release(string path, bool unloadAllLoadedObjects = true)
	{
	}

	private void RemoveUnmatchedBundles(string InPrefix, string InHash)
	{
	}
}
