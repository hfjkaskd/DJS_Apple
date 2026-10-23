using UnityEngine.UI;

namespace WZSDK
{
	public class BloatView : BaseView
	{
		private Button clickBtn;
		internal Button HostContinueButton => clickBtn;

		private Button close;

		private LuckySpinOpenData luckySpinOpenData;

		private bool isInitialized;

		private bool hasClicked;

		protected override void HandleViewArgs(object[] args)
		{
			luckySpinOpenData = null;
			if (args == null)
			{
				return;
			}
			for (int i = 0; i < args.Length; i++)
			{
				LuckySpinOpenData openData = args[i] as LuckySpinOpenData;
				if (openData != null)
				{
					luckySpinOpenData = openData;
					return;
				}
			}
		}

		public override void InitView()
		{
			InitializeIfNeeded();
			hasClicked = false;
		}

		public override void Start()
		{
			InitializeIfNeeded();
		}

		public override void Update()
		{
		}

		private void InitializeIfNeeded()
		{
			if (isInitialized)
			{
				return;
			}
			clickBtn = FindChildUtility.FindChild<Button>(
				gameObject, "ClickBtn");
			close = FindChildUtility.FindChild<Button>(
				gameObject, "close");
			if (clickBtn != null)
			{
				clickBtn.onClick.AddListener(OnClick);
			}
			if (close != null)
			{
				close.onClick.AddListener(OnClick);
			}
			isInitialized = true;
		}

		private void OnClick()
		{
			if (hasClicked || UIManager.instance == null)
			{
				return;
			}
			hasClicked = true;
			MahjongProductAnalytics.Track("f2_compensation_intro_continue", "next_page", "bonus_wheel", "amount", luckySpinOpenData != null ? luckySpinOpenData.ForcedMoneyAmount : 0f, "mandatory", luckySpinOpenData != null && luckySpinOpenData.IsMandatory);
			UIManager.instance.CloseView(EUIType.BloatView);
			if (luckySpinOpenData == null)
			{
				UIManager.instance.ShowView(EUIType.BonusWheelView);
				return;
			}
			UIManager.instance.ShowView(
				EUIType.BonusWheelView, luckySpinOpenData);
		}
	}
}
