using System.Globalization;

namespace WZSDK
{
	public enum ViewType
	{
		kaiju_ui_1 = 0,
		kaiju_ui_1_1 = 1,
		kaiju_ui_2 = 2,
		kaiju_ui_2_1 = 3,
		kaiju_ui_3 = 4,
		kaiju_ui_4 = 5,
		kaiju_ui_5 = 6,
		SuccessMessage = 7,
		WithDrawSucMessage = 8,
		WithDraw = 9,
		WithDrawMethod = 10,
		WithDrawQues = 11,
		WithDrawConfirm = 12,
		WithDrawProcess = 13,
		WithDrawProcess1 = 14,
		WithDrawKeepEarn = 15,
		WithDrawContinue = 16,
		WithDrawSuc = 17,
		WithDrawDateNow = 18,
		WithDrawHistory = 19,
		RewardMessageWith = 20,
		RewardMessageMoney = 21,
		RewardMessageDia = 22
	}

	internal static class ViewTypeContract
	{
		public static string GetStableName(ViewType value)
		{
			switch (value)
			{
				case ViewType.kaiju_ui_1: return "kaiju_ui_1";
				case ViewType.kaiju_ui_1_1: return "kaiju_ui_1_1";
				case ViewType.kaiju_ui_2: return "kaiju_ui_2";
				case ViewType.kaiju_ui_2_1: return "kaiju_ui_2_1";
				case ViewType.kaiju_ui_3: return "kaiju_ui_3";
				case ViewType.kaiju_ui_4: return "kaiju_ui_4";
				case ViewType.kaiju_ui_5: return "kaiju_ui_5";
				case ViewType.SuccessMessage: return "SuccessMessage";
				case ViewType.WithDrawSucMessage: return "WithDrawSucMessage";
				case ViewType.WithDraw: return "WithDraw";
				case ViewType.WithDrawMethod: return "WithDrawMethod";
				case ViewType.WithDrawQues: return "WithDrawQues";
				case ViewType.WithDrawConfirm: return "WithDrawConfirm";
				case ViewType.WithDrawProcess: return "WithDrawProcess";
				case ViewType.WithDrawProcess1: return "WithDrawProcess1";
				case ViewType.WithDrawKeepEarn: return "WithDrawKeepEarn";
				case ViewType.WithDrawContinue: return "WithDrawContinue";
				case ViewType.WithDrawSuc: return "WithDrawSuc";
				case ViewType.WithDrawDateNow: return "WithDrawDateNow";
				case ViewType.WithDrawHistory: return "WithDrawHistory";
				case ViewType.RewardMessageWith: return "RewardMessageWith";
				case ViewType.RewardMessageMoney: return "RewardMessageMoney";
				case ViewType.RewardMessageDia: return "RewardMessageDia";
				default:
					return "Unknown(" +
						((int)value).ToString(CultureInfo.InvariantCulture) + ")";
			}
		}
	}
}
