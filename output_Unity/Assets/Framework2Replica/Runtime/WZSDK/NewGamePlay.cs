using ArrowOut.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	public class NewGamePlay : BaseView
	{
		public Image index;

		public Text dec;

		public Button Continue;

		private bool _isEventsBound;

		private int _contentLoadVersion;

		public override void InitView()
		{
		}

		public override void Start()
		{
			BindEvents();
		}

		private void BindEvents()
		{
			if (_isEventsBound || Continue == null)
			{
				return;
			}

			Continue.onClick.AddListener(OnContinueClick);
			_isEventsBound = true;
		}

		private void RemoveEvents()
		{
			if (Continue == null || Continue.onClick == null)
			{
				return;
			}

			Continue.onClick.RemoveAllListeners();
			_isEventsBound = false;
		}

		private void OnContinueClick()
		{
			SoundManager.Instance?.PlayUIClickSFX();
			UIManager.instance?.CloseView(EUIType.NewGamePlay);

			GameManagerWZ manager = GameManagerWZ.instance;
			if (manager != null && manager.levelGen != null)
			{
				manager.levelGen.ExecuteStageSettings();
			}
		}

		protected override void HandleViewArgs(object[] args)
		{
			if (args != null && args.Length > 0 && args[0] is int type)
			{
				SetContent(type);
			}
		}

		private async void SetContent(int type)
		{
			int loadVersion = ++_contentLoadVersion;
			string spritePath = string.Empty;
			string localizationKey = string.Empty;
			switch (type)
			{
				case 1:
					spritePath = "Game/New GamePlay/index1";
					localizationKey = "1023";
					break;
				case 2:
					spritePath = "Game/New GamePlay/index2";
					localizationKey = "1024";
					break;
				case 3:
					spritePath = "Game/New GamePlay/index3";
					localizationKey = "1025";
					break;
				case 4:
					spritePath = "Game/New GamePlay/index4";
					localizationKey = "1026";
					break;
			}

			if (!string.IsNullOrEmpty(localizationKey) &&
				dec != null &&
				LocalizationManager.Instance != null)
			{
				dec.text = LocalizationManager.Instance.GetText(localizationKey);
			}

			if (!string.IsNullOrEmpty(spritePath) && index != null)
			{
				Sprite sprite =
					await AddressableAssetService.LoadOptionalAsync<Sprite>(spritePath);
				if (this == null || loadVersion != _contentLoadVersion || index == null)
				{
					return;
				}
				if (sprite != null)
				{
					index.sprite = sprite;
					index.SetNativeSize();
				}
			}
		}

		private void OnDestroy()
		{
			_contentLoadVersion++;
			if (_isEventsBound)
			{
				RemoveEvents();
			}
		}

		public override void Update()
		{
		}
	}
}
