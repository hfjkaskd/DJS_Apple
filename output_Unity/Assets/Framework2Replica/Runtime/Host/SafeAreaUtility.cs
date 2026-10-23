using UnityEngine;

/// <summary>One window-relative safe rectangle for gameplay and every UI canvas.</summary>
public static class SafeAreaUtility
{
    static Rect lastReportedArea;
    static Rect currentArea;
    static Vector2Int lastScreenSize;
    static ScreenOrientation lastOrientation;
    static float nextRefresh;
    static bool initialized;
    static bool nativeWarningLogged;

    public static Rect Current
    {
        get
        {
            var size = new Vector2Int(Screen.width, Screen.height);
            Rect reported = Screen.safeArea;
            ScreenOrientation orientation = Screen.orientation;
            float now = Time.realtimeSinceStartup;
            if (!initialized || size != lastScreenSize || reported != lastReportedArea ||
                orientation != lastOrientation || now >= nextRefresh || now + 0.5f < nextRefresh)
            {
                initialized = true;
                lastScreenSize = size;
                lastReportedArea = reported;
                lastOrientation = orientation;
                // Cutout arrays and native JNI calls are shared and throttled, not queried by every UI every frame.
                nextRefresh = now + 0.5f;
                currentArea = Calculate(reported, Screen.cutouts, size.x, size.y);
                if (Application.platform == RuntimePlatform.Android)
                    currentArea = Intersect(currentArea, ReadAndroidSafeArea(size.x, size.y));
            }
            return currentArea;
        }
    }

    public static Rect Calculate(Rect reported, Rect[] cutouts, int width, int height)
    {
        if (width <= 0 || height <= 0) return Rect.zero;
        Rect screen = new Rect(0f, 0f, width, height);
        Rect safe = reported.width > 0f && reported.height > 0f ? Intersect(screen, reported) : screen;
        for (int i = 0; cutouts != null && i < cutouts.Length; i++)
        {
            Rect hole = Intersect(screen, cutouts[i]);
            if (hole.width <= 0f || hole.height <= 0f || !safe.Overlaps(hole)) continue;

            // Floating punch holes need not touch an edge. Exclude the strip that loses the least screen area.
            float left = hole.xMax / width;
            float right = (width - hole.xMin) / width;
            float bottom = hole.yMax / height;
            float top = (height - hole.yMin) / height;
            float least = Mathf.Min(Mathf.Min(left, right), Mathf.Min(bottom, top));
            if (least >= 1f) continue; // Ignore a transient, invalid full-window cutout.
            if (top == least) safe.yMax = Mathf.Min(safe.yMax, hole.yMin);
            else if (bottom == least) safe.yMin = Mathf.Max(safe.yMin, hole.yMax);
            else if (left == least) safe.xMin = Mathf.Max(safe.xMin, hole.xMax);
            else safe.xMax = Mathf.Min(safe.xMax, hole.xMin);
        }
        return safe;
    }

