using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

public class GrayFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Setting
    {
        public Material grayMaterial;
        public RenderPassEvent renderPassEvent;
        public string tag;
        public FilterMode filterMode;
    }

    public Setting setting;
   
    GrayPass m_GrayRenderPass;

    public override void Create()
    {
        m_GrayRenderPass = new GrayPass(setting.renderPassEvent,setting.grayMaterial,setting.tag,setting.filterMode);
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        //获取相机的主纹理
        m_GrayRenderPass.Setup(renderer.cameraColorTarget);
        renderer.EnqueuePass(m_GrayRenderPass);
    }
}
