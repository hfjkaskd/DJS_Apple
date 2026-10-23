using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using ArrowOut.Runtime;
using UnityEngine;

namespace WZSDK
{
	public class UIManager : MonoBehaviour
	{
		public enum UILayer
		{
			Layer1 = 1,
			Layer2 = 2,
			Layer3 = 3,
			Layer4 = 4,
			Layer5 = 5,
			Layer6 = 6,
			Layer7 = 7,
			Layer8 = 8
		}

		[Serializable]
		private class UIConfig
		{
			public int sn;

			public UILayer layer;

			public string prefabPath;

			public EUIType uiType;
		}

		private class LegacyUIConfig
		{
			public int legacyId;

			public UILayer layer;

			public string prefabPath;

			public string prefabName;

			public Transform parentTf;
		}

		public static UIManager instance;

		public static HashSet<string> noAnimationViewNames = new HashSet<string>
		{
			"LoadingView",
			"GameView",
			"GamePlayerView",
			"Tutorial",
			"UIEffectView"
		};

		[SerializeField]
		private Transform uiContainer;

		[SerializeField] private Transform[] configuredLayerRoots;

		[SerializeField]
		private int maxInstanceCount;

		private Dictionary<UILayer, Transform> layerContainers;

		private Dictionary<EUIType, UIConfig> uiConfigs;

		private Dictionary<EUIType, GameObject> prefabCache;

		private Dictionary<EUIType, SemaphoreSlim> viewShowLocks;

		private Dictionary<EUIType, int> viewRequestVersions;

		private Dictionary<EUIType, Queue<BaseView>> viewPool;

		private Dictionary<EUIType, List<BaseView>> activeViews;
		private readonly List<BaseView> closingHostViews = new List<BaseView>();

		public Dictionary<UILayer, List<BaseView>> layerViews;

		private Dictionary<int, LegacyUIConfig> legacyConfigs;

		private Dictionary<int, GameObject> legacyPrefabCache;

		private Dictionary<int, SemaphoreSlim> legacyViewOpenLocks;

		private Dictionary<int, int> legacyViewRequestVersions;

		private Dictionary<int, IBaseView> legacyActiveViews;

		// The recovered player creates plain Transform layer objects. Unity 6
		// resolves a stretch-anchored UI root below those layers against the
		// Canvas, while Unity 2022.3 gives it a 0x0 parent rect. Keep the native
		// layer hierarchy and compensate only those full-stretch view roots so
		// their logical rect remains identical to the Canvas in Player builds.
		private Dictionary<RectTransform, Vector2> stretchRootSizeOffsets;

		private Vector2 lastCanvasRectSize;

		private readonly object initializationTaskLock = new object();

		private Task initializationTask;

		private bool shuttingDown;
		private bool IsShutDown => shuttingDown || this == null;

		private static readonly HashSet<EUIType> gameplayPersistentViews = new HashSet<EUIType>
		{
			EUIType.GameView,
			EUIType.GamePlayerView,
			EUIType.UIEffectView
		};

		private static readonly UILayer[] allLayers =
		{
			UILayer.Layer1,
			UILayer.Layer2,
			UILayer.Layer3,
			UILayer.Layer4,
			UILayer.Layer5,
			UILayer.Layer6,
			UILayer.Layer7,
			UILayer.Layer8
		};

		private static readonly Dictionary<string, EUIType> uiTypesByConfigName =
			new Dictionary<string, EUIType>(StringComparer.OrdinalIgnoreCase)
			{
				{ "ENone", EUIType.ENone },
				{ "GameView", EUIType.GameView },
				{ "Tutorial", EUIType.Tutorial },
				{ "SettingsView", EUIType.SettingsView },
				{ "ProfileView", EUIType.ProfileView },
				{ "FinishView", EUIType.FinishView },
				{ "WarningView", EUIType.WarningView },
				{ "UserLevelView", EUIType.UserLevelView },
				{ "ReStartView", EUIType.ReStartView },
				{ "ChallangeSuccessView", EUIType.ChallangeSuccessView },
				{ "UIEffectView", EUIType.UIEffectView },
				{ "LuckySpinView", EUIType.LuckySpinView },
				{ "PropView", EUIType.PropView },
				{ "RewardView", EUIType.RewardView },
				{ "NewGamePlay", EUIType.NewGamePlay },
				{ "LoadingView", EUIType.LoadingView },
				{ "NoticeView", EUIType.NoticeView },
				{ "GamePlayerView", EUIType.GamePlayerView },
				{ "WithdrawView", EUIType.WithdrawView },
				{ "WelcomeView", EUIType.WelcomeView },
				{ "FailView", EUIType.FailView },
				{ "SlotView", EUIType.SlotView },
				{ "WithdrawMissionView", EUIType.WithdrawMissionView },
				{ "TargetWithdrawView", EUIType.TargetWithdrawView },
				{ "GameBalancView", EUIType.GameBalancView },
				{ "LuckyWalletView", EUIType.LuckyWalletView },
				{ "LuckySuccessView", EUIType.LuckySuccessView },
				{ "PayPalErrorView", EUIType.PayPalErrorView },
				{ "BloatView", EUIType.BloatView },
				{ "BloatProgressView", EUIType.BloatProgressView },
				{ "BonusWheelView", EUIType.BonusWheelView },
				{ "LuckyTips", EUIType.LuckyTips },
				{ "WithdrawFeedbackView", EUIType.WithdrawFeedbackView },
				{ "ProgressView", EUIType.ProgressView },
				{ "ToUView", EUIType.ToUView }
			};

