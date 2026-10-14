using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FruitApkEarning
{
    // All visual nodes and aliases are saved in the prefab by ApkEarningUIAuthoring.
    public sealed class ApkEarningPageView : MonoBehaviour
    {
        [Serializable] public sealed class TextSlot { public string Key; public TMP_Text Target; }
        [Serializable] public sealed class ButtonSlot { public string Key; public Button Target; }
        [Serializable] public sealed class ObjectSlot { public string Key; public GameObject Target; }
        [Serializable] public sealed class ImageSlot { public string Key; public Image Target; }
        [Serializable] public sealed class InputSlot { public string Key; public TMP_InputField Target; }
        public TextSlot[] Texts = Array.Empty<TextSlot>();
        public ButtonSlot[] Buttons = Array.Empty<ButtonSlot>();
        public ObjectSlot[] Objects = Array.Empty<ObjectSlot>();
        public ImageSlot[] Images = Array.Empty<ImageSlot>();
        public InputSlot[] Inputs = Array.Empty<InputSlot>();
        public RectTransform ArtworkRoot;
        public float ReferenceWidth = 1080;
        public float MinimumReferenceHeight = 1800;
        public bool FitWidth;
        public Color EmphasisColor = new Color32(53, 110, 29, 255);
        private readonly Dictionary<Button, UnityAction> listeners = new Dictionary<Button, UnityAction>();
        private bool fitting;

        public TMP_Text GetText(string key) { foreach(var s in Texts) if(s.Key==key)return s.Target; return null; }
        public Button GetButton(string key) { foreach(var s in Buttons) if(s.Key==key)return s.Target; return null; }
        public GameObject GetObject(string key) { foreach(var s in Objects) if(s.Key==key)return s.Target; return null; }
        public Image GetImage(string key) { foreach(var s in Images) if(s.Key==key)return s.Target; return null; }
        public TMP_InputField GetInput(string key) { foreach(var s in Inputs) if(s.Key==key)return s.Target; return null; }
        public void SetText(string key,string value) { var t=GetText(key); if(t!=null&&t.text!=value)t.text=value; }
        public void SetVisible(string key,bool visible) { var o=GetObject(key); if(o!=null&&o.activeSelf!=visible)o.SetActive(visible); }
        public void SetProgress(string key,float value) { var i=GetImage(key); if(i!=null)i.fillAmount=Mathf.Clamp01(value); }
        public void Bind(string key,Action action)
        {
            var button=GetButton(key); if(button==null)return;
            UnityAction previous;
            if(listeners.TryGetValue(button,out previous))button.onClick.RemoveListener(previous);
            UnityAction next=()=>action(); listeners[button]=next; button.onClick.AddListener(next);
        }
        public void Unbind()
        {
            foreach(var item in listeners)if(item.Key!=null)item.Key.onClick.RemoveListener(item.Value);
            listeners.Clear();
        }
        private void OnDestroy(){Unbind();}
        private void OnEnable(){RefreshLayout();}
        private void OnRectTransformDimensionsChange(){RefreshLayout();}
        public void RefreshLayout()
        {
            if(fitting||!FitWidth||ArtworkRoot==null||ArtworkRoot==transform||ReferenceWidth<=0)return;
            fitting=true;
            try
            {
                var rect=transform as RectTransform;
                if(rect==null||rect.rect.width<=0||rect.rect.height<=0)return;
                float scale=rect.rect.width/ReferenceWidth;
                if(MinimumReferenceHeight>0)scale=Mathf.Min(scale,rect.rect.height/MinimumReferenceHeight);
                if(scale<=0)return;
                ArtworkRoot.sizeDelta=new Vector2(ReferenceWidth,rect.rect.height/scale);
                ArtworkRoot.localScale=Vector3.one*scale;
            }
            finally {fitting=false;}
        }
    }
}
