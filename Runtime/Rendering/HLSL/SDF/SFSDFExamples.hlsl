#include "SFSDFUtilities.hlsl";

/* Abbreviation defintions
    p = point - point in some situation is a built-in keyword. Can cause problems so we shorthand it to p.
    Dst/dst = distance


    Words with national dependent spelling: 
    centre - some nations and languages use centre instead of center. I will use center though.
*/

/* SDF Scenes and techniques
    
    Scene/scene =  In Raymarching when using SDFs, a scene represents the set of render targets
    (models, SDFs, and so forth) being rendered.

    Note in most raytracing the calculations are done backwords.
    Instead of going from the light to the objects and bouncing off to the camera.
    They go from the camera to the object to the light.
    This is way cheaper because you only calculate the ray marching hitting the camera.
    Doing it from the light first will calculate bounce light, reflections, and shadows even out of camera views.
    
    Note some forms limit it to what is rendered on the current camera screen aka what is visible.
    If you ever heard of Screen Space Reflection, then you already know of an example of this.
    The weaknes of this if you have an object lightly offscreen with light hitting,
    it won't create a reflection because only objects on screen are taken into consideration.
    So even if the light reflection would be bounced into the camera view from outside the scene it won't show.
    
    NVidia Ray Tracing is a technique that takes into consideration (depending on game settings) things off-screen.
    This is why it looks somewhat better, but also more expensive to calculate.
    Another reason is it also checks for indirect lighting with physical accurate reflections.
    Combine the three: 1. Off-screen calculations, indirect lighting, and physical accurate reflections
    and you get why NVidia Raytracing is decent looking, but stupid expensive on the GPU.
      
 */


// Example Data structs for SDF calculations.
// You would normally set this from CPU code side or a compute shader.
// In Unity C# using GraphicsBuffer API or the Unity 6 Render Graph BufferHandle API. 
struct CircleData
{
    float2 Center;
    float Radius;
};

struct BoxData
{
    float2 Center;
    float HalfSize;
};

// Grouped shared is an array structure for thread-group-shared memory.
// The 8*8*1 would be the thread ids used in a Compute Shader.
// These can not be declared as a local variables
groupshared CircleData circles[8*8*1];
groupshared BoxData boxes[8*8*1];

/* Example compute shader function on how you would do a RayMarch sweep in a Scene
 * You would have an array of your SDFs and go through a loop to check the point against them.
 * The point in cases of doing something like Ray Tracing for reflections is the starting point for the path of light.
 * The light bounces off reflective material and that bounce point becomes the new point to start from.
 * Then the point is given a new view direction to point toward the angle of reflection.
 * 
 * Below is a link to a video that shows this off visually.
 * Note that video is not showing off things like groupshared types or custom structs types for multithreading.
 * I am implementing support in my shaders for that.
 * https://youtu.be/Cp5WWtMoeKg?t=74
 */
float SignedDstToSceneCompute2D(float2 p, float maxDst = 50)
{
    // Example amount of boxes
    int numCircles = 8*8*1;
    int numBoxes = 10;
    
    // Normally you would have the MaxDst as a property you can change from the inspector in Unity.
    // The most common thing to do is make it equal to or less than your camera far view.
    // This prevents extra calculations on objects not in the render view saving performance.
    float dstToScene = maxDst;

    for (int i =0; i < numCircles; i++)
    {
        float dstToCircle = SignedDstToCircle(p,circles[i].Center,circles[i].Radius);
        dstToScene = min(dstToCircle,dstToScene);
    }

    for (int i = 0; i < numBoxes; i++)
    {
        float dstToBox = SignedDstToBox(p,boxes[i].Center,boxes[i].HalfSize);
        dstToScene = min(dstToBox,dstToScene);
    }

    return dstToScene;
}

// Here are examples of doing boolean modifiers for subtracting, adding, or only including overlapping SDFs.

/* Example for showing off rendering an SDF only when two other SDF are intersecting with each other.
 * Note this works for 3D objects as well. This can be used to create a cave in a terrain mesh, sculpting meshes,
 * VFX SDF caches (Unity's VFX graph support this actually out of box), and to create a 3D Volumetric Cloud System that can be moved around.
 *
 * For the cloud system you can think of a smokescreen from a smoke grenade (think new CS:GO 2 update for example)
 * and move through it to move the smoke around.
 * 1. You use the Character's model and build an SDF to represent its shape.
 * 2. Do a reverse SignedIntersectionDstToScene and cull out the smoke and displace it where the value is below 1.
 * This is called reverse because you don't render the stuff below zero where normally you only render stuff below zero.
 *
 * See reference video at link below for example.
 * https://youtu.be/Cp5WWtMoeKg?t=166
 */

float SignedIntersectionDstToScene2D(float2 p, BoxData box, CircleData circle)
{
    float dstToBox = SignedDstToBox(p,box.Center,box.HalfSize);
    float dstToCircle = SignedDstToCircle(p,circle.Center,circle.Radius);

    /* This works because calculations returns a -1 when the point being tested is inside the shape of an SDF.
     * If the point is not inside either the circle or the box than one of them will return at least a zero or higher value.
     * If one returns negative aka the point is in it and the other values returns 0 or more saying it is not in the other shape.
     * Then the max between them will always be 0 or higher. Thus returning a value to not be rendered for an SDF. 
    */
    return max(dstToCircle,dstToBox);
}