using UnityEngine;

namespace WZSDK
{
	public class RotateObject : MonoBehaviour
	{
		public float degreesPerSecond;

		private void Update()
		{
			transform.Rotate(
				0f,
				0f,
				degreesPerSecond * Time.deltaTime);
		}
	}
}
