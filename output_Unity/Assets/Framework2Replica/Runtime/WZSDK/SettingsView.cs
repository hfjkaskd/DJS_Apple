using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	public class SettingsView : BaseView
	{
		protected RectTransform mPlane;

		protected Button mExitBtn;

		protected Image mPlayerIcon;

		protected Text mUserNameText;

		protected Text mUserIDText;

		protected Text mUserLv;

		protected Button mUserLevelBtn;

		protected Toggle mM_Toggle;

		protected Text mMON;

		protected Text mMOFF;

		protected Toggle mS_Toggle;

		protected RectTransform SoundBackground;

		protected RectTransform VibrationBackground;

		protected Toggle mV_Toggle;

		protected Button mWRBtn;

		protected Button mPrivacyPolicyBtn;

		protected Button TermsofUseBtn;

		protected RectTransform mS_Icon;

		protected RectTransform mV_Icon;

		private bool mIsEventsBound;

		private GameManagerWZ Gm => GameManagerWZ.instance;

		public void Awake()
		{
			mPlane = transform.Find("Plane").GetComponent<RectTransform>();
			mExitBtn = transform.Find("Plane/ExitBtn").GetComponent<Button>();
			mPlayerIcon = transform.Find("Plane/Bg2/PlayerIcon").GetComponent<Image>();
			mUserNameText = transform.Find("Plane/Bg2/UserNameText").GetComponent<Text>();
			mUserIDText = transform.Find("Plane/Bg2/UserIDText").GetComponent<Text>();
			mUserLv = transform.Find("Plane/Bg2/UserLv").GetComponent<Text>();
			mUserLevelBtn = transform.Find("Plane/Bg2/UserLevelBtn").GetComponent<Button>();
			mWRBtn = transform.Find("Plane/WRBtn").GetComponent<Button>();
			mM_Toggle = transform.Find("Plane/Muslc/m_Toggle").GetComponent<Toggle>();
			mMON = transform.Find("Plane/Muslc/m_Toggle/mON").GetComponent<Text>();
			mMOFF = transform.Find("Plane/Muslc/m_Toggle/mOFF").GetComponent<Text>();
			mS_Toggle = transform.Find("Plane/Sound/s_Toggle").GetComponent<Toggle>();
			SoundBackground = transform.Find("Plane/Sound/s_Toggle/Background").GetComponent<RectTransform>();
			mV_Toggle = transform.Find("Plane/Vibration/v_Toggle").GetComponent<Toggle>();
			VibrationBackground = transform.Find("Plane/Vibration/v_Toggle/Background").GetComponent<RectTransform>();
			mPrivacyPolicyBtn = transform.Find("Plane/Bg2/PrivacyPolicyBtn").GetComponent<Button>();

			Transform termsOfUseTransform = transform.Find("Plane/Bg2/TermsofUseBtn");
			TermsofUseBtn = termsOfUseTransform != null
				? termsOfUseTransform.GetComponent<Button>()
				: null;

			mS_Icon = transform.Find("Plane/Sound/s_Toggle/s_Icon").GetComponent<RectTransform>();
			mV_Icon = transform.Find("Plane/Vibration/v_Toggle/v_Icon").GetComponent<RectTransform>();
		}

		public override void InitView()
		{
			RefreshUserData();
			InitializeToggleStates();
			BindButtonEvent();
		}

		public override void Update()
		{
		}

		internal override void RefreshRuntimeGmPresentation()
		{
			RefreshUserData();
		}

		private void RefreshUserData()
		{
			GameManagerWZ gm = Gm;
			if (gm == null)
			{
				return;
			}

			mUserNameText.text = gm.userName ?? string.Empty;
			if (gm.userID == null)
			{
				return;
			}

			mUserIDText.text = "ID:" + gm.userID.Replace("-", string.Empty).Substring(0, 16);
			mUserLv.text = string.Format("LV.{0}", gm.userLevel + 1);
			mWRBtn.gameObject.SetActive(false);
		}

		private void InitializeToggleStates()
		{
			SoundManager soundManager = SoundManager.Instance;
			if (soundManager == null)
			{
				return;
			}

			if (mM_Toggle != null)
			{
				mM_Toggle.SetIsOnWithoutNotify(soundManager.musicState == 1);
			}

			if (mS_Toggle != null)
			{
				bool soundOn = HasDedicatedMusicToggle()
					? soundManager.soundState == 1
					: soundManager.musicState == 1;
				mS_Toggle.SetIsOnWithoutNotify(soundOn);
				UpdateSoundToggleVisual(soundOn);
			}

			if (mV_Toggle != null)
			{
				bool vibrationOn = soundManager.hapticState == 1;
				mV_Toggle.SetIsOnWithoutNotify(vibrationOn);
				UpdateVibrationToggleVisual(vibrationOn);
			}
		}

		protected void BindButtonEvent()
		{
			if (mIsEventsBound)
			{
				RemoveButtonEvents();
			}

			mExitBtn.onClick.AddListener(OnExitBtnClickHandle);
			mUserLevelBtn.onClick.AddListener(OnUserLevelBtnClickHandle);
			mPrivacyPolicyBtn.onClick.AddListener(OnPrivacyPolicyBtnClickHandle);
			mWRBtn.onClick.AddListener(OnWRButtonBtnClickHandle);
			if (TermsofUseBtn != null)
			{
				TermsofUseBtn.onClick.AddListener(OnTermsOfUseBtnClickHandle);
			}
			mM_Toggle.onValueChanged.AddListener(OnM_ToggleValueChange);
			mS_Toggle.onValueChanged.AddListener(OnS_ToggleValueChange);
			mV_Toggle.onValueChanged.AddListener(OnV_ToggleValueChange);
			mIsEventsBound = true;
		}

		private void OnTermsOfUseBtnClickHandle()
		{
			if (SoundManager.Instance != null)
			{
				SoundManager.Instance.PlayUIClickSFX();
			}
			Apple.Framework2.AppleF2Runtime.OpenHostTerms?.Invoke();
		}

		private void RemoveButtonEvents()
		{
			mExitBtn.onClick.RemoveAllListeners();
			mUserLevelBtn.onClick.RemoveAllListeners();
			mPrivacyPolicyBtn.onClick.RemoveAllListeners();
			if (mWRBtn != null)
			{
				mWRBtn.onClick.RemoveAllListeners();
			}
			if (TermsofUseBtn != null)
			{
				TermsofUseBtn.onClick.RemoveAllListeners();
			}
			mM_Toggle.onValueChanged.RemoveAllListeners();
			mS_Toggle.onValueChanged.RemoveAllListeners();
			mV_Toggle.onValueChanged.RemoveAllListeners();
		}

		private void OnWRButtonBtnClickHandle()
		{
			GameManagerWZ gm = Gm;
			if (gm == null)
			{
				return;
			}

			int level = gm.currentLv;
			double money = gm.GetCoin();
			DataModel dataModel = new DataModel
			{
				Name = gm.getPlayerName(),
				Phone = gm.getPlayerPhone(),
				Email = gm.getPlayerEmail(),
				Level = level - 1,
				Money = money,
				Type = 1
			};

			var history = SPlayerPrefs.GetListTem<HistoryModel>(FacadePlayerPrefExtend.History);
			if (history == null || history.Count == 0 || GameApp.viewManager == null)
			{
				return;
			}

			GameApp.viewManager.Open(ViewType.WithDrawHistory, dataModel);
		}

		private void OnPrivacyPolicyBtnClickHandle()
		{
			if (SoundManager.Instance != null)
			{
				SoundManager.Instance.PlayUIClickSFX();
			}
			Apple.Framework2.AppleF2Runtime.OpenHostPrivacy?.Invoke();
		}

		private void OnExitBtnClickHandle()
		{
			if (SoundManager.Instance != null)
			{
				SoundManager.Instance.PlayUIClickSFX();
			}
			if (UIManager.instance != null)
			{
				UIManager.instance.CloseView(EUIType.SettingsView);
			}
		}

		private void OnUserLevelBtnClickHandle()
		{
			if (SoundManager.Instance != null)
			{
				SoundManager.Instance.PlayUIClickSFX();
			}
			if (UIManager.instance != null)
			{
				UIManager.instance.CloseView(EUIType.SettingsView);
				if (UIManager.instance != null)
				{
					UIManager.instance.ShowView(EUIType.UserLevelView);
				}
			}
		}

		private void OnM_ToggleValueChange(bool b)
		{
			if (SoundManager.Instance != null)
			{
				SoundManager.Instance.ToogleMusic(b);
			}
		}

		private void OnS_ToggleValueChange(bool b)
		{
			if (SoundManager.Instance != null)
			{
				if (HasDedicatedMusicToggle())
				{
					SoundManager.Instance.ToogleSound(b);
				}
				else
				{
					SoundManager.Instance.ToogleAllAudio(b);
				}
			}
			UpdateSoundToggleVisual(b);
		}

		private void OnV_ToggleValueChange(bool b)
		{
			if (SoundManager.Instance != null)
			{
				SoundManager.Instance.ToogleHaptic(b);
			}
			UpdateVibrationToggleVisual(b);
		}

		private bool HasDedicatedMusicToggle()
		{
			return mM_Toggle != null && mM_Toggle.gameObject.activeInHierarchy;
		}

		private void UpdateSoundToggleVisual(bool isOn)
		{
			SoundBackground.gameObject.SetActive(!isOn);
			mS_Icon.localPosition = new Vector3(isOn ? 88f : -88f, 36f, 0f);
		}

		private void UpdateVibrationToggleVisual(bool isOn)
		{
			VibrationBackground.gameObject.SetActive(!isOn);
			mV_Icon.localPosition = new Vector3(isOn ? 88f : -88f, 36f, 0f);
		}

		private void OnDestroy()
		{
			if (mIsEventsBound)
			{
				RemoveButtonEvents();
			}
		}

		public override void Start()
		{
		}
	}
}
