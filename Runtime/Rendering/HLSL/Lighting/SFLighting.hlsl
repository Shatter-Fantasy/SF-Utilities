#ifndef SF_LIGHTING
    #define SF_LIGHTING

#include "Packages/shatter-fantasy.sf-utilities/Runtime/Rendering/HLSL/Lighting/SFLIghtingData.hlsl"

// Need these for supporting shadows.
#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN

void InitialiizeInputData(SFLightingData lightingData,out InputData inputData)
{
    inputData = (InputData)0; // This avoids any chances of "not completely initialized" errors.

    inputData.positionWS = lightingData.positionWS;

    // TODO: Normal map ifdef checking here.

    // When no normal map defined
    //half3 viewDirWS = half3(lightingData.normalWS.w, lightingData.tangentWS.w, lightingData.bitangentWS.w);
    
}
#endif

