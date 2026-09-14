using UnityEngine;

namespace CorePlay
{
	public class NotificationsAuthItem : MonoBehaviour
	{
		[SerializeField]
		private GameObject m_CloseBtn;

		[SerializeField]
		private GameObject m_ExitWithTurnOnBtn;

		[SerializeField]
		private GameObject m_ExitWithoutTurnBtn;

		private NotificationsAuthUI popUI;

		public void InitContent(NotificationsAuthUI ui)
		{
		}

		private void BindEvent()
		{
		}

		private void OnCloseBtnClick(GameObject go)
		{
		}

		private void OnExitWithoutTurnBtnClick(GameObject go)
		{
		}

		private void OnExitWithTurnOnBtnOk(GameObject go)
		{
		}

		private void Quit()
		{
		}

		private void CloseUI()
		{
		}
	}
}
