using System.Collections.Generic;
using UnityEngine;

namespace WZSDK
{
	public class BaseViewWithDraw : UIManagedViewBase
	{
		protected Dictionary<string, GameObject> m_cache_gos;

		protected bool isShow;

		private object[] lastViewArgs;

		private void Awake()
		{
			OnAwake();
		}

		private void Start()
		{
			OnStart();
		}

		protected virtual void OnAwake()
		{
		}

		protected virtual void OnStart()
		{
		}

		public override bool IsShow()
		{
			return isShow;
		}

		public override void InitUi()
		{
		}

		public override void InitData()
		{
			MarkInitialized();
		}

		public virtual void InitView()
		{
		}

		protected virtual void HandleViewArgs(object[] args)
		{
		}

		public virtual void ShowView()
		{
			ShowView(null);
		}

		public virtual void ShowView(params object[] args)
		{
			gameObject.SetActive(true);
			isShow = true;
			if (args != null && args.Length > 0)
			{
				lastViewArgs = (object[])args.Clone();
				HandleViewArgs(args);
			}
			MarkInitialized();
			InitView();
			LocalizationManager.Instance?.ApplyView(gameObject);
            MahjongAnalyticsView.Observe(gameObject, this is kaiju_ui_1 ? "startup_introduction" : "f2.legacy." + ViewTypeContract.GetStableName((ViewType)ViewId));
            if (lastViewArgs != null)
                foreach (object arg in lastViewArgs)
                    if (arg is DataModel data) Framework2BusinessAnalytics.Withdraw("view_shown", data, "shown", ViewId);
		}

		internal void ReapplyLastViewArgsForRuntimeGm()
		{
			if (lastViewArgs == null || lastViewArgs.Length == 0)
			{
				return;
			}
			HandleViewArgs((object[])lastViewArgs.Clone());
		}

		public virtual void HideView()
		{
            MahjongAnalyticsView.Closed(gameObject, "hide");
			isShow = false;
			gameObject.SetActive(false);
		}

		public override void Open(params object[] args)
		{
			ShowView(args);
		}

		public override void Close(params object[] args)
		{
			HideView();
			DestroyView();
		}

		public override void SetVisible(bool value)
		{
			gameObject.SetActive(value);
			isShow = value;
		}

		public GameObject Find(string res)
		{
			GameObject cached;
			if (!m_cache_gos.TryGetValue(res, out cached) || cached == null)
			{
				Transform child = transform.Find(res);
				cached = child != null ? child.gameObject : null;
				if (cached != null)
				{
					m_cache_gos[res] = cached;
				}
			}
			return cached;
		}

		public T Find<T>(string res) where T : Component
		{
			GameObject target = Find(res);
			return target != null ? target.GetComponent<T>() : null;
		}

		public BaseViewWithDraw()
		{
			m_cache_gos = new Dictionary<string, GameObject>();
		}
	}
}
