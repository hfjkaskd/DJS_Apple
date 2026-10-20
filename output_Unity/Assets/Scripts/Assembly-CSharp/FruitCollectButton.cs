using System;
using UnityEngine.UI;

/// <summary>
/// Standard Button input on the fruit Image, with the existing fruit animation
/// driven by Selectable state changes instead of a drag-capable input handler.
/// </summary>
public sealed class FruitCollectButton : Button
{
	public event Action<bool> PressedStateChanged;

	private bool wasPressed;

	protected override void DoStateTransition(SelectionState state, bool instant)
	{
		base.DoStateTransition(state, instant);
		SetPressed(state == SelectionState.Pressed && IsActive() && IsInteractable());
	}

	protected override void InstantClearState()
	{
		base.InstantClearState();
		SetPressed(false);
	}

	private void SetPressed(bool pressed)
	{
		if (wasPressed == pressed)
		{
			return;
		}
		wasPressed = pressed;
		PressedStateChanged?.Invoke(pressed);
	}
}
