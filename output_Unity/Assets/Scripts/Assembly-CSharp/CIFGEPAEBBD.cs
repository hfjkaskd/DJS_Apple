using System;
using System.Collections.Generic;
using UnityEngine;

public class CIFGEPAEBBD
{
	public class ILADOGLDHFE
	{
		public string name;

		public Action<UnityEngine.Object> loadAction;

		public Action successAction;

		public Action<string> failedAction;

		public Action<float, float> progressAction;
	}

	public string path;

	public Dictionary<string, ILADOGLDHFE> nameActionDic;

	public void AddNameAction(string name, Action<UnityEngine.Object> loaded, Action<string> failed, Action<float, float> progressAction, Action success)
	{
	}

	public void RemoveNameAction(string name, Action<UnityEngine.Object> loaded, Action<string> failed, Action<float, float> progressAction, Action success)
	{
	}
}
