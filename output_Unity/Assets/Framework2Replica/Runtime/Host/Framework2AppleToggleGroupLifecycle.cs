using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Let authored inactive regional groups register children before enforcing one selection.</summary>
[RequireComponent(typeof(ToggleGroup))]
public sealed class Framework2AppleToggleGroupLifecycle : MonoBehaviour
{
    private ToggleGroup group;
    private void Awake() { group = GetComponent<ToggleGroup>(); }
    private void OnEnable()
    {
        group.allowSwitchOff = true;
        StartCoroutine(EnforceAfterRegistration());
    }
    private IEnumerator EnforceAfterRegistration()
    {
        yield return null;
        if (isActiveAndEnabled) group.allowSwitchOff = false;
    }
    private void OnDisable() { if (group != null) group.allowSwitchOff = true; }
}
