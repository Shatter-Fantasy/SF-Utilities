const int SF_Ray_MaxSteps = 64;
const half SF_Ray_StepSize= 0.01f;

struct SDFSphere
{
    float radius;
    float3 origin; // Center of the sphere.
    
    float SignedDistance(in float3 p)
    {
        return distance(p, origin) - radius;
    }

    // Use this if you want to just to get a signed distance for a sphere.
    // This is marked static to help performance.
    float SignedDistance(in float3 p, in float3 origin, in float radius)
    {
        return distance(p, origin) - radius;
    }
};
