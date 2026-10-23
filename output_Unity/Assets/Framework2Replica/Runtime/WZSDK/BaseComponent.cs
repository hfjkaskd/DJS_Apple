namespace WZSDK
{
	public class BaseComponent : IComponent<IEntity>, IUnityObject, IDispose
	{
		public IEntity Owner { get; }

		public void Enable()
		{
		}

		public void Disable()
		{
			OnDisable();
		}

		protected virtual void OnDisable()
		{
		}

		public void Dispose()
		{
			OnDispose();
		}

		public virtual void OnDispose()
		{
		}
	}
}
