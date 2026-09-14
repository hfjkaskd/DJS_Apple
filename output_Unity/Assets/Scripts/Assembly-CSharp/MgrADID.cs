using System;
using UnityEngine;

public class MgrADID : MonoBehaviour
{
	public const string BIND_OBJ_NAME_S = "MgrADID";

	private static MgrADID instance;

	private const int ADID_ENABLE = 1;

	private const int ADID_DISABLE = 2;

	private const int ADID_UNKNOWN = 3;

	private const string ADID_DEFAULT = "0000-0000-0000-0000";

	private Action<int, string> innerADIDCallback;

	public static MgrADID Instance => null;

	public void Init(Action<int, string> adidCallback)
	{
	}

	public void OnAdvertisingIdReceived(string result)
	{
	}
}
