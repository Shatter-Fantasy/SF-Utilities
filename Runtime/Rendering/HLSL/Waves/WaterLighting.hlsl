#ifndef SF_Water_Lighting
#define SF_Water_Lighting

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

/////////             Lighting Calculations Section              ///////
half CalculateFresnelTerm(half3 normalWS, half3 viewDirectionWS, float distance)
{
    // F_Schlick is part of the BDSF hlsl include that the Lighting.hlsl file includes for us.
    // It stands for Fresnel Schlick aka Schlick's approximation getting the Frsnel Factor in the specular lighting reflection between two surfaces.
    const half fresnel = F_Schlick(0.02, dot(normalWS, viewDirectionWS));
    return fresnel * (1 - saturate((half)distance * 0.005) * 0.5);
}

// TODO: Specular Lighting
#endif