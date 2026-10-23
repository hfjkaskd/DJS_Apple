using UnityEngine;

namespace WZSDK
{
	public class SafeArea : SafeAreaAdapter
	{
		public enum SimDevice
		{
			None = 0,
			iPhoneX = 1,
			iPhoneXsMax = 2,
			Pixel3XL_LSL = 3,
			Pixel3XL_LSR = 4
		}

		public static SimDevice Sim;

		private Rect[] NSA_iPhoneX =
		{
			new Rect(0f, 0.04187192f, 1f, 0.9039409f),
			new Rect(0.0541871935f, 0.056f, 0.891625643f, 0.944f)
		};

		private Rect[] NSA_iPhoneXsMax =
		{
			new Rect(0f, 0.03794643f, 1f, 0.9129464f),
			new Rect(0.04910714f, 0.0507246368f, 0.901785731f, 0.9492754f)
		};

		private Rect[] NSA_Pixel3XL_LSL =
		{
			new Rect(0f, 0f, 1f, 0.942229748f),
			new Rect(0f, 0f, 0.942229748f, 1f)
		};

		private Rect[] NSA_Pixel3XL_LSR =
		{
			new Rect(0f, 0f, 1f, 0.942229748f),
			new Rect(0.05777027f, 0f, 0.942229748f, 1f)
		};

		[SerializeField]
		private bool ConformX = true;

		[SerializeField]
		private bool ConformY = true;

		[SerializeField]
		private bool Logging;

        protected override SafeAreaEdges Edges =>
            (ConformX ? SafeAreaEdges.Horizontal : SafeAreaEdges.None) |
            (ConformY ? SafeAreaEdges.Top | SafeAreaEdges.Bottom : SafeAreaEdges.None);

        protected override void OnEnable()
        {
            base.OnEnable();
            if (Logging) Debug.LogFormat("[SafeArea] {0}: {1}", name, GetSafeArea());
        }

		protected override Rect GetSafeArea()
		{
			Rect safeArea = SafeAreaUtility.Current;
			if (Sim == SimDevice.None)
			{
				return safeArea;
			}

			Rect[] normalizedSafeAreas = null;
			switch (Sim)
			{
				case SimDevice.iPhoneX:
					normalizedSafeAreas = NSA_iPhoneX;
					break;
				case SimDevice.iPhoneXsMax:
					normalizedSafeAreas = NSA_iPhoneXsMax;
					break;
				case SimDevice.Pixel3XL_LSL:
					normalizedSafeAreas = NSA_Pixel3XL_LSL;
					break;
				case SimDevice.Pixel3XL_LSR:
					normalizedSafeAreas = NSA_Pixel3XL_LSR;
					break;
			}

			if (normalizedSafeAreas == null ||
				normalizedSafeAreas.Length < 2)
			{
				return safeArea;
			}
			Rect normalized = normalizedSafeAreas[
				Screen.height > Screen.width ? 0 : 1];
			return new Rect(
				normalized.x * Screen.width,
				normalized.y * Screen.height,
				normalized.width * Screen.width,
				normalized.height * Screen.height);
		}

	}
}
