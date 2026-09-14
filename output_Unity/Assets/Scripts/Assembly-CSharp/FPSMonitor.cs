using System.Collections.Generic;

public class FPSMonitor : BaseSingleMono<FPSMonitor>
{
	public float AvgFPS1Min;

	public float AvgFPS3Min;

	public float AvgFPS5Min;

	private Queue<float> fpsHistory1Min;

	private Queue<float> fpsHistory3Min;

	private Queue<float> fpsHistory5Min;

	private int frameCount;

	private float elapsedTime;

	private const float FPS_CALCULATE_INTERVAL = 0.5f;

	private void Start()
	{
	}

	private void Update()
	{
	}

	private float CalculateAverageFPS(Queue<float> InFPSQueue)
	{
		return 0f;
	}
}
