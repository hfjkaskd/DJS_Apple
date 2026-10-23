using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	public class TargetWithdrawView : BaseView
	{
		private const int Stage4Id = 4;

		private const float RefreshInterval = 0.2f;

		private Text curCoinText;

		private Text titleText;

		private Text progressText;

		private Image progressImage;

		private Button generateMoreBtn;

		private Button close;

		private bool isInitialized;

		private bool isEventsBound;

		private int targetStageId;

		private float currentTargetCoin;

		private float currentRemainingCoin;

		private float nextRefreshTime;

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
			RefreshView();
		}

		public override void Update()
		{
			if (isInitialized && Time.unscaledTime >= nextRefreshTime)
			{
				RefreshView();
				nextRefreshTime = Time.unscaledTime + RefreshInterval;
			}
		}

		private void OnDestroy()
		{
			RemoveEvents();
		}

		protected override void HandleViewArgs(object[] args)
		{
			targetStageId = GetStageId(args);
		}

		private void ResolveReferencesIfNeeded()
		{
			if (isInitialized)
			{
				return;
			}

			curCoinText = FindByPath<Text>(
				"RewardMessage/Panel/Top/CurCoin", "CurCoin");
			titleText = FindByPath<Text>(
				"RewardMessage/Panel/Top/Title", "Title");
			progressText = FindByPath<Text>(
				"RewardMessage/Panel/Top/Progress/ProgressTxt",
				"ProgressTxt");
			progressImage = FindByPath<Image>(
				"RewardMessage/Panel/Top/Progress", "Progress");
			generateMoreBtn = FindByPath<Button>(
				"RewardMessage/Panel/Bottom/GenerateMoreBtn",
				"GenerateMoreBtn");
			close = FindByPath<Button>(
				"RewardMessage/Panel/close", "close");
			isInitialized = curCoinText != null ||
				titleText != null ||
				progressText != null ||
				progressImage != null ||
				generateMoreBtn != null ||
				close != null;
		}

		private T FindByPath<T>(string path, string fallbackName) where T : Component
		{
			GameObject target =
				FindChildUtility.FindChildByPath(gameObject, path);
			if (target == null)
			{
				target = FindChildUtility.FindChild(
					gameObject, fallbackName);
			}
			return target != null ? target.GetComponent<T>() : null;
		}

		private void BindEvents()
		{
			if (isEventsBound)
			{
				return;
			}
			if (generateMoreBtn != null)
			{
				generateMoreBtn.onClick.AddListener(OnGenerateMoreClick);
			}
			if (close != null)
			{
				close.onClick.AddListener(OnGenerateMoreClick);
			}
			isEventsBound = true;
		}

		private void RemoveEvents()
		{
			if (!isEventsBound)
			{
				return;
			}
			if (generateMoreBtn != null)
			{
				generateMoreBtn.onClick.RemoveListener(OnGenerateMoreClick);
			}
			isEventsBound = false;
		}

		internal override void RefreshRuntimeGmPresentation()
		{
			RefreshView();
		}

		private void RefreshView()
		{
			GameManagerWZ gm = GameManagerWZ.instance;
			if (gm == null)
			{
				return;
			}

			int stageId = targetStageId;
			if (stageId < 1)
			{
				stageId = Mathf.Max(gm.currentStage, 1);
			}
			targetStageId = stageId;

			float stageTargetCoin;
			if (!TryGetCoinStageTarget(
					gm, stageId, out stageTargetCoin))
			{
				RefreshFallbackState(gm);
				return;
			}

			float currentCoin = Mathf.Max(gm.currentCoin, 0f);
			currentTargetCoin = stageTargetCoin;
			currentRemainingCoin = stageTargetCoin - currentCoin;

			if (titleText != null)
			{
				titleText.text = string.Format(
					GetText(
						"2100",
						"Earn {0} more to withdraw {1}"),
					FormatRegionalNumber(currentRemainingCoin),
					FormatRegionalNumber(currentTargetCoin));
			}
			if (curCoinText != null)
			{
				curCoinText.text = FormatCoin(currentCoin) ?? string.Empty;
			}
			if (progressText != null)
			{
				progressText.text = FormatCoin(currentCoin) + "/" +
					FormatCoin(currentTargetCoin);
			}
			if (progressImage != null)
			{
				progressImage.fillAmount = Mathf.Clamp01(
					currentCoin / Mathf.Max(currentTargetCoin, 1f));
			}

			SetGenerateMoreState(
				stageId == Stage4Id && currentRemainingCoin > 0f);
		}

		private void RefreshFallbackState(GameManagerWZ gm)
		{
			currentTargetCoin = 0f;
			currentRemainingCoin = 0f;
			if (titleText != null)
			{
				titleText.text = GetText("2149", "Task completed");
			}
			if (curCoinText != null && gm != null)
			{
				curCoinText.text =
					FormatCoin(gm.currentCoin) ?? string.Empty;
			}
			if (progressText != null)
			{
				progressText.text = "0/0";
			}
			if (progressImage != null)
			{
				progressImage.fillAmount = 0f;
			}
			SetGenerateMoreState(false);
		}

		private void SetGenerateMoreState(bool isVisible)
		{
			if (generateMoreBtn == null)
			{
				return;
			}
			generateMoreBtn.gameObject.SetActive(isVisible);
			generateMoreBtn.interactable = isVisible;
		}

		private void OnGenerateMoreClick()
		{
			SoundManager.Instance?.PlayUIClickSFX();
			int stageId = targetStageId;
			CloseCurrentView();
			if (stageId == Stage4Id)
			{
				UIManager.instance?.ShowView(EUIType.SlotView, 50);
			}
		}

		private void CloseCurrentView()
		{
			if (canvasGroup != null)
			{
				HideView();
				return;
			}
			gameObject.SetActive(false);
		}

		private bool TryGetCoinStageTarget(GameManagerWZ gm, int stageId, out float stageTargetCoin)
		{
			stageTargetCoin = 0f;
			if (gm == null)
			{
				return false;
			}

			int type;
			int value;
			if (!gm.TryGetStageRequirement(
					stageId, out type, out value) ||
				type != 2 ||
				value < 1)
			{
				return false;
			}
			stageTargetCoin = value;
			return true;
		}

		private int GetStageId(object[] args)
		{
			if (args != null && args.Length > 0)
			{
				if (args[0] is int && (int)args[0] > 0)
				{
					return (int)args[0];
				}
				int parsedStageId;
				if (args[0] is string &&
					int.TryParse((string)args[0], out parsedStageId) &&
					parsedStageId > 0)
				{
					return parsedStageId;
				}
			}

			GameManagerWZ gm = GameManagerWZ.instance;
			return gm != null ? Mathf.Max(gm.currentStage, 1) : 1;
		}

		private string FormatCoin(float coin)
		{
			GameManagerWZ gm = GameManagerWZ.instance;
			if (gm != null)
			{
				return gm.FormatRegionalValue((double)coin);
			}
			if (FacadePayTypeExtend.RegionalChangeHandle != null)
			{
				return FacadePayTypeExtend.RegionalChangeHandle(coin);
			}
			return GetCurrencyMark() + FormatPlainCoin(coin);
		}

		private string FormatRegionalNumber(float coin)
		{
			GameManagerWZ gm = GameManagerWZ.instance;
			return gm != null
				? gm.FormatRegionalValue((double)coin)
				: FormatPlainCoin(coin);
		}

		private string FormatPlainCoin(float coin)
		{
			GameManagerWZ gm = GameManagerWZ.instance;
			int decimalCount = gm != null
				? Mathf.Max(gm.decimals, 0)
				: 2;
			return coin.ToString(string.Format("F{0}", decimalCount))
				.TrimEnd('0')
				.TrimEnd('.');
		}

		private string GetCurrencyMark()
		{
			GameManagerWZ gm = GameManagerWZ.instance;
			return gm != null ? gm.mark : "$";
		}

		private string GetText(string key, string fallback)
		{
			LocalizationManager localization = LocalizationManager.Instance;
			if (localization == null)
			{
				return fallback;
			}
			string value = localization.GetText(key);
			return string.IsNullOrEmpty(value) ? fallback : value;
		}
	}
}
