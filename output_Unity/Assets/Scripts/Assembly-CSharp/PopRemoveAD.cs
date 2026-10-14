using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PopRemoveAD : BaseUI
{
	[SerializeField]
	private GameObject m_CloseBtn;

	[SerializeField]
	private List<TextMeshProUGUI> m_Prices;

	[SerializeField]
	private GameObject m_Buybtn;

	[SerializeField]
	private GameObject m_RestoreBtn;

	public override PAIEAGDLCBJ Layer => default(PAIEAGDLCBJ);

	protected override void Init()
	{
		if (m_CloseBtn != null)
		{
			MCCIJBJGMCK.Get(m_CloseBtn).onClick = Close;
		}
	}

	protected override void BeforeOpen()
	{
		if (m_Buybtn != null) m_Buybtn.SetActive(false);
		if (m_RestoreBtn != null) m_RestoreBtn.SetActive(false);
	}

	protected override void BeforeClose()
	{
	}

	private void OnBtnBuyClick(GameObject gameObject)
	{
	}

	private void OnBtnRestoreClick(GameObject gameObject)
	{
	}

	private void OnBecomeNoAd()
	{
	}

	private void Close(GameObject gameObject)
	{
		MgrUI.Instance.Close("pops/removead/RemoveADUI", false);
	}
}
