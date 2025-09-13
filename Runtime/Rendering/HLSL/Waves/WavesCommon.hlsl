#ifndef SF_Wave_Common
#define SF_Wave_Common
/*  The functions in this hlsl file are used for helping set up different types of waves.
    They mainly include calculations for wavelength (wave lambda), wave value (repetency of frequency), and stuff to help with Undulation. 
 */

/*  Symbol definitions

    All letters have a space between them unless two letters are a single symbol
    Example pv below is not the same as pv.
    w = angular frequency
    w = 2 * pi * v

    v = Spectroscopic Wavenumber

    vp = phase velocity
    

    k = wave number for wave equations
    k = 2 * pi / wavelength
    k = w / phase velocity same as = k = (2 * pi * v) / pv
    
    lambda symbol aka upside down y = Wavelength
 */

/*  Key terms and information
    Traverse Waves = Water waves that are ripples of gravity waves. This is what we are calculating for.

    Wave Properties:

    Amplitude = height of the wave. Directly related to the amount of energy in a wave.

    Wavelength = distance between identical points in the adjacent cycles of crests of a wave.

    Period = the time for a particle on a medium to make on complete cycle measured in time usually.
    Period is the reciprocal of the frequency and vice verse
    Period = 1 / frequency.
    
    Frequency = the number of waves passing through a point in a certain time.
    Frequency is the reciprocal of the period and vice verse
    Frequency = 1 / period.

    Speed = how fast the waves is travelling per a unit of time.
    Speed for waves is usually the distance travelled by the crest of a wave at a given point.
    Speed = distance / time
 */

#ifndef SF_MATH_UTILITIES
float SF_PI = 3.1459;
#endif

// This how data injected by Unity from C#/C++ side.
// Including stuff like _Time.y
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/shatter-fantasy.sf-utilities/Runtime/Rendering/HLSL/Waves/WaterLighting.hlsl"
#include "Packages/shatter-fantasy.sf-utilities/Runtime/Rendering/HLSL/Waves/WaterInput.hlsl"
#include "Packages/shatter-fantasy.sf-utilities/Runtime/Rendering/HLSL/Math/SFNoiseUtilities.hlsl"

/*  Setting this for only certain types of water shaders.
    It makes the compiler set a static value for certain variable declaration.
    This makes certain things not animated that check _Time.y;
    Also will make performance better for some things and allow us to stop certain things from moving like Caustics. */
#if defined(_STATIC_SHADER) 
#define WATER_TIME 0.0
#else
#define WATER_TIME _Time.y
#endif

#define DEPTH_MULTIPLIER 1 / _MaxDepth

// Precomputed wave property values.
CBUFFER_START(UnityPerMaterial)
half _Amplitude;
half _WaveLength;
half _Frequency;
half _Speed;
half _WaveNumber;
half _WavePhase;
CBUFFER_END
// End of Precomputed wave property values. 

half3 _WavePosition;

half3 _WaveTangent;
half3 _WaveNormal;
/*  Wavenumber is the repetency which is the frequency of a wave measured in cycles per distance for ordinary wave
    numbers or radians per unity for angular wave numbers.
    how many cycles of a wave is completed over its wavelength.
*/

/*  These functions should only be used if you are not pre-computing the Wave Properties 
    This means calculating them once on C# side and setting the property when first setting up the water object.
    That way you only have to calculate them a single time at the start of a scene.
    If you don't than you would have to recalculate them every GPU cycle which wastes performance. 
*/
inline void WaveNumberRadian()
{
    _WaveNumber = 2 * 3.1459 / _WaveLength;
}

// Scalar Wave length is just the inverse of the wavelength
inline void WaveNumberScalar()
{
    _WaveNumber = 1 / _WaveLength;
}

/////  End of properties that should be precomputed. /////

inline void WaveX(float positionX)
{
    _WavePosition.x =  _WaveNumber * (positionX - _Speed * _Time.y);
}

// The calculated value of the Wave's Crest when taking into consideration current position x and the amplitude.
inline void WaveY()
{
    _WavePosition.y = _Amplitude * sin(_Frequency);
}

inline void WaveDerivativeX()
{
    /*  Formula is as following:
        Direction of wave x * cos( )
        
     */
}
inline void WaveFrequency()
{
    _Frequency = 2 / _WaveLength;
}

inline void WavePhase()
{
    _WavePhase = _Speed * 2 / _WaveLength;
}

inline void WaveTangent()
{
    _WaveTangent =  normalize(half3(1, _WaveNumber * _Amplitude * cos(_Frequency),0));
}

///Make sure you calculate the tangent before calling WaveNormal.
inline void WaveNormal()
{
    _WaveNormal = half3(-_WaveTangent.y, _WaveTangent.x,0);
}

inline void WaveTangentAndNormal()
{
    _WaveTangent =  normalize(half3(1, _WaveNumber * _Amplitude * cos(_Frequency),0));
    _WaveNormal = half3(-_WaveTangent.y, _WaveTangent.x,0);
}