		private void Awake()
		{
			if (instance == null)
			{
				instance = this;
				InitializeUIManager();
				_ = InitializeAsync();
				return;
			}

			if (instance != this)
			{
				Destroy(gameObject);
			}
		}

		private void InitializeUIManager()
		{
			layerContainers = new Dictionary<UILayer, Transform>();
			uiConfigs = new Dictionary<EUIType, UIConfig>();
			prefabCache = new Dictionary<EUIType, GameObject>();
			viewShowLocks = new Dictionary<EUIType, SemaphoreSlim>();
			viewRequestVersions = new Dictionary<EUIType, int>();
			viewPool = new Dictionary<EUIType, Queue<BaseView>>();
			activeViews = new Dictionary<EUIType, List<BaseView>>();
			layerViews = new Dictionary<UILayer, List<BaseView>>();
			legacyConfigs = new Dictionary<int, LegacyUIConfig>();
			legacyPrefabCache = new Dictionary<int, GameObject>();
			legacyViewOpenLocks = new Dictionary<int, SemaphoreSlim>();
			legacyViewRequestVersions = new Dictionary<int, int>();
			legacyActiveViews = new Dictionary<int, IBaseView>();
			stretchRootSizeOffsets =
				new Dictionary<RectTransform, Vector2>();
			lastCanvasRectSize = new Vector2(-1f, -1f);
			InitializeLayerContainers();
			InitializeViewPools();
			InitializeLayerManagement();
		}

		public Task InitializeAsync()
		{
			if (IsShutDown) return Task.CompletedTask;
			lock (initializationTaskLock)
			{
				if (initializationTask == null ||
					initializationTask.IsFaulted ||
					initializationTask.IsCanceled)
				{
					initializationTask = InitializeAsyncCore();
				}
				return initializationTask;
			}
		}

		private async Task InitializeAsyncCore()
		{
			try
			{
				TextAsset configAsset =
					await AddressableAssetService.LoadAsync<TextAsset>("csv/UIRes");
				if (IsShutDown)
				{
					return;
				}
				LoadUIConfigFromCSV(configAsset);
			}
			catch (Exception exception)
			{
				Debug.LogError(
					"UIManager initialization failed: " + exception.Message);
				throw;
			}
		}

		private void InitializeLayerContainers()
		{
			if (uiContainer == null)
			{
				uiContainer = transform;
			}

			foreach (UILayer layer in allLayers)
			{
				string layerName = "Layer" +
					((int)layer).ToString(CultureInfo.InvariantCulture);
				Transform layerTransform = configuredLayerRoots != null && configuredLayerRoots.Length >= (int)layer
					? configuredLayerRoots[(int)layer - 1] : uiContainer.Find(layerName);
				if (layerTransform == null)
				{
					throw new InvalidOperationException("Framework 2 prefab is missing UI layer " + layerName);
				}

				layerContainers[layer] = layerTransform;
			}
		}

		private void LoadUIConfigFromCSV(TextAsset configAsset)
		{
			if (configAsset == null)
			{
				throw new InvalidOperationException(
					"UIRes configuration is unavailable.");
			}

			if (string.IsNullOrWhiteSpace(configAsset.text))
			{
				throw new InvalidOperationException(
					"UIRes configuration is empty.");
			}

			Dictionary<EUIType, UIConfig> parsedConfigs =
				new Dictionary<EUIType, UIConfig>();
			string[] lines = configAsset.text.Replace("\r", string.Empty).Split('\n');
			for (int i = 1; i < lines.Length; i++)
			{
				if (string.IsNullOrWhiteSpace(lines[i]))
				{
					continue;
				}

				string[] columns = lines[i].Split(',');
				string prefabPath = columns.Length >= 3
					? columns[2].Trim().Replace(".prefab", string.Empty)
					: string.Empty;
				int serialNumber;
				int layerNumber;
				EUIType uiType;
				if (columns.Length < 4 ||
					!int.TryParse(columns[0].Trim(), out serialNumber) ||
					!int.TryParse(columns[1].Trim(), out layerNumber) ||
					string.IsNullOrWhiteSpace(prefabPath) ||
					!uiTypesByConfigName.TryGetValue(columns[3].Trim(), out uiType))
				{
					Debug.LogWarning("Invalid UIRes row: " + lines[i]);
					continue;
				}

				UIConfig config = new UIConfig
				{
					sn = serialNumber,
					layer = (UILayer)Mathf.Clamp(layerNumber, 1, 8),
					prefabPath = prefabPath,
					uiType = uiType
				};
				parsedConfigs[uiType] = config;
			}

			if (parsedConfigs.Count == 0)
			{
				throw new FormatException(
					"UIRes configuration contains no valid entries.");
			}

			uiConfigs.Clear();
			foreach (KeyValuePair<EUIType, UIConfig> pair in parsedConfigs)
			{
				uiConfigs[pair.Key] = pair.Value;
			}
		}

