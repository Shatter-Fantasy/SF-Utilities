Shader "SF/SF 2D SDF Unlit"
{
    // This is Shader is to create 2D SDF for screen space.
    // For 3D SDF use the SF 3D SDF Shaders.
    Properties
    {
        _MainColor ("Main Color", Color) = (1,1,1,1)
        
        _Rotation("Rotation", float) = 0
        _OffSet("Offset", float) = 0
        
        _Radius("Radius", float) = 1
        _EdgeOutlineWidth("Edge Outline Width", float) = 0.25
        _EdgeOutlineColor("Edge Outline Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags {  "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            BlendOp Add
            ZWrite Off
            
            Name "SDF Forward"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/shatter-fantasy.sf-utilities/Runtime/Rendering/HLSL/Math/SFMathUtilities.hlsl"
            #include "Packages/shatter-fantasy.sf-utilities/Runtime/Rendering/HLSL/SDF/SFSDFUtilities.hlsl"

            // SRP Combpatability
            CBUFFER_START(UnityPerMaterial)
            half4 _MainColor;
            float _Rotation;
            float _Radius;
            float _EdgeOutlineWidth;
            half4 _EdgeOutlineColor;
            float _OffSet;
            CBUFFER_END
            
            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionOS : TEXCOORD0;
            };
            
            Varyings vert (Attributes IN)
            {
                Varyings OUT;

                // Get and cache the Vertex Positions from the input so we can use it in multiple places in the pass.
                VertexPositionInputs positions = GetVertexPositionInputs(IN.positionOS.xyz);
                // Convert the position from object space to homogeneous clip space.
                OUT.positionHCS = positions.positionCS;
                // We use Object Space so the SDF doesn't have to be centered on the screen for correct calculations.
                // With ObjectSpace we do a swizzle switching the z and y value we can now use a rotated plane facing the camera for the SDF Element.
                // This also makes it where rotating the SDF won't cause artifacts. Now we can have rotated SDF partially looking at the camera.
                OUT.positionOS = IN.positionOS.xzy;
                return OUT;
            }

            // This gives better results if we are in the fragment shader.
            // Most of the time lower vertex count objects don't give smooth SDF shapes.
            float scene(float2 position, float2 offset = 0)
            {
                // Rotate before translating or the translation will cause the rotation to be off.
                float2 originPos = position;
                originPos = SDF_Rotate2D(originPos,_Rotation);
                originPos = SDF_Translate2D(originPos,offset);
                
                float sceneDistance = SDF_Circle2D(originPos,_Radius);

                originPos = SDF_Translate2D(originPos, -offset);
                float dstBox = SDF_Rectangle2D(originPos, _Radius); 
                
                sceneDistance = SFSmoothMinDistanceCubic(sceneDistance,dstBox,1);
                return sceneDistance;
            }
            
            
            half4 frag (Varyings IN) : SV_Target
            {
                float dist = scene(IN.positionOS.xy, _OffSet);

                // If the fragment being checked is outside the shape discard it for better performance.
                if (dist > _Radius + _EdgeOutlineWidth)
                    discard;

                half4 col = half4(dist,dist,dist,1);
                
                if(dist > _Radius) // If we are out of the SDF, draw the outline color.
                    col = _EdgeOutlineColor;
                else
                    col = _MainColor;

                return col;
            }
            ENDHLSL
        }
    }
}
