#ifndef SF_SDF2D_UTILITIES
    #define SF_SDF2D_UTILITIES

/* For examples look at the SFSDFExamples.hlsl file located at:
 * Packages/shatter-fantasy.sf-utilities/Runtime/Rendering/HLSL/SDF/SFSDFExamples.hlsl
*/

/* Abbreviation defintions
    p = point - point in some situation is a built-in keyword. Can cause problems so we shorthand it to p.
    Dst/dst = distance


    Words with national dependent spelling: 
    centre - some nations and languages use centre instead of center. I will use center though.
*/

// SFMathUtilities has helper functions and some constants predefined for better performance.
// This includes SF_PI, 
#include "Packages/shatter-fantasy.sf-utilities/Runtime/Rendering/HLSL/Math/SFMathUtilities.hlsl"


float SDF_Circle2D(float2 origin, float radius)
{
    return length(origin) - radius;
}

float SDF_Rectangle2D(float2 origin, float2 halfSize)
{
    float2 edgeDistance = abs(origin)  - halfSize;
    float outsideDistance = length(max(edgeDistance,0));
    float insideDistance = min(max(edgeDistance.x,edgeDistance.y),0);
    return outsideDistance + insideDistance;
}

/* Gets the distance from the point to the center of a circle minus the circle's radius.
 * Returns negative when the point is inside the center radius.
 * Returns positive when outside the circle radius.
 * This is just a shorthand for an SDF that returns negative
 *
 * Usages: Ray marching and SDF methods.
 */
float SignedDstToCircle(float2 p, float center, float radius)
{
    return length(center - p) - radius;   
}


/* Gets the distance to the center of a box/rectangle
 * Returns negative if inside the rectangle and positive if outside of it.
 */
float SignedDstToBox(float2 p, float2 center, float2 size)
{
    float offset = abs(p-center) - size;
    // Distance from point outside the box to the edge. Returns 0 if inside the box.
    float unsignedDst = length(max(offset,0));
    // Negative distance from point outside the box to the edge. Returns 0 if inside the box.
    float dstInsideBox = max(min(offset,0),0);
    
    return unsignedDst + dstInsideBox;
}

/* fullRotations is a number mapped between 0 and 1. 90 = .25 180 = .5, ect.*/
float2 SDF_Rotate2D(float2 p, float fullRotations)
{
    float angle = fullRotations * SF_PI * 2 * -1;
    float sine, cosine;
    sincos(angle, sine, cosine);
    return  float2(
        (cosine * p.x + sine * p.y),
        (cosine * p.y - sine * p.x)
    );
}

/* Tricks with scaling.
    Trick 1 Pulse Effect:
    Pass in a _Time.y * SDF_PI to make the SDF do pulse like heart beat.
    See SDF_PulseScale
*/
float2 SDF_Scale2D(float2 p, float scale)
{
    return  p / scale;
}

// Note for custom edge/outline widths.
// If you are scaling and then adding an outline always make sure to take into account the outline width. 
float2 SDF_PulseScale(float2 p, float scale)
{
    return SDF_Scale2D(p,scale + 0.5 * sin(_Time.y * SF_PI));
}

float2 SDF_Translate2D(float2 p, float2 offset)
{
    return  p - offset;
}
#endif