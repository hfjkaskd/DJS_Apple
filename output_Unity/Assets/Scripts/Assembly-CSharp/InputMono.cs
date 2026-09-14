using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputMono : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	public Action<GameObject> onClick;

	public Action<GameObject> onDown;

	public Action<GameObject> onUp;

	public Action<GameObject> onBeginDrag;

	public Action<GameObject> onDrag;

	public Action<GameObject> onEndDrag;

	public bool playClickSound = true;

	public bool LockSelf { get; set; }

	private bool Blocked => LockSelf || MCCIJBJGMCK.IsLock();

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (!Blocked)
		{
			onBeginDrag?.Invoke(gameObject);
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (!Blocked)
		{
			onDrag?.Invoke(gameObject);
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		if (!Blocked)
		{
			onEndDrag?.Invoke(gameObject);
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (Blocked || eventData.dragging)
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
