using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>验证 Plugins 里的 UnityEngine.UI.dll 是否是可正常渲染的完整实现。</summary>
public static class UIRenderCheck
{
	public static void Run()
	{
		Assembly uiAsm = typeof(Image).Assembly;
		Debug.Log($"[UIRenderCheck] UnityEngine.UI 程序集位置: {uiAsm.Location}");

		// 离屏渲染一个红色 Image，读回像素判断是否绘制成功
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

		var imgGo = new GameObject("img");
		imgGo.transform.SetParent(canvasGo.transform, false);
		Image img = imgGo.AddComponent<Image>();
		img.color = Color.red;
		RectTransform rt = img.rectTransform;
		rt.anchorMin = Vector2.zero;
		rt.anchorMax = Vector2.one;
		rt.sizeDelta = Vector2.zero;

		RenderTexture rtex = new RenderTexture(64, 64, 24);
		cam.targetTexture = rtex;
		Canvas.ForceUpdateCanvases();
		Debug.Log($"[UIRenderCheck] image canvasRenderer: cull={img.canvasRenderer.cull} materialCount={img.canvasRenderer.materialCount} absoluteDepth={img.canvasRenderer.absoluteDepth}");
		cam.Render();

		RenderTexture.active = rtex;
		Texture2D tex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
		tex.ReadPixels(new Rect(0, 0, 64, 64), 0, 0);
		tex.Apply();
		Color center = tex.GetPixel(32, 32);
		Debug.Log($"[UIRenderCheck] 中心像素 = {center} (期望红色)");

		RenderTexture.active = null;
		cam.targetTexture = null;
		Object.DestroyImmediate(camGo);
		Object.DestroyImmediate(canvasGo);
		Object.DestroyImmediate(rtex);
		Object.DestroyImmediate(tex);

		Debug.Log(center.r > 0.5f && center.g < 0.2f
			? "[UIRenderCheck] 结果: UI渲染正常"
			: "[UIRenderCheck] 结果: UI渲染失败!");
	}
}
