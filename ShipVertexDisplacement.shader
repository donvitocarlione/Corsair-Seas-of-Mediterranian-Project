Shader "Custom/ShipVertexDisplacement"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _WaveDisplacement ("Wave Displacement", Float) = 0
        _DisplacementAmount ("Displacement Amount", Range(0, 1)) = 0.1
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _WaveDisplacement;
            float _DisplacementAmount;

            v2f vert (appdata v)
            {
                v2f o;
                // Apply wave displacement along the normal
                float3 displacement = v.normal * _WaveDisplacement * _DisplacementAmount;
                float4 modifiedVertex = v.vertex + float4(displacement, 0);
                o.vertex = UnityObjectToClipPos(modifiedVertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