		private void InitializeViewPools()
		{
			foreach (EUIType viewType in uiTypesByConfigName.Values)
			{
				if (viewType == EUIType.ENone)
				{
					continue;
				}

				viewPool[viewType] = new Queue<BaseView>();
				activeViews[viewType] = new List<BaseView>();
				viewShowLocks[viewType] = new SemaphoreSlim(1, 1);
			}
		}

		private void InitializeLayerManagement()
		{
			foreach (UILayer layer in allLayers)
			{
				layerViews[layer] = new List<BaseView>();
			}
		}

		public async Task<GameObject> LoadPrefabAsync(EUIType viewType)
		{
			await InitializeAsync();
			if (IsShutDown)
			{
				return null;
			}

			GameObject prefab;
			if (prefabCache.TryGetValue(viewType, out prefab) && prefab != null)
			{
				return prefab;
			}

			UIConfig config;
			if (!uiConfigs.TryGetValue(viewType, out config))
			{
				Debug.LogError(
					"No UI configuration for " + GetUITypeName(viewType));
				return null;
			}

			prefab = await AddressableAssetService.LoadAsync<GameObject>(
				config.prefabPath);
			if (!IsShutDown && prefab != null)
			{
				prefabCache[viewType] = prefab;
			}
			return prefab;
		}

		private UILayer GetUILayer(EUIType viewType)
		{
			UIConfig config;
			return uiConfigs.TryGetValue(viewType, out config) ? config.layer : UILayer.Layer5;
		}

		private Transform GetLayerContainer(UILayer layer)
		{
			Transform container;
			return layerContainers.TryGetValue(layer, out container) ? container : uiContainer;
		}

		public Transform GetLayerRoot(UILayer layer)
		{
			return GetLayerContainer(layer);
		}

		public void TrackCanvasSizedRoot(Transform viewTransform)
		{
			TrackAndApplyCanvasSizedRoot(viewTransform);
		}

		private BaseView GetViewFromPool(EUIType viewType)
		{
			Queue<BaseView> pool;
			if (viewPool.TryGetValue(viewType, out pool))
			{
				while (pool.Count > 0)
				{
					BaseView pooled = pool.Dequeue();
					if (pooled != null)
					{
						pooled.gameObject.SetActive(true);
						return pooled;
					}
				}
			}

			GameObject prefab;
			if (!prefabCache.TryGetValue(viewType, out prefab) || prefab == null)
			{
				return null;
			}
			return CreateViewInstance(viewType, prefab);
		}

		private BaseView CreateViewInstance(EUIType viewType, GameObject prefab)
		{
			UILayer layer = GetUILayer(viewType);
			GameObject instanceObject = Instantiate(
				prefab,
				GetLayerContainer(layer));
			BaseView view = instanceObject.GetComponent<BaseView>();
			if (view == null)
			{
				view = instanceObject.GetComponentInChildren<BaseView>(true);
			}
			if (view == null)
			{
				Debug.LogError(prefab.name + " does not contain a BaseView.");
				Destroy(instanceObject);
			}
			return view;
		}

		public BaseView ShowView(EUIType viewType)
		{
			return ShowView(viewType, new object[] { string.Empty });
		}

		public BaseView ShowView(EUIType viewType, params object[] args)
		{
			if (IsShutDown) return null;
			int requestVersion = BeginViewRequest(viewType);
            MahjongProductAnalytics.Track("f2_view_requested", "view", GetUITypeName(viewType), "request_version", requestVersion);
			if (CanShowViewSynchronously(viewType))
			{
				BaseView synchronousView =
					ShowViewFromLoadedResources(viewType, args);
				if (synchronousView != null)
				{
					return synchronousView;
				}
			}

			_ = ObserveShowViewAsync(viewType, args, requestVersion);
			return null;
		}

		public Task<BaseView> ShowViewAsync(EUIType viewType)
		{
			return ShowViewAsync(viewType, new object[] { string.Empty });
		}

		public Task<BaseView> ShowViewAsync(
			EUIType viewType,
			params object[] args)
		{
			int requestVersion = BeginViewRequest(viewType);
            MahjongProductAnalytics.Track("f2_view_requested", "view", GetUITypeName(viewType), "request_version", requestVersion);
			return ShowViewAsyncCore(viewType, args, requestVersion);
		}

		private int pendingHostViewRequests;
		private int pendingTutorialViewRequest;
		private int pendingWithdrawViewRequest;

		private async Task<BaseView> ShowViewAsyncCore(EUIType viewType, object[] args, int requestVersion)
		{
			bool tutorialRequest = viewType == EUIType.Tutorial;
			if (tutorialRequest) pendingTutorialViewRequest = requestVersion;
			else pendingHostViewRequests++;
			try
            {
                var shown = await ShowViewAsyncCoreImpl(viewType, args, requestVersion);
                MahjongProductAnalytics.Track("f2_view_request_result", "view", GetUITypeName(viewType),
                    "request_version", requestVersion, "shown", shown != null);
                return shown;
            }
			finally
			{
				if (!tutorialRequest) pendingHostViewRequests--;
				else if (pendingTutorialViewRequest == requestVersion) pendingTutorialViewRequest = 0;
			}
		}

