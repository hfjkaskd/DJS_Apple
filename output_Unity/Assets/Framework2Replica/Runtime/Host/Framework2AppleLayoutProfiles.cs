using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Chooses an authored prefab layout for an existing presentation state.</summary>
[DisallowMultipleComponent]
public sealed class Framework2AppleLayoutProfiles : MonoBehaviour
{
    [Serializable] public sealed class Entry
    {
        public RectTransform target;
        public Vector2 position,size;
        public Vector2 anchorMin,anchorMax,pivot;
        public Text text;
        public int font,minFont,maxFont;
    }
    [Serializable] public sealed class Visibility
    {
        public GameObject target;
        public bool visible;
    }
    [Serializable] public sealed class Profile
    {
        public string key;
        public Entry[] entries;
        public Visibility[] visibility;
    }
    [SerializeField] Profile[] profiles=Array.Empty<Profile>();
    [SerializeField] float fontScale=1f;
    [SerializeField,Range(.2f,1f)] float minimumFontRatio=.55f;
    string current;
    public void Configure(Profile[] value) {profiles=value; fontScale=1f; current=null;}
    public void ConfigureFontScale(float value,float minimumRatio) {fontScale=value;minimumFontRatio=minimumRatio;}
    public void Apply(string key)
    {
        if(current==key)return;
        for(int i=0;i<profiles.Length;i++)
        {
            Profile profile=profiles[i];if(profile.key!=key)continue;
            foreach(Entry entry in profile.entries)
            {
                if(entry.target==null)continue;
                entry.target.anchorMin=entry.anchorMin;entry.target.anchorMax=entry.anchorMax;entry.target.pivot=entry.pivot;
                entry.target.anchoredPosition=entry.position;entry.target.sizeDelta=entry.size;
                if(entry.text!=null)
                {
                    entry.text.fontSize=Mathf.RoundToInt(entry.font*fontScale);
                    entry.text.resizeTextMinSize=Mathf.Min(Mathf.RoundToInt(entry.minFont*fontScale),Mathf.RoundToInt(entry.text.fontSize*minimumFontRatio));
                    entry.text.resizeTextMaxSize=Mathf.RoundToInt(entry.maxFont*fontScale);
                }
            }
            foreach(Visibility item in profile.visibility)
                if(item.target!=null)item.target.SetActive(item.visible);
            current=key; return;
        }
    }
}
