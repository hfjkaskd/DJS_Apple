using System.Collections.Generic;
using UnityEngine;

/// <summary>Small shared skin atlases, loaded only when a B view requests them.</summary>
public static class Framework2AppleSkinAssets
{
    private static readonly Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();
    private static readonly HashSet<string> Loaded = new HashSet<string>();
    private static readonly Dictionary<string,Sprite> ReferenceAtlases=new Dictionary<string,Sprite>();
    public static Sprite LoadReferenceAtlas(string path)
    {
        if(string.IsNullOrEmpty(path))return null;
        if(ReferenceAtlases.TryGetValue(path,out Sprite sprite))return sprite;
        sprite=Resources.Load<Sprite>(path);
        if(sprite==null){Debug.LogError("Missing Framework 2 reference atlas: "+path);return null;}
        ReferenceAtlases[path]=sprite;return sprite;
    }
    public static Sprite LoadSprite(string key)
    {
        if (string.IsNullOrEmpty(key)) return null;
        if (Sprites.TryGetValue(key, out Sprite sprite)) return sprite;
        string atlas;
        switch (key)
        {
            case "panel": case "inset": case "button": case "title": case "secondary":
            case "selected": case "close": case "track": case "fill": atlas = "Chrome"; break;
            case "wheel": case "wheel8": atlas = "Wheels"; break;
            case "receipt": atlas = "Receipt"; break;
            case "bank": case "card": case "phone": case "knob": atlas = "Methods"; break;
            default: atlas = "Icons"; break;
        }
        if (Loaded.Add(atlas))
        {
            Sprite[] items = Resources.LoadAll<Sprite>("Framework2AppleSkin/" + atlas);
            for (int i = 0; i < items.Length; i++) Sprites[items[i].name] = items[i];
        }
        if (Sprites.TryGetValue(key, out sprite)) return sprite;
        Debug.LogError("Missing authored Framework 2 skin sprite: " + key);
        return null;
    }
}