inline void WaveEquation(half3 positionOS)
{
    _WavePosition =  positionOS;
    WaveNumberRadian();
    WaveFrequency();
    WavePhase();
    WaveTangentAndNormal();
    _WavePosition.y =  _Amplitude * sin( (_WavePosition.x * _Frequency) + (_Time.y * _WavePhase) );
}

void InitializeInputData(Varyings input, out WaterInputData inputData ,float2 screenUV)
{
    // Remove this line after finishing. This just prevent an error while making it saying we havcen't initialized everything yet.
    inputData = (WaterInputData)0;

    // TODO: Create some form of noise or get a sampled texture for it.

    // TODO: Find a way to grab depth better.

    // TODO: Sample textures from buffers
    
    inputData.positionWS = input.positionWS;
    
    inputData.normalWS = input.normalWS;
}

void InitializeSurfaceData(WaterInputData input, out WaterSurfaceData surfaceData, float4 packedData)
{
    surfaceData = (WaterSurfaceData)0;
}
// Using a float4 to pack multiple sets of data into one variable for better performance. 
half3 WaterShading(WaterInputData input, WaterSurfaceData surfaceData,float4 packedData, half2 screenUV)
{
    // Fresnel Effect/Term calculation
    half fresnelEffect = CalculateFresnelTerm(input.normalWS,input.viewDirectionWS, distance(input.positionWS, GetCameraPositionWS())); 
    
    // Lighting
    Light mainLight = GetMainLight(TransformWorldToShadowCoord(input.positionWS), input.positionWS, 1);

    // TODO: VolumeShadow
    // TODO: GI sampling of the SH from ambient probes

    // TODO: SSS
    // SSS: Direct lighting from F_Schlick and pass in the volume shadow with the GI value
    
    /* Specular - Using Lambert Lighting Function
        Lambert Lighting is basically the saturated value of nDotL times the color and brightness/intensity/radiance.

        Lambert Lighting can be calculated as following.
        Step one: dot product of surface (we are using normalWS for this) and the lighting direction normalized most of the time.
        Step Two: use saturaye to prevent the dot product from being out of the normalized value range.
        Step Three: get the radiance by multiplying the color value of the light by the result of the light.distanceAttenuation
        multiplied by the nDotL
        multiplied by the shadow shadowAttenuation

        half3 radiance = mainLight.color * (mainLight.distanceAttenuation * NdotL) * mainLight.shadowAttenuation;
        
        nDotL (normal dot product of light direction) is just do product between the world normal and the light direction.
        Need to make sure this is accurate for this sentence: nDotL is the same as the cos of the angle between angles normalized.
    */
    half3 lightingLambert = LightingLambert(mainLight.color, mainLight.direction,input.normalWS);
    half3 radiance = lightingLambert * mainLight.shadowAttenuation;

    
    // This is part of the BRDF include the lighting.hlsl files has built into Unity.
    // Set up the BRDF Data
    BRDFData brdfData;
    half alpha = 1;
    // Initialize with a temp specular. This allows us to have the data needed for the DirectBRDFSpecular function to get the proper value.
    InitializeBRDFData(half3(0.0, 0.0, 0.0), 0, half3(1, 1, 1), 0.9, alpha, brdfData);
    brdfData.specular = DirectBRDFSpecular(brdfData, input.normalWS, mainLight.direction, input.viewDirectionWS) * brdfData.specular;
    brdfData.specular *= radiance;
    brdfData.specular *= 1 - saturate(surfaceData.foamMask * 2);

    // TODO: Foam

    // TODO: Reflections

    // TODO Refraction.

    // TODO: Compositing part one: relection with fresnel and specular using the calculated sss value.
    // half3 compA = lerp(refraction, reflection, fresnelEffect) + brdfData.specular + sss;
    // TODO: Compositing part two: take composite one and add on the foam mask.

    // TODO: Finally mix fog into the final value and return the hal3 result.
    // half 3 output = MixFog(compB, input.fogCoord); 

    return brdfData.specular;
}

Varyings WaveVertCalculation(Varyings IN)
{
    // Making the geometry plane normals point up at start.
    IN.normalWS = float3(0,1,0);
    
    IN.fogFactorNoise.y = (
        ( noise((IN.positionWS.xz * 0.5f) + WATER_TIME)
        + noise((IN.positionWS.xz * 1)+ WATER_TIME))
        * 0.25 - 0.5 ) + 1;

    // TODO: Figure how the math for detail map should be.
    // The details map will need to have the details for cascades used in it. 
}

/**
 * \brief This is the function that calculates the Water's vertex values.
 * @param IN 
 * @return New Vertex value after doing wave calculations.
 */
Varyings WaterVertex(Attributes IN)
{
    Varyings OUT = (Varyings)0;
    
    // Set up instancing IDs
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, OUT);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

    OUT.uv.xy = IN.texCoord; // Using this for the geometry uvs.
    OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
    
    OUT = WaveVertCalculation(OUT);
    return OUT;
}
#endif