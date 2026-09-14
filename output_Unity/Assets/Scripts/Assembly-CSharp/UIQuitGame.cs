using UnityEngine;

public class UIQuitGame : BaseUI
{
	[SerializeField]
	private GameObject m_CloseBtn;

	[SerializeField]
	private GameObject m_QuitBtn;

	public override PAIEAGDLCBJ Layer => default(PAIEAGDLCBJ);

	protected override void Init()
	{
	}

	protected override void BeforeOpen()
	{
	}

	private void OnCloseBtnClick(GameObject gameObject)
	{
	}

	private void OnQuitBtnClick(GameObject gameObject)
	{
	}
}
