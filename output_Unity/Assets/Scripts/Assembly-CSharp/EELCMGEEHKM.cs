using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using UnityEngine;

public class EELCMGEEHKM : global::FOLJNEPEKCA<EELCMGEEHKM>
{
	[Serializable]
	private class ConfAsk
	{
		public int version;

		public int env;

		public string apiKey;

		public string pName;

		public string timestamp;

		public string uuid;

		public string requestPath;

		public string signature;
	}

	[Serializable]
	private class ConfRespons
	{
		public string url;

		public int version;

		public int cVersion;
	}

	private sealed class POHICJJJENE
	{
		[StructLayout((LayoutKind)3)]
		private struct _003C_003CDownloadNewConf_003Eg__DataLoaded_007C1_003Ed : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncVoidMethodBuilder _003C_003Et__builder;

			public POHICJJJENE _003C_003E4__this;

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

		public EELCMGEEHKM _003C_003E4__this;

		public ConfRespons confRespons;

		public Action _003C_003E9__2;

		internal void _003CDownloadNewConf_003Eb__0()
		{
		}

		internal void _003CDownloadNewConf_003Eb__2()
		{
		}

		[AsyncStateMachine(typeof(_003C_003CDownloadNewConf_003Eg__DataLoaded_007C1_003Ed))]
		internal void _003CDownloadNewConf_003Eg__DataLoaded_007C1(byte[] bytes)
		{
		}
	}

	private string lastLoadedMainLevelConfig;

	private bool isPosting;

	private static readonly RijndaelManaged RForSignature;

	public AssetBundle Use { get; private set; }

	public void Init()
	{
	}

	private void CheckSaved(int v, int cv)
	{
	}

	private void LoadConfigs()
	{
	}

	public void LoadMainLevelConfigs()
	{
	}

	public void CheckServer()
	{
	}

	private void DownloadNewConf(ConfRespons confRespons)
	{
	}

	private static bool TryParseVersionString(TextAsset textAsset, out int v, out int cv)
	{
		v = default(int);
		cv = default(int);
		return false;
	}

	private static bool TryParseVersionString(string text, out int v, out int cv)
	{
		v = default(int);
		cv = default(int);
		return false;
	}
}
