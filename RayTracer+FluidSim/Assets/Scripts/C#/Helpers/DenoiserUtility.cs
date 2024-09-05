using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Denoising;

public class DenoiserUtility : MonoBehaviour
{
    // Function to denoise a render texture using the synchronous API
    public void ApplyDenoising(ref RenderTexture tex, ScriptableRenderContext context)
    {
        // Initialize command buffer and denoiser
        CommandBuffer cmd = new CommandBuffer { name = "ApplyDenoising" };
        var denoiser = new CommandBufferDenoiser();

        // Initialize the denoiser
        Denoiser.State result = denoiser.Init(DenoiserType.OpenImageDenoise, tex.width, tex.height);
        Assert.AreEqual(Denoiser.State.Success, result);

        // Request denoising
        denoiser.DenoiseRequest(cmd, "color", tex);

        // Execute the command buffer to apply denoising
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear(); // Clear after execution

        // // Wait for completion of denoising
        // result = denoiser.WaitForCompletion(context, cmd); CRASHHAHSHAHASHASHASHHASHSA
        // Assert.AreEqual(Denoiser.State.Success, result);

        // // Create an output texture and get the results
        // var dst = new RenderTexture(tex.descriptor);
        // result = denoiser.GetResults(cmd, dst);
        // Assert.AreEqual(Denoiser.State.Success, result);
        
        // // Set the output texture
        // tex = dst;

        // // Execute the final command buffer and clean up
        // context.ExecuteCommandBuffer(cmd);
        // cmd.Release();
    }
}