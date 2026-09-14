using TMPro;
using UnityEngine;

[ExecuteAlways]
public class TextSizeControl : MonoBehaviour
{
	[SerializeField]
	private float m_Spacing;

	[SerializeField]
	private float m_MaxWidth;

	[SerializeField]
	private RectTransform m_IconRect;

	[SerializeField]
	private RectTransform m_TextRect;

	[SerializeField]
	private TextMeshProUGUI m_TextComp;

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
	}

	private void Start()
	{
	}

	private void InitAnchors()
	{
	}

	private void OnTextChanged(Object obj)
	{
	}

	public void UpdateLayout()
	{
	}
}
