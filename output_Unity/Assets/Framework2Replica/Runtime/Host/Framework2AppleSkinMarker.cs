using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>Identifies an authored Apple skin subtree whose configured fonts must be retained.</summary>
[DisallowMultipleComponent]
public sealed class Framework2AppleSkinMarker : MonoBehaviour
{
    [Serializable] public sealed class FontMetric
    {
        public Text target;
        public int size,min,max;
    }
    [SerializeField,HideInInspector] FontMetric[] authoringFontMetrics=Array.Empty<FontMetric>();
    // Editor authoring calls these explicitly. They have no runtime lifecycle hook.
    public void RestoreAuthoringFonts()
    {
        foreach(var item in authoringFontMetrics)
            if(item.target!=null){item.target.fontSize=item.size;item.target.resizeTextMinSize=item.min;item.target.resizeTextMaxSize=item.max;}
    }
    public void RecordAuthoringFonts()
    {
        Text[] texts=GetComponentsInChildren<Text>(true);
        authoringFontMetrics=new FontMetric[texts.Length];
        for(int i=0;i<texts.Length;i++)authoringFontMetrics[i]=new FontMetric {target=texts[i],size=texts[i].fontSize,min=texts[i].resizeTextMinSize,max=texts[i].resizeTextMaxSize};
    }
}
