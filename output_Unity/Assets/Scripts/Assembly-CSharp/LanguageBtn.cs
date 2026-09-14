using TMPro;
using UnityEngine;

public class LanguageBtn : MonoBehaviour
{
	[SerializeField]
	private GameObject m_Off;

	[SerializeField]
	private GameObject m_On;

	[SerializeField]
	private TextMeshProUGUI m_OffText;

	[SerializeField]
	private TextMeshProUGUI m_OnText;

	public void Init(string InLanguage)
	{
	}

	public void RefreshBtnState(bool InSelected)
	{
	}
}
