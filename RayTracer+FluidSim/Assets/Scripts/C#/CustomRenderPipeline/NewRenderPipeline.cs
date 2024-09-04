using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Denoising;

public class NewRenderPipeline : RenderPipeline
{
    public DenoiserUtility denoiser;
    private RenderTexture renderTexture;
    public NewRenderPipeline(RenderTexture renderTexture)
    {
        this.renderTexture = renderTexture;
    }
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (var camera in cameras)
        {
            denoiser.DenoiseRenderTexture(renderTexture, renderTexture.width, renderTexture.height, context);

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

            // Blit RenderTexture to screen
            using (CommandBuffer cmd = new CommandBuffer { name = "Blit" })
            {
                // Blit from renderTexture to the camera's target texture
                cmd.Blit(renderTexture, BuiltinRenderTextureType.CameraTarget);
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear(); // Clear command buffer after execution
            }

            context.Submit();
        }
    }
}