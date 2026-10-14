Shader "UI/CircleImage"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Radius ("Radius", Range(0,0.5)) = 0.3
        _AASmoothing ("AA Smoothing", Range(0,0.1)) = 0.01
        _AspectRatio ("Aspect Ratio", Float) = 1
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask] }
        Cull Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct Input { float4 vertex:POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };
            struct Output { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };
            sampler2D _MainTex;
            float4 _MainTex_ST;
            half4 _Color;
            float _Radius, _AASmoothing, _AspectRatio;
            Output vert(Input input)
            {
                Output output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = TRANSFORM_TEX(input.uv,_MainTex);
                output.color = input.color * _Color;
                return output;
            }
            half4 frag(Output input):SV_Target
            {
                float2 delta = float2((input.uv.x-0.5)*_AspectRatio,input.uv.y-0.5);
                float coverage = saturate((length(delta)-(_Radius+_AASmoothing))/(-2.0*_AASmoothing));
                coverage = coverage*coverage*(3.0-2.0*coverage);
                half4 color = tex2D(_MainTex,input.uv)*input.color;
                color.a *= coverage;
                return color;
            }
            ENDCG
        }
    }
    Fallback "UI/Default"
}
