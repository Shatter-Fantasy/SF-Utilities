#ifndef SF_Water_Input
#define SF_Water_Input

half _WaveHeight;

struct Attributes
{
    float4 positionOS : POSITION;
    float2 texCoord : TEXCOORD0; // Local tex coordinates will be ued for detail maps.
    UNITY_VERTEX_INPUT_INSTANCE_ID // This comes from Unity's UnityInstancing.hlsl file.
};

struct Varyings
{
    float4 uv : TEXCOORD0;
    float3 positionWS : TEXCOORD1;
    float3 normalWS : NORMAL; // Vertex normals
    float4 viewDirectionWS : TEXCOORD2; // View direction
    half2 fogFactorNoise : TEXCOORD4;
    float4 screenPosition  : TEXCOORD6;
    float4 positionHCS : SV_POSITION;
    // These come from Unity's UnityInstancing.hlsl file.
    UNITY_VERTEX_INPUT_INSTANCE_ID 
    UNITY_VERTEX_OUTPUT_STEREO
};

struct WaterInputData
{
    float3 positionWS;
    float3 normalWS;
    float3 viewDirectionWS;
};

struct WaterSurfaceData
{
    half foamMask;
};

struct WaterLighting
{
    half3 lightAmount; // Store the lighting data at the current vertex coordinate.
    half3 shadow; 
};

#endif