using UnityEngine;

[CreateAssetMenu(menuName = "Harvest/Simulated Reward Config", fileName = "HarvestRewardConfig")]
public sealed class HarvestRewardConfig : ScriptableObject
{
    [SerializeField] private HarvestRewardSettings settings = new HarvestRewardSettings();
    public HarvestRewardSettings Settings { get { return settings; } }
}
