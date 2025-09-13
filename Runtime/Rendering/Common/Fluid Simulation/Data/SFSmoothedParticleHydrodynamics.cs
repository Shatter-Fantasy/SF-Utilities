using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace SF.Utilities.Rendering.Fluid
{
    public class SFSmoothedParticleHydrodynamics : MonoBehaviour
    {
        [Header("General Data")] public bool ShowSpheres = true;
        // Grid of particles for each axis
        public Vector3Int NumToSpawn = new Vector3Int(10,10,10);

        private int TotalParticles
        {
            get { return NumToSpawn.x * NumToSpawn.y * NumToSpawn.z;  }
        }

        /// <summary>
        /// The bounding box size that contains the particles.
        /// </summary>
        /// <returns></returns>
        [SerializeField] private Vector3 _boxSize = new Vector3(4, 10, 3);

        public Vector3 BoxSize
        {
            get { return _boxSize; }
            set
            {
                // If the box size hasn't changed don't bother doing any compute buffer updates for properties.
                if (_boxSize == value)
                    return;

                _boxSize = value;
                SPHComputeShader.SetVector(BoxSizeProperty,_boxSize);
            }
        }
        public Vector3 SpawnCenter;
        public float SpawnOffset = 0.2f;
        public float ParticleRadius = 0.1f;

        /// <summary>
        /// This will be the mesh for spawned particles. This should be as low of poly as possible.
        /// This will make the performance a lot better than having a higher poly.
        /// We will switch this out for the multithreaded mesh system later.
        /// <remarks>
        ///     Word of advice. Get blender make an IcoSphere with less than 3 subdivisions.
        ///     Using that for the low polly model will do wonders for performance. 
        /// </remarks>
        /// </summary>
        [Header("Particle Rendering")] 
        public Mesh ParticleMesh;
        public float ParticleRenderSize = 8f;
        public Material ParticleMaterial;

        [Header("Compute")] 
        public ComputeShader SPHComputeShader;

        // BLOODY IMPORTANT READ THE SUMMARY HERE.
        /// <summary>
        /// This has to match the numthreads property value inside the compute shader, you are going to have a bad time if it doesn't.
        /// The total TotalParticles value must be dividable by the KernelThreadGroups x value.
        /// </summary>
        public Vector3Int KernelThreadGroups = new (100,1,1);
        public FluidData[] Particles;

        /// <summary>
        /// The amount of damping for the bounce when the particles hit the fluid containers walls.
        /// </summary>
        [Header("Fluid Constants"), Tooltip(" This value should be negative and usually works better with small negative values.")] 
        public float BoundDamping = -0.3f;
        [Tooltip("This value should be negative and be an extremely small number like -0.001 or at least a couple decimals.")]
        public float Viscosity = -0.003f;
        public float ParticleMass = 1f;
        public float GasConstant = 2f;
        public float RestDensity = 1f;
        /// <summary>
        /// The simulated time step of the particle systems. This allows for tweaking the simulation rate outside-of the normal game rate.
        /// This can be tweaked for performance reasons.
        /// Important note: Dispatching still happens in FixedUpdate to somewhat sync simulation physics.
        /// </summary>
        [Tooltip("This value should be positive for normal flow. You could make it negative to flow backwards though.")]
        public float _timeStep = 0.0007f;
        public float TimeStep
        {
            get { return _timeStep; }
            set
            {
                // If the box size hasn't changed don't bother doing any compute buffer updates for properties.
                if (_timeStep == value)
                    return;

                _timeStep = value;
                SPHComputeShader.SetFloat(TimeStepProperty,_timeStep);
            }
        }
        /// <summary>
        /// The compute buffer to hold the GPU spheres that we will instance.
        /// </summary>
        private ComputeBuffer _argsBuffer;
        /// <summary>
        /// The buffer that holds the particles themselves.
        /// </summary>
        private ComputeBuffer _particlesBuffer;
        private int _simulateKernelID;
        private int _calculateForceKernelID;
        private int _calculatePressureDensityKernelID;
        
        
        /*  Major performance improvement below by creating static readonly int properties for the shaders.
            Normally people pass in a string when set properties of shaders. 
            Example SPHComputeShader.SetInt("_ParticleLength",TotalParticles);
            
            If you pass in a string to one of the Shader.SetProperties it will call a second method
            that hashes the string into an int. Unity literally does this behind the scenes. 
            
            See around line 730 for the Unity ComputeShader.cs file for this line. 
            public void SetInt(string name, int val) => this.SetInt(Shader.PropertyToID(name), val);
            
            If you set properties every frame for compute shaders this means for each property, every frame 
            you will be allocating memory for the second method call done behind the scenes.
            
            Instead, create a static readonly property id via the static method PropertyToID() in the normal Shader class.
            This way you literally only do turn the string into an int once during compilation.
            No string to int method calls are done during running.
            
            This performance improvement can vastly help with larger particle set-ups with. The more
            properties needing sent to the GPU, the more performance you save.
         */
        
        // These must match the properties in the normal vertex shader.
        // This section is for the Shader properties in the normal vertex shader, not the compute shader
        private static readonly int SizeProperty = Shader.PropertyToID("_Size");
        private static readonly int ParticlesBufferProperty = Shader.PropertyToID("_ParticlesBuffer");
        
        // These must match the properties in the compute shader.
        private static readonly int ParticleLengthProperty = Shader.PropertyToID("_ParticleLength");
        private static readonly int BoundDampingProperty = Shader.PropertyToID("_BoundDamping");
        private static readonly int ViscosityProperty = Shader.PropertyToID("_Viscosity");
        private static readonly int ParticleMassProperty = Shader.PropertyToID("_ParticleMass");
        private static readonly int GasConstantProperty = Shader.PropertyToID("_GasConstant");
        private static readonly int RestDensityProperty = Shader.PropertyToID("_RestDensity");
        private static readonly int TimeStepProperty = Shader.PropertyToID("_TimeStep");
        private static readonly int BoxSizeProperty = Shader.PropertyToID("_BoxSize");


        // This is the compute shader property for the Particle Buffer => RWStructuredBuffer<Particle> _Particles;
        // Remember there is also one for the ParticlesBuffer of the Vertex shader. Don't mix these up.
        private static readonly int ComputeParticlesBufferProperty = Shader.PropertyToID("_Particles");
        private void Awake()
        {
            Debug.Log(TotalParticles);
            Assert.IsTrue((TotalParticles % KernelThreadGroups.x) == 0,
                    "The total amount of particles has to be dividable by KernelThreadGroups X value perfectly with no decimals. It has to be a whole number result."
                );
            
            SpawnParticlesInBox();
            
            uint[] args =
            {
                ParticleMesh.GetIndexCount(0),
                (uint)TotalParticles,
                ParticleMesh.GetIndexStart(0),
                ParticleMesh.GetBaseVertex(0),
                0
            };
            
            
            // Set up a buffer for a DrawMeshInstancedIndirect call using IndirectArguments.
            _argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            _argsBuffer.SetData(args);
            
            // Set up the particle buffer
            _particlesBuffer = new ComputeBuffer(TotalParticles,44);
            _particlesBuffer.SetData(Particles);
            
            SetupComputeBuffers();
        }
        
        private void Update()
        {
            // Set the particle data in the Compute Shaders buffer. 
            ParticleMaterial.SetFloat(SizeProperty,ParticleRenderSize);
            ParticleMaterial.SetBuffer(ParticlesBufferProperty,_particlesBuffer);

            if (ShowSpheres)
            {
                Graphics.DrawMeshInstancedIndirect(
                    ParticleMesh,
                    0,
                    ParticleMaterial,
                    new Bounds(Vector3.zero,BoxSize),
                    _argsBuffer,
                    castShadows: UnityEngine.Rendering.ShadowCastingMode.Off
                );
            }
        }
        
        private void FixedUpdate()
        {
            _particlesBuffer.GetData(Particles);
            
            /*  To sort of sync up with Unity's physics system we dispatch the simulation kernel
                for the compute shader here. */
            
            // These have to be calculated in a certain order. Pressure/density, then force, then simulation of movement.
            SPHComputeShader.Dispatch(_calculatePressureDensityKernelID, 
                KernelThreadGroups.x,
                KernelThreadGroups.y,
                KernelThreadGroups.z
            );
            SPHComputeShader.Dispatch(_calculateForceKernelID, 
                KernelThreadGroups.x,
                KernelThreadGroups.y,
                KernelThreadGroups.z
            );
            
            SPHComputeShader.Dispatch(_simulateKernelID, 
                KernelThreadGroups.x,
                KernelThreadGroups.y,
                KernelThreadGroups.z
            );
            
            ParticleMaterial.SetBuffer(ParticlesBufferProperty,_particlesBuffer);
        }

        /// <summary>
        /// Spawns the particles in the Bounding Box we declared.
        /// </summary>
        private void SpawnParticlesInBox()
        {
            Vector3 spawn = SpawnCenter;
            List<FluidData> particles = new List<FluidData>();

            for (int x = 0; x < NumToSpawn.x; x++)
            {
                for (int y = 0; y < NumToSpawn.y; y++)
                {
                    for (int z = 0; z < NumToSpawn.z; z++)
                    {
                        Vector3 spawnPos = spawn + new Vector3(
                                                x * 2 * ParticleRadius, 
                                                y * 2 * ParticleRadius, 
                                                z * 2 * ParticleRadius
                                            );
                        spawnPos += Random.onUnitSphere * SpawnOffset * ParticleRadius;
                        
                        FluidData p = new FluidData
                        {
                            Position =  spawnPos,
                        };
                        particles.Add(p);
                        
                    }
                }
            } // end of for loops

            Particles = particles.ToArray();
        }
        
        /// <summary>
        /// Call this to set up a new set of default values for the compute shader via a compute buffer.
        /// This should not be called every frame, but only called when needing to update the constant particle properties.
        /// </summary>
        private void SetupComputeBuffers()
        {
            _simulateKernelID = SPHComputeShader.FindKernel("Simulate");
            _calculateForceKernelID = SPHComputeShader.FindKernel("CalculateForce");
            _calculatePressureDensityKernelID = SPHComputeShader.FindKernel("CalculateDensityPressure");
            
            SPHComputeShader.SetInt("_KernelThreadID", KernelThreadGroups.x);
            SPHComputeShader.SetInt(ParticleLengthProperty,TotalParticles);
            SPHComputeShader.SetFloat(BoundDampingProperty,BoundDamping);
            SPHComputeShader.SetFloat(ViscosityProperty,Viscosity);
            SPHComputeShader.SetFloat(ParticleMassProperty,ParticleMass);
            SPHComputeShader.SetFloat(GasConstantProperty,GasConstant);
            SPHComputeShader.SetFloat(RestDensityProperty,RestDensity);
            SPHComputeShader.SetVector(BoxSizeProperty,BoxSize);
            SPHComputeShader.SetFloat(TimeStepProperty,_timeStep);
            
            /*  It is better performance to manually do the times here instead of using MathF.pow or using pow in the shader.
                This can be done a single time when setting up the compute shader and never need recalculated.*/
            SPHComputeShader.SetFloat("_Radius", ParticleRadius);
            SPHComputeShader.SetFloat("_Radius2",ParticleRadius * ParticleRadius);
            SPHComputeShader.SetFloat("_Radius3",ParticleRadius * ParticleRadius * ParticleRadius);
            SPHComputeShader.SetFloat("_Radius4",ParticleRadius * ParticleRadius * ParticleRadius * ParticleRadius);
            SPHComputeShader.SetFloat("_Radius5",ParticleRadius * ParticleRadius * ParticleRadius * ParticleRadius * ParticleRadius);
            
            // The second value is the name of the RWStructuredBuffer<Particles> inside the compute shader.
            SPHComputeShader.SetBuffer(_simulateKernelID, ComputeParticlesBufferProperty,_particlesBuffer);
            SPHComputeShader.SetBuffer(_calculateForceKernelID, ComputeParticlesBufferProperty,_particlesBuffer);
            SPHComputeShader.SetBuffer(_calculatePressureDensityKernelID, ComputeParticlesBufferProperty,_particlesBuffer);
            
        }
        private void OnDestroy()
        {
            // Release the buffers to prevent memory leaks.
            _argsBuffer.Release();
            _particlesBuffer.Release();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.position,BoxSize);

            if (!Application.isPlaying)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(SpawnCenter,0.2f);
            }
        }
#endif
    }
}
