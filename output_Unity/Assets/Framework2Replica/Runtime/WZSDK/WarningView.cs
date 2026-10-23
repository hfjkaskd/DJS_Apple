using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	public class WarningView : BaseView
	{
		public TextMeshProUGUI warningTxt;

		[SerializeField]
		private Button cancelButton;

		[SerializeField]
		private Button closeButton;

		public override void InitView()
		{
			BindCloseButtons();
		}

		public override void Start()
		{
			BindCloseButtons();
		}

		public override void Update()
		{
		}

		public override void ShowView()
		{
			base.ShowView();
		}

		public override void HideView()
		{
			base.HideView();
			SoundManager.Instance?.PlayUIClickSFX();
		}

		private void OnDestroy()
		{
			UnbindButton(cancelButton);
			UnbindButton(closeButton);
		}

		private void BindCloseButtons()
		{
			BindButton(cancelButton);
			BindButton(closeButton);
		}

		private void BindButton(Button button)
		{
			if (button == null)
			{
				return;
			}
			button.onClick.RemoveListener(HideView);
			button.onClick.AddListener(HideView);
		}

		private void UnbindButton(Button button)
		{
			if (button != null)
			{
				button.onClick.RemoveListener(HideView);
			}
		}
	}
}
