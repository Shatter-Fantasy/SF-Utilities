using System.Runtime.InteropServices;
using UnityEngine;

namespace SF.Utilities.Rendering.Fluid
{
    /* Math Notes:
        We are using Smoothed Particle Hydrodynamics for our fluid simulations.
     */
    
    /// <summary>
    /// Serialized data used in fluid simulation like simulated water, clouds, smoke, and volumetric particles.
    /// </summary>
    [System.Serializable]
    [StructLayout(LayoutKind.Sequential,Size = 44)]
    public struct FluidData
    {
        /* Numbers to the right of the variable declartion is for keeping
         track of the amount of bytes of the struct for the StructLayout attribute on the struct definition.*/
        
        // This needs to line up in the exact same order and variable type as the variable struct in whatever
        // shader you are sending the data into.
        
        public float Pressure; //4 bytes
        public float Density; // 8 bytes
        public Vector3 CurrentForce; // 20 bytes
        public Vector3 Velocity; // 32 bytes
        public Vector3 Position; // 44 total bytes.
    }

    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct SFFluidData
    {
        public float Mass;
        /// <summary>
        /// This is the property we are going to be smoothing out.
        /// This can be density, pressure, viscosity, etc.
        /// </summary>
        public float ScalarValue;
        public Vector3 Position;
        
        /// <summary>
        /// Math function to use to see how much neighbor fluid particles effect the current particle being processed.
        /// </summary>
        public SmoothingFucntion Smoothing;
    }
    public enum SmoothingFucntion
    {
        SmoothMinCubic
    }
}
