using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	public class WithdrawView : BaseView
	{
		public Text curGem;

		public Text CashCount;

		public Button WithdrawBtn;

		public Button ExitBtn;

		public RectTransform WTGroup;

		public Slider Process;

		public Text num;

		public Text Withdrawal;

		public Text juli;

		public Text mark;

		private int[] gemAmounts => Framework2Host.Settings.economy.rewards.gemExchangeCosts;

		private float[] cashAmounts => Framework2Host.Settings.economy.rewards.gemExchangeCashAmounts;

		private int currentSelectedIndex = -1;

		private Toggle[] toggles;

		private bool _isEventsBound;

		private Framework2AppleSkinPresenter appleSkin;

		private GameManagerWZ GM => GameManagerWZ.instance;

		public override void InitView()
		{
			if (appleSkin == null) appleSkin = GetComponent<Framework2AppleSkinPresenter>();
			appleSkin?.SetGemCosts(gemAmounts);
			if (WTGroup != null)
			{
				toggles = WTGroup.GetComponentsInChildren<Toggle>();
			}
			if (GM != null)
			{
				RefreshAllData();
			}
			BindEvents();
			RefreshAllData();
		}

		private void BindEvents()
		{
			if (_isEventsBound)
			{
				RemoveEvents();
			}
			if (toggles != null && gemAmounts != null)
			{
				for (int i = 0; i < toggles.Length && i < gemAmounts.Length; i++)
				{
					int index = i;
					Toggle toggle = toggles[i];
					if (toggle != null)
					{
						toggle.onValueChanged.AddListener(
							isOn => OnToggleValueChanged(index, isOn));
						Transform labelTransform =
							toggle.transform.Find("WTTI2Label");
						if (labelTransform != null)
						{
							Text label = labelTransform.GetComponent<Text>();
							if (label != null &&
								GM != null &&
								cashAmounts != null &&
								i < cashAmounts.Length)
							{
								label.text = GM.FormatRegionalValue(
									(double)cashAmounts[i]);
							}
						}
					}
				}
			}
			if (ExitBtn != null)
			{
				ExitBtn.onClick.AddListener(ExitBtnClick);
			}
			if (WithdrawBtn != null)
			{
				WithdrawBtn.onClick.AddListener(OnWithdrawBtnClick);
			}
			if (toggles != null && toggles.Length != 0)
			{
				toggles[0].isOn = true;
				currentSelectedIndex = 0;
				UpdateProcessAndTexts();
			}
			_isEventsBound = true;
		}

		private void ExitBtnClick()
		{
			if (UIManager.instance != null)
			{
				UIManager.instance.CloseView(EUIType.WithdrawView);
			}
		}

		private void RemoveEvents()
		{
			if (toggles != null)
			{
				for (int i = 0; i < toggles.Length; i++)
				{
					if (toggles[i] != null)
					{
						toggles[i].onValueChanged.RemoveAllListeners();
					}
				}
			}
			if (ExitBtn != null)
			{
				ExitBtn.onClick.RemoveAllListeners();
			}
			if (WithdrawBtn != null)
			{
				WithdrawBtn.onClick.RemoveAllListeners();
			}
			_isEventsBound = false;
		}

		internal override void RefreshRuntimeGmPresentation()
		{
			RefreshAllData();
		}

		private void RefreshAllData()
		{
			if (GM == null)
			{
				return;
			}
			RefreshGemInfo();
			UpdateCashCount();
			UpdateProcessAndTexts();
		}

		private void RefreshGemInfo()
		{
			if (curGem != null && GM != null)
			{
				curGem.text = GM.currentGems.ToString();
			}
		}

		private void UpdateCashCount()
		{
			if (GM == null || CashCount == null)
			{
				return;
			}
			int index = currentSelectedIndex >= 0
				? currentSelectedIndex
				: 0;
			CashCount.text = GM.FormatRegionalValue(
				GetCashEquivalent(GM.currentGems, index));
		}

		private void UpdateProcessAndTexts()
		{
			if (currentSelectedIndex < 0 ||
				gemAmounts == null ||
				cashAmounts == null ||
				currentSelectedIndex >= gemAmounts.Length ||
				currentSelectedIndex >= cashAmounts.Length)
			{
				return;
			}

			int requiredGems = gemAmounts[currentSelectedIndex];
			float cashAmount = cashAmounts[currentSelectedIndex];
			if (GM == null)
			{
				return;
			}

			float currentGems = GM.currentGems;
			if (Withdrawal != null)
			{
				Withdrawal.text = string.Format(
					LocalizationManager.Instance.GetText("2028"),
					GM.FormatRegionalValue((double)cashAmount));
			}
			if (Process != null)
			{
				Process.value = Mathf.Clamp01(currentGems / requiredGems);
			}
			if (num != null)
			{
				num.text = string.Format(
					"{0}/{1}",
					(int)currentGems,
					requiredGems);
			}
			if (juli != null)
			{
				double currentCash = GetCashEquivalent(
					currentGems, currentSelectedIndex);
				double remainingCash = Mathf.Max(
					0f, cashAmount - (float)currentCash);
				juli.text = string.Format(
					LocalizationManager.Instance.GetText("2030"),
					GM.FormatRegionalValue((double)cashAmount),
					GM.FormatRegionalValue(remainingCash));
			}
			if (mark != null)
			{
				double cashPerGem = requiredGems > 0
					? (double)cashAmount / requiredGems
					: 0.0;
				mark.text = "1=" +
					GM.FormatRegionalValue(cashPerGem);
			}
			RefreshGemInfo();
			UpdateCashCount();
		}

		private double GetCashEquivalent(float gems, int index)
		{
			if (gemAmounts == null ||
				cashAmounts == null ||
				index < 0 ||
				index >= gemAmounts.Length ||
				index >= cashAmounts.Length ||
				gemAmounts[index] <= 0)
			{
				return 0.0;
			}
			return (double)gems * cashAmounts[index] /
				gemAmounts[index];
		}

		private void OnToggleValueChanged(int index, bool isOn)
		{
			if (isOn)
			{
				MahjongProductAnalytics.Track("f2_gem_exchange_tier_selected", "index", index);
				currentSelectedIndex = index;
				UpdateProcessAndTexts();
			}
		}

		private void OnWithdrawBtnClick()
		{
			MahjongProductAnalytics.Track("f2_gem_exchange_requested", "index", currentSelectedIndex);
			PlayClick();
			if (currentSelectedIndex < 0)
			{
				WithdrawApiService.LogPaymentNotice(
					"WithdrawView",
					LocalizationManager.Instance.GetText("2143"),
					LogType.Warning);
				return;
			}

			int requiredGems = gemAmounts[currentSelectedIndex];
			if (GM == null)
			{
				return;
			}
			if (GM.currentGems < requiredGems)
			{
				MahjongProductAnalytics.Track("f2_gem_exchange_result", "result", "insufficient_gems", "required", requiredGems, "gems", GM.currentGems);
				WithdrawApiService.LogPaymentNotice(
					"WithdrawView",
					LocalizationManager.Instance.GetText("2142"),
					LogType.Warning);
				return;
			}

			float analyticsGemsBefore = GM.currentGems;
			double analyticsCashBefore = GM.GetCoin();
			GM.SubGem(requiredGems);
			float cashAmount = cashAmounts[currentSelectedIndex];
			GM.AddCoin(cashAmount);
			MahjongProductAnalytics.Track("f2_gem_exchange_result", "result", "completed", "tier", currentSelectedIndex, "requested_cash", cashAmount, "cash_before", analyticsCashBefore, "cash_after", GM.GetCoin(), "gems_before", analyticsGemsBefore, "gems_after", GM.currentGems, "withdrawal_application", false);
			WithdrawApiService.LogPaymentNotice(
				"WithdrawView",
				LocalizationManager.Instance.GetText("2058"),
				LogType.Log);
			RefreshAllData();
			if (UIManager.instance != null)
			{
				UIManager.instance.CloseView(EUIType.WithdrawView);
			}
		}

		private void PlayClick()
		{
			if (SoundManager.Instance != null)
			{
				SoundManager.Instance.PlayUIClickSFX();
			}
		}

		private void OnDestroy()
		{
			if (_isEventsBound)
			{
				RemoveEvents();
			}
		}

		public override void Start()
		{
		}

		public override void Update()
		{
		}
	}
}
