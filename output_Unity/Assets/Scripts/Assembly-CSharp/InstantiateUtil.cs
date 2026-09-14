using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class InstantiateUtil : MonoBehaviour
{
	private static readonly List<Material> listMat;

	private static readonly List<Graphic> sGrps;

	private static readonly List<Renderer> renderers;

	[Conditional("UNITY_EDITOR")]
	public static void ResetGameObjectMaterial(GameObject go)
	{
	}
}
