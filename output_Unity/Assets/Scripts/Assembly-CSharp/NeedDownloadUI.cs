using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

public class NeedDownloadUI : BaseUI
{
	private sealed class DHCDHADNMLA
	{
		public NeedDownloadUI _003C_003E4__this;

		public int batchGen;
	}

	private sealed class ACAGBNGKCHI
	{
		public string groupKey;

		public HashSet<string> levelIds;

		public DHCDHADNMLA CS_0024_003C_003E8__locals1;

		internal void _003CDoBatchDownload_003Eb__0()
		{
		}
	}

	private sealed class EJOPMBEIFJP
	{
		public int batchGen;

		public NeedDownloadUI _003C_003E4__this;

		public string path;

		public float sizeBytes;

		public Action onSuccess;

		internal void _003CEnqueueFrontOrMarkDone_003Eb__0()
		{
		}

		internal void _003CEnqueueFrontOrMarkDone_003Eb__1()
		{
		}

		internal void _003CEnqueueFrontOrMarkDone_003Eb__2(float downloaded, float ratio)
		{
		}
	}

	private sealed class HEKMNBPAKNH : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public NeedDownloadUI _003C_003E4__this;

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
		public HEKMNBPAKNH(int _003C_003E1__state)
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
	private GameObject m_CloseBtn;

	[SerializeField]
	private GameObject m_DownloadBtn;

	[SerializeField]
	private CommonProgressBar m_DownloadProgress;

	[SerializeField]
	private TextMeshProUGUI m_ProgressText;

	[SerializeField]
	private GameObject m_RedownloadBtn;

	public static int BatchStartLevel;

	public static int BatchCount;

	public static Action OnBatchComplete;

	private Dictionary<string, float> fileBytes;

	private HashSet<string> countedPaths;

	private float totalBytes;

	private float doneBytes;

	private int pendingCount;

	private int doneCount;

	private int failedCount;

	private int foregroundMapCount;

	private bool isCancelled;

	private float lastProgressTime;

	private float downloadStartTime;

	private int batchGeneration;

	private const int MaxForegroundMaps = 5;

	private const float StallTimeoutSeconds = 15f;

	private const float OverallTimeoutSeconds = 120f;

	public override PAIEAGDLCBJ Layer => default(PAIEAGDLCBJ);

	protected override void Init()
	{
	}

	protected override void BeforeOpen()
	{
	}

	protected override void BeforeClose()
	{
	}

	private void OnCloseBtnClick(GameObject gameObject)
	{
	}

	private void OnDownloadBtnClick(GameObject gameObject)
	{
	}

	private void OnReDownloadBtnClick(GameObject gameObject)
	{
	}

	[IteratorStateMachine(typeof(HEKMNBPAKNH))]
	private IEnumerator DoBatchDownload()
	{
		return null;
	}

	private void EnqueueFrontOrMarkDone(string path, Action onSuccess, int batchGen = 0)
	{
	}

	private void OnGroupConfigReady(string groupKey, HashSet<string> levelIds, int batchGen)
	{
	}

	private void UpdateProgressUI()
	{
	}

	private void OnAllDownloaded()
	{
	}

	private void OnDownloadFailed()
	{
	}

	private static float GetSizeBytes(string path)
	{
		return 0f;
	}
}
