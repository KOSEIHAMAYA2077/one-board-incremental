Shader "Lucent/Crystal Glass"
{
    Properties { _Tint("Shell tint",Color)=(.3,.9,1,1) _Opacity("Opacity",Range(0,1))=.32 _Damage("Damage",Range(0,1))=0 _Flash("Impact",Range(0,1))=0 }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
        CGINCLUDE
        #include "UnityCG.cginc"
        struct appdata { float4 vertex:POSITION; float3 normal:NORMAL; };
        struct vary { float4 pos:SV_POSITION; float3 normal:TEXCOORD0; float3 world:TEXCOORD1; float3 local:TEXCOORD2; };
        float4 _Tint; float _Opacity,_Damage,_Flash;
        vary vert(appdata v) { vary o; o.pos=UnityObjectToClipPos(v.vertex);o.normal=UnityObjectToWorldNormal(v.normal);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;o.local=v.vertex.xyz;return o; }
        float4 Shade(vary i,float back)
        {
            float3 n=normalize(i.normal); float3 view=unity_OrthoParams.w>0?normalize(UNITY_MATRIX_V[2].xyz):normalize(_WorldSpaceCameraPos-i.world);
            float rim=pow(1-abs(dot(n,view)),2.4);
            float light=saturate(dot(n,normalize(float3(-.5,.75,-.85))));
            float spec=pow(saturate(dot(reflect(-normalize(float3(-.5,.75,-.85)),n),view)),38);
            float band=pow(saturate(1-abs(dot(i.local,float3(.8,.5,.2))-.17)*28),9)*.2;
            float crack=step(.78,_Damage)*(1-smoothstep(.003,.018,abs(i.local.x+i.local.y*.63-.1)))*.6;
            float3 color=_Tint.rgb*(.26+light*.75)+float3(.6,.88,1)*(rim*.6+spec*1.1+band)+crack+_Flash*.65;
            return float4(color,saturate((_Opacity+rim*.28+spec*.25+crack*.4+_Flash*.3)*back));
        }
        float4 fragBack(vary i):SV_Target { return Shade(i,.38); }
        float4 fragFront(vary i):SV_Target { return Shade(i,1); }
        ENDCG
        Pass { Cull Front CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragBack
            ENDCG }
        Pass { Cull Back CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragFront
            ENDCG }
    }
}
