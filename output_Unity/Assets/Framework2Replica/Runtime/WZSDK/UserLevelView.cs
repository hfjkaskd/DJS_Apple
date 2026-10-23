using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	public class UserLevelView : BaseView
	{
		protected RectTransform mPlane;

		protected Button mExitBtn;

		protected Text mCurLevelText;

		protected Button mKeepPlayingBtn;

		protected Slider mLevelProgress;

		protected Text mLevelProgressText;

		private bool mIsEventsBound;

		private void Awake()
		{
			mPlane = transform.Find("Plane").GetComponent<RectTransform>();
			mExitBtn = transform.Find("Plane/ExitBtn").GetComponent<Button>();
			mCurLevelText = transform.Find("Plane/CurLevelText").GetComponent<Text>();
			mKeepPlayingBtn = transform.Find("Plane/KeepPlayingBtn").GetComponent<Button>();
			mLevelProgress = transform.Find("Plane/LevelProgress").GetComponent<Slider>();
			mLevelProgressText = transform.Find("Plane/LevelProgress/LevelProgressText").GetComponent<Text>();
		}

		public override void InitView()
		{
			BindButtonEvent();
			RefreshLevelData();
		}

		internal override void RefreshRuntimeGmPresentation()
		{
			RefreshLevelData();
		}

		private void RefreshLevelData()
		{
			GameManagerWZ gameManager = GameManagerWZ.instance;
			if (gameManager == null)
			{
				Debug.LogWarning("GameManagerWZ is null, unable to refresh user level data.");
				return;
			}

			int currentLevel = gameManager.userLevel;
			if (mCurLevelText != null)
			{
				mCurLevelText.text = string.Format(
					LocalizationManager.Instance.GetText("2128"),
					currentLevel + 1);
			}

			int currentExp = gameManager.userExp;
			int nextLevelNeedExp = GetNextLevelNeedExp(currentLevel);
			float progress = nextLevelNeedExp < 1
				? 1f
				: Mathf.Clamp01((float)currentExp / nextLevelNeedExp);

			if (mLevelProgress != null)
			{
				mLevelProgress.value = progress;
			}

			if (mLevelProgressText != null)
			{
				mLevelProgressText.text = string.Format("{0}%", (int)(progress * 100f));
			}

			Debug.Log(string.Format(
				"User level: {0}, current exp: {1}, next level need exp: {2}, progress: {3}",
				currentLevel + 1,
				currentExp,
				nextLevelNeedExp,
				progress));
		}

		private int GetNextLevelNeedExp(int currentLevel)
		{
			return UserDataManager.Instance != null
				? UserDataManager.Instance.GetNextLevelNeedExp()
				: 100;
		}

		protected void BindButtonEvent()
		{
			if (mIsEventsBound)
			{
				UnBindButtonEvent();
			}

			mExitBtn.onClick.AddListener(OnExitBtnClickHandle);
			mKeepPlayingBtn.onClick.AddListener(OnKeepPlayingBtnClickHandle);
			mIsEventsBound = true;
		}

		protected void UnBindButtonEvent()
		{
			mExitBtn.onClick.RemoveAllListeners();
			mKeepPlayingBtn.onClick.RemoveAllListeners();
		}

		private void OnExitBtnClickHandle()
		{
			UIManager.instance.CloseView(EUIType.UserLevelView);
		}

		private void OnKeepPlayingBtnClickHandle()
		{
			UIManager.instance.CloseView(EUIType.UserLevelView);
		}

		private void OnDestroy()
		{
			if (mIsEventsBound)
			{
				UnBindButtonEvent();
			}
		}

		public override void Start()
		{
		}

		public override void Update()
		{
		}
	}
}
