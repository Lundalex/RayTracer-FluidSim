using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Denoising;

public class NewRenderPipeline : RenderPipeline
{
    private RenderTexture renderTexture;
    private DenoiserUtility denoiser;

    public NewRenderPipeline(RenderTexture renderTexture, DenoiserUtility denoiser)
    {
        this.renderTexture = renderTexture;
        this.denoiser = denoiser;
    }

    public void SetRendertexture(RenderTexture renderTexture)
    {
        this.renderTexture = renderTexture;
    }

    public void SetDenoiser(DenoiserUtility denoiser)
    {
        this.denoiser = denoiser;
    }

protected override void Render(ScriptableRenderContext context, Camera[] cameras)
{
    foreach (var camera in cameras)
    {
        // Set up the camera and render context
        context.SetupCameraProperties(camera);

        // Clear the camera's color and depth buffers
        using (CommandBuffer cmd = new CommandBuffer { name = "Clear" })
        {
            cmd.ClearRenderTarget(true, true, Color.black);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear(); // Clear command buffer after execution
        }

        // Set the render target to our RenderTexture
        using (CommandBuffer cmd = new CommandBuffer { name = "SetRenderTarget" })
        {
            cmd.SetRenderTarget(renderTexture);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear(); // Clear command buffer after execution
        }

        // Render the scene
        context.DrawSkybox(camera);

        // Apply denoising if both renderTexture and denoiser are not null
        if (renderTexture != null && denoiser != null)
        {
            denoiser.ApplyDenoising(ref renderTexture, context);
        }

        // Blit RenderTexture to screen
        using (CommandBuffer cmd = new CommandBuffer { name = "Blit" })
        {
            // Blit from renderTexture to the camera's target texture
            cmd.Blit(renderTexture, BuiltinRenderTextureType.CameraTarget);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear(); // Clear command buffer after execution
        }

        // Finalize rendering
        context.Submit();
    }
}
}