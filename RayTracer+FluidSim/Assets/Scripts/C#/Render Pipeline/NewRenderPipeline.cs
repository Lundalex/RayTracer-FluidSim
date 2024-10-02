using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using UnityEngine.Rendering.Denoising;
using UnityEngine.Assertions;
using System.Diagnostics;
using RendererResources;

public class NewRenderPipeline : RenderPipeline
{
    private RenderTexture renderTexture;
    private Denoiser denoiser;
    private bool doDenoisingPass;
    private bool doLogPerformance;

    public NativeArray<Vector4> colorImage;
    private Texture2D tempTexture;
    public NativeArray<Vector4> dst;

    public NewRenderPipeline() {}

    public void SetNecessaryData(RenderTexture renderTexture, bool doDenoisingPass, bool doLogPerformance)
    {
        this.renderTexture = renderTexture;
        this.doDenoisingPass = doDenoisingPass;
        this.doLogPerformance = doLogPerformance;

        // Ensure NativeArrays are properly allocated
        if (!colorImage.IsCreated || colorImage.Length != renderTexture.width * renderTexture.height)
        {
            colorImage.Dispose();
            colorImage = new NativeArray<Vector4>(renderTexture.width * renderTexture.height, Allocator.Persistent);
        }

        if (!dst.IsCreated || dst.Length != renderTexture.width * renderTexture.height)
        {
            dst.Dispose();
            dst = new NativeArray<Vector4>(renderTexture.width * renderTexture.height, Allocator.Persistent);
        }

        // Initialize or resize tempTexture if needed
        if (tempTexture == null || tempTexture.width != renderTexture.width || tempTexture.height != renderTexture.height)
        {
            if (tempTexture != null)
                Object.Destroy(tempTexture); // Clean up previous texture if needed

            tempTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBAFloat, false);
        }

        denoiser ??= new Denoiser();
    }

    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (var camera in cameras)
        {
            context.SetupCameraProperties(camera);

            CommandBuffer cmd = new CommandBuffer { name = "Render Scene" };
            cmd.ClearRenderTarget(true, true, Color.black);
            cmd.SetRenderTarget(renderTexture);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            context.DrawSkybox(camera);
            context.Submit();

            // Immediate API: Convert RenderTexture to NativeArray
            if (renderTexture != null && doDenoisingPass)
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                // Get pixels from RenderTexture (GPU to CPU transfer)
                RenderTexture.active = renderTexture;
                tempTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                RenderTexture.active = null;

                // Copy pixels to NativeArray
                colorImage.CopyFrom(tempTexture.GetRawTextureData<Vector4>());

                if (doLogPerformance) DebugUtils.LogStopWatch("Denoiser - read data to NativeArray(s)", ref stopwatch); // 4ms
                stopwatch = Stopwatch.StartNew();

                // Initialize the denoiser
                // TEST OPTIX or RADION FOR NVIDIA CARD
                Denoiser.State result = denoiser.Init(DenoiserType.Optix, renderTexture.width, renderTexture.height);
                Assert.AreEqual(Denoiser.State.Success, result);

                // Denoise the image using Immediate API
                result = denoiser.DenoiseRequest("color", colorImage);
                Assert.AreEqual(Denoiser.State.Success, result);

                // Retrieve denoising results
                result = denoiser.GetResults(dst); // 140ms
                Assert.AreEqual(Denoiser.State.Success, result);

            if (doLogPerformance) DebugUtils.LogStopWatch("Denoiser - Execute denoising algorithm", ref stopwatch); // 140ms

                // Copy results back to RenderTexture
                tempTexture.LoadRawTextureData(dst);
                tempTexture.Apply();

                // Copy the denoised texture back to the render texture
                Graphics.Blit(tempTexture, renderTexture);
            }

            // Blit the denoised RenderTexture to the screen
            cmd.Blit(renderTexture, BuiltinRenderTextureType.CameraTarget);
            context.ExecuteCommandBuffer(cmd);
            context.Submit();

            cmd.Release();
        }
    }
}