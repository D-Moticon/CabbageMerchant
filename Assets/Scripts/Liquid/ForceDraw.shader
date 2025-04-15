Shader "Custom/ForceDraw"
{
    Properties{ _MainTex("Texture", 2D) = "white" {} }

        SubShader
    {
        Tags { "RenderType" = "Transparent" }
        Blend One OneMinusSrcAlpha // Allows controlled additive blending
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct v2f { float4 pos : SV_POSITION; };
            float4 _Point;
            float4 _Color;

            v2f vert(float4 vertex : POSITION)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float dist = distance(i.pos.xy, _Point.xy);

            // If within radius, apply force
            if (dist < _Point.z)
                return _Color; // Apply force

            return fixed4(0, 0, 0, 0); // No change
        }
        ENDCG
    }
    }
}
