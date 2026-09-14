using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputMonoNoDrag : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerDownHandler, IPointerUpHandler
{
	public Action<GameObject> onClick;

	public Action<GameObject> onDown;

	public Action<GameObject> onUp;

	public bool playClickSound = true;

	private bool Blocked => MCCIJBJGMCK.IsLock();

	public void OnPointerClick(PointerEventData eventData)
	{
		if (Blocked)
		{
			return;
		}
		if (playClickSound && onClick != null)
		{
			GameAudio.Play(DLMJOHCOJKN.Play_sfx_ui_button_common);
		}
		onClick?.Invoke(gameObject);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (!Blocked)
		{
			onDown?.Invoke(gameObject);
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (!Blocked)
		{
			onUp?.Invoke(gameObject);
		}
	}
}
