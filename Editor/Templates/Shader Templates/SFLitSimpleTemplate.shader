Shader "SF/SF Lit Simple"
{
    // This shader uses the Lambert technique to calculate lighting with Vertexes for simple lighting.
    // It only uses the Main Light, most cases a directional light.  
    // It takes into consideration the shadows caused by the Main light while supporting shadow cascade levels.
    Properties
    {
        [MainTexture] _MainTex ("Main Texture", 2D) = "white" {}
        _ShadowStrength("Shadow Strength", Float) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="AlphaTest" "RenderPipeline"="UniversalPipeline"  }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // Need these for supporting shadows.
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionHCS : SV_POSITION;
                half3 lightAmount : TEXCOORD2; // Store the lighting data at the current vertex coordinate.
                float4 shadowCoords : TEXCOORD3; // Store the shadow data
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float1 _ShadowStrength; 
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings OUT;

                // Get and cache the Vertex Positions from the input so we can use it in multiple places in the pass.
                VertexPositionInputs positions = GetVertexPositionInputs(IN.positionOS.xyz);

                // Convert the position from object space to homogeneous clip space.
                OUT.positionHCS = positions.positionCS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                
                // Get the position of the normals for each vertex.
                VertexNormalInputs normalPositions = GetVertexNormalInputs(IN.positionOS.xyz);
                
                // This shader only uses the main directional light for calculations. 
                Light light = GetMainLight();
                OUT.lightAmount = LightingLambert(light.color,light.direction,normalPositions.normalWS.xyz);

                // Get the coordinates for the shadows.
                // Note this uses the VertexPositionInputs not VertexNormalInputs used for lighting.
                OUT.shadowCoords = GetShadowCoord(positions);
                
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                half shadowAmount = MainLightRealtimeShadow(IN.shadowCoords);
                half strength = 1.0 - _ShadowStrength;
                
                // Sample Texture times the light amount for the final color.
                // We add one to make it a half4 for better performance calculations.
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * half4(IN.lightAmount,1);

                // Times the calculated light color by the shadow amount before returning it.
                return col * max(strength, shadowAmount);
            }
            ENDHLSL
        }
    }
}
