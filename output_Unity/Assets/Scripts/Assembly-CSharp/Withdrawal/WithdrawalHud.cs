using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FruitWithdrawal
{
    public sealed class WithdrawalHud : MonoBehaviour
    {
        [SerializeField] public Button CashButton;
        [SerializeField] public Button DiamondButton;
        [SerializeField] public Button RewardsButton;
        [SerializeField] public TMP_Text Cash;
        [SerializeField] public TMP_Text Diamonds;
        [SerializeField] public TMP_Text Rewards;
        private void Awake()
        {
            CashButton.onClick.AddListener(() => WithdrawalHost.Open(WithdrawalCurrency.Cash));
            DiamondButton.onClick.AddListener(() => WithdrawalHost.Open(WithdrawalCurrency.Diamonds));
            RewardsButton.onClick.AddListener(() => WithdrawalHost.Open(WithdrawalCurrency.Cash, true));
        }
        private void OnEnable() { WithdrawalService.Instance.Changed += Refresh; Refresh(); }
        private void OnDisable() { WithdrawalService.Instance.Changed -= Refresh; }
        private void Refresh()
        {
            var service = WithdrawalService.Instance;
            if (!service.IsAvailable || service.State == null) { Rewards.text = "Test unavailable"; return; }
            Cash.text = WithdrawalHost.Money(service.State.CashCents);
            Diamonds.text = service.State.DiamondUnits.ToString("N0", System.Globalization.CultureInfo.InvariantCulture);
            Rewards.text = service.State.PendingRewards.Count > 0 ? "Claim (" + service.State.PendingRewards.Count + ")" : "Rewards";
        }
    }
}
