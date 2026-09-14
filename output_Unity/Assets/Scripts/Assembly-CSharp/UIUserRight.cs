using UnityEngine;
using UnityEngine.UI;

public class UIUserRight : BaseUI
{
	[SerializeField]
	private GameObject m_CloseBtn;

	[SerializeField]
	private InputField m_InputField;

	[SerializeField]
	private GameObject m_SubmitBtn;

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

	private void OnSubmitBtnClick(GameObject gameObject)
	{
	}

	private bool CheckInput(string input)
	{
		return false;
	}
}
