using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "NewRenderPipelineAsset", menuName = "Rendering/New Render Pipeline Asset")]
public class NewRenderPipelineAsset : RenderPipelineAsset
{
    public RenderTexture renderTexture;
    private DenoiserUtility denoiser;
    protected override RenderPipeline CreatePipeline()
    {
        return new NewRenderPipeline(renderTexture, denoiser);
    }
}