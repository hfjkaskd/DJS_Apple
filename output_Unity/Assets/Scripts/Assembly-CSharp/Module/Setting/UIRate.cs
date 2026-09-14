using System;
using UnityEngine;

namespace Module.Setting
{
	/// <summary>评分弹窗：关闭 / 1–4★ / 5★ 三个入口，关闭后继续结算队列。</summary>
	public class UIRate : BaseUI
	{
		[SerializeField]
		private GameObject m_CloseBtn;

		[SerializeField]
		private GameObject m_OtherStarBtn;

		[SerializeField]
		private GameObject m_FiveStarBtn;

		public static Action onCloseAction;

		public override PAIEAGDLCBJ Layer => PAIEAGDLCBJ.Top;

		protected override void Init()
		{
			if (m_CloseBtn != null)
			{
				MCCIJBJGMCK.Get(m_CloseBtn).onClick = OnCloseClick;
			}
			if (m_OtherStarBtn != null)
			{
				MCCIJBJGMCK.Get(m_OtherStarBtn).onClick = OnClickRate14Star;
			}
			if (m_FiveStarBtn != null)
			{
				MCCIJBJGMCK.Get(m_FiveStarBtn).onClick = OnClickRate5Star;
			}
		}

		protected override void BeforeOpen()
		{
		}

		private void OnClickRate5Star(GameObject gameObject)
		{
			BMNFNJFCPHG.Instance.SetHasRated(true);
			Application.OpenURL("https://play.google.com/store/apps");
			CloseUI();
		}

		private void OnClickRate14Star(GameObject gameObject)
		{
			BMNFNJFCPHG.Instance.SetHasRated(true);
			CloseUI();
		}

		private void OnCloseClick(GameObject gameObject)
		{
			CloseUI();
		}

		private void CloseUI()
		{
			MgrUI.Instance.Close("pops/rate/RateUI", true);
			Action next = onCloseAction;
			onCloseAction = null;
			next?.Invoke();
		}
	}
}
