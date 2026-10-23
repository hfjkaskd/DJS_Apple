namespace WZSDK
{
	public interface IComponent<T> : IUnityObject where T : IEntity
	{
		T Owner { get; }
	}
}
