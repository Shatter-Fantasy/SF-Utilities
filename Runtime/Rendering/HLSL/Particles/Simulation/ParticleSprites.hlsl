/* PS in the function names stands for Point Sprites 
   not Particle Sprites like the name of this HLSL file. 
   Point Sprites are used in many different things.
   Example usage create Screen Space Fluid Rendering without meshes. 
   Think liquid physics with splash and foam spray.
   Link below gives a shader run down of using Points Sprites to do liquid rendering.

    The shader looks similar, but is slightly different from 
    the linked article due to each engine having 
    slight syntax differences. 

   https://developer.download.nvidia.com/presentations/2010/gdc/Direct3D_Effects.pdf
*/

void ParticleSpherePS_float(
    float2 TexCoord0, 
    float3 EyeSpacePos, 
    float3 LightDirection,
    float SphereRadius,
    float4 Color,
    out float3 Result,
    out float Depth
) {
    
    // Calculate the eye-space sphere normal from texture coordinates
    float3 eyeSphereNormal;
    /*  TexCoord0 (aka uv) * 2.0 - 1.0 is a trick to make (0,0) the center of the uv texture.
        This allows going from a negative number to positive number which can be useful for 
        dot and cross product calculations
        
        Note we use 2.0 and 1.0 instead of 2 and 1 because this makes them start as floats.
        If they start as float values there is no type casting from int to float needed.
        This saves performance. */
    eyeSphereNormal.xy = TexCoord0 * 2.0 - 1.0;
    
    /*  Doing the dot product of a value with the same value gives 
        a radius in the center of the shape.
    
        This allows us to get a radius from the center for culling out viewing angles.
        Example the Fresnel Effect is calculated this way a good amount of times.
        Only different is Fresnel Effect also normalizes the values first than saturates the
        dot product into a pow formula.
        https://docs.unity3d.com/Packages/com.unity.shadergraph@17.0/manual/Fresnel-Effect-Node.html
        */
    float r2 = dot(eyeSphereNormal.xy, eyeSphereNormal.xy);
    
    // Kills pixels outside the circle to save performance and prevent alpha calculations.
    if (r2 > 1.0) 
        discard;
    
    /* TODO: Apologies I haven't done this part too much so will update the comment here with better documentation.
       But it works great, so oh well.*/
    
    /* What I figured out so far.
    
        Using the previous calculated Sphere Point radius we set the eye 
        sphere normal z position to see what particles are possible facing away fromt he camera.
        
        Think this is what starts the guarantee that the particles are always facing the camera 
        making them Screen Space rendered. 
        Note below we have to also apply a Matrix TRS to go to 
        screen space for the final calculations.
    
        Because we discard any pixels above that is above r2 value of 1,
        we never have a pixel calculation that causes this to calculate a negative sqrt number.
        So no worries about getting an imaginary number below.    
    */
    eyeSphereNormal.z = -sqrt(1.0 - r2);
    
    
    // Depth Calculation Start
    
    // First calculate the position of the pixels we will be rendering the Point Sprite to.
    /*  Make sure you pass in the EyeSpacePos that is passed into this function as the first parameter
        and not the previously calculated eye normal. 
        You use the eyenormal as the second part of the formula. 
    
        Again use 1.0 not 1 to prevent wasting performance doing type casting from int to float.
    */  
    float4 pixelPos = float4(EyeSpacePos + (eyeSphereNormal * SphereRadius), 1.0);
    
    /*  Second calculate the clip space pos of the current projection matrix.
    
        The UNITY_MATRIX_P is a built in Unity macro injected when a shader is compiled.
        If using this function to create a custom shader graph node enable the  use pragmas option on 
        the custom node settings inspector.
    
        To use this function in a shader HLSL file you have to use the Unity include file that Unity 
        has for it's built in shader variables.
        https://docs.unity3d.com/6000.0/Documentation/Manual/SL-UnityShaderVariables.html
    */
    float4 clipSpacePos = mul(pixelPos, UNITY_MATRIX_P);
    
    /*  TLDR w can be used as the scale of rendered objects in clip space relative 
        to the distance of the projection. 
    
        More information
        clipSpacePos.w = 1 means nothing scales smaller or bigger based on distance.
        2D devs might have a lightbulb go off thinking wait orthographic view where distance from 
        cameras don't affect Sprites. Unity, Godot, and couple others engines use this trick.
    
        For those wanting to know about what the w stands for, here is an article
        about clip space and NDC space relationship with the viewport.
       https://carmencincotti.com/2022-05-02/homogeneous-coordinates-clip-space-ndc/    
    */
    Depth = clipSpacePos.z / clipSpacePos.w;
    
    // Get the color difuused lighting value based on the direction of the light
    // and the viewing angle from the Point Sprites eye normal.
    float diffuse = max(0.0, dot(eyeSphereNormal, LightDirection));

    // Multiply the calculated diffuse adjustement and the color we want the particles to be.
    // That is it.
    Result = diffuse * Color;
}