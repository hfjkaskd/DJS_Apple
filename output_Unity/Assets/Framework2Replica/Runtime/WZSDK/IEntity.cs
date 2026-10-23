using UnityEngine;

namespace WZSDK
{
	public interface IEntity
	{
		int ID { get; }

		Vector3 Position { get; }
	}
}
