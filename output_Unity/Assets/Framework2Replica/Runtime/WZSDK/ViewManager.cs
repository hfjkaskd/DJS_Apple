using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace WZSDK
{
	public class ViewManager
	{
		public Transform canvasTf;

		public Dictionary<int, IBaseView> _opens;

		public Dictionary<int, IBaseView> _viewCache;

		public Dictionary<int, ViewInfo> _views;

		public ViewManager()
		{
			canvasTf = ResolveCanvasTransform();
			_opens = new Dictionary<int, IBaseView>();
			_views = new Dictionary<int, ViewInfo>();
			_viewCache = new Dictionary<int, IBaseView>();
			RegisterDefaultViews();
		}

		public void Register(int key, ViewInfo viewInfo)
		{
			if (viewInfo == null)
			{
				return;
			}
			if (viewInfo.parentTf == null)
			{
				viewInfo.parentTf = ResolveCanvasTransform();
			}
			_views[key] = viewInfo;
			SyncRegistration(key);
		}

		public void Register(ViewType viewType, ViewInfo viewInfo)
		{
			Register((int)viewType, viewInfo);
		}

		public void UnRegister(int key)
		{
			_views.Remove(key);
			_viewCache.Remove(key);
			_opens.Remove(key);
			if (UIManager.instance != null)
			{
				UIManager.instance.UnregisterLegacyView(key);
			}
		}

		public void RemoveView(int key)
		{
			UnRegister(key);
		}

		public bool IsOpen(int key)
		{
			return UIManager.instance != null && UIManager.instance.HasActiveLegacyView(key);
		}

		public IBaseView GetView(int key)
		{
			SyncRegistration(key);
			IBaseView view = UIManager.instance != null ? UIManager.instance.GetLegacyView(key) : null;
			if (view != null)
			{
				_opens[key] = view;
				_viewCache[key] = view;
			}
			return view;
		}

		public T GetView<T>(int key) where T : class, IBaseView
		{
			return GetView(key) as T;
		}

		public void Destroy(int key)
		{
			if (UIManager.instance == null)
			{
				return;
			}
			UIManager.instance.CloseLegacyView(key);
			UIManager.instance.UnregisterLegacyView(key);
			_opens.Remove(key);
			_viewCache.Remove(key);
			_views.Remove(key);
		}

		public void Close(int key, params object[] args)
		{
			if (UIManager.instance == null)
			{
				Debug.LogError("UIManager instance is unavailable while closing legacy view.");
				return;
			}
			UIManager.instance.CloseLegacyView(key, args);
			_opens.Remove(key);
			_viewCache.Remove(key);
		}

		public async void Open(ViewType viewType, params object[] args)
		{
			try
			{
				await OpenAsync((int)viewType, args);
			}
			catch (Exception exception)
			{
				Debug.LogError(
					"Failed to open legacy view " +
					ViewTypeContract.GetStableName(viewType) + ": " +
					exception.Message);
			}
		}

		public async void Open(int key, params object[] args)
		{
			try
			{
				await OpenAsync(key, args);
			}
			catch (Exception exception)
			{
				Debug.LogError(
					"Failed to open legacy view " + key + ": " +
					exception.Message);
			}
		}

		public Task<IBaseView> OpenAsync(
			ViewType viewType,
			params object[] args)
		{
			return OpenAsync((int)viewType, args);
		}

		internal Task<GameObject> PreloadAsync(ViewType viewType)
		{
			UIManager manager = UIManager.instance;
			if (manager == null)
				return Task.FromException<GameObject>(new InvalidOperationException(
					"UIManager instance is unavailable while preloading legacy view."));
			// Default entries live in this facade until first use. Preloading must
			// publish the same registration as OpenAsync before accessing its cache.
			SyncRegistration((int)viewType);
			return manager.PreloadLegacyPrefabAsync(viewType);
		}

		public async Task<IBaseView> OpenAsync(
			int key,
			params object[] args)
		{
			UIManager manager = UIManager.instance;
			if (manager == null)
			{
				Debug.LogError("UIManager instance is unavailable while opening legacy view.");
				return null;
			}
			SyncRegistration(key);
			IBaseView view = await manager.OpenLegacyViewAsync(key, args);
			if (view == null)
			{
				return null;
			}
			_opens[key] = view;
			_viewCache[key] = view;
			return view;
		}

		private void SyncRegistration(int key)
		{
			if (UIManager.instance == null)
			{
				return;
			}
			ViewInfo viewInfo;
			if (!_views.TryGetValue(key, out viewInfo) || viewInfo == null)
			{
				return;
			}
			if (viewInfo.parentTf == null)
			{
				viewInfo.parentTf = ResolveCanvasTransform();
			}
			UIManager.instance.RegisterLegacyView(key, viewInfo, UIManager.UILayer.Layer5);
		}

		private Transform ResolveCanvasTransform()
		{
			if (canvasTf != null)
			{
				return canvasTf;
			}
			if (UIManager.instance != null)
			{
				// Keep legacy withdrawal panels inside their registered layer.
				// Appending them to the root Canvas puts them above Layer6 wallet
				// dialogs (and their masks), regardless of the dialog's open order.
				return UIManager.instance.GetLayerRoot(UIManager.UILayer.Layer5);
			}
			return null;
		}

		private void RegisterDefaultViews()
		{
			RegisterDefaultView(ViewType.kaiju_ui_1, "kaiju_ui_1");
			RegisterDefaultView(ViewType.kaiju_ui_1_1, "kaiju_ui_1");
			RegisterDefaultView(ViewType.kaiju_ui_2, "kaiju_ui_2");
			RegisterDefaultView(ViewType.kaiju_ui_2_1, "kaiju_ui_2_1");
			RegisterDefaultView(ViewType.kaiju_ui_3, "kaiju_ui_3");
			RegisterDefaultView(ViewType.kaiju_ui_4, "kaiju_ui_4");
			RegisterDefaultView(ViewType.kaiju_ui_5, "kaiju_ui_5");
			RegisterDefaultView(ViewType.SuccessMessage, "SuccessMessage");
			RegisterDefaultView(ViewType.WithDrawSucMessage, "WithDrawSucMessage");
			RegisterDefaultView(ViewType.WithDraw, "WithDraw");
			RegisterDefaultView(ViewType.WithDrawMethod, "WithDrawmethod");
			RegisterDefaultView(ViewType.WithDrawQues, "WithDrawQues");
			RegisterDefaultView(ViewType.WithDrawConfirm, "WithDrawConfirm");
			RegisterDefaultView(ViewType.WithDrawProcess, "WithDrawProcess");
			RegisterDefaultView(ViewType.WithDrawProcess1, "WithDrawProcess1");
			RegisterDefaultView(ViewType.WithDrawKeepEarn, "WithDrawKeepEarn");
			RegisterDefaultView(ViewType.WithDrawContinue, "WithDrawContinue");
			RegisterDefaultView(ViewType.WithDrawSuc, "WithDrawSuc");
			RegisterDefaultView(ViewType.WithDrawDateNow, "WithDrawDateNow");
			RegisterDefaultView(ViewType.WithDrawHistory, "WithDrawHistory");
			RegisterDefaultView(ViewType.RewardMessageWith, "RewardMessageWith");
			RegisterDefaultView(ViewType.RewardMessageMoney, "RewardMessageMoney");
			RegisterDefaultView(ViewType.RewardMessageDia, "RewardMessageDia");
		}

		private void RegisterDefaultView(ViewType viewType, string prefabName)
		{
			_views[(int)viewType] = new ViewInfo
			{
				prefabName = prefabName,
				parentTf = ResolveCanvasTransform()
			};
		}
	}
}
