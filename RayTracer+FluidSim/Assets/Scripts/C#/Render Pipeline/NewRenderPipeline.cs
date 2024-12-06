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
            if (camera.cameraType == CameraType.SceneView) continue;

            context.SetupCameraProperties(camera);

            CommandBuffer cmd = new CommandBuffer { name = "Render Scene" };
            cmd.ClearRenderTarget(true, true, Color.black);
            cmd.SetRenderTarget(renderTexture);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            // Draw skybox (no submit here)
            context.DrawSkybox(camera);

            // Final blit to camera target
            cmd.Blit(renderTexture, BuiltinRenderTextureType.CameraTarget);
            context.ExecuteCommandBuffer(cmd);

            // Now submit once, at the end
            context.Submit();

            cmd.Release();
        }
    }
}

//                 // Copy pixels to NativeArray
//                 colorImage.CopyFrom(tempTexture.GetRawTextureData<Vector4>());

//                 if (doLogPerformance) DebugUtils.LogStopWatch("Denoiser - read data to NativeArray(s)", ref stopwatch); // 4ms
//                 stopwatch = Stopwatch.StartNew();

//                 // Initialize the denoiser
//                 // TEST OPTIX or RADION FOR NVIDIA CARD
//                 Denoiser.State result = denoiser.Init(DenoiserType.Optix, renderTexture.width, renderTexture.height);
//                 Assert.AreEqual(Denoiser.State.Success, result);

//                 // Denoise the image using Immediate API
//                 result = denoiser.DenoiseRequest("color", colorImage);
//                 Assert.AreEqual(Denoiser.State.Success, result);

//                 // Retrieve denoising results
//                 result = denoiser.GetResults(dst); // 140ms
//                 Assert.AreEqual(Denoiser.State.Success, result);

//                 if (doLogPerformance) DebugUtils.LogStopWatch("Denoiser - Execute denoising algorithm", ref stopwatch); // 140ms