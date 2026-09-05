Shader "Lucent/Facet Metal and Light"
{
    Properties { _Tint("Tint",Color)=(.3,.8,1,1) _Emission("Emission",Range(0,3))=.4 _Inflate("Small-scale edge expansion",Float)=0 }
    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        Cull Back ZWrite On
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex:POSITION; float3 normal:NORMAL; };
            struct vary { float4 pos:SV_POSITION; float3 normal:TEXCOORD0; };
            float4 _Tint; float _Emission,_Inflate;
            vary vert(appdata v) { vary o;v.vertex.xyz+=v.normal*_Inflate;o.pos=UnityObjectToClipPos(v.vertex);o.normal=UnityObjectToWorldNormal(v.normal);return o; }
            float4 frag(vary i):SV_Target { float light=saturate(dot(normalize(i.normal),normalize(float3(-.5,.75,-.85))));return float4(_Tint.rgb*(.22+.78*light+_Emission),1); }
            ENDCG
        }
    }
}