		private async Task<BaseView> ShowViewAsyncCoreImpl(
			EUIType viewType,
			object[] args,
			int requestVersion)
		{
			await InitializeAsync();
			if (IsShutDown)
			{
				InvokeShownCallback(args, null);
				return null;
			}

			SemaphoreSlim showLock = GetViewShowLock(viewType);
			await showLock.WaitAsync();
			try
			{
				if (IsShutDown ||
					!IsViewRequestCurrent(viewType, requestVersion))
				{
					InvokeShownCallback(args, null);
					return null;
				}

				BaseView view = ShowViewFromLoadedResources(viewType, args);
				if (view != null)
				{
					return view;
				}

				GameObject prefab = await LoadPrefabAsync(viewType);
				if (IsShutDown || prefab == null ||
					!IsViewRequestCurrent(viewType, requestVersion))
				{
					InvokeShownCallback(args, null);
					return null;
				}

				// A synchronous caller may have opened the view while the prefab was
				// loading. Re-check all active/pool state while still holding the gate.
				view = ShowViewFromLoadedResources(viewType, args);
				if (view == null)
				{
					InvokeShownCallback(args, null);
				}
				return view;
			}
			catch (Exception exception)
			{
				Debug.LogError(
					"Failed to show " + GetUITypeName(viewType) +
					" asynchronously: " + exception.Message);
				InvokeShownCallback(args, null);
				return null;
			}
			finally
			{
				showLock.Release();
			}
		}

		private bool CanShowViewSynchronously(EUIType viewType)
		{
			List<BaseView> active;
			if (activeViews.TryGetValue(viewType, out active) && active.Count > 0)
			{
				return true;
			}

			Queue<BaseView> pool;
			if (viewPool.TryGetValue(viewType, out pool) && pool.Count > 0)
			{
				return true;
			}

			GameObject prefab;
			return initializationTask != null &&
				initializationTask.IsCompleted &&
				prefabCache.TryGetValue(viewType, out prefab) &&
				prefab != null;
		}

		private BaseView ShowViewFromLoadedResources(
			EUIType viewType,
			object[] args)
		{
			if (IsShutDown) return null;
			List<BaseView> active;
			if (!activeViews.TryGetValue(viewType, out active))
			{
				active = new List<BaseView>();
				activeViews[viewType] = active;
			}
			if (active.Count > 0)
			{
				BaseView existing = active[active.Count - 1];
				if (existing != null)
				{
					existing.transform.SetAsLastSibling();
					TrackAndApplyCanvasSizedRoot(existing.transform);
					OpenView(existing, args);
					return existing;
				}

				// A destroyed pooled object cannot occur in the native live path, but
				// removing the stale slot keeps the restored manager recoverable.
				active.RemoveAt(active.Count - 1);
			}

			int limit = maxInstanceCount > 0 ? maxInstanceCount : 3;
			if (active.Count >= limit)
			{
				CloseOldestView(viewType);
			}

			BaseView view = GetViewFromPool(viewType);
			if (view == null)
			{
				return null;
			}

			UILayer layer = GetUILayer(viewType);
			view.transform.SetParent(GetLayerContainer(layer));
			view.transform.SetAsLastSibling();
			TrackAndApplyCanvasSizedRoot(view.transform);
			layerViews[layer].Add(view);
			active.Add(view);

			Canvas canvas = view.GetComponent<Canvas>();
			if (canvas != null)
			{
				canvas.overrideSorting = true;
				canvas.sortingOrder = layerViews[layer].Count + (int)layer * 100 - 1;
			}

			OpenView(view, args);
			return view;
		}

		private void OpenView(BaseView view, object[] args)
		{
			if (args != null && args.Length > 0 &&
				args[0] is Action<BaseView>)
			{
				view.Open(Array.Empty<object>());
				InvokeShownCallback(args, view);
				return;
			}

			view.Open(args != null && args.Length > 0
				? args
				: Array.Empty<object>());
		}

		private void InvokeShownCallback(object[] args, BaseView view)
		{
			if (IsShutDown || args == null || args.Length == 0 ||
				!(args[0] is Action<BaseView> onShown))
			{
				return;
			}

			try
			{
				onShown(view);
			}
			catch (Exception exception)
			{
				Debug.LogError("UI shown callback failed: " + exception.Message);
			}
		}

		private SemaphoreSlim GetViewShowLock(EUIType viewType)
		{
			SemaphoreSlim showLock;
			if (!viewShowLocks.TryGetValue(viewType, out showLock))
			{
				showLock = new SemaphoreSlim(1, 1);
				viewShowLocks[viewType] = showLock;
			}
			return showLock;
		}

		private int BeginViewRequest(EUIType viewType)
		{
			int version;
			viewRequestVersions.TryGetValue(viewType, out version);
			version++;
			viewRequestVersions[viewType] = version;
			return version;
		}

		private bool IsViewRequestCurrent(EUIType viewType, int version)
		{
			int currentVersion;
			return viewRequestVersions.TryGetValue(
				viewType,
				out currentVersion) && currentVersion == version;
		}

		private async Task ObserveShowViewAsync(
			EUIType viewType,
			object[] args,
			int requestVersion)
		{
			try
			{
				await ShowViewAsyncCore(viewType, args, requestVersion);
			}
			catch (Exception exception)
			{
				Debug.LogError(
					"Unhandled asynchronous UI load failure for " +
					GetUITypeName(viewType) + ": " + exception.Message);
				InvokeShownCallback(args, null);
			}
		}

