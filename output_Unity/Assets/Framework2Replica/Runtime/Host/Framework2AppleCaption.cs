using UnityEngine;
using UnityEngine.UI;

/// <summary>Static art-approved caption. Dynamic source values do not use this component.</summary>
[RequireComponent(typeof(Text))]
public sealed class Framework2AppleCaption : MonoBehaviour
{
    [SerializeField] private Text target;
    [SerializeField] private string caption;
    public void Configure(Text value,string text){target=value;caption=text;Apply();}
    public void Apply(){if(target!=null)target.text=caption;}
}
