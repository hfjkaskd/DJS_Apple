using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Editor evidence only. Records the measured approval boxes during prefab authoring;
// the capture runner compares them with real, settled runtime RectTransforms.
public static class Framework2ReferenceLayoutAudit
{
    public const string ManifestPath="Docs/CommercializationAB/Framework2Replica/ReskinLayoutParity-20261027/reference-layout.json";
    [Serializable] public sealed class Box { public string path; public Rect reference; }
    [Serializable] public sealed class Page
    {
        public string template;
        public Rect phone;
        public List<Box> boxes=new List<Box>();
    }
    [Serializable] public sealed class Document
    {
        public string mapping="Uniform width scale, centred approval viewport; no independent vertical scaling.";
        public List<Page> pages=new List<Page>();
    }
    static Document document;
    static string variant;
    public static void Begin() { document=new Document(); }
    public static void Variant(string value) { variant=value; }
    public static void Record(GameObject root,string path,Rect phone,Rect reference)
    {
        if(document==null)return;
        string template=root.name;
        if(!string.IsNullOrEmpty(variant))template+="#"+variant;
        const string prefix="TestOverlay/ModalRoot/";
        if(path.StartsWith(prefix,StringComparison.Ordinal))
        {
            string suffix=path.Substring(prefix.Length);
            int slash=suffix.IndexOf('/');
            template=suffix.Substring(0,slash); path=suffix.Substring(slash+1);
        }
        Page page=document.pages.Find(item=>item.template==template);
        if(page==null) {page=new Page {template=template,phone=phone}; document.pages.Add(page);}
        Box box=page.boxes.Find(item=>item.path==path);
        if(box==null) {box=new Box {path=path};page.boxes.Add(box);}
        box.reference=reference;
    }
    public static void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(ManifestPath));
        File.WriteAllText(ManifestPath,JsonUtility.ToJson(document,true));
    }
    public static Document Read() => JsonUtility.FromJson<Document>(File.ReadAllText(ManifestPath));
}
