using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Denoising;

public class DenoiserUtility : MonoBehaviour
{
    // Function to denoise a render texture using the synchronous API
    public void ApplyDenoising(RenderTexture inTex, out RenderTexture outTex, ScriptableRenderContext context)
    {
        // Initialize command buffer and denoiser
        CommandBuffer cmd = new CommandBuffer { name = "ApplyDenoising" };
        var denoiser = new CommandBufferDenoiser();

        // Initialize the denoiser
        Denoiser.State result = denoiser.Init(DenoiserType.OpenImageDenoise, inTex.width, inTex.height);
        Assert.AreEqual(Denoiser.State.Success, result);

        // Request denoising
        denoiser.DenoiseRequest(cmd, "color", inTex);

        // Execute the command buffer to apply denoising
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear(); // Clear after execution

        // Wait for completion of denoising
        result = denoiser.WaitForCompletion(context, cmd);
        Assert.AreEqual(Denoiser.State.Success, result);

        // Create an output texture and get the results
        var dst = new RenderTexture(inTex.descriptor);
        result = denoiser.GetResults(cmd, dst);
        Assert.AreEqual(Denoiser.State.Success, result);
        
        // Set the output texture
        outTex = dst;

        // Execute the final command buffer and clean up
        context.ExecuteCommandBuffer(cmd);
        cmd.Release();
    }
}