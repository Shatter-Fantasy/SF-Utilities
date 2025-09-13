Shader "SF/Water Complex"
{
    Properties
    {
        _WaveHeight ("Wave Height", float) = 1
    }
    SubShader
    {
        // Setting the transparency queue to 110 here to make sure we get all transparent water shaders in the correct alpha blend order.
        Tags { "RenderType"="Transparent" "Queue"="Transparent-110" "RenderPipeline"="UniversalPipeline"  }
        ZWrite off
        cull off
        LOD 100

        Pass
        {
            
            Name "Infinite Water"
            Tags {"LightMode" = "UniversalForward" }
            
            // Adding the pragma keywords here instead of the sub shaders because I might want different types of rendering for different water set up.
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // Need these for supporting shadows.
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/shatter-fantasy.sf-utilities/Runtime/Rendering/HLSL/Waves/GerstnerWaves.hlsl"
            
            half _WaveHeight;
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 texCoord : TEXCOORD0;
            };
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD2;
                float4 screenPosition : TEXCOORD3;
                float4 viewDirectionWS : TEXCOORD4;
            };
            
            // The Varying and Attribute structs are located in the WaterInput.hlsl file being included.

            Varyings vert (Attributes IN)
            {
                // (Varyings)0 just tells all values to be initalized to 0.
                // We do this because we are injecting data from a StructuredBuffer of WaterInputData structs.
                Varyings OUT;

                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                
                OUT.uv.xy = IN.texCoord;

                float3 cameraOffset = GetCameraPositionWS();
                // TODO: We need to finish the positionOS.xz blending.
                // Need some kind of blending here for the water
                //IN.positionOS.xz *= some blending * 0.55;  // The 0.55 will be scale range to blend the distance over
                IN.positionOS.y *= cameraOffset.y - _WaveHeight * 2; // scale height to camera
				IN.positionOS.y -= cameraOffset.y - _WaveHeight * 2;
                
                VertexPositionInputs positions = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionHCS = positions.positionCS;    // Convert the position from object space to homogeneous clip space.
                OUT.positionWS = positions.positionWS;
                OUT.screenPosition = positions.positionNDC; // Same as the old ComputeScreenPos function.

                // So yeah, this section just calculates the view space position based on the depth value.
                float3 viewPos = positions.positionVS; // positionVS gets the vertex in view space.
                OUT.viewDirectionWS.xyz = UNITY_MATRIX_IT_MV[2].xyz; // Inverse transpose of model * view matrix
                OUT.viewDirectionWS.w = length(viewPos / viewPos.z);
                
                return OUT;
            }

            struct Output
            {
                half4 color : SV_Target;
                // We have to read back the depth in out fragment shader for other water FX shader files.
                float depth : SV_Depth; 
            };

            // Notice we are sending an output struct instead of the normal half4 just color.
            // See the struct declaration for notes on the depth value.
            half4 frag (Varyings IN) : SV_Target
            {

                // Might have to do  half2 screenUV = IN.screenPosition.xy / IN.screenPosition.w; instead.
                half2 screenUV = GetNormalizedScreenSpaceUV(IN.screenPosition);

                // TODO: Set up the buffers for the water.

                // TODO: Set up the infinite plane for the water.

                // TODO: Compute fopg factor again.

                // TODO At this point we need to reset some of the Varyings IN and push it into the InitializeInputData call.
                // TODO: Actually pack the water data here.
                half4 packedData = (1,1,1,1);
                
                // Set up our shader data.
               // WaterInputData inputData;
                //InitializeInputData(IN,inputData,screenUV);

                //WaterSurfaceData surfaceData;
                //InitializeSurfaceData(inputData,surfaceData, packedData);

                
                half4 color = half4(1,1,1,1);
                //color.rgb = WaterShading(inputData,surfaceData,packedData,screenUV.xy);
                
                Output output;
                output.color = color;
                // TODO: output.depth = plane.depth;
                //output.depth = 2;
                return color;  // Finished and freedom.
            }
            ENDHLSL
        }
    }
}
