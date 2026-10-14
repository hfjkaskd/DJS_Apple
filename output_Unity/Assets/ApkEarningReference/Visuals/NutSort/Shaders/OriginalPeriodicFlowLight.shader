Shader "NutSort/Original Periodic Flow Light"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _FlowColor ("Flow Color", Color) = (1,1,1,1)
        _FlowSpeed ("Flow Speed (source unused)", Range(0.1,10)) = 2
        _FlowWidth ("Flow Width", Range(0.01,0.5)) = 0.1
        _FlowIntensity ("Flow Intensity", Range(0.1,5)) = 1.5
        _FlowAngle ("Flow Angle", Range(0,360)) = 45
        _FlowInterval ("Flow Interval", Range(0,5)) = 1
        _FlowDuration ("Flow Duration", Range(0.1,3)) = 0.8
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "CanUseSpriteAtlas"="True" "PreviewType"="Plane" }
        Cull Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _MainTex_ST;
            half4 _Color, _FlowColor;
            float _FlowWidth, _FlowIntensity, _FlowAngle, _FlowInterval, _FlowDuration;
            struct Input { float4 position:POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };
            struct Output { float4 position:SV_POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };
            Output vert(Input input)
            {
                Output output;
                output.position=UnityObjectToClipPos(input.position);
                output.uv=TRANSFORM_TEX(input.uv,_MainTex);
                output.color=input.color*_Color;
                return output;
            }
            half4 frag(Output input):SV_Target
            {
                half4 baseColor=tex2D(_MainTex,input.uv)*input.color;
                float cycle=fmod(_Time.y,_FlowInterval+_FlowDuration);
                if(cycle>=_FlowDuration)return baseColor;
                float angle=_FlowAngle*0.0174532924;
                float projection=dot(input.uv,float2(cos(angle),sin(angle)));
                float center=(cycle/_FlowDuration)*1.4-0.2;
                float strength=(1-saturate(abs(projection-center)/_FlowWidth))*_FlowIntensity;
                float3 lit=baseColor.rgb+strength*_FlowColor.rgb*_FlowColor.a;
                return half4(lit*(1+strength*0.25),baseColor.a);
            }
            ENDCG
        }
    }
}
