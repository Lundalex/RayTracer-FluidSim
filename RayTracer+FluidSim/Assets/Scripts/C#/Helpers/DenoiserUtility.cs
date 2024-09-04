using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Denoising;

public class DenoiserUtility : MonoBehaviour
{
    // Function to denoise a render texture using the synchronous API
    public void DenoiseRenderTexture(RenderTexture tex, int width, int height, ScriptableRenderContext context)
    {
        CommandBuffer cmd = new CommandBuffer();
		var denoiser = new CommandBufferDenoiser();

        Denoiser.State result = denoiser.Init(DenoiserType.OpenImageDenoise, width, height);
		Assert.AreEqual(Denoiser.State.Success, result);

        denoiser.DenoiseRequest(cmd, "color", tex);

        result = denoiser.WaitForCompletion(context, cmd);
        Assert.AreEqual(Denoiser.State.Success, result);

        // Get the results
        var dst = new RenderTexture(tex.descriptor);
		result = denoiser.GetResults(cmd, dst);
		Assert.AreEqual(Denoiser.State.Success, result);
    }
}