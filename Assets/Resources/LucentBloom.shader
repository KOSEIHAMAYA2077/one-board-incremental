Shader "Hidden/Lucent/Bloom"
{
    Properties { _MainTex("Source",2D)="white" {} _Bloom("Bloom",2D)="black" {} _Strength("Strength",Float)=.38 }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        CGINCLUDE
        #include "UnityCG.cginc"
        sampler2D _MainTex,_Bloom;float4 _Direction;float _Strength;
        float4 extract(v2f_img i):SV_Target { float3 c=tex2D(_MainTex,i.uv).rgb;float peak=max(c.r,max(c.g,c.b));return float4(c*saturate((peak-.68)/max(peak,.001)),1); }
        float4 blur(v2f_img i):SV_Target { float2 d=_Direction.xy*1.65;return tex2D(_MainTex,i.uv)*.227027+(tex2D(_MainTex,i.uv+d*1.384615)+tex2D(_MainTex,i.uv-d*1.384615))*.316216+(tex2D(_MainTex,i.uv+d*3.230769)+tex2D(_MainTex,i.uv-d*3.230769))*.070270; }
        float4 composite(v2f_img i):SV_Target { return float4(tex2D(_MainTex,i.uv).rgb+tex2D(_Bloom,i.uv).rgb*_Strength,1); }
        ENDCG
        Pass { CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment extract
            ENDCG }
        Pass { CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment blur
            ENDCG }
        Pass { CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment composite
            ENDCG }
    }
}
