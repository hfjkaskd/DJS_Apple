using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>
/// Renders existing scene/game/UI instances to a portrait target in batch Editor.
/// No prefab, artwork, text, or UI hierarchy is replaced. This is an instance render,
/// not a hardware screenshot or viewport-parity proof. Safe-area adapters are paused
/// for this no-notch diagnostic because Screen.safeArea belongs to the real viewport,
/// not the temporary render target. All temporary changes are restored.
/// </summary>
public static class Framework2ActualCapture
{
    private sealed class CanvasState
    {
        public Canvas canvas;
        public RenderMode mode;
        public Camera camera;
        public float planeDistance;
        public CanvasScaler scaler;
        public bool scalerEnabled;
    }

    public static bool TryCapturePortrait(string absolutePath, out string error,
        Action beforeRender = null, Action afterRestore = null, int width = 1080, int height = 1920)
    {
        error = null;
        Camera renderCamera = null;
        RenderTexture target = null;
        Texture2D pixels = null;
        RenderTexture previousActive = RenderTexture.active;
        var states = new List<CanvasState>();
        var enabledSafeAreas = new List<SafeAreaAdapter>();
        bool projectionChanged = false;
        try
        {
            if (width <= 0 || height <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            Camera source = Camera.main;
            if (source == null || !source.isActiveAndEnabled)
                foreach (Camera candidate in Object.FindObjectsOfType<Camera>())
                    if (candidate.isActiveAndEnabled) { source = candidate; break; }
            if (source == null) throw new InvalidOperationException("No scene camera is available for the actual-instance capture.");
            renderCamera = new GameObject("Framework2 transient portrait capture", typeof(Camera)).GetComponent<Camera>();
            renderCamera.CopyFrom(source);
            renderCamera.enabled = false;
            renderCamera.transform.SetPositionAndRotation(source.transform.position, source.transform.rotation);
            renderCamera.transform.localScale = source.transform.lossyScale;
            renderCamera.rect = new Rect(0, 0, 1, 1);
            renderCamera.aspect = (float)width / height;
            renderCamera.targetDisplay = 0;
            target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            target.name = "Framework2 portrait validation target";
            target.Create();
            renderCamera.targetTexture = target;

            // Global Screen.safeArea cannot describe a differently sized offscreen
            // target. Pause the real adapters before any canvas dimension callbacks;
            // their original prefab offsets/layout remain in the rendered instances.
            foreach (SafeAreaAdapter adapter in Object.FindObjectsOfType<SafeAreaAdapter>())
                if (adapter.enabled) { enabledSafeAreas.Add(adapter); adapter.enabled = false; }

            foreach (Canvas canvas in Object.FindObjectsOfType<Canvas>())
            {
                if (!canvas.isActiveAndEnabled || !canvas.isRootCanvas || canvas.renderMode == RenderMode.WorldSpace) continue;
                var state = new CanvasState
                {
                    canvas = canvas, mode = canvas.renderMode, camera = canvas.worldCamera,
                    planeDistance = canvas.planeDistance, scaler = canvas.GetComponent<CanvasScaler>()
                };
                state.scalerEnabled = state.scaler != null && state.scaler.enabled;
                states.Add(state);
                projectionChanged = true;
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = renderCamera;
                canvas.planeDistance = Mathf.Clamp(1f, renderCamera.nearClipPlane + .01f, renderCamera.farClipPlane - .01f);
                foreach (Graphic graphic in canvas.GetComponentsInChildren<Graphic>())
                    renderCamera.cullingMask |= 1 << graphic.gameObject.layer;
                RefreshScaler(state);
            }
            Canvas.ForceUpdateCanvases();
            beforeRender?.Invoke();
            Canvas.ForceUpdateCanvases();
            renderCamera.Render();
            RenderTexture.active = target;
            pixels = new Texture2D(width, height, TextureFormat.RGBA32, false);
            pixels.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
            pixels.Apply(false, false);
            string directory = Path.GetDirectoryName(Path.GetFullPath(absolutePath));
            Directory.CreateDirectory(directory);
            File.WriteAllBytes(absolutePath, pixels.EncodeToPNG());
        }
        catch (Exception exception) { error = exception.ToString(); }
        finally
        {
            RenderTexture.active = previousActive;
            try
            {
                foreach (CanvasState state in states)
                {
                    if (state.canvas == null) continue;
                    state.canvas.renderMode = state.mode;
                    state.canvas.worldCamera = state.camera;
                    state.canvas.planeDistance = state.planeDistance;
                    RefreshScaler(state);
                }
                foreach (SafeAreaAdapter adapter in enabledSafeAreas)
                    if (adapter != null) adapter.enabled = true;
                if (projectionChanged)
                {
                    Canvas.ForceUpdateCanvases();
                    afterRestore?.Invoke();
                    Canvas.ForceUpdateCanvases();
                }
            }
            catch (Exception restore) { error = (error ?? string.Empty) + "\nPortrait capture restoration failed: " + restore; }
            if (renderCamera != null)
            {
                renderCamera.targetTexture = null;
                Object.DestroyImmediate(renderCamera.gameObject);
            }
            if (pixels != null) Object.DestroyImmediate(pixels);
            if (target != null) { target.Release(); Object.DestroyImmediate(target); }
        }
        return error == null;
    }

    private static void RefreshScaler(CanvasState state)
    {
        if (state.scaler == null || !state.scalerEnabled) return;
        // CanvasScaler's public enable lifecycle recomputes scale against the current
        // render target. No reflection, private Editor GameView API or copied layout.
        state.scaler.enabled = false;
        state.scaler.enabled = true;
    }
}
