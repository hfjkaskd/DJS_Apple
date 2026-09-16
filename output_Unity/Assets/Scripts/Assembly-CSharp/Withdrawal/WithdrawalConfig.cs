using UnityEngine;

namespace FruitWithdrawal
{
    [CreateAssetMenu(menuName = "Fruit/Withdrawal Simulation Config")]
    public sealed class WithdrawalConfig : ScriptableObject
    {
        public WithdrawalRules Rules = WithdrawalRules.CreateDefault();
    }
}
