namespace WZSDK
{
	public class BaseModFrame : ILoad, IDispose
	{
		public void Load()
		{
			OnLoad();
		}

		protected virtual void OnLoad()
		{
		}

		public void Update()
		{
			OnUpdate();
		}

		public void FixedUpdate()
		{
			OnFixedUpdate();
		}

		protected virtual void OnUpdate()
		{
		}

		protected virtual void OnFixedUpdate()
		{
		}

		public void Dispose()
		{
			OnDispose();
		}

		protected virtual void OnDispose()
		{
		}

		protected void RegisetUpdateObj()
		{
		}
	}
}
