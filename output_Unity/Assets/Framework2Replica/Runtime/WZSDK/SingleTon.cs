namespace WZSDK
{
	public class SingleTon<T>
		where T : new()
	{
		private static readonly T instance = new T();

		public static T Instance => instance;

		public virtual void Init()
		{
		}

		public virtual void Update(float dt)
		{
		}

		public virtual void OnDestroy()
		{
		}
	}
	public abstract class Singleton<T> where T : ILoad, new()
	{
		private static readonly object obj = new object();

		private static T mInstance;

		public static T Instance
		{
			get
			{
				if (mInstance == null)
				{
					lock (obj)
					{
						if (mInstance == null)
						{
							mInstance = new T();
							mInstance.Load();
						}
					}
				}
				return mInstance;
			}
		}

		protected virtual void DestroyInstance()
		{
			IDispose disposable = mInstance as IDispose;
			disposable?.Dispose();
			mInstance = default(T);
		}
	}
}
