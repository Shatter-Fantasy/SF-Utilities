using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

public sealed class ColorBlitRenderFeature : ScriptableRendererFeature
{
    [Range(0f,1f)] public float Intensity = 0.25f;
    public Shader ColorBlitShader;
    
    private Material _material;

    private ColorBlitRenderPass _colorBlitRenderPass;

    #region FEATURE_METHODS

    public override void Create()
    {
        if(ColorBlitShader == null)
            return;

        _material = CoreUtils.CreateEngineMaterial(ColorBlitShader);
        _colorBlitRenderPass = new ColorBlitRenderPass(_material);
        _colorBlitRenderPass.renderPassEvent = RenderPassEvent.AfterRendering;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, 
        ref RenderingData renderingData)
    {
        // Skip rendering if _material or the pass instance are null for whatever reason
        if (_material == null || _colorBlitRenderPass == null)
            return;

        if (renderingData.cameraData.cameraType == CameraType.Preview 
            || renderingData.cameraData.cameraType == CameraType.Reflection)
            return;

        // Tell the Scriptable Render Pipeline we don't need any global textures like depth, normal, or camera color opaque textures.
        _colorBlitRenderPass.ConfigureInput(ScriptableRenderPassInput.None);

        // Set the intensity inside the ColorBlitRenderPass
        _colorBlitRenderPass.SetIntensity(Intensity);

        renderer.EnqueuePass(_colorBlitRenderPass);
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(_material);
    }
    #endregion

    private class ColorBlitRenderPass : ScriptableRenderPass
    {
        #region PASS_FIELDS
        private const string PASS_NAME = "Color Blit Pass";
        private Material _material;
        private float _intensity;
        /// <summary>
        /// This is the parameter method for the value inside of the actual shader file.
        /// _Intensity ("Intensity", float) = 0.25
        /// </summary>
        private static readonly int Intensity_ID = Shader.PropertyToID("_Intensity");
        #endregion

        public ColorBlitRenderPass(Material material)
        {
            _material = material;

            // Note the class ProfilingSampler returns null in a runtime build to prevent any wasted performance for profiling. So no need to worry about checking for if in editor mode when using it.
            profilingSampler = new ProfilingSampler(passName);
        }

        public void SetIntensity(float intensity)
        {
            _intensity = intensity;
        }

        // Use the RecordRenderGraph method to configure the input and output parameters for the AddBlitPass method and execute the AddBlitPass method.
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var resourceData = frameData.Get<UniversalResourceData>();

            // The following line ensures that the render pass doesn't blit
            // from the back buffer.
            // The back buffer is the buffer used to set pixels.
            // The front buffer is the buffer used to display pixels.
            if(resourceData.isActiveTargetBackBuffer)
            {
                Debug.LogError($"Skipping render pass. ColorBlitRendererFeature requires an intermediate ColorTexture, we can't use the BackBuffer as a texture input.");
                return;
            }

            // Get the active texture that is between the front and back buffer.
            var source = resourceData.activeColorTexture;

            // Get the Texture Decriptor that holds the data of the texture.
            // Think height, width, scale, render format, and so forth.
            var destinationDesc = renderGraph.GetTextureDesc(source);
            destinationDesc.name = $"CameraColor-{passName}";
            destinationDesc.clearBuffer = false;
            destinationDesc.depthBufferBits = 0;

            // Create the texture
            TextureHandle destination = renderGraph.CreateTexture(destinationDesc);

            // Add the render graph pass that blits from the source to the destination texture.
            RenderGraphUtils.BlitMaterialParameters para =
                new(source, destination, _material, 0);
            para.material.SetFloat(Intensity_ID,_intensity);
            renderGraph.AddBlitPass(para,passName: passName);

            // Use the destination texture as the camera texture to avoid an extra blit from the destination texture back to the camera texture.
            resourceData.cameraColor = destination;
        }
    }
}