		public BaseView ShowView(EUIType viewType, Action<BaseView> onShown)
		{
			return ShowView(viewType, new object[] { onShown });
		}

		public void CloseView(EUIType viewType, BaseView specificView = null)
		{
			BeginViewRequest(viewType);
			// A canceled/timed-out guide load may still be running in Addressables.
			// Its stale request cannot keep gameplay blocked or clear a newer request.
			if (viewType == EUIType.Tutorial) pendingTutorialViewRequest = 0;
			List<BaseView> active;
			if (!activeViews.TryGetValue(viewType, out active) || active.Count == 0)
			{
				return;
			}

			BaseView target = specificView != null ? specificView : active[active.Count - 1];
			if (active.Contains(target))
			{
				ReturnViewToPool(viewType, target);
			}
		}

		private void ReturnViewToPool(EUIType viewType, BaseView view)
		{
			if (view == null)
			{
				return;
			}

			view.Close(Array.Empty<object>());
			if (view.IsClosing && !closingHostViews.Contains(view)) closingHostViews.Add(view);
			List<BaseView> layerList;
			if (layerViews.TryGetValue(GetUILayer(viewType), out layerList))
			{
				layerList.Remove(view);
			}

			List<BaseView> active;
			if (activeViews.TryGetValue(viewType, out active))
			{
				active.Remove(view);
			}

			Queue<BaseView> pool;
			if (!viewPool.TryGetValue(viewType, out pool))
			{
				pool = new Queue<BaseView>();
				viewPool[viewType] = pool;
			}
			pool.Enqueue(view);
		}

		private void CloseOldestView(EUIType viewType)
		{
			List<BaseView> active;
			if (activeViews.TryGetValue(viewType, out active) && active.Count > 0)
			{
				ReturnViewToPool(viewType, active[0]);
			}
		}

		public T GetView<T>(EUIType viewType) where T : BaseView
		{
			List<BaseView> active;
			if (!activeViews.TryGetValue(viewType, out active))
			{
				return null;
			}

			for (int i = active.Count - 1; i >= 0; i--)
			{
				T typed = active[i] as T;
				if (typed != null)
				{
					return typed;
				}
			}
			return null;
		}

		public bool HasActiveView(EUIType viewType)
		{
			List<BaseView> active;
			if (!activeViews.TryGetValue(viewType, out active))
			{
				return false;
			}
			return ContainsVisibleView(active);
		}

		public void CloseAllViews()
		{
			// A host may dispose a partially initialized runtime after a failed load.
			if (activeViews == null || layerViews == null || legacyActiveViews == null) return;
			foreach (EUIType viewType in new List<EUIType>(activeViews.Keys))
			{
				while (activeViews[viewType].Count > 0)
				{
					ReturnViewToPool(viewType, activeViews[viewType][0]);
				}
			}
			foreach (UILayer layer in allLayers)
			{
				List<BaseView> views;
				if (layerViews.TryGetValue(layer, out views))
				{
					views.Clear();
				}
			}

			foreach (int key in new List<int>(legacyActiveViews.Keys))
			{
				CloseLegacyView(key);
			}
		}

		public void RegisterLegacyView(int key, ViewInfo viewInfo, UILayer defaultLayer = UILayer.Layer5)
		{
			if (viewInfo == null || string.IsNullOrEmpty(viewInfo.prefabName))
			{
				Debug.LogWarning("Cannot register legacy view " + key + " without a prefab.");
				return;
			}

			legacyConfigs[key] = new LegacyUIConfig
			{
				legacyId = key,
				layer = defaultLayer,
				prefabName = viewInfo.prefabName,
				prefabPath = NormalizeLegacyPrefabPath(viewInfo.prefabName),
				parentTf = viewInfo.parentTf != null ? viewInfo.parentTf : GetLayerContainer(defaultLayer)
			};
		}

		public void UnregisterLegacyView(int key)
		{
			CloseLegacyView(key);
			legacyConfigs.Remove(key);
			legacyPrefabCache.Remove(key);
		}

		public bool HasLegacyRegistration(int key)
		{
			return legacyConfigs.ContainsKey(key);
		}

		public bool HasActiveLegacyView(int key)
		{
			return IsLegacyViewAlive(GetLegacyView(key));
		}

		public IBaseView GetLegacyView(int key)
		{
			IBaseView view;
			return legacyActiveViews.TryGetValue(key, out view) ? view : null;
		}

		public T GetLegacyView<T>(int key) where T : class, IBaseView
		{
			return GetLegacyView(key) as T;
		}

		public IBaseView OpenLegacyView(int key, params object[] args)
		{
			if (IsShutDown) return null;
			int requestVersion = BeginLegacyViewRequest(key);
            MahjongProductAnalytics.Track("popup_trigger", "popup_id", "f2.legacy." + ViewTypeContract.GetStableName((ViewType)key), "request_version", requestVersion);
			IBaseView existing = GetLegacyView(key);
			if (IsLegacyViewAlive(existing))
			{
				return existing;
			}

			LegacyUIConfig config;
			if (!legacyConfigs.TryGetValue(key, out config))
			{
				return null;
			}

			GameObject prefab;
			if (!legacyPrefabCache.TryGetValue(key, out prefab) || prefab == null)
			{
				_ = ObserveOpenLegacyViewAsync(key, args, requestVersion);
				return null;
			}

			return OpenLegacyViewFromLoadedResources(key, config, prefab, args);
		}

