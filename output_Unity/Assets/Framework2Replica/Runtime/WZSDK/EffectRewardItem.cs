using ArrowOut.Runtime;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace WZSDK
{
	public class EffectRewardItem : MonoBehaviour
	{
		public Image Icon;

		public Text Desc;

		[HideInInspector]
		public ERewardType ErType;

		[HideInInspector]
		public float Count;

		private int _showVersion;

		public async void Show(ERewardType erType, float count)
		{
			int showVersion = ++_showVersion;
			ErType = erType;
			Count = count;
			if (Desc != null)
			{
				if ((erType == ERewardType.Money ||
					 erType == ERewardType.LuckyWalletMoney) &&
					FacadePayTypeExtend.RegionalChangeHandle != null)
				{
					Desc.text =
						FacadePayTypeExtend.RegionalChangeHandle(count);
				}
				else
				{
					Desc.text = count.ToString();
				}
			}

			Sprite sprite = null;
			switch (erType)
			{
				case ERewardType.Money:
					if (GameDefines.ifIAA)
					{
						sprite = await LoadSpriteAsync(
							GameDefines.ERIAAMoneyIconPath,
							"UI/LuckySpinIcons/icon_qianbi_IAA");
					}
					else if (FacadePayTypeExtend.GetMultCoinCoinAct != null)
					{
						sprite = FacadePayTypeExtend.GetMultCoinCoinAct();
					}
					break;
				case ERewardType.Gem:
					if (FacadePayTypeExtend.GetMulteGem != null)
					{
						sprite = FacadePayTypeExtend.GetMulteGem();
					}
					if (sprite == null)
					{
						sprite = await LoadSpriteAsync(null, "UI/Money/gem");
					}
					break;
				case ERewardType.AddBottle:
					sprite = await LoadSpriteAsync(
						GameDefines.ERAddSpaceIconPath,
						"UI/FuncIcon/Bottle");
					break;
				case ERewardType.Clear:
					sprite = await LoadSpriteAsync(
						GameDefines.ERClearIconPath,
						"UI/FuncIcon/icon_mofab");
					break;
				case ERewardType.Undo:
					sprite = await LoadSpriteAsync(
						GameDefines.ERHammerIconPath,
						"UI/FuncIcon/icon_shuaxin");
					break;
				case ERewardType.Prompt:
					sprite = await LoadSpriteAsync(
						GameDefines.ERAddPromptIconPath,
						"UI/FuncIcon/icon_mofab");
					break;
				case ERewardType.Guid:
					sprite = await LoadSpriteAsync(
						GameDefines.ERAddGuidIconPath,
						"UI/FuncIcon/icon_shuaxin");
					break;
				case ERewardType.LuckyWalletMoney:
					sprite = await LoadSpriteAsync(
						GameDefines.LSLuckyWalletIconPath,
						"UI/LuckySpinIcons/icon_qianbi_IAA");
					break;
			}

			if (this == null || showVersion != _showVersion)
			{
				return;
			}
			if (Icon != null && sprite != null)
			{
				Icon.sprite = sprite;
			}
		}

		private static async Task<Sprite> LoadSpriteAsync(
			string configuredAddress, string fallbackAddress)
		{
			Sprite sprite = null;
			if (!string.IsNullOrEmpty(configuredAddress))
			{
				sprite = await AddressableAssetService.LoadAsync<Sprite>(
					configuredAddress);
			}
			return sprite != null
				? sprite
				: await AddressableAssetService.LoadAsync<Sprite>(fallbackAddress);
		}
	}
}
