using System;
using UnityEngine;

namespace Project.UI
{
	/// <summary>广告按钮状态：本工程无广告 SDK，始终视为有广告可看。</summary>
	public class AdButtonState : MonoBehaviour
	{
		[SerializeField]
		private GameObject hasAdGameObject;

		[SerializeField]
		private GameObject noneAdGameObject;

		private Action<GameObject> hasAdClick;

		public void InitClickAction(Action<GameObject> InHasADClick)
		{
			hasAdClick = InHasADClick;
			if (hasAdGameObject != null)
			{
				MCCIJBJGMCK.Get(hasAdGameObject).onClick = go =>
				{
					hasAdClick?.Invoke(go);
				};
			}
			if (noneAdGameObject != null)
			{
				MCCIJBJGMCK.Get(noneAdGameObject).onClick = OnNoADclick;
			}
			ChangeVideoBtn(true);
		}

		public void CancelClickAction()
		{
			hasAdClick = null;
		}

		private void OnEnable()
		{
			ChangeVideoBtn(true);
		}

		private void OnDisable()
		{
		}

		private void ChangeVideoBtn(bool hasAD)
		{
			if (hasAdGameObject != null)
			{
				hasAdGameObject.SetActive(hasAD);
			}
			if (noneAdGameObject != null)
			{
				noneAdGameObject.SetActive(!hasAD);
			}
		}

		public void OnNoADclick(GameObject gameObject)
		{
			MgrGlobalUI.Instance.ShowGlobalText(OJEEJGGLNPC.Instance.GetText("gameplay_undogray_tips"), FFMLGGBCOOO.neutral);
		}
	}
}
