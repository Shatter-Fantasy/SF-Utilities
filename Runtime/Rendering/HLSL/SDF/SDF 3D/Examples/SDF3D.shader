Shader "SF/SDF/ 3D"
{
    // This shader uses the Lambert technique to calculate lighting with Vertexes for simple lighting.
    // It only uses the Main Light, most cases a directional light.  
    // It takes into consideration the shadows caused by the Main light while supporting shadow cascade levels.
    Properties
    {
        _SDFColor("SDF Color Fill", Color) = (1,0,0,1)
        _SDFRadius("SDF Radius", float) = 1
        _SDFPosition("SDF Position", Vector) = (0,0,0)
        _RaySteps("Ray Steps", float) = 64
        _RayStepAmount("Ray Step Amount", float) = 0.02
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
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/shatter-fantasy.sf-utilities/Runtime/Rendering/HLSL/Ray Marching/RayMarchingFunctions.hlsl"

            CBUFFER_START(UnityPerMaterial)
            half4 _SDFColor;
            float3 _SDFPosition;
            float _SDFRadius;
            float _RaySteps;
            float _RayStepAmount;
            CBUFFER_END
            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD2;
                float3 viewDirection : TEXCOORD3;
            };


            Varyings vert (Attributes IN)
            {
                Varyings OUT;

                // Get and cache the Vertex Positions from the input so we can use it in multiple places in the pass.
                VertexPositionInputs positions = GetVertexPositionInputs(IN.positionOS.xyz);

                // Convert the position from object space to homogeneous clip space.
                OUT.positionHCS = positions.positionCS;
                OUT.positionWS = positions.positionWS;
                OUT.viewDirection = GetCameraPositionWS() - positions.positionWS.xyz;
                return OUT;
            }

            
            bool RayMarchHit(float3 position, float3 direction)
            {
                SDFSphere sphere = { _SDFRadius, _SDFPosition.xyz};
                
                for (int i = 0; i < _RaySteps; i++)
                {
                    if (sphere.SignedDistance(position) <= 0 )
                        return true;

                    position += direction * _RayStepAmount;
                }

                return false;
            }
            
            half4 frag (Varyings IN) : SV_Target
            {
                half4 color = float4(1,1,1,1);
                
                if (RayMarchHit(GetCameraPositionWS(),GetViewForwardDir()))
                    color = _SDFColor;
                
                return  color;
            }
            ENDHLSL
        }
    }
}
