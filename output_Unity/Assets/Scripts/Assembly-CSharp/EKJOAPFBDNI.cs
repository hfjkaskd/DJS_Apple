using System;
using UnityEngine;

public interface EKJOAPFBDNI
{
	T Load<T>(string path) where T : UnityEngine.Object;

	void LoadAsync<T>(string path, MonoBehaviour coroutineHost, Action<T> onComplete) where T : UnityEngine.Object;
}
