using UnityEngine;
using UnityEngine.UI;

public class UIFeedBack : BaseUI
{
	[SerializeField]
	private GameObject m_CloseBtn;

	[SerializeField]
	private GameObject m_SubmitBtn;

	[SerializeField]
	private InputField m_InputField;

	public override PAIEAGDLCBJ Layer => default(PAIEAGDLCBJ);

	protected override void Init()
	{
	}

	protected override void BeforeOpen()
	{
	}

	public void OnCloseClick(GameObject gameObject)
	{
	}

	private void OnSubmitClick(GameObject gameObject)
	{
	}

	private bool CheckInput(string input)
	{
		return false;
	}
}
