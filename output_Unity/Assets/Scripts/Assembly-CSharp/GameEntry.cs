using System.Collections;
using UnityEngine;

/// <summary>游戏入口：初始化框架、数据、UI，然后直接进入当前关卡。</summary>
public class GameEntry : MonoBehaviour
{
	public enum GNDIEJJKAJM
	{
		FirstLaunchAfterAgree = 0,
		NormalLaunchBeforeLoadingBar = 1,
		NormalLaunchAfterLoadingBar = 2
	}

	public const float DoLoadingTime = 3f;

	public static GameEntry Instance { get; private set; }

	public bool InitFinish { get; private set; }

	public bool IsFirstLogin { get; private set; }

	public bool NeedWatiAbSyncExp { get; set; }

	private void Start()
	{
		Instance = this;
		Application.targetFrameRate = 60;
		Input.multiTouchEnabled = false;
		if (System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-clearsave") >= 0)
		{
			PlayerPrefs.DeleteAll();
			PlayerPrefs.Save();
			foreach (string file in System.IO.Directory.GetFiles(Application.persistentDataPath, "*.json"))
			{
				System.IO.File.Delete(file);
			}
			Debug.Log("[GameEntry] 已清空存档");
		}
		if (DebugAutoPilot.IsEnabled())
		{
			gameObject.AddComponent<DebugAutoPilot>();
		}
		StartCoroutine(ProjectInit());
	}

	private IEnumerator ProjectInit()
	{
		FruitEarningVariants.EarningVariantSelection.Initialize();
		// 框架层
		MCCIJBJGMCK.Init();
		Timer.Instance.Init();
		DGNMMHCBFMI.Init();
		GameAudio.Init();

		// 数据层
		EEEHAEKANLA.Instance.Init();
		IsFirstLogin = EEEHAEKANLA.Instance.IsFirstColdStart;
		BMNFNJFCPHG.Instance.Init();
		ApplyLanguageOverride();
		OJEEJGGLNPC.Instance.LoadConfig();
		PLMIHDHFAAL.Instance.Init(GameRes.LoadTextAsset("res_server/server_configs/MainLevelConfig"));
		GNEJJHEDEBL.Instance.Init();
		JEFOMCDAPGK.Instance.Init();
		FPFGGCMEDND.Instance.Init();
		KDBNHIIFGLD.Instance.Init();
		EDLHEMMBABM.Instance.Init();
		FruitWithdrawal.WithdrawalGameplayBridge.Initialize();

		// UI层
		MgrUI mgrUI = Object.FindObjectOfType<MgrUI>();
		if (mgrUI == null)
		{
			GameObject mgrUIGo = GameObject.Find("MgrUI");
			if (mgrUIGo != null)
			{
				mgrUI = mgrUIGo.AddComponent<MgrUI>();
			}
		}
		if (mgrUI != null)
		{
			mgrUI.Init();
		}
		MgrGlobalUI mgrGlobalUI = Object.FindObjectOfType<MgrGlobalUI>();
		if (mgrGlobalUI != null)
		{
			mgrGlobalUI.Init();
		}

		var hostPrefab = Resources.Load<GameObject>(FruitEarningVariants.EarningVariantController.ResourcePath);
		if (hostPrefab == null)
		{
			Debug.LogError("[A/B] The authored earning variant controller prefab is missing.");
			yield break;
		}
		var earningHost = Instantiate(hostPrefab).GetComponent<FruitEarningVariants.EarningVariantController>();
		var initialization = earningHost.InitializeAsync();
		while (!initialization.IsCompleted) yield return null;
		if (initialization.IsFaulted || !earningHost.Ready) yield break;
		yield return null;
		InitFinish = true;

		GameAudio.PlayMusic();
		// CorePlayUI owns both saved-board recovery and fresh-level creation.
		MgrUI.Instance.Open("coreplay/CorePlayUI", false);
	}

	/// <summary>命令行 -lang xx（如 zh_cn）可强制语言，用于与目标录屏环境对齐。</summary>
	private static void ApplyLanguageOverride()
	{
		string[] args = System.Environment.GetCommandLineArgs();
		for (int i = 0; i < args.Length - 1; i++)
		{
			if (args[i] == "-lang" && System.Enum.TryParse(args[i + 1], out EDMAJFIKJLE lan))
			{
				BMNFNJFCPHG.Instance.SettingData.localizationType = lan;
				return;
			}
		}
	}

	private void DoMgrSDKStartAfterAgreePolicy()
	{
	}

	public void InitSDK(GNDIEJJKAJM initType)
	{
	}

	private void OnProgressFinish()
	{
	}

	private void OnApplicationPause(bool pause)
	{
		if (pause)
		{
			TrySaveAll();
		}
	}

	private void OnApplicationQuit()
	{
		TrySaveAll();
	}

	private static void TrySaveAll()
	{
		EEEHAEKANLA.Instance.TrySaveToDisk();
		BMNFNJFCPHG.Instance.TrySaveToDisk();
		GNEJJHEDEBL.Instance.TrySaveToDisk();
		JEFOMCDAPGK.Instance.TrySaveToDisk();
		FPFGGCMEDND.Instance.TrySaveToDisk();
		KDBNHIIFGLD.Instance.TrySaveToDisk();
		FruitWithdrawal.WithdrawalGameplayBridge.Flush();
	}
}
