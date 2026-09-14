using System;
using UnityEngine;

public abstract class BaseSDK : MonoBehaviour
{
	public abstract void Config(string sdkKey, bool user_deny, Action<bool, string> callback);

	public abstract void Init(string sdkKey, bool user_deny, Action<bool, string> callback);

	public abstract void ConfigSDK(bool user_deny);
}
