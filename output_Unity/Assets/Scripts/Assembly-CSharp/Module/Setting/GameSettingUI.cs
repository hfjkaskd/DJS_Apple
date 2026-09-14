using UnityEngine;

namespace Module.Setting
{
	/// <summary>局内设置弹窗：音效/音乐/震动开关、语言、回主页、重开。</summary>
	public class GameSettingUI : BaseUI
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
		private GameObject m_HomeBtn;

		[SerializeField]
		private GameObject m_ReplayBtn;

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
			if (m_HomeBtn != null)
			{
				MCCIJBJGMCK.Get(m_HomeBtn).onClick = OnHomeClick;
			}
			if (m_ReplayBtn != null)
			{
				MCCIJBJGMCK.Get(m_ReplayBtn).onClick = OnReplayClick;
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
			MgrUI.Instance.Close("pops/setting/GameSettingUI", false);
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

		private void OnHomeClick(GameObject gameObject)
		{
			MgrUI.Instance.Close("pops/setting/GameSettingUI", false, false);
			MgrUI.Instance.Close("coreplay/CorePlayUI", false, false);
			EDLHEMMBABM.Instance.ReleaseAllItems();
			MgrUI.Instance.Open("home/Home", false);
		}

		private void OnReplayClick(GameObject gameObject)
		{
			MgrUI.Instance.Close("pops/setting/GameSettingUI", false, false);
			LevelConfig config = PLMIHDHFAAL.Instance.GetMainLevelConfig();
			if (config != null)
			{
				JEFOMCDAPGK.Instance.MainLevelData.Reset(true);
				Framework.Base.UtilModule.CoroutineManager.Instance.StartCor(EDLHEMMBABM.Instance.ParseConfig(config, true, null));
			}
		}
	}
}