    public static bool TryGetLocalRect(RectTransform parent, Canvas canvas, Rect safe, out Rect local)
    {
        local = Rect.zero;
        if (parent == null || parent.rect.width <= 0f || parent.rect.height <= 0f) return false;
        Canvas root = canvas != null ? canvas.rootCanvas : null;
        Camera camera = root != null && root.renderMode != RenderMode.ScreenSpaceOverlay ? root.worldCamera : null;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, safe.min, camera, out Vector2 min) ||
            !RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, safe.max, camera, out Vector2 max)) return false;
        // A parent already inside the safe area consumes that inset; nested children must not add it again.
        local = Intersect(parent.rect, Rect.MinMaxRect(min.x, min.y, max.x, max.y));
        return true;
    }

    public static Rect Intersect(Rect first, Rect second)
    {
        // Keep a disjoint result on the parent edge as well: animation can temporarily move a panel offscreen.
        float xMin = Mathf.Clamp(second.xMin, first.xMin, first.xMax);
        float yMin = Mathf.Clamp(second.yMin, first.yMin, first.yMax);
        return Rect.MinMaxRect(xMin, yMin, Mathf.Clamp(second.xMax, xMin, first.xMax),
            Mathf.Clamp(second.yMax, yMin, first.yMax));
    }

    static Rect ReadAndroidSafeArea(int width, int height)
    {
        Rect screen = new Rect(0f, 0f, width, height);
        if (width <= 0 || height <= 0) return screen;
        try
        {
            using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                if (activity == null) return screen;
                using (var window = activity.Call<AndroidJavaObject>("getWindow"))
                using (var decor = window.Call<AndroidJavaObject>("getDecorView"))
                using (var insets = decor.Call<AndroidJavaObject>("getRootWindowInsets"))
                {
                    if (insets == null) return screen;
                    int windowWidth = decor.Call<int>("getWidth");
                    int windowHeight = decor.Call<int>("getHeight");
                    if (windowWidth <= 0 || windowHeight <= 0) return screen;
                    int left = insets.Call<int>("getSystemWindowInsetLeft");
                    int right = insets.Call<int>("getSystemWindowInsetRight");
                    int top = insets.Call<int>("getSystemWindowInsetTop");
                    // The IME is not a device safe-area inset. Do not move the entire game above its keyboard.
                    int bottom = Mathf.Min(insets.Call<int>("getSystemWindowInsetBottom"),
                        insets.Call<int>("getStableInsetBottom"));
                    if (height >= width)
                    {
                        // Some Android firmware reports neither a cutout nor a Unity safe inset.
                        // Reserve its system-defined status-bar region even while the bar is hidden.
                        top = Mathf.Max(top, insets.Call<int>("getStableInsetTop"));
                        if (top == 0)
                        {
                            using (var resources = activity.Call<AndroidJavaObject>("getResources"))
                            {
                                int id = resources.Call<int>("getIdentifier", "status_bar_height", "dimen", "android");
                                if (id != 0) top = resources.Call<int>("getDimensionPixelSize", id);
                            }
                        }
                    }
                    using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
                    {
                        int api = version.GetStatic<int>("SDK_INT");
                        if (api >= 30)
                        {
                            using (var types = new AndroidJavaClass("android.view.WindowInsets$Type"))
                            {
                                int barsAndCutout = types.CallStatic<int>("systemBars") | types.CallStatic<int>("displayCutout");
                                using (var bars = insets.Call<AndroidJavaObject>("getInsets", barsAndCutout))
                                {
                                    left = bars.Get<int>("left");
                                    right = bars.Get<int>("right");
                                    top = Mathf.Max(top, bars.Get<int>("top"));
                                    bottom = bars.Get<int>("bottom");
                                }
                                if (height >= width)
                                {
                                    using (var status = insets.Call<AndroidJavaObject>("getInsetsIgnoringVisibility",
                                        types.CallStatic<int>("statusBars")))
                                        top = Mathf.Max(top, status.Get<int>("top"));
                                }
                            }
                        }
                        else if (api >= 28)
                        {
                            using (var cutout = insets.Call<AndroidJavaObject>("getDisplayCutout"))
                            {
                                if (cutout != null)
                                {
                                    left = Mathf.Max(left, cutout.Call<int>("getSafeInsetLeft"));
                                    right = Mathf.Max(right, cutout.Call<int>("getSafeInsetRight"));
                                    top = Mathf.Max(top, cutout.Call<int>("getSafeInsetTop"));
                                    bottom = Mathf.Max(bottom, cutout.Call<int>("getSafeInsetBottom"));
                                }
                            }
                        }
                    }
                    Rect contentRect = new Rect(0f, 0f, windowWidth, windowHeight);
                    using (var ids = new AndroidJavaClass("android.R$id"))
                    using (var content = activity.Call<AndroidJavaObject>("findViewById", ids.GetStatic<int>("content")))
                    using (var bounds = new AndroidJavaObject("android.graphics.Rect"))
                    {
                        if (content != null)
                        {
                            content.Call("getDrawingRect", bounds);
                            decor.Call("offsetDescendantRectToMyCoords", content, bounds);
                            contentRect = Rect.MinMaxRect(bounds.Get<int>("left"), windowHeight - bounds.Get<int>("bottom"),
                                bounds.Get<int>("right"), windowHeight - bounds.Get<int>("top"));
                        }
                    }
                    if (contentRect.width <= 0f || contentRect.height <= 0f) return screen;
                    Rect usable = Intersect(contentRect, Rect.MinMaxRect(left, bottom, windowWidth - right, windowHeight - top));
                    float scaleX = width / contentRect.width;
                    float scaleY = height / contentRect.height;
                    return Rect.MinMaxRect((usable.xMin - contentRect.xMin) * scaleX, (usable.yMin - contentRect.yMin) * scaleY,
                        (usable.xMax - contentRect.xMin) * scaleX, (usable.yMax - contentRect.yMin) * scaleY);
                }
            }
        }
        catch (AndroidJavaException exception)
        {
            if (!nativeWarningLogged)
            {
                nativeWarningLogged = true;
                Debug.LogWarning("[SafeArea] Android insets unavailable; using Unity safe area. " + exception.Message);
            }
            return screen;
        }
    }
}
