using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AzureFogScatteringFeature : ScriptableRendererFeature
{
    public Material blitMaterial = null;
    public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingSkybox;
    private AzureFogScatteringPass m_azureFogScatteringPass;

    public override void Create()
    {
        m_azureFogScatteringPass = new AzureFogScatteringPass();
        m_azureFogScatteringPass.blitMaterial = blitMaterial;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (blitMaterial == null)
        {
            Debug.LogWarningFormat("Missing Blit Material. {0} blit pass will not execute. Check for missing reference in the assigned renderer.", GetType().Name);
            return;
        }

        m_azureFogScatteringPass.renderPassEvent = renderPassEvent;
        renderer.EnqueuePass(m_azureFogScatteringPass);
    }

    private class AzureFogScatteringPass : ScriptableRenderPass
    {
        public Material blitMaterial = null;
        private RTHandle m_sourceColor;
        private RTHandle m_DestinationColor;

        private Camera m_camera;
        private Vector3[] m_frustumCorners = new Vector3[4];
        private Rect m_rect = new Rect(0, 0, 1, 1);
        private Matrix4x4 m_frustumCornersArray;


        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            m_sourceColor = renderingData.cameraData.renderer.cameraColorTargetHandle;
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;
            RenderingUtils.ReAllocateIfNeeded(ref m_DestinationColor, desc, FilterMode.Point, TextureWrapMode.Clamp, name: "_TemporaryDestinationHandle");
        }

        /// <inheritdoc/>
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (blitMaterial == null) return;
            m_camera = renderingData.cameraData.camera;
            if (m_camera == null) return;

            CommandBuffer cmd = CommandBufferPool.Get();

            m_camera.CalculateFrustumCorners(m_rect, m_camera.farClipPlane, m_camera.stereoActiveEye, m_frustumCorners);
            m_frustumCornersArray = Matrix4x4.identity;
            m_frustumCornersArray.SetRow(0, m_camera.transform.TransformVector(m_frustumCorners[0]));  // bottom left
            m_frustumCornersArray.SetRow(2, m_camera.transform.TransformVector(m_frustumCorners[1]));  // top left
            m_frustumCornersArray.SetRow(3, m_camera.transform.TransformVector(m_frustumCorners[2]));  // top right
            m_frustumCornersArray.SetRow(1, m_camera.transform.TransformVector(m_frustumCorners[3]));  // bottom right
            blitMaterial.SetMatrix("_FrustumCorners", m_frustumCornersArray);

            //Blitter.BlitCameraTexture(cmd, sourceColor, m_DestinationColor, blitMaterial, -1);
            //Blitter.BlitCameraTexture(cmd, m_DestinationColor, sourceColor);

            cmd.Blit(m_sourceColor, m_DestinationColor, blitMaterial, -1);
            cmd.Blit(m_DestinationColor, m_sourceColor);

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }
}