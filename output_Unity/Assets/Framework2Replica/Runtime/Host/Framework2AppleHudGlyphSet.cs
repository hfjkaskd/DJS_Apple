using System;
using UnityEngine;

/// <summary>Small bitmap typeface authored from the approved HUD, with regional currency glyphs.</summary>
public sealed class Framework2AppleHudGlyphSet : ScriptableObject
{
    [Serializable] public struct Glyph
    {
        public int character;
        public Rect uv;
        public Vector2 size, bearing;
        public float advance;
    }
    [SerializeField] private Texture2D atlas;
    [SerializeField] private Glyph[] glyphs;
    [SerializeField] private float ascent, descent;
    public Texture2D Atlas => atlas;
    public float Ascent => ascent;
    public float Descent => descent;
    public void Configure(Texture2D texture, Glyph[] characters, float top, float bottom)
    { atlas=texture;glyphs=characters;ascent=top;descent=bottom; }
    public bool TryGet(char character, out Glyph result)
    {
        if(character=='\u00a0' || character=='\u202f')character=' ';
        // The main set is ASCII, stored in codepoint order. No per-refresh map allocation.
        int index=character-32;
        if(index>=0 && index<glyphs.Length && glyphs[index].character==character)
        {result=glyphs[index];return true;}
        for(int i=95;i<glyphs.Length;i++)
            if(glyphs[i].character==character){result=glyphs[i];return true;}
        result=default;return false;
    }
}
