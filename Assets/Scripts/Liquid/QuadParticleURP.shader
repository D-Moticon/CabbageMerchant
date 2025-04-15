Shader "Unlit/QuadParticleURP"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (0,0,0,1)
        _ParticleSize("Particle Size", Float) = 0.1
        _EnableStretching("Enable Stretching", Float) = 0  // 0 = Off, 1 = On
        _StretchFactor("Stretch Factor", Float) = 0.1
        _SqueezeFactor("Squeeze Factor", Float) = 0.1
    }
        SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            StructuredBuffer<float2> _Positions;
            StructuredBuffer<float2> _Velocities;
            StructuredBuffer<uint2> _ParticleCellAndType;

            float4 _BaseColor;
            float4 _Type1Color;
            float4 _Type2Color;
            float4 _Type3Color;
            float4 _Type4Color;
            float4 _Type5Color;
            float  _ParticleSize;
            float  _EnableStretching;
            float  _StretchFactor;
            float  _SqueezeFactor;
            float _MinimumStretchSpeed;
            int _ParticleCount;

            struct Attributes
            {
                float3 localPos   : POSITION;
                uint   instanceID : SV_InstanceID;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 color       : COLOR;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                // Read the per-particle position
                float2 pos2D = _Positions[IN.instanceID];
                float3 worldPos = float3(pos2D, 0);

                if (_EnableStretching > 0.5)
                {
                // Read velocity
                    float2 velocity = _Velocities[IN.instanceID];
                    float speed = clamp(length(velocity)-_MinimumStretchSpeed, 0.0, 5.0);

                    float2 dir = normalize(velocity);
                    float2 perp = float2(-dir.y, dir.x);

                    float squeeze = 1.0 / (1.0+_SqueezeFactor * speed);

                // First two vertices (regular size)
                    if (IN.localPos.y > 0.0)
                    {
                        if (IN.localPos.x > 0.0)
                        {
                            worldPos.xy += (_ParticleSize * (dir - perp*squeeze))*0.5f;
                        }
                        
                        else
                        {
                            worldPos.xy += (_ParticleSize * (dir + perp*squeeze))*0.5f;
                        }
                    }
                // Stretched vertices
                    else
                    {
                        if (IN.localPos.x > 0.0)
                        {
                            worldPos.xy += (_ParticleSize * (-dir - perp*squeeze) - _StretchFactor*dir*speed)*0.5f;
                        }
                        else
                        {
                            worldPos.xy += (_ParticleSize * (-dir + perp*squeeze) - _StretchFactor * dir*speed)*0.5f;
                        }
                    }
                }
    
                else
                {
                    // Regular non-stretched position
                    worldPos.xy += IN.localPos.xy * _ParticleSize;
                }

                // Transform to clip space
                OUT.positionHCS = TransformWorldToHClip(worldPos);

                uint particleType = _ParticleCellAndType[IN.instanceID].y;

                if (particleType == 1)
                {
                    OUT.color = _Type1Color;
                }

                else if (particleType == 2)
                {
                    OUT.color = _Type2Color;
                }

                else if (particleType == 3)
                {
                    OUT.color = _Type3Color;
                }

                else if (particleType == 4)
                {
                    OUT.color = _Type4Color;
                }

                else if (particleType == 5)
                {
                    OUT.color = _Type5Color;
                }

                else
                {
                    OUT.color = _BaseColor;
                }
                

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                return IN.color;
            }
            ENDHLSL
        }
    }
}