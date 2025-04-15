Shader "Unlit/ParticleURP"
{
    Properties
    {
        _ParticleSize("Particle Size", Float) = 0.1
        _Color("Color", Color) = (1,1,1,1)
    }
        SubShader
    {
        Tags{"RenderType" = "Transparent" "Queue" = "Transparent"}
        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            StructuredBuffer<float2> _Positions;
            float _ParticleSize;
            float4 _Color;

            struct appdata
            {
                uint vertexID : SV_VertexID;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                float2 pos = _Positions[v.vertexID];
                o.position = TransformWorldToHClip(float3(pos, 0.0));
                o.color = _Color;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                return i.color;
            }
            ENDHLSL
        }
    }
}
