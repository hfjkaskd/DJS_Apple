using UnityEngine;

public class IAPUnknowFailedUI : BaseUI
{
	[SerializeField]
	private GameObject m_CloseBtn;

	[SerializeField]
	private GameObject m_ContinueBtn;

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
}
