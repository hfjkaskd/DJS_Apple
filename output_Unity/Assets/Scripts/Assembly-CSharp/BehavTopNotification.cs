using System;
using TMPro;
using UnityEngine;

public class BehavTopNotification : MyMonoBehaviour
{
	public static BehavTopNotification Instance;

	[SerializeField]
	private TextMeshProUGUI content;

	private Action onClickAction;

	private bool show;

	private void Awake()
	{
	}

	private void OnClick(GameObject obj)
	{
	}

	public void Show(string s, Action onClick)
	{
	}

	private void Close()
	{
	}
}
