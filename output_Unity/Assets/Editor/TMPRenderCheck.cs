using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>验证 TMP 文本是否能用项目字体资产正常渲染。</summary>
public static class TMPRenderCheck
{
	public static void Run()
	{
		TMP_Settings settings = Resources.Load<TMP_Settings>("TMP Settings");
		Debug.Log($"[TMPRenderCheck] TMP Settings: {(settings != null ? "已加载" : "缺失")}");

		bool useDefaultFont = System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-tmpdefault") >= 0;
		bool useSpriteShader = System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-spriteshader") >= 0;
		string fontPath = useDefaultFont
			? "Packages/com.unity.textmeshpro/Package Resources/Fonts & Materials/LiberationSans SDF.asset"
			: "Assets/res/local/textmeshpro/resources/fonts & materials/802-CAI978 SDF.asset";
		TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
		if (useDefaultFont)
		{
			Debug.Log($"[TMPRenderCheck] 使用TMP默认字体测试: {(font != null ? font.name : "null")}");
		}
		Debug.Log($"[TMPRenderCheck] 字体资产: {(font != null ? font.name : "null")} material={(font != null && font.material != null ? font.material.shader.name : "null")} atlas={(font != null && font.atlasTexture != null ? font.atlasTexture.name : "null")}");
		if (font != null)
		{
			Debug.Log($"[TMPRenderCheck] 字符表={font.characterTable.Count} 字形表={font.glyphTable.Count} pointSize={font.faceInfo.pointSize} scale={font.faceInfo.scale} atlasSize={font.atlasWidth}x{font.atlasHeight} version={font.version}");
			bool has8 = font.HasCharacter('8');
			Debug.Log($"[TMPRenderCheck] 是否含字符'8': {has8}");
		}

		var camGo = new GameObject("cam");
		Camera cam = camGo.AddComponent<Camera>();
		cam.clearFlags = CameraClearFlags.SolidColor;
		cam.backgroundColor = Color.black;
		cam.orthographic = true;

		var canvasGo = new GameObject("canvas");
		Canvas canvas = canvasGo.AddComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceCamera;
		canvas.worldCamera = cam;
		canvasGo.AddComponent<CanvasScaler>();

		var txtGo = new GameObject("txt", typeof(RectTransform));
		txtGo.transform.SetParent(canvasGo.transform, false);
		TextMeshProUGUI txt = txtGo.AddComponent<TextMeshProUGUI>();
		if (font != null)
		{
			txt.font = font;
		}
		txt.text = "8888";
		txt.fontSize = 100;
		txt.color = Color.white;
		txt.alignment = TextAlignmentOptions.Center;
		RectTransform rt = txt.rectTransform;
		rt.anchorMin = Vector2.zero;
		rt.anchorMax = Vector2.one;
		rt.sizeDelta = Vector2.zero;

		if (useSpriteShader)
		{
			Shader sprShader = Shader.Find("UI/Default");
			Debug.Log($"[TMPRenderCheck] 改用UI/Default着色器: {(sprShader != null)}");
			txt.fontMaterial.shader = sprShader;
		}
		RenderTexture rtex = new RenderTexture(128, 128, 24);
		cam.targetTexture = rtex;
		txt.ForceMeshUpdate(true, true);
		Canvas.ForceUpdateCanvases();
		Debug.Log($"[TMPRenderCheck] canvasRenderer: cull={txt.canvasRenderer.cull} hasMoved={txt.canvasRenderer.hasMoved} materialCount={txt.canvasRenderer.materialCount} absoluteDepth={txt.canvasRenderer.absoluteDepth}");
		Debug.Log($"[TMPRenderCheck] mesh bounds={txt.mesh.bounds} textBounds={txt.bounds}");
		txt.canvasRenderer.SetMesh(txt.mesh);
		txt.canvasRenderer.SetMaterial(txt.fontMaterial, 0);
		Debug.Log($"[TMPRenderCheck] mesh顶点数={txt.mesh.vertexCount} shaderSupported={(txt.fontMaterial != null ? txt.fontMaterial.shader.isSupported.ToString() : "null")} renderedText長={txt.textInfo.characterCount}");
		Material mat = txt.fontMaterial;
		Debug.Log($"[TMPRenderCheck] 材质 _MainTex={(mat.HasProperty("_MainTex") && mat.GetTexture("_MainTex") != null ? mat.GetTexture("_MainTex").name : "null")} _FaceColor={(mat.HasProperty("_FaceColor") ? mat.GetColor("_FaceColor").ToString() : "n/a")} _GradientScale={(mat.HasProperty("_GradientScale") ? mat.GetFloat("_GradientScale").ToString() : "n/a")}");
		if (font != null)
		{
			var imp = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(font.atlasTexture)) as TextureImporter;
			if (imp != null)
			{
				Debug.Log($"[TMPRenderCheck] 图集导入: format={imp.textureCompression} type={imp.textureType} alphaSource={imp.alphaSource} sRGB={imp.sRGBTexture}");
			}
			Texture2D atlas2D = font.atlasTexture as Texture2D;
			if (atlas2D != null)
			{
				Debug.Log($"[TMPRenderCheck] 图集纹理: format={atlas2D.format}");
			}
		}
		cam.Render();

		RenderTexture.active = rtex;
		Texture2D tex = new Texture2D(128, 128, TextureFormat.RGBA32, false);
		tex.ReadPixels(new Rect(0, 0, 128, 128), 0, 0);
		tex.Apply();
		float maxLum = 0f;
		int lit = 0;
		for (int y = 0; y < 128; y++)
		{
			for (int x = 0; x < 128; x++)
			{
				Color c = tex.GetPixel(x, y);
				float l = c.r + c.g + c.b;
				if (l > 0.2f)
				{
					lit++;
				}
				maxLum = Mathf.Max(maxLum, l);
			}
		}
		Debug.Log($"[TMPRenderCheck] 全图最大亮度={maxLum} 亮像素数={lit}");
		Debug.Log(maxLum > 0.5f ? "[TMPRenderCheck] 结果: TMP渲染正常" : "[TMPRenderCheck] 结果: TMP渲染失败!");

		RenderTexture.active = null;
		cam.targetTexture = null;
		Object.DestroyImmediate(camGo);
		Object.DestroyImmediate(canvasGo);
		Object.DestroyImmediate(rtex);
		Object.DestroyImmediate(tex);
	}
}
