using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelTipUI : BaseUI
{
	public static int ContentType;

	public static int TextIndex;

	public static int Percentage;

	[SerializeField]
	private List<TextMeshProCustom> m_TipTexts;

	private static readonly string[] TextKeyPrefixes;

	public override PAIEAGDLCBJ Layer => default(PAIEAGDLCBJ);

	private string GetTextKey()
	{
		return null;
	}

	protected override void Init()
	{
	}

	protected override void BeforeOpen()
	{
	}
}
