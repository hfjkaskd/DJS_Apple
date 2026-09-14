using UnityEngine;

/// <summary>商店条目动画基类。</summary>
public class ShopItem : MonoBehaviour
{
	[SerializeField]
	private Animation m_Animation;

	private const string ITEM_IN_ANI = "anim_ShopTransform_in";

	private const string ITEM_INITIAL_STATE = "anim_ShopTransform_initial";

	private const string ITEM_SHOW_MORE_ANI = "anim_ShopTransform_spread";

	public void PlayInAni()
	{
		PlayByName(ITEM_IN_ANI);
	}

	public void PlayInitialAni()
	{
		PlayByName(ITEM_INITIAL_STATE);
	}

	public void PlayShowMoreAni()
	{
		PlayByName(ITEM_SHOW_MORE_ANI);
	}

	private void PlayByName(string aniName)
	{
		if (m_Animation == null)
		{
			m_Animation = GetComponent<Animation>();
		}
		if (m_Animation != null && m_Animation[aniName] != null)
		{
			m_Animation.Play(aniName);
		}
	}
}
