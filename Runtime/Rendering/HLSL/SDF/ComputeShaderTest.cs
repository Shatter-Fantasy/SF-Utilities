using System.Collections.Generic;
using System.Runtime.InteropServices; // Using Marshal SizeOf to get the correct memory footprint of the SDFPassData.

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;


namespace SF.Utilities.Rendering
{
   
    public class SDFComputeRendererFeature : ScriptableRendererFeature
    {
        
        [SerializeField] private ComputeShader _SDFCompute;
        [SerializeField] private RenderPassEvent _injectionPassEvent = RenderPassEvent.AfterRendering;
        
        public List<SDFBufferData> SDFInputs = new List<SDFBufferData>();
        private SDFComputePass _SDFComputePass;
        
        public override void Create()
        {
            _SDFComputePass = new SDFComputePass(SDFInputs);
            _SDFComputePass.renderPassEvent = _injectionPassEvent;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!SystemInfo.supportsComputeShaders)
            {
                Debug.LogWarning("The current system or platform target does not support compute shaders. SDFComputeRendererFeature pass will be skipped.");
                return;
            }

            if (_SDFCompute == null)
            {
                Debug.LogWarning("No Compute Shader was assigned to the SDFComputeRendererFeature being used. SDFComputeRendererFeature pass will be skipped.");
                return;
            }
            _SDFComputePass.Setup(_SDFCompute,SDFInputs);
            renderer.EnqueuePass(_SDFComputePass);
        }
    }
    
    public class SDFComputePass : ScriptableRenderPass
    {
        public ComputeShader SDFComputeShader;

        public List<SDFBufferData> SDFInputs = new List<SDFBufferData>();
        
        // The buffer that has data being sent into the Compute Shader.
        private GraphicsBuffer _inputBuffer;
        private TextureHandle _SDFTexture;

        public SDFComputePass(List<SDFBufferData> sdfInputs)
        {
            SDFInputs = sdfInputs;
            if (sdfInputs.Count < 1)
            {
                Debug.LogWarning("The SDFInput list passed into the constructor for theSDFComputePass didn't have any values. Can't run pass");
                return;
            }
            
            // Create the Graphics Buffers as a structured buffer.
            // Create the Graphics Buffers with a length of a set number of integers, so the compute shader can out put certain values.
            _inputBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, SDFInputs.Count, SDFBufferData.SizeOf());
            _inputBuffer.SetData<SDFBufferData>(SDFInputs);
        }

        public void Setup(ComputeShader sdfComputeShader, List<SDFBufferData> sdfInputs)
        {
            SDFComputeShader = sdfComputeShader;
            SDFInputs = sdfInputs;
        }
        public class PassData
        {
            // Handle for buffers that the RenderGraph can manage.
            public BufferHandle Input;
            public TextureHandle SDFTextureOutput;
            public ComputeShader SDFComputeShaderRG;
        }
        
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            
            Debug.Log(_SDFTexture);
            
            var resourceData = frameData.Get<UniversalResourceData>();
            
            // Import buffers outside the render graph building statements.
            // Convert the buffer to a handle the Render Graph can manage.
            BufferHandle inputHandleRG = renderGraph.ImportBuffer(_inputBuffer);
            TextureHandle sdfTextureHandleRG = renderGraph.ImportTexture(_SDFTexture); 
            
            
            using (var builder = renderGraph.AddComputePass("SDFComputePass", out PassData passData))
            {
                // Set all pass data for the SDF
                passData.Input = inputHandleRG;
                passData.SDFTextureOutput = sdfTextureHandleRG;
                passData.SDFComputeShaderRG = SDFComputeShader;

                builder.UseBuffer(passData.Input);
                // Tell the buffer that it can have data written back into it.
                builder.UseTexture(passData.SDFTextureOutput, AccessFlags.Write);
                builder.SetRenderFunc((PassData data, ComputeGraphContext context) => ExecutePass(data,context));
            }
        }
        
        // Setting this to static allows the lambda it is called from to not doing any memory allocations.
        static void ExecutePass(PassData data, ComputeGraphContext context)
        {
            // Attach the buffer to the compute shader.
            context.cmd.SetComputeBufferParam(data.SDFComputeShaderRG, data.SDFComputeShaderRG.FindKernel("SDFMain"), "InputData", data.Input);
            context.cmd.SetComputeTextureParam(data.SDFComputeShaderRG, data.SDFComputeShaderRG.FindKernel("SDFMain"),"SDFTexture",data.SDFTextureOutput);
            
            // execute the compute shader.
            context.cmd.DispatchCompute(data.SDFComputeShaderRG,data.SDFComputeShaderRG.FindKernel("SDFMain"),1,1,1);
        }
    }
    
    /// <summary>
    /// A class that holds SDF data to put in a compute shader.
    /// The data is laid out in Sequential structure to make sure the data is being sent in the correct order into the Graphics Buffers. 
    /// </summary>
    [System.Serializable]
    public struct SDFBufferData
    {
        public float Distance ;
        public static int SizeOf()
        {
           return Marshal.SizeOf(typeof(SDFBufferData));
        }
    }
}
