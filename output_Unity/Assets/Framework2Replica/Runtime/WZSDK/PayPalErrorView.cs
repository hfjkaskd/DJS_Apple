using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	public class PayPalErrorView : BaseView
	{
		private Button reEnterButton;

		private Button confirmButton;

		private Button closeButton;

		private Text curLuckyCoinText;

		private Text email;

		private DataModel dataModel;

		private bool isBound;

		private void Awake()
		{
			EnsureCanvasGroup();
			ResolveReferencesIfNeeded();
			BindEvents();
			RefreshLuckyWalletCoin();
		}

		private void OnEnable()
		{
			RefreshLuckyWalletCoin();
		}

		private void OnDestroy()
		{
			RemoveEvents();
		}

		public void Show(DataModel model = null)
		{
			if (model != null)
			{
				dataModel = model;
			}
			ShowView(model);
		}

		public void Hide()
		{
			HideView();
		}

		public void SetDataModel(DataModel model)
		{
			dataModel = model;
			RefreshLuckyWalletCoin();
		}

		public override void InitView()
		{
			EnsureCanvasGroup();
			ResolveReferencesIfNeeded();
			BindEvents();
			RefreshWalletBalance();
			RefreshLuckyWalletCoin();
		}

		public override void Start()
		{
		}

		public override void Update()
		{
		}

		protected override void HandleViewArgs(object[] args)
		{
			if (args != null && args.Length > 0)
			{
				DataModel model = args[0] as DataModel;
				if (model != null)
				{
					dataModel = model;
				}
			}
		}

		private void ResolveReferencesIfNeeded()
		{
			if (reEnterButton == null)
			{
				reEnterButton = FindChildUtility.FindChild<Button>(
					gameObject, "ReEnter");
			}
			if (confirmButton == null)
			{
				confirmButton = FindChildUtility.FindChild<Button>(
					gameObject, "Confirm");
			}
			if (email == null)
			{
				email = FindChildUtility.FindChild<Text>(
					gameObject, "email");
			}
			if (closeButton == null)
			{
				closeButton = FindChildUtility.FindChild<Button>(
					gameObject, "close");
			}
			if (curLuckyCoinText == null)
			{
				curLuckyCoinText = FindChildUtility.FindChild<Text>(
					gameObject, "CurLuckyCoin");
			}
			if (email != null)
			{
				email.text = GameManagerWZ.instance != null
					? GameManagerWZ.instance.getPlayerEmail() : string.Empty;
			}
		}

		private void EnsureCanvasGroup()
		{
			if (canvasGroup == null)
			{
				canvasGroup = GetComponent<CanvasGroup>();
			}
			if (canvasGroup == null)
			{
				canvasGroup = gameObject.AddComponent<CanvasGroup>();
			}
		}

		private void BindEvents()
		{
			if (isBound)
			{
				return;
			}
			if (reEnterButton != null)
			{
				reEnterButton.onClick.AddListener(OnReEnterClick);
			}
			if (confirmButton != null)
			{
				confirmButton.onClick.AddListener(OnCloseClick);
			}
			if (closeButton != null)
			{
				closeButton.onClick.AddListener(OnCloseClick);
			}
			isBound = true;
		}

		private void RemoveEvents()
		{
			if (!isBound)
			{
				return;
			}
			if (reEnterButton != null)
			{
				reEnterButton.onClick.RemoveListener(OnReEnterClick);
			}
			if (confirmButton != null)
			{
				confirmButton.onClick.RemoveListener(OnCloseClick);
			}
			if (closeButton != null)
			{
				closeButton.onClick.RemoveListener(OnCloseClick);
			}
			isBound = false;
		}

		private void OnReEnterClick()
		{
			Framework2BusinessAnalytics.Withdraw("receiver_error_reenter", dataModel, "open_input", (int)ViewType.WithDrawMethod);
			if (SoundManager.Instance != null)
			{
				SoundManager.Instance.PlayUIClickSFX();
			}
			if (dataModel != null && dataModel.RetryWithdraw)
			{
				dataModel.UseProcessViewOnMethodExit = false;
				dataModel.SubmitRetryWithdrawOnMethodExit = true;
			}
			if (GameApp.viewManager != null)
			{
				GameApp.viewManager.Open(
					ViewType.WithDrawMethod,
					GetOrCreateDataModel());
			}
			Hide();
		}

		private void OnCloseClick()
		{
			Framework2BusinessAnalytics.Withdraw("receiver_error_close", dataModel);
			if (SoundManager.Instance != null)
			{
				SoundManager.Instance.PlayUIClickSFX();
			}
			Hide();
		}

		private DataModel GetOrCreateDataModel()
		{
			if (dataModel == null)
			{
				dataModel =
					WithdrawApiService.CreateWalletWithdrawDataModel();
			}
			return dataModel;
		}

		internal override void RefreshRuntimeGmPresentation()
		{
			RefreshLuckyWalletCoin();
		}

		private void RefreshLuckyWalletCoin()
		{
			if (curLuckyCoinText == null ||
				GameManagerWZ.instance == null)
			{
				return;
			}
			double amount = dataModel != null && dataModel.Money > 0.0
				? dataModel.Money
				: WithdrawApiService.GetCachedWalletBalance();
			curLuckyCoinText.text =
				GameManagerWZ.instance.FormatRegionalValue(amount);
		}

		private void RefreshWalletBalance()
		{
			if (GameManagerWZ.instance == null)
			{
				return;
			}
			StartCoroutine(WithdrawApiService.QueryWalletBalance(
				wallet => RefreshLuckyWalletCoin(),
				error =>
				{
					Debug.LogError(
						"[PayPalErrorView] Wallet balance request failed: " +
						error);
					RefreshLuckyWalletCoin();
				}));
		}

	}
}
