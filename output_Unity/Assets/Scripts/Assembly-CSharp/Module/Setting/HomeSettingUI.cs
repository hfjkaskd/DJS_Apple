using UnityEngine;

namespace Module.Setting
{
	/// <summary>首页设置弹窗：开关、语言、评分、反馈、隐私与退出。</summary>
	public class HomeSettingUI : BaseUI
	{
		[SerializeField]
		private GameObject m_CloseBtn;

		[SerializeField]
		private GameObject m_SoundBtn;

		[SerializeField]
		private GameObject m_SoundOn;

		[SerializeField]
		private GameObject m_SoundOff;

		[SerializeField]
		private GameObject m_MusicBtn;

		[SerializeField]
		private GameObject m_MusicOn;

		[SerializeField]
		private GameObject m_MusicOff;

		[SerializeField]
		private GameObject m_VibrateBtn;

		[SerializeField]
		private GameObject m_VibrateOn;

		[SerializeField]
		private GameObject m_VibrateOff;

		[SerializeField]
		private GameObject m_LanguageBtn;

		[SerializeField]
		private GameObject m_RateBtn;

		[SerializeField]
		private GameObject m_FeedBackBtn;

		[SerializeField]
		private GameObject m_PrivacyPolicyBtn;

		[SerializeField]
		private GameObject m_ExitBtn;

		public override PAIEAGDLCBJ Layer => PAIEAGDLCBJ.Top;

		protected override void Init()
		{
			if (m_CloseBtn != null)
			{
				MCCIJBJGMCK.Get(m_CloseBtn).onClick = OnCloseClick;
			}
			if (m_SoundBtn != null)
			{
				MCCIJBJGMCK.Get(m_SoundBtn).onClick = OnSoundClick;
			}
			if (m_MusicBtn != null)
			{
				MCCIJBJGMCK.Get(m_MusicBtn).onClick = OnMusicClick;
			}
			if (m_VibrateBtn != null)
			{
				MCCIJBJGMCK.Get(m_VibrateBtn).onClick = OnVibrateClick;
			}
			if (m_LanguageBtn != null)
			{
				MCCIJBJGMCK.Get(m_LanguageBtn).onClick = OnLanguageClick;
			}
			if (m_RateBtn != null)
			{
				MCCIJBJGMCK.Get(m_RateBtn).onClick = OnRateClick;
			}
			if (m_FeedBackBtn != null)
			{
				MCCIJBJGMCK.Get(m_FeedBackBtn).onClick = OnFeedBackClick;
			}
			if (m_PrivacyPolicyBtn != null)
			{
				MCCIJBJGMCK.Get(m_PrivacyPolicyBtn).onClick = OnPrivacyPolicyClick;
			}
			if (m_ExitBtn != null)
			{
				MCCIJBJGMCK.Get(m_ExitBtn).onClick = OnExitClick;
			}
		}

		protected override void BeforeOpen()
		{
			UpdateUI();
		}

		private void UpdateUI()
		{
			UpdateSoundUI();
			UpdateMusicUI();
			UpdateVibrateUI();
		}

		private void OnCloseClick(GameObject gameObject)
		{
			MgrUI.Instance.Close("pops/setting/HomeSettingUI", false);
		}

		private void OnSoundClick(GameObject gameObject)
		{
			SettingData data = BMNFNJFCPHG.Instance.SettingData;
			data.sound = !data.sound;
			GameAudio.SetSfxEnabled(data.sound);
			BMNFNJFCPHG.Instance.UpdateSaveData();
			UpdateSoundUI();
		}

		private void OnMusicClick(GameObject gameObject)
		{
			SettingData data = BMNFNJFCPHG.Instance.SettingData;
			data.music = !data.music;
			GameAudio.SetMusicEnabled(data.music);
			BMNFNJFCPHG.Instance.UpdateSaveData();
			UpdateMusicUI();
		}

		private void OnVibrateClick(GameObject gameObject)
		{
			SettingData data = BMNFNJFCPHG.Instance.SettingData;
			data.vibrate = !data.vibrate;
			BMNFNJFCPHG.Instance.UpdateSaveData();
			UpdateVibrateUI();
		}

		private void UpdateSoundUI()
		{
			bool on = BMNFNJFCPHG.Instance.SettingData.sound;
			if (m_SoundOn != null)
			{
				m_SoundOn.SetActive(on);
			}
			if (m_SoundOff != null)
			{
				m_SoundOff.SetActive(!on);
			}
		}

		private void UpdateMusicUI()
		{
			bool on = BMNFNJFCPHG.Instance.SettingData.music;
			if (m_MusicOn != null)
			{
				m_MusicOn.SetActive(on);
			}
			if (m_MusicOff != null)
			{
				m_MusicOff.SetActive(!on);
			}
		}

		private void UpdateVibrateUI()
		{
			bool on = BMNFNJFCPHG.Instance.SettingData.vibrate;
			if (m_VibrateOn != null)
			{
				m_VibrateOn.SetActive(on);
			}
			if (m_VibrateOff != null)
			{
				m_VibrateOff.SetActive(!on);
			}
		}

		private void OnLanguageClick(GameObject gameObject)
		{
			MgrUI.Instance.Open("pops/language/LanguageUI");
		}

		private void OnRateClick(GameObject gameObject)
		{
			MgrUI.Instance.Open("pops/rate/RateUI");
		}

		private void OnFeedBackClick(GameObject gameObject)
		{
			MgrUI.Instance.Open("pops/feedback/FeedBackUI");
		}

		private void OnPrivacyPolicyClick(GameObject gameObject)
		{
			MgrUI.Instance.Open("pops/privacypolicy/PrivacyPolicyUI");
		}

		private void OnExitClick(GameObject InGameObject)
		{
			MgrUI.Instance.Open("pops/quit/QuitUI");
		}
	}
}
