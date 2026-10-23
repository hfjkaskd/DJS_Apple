using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	public class FailView : BaseView
	{
		protected RectTransform mPlane;

		protected Button mExitBtn;

		protected Button mReStartBtn;

		protected Button mReAdBtn;

		protected Button ContinueBtn;

		private bool mIsEventsBound;

		public void Awake()
		{
			mPlane = transform.Find("Plane").GetComponent<RectTransform>();
			mExitBtn = transform.Find("Plane/ExitBtn").GetComponent<Button>();
			ContinueBtn = transform.Find("Plane/ContinueBtn").GetComponent<Button>();
			mReStartBtn = transform.Find("Plane/ReStartBtn").GetComponent<Button>();
			mReAdBtn = transform.Find("Plane/adBtn").GetComponent<Button>();
		}

		public override void InitView()
		{
			BindButtonEvent();
		}

		public override void Start()
		{
		}

		public override void Update()
		{
		}

		protected void BindButtonEvent()
		{
			if (mIsEventsBound)
			{
				UnBindButtonEvent();
			}
			mExitBtn.onClick.AddListener(OnExitBtnClickHandle);
			ContinueBtn.onClick.AddListener(OnExitBtnClickHandle);
			mReStartBtn.onClick.AddListener(OnReStartBtnClickHandle);
			mReAdBtn.onClick.AddListener(OnADBtnClickHandle);
			mIsEventsBound = true;
		}

		private void OnExitBtnClickHandle()
		{
			UIManager.instance.CloseView(EUIType.FailView);
		}

		private void OnReStartBtnClickHandle()
		{
			GameManagerWZ.instance?.ReplayGame();
			UIManager.instance.CloseView(EUIType.FailView);
		}

		private void OnADBtnClickHandle()
		{
			AdsControl.Instance.ShowRewardedAd(
				"FailView_success_reward",
				success =>
				{
					if (success)
					{

					}
					else
					{
						GameManagerWZ.instance?.ReplayGame();
					}
				},
				true);
			UIManager.instance.CloseView(EUIType.FailView);
		}

		protected void UnBindButtonEvent()
		{
			mExitBtn.onClick.RemoveAllListeners();
			mReStartBtn.onClick.RemoveAllListeners();
			mReAdBtn.onClick.RemoveAllListeners();
			mIsEventsBound = false;
		}

		private void OnDestroy()
		{
			if (mIsEventsBound)
			{
				UnBindButtonEvent();
			}
		}
	}
}
