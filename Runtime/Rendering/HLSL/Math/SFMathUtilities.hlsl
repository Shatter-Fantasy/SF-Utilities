#ifndef SF_MATH_UTILITIES
    #define SF_MATH_UTILITIES

/* Abbreviation defintions
    p = point - point in some situation is a built-in keyword. Can cause problems so we shorthand it to p.
    Dst/dst = distance


    Words with national dependent spelling: 
    centre - some nations and languages use centre instead of center. I will use center though.
*/

/*  Credits
    Benoit Mandelbrot (father of Fractal Geometry) - Mathematician that discovered the 2D The Mandelbrot Set formula
    Inigo - For a lot of math formulas explanations being used.
    Sebastion - Gave a lot of links to research papers I learned from and shared some example math formulas I tweaked a bit.
    Debrun - Lot of math used for simulating particles.  
 */

/*  By doing constants of certain math values instead of doing the division we can improve performance.
    Division is more intensive in most cases.

    Normal values use five decimals of precision.
    Accurate values use nine decimals for precision.
*/ 

static const float SF_PI = 3.1459;
static const float SF_PI_Accurate = 3.141592653589;

static const float  SF_ONE_SIXTH = 0.16667; // same as 1/6 rounded early for better performance.
static const float  SF_ONE_SIXTH_ACCURATE = 0.166666667; // same as 1/6 rounded early for better performance.

/* Smooth Min documentation and notes
 * This works for all SmoothMin function below.
 * 
 * Combines the two values by blending and melting them together, if they are close enough.
 * Gets the minimum of two values and then smooths them out at the upper limit.
 *
 * Main Usage:
 * This is used to take mainly a set of values that make two different curves.
 * It gets the minimum of each curve at an index and uses the min of those two values.
 * After getting the minimum value it blends them.
 * 
 * The end result makes a new curve where at the top of each peak of the curve
 * it is smooth instead of straight jagged edges.
 *
 * Real World Usages:
 * 1. Real World Physical Fluid Simulation...yes, this is useful for it. Read below for more information.
 * 2. Blender sculpting tools.
 * 3. Terrain brushes (think Unity)
 *
 *  Real World Fluid Simulation Section:
 *  Smooth min is useful for the visual rendering part of fluid simulation. 
 *  Some fluid simulation techniques use SDF to calculate the neighbor particles to see if they need to apply surface tension and more to them.
 *  By using a smooth min you on the same SDF you can blend neighbor particles together from looking like individual particles into a collection of simulated fluid.
 *  Works really nice with compute shaders to make real looking and acting water physics without losing performance in games. 
 */

/* SmoothMin using a cubic formula without a pre-defined multiplier amount.*/
float SFSmoothMinDistance(float dstA, float dstB, float k)
{
    float h = max(k-abs(dstA-dstB), 0) / k;
    return min(dstA,dstB) - h * h * h * k * SF_ONE_SIXTH;
}

/* SmoothMin using a cubic polynomial formula.*/
float SFSmoothMinDistanceCubic(float dstA, float dstB, float maxBlendThickness)
{
    // In math formulas you will see the letter k which is the maxBlendThickness.
    // This thickness is added to both values being checked not just the overall thickness.
    maxBlendThickness *= 6;
    float h = max(maxBlendThickness-abs(dstA-dstB), 0.0) / maxBlendThickness;
    return min(dstA,dstB) - (h * h * h * maxBlendThickness * SF_ONE_SIXTH);
    
    /* The math for h calculation and the return comes out to this:
        Think of this as an alternative to the cubic polynomial function:
        (x * x * x) + b (x * x) + c * x + d
     
        float x = (b-a)/k;
        float g = (x> 1.0) ? x :
              (x<-1.0) ? 0.0 :
              (1.0+3.0*x*(x+1.0)-abs(x*x*x))/6.0;
        return b - k * g;     
     */
}
/* Spiky Kernel Derivatives */

/*  Radius 4 is usually the radius to the power of four.
    It is just faster to precompute or multiply the value four times instead of using the pow math function. */
float SpikyKernelFirstDerivative(float distance, float radius)
{
    float x = 1.0f - distance/ radius;
    return -45.f/ (SF_PI * radius * radius * radius * radius) * x * x;
}
float SpikyKernelFirstDerivative(float distance, float radius, float radius4)
{
    float x = 1.0f - distance/ radius;
    return -45.f/ (SF_PI * radius4) * x * x;
}

float SpikyKernelSecondDerivative(float distance, float radius)
{
    float x = 1.0f - distance/ radius;
    return 90.f/ (SF_PI * radius * radius * radius *  radius * radius) * x;
}
float SpikyKernelSecondDerivative(float distance, float radius, float radius5)
{
    float x = 1.0f - distance/ radius;
    return 90.f/ (SF_PI * radius5) * x;
}

float SpikyKernelGradient(float distance, float3 direction,float radius)
{
    return SpikyKernelFirstDerivative(distance,radius) * direction;
}
float SpikyKernelGradient(float distance, float3 direction,float radius, float radius4)
{
    return SpikyKernelFirstDerivative(distance,radius,radius4) * direction;
}

/* Fractal Magic Documentation:
 * I am still learning some of these, so I don't fully get the math behind all of them yet.
 * Some might be missing documentation on steps.
 */

/* A 3D representation of the Mandelbrot Set. It creates an infinitely complex, naturally occurring fractal object.
 * The 2D Mandelbrot Set was discovered by Benoit Mandelbrot.
 *
 * Mandelbrot Set formula: z = (z * z) + c
 * in a recursive logic loop that the next iteration takes the previous calculated value as the starting z
 * for the new z to calculate the new iteration from.
 */
float SFMandelBulb(float3 position, float power, int iterations)
{
    float3 z = position;
    float dr = 1;
    float r = 1;

    // Modified Mandelbrot Set formula to add calculations with angles.
    for (int i = 0; i < iterations; i++)
    {
        r = length(z);
        if (r > 2)
            break;

        float theta = acos(z.z / r) * power;
        float phi = atan2(z.y,z.x) * power;
        float zr = pow(r,power);
        dr = pow(r,power - 1) * power * dr + 1;

        z = zr * float3(sin(theta) * cos(phi), sin(phi) * sin(theta), cos(theta));
        z += position;
    }

    return 0.5 * log(r) * r / dr;
}
#endif
