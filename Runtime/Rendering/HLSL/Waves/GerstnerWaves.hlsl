#ifndef SF_GerstnerWaves
#define SF_GerstnerWaves

/* Gerstner Waves also commonly known as Trochoidal waves */

#include "WavesCore.hlsl"

/*  Side note for how to create whirlpools.
    You can take the x / z value and use arcsin(angle theta) / arccos(angle theta) to
    find the distance position away from the angle of the center than time that half 2 by
    the radius size of the whirlpool to make a twist. Take a depth value that increases the closer
    it gets to the center of th x/z plane non-linearly to get the y value depth of the swirl at certain positions. 
 */

/*   Gerstner Wave formulas

    Symbol meanings:
    
    (x,y) = (a,b) =  (a,b) is called Lagrangian coordinates = the coordinates system used for fluid dynamics.
    Fun side note: Lagrangian coordinates can be used to make flow fields.
    a = origin position on x-axis.
    b = origin position on y-axis.
    
    bs below is a non-positive constant for the free surface line at certain y positions of the wave.
    k = wave number = 2pi / wavelength
    k reciprocal = 1 / (2 pi) - This can be used when calculating c when missing other values.
    (exp(kb) / k) = use simplified c for phase speed instead = constant speed of fluid.
    c = phase speed = wave speed = square root of (g/k) = sqrt(g/k)
    c also = (reciprocal of k basically here) sqrt(g * wavelength / 2pi)
    g = (not negative) 9.81 for our use cases = gravity force - This is how to calculate the moon effecting low/high tides.
    lambda symbol = upside down y = wavelength
    t = time = in Unity we use _Time.y
    
    Original formula before simplifying for computer calculations
    Note this is for getting the x and y positions. For 3D coordinates there is a dot product involved.
    This formula is just shown to help make it easier to understand the 3D formula.

    f(X) = a + (exp(kb) / k ) * sin(k(a + ct))  
    f(Y) = b + (exp(kb) / k ) * sin(k(b + ct))

    Choices to simplify it:
    
    Choice one replace exp(kb) / k with c for phase speed.
    f(X) = a + sqrt(g/k) * sin(k(a + ct))  
    f(Y) = b + sqrt(g/k) * sin(k(b + ct))

    Choice two:
    Use this if you want to pass in the desired phase speed/wave speed
    via setting it from script/material inspector

    f(X) = a + c * sin(k(a + ct))  
    f(Y) = b + c * sin(k(b + ct))

    Wave Crests: The highest point of a wave
    Rotational trochoidal waves highest crest angle is 0, which is what is generally used for calculations.
    Stoke Wave has a crest angle of 120

    Wave Trough: The lowest point of a wave.
    Wave Height: The distance from the trough to the crest.

    Example drawing a straight line through the center of a wave is just b.
    Wave Height = H = 2/k * exp(kbs)

    Wave Period (Time) = T = wavelength (lambda symbol) / phase speed/
    
*/

/* Returns the wave position*/
half2 GerstnerWaves2DSimple(WaveDataSimple waveData, WaveVertexData vertexData)
{

}

#endif