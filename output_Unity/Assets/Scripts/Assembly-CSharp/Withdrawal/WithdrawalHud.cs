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
        private bool bound;
        private WithdrawalService service;

        private void OnEnable() { Bind(); }
        private void OnDisable() { Unbind(); }
        private void OnDestroy() { Unbind(); }

        public void Bind()
        {
            if (bound) return;
            service = WithdrawalService.Instance;
            CashButton.onClick.AddListener(OpenCash);
            DiamondButton.onClick.AddListener(OpenDiamonds);
            RewardsButton.onClick.AddListener(OpenRewards);
            service.Changed += Refresh;
            bound = true;
            Refresh();
        }

        public void Unbind()
        {
            if (!bound) return;
            service.Changed -= Refresh;
            CashButton.onClick.RemoveListener(OpenCash);
            DiamondButton.onClick.RemoveListener(OpenDiamonds);
            RewardsButton.onClick.RemoveListener(OpenRewards);
            bound = false;
            service = null;
        }

        private void OpenCash() { WithdrawalHost.Open(WithdrawalCurrency.Cash); }
        private void OpenDiamonds() { WithdrawalHost.Open(WithdrawalCurrency.Diamonds); }
        private void OpenRewards() { WithdrawalHost.Open(WithdrawalCurrency.Cash, true); }

        public void Refresh()
        {
            if (service == null) service = WithdrawalService.Instance;
            bool available = service.IsAvailable && service.State != null;
            CashButton.interactable = DiamondButton.interactable = RewardsButton.interactable = available;
            if (!available) { Cash.text = Diamonds.text = "--"; Rewards.text = "Unavailable"; return; }
            Cash.text = WithdrawalHost.Money(service.State.CashCents);
            Diamonds.text = service.State.DiamondUnits.ToString("N0", System.Globalization.CultureInfo.InvariantCulture);
            Rewards.text = service.State.PendingRewards.Count > 0 ? "Claim (" + service.State.PendingRewards.Count + ")" : "Rewards";
        }
    }
}
