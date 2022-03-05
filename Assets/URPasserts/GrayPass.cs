using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GrayPass : ScriptableRenderPass
{
    private string m_commandBufferTag;//用于在frame debugger中与其他标签区分SwitchableBase
    private Material m_grayMaterial;
    private RenderTargetIdentifier m_cameraColorIdentifier;
    private FilterMode filterMode;

    private RenderTargetHandle m_temp;
    public GrayPass(RenderPassEvent evt, Material material, string tag,FilterMode filterMode)
    {
        renderPassEvent = evt;
        m_commandBufferTag = tag;
        this.filterMode = filterMode;
        if (material == null)
        {
            Debug.LogWarningFormat("urp pp's Gray Material is missing,{0} wouldn't be executed", GetType().Name);
            return;
        }
        m_grayMaterial = material;
        m_temp.Init("temp");
    }
    public void Setup(RenderTargetIdentifier cameraColorTarget)
    {
        m_cameraColorIdentifier = cameraColorTarget;
    }


    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (!renderingData.cameraData.postProcessEnabled)
        {
            return;
        }
        var cmd = CommandBufferPool.Get(m_commandBufferTag);
        RenderTextureDescriptor cameraTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
        cmd.GetTemporaryRT(m_temp.id, cameraTargetDescriptor,filterMode);
        cmd.Blit(m_cameraColorIdentifier, m_temp.Identifier(), m_grayMaterial);
        cmd.Blit(m_temp.Identifier(), m_cameraColorIdentifier);
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
        cmd.ReleaseTemporaryRT(m_temp.id);
    }
}
