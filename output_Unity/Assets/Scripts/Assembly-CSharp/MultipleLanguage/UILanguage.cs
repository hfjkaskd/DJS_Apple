using System.Collections.Generic;
using UnityEngine;

namespace MultipleLanguage
{
	public class UILanguage : BaseUI
	{
		[SerializeField]
		private GameObject m_CloseBtn;

		[SerializeField]
		private LanguageBtn m_LanguageTemplate;

		[SerializeField]
		private Transform m_Content;

		private List<LanguageBtn> languageList;

		private List<EDMAJFIKJLE> sortList;

		public override PAIEAGDLCBJ Layer => default(PAIEAGDLCBJ);

		protected override void Init()
		{
		}

		protected override void BeforeOpen()
		{
		}

		private void OnCloseClick(GameObject gameObject)
		{
		}

		private void InitAllButtons()
		{
		}

		private void OnBtnClick(GameObject obj)
		{
		}

		private void RefreshAllButtons()
		{
		}
	}
}
