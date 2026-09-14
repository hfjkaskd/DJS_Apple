using UnityEngine;

public class UIPrivacyPolicy : BaseUI
{
	[SerializeField]
	private GameObject m_CloseBtn;

	[SerializeField]
	private GameObject m_PrivacyBtn;

	[SerializeField]
	private GameObject m_EuropeanBtn;

	public override PAIEAGDLCBJ Layer => default(PAIEAGDLCBJ);

	protected override void Init()
	{
	}

	protected override void BeforeOpen()
	{
	}

	private void OnCloseClick(GameObject InGameObject)
	{
	}

	private void OnPrivacyBtnClick(GameObject InGameObject)
	{
	}

	private void OnEuropeanBtnClick(GameObject InGameObject)
	{
	}
}
