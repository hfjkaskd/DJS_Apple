using System;
using UnityEngine;

public class MgrUMP : MonoBehaviour
{
	public const string BIND_OBJ_NAME_S = "MgrUMP";

	private static MgrUMP instance;

	private Action endAction;

	private const string SUSSESS_CACHED_CONSENT = "success_cached_consent";

	public static MgrUMP Instance => null;

	public void Init(Action action)
	{
	}

	public void OnUMPFinish(string errorInfo)
	{
	}

	public void ShowUMPDialog()
	{
	}
}
