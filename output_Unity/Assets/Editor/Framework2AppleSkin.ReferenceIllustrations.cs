using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static partial class Framework2AppleSkin
{
    static readonly Dictionary<int,Sprite> referenceBoards=new Dictionary<int,Sprite>();
    static Sprite ReferenceBoard(int number)
    {
        if(referenceBoards.TryGetValue(number,out Sprite sprite))return sprite;
        string path=Art+"Reference"+number.ToString("00")+".png";
        if(!File.Exists(path))File.Copy("Docs/CommercializationAB/Framework2Replica/ReskinPreview-AllUI-20261023/board-"+number.ToString("00")+".png",path);
        AssetDatabase.ImportAsset(path);
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType=TextureImporterType.Sprite; importer.spriteImportMode=SpriteImportMode.Single;
        importer.mipmapEnabled=false; importer.alphaIsTransparency=false; importer.maxTextureSize=2048;
        importer.wrapMode=TextureWrapMode.Clamp; importer.filterMode=FilterMode.Bilinear;
        importer.textureCompression=TextureImporterCompression.CompressedHQ;
        importer.SaveAndReimport(); sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);
        referenceBoards[number]=sprite; return sprite;
    }

    static void RefCutout(Image image,int board,Rect bounds,Vector2[] polygon)
    {
        if(image==null)return;
        var old=image.GetComponent<Framework2ReferenceSurface>();
        if(old!=null)Object.DestroyImmediate(old);
        Vector2[] points=new Vector2[polygon.Length],uvs=new Vector2[polygon.Length];
        for(int i=0;i<polygon.Length;i++)
        {
            points[i]=new Vector2((polygon[i].x-bounds.x)/bounds.width,1-(polygon[i].y-bounds.y)/bounds.height);
            uvs[i]=new Vector2(polygon[i].x/1672f,1-polygon[i].y/941f);
        }
        List<int> remaining=new List<int>();
        float signed=0;
        for(int i=0;i<points.Length;i++) {remaining.Add(i);Vector2 a=points[i],b=points[(i+1)%points.Length];signed+=a.x*b.y-b.x*a.y;}
        if(signed<0)remaining.Reverse();
        List<int> indices=new List<int>(); int attempts=0;
        while(remaining.Count>2 && attempts++<polygon.Length*polygon.Length)
        {
            bool found=false;
            for(int i=0;i<remaining.Count;i++)
            {
                int a=remaining[(i+remaining.Count-1)%remaining.Count],b=remaining[i],c=remaining[(i+1)%remaining.Count];
                if(Cross(points[b]-points[a],points[c]-points[b])<=0)continue;
                bool contains=false;
                for(int j=0;j<remaining.Count;j++)
                {
                    int p=remaining[j]; if(p==a||p==b||p==c)continue;
                    if(Cross(points[b]-points[a],points[p]-points[a])>=0 &&
                       Cross(points[c]-points[b],points[p]-points[b])>=0 &&
                       Cross(points[a]-points[c],points[p]-points[c])>=0){contains=true;break;}
                }
                if(contains)continue;
                indices.Add(a);indices.Add(b);indices.Add(c);remaining.RemoveAt(i);found=true;break;
            }
            if(!found)throw new System.InvalidOperationException("Invalid approved sprite contour: "+image.name);
        }
        var mesh=image.GetComponent<Framework2ReferenceCutout>();
        if(mesh==null)mesh=image.gameObject.AddComponent<Framework2ReferenceCutout>();
        float[] alpha=null;
        if(image.sprite!=null && image.sprite.name=="cashpile")
        {
            // Feather the glow around the extracted hero art in mesh alpha.
            // No runtime texture readback or full-screen effect is required.
            int n=points.Length; Vector2[] featherPoints=new Vector2[n*2],featherUVs=new Vector2[n*2]; alpha=new float[n*2];
            for(int i=0;i<n;i++)
            {
                Vector2 inward=Vector2.MoveTowards(polygon[i],bounds.center,9);
                featherPoints[i]=new Vector2((inward.x-bounds.x)/bounds.width,1-(inward.y-bounds.y)/bounds.height);
                featherUVs[i]=new Vector2(inward.x/1672f,1-inward.y/941f);alpha[i]=1;
                featherPoints[n+i]=points[i];featherUVs[n+i]=uvs[i];alpha[n+i]=0;
                int j=(i+1)%n;
                indices.Add(i);indices.Add(n+i);indices.Add(n+j);
                indices.Add(i);indices.Add(n+j);indices.Add(j);
            }
            points=featherPoints;uvs=featherUVs;
        }
        mesh.Configure(board==3?referenceAtlas:ReferenceBoard(board),points,uvs,indices.ToArray(),alpha);
        image.color=Color.white; image.enabled=true;
    }
    static float Cross(Vector2 a,Vector2 b)=>a.x*b.y-a.y*b.x;
    static Vector2 P(float x,float y)=>new Vector2(x,y);
    static void RefEllipse(Image image,int board,Rect bounds)
    {
        Vector2[] polygon=new Vector2[64];
        for(int i=0;i<64;i++) {float angle=i*Mathf.PI*2/64;polygon[i]=new Vector2(bounds.center.x+Mathf.Cos(angle)*bounds.width*.5f,bounds.center.y+Mathf.Sin(angle)*bounds.height*.5f);}
        RefCutout(image,board,bounds,polygon);
    }

    static void AlignReferenceIllustrations(GameObject root,string name)
    {
        // The approval sheet is immutable. These are ordinary sprite UVs with
        // authored contours, not a screenshot drawn over the live game UI.
        foreach(Image image in root.GetComponentsInChildren<Image>(true))
        {
            if(image.sprite==null)continue;
            switch(image.sprite.name)
            {
                case "cashpile":
                    RefCutout(image,7,new Rect(1318,356,288,152),new[]{P(1318,462),P(1333,433),P(1342,429),P(1332,411),P(1336,389),P(1351,369),P(1362,388),P(1374,378),P(1385,386),P(1404,378),P(1464,367),P(1528,394),P(1539,403),P(1548,387),P(1572,389),P(1563,415),P(1587,424),P(1606,448),P(1591,457),P(1605,478),P(1582,495),P(1544,494),P(1511,507),P(1436,503),P(1400,491),P(1368,501),P(1348,488),P(1320,482)});break;
            }
        }
        if(name=="WithDrawConfirm")
        {
            Image method=AccComponent<Image>(root,"PanelA/method");
            method.gameObject.SetActive(true); method.enabled=true;
            RefCutout(method,3,new Rect(1013,342,65,72),new[]{P(1028,342),P(1061,342),P(1071,349),P(1074,362),P(1078,367),P(1077,382),P(1064,393),P(1052,393),P(1048,413),P(1031,413),P(1031,404),P(1014,404)});
            method.transform.SetAsLastSibling();
        }
        if(name=="BonusWheelView")
        {
            RefEllipse(RewardComponent<Image>(root,"Plane/Turntable/Table"),7,new Rect(881,302,330,329));
            for(int i=0;i<8;i++)root.transform.Find("Plane/Turntable/Table/SkinBonusCash"+i)?.gameObject.SetActive(false);
            RefEllipse(RewardComponent<Image>(root,"Plane/LotteryBtn"),7,new Rect(987,409,116,117));
            foreach(Text text in root.transform.Find("Plane/LotteryBtn").GetComponentsInChildren<Text>(true))text.enabled=false;
        }
        if(name=="BonusWheelView" || name=="LuckySpinView")
            RefCutout(AccComponent<Image>(root,"Plane/Turntable/Pointer"),7,new Rect(1024,289,41,53),
                new[]{P(1045,290),P(1058,295),P(1064,309),P(1059,322),P(1045,342),P(1031,323),P(1025,307),P(1031,294)});
        if(name=="SettingsView")
        {
            RefRoundedIllustration(AccComponent<Image>(root,"Plane/Bg2/PlayerIcon"),12,new Rect(75,326,96,103),14);
            RefCutout(AccComponent<Image>(root,"Plane/Sound/NavSoundIcon"),12,new Rect(83,497,42,37),new[]{P(84,508),P(95,507),P(106,498),P(106,508),P(113,501),P(122,507),P(125,516),P(121,526),P(113,533),P(108,526),P(106,534),P(95,524),P(84,523)});
            RefRoundedIllustration(AccComponent<Image>(root,"Plane/Vibration/NavVibrationIcon"),12,new Rect(85,554,38,44),2);
        }
    }

    static void RefRoundedIllustration(Image image,int board,Rect bounds,float radius)
    {
        var points=new List<Vector2>();
        for(int corner=0;corner<4;corner++)
        {
            float cx=(corner==0||corner==3)?bounds.xMax-radius:bounds.x+radius;
            float cy=corner<2?bounds.yMax-radius:bounds.y+radius;
            for(int i=0;i<=8;i++)
            {
                float angle=(corner*90+i*90f/8)*Mathf.Deg2Rad;
                points.Add(new Vector2(cx+Mathf.Cos(angle)*radius,cy+Mathf.Sin(angle)*radius));
            }
        }
        RefCutout(image,board,bounds,points.ToArray());
    }
}
