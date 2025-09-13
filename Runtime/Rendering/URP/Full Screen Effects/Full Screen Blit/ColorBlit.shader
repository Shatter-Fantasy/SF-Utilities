Shader "SF/Color Blit"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        ZWrite Off Cull Off
        Pass
        {
            Name "ColorBlitPass"

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma vertex Vert
            #pragma fragment Frag

            float _Intensity;

            float4 Frag(Varyings input) : SV_Target
            {
                // This function handles the different ways XR platforms handle texture arrays.
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                // Sample the texture using the SAMPLE_TEXTURE2D_X_LOD function
                float2 uv = input.texcoord.xy;
                half4 color = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearRepeat, uv, _BlitMipLevel);
                    
                // Modify the sampled color
                return half4(0, _Intensity, 0, 1) * color;
            }

            ENDHLSL
        }
    }
}