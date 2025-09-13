/*  This file includes commone data structs, functions, and properties for different types of Waves
    This includes more than just waves for water. Some properties can be used for other types of waves as well.
*/

/*   A smaller, more performant, weight set of data for calculating different waves.
 *   This holds less data needed for waves. The data not stored in this struct would be calculated
 *   in most shaders or on the CPU side and set via material properties/structured buffers
 */
struct WaveDataSimple
{
    // Static variables during runtime.
    half WaveLength; // The lambda symbol in math formulas
    half Amplitude; // The distance to the crest from the center / height of the wave from the center.
    half PhaseSpeed; //The c in most math formulas.
    half2 OriginPosition; // The starting point of a wave to be calculated for. // Don't need Z here. It will be calculated in methods. 
};

struct WaveVertexData
{
    half3 positionWS; // Vertex calculated positions in world space
    /*  Required Lighting Data */
    half3 normalWS; // Normal for vertexes in world space.
    /*  Tangents for the vertexes in world space with the w value being used for bi-normals when needed.
        Tangents are used for bump maps in 3D models, but we use them for adding details to waves. */
    half4 tangentWS; 
};

WaveDataSimple DefaultWaveDataSimple()
{
    WaveDataSimple data = (WaveDataSimple)0;

    data.WaveLength = 1.f;
    data.PhaseSpeed = .5f;
    data.OriginPosition = half2(0.f,0.f);

    return data;
}

WaveVertexData DefaultWaveVertexData()
{
    WaveVertexData data = (WaveVertexData)0;

    // Set the normals to point upwards because plans are commonly used for wave shaders.
    data.normalWS = half3(0.f,1.f,0.f);
    // Setting the tangent to be pointed positive right for the normal U texture coordinates.
    // The fourth value is used for to calculate the binormal. It has to be 1 or -1. Nothing else.
    data.tangentWS = half4(1.f,0.f,0.f,1);

    return data;
}