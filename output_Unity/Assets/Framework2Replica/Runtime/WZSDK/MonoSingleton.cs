using UnityEngine;

namespace WZSDK
{
	public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>, ILoad
	{
		private static T mInstance;

		public static T Instance => mInstance;

		protected virtual void Awake()
		{
			mInstance = (T)this;
			mInstance.Load();
		}

		protected virtual void OnDestroy()
		{
			if (mInstance == this) mInstance = null;
		}

		public void Destroy()
		{
			mInstance = null;
		}
	}
}