		public Task<IBaseView> OpenLegacyViewAsync(
			int key,
			params object[] args)
		{
			int requestVersion = BeginLegacyViewRequest(key);
            MahjongProductAnalytics.Track("popup_trigger", "popup_id", "f2.legacy." + ViewTypeContract.GetStableName((ViewType)key), "request_version", requestVersion);
			return OpenLegacyViewAsyncCore(key, args, requestVersion);
		}

		private async Task<IBaseView> OpenLegacyViewAsyncCore(int key, object[] args, int requestVersion)
		{
			bool withdrawRequest = key == (int)ViewType.WithDraw;
			if (withdrawRequest) pendingWithdrawViewRequest = requestVersion;
			else pendingHostViewRequests++;
			try
            {
                var shown = await OpenLegacyViewAsyncCoreImpl(key, args, requestVersion);
                if (shown == null) MahjongProductAnalytics.Track("popup_fail", "popup_id", "f2.legacy." + ViewTypeContract.GetStableName((ViewType)key),
                    "request_version", requestVersion, "reason", "not_shown");
                return shown;
            }
			finally
			{
				if (!withdrawRequest) pendingHostViewRequests--;
				else if (pendingWithdrawViewRequest == requestVersion) pendingWithdrawViewRequest = 0;
			}
		}

		private async Task<IBaseView> OpenLegacyViewAsyncCoreImpl(
			int key,
			object[] args,
			int requestVersion)
		{
			SemaphoreSlim openLock = GetLegacyViewOpenLock(key);
			await openLock.WaitAsync();
			try
			{
				if (IsShutDown ||
					!IsLegacyViewRequestCurrent(key, requestVersion))
				{
					return null;
				}

				IBaseView existing = GetLegacyView(key);
				if (IsLegacyViewAlive(existing))
				{
					return existing;
				}

				LegacyUIConfig config;
				if (!legacyConfigs.TryGetValue(key, out config))
				{
					return null;
				}

				GameObject prefab = await LoadLegacyPrefabAsync(key, config);
				if (IsShutDown || prefab == null ||
					!IsLegacyViewRequestCurrent(key, requestVersion))
				{
					return null;
				}

				// Registration can change while the Addressable is loading. Use the
				// latest parent/layer data and do not resurrect an unregistered view.
				if (!legacyConfigs.TryGetValue(key, out config))
				{
					return null;
				}

				existing = GetLegacyView(key);
				if (IsLegacyViewAlive(existing))
				{
					return existing;
				}

				return OpenLegacyViewFromLoadedResources(
					key,
					config,
					prefab,
					args);
			}
			catch (Exception exception)
			{
				Debug.LogError(
					"Failed to open legacy view " + key +
					" asynchronously: " + exception.Message);
				return null;
			}
			finally
			{
				openLock.Release();
			}
		}

		private IBaseView OpenLegacyViewFromLoadedResources(
			int key,
			LegacyUIConfig config,
			GameObject prefab,
			object[] args)
		{
			if (IsShutDown || prefab == null)
			{
				return null;
			}

			GameObject instanceObject = Instantiate(prefab, config.parentTf, false);
			TrackAndApplyCanvasSizedRoot(instanceObject.transform);
			IBaseView view = ResolveLegacyView(instanceObject);
			if (view == null)
			{
				Destroy(instanceObject);
				return null;
			}

			view.ViewId = key;
			legacyActiveViews[key] = view;
			view.InitUi();
			view.InitData();
			view.Open(args);
			return view;
		}

		public void CloseLegacyView(int key, params object[] args)
		{
			BeginLegacyViewRequest(key);
			if (key == (int)ViewType.WithDraw) pendingWithdrawViewRequest = 0;
			IBaseView view = GetLegacyView(key);
			if (view == null)
			{
				legacyActiveViews.Remove(key);
				return;
			}

			legacyActiveViews.Remove(key);
			view.Close(args);
		}

		private async Task<GameObject> LoadLegacyPrefabAsync(
			int key,
			LegacyUIConfig config)
		{
			GameObject prefab;
			if (legacyPrefabCache.TryGetValue(key, out prefab) && prefab != null)
			{
				return prefab;
			}

			prefab = await AddressableAssetService.LoadAsync<GameObject>(
				config.prefabPath);
			if (!IsShutDown && prefab != null)
			{
				legacyPrefabCache[key] = prefab;
			}
			return prefab;
		}

		internal Task<GameObject> PreloadLegacyPrefabAsync(ViewType type)
		{
			int key = (int)type;
			return legacyConfigs.TryGetValue(key, out LegacyUIConfig config)
				? LoadLegacyPrefabAsync(key, config)
				: Task.FromException<GameObject>(new InvalidOperationException(
					"Framework 2 preload registration is missing: " + ViewTypeContract.GetStableName(type)));
		}

		private SemaphoreSlim GetLegacyViewOpenLock(int key)
		{
			SemaphoreSlim openLock;
			if (!legacyViewOpenLocks.TryGetValue(key, out openLock))
			{
				openLock = new SemaphoreSlim(1, 1);
				legacyViewOpenLocks[key] = openLock;
			}
			return openLock;
		}

