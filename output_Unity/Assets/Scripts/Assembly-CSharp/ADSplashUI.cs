using UnityEngine;

public class ADSplashUI : MonoBehaviour
{
	private static ADSplashUI _aDSplashUI;

	[SerializeField]
	private GameObject m_BtnClose;

	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	private bool isAdCloseProcessed;

	private bool willClose;

	private const float FADE_IN_TIME = 0.2f;

	private const float FADE_OUT_TIME = 0.2f;

	private uint m_CanvasFadeTweenId;

	public static ADSplashUI instance => null;

	public bool CurOpenState { get; private set; }

	public void Init()
	{
	}

	private void OnBtnCloseClick(GameObject gameObject)
	{
	}

	public void Show()
	{
	}

	public void CloseImmediately()
	{
	}

	private void Close()
	{
	}

	private void TryShowInterstitial()
	{
	}
}
