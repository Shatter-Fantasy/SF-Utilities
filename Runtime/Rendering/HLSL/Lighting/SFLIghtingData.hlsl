#ifndef SF_LIGHTING_DATA
#define SF_LIGHTING_DATA

// This is the common data struct for all SF lighting shader files.
struct SFLightingData
{
    half3 positionWS; // The world space position for the vertex we are calculating for.
    half4 normalWS; // The world space position of the normals.
    half4 tangentWS; // The world space of the tangents.
    half3 bitangentWS; // The world space opf the bi-tangents.
    
};

#endif