		private int BeginLegacyViewRequest(int key)
		{
			int version;
			legacyViewRequestVersions.TryGetValue(key, out version);
			version++;
			legacyViewRequestVersions[key] = version;
			return version;
		}

		private bool IsLegacyViewRequestCurrent(int key, int version)
		{
			int currentVersion;
			return legacyViewRequestVersions.TryGetValue(
				key,
				out currentVersion) && currentVersion == version;
		}

		private async Task ObserveOpenLegacyViewAsync(
			int key,
			object[] args,
			int requestVersion)
		{
			try
			{
				await OpenLegacyViewAsyncCore(key, args, requestVersion);
			}
			catch (Exception exception)
			{
				Debug.LogError(
					"Unhandled asynchronous legacy UI load failure for " +
					key + ": " + exception.Message);
			}
		}

		private IBaseView ResolveLegacyView(GameObject uiObject)
		{
			MonoBehaviour[] behaviours = uiObject.GetComponents<MonoBehaviour>();
			foreach (MonoBehaviour behaviour in behaviours)
			{
				IBaseView view = behaviour as IBaseView;
				if (view != null)
				{
					return view;
				}
			}
			return null;
		}

		private bool IsLegacyViewAlive(IBaseView view)
		{
			if (view == null)
			{
				return false;
			}
			UnityEngine.Object unityObject = view as UnityEngine.Object;
			return unityObject != null;
		}

		private string NormalizeLegacyPrefabPath(string prefabName)
		{
			string path = prefabName.Replace('\\', '/').Replace(".prefab", string.Empty);
			return path.Contains("/") ? path : "prefab/" + path;
		}

		public void CleanupPool(EUIType viewType)
		{
			Queue<BaseView> pool;
			if (!viewPool.TryGetValue(viewType, out pool))
			{
				return;
			}
			while (pool.Count > maxInstanceCount)
			{
				BaseView view = pool.Dequeue();
				if (view != null)
				{
					Destroy(view.gameObject);
				}
			}
		}

		public void PreloadViews(EUIType viewType, int count)
		{
			if (count <= 0)
			{
				return;
			}

			_ = ObservePreloadViewsAsync(viewType, count);
		}

		public async Task PreloadViewsAsync(EUIType viewType, int count)
		{
			if (count <= 0)
			{
				return;
			}

			await InitializeAsync();
			if (IsShutDown)
			{
				return;
			}

			SemaphoreSlim showLock = GetViewShowLock(viewType);
			await showLock.WaitAsync();
			try
			{
				GameObject prefab = await LoadPrefabAsync(viewType);
				if (IsShutDown || prefab == null)
				{
					return;
				}

				Queue<BaseView> pool;
				if (!viewPool.TryGetValue(viewType, out pool))
				{
					pool = new Queue<BaseView>();
					viewPool[viewType] = pool;
				}

				for (int i = 0; i < count; i++)
				{
					GameObject viewObject = Instantiate(
						prefab,
						GetLayerContainer(GetUILayer(viewType)));
					BaseView view = viewObject != null
						? viewObject.GetComponent<BaseView>()
						: null;
					if (view == null && viewObject != null)
					{
						view = viewObject.GetComponentInChildren<BaseView>(true);
					}
					if (view == null)
					{
						if (viewObject != null)
						{
							Destroy(viewObject);
						}
						Debug.LogError(
							prefab.name + " does not contain a BaseView.");
						continue;
					}

					viewObject.SetActive(false);
					pool.Enqueue(view);
					await Task.Yield();
					if (IsShutDown)
					{
						return;
					}
				}
			}
			finally
			{
				showLock.Release();
			}
		}

		private async Task ObservePreloadViewsAsync(
			EUIType viewType,
			int count)
		{
			try
			{
				await PreloadViewsAsync(viewType, count);
			}
			catch (Exception exception)
			{
				Debug.LogError(
					"Unhandled asynchronous UI preload failure for " +
					GetUITypeName(viewType) + ": " + exception.Message);
			}
		}

		public bool HasHostBlockingViews
		{
			get
			{
				if (pendingHostViewRequests > 0 || pendingTutorialViewRequest != 0 ||
					pendingWithdrawViewRequest != 0) return true;
				for (int i = closingHostViews.Count - 1; i >= 0; i--)
				{
					BaseView closing = closingHostViews[i];
					if (closing != null && closing.IsClosing) return true;
					closingHostViews.RemoveAt(i);
				}
				foreach (var pair in activeViews)
				{
					if (pair.Key == EUIType.UIEffectView || pair.Key == EUIType.GameView || pair.Key == EUIType.GamePlayerView) continue;
					if (ContainsVisibleView(pair.Value)) return true;
				}
				return HasActiveLegacyView();
			}
		}

		public void RefreshRuntimeGmPresentation()
		{
			foreach (var pair in activeViews)
				for (int i = 0; i < pair.Value.Count; i++)
				{
					BaseView view = pair.Value[i];
					if (view == null) continue;
					view.RefreshRuntimeGmPresentation();
					LocalizationManager.Instance?.ApplyView(view.gameObject);
				}
			foreach (var pair in legacyActiveViews)
				if (pair.Value is Component legacyView && legacyView != null)
					LocalizationManager.Instance?.ApplyView(legacyView.gameObject);
		}

