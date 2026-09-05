Shader "Momentum/Glow"
{
    Properties { _Tint ("Tint", Color) = (1,1,1,1) }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off ZWrite Off Blend SrcAlpha One
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct input { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct output { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; };
            fixed4 _Tint;
            output vert(input v) { output o; o.vertex=UnityObjectToClipPos(v.vertex); o.uv=v.uv; return o; }
            fixed4 frag(output i) : SV_Target
            {
                float r=length(i.uv*2-1);
                float a=pow(saturate(1-r), 2.5);
                return fixed4(_Tint.rgb, _Tint.a*a);
            }
            ENDCG
        }
    }
}
