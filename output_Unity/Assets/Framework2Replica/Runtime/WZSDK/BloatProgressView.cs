using System;
using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	public class BloatProgressView : BaseView
	{
		private const int Stage5Id = 5;

		private const float RefreshInterval = 0.2f;

		private Image progress;

		private Image fill;

		private Text overallCount;

		private Text count;

		private RectTransform progressRoot;

		private Button clickBtn;

		private Button close;

		private bool isInitialized;

		private bool isEventsBound;

		private float nextRefreshTime;

		private float targetCoinOverride;
		internal float AppleSkinTargetAmount => GetTargetCoin(GameManagerWZ.instance);
		private float analyticsCurrent = float.NaN;
		private float analyticsTarget = float.NaN;

		private void Awake()
		{
			ResolveReferencesIfNeeded();
		}

		public override void InitView()
		{
			ResolveReferencesIfNeeded();
			BindEvents();
			RefreshView();
		}

		public override void Start()
		{
			ResolveReferencesIfNeeded();
			BindEvents();
			RefreshView();
		}

		public override void Update()
		{
			if (!isInitialized ||
				Time.unscaledTime < nextRefreshTime)
			{
				return;
			}
			RefreshView();
			nextRefreshTime =
				Time.unscaledTime + RefreshInterval;
		}

		private void OnDestroy()
		{
			RemoveEvents();
		}

		protected override void HandleViewArgs(object[] args)
		{
			targetCoinOverride = 0f;
			if (args == null)
			{
				return;
			}
			for (int i = 0; i < args.Length; i++)
			{
				if (args[i] is float)
				{
					targetCoinOverride = (float)args[i];
				}
				else if (args[i] is int)
				{
					targetCoinOverride = (int)args[i];
				}
				else if (args[i] is double)
				{
					targetCoinOverride = (float)(double)args[i];
				}
			}
		}

		private void ResolveReferencesIfNeeded()
		{
			if (isInitialized)
			{
				return;
			}
			progress = FindComponent<Image>("Progress");
			fill = FindComponent<Image>("Fill");
			overallCount = FindComponent<Text>("OverallCount");
			count = FindComponent<Text>("Count");
			progressRoot = FindComponent<RectTransform>(
				"ProgressRoot");
			clickBtn = FindComponent<Button>("ClickBtn");
			close = FindComponent<Button>("close");
			isInitialized = true;
		}

		private T FindComponent<T>(string childName) where T : Component
		{
			return FindChildUtility.FindChild<T>(
				gameObject, childName);
		}

		private void BindEvents()
		{
			if (isEventsBound)
			{
				return;
			}
			if (clickBtn != null)
			{
				clickBtn.onClick.AddListener(OnClick);
			}
			if (close != null)
			{
				close.onClick.AddListener(OnClick);
			}
			isEventsBound = true;
		}

		private void RemoveEvents()
		{
			if (!isEventsBound)
			{
				return;
			}
			if (clickBtn != null)
			{
				clickBtn.onClick.RemoveListener(OnClick);
			}
			if (close != null)
			{
				close.onClick.RemoveListener(OnClick);
			}
			isEventsBound = false;
		}

		private void OnClick()
		{
			if (UIManager.instance != null)
			{
				UIManager.instance.CloseView(
					EUIType.BloatProgressView);
			}
		}

		internal override void RefreshRuntimeGmPresentation()
		{
			RefreshView();
		}

		private void RefreshView()
		{
			if (progressRoot != null)
			{
				progressRoot.gameObject.SetActive(true);
			}
			GameManagerWZ gm = GameManagerWZ.instance;
			float current = Mathf.Max(
				0f, gm != null ? gm.currentCoin : 0f);
			float target = GetTargetCoin(gm);
			if (analyticsCurrent != current || analyticsTarget != target)
			{
				analyticsCurrent = current;
				analyticsTarget = target;
				MahjongProductAnalytics.Track("f2_compensation_progress", "current", current, "target", target, "target_override", targetCoinOverride, "complete", current >= target);
			}
			float ratio = Mathf.Clamp01(
				current / Mathf.Max(1f, target));
			if (progress != null)
			{
				progress.fillAmount = ratio;
			}
			if (fill != null)
			{
				fill.fillAmount = ratio;
			}
			if (overallCount != null)
			{
				overallCount.text = string.Format(
					"{0}%",
					Mathf.RoundToInt(ratio * 100f));
			}
			if (count != null && gm != null)
			{
				count.text = gm.FormatRegionalValue((double)current) +
					"/" + gm.FormatRegionalValue((double)target);
			}
		}

		private float GetTargetCoin(GameManagerWZ gm)
		{
			if (targetCoinOverride > 0f)
			{
				return targetCoinOverride;
			}
			int type;
			int value;
			if (gm != null &&
				gm.TryGetStageRequirement(
					Stage5Id, out type, out value) &&
				type == 2)
			{
				return value > 0
					? value
					: GameManagerWZ.Stage5CompensationTargetCoin;
			}
			return GameManagerWZ.Stage5CompensationTargetCoin;
		}

	}
}