        public bool IsGameplayOnlyUIActive()
        {
			if (Framework2Host.Runtime != null)
				return !HasHostBlockingViews && Framework2Host.GameplayActive != null && Framework2Host.GameplayActive();
			foreach (KeyValuePair<EUIType, List<BaseView>> pair in activeViews)
			{
				if (!ContainsVisibleView(pair.Value))
				{
					continue;
				}
				if (!gameplayPersistentViews.Contains(pair.Key))
				{
					return false;
				}
			}
			return !HasActiveLegacyView() &&
				(HasVisibleView(EUIType.GameView) ||
				 HasVisibleView(EUIType.GamePlayerView));
		}

		public bool PrintActiveViews()
		{
			List<string> names = new List<string>();
			foreach (KeyValuePair<EUIType, List<BaseView>> pair in activeViews)
			{
				if (ContainsVisibleView(pair.Value))
				{
					names.Add(GetUITypeName(pair.Key));
				}
			}
			Debug.Log("Active views: " + string.Join(", ", names));
			return IsGameplayOnlyUIActive();
		}

		private static string GetUITypeName(EUIType viewType)
		{
			return EUITypeContract.GetStableName(viewType);
		}

		private bool HasVisibleView(EUIType viewType)
		{
			return HasActiveView(viewType);
		}

		private static bool ContainsVisibleView(List<BaseView> views)
		{
			if (views == null)
			{
				return false;
			}

			for (int i = 0; i < views.Count; i++)
			{
				if (IsViewVisible(views[i]))
				{
					return true;
				}
			}
			return false;
		}

		private static bool IsViewVisible(BaseView view)
		{
			if (view == null || !view.gameObject.activeInHierarchy)
			{
				return false;
			}
			if (view.IsShow())
			{
				return true;
			}
			return view.canvasGroup != null &&
				(view.canvasGroup.alpha > 0.001f ||
				 view.canvasGroup.blocksRaycasts ||
				 view.canvasGroup.interactable);
		}

		private bool HasActiveLegacyView()
		{
			List<int> staleKeys = null;
			foreach (KeyValuePair<int, IBaseView> pair in legacyActiveViews)
			{
				if (IsLegacyViewAlive(pair.Value))
				{
					return true;
				}
				if (staleKeys == null)
				{
					staleKeys = new List<int>();
				}
				staleKeys.Add(pair.Key);
			}
			if (staleKeys != null)
			{
				foreach (int key in staleKeys)
				{
					legacyActiveViews.Remove(key);
				}
			}
			return false;
		}

		private void Update()
		{
		}

		private void OnRectTransformDimensionsChange()
		{
			RefreshCanvasSizedRootsIfNeeded();
		}

		private void TrackAndApplyCanvasSizedRoot(Transform viewTransform)
		{
			RectTransform viewRoot = viewTransform as RectTransform;
			RectTransform canvasRoot = uiContainer as RectTransform;
			if (viewRoot == null ||
				canvasRoot == null ||
				viewRoot.parent is RectTransform ||
				!IsFullStretch(viewRoot))
			{
				return;
			}

			Vector2 sizeOffset;
			if (!stretchRootSizeOffsets.TryGetValue(viewRoot, out sizeOffset))
			{
				sizeOffset = viewRoot.sizeDelta;
				stretchRootSizeOffsets[viewRoot] = sizeOffset;
			}

			Vector2 canvasSize = canvasRoot.rect.size;
			if (canvasSize.x <= 0f || canvasSize.y <= 0f)
			{
				return;
			}

			Vector2 targetSize = canvasSize + sizeOffset;
			if ((viewRoot.sizeDelta - targetSize).sqrMagnitude <= 0.0001f)
			{
				return;
			}

			viewRoot.sizeDelta = targetSize;
			UIEffectView effectView = viewRoot.GetComponent<UIEffectView>();
			effectView?.RefreshCanvasSizedLayout();
		}

		private void RefreshCanvasSizedRootsIfNeeded()
		{
			RectTransform canvasRoot = uiContainer as RectTransform;
			if (canvasRoot == null || stretchRootSizeOffsets == null)
			{
				return;
			}

			Vector2 canvasSize = canvasRoot.rect.size;
			if (canvasSize.x <= 0f ||
				canvasSize.y <= 0f ||
				(canvasSize - lastCanvasRectSize).sqrMagnitude <= 0.0001f)
			{
				return;
			}

			lastCanvasRectSize = canvasSize;
			List<RectTransform> trackedRoots =
				new List<RectTransform>(stretchRootSizeOffsets.Keys);
			foreach (RectTransform root in trackedRoots)
			{
				if (root == null)
				{
					stretchRootSizeOffsets.Remove(root);
					continue;
				}
				TrackAndApplyCanvasSizedRoot(root);
			}
		}

		private static bool IsFullStretch(RectTransform rectTransform)
		{
			return rectTransform.anchorMin == Vector2.zero &&
				rectTransform.anchorMax == Vector2.one;
		}

		public void Shutdown()
		{
			if (shuttingDown) return;
			shuttingDown = true;
			StopAllCoroutines();
			CloseAllViews();
			prefabCache?.Clear();
			legacyPrefabCache?.Clear();
			stretchRootSizeOffsets?.Clear();
		}

		private void OnDestroy()
		{
			Shutdown();
			if (instance == this)
			{
				instance = null;
			}
		}
	}
}
