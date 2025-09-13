using System;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace SF.Utilities.Rendering
{
    public class BlurRendererFeature : ScriptableRendererFeature
    {
        [Serializable]
        public class BlurSettings
        {
            [Range(0, 0.4f)] public float HorizontalBlur;
            [Range(0, 0.4f)] public float VerticalBlur;
        }

        [SerializeField] private BlurSettings _blurSettings;
        [SerializeField] private Shader _blurShader;
        private Material _material;
        private BlurRenderPass _blurRenderPass;



        public override void Create()
        {
            if(_blurShader == null) return;

            _material = new Material(_blurShader);
            _blurRenderPass = new BlurRenderPass(_material,_blurSettings);

            _blurRenderPass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        }

        public override void AddRenderPasses(
            ScriptableRenderer renderer, 
            ref RenderingData renderingData)
        {
            if(_blurRenderPass == null)
                return;

            if(renderingData.cameraData.cameraType == CameraType.Game)
                renderer.EnqueuePass(_blurRenderPass);
        }

        protected override void Dispose(bool disposing)
        {
            if(Application.isPlaying)
            {
                Destroy(_material);
            }
            else
            {
                DestroyImmediate(_material);
            }
        }

        [Serializable]
        public class BlurVolumeComponent : VolumeComponent
        {
            public ClampedFloatParameter HorizontalBlur =
                new ClampedFloatParameter(0.05f, 0, 0.5f);
            public ClampedFloatParameter VerticalBlur =
                new ClampedFloatParameter(0.05f, 0, 0.5f);
        }

        private class BlurRenderPass : ScriptableRenderPass
        {
            private BlurSettings _defaultSettings;

            private Material _material;
            private RenderTextureDescriptor _blurTextureDescriptor;

            #region Shader Properties
            private static readonly int HorizontalBlurId = Shader.PropertyToID("_HorizontalBlur");
            private static readonly int VerticalBlurId = Shader.PropertyToID("_VerticalBlur");
            private const string BlurTextureName = "_BlurTexture";
            private const string VerticalPassName = "VerticalBlurRenderPass";
            private const string HorizontalPassName = "HorizontalBlurRenderPass";
            #endregion
            public BlurRenderPass(Material material, BlurSettings blurSettings)
            {
                _material = material;
                _defaultSettings = blurSettings;

                _blurTextureDescriptor = new RenderTextureDescriptor(
                    Screen.width,
                    Screen.height,
                    RenderTextureFormat.Default,
                    0
                );
            }

            public override void RecordRenderGraph(
                RenderGraph renderGraph,
                ContextContainer frameData)
            {
                /* Contains the data and references used by URP. 
                    This includes active color and depth textures of the camera.
                 */
                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

                // Texture handle for the source color texture from the camerea
                TextureHandle srcCamColor = resourceData.activeColorTexture;
                // Destination texture handle
                TextureHandle dst = UniversalRenderer.CreateRenderGraphTexture(
                    renderGraph,
                    _blurTextureDescriptor,
                    BlurTextureName,
                    false
                );

                UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

                // The following line ensures that the render pass doesn't blit
                // from the back buffer.
                if(resourceData.isActiveTargetBackBuffer)
                    return;

                // Set the blur texture size to be the same as the camera target size.
                _blurTextureDescriptor.width = cameraData.cameraTargetDescriptor.width;
                _blurTextureDescriptor.height = cameraData.cameraTargetDescriptor.height;
                _blurTextureDescriptor.depthBufferBits = 0;

                // Update the blur settings of the material.
                UpdateBlurSettings();

                // This check prevents the preview camera from breaking in the scene view. 
                if(!srcCamColor.IsValid() || !dst.IsValid())
                    return;

                // The AddBlitPass method adds a vertical blur render graph pass that blits from the source texture (camera color in this case) to the destination texture using the first shader pass (the shader pass is defined in the last parameter).
                RenderGraphUtils.BlitMaterialParameters paraVertical = new(srcCamColor, dst, _material, 0);
                renderGraph.AddBlitPass(paraVertical, VerticalPassName);

                // The AddBlitPass method adds a horizontal blur render graph pass that blits from the texture written by the vertical blur pass to the camera color texture. The method uses the second shader pass.
                RenderGraphUtils.BlitMaterialParameters paraHorizontal = new(dst, srcCamColor, _material, 1);
                renderGraph.AddBlitPass(paraHorizontal, HorizontalPassName);
            }

            private void UpdateBlurSettings()
            {
                if(_material == null) return;

                // Use the Volume settings or the default settings if no Volume is set.
                var volumeComponent =
                    VolumeManager.instance.stack.GetComponent<BlurVolumeComponent>();
                float horizontalBlur = 
                    volumeComponent.HorizontalBlur.overrideState 
                    ?  volumeComponent.HorizontalBlur.value 
                    : _defaultSettings.HorizontalBlur;
                float verticalBlur = 
                    volumeComponent.VerticalBlur.overrideState 
                    ? volumeComponent.VerticalBlur.value 
                    : _defaultSettings.VerticalBlur;

                _material.SetFloat(HorizontalBlurId, horizontalBlur);
                _material.SetFloat(VerticalBlurId, verticalBlur);
            }
        }
    }
}
