using UnityEngine;
using Debug = UnityEngine.Debug;
using Unity.Mathematics;
using System;
using UnityEngine.Rendering;
public class NewRenderer : MonoBehaviour
{
#region Inspector

    [Header("Camera Settings")]
    public float CameraMoveSpeed;
    public float CameraPanSpeed;

    [Header("Ray Tracer Settings")]
    public float fieldOfView;
    public int RaysNum;
    public int MaxBounceCount;
    [Range(0.0f, 1.0f)] public float ScatterProbability;
    [Range(0.0f, 0.2f)] public float DefocusStrength;
    public float FocalPlaneFactor; // FocalPlaneFactor must be positive
    public int FrameCount;
    public int AccFrameCount;

    [Header("ReStir Settings")]
    public int SceneObjectReservoirTestsNum;
    public int TriReservoirTestsNum;
    public int CandidateReservoirTestsNum;
    [Range(0, 5)] public int SpatialReuseIterations;
    public int TemporalCandidatesNum;
    public int SpatialReuseBlur;
    public float TemporalPrecisionThreshold;
    [Range(0.0f, 1.0f)] public float TemporalReuseWeight;
    public float PixelMovementThreshold;
    public float SpatialHitPointDiffThreshold;
    public float SpatialNormalsAngleThreshold;
    public float SpatialBRDFThreshold;

    [Header("Post Processing Settings")]
    public RenderTargetSelect renderTarget;
    public bool DoAccumulateFrames;
    public bool UseDenoiser;
    public bool DoLogDenoiserPerformance;
    public bool RenderAsciiArt;

    [Header("Debug Settings")]
    [Range(1, 100)] public int DebugMaxTriChecks;
    [Range(1, 100)] public int DebugMaxBVChecks;
    [Range(1, 100)] public int DebugMaxVoxelChecks;

    [Header("Multi-compile Settings")]
    public bool DoVisibilityReuse;
    public bool DoWeightRecalc;
    public bool DoBRDF;
 
    [Header("References")]
    public ComputeShader rtShader; // BVH + ReStir (direct lighting)
    public ComputeShader ppShader;
    public ComputeShader pcShader;
    public RendererShaderHelper shaderHelper;
    public TextureManager textureManager;
    public MarchingCubes mCubes;
    public ObjectManager objectManager;
    public AsciiManager asciiManager;
    public Texture2D EnvironmentMapTexture;
    public Texture2D BlackTexture;
    public NewRenderPipeline renderPipeline;
#endregion
 
#region Script communication
    [NonSerialized] public bool DoUpdateSettings;
    [NonSerialized] public bool DoReloadData = false;
    [NonSerialized] public bool ProgramStarted = false;
    [NonSerialized] public int StaticVerticesNum;
    [NonSerialized] public int StaticTrisNum;
#endregion

#region Private variables
    [NonSerialized] private RenderTexture RTPassResultTexture;
    private RenderTexture AccumulatedResultTexture;
    private RenderTexture DebugOverlayTexture;
    private int RayTracerThreadSize = 8; // /32
    private int PostProcesserThreadSize = 8; // /32
    private int PreCalcThreadSize = 256;
    [NonSerialized] public Material2[] Material2s;
    [NonSerialized] public RenderTriangle[] RenderTriangles;
    [NonSerialized] public Vertex[] StaticVertices;
    [NonSerialized] public RenderSceneObject[] RenderSceneObjects;
    [NonSerialized] public LightObject[] LightObjects;
    [NonSerialized] public RenderBV[] BVs;
    private ComputeBuffer BVBuffer;
    private ComputeBuffer RenderTriangleBuffer;
    private ComputeBuffer StaticVertexBuffer;
    private ComputeBuffer DynamicVertexBuffer;
    private ComputeBuffer RenderSceneObjectBuffer;
    private ComputeBuffer LightObjectBuffer;
    private ComputeBuffer MaterialBuffer;
 
    // ReStir
    private ComputeBuffer CandidateBuffer;
    private ComputeBuffer CandidateReuseBuffer;
    private ComputeBuffer TemporalFrameBuffer;
    private ComputeBuffer HitInfoBuffer;
    private RenderTexture RayHitPointATexture;
    private RenderTexture RayHitPointBTexture;
    private RenderTexture DepthBufferTexture;
    private RenderTexture NormalsBufferTexture;
    private Texture2D TextureAtlas;
 
    // Camera data records
    private Vector3 lastCameraPosition;
    private Quaternion lastCameraRotation;
 
    // Script-specific variables
    private bool ProgramPaused = false;
    private bool FrameStep = false;
    private bool RenderThisFrame = true;
    private Vector3 lastWorldSpaceCameraPos;
    private Matrix4x4 lastCameraTransform;
    private int2 Resolution;
    private bool[] LoggedWarnings = new bool[2];
#endregion

    public void ScriptSetup()
    {
        Resolution = new int2(Screen.width, Screen.height);
        lastCameraPosition = transform.position;
        lastCameraRotation = transform.rotation;
        
        UpdatePerFrame();
        UpdateSettings(true);
        SetCameraData();
        CreateTextures();
 
        ProgramStarted = true;
    }

    private void SetRenderTargetTexture()
    {
        switch (renderTarget)
        {
            case RenderTargetSelect.RTPassResultTexture:
                renderPipeline.SetNecessaryData(RTPassResultTexture, UseDenoiser, DoLogDenoiserPerformance);
                break;
            case RenderTargetSelect.AccumulatedResultTexture:
                renderPipeline.SetNecessaryData(AccumulatedResultTexture, UseDenoiser, DoLogDenoiserPerformance);
                break;
            case RenderTargetSelect.DebugOverlayTexture:
                renderPipeline.SetNecessaryData(DebugOverlayTexture, UseDenoiser, DoLogDenoiserPerformance);
                break;
            case RenderTargetSelect.DepthBufferTexture:
                renderPipeline.SetNecessaryData(DepthBufferTexture, UseDenoiser, DoLogDenoiserPerformance);
                break;
            case RenderTargetSelect.NormalsBufferTexture:
                renderPipeline.SetNecessaryData(NormalsBufferTexture, UseDenoiser, DoLogDenoiserPerformance);
                break;
            case RenderTargetSelect.RayHitPointATexture:
                renderPipeline.SetNecessaryData(RayHitPointATexture, UseDenoiser, DoLogDenoiserPerformance);
                break;
            case RenderTargetSelect.RayHitPointBTexture:
                renderPipeline.SetNecessaryData(RayHitPointBTexture, UseDenoiser, DoLogDenoiserPerformance);
                break;
            case RenderTargetSelect.GridDensitiesTexture:
                renderPipeline.SetNecessaryData(mCubes.GridDensitiesTexture, UseDenoiser, DoLogDenoiserPerformance);
                break;
            case RenderTargetSelect.None:
                break;
        }
    }
 
    public void ScriptUpdate()
    {
        PauseControls();
 
        if (ProgramPaused && FrameStep) Debug.Log("Stepped forward 1 frame");
        if (!ProgramPaused || (ProgramPaused && FrameStep))
        {
            RenderThisFrame = true;
            FrameStep = false;
 
            // Per-frame shader settings
            UpdatePerFrame();

            // Camera
            CameraMovement();
            // CameraPanning();
            SetCameraData();

            // Pipeline
            ConfigureRenderPipeline();
 
            if (DoUpdateSettings) { DoUpdateSettings = false; UpdateSettings(DoReloadData); DoReloadData = false; }
        }
        else RenderThisFrame = false;
    }
 
    private void UpdatePerFrame()
    {
        // Frame set variables
        int FrameRand = UnityEngine.Random.Range(0, 999999);
        rtShader.SetInt("FrameRand", FrameRand);
        rtShader.SetInt("FrameCount", FrameCount);
        ppShader.SetInt("FrameCount", FrameCount);
        FrameCount++;

        ppShader.SetInt("AccFrameCount", AccFrameCount);
        AccFrameCount++;
    }
 
    private void CameraMovement()
    {
        Vector3 direction = Vector3.zero;
 
        if (Input.GetKey(KeyCode.W)) direction += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) direction -= Vector3.forward;
        if (Input.GetKey(KeyCode.A)) direction -= Vector3.right;
        if (Input.GetKey(KeyCode.D)) direction += Vector3.right;
        if (Input.GetKey(KeyCode.Space)) direction += Vector3.up;
        if (Input.GetKey(KeyCode.LeftShift)) direction -= Vector3.up;

        direction.Normalize();
 
        transform.position += CameraMoveSpeed * Time.deltaTime * direction;
    }
 
    private void PauseControls()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            ProgramPaused = !ProgramPaused;
            Debug.Log("Program paused");
        }
        if (Input.GetKeyDown(KeyCode.F)) FrameStep = !FrameStep;
    }
 
    private void CameraPanning()
    {
        if (Input.GetMouseButton(0))
        {
            float mouseY = Input.GetAxis("Mouse Y") * CameraPanSpeed;
            float mouseX = Input.GetAxis("Mouse X") * CameraPanSpeed;
 
            transform.rotation *= Quaternion.Euler(mouseY, -mouseX, 0.0f);
        }
    }
 
    private void SetCameraData()
    {
        // Camera position
        Vector3 worldSpaceCameraPos = transform.position;
        if (lastWorldSpaceCameraPos == null) lastWorldSpaceCameraPos = worldSpaceCameraPos;

        rtShader.SetVector("WorldSpaceCameraPos", worldSpaceCameraPos);
        rtShader.SetVector("LastWorldSpaceCameraPos", lastWorldSpaceCameraPos);
    
        // Camera transform matrix
        Matrix4x4 cameraTransform = transform.localToWorldMatrix;

        if (lastCameraTransform == Matrix4x4.zero) lastCameraTransform = cameraTransform;

        rtShader.SetMatrix("CameraTransform", cameraTransform);
        rtShader.SetMatrix("LastCameraTransformInverse", lastCameraTransform.inverse);

        // Update last frame's camera data
        lastWorldSpaceCameraPos = worldSpaceCameraPos;
        lastCameraTransform = cameraTransform;
    }
 
    private void OnValidate()
    {
        if (ProgramStarted) { DoUpdateSettings = true; DoReloadData = true; }
    }
 
    public void UpdateSettings(bool resetBufferData)
    {
        FrameCount = 0;
        AccFrameCount = 0;
        if (resetBufferData) SetData();

        // Camera & display
        int[] resolutionArray = new int[] { Resolution.x, Resolution.y };
        rtShader.SetInts("Resolution", resolutionArray);

        if (MaxBounceCount != 1 && !LoggedWarnings[0]) { LoggedWarnings[0] = true; Debug.LogWarning("ReSTIR DL not designed for multi-bounce rays. Additional ray bounces may lead to unexpected results"); }
        rtShader.SetInt("MaxBounceCount", MaxBounceCount);
        rtShader.SetFloat("ScatterProbability", ScatterProbability);
 
        float aspectRatio = Resolution.x / Resolution.y;
        float fieldOfViewRad = fieldOfView * Mathf.PI / 180;
        float viewSpaceHeight = Mathf.Tan(fieldOfViewRad * 0.5f);
        float viewSpaceWidth = aspectRatio * viewSpaceHeight;
 
        rtShader.SetVector("ViewSpaceDims", new Vector2(viewSpaceWidth, viewSpaceHeight));
 
        if (DefocusStrength != 0 && !LoggedWarnings[1]) { LoggedWarnings[1] = true; Debug.LogWarning("ReSTIR not designed for focus ray modifiers. Enabling this will decrease the effectiveness of spatiotemporal reuse"); }
        rtShader.SetFloat("DefocusStrength", DefocusStrength);
        rtShader.SetFloat("FocalPlaneFactor", FocalPlaneFactor);
 
        // Debug overlay
        rtShader.SetVector("DebugDataMaxValues", new Vector3(DebugMaxTriChecks, DebugMaxBVChecks, DebugMaxVoxelChecks));
 
        // ReStir
        rtShader.SetInt("SceneObjectReservoirTestsNum", SceneObjectReservoirTestsNum);
        rtShader.SetInt("TriReservoirTestsNum", TriReservoirTestsNum);
        rtShader.SetInt("CandidateReservoirTestsNum", CandidateReservoirTestsNum);
        rtShader.SetFloat("TemporalReuseWeight", TemporalReuseWeight);
        rtShader.SetFloat("PixelMovementThreshold", PixelMovementThreshold);
        rtShader.SetFloat("SpatialHitPointDiffThreshold", SpatialHitPointDiffThreshold);
        rtShader.SetFloat("SpatialNormalsAngleThreshold", SpatialNormalsAngleThreshold);
        rtShader.SetFloat("SpatialBRDFThreshold", SpatialBRDFThreshold);
        rtShader.SetInt("TemporalCandidatesNum", TemporalCandidatesNum);
        rtShader.SetFloat("TemporalPrecisionThreshold", TemporalPrecisionThreshold);

        // Multi-compilation
        if (DoVisibilityReuse) rtShader.EnableKeyword("VIS_REUSE");
        else rtShader.DisableKeyword("VIS_REUSE");
        if (DoWeightRecalc) rtShader.EnableKeyword("WEIGHT_RECALC");
        else rtShader.DisableKeyword("WEIGHT_RECALC");
        if (DoBRDF) rtShader.EnableKeyword("BRDF");
        else rtShader.DisableKeyword("BRDF");

        // Object textures
        int[] textureAtlasDims = new int[] { TextureAtlas.width, TextureAtlas.height };
        rtShader.SetInts("TextureAtlasDims", textureAtlasDims);
        rtShader.SetTexture(0, "TextureAtlas", TextureAtlas);
        rtShader.SetTexture(4, "TextureAtlas", TextureAtlas);
 
        // Environment map texture
        int[] environmentMapTexDims = new int[] { EnvironmentMapTexture.width, EnvironmentMapTexture.height };
        rtShader.SetInts("EnvironmentMapTexDims", environmentMapTexDims);
        rtShader.SetTexture(4, "EnvironmentMap", EnvironmentMapTexture);

        // Marching cubes - fluid transform
        rtShader.SetVector("FluidDims", new Vector3(mCubes.FluidDims.x, mCubes.FluidDims.y, mCubes.FluidDims.z));
        rtShader.SetVector("FluidPos", new Vector3(mCubes.FluidPos.x, mCubes.FluidPos.y, mCubes.FluidPos.z));
        rtShader.SetVector("SmNumCells", new Vector3(mCubes.NumCells.x, mCubes.NumCells.y, mCubes.NumCells.z));
        rtShader.SetFloat("SmCellSize", mCubes.sim.MaxInfluenceRadius);
 
        Debug.Log("Internal program settings updated");
    }

    private void ConfigureRenderPipeline()
    {
        if (RenderPipelineManager.currentPipeline != null)
        {
            renderPipeline ??= (NewRenderPipeline)RenderPipelineManager.currentPipeline;

            // Set render texture
            SetRenderTargetTexture();
        }
    }
 
    private void SetData()
    {
        ComputeHelper.Release(AllBuffers());

        // Construct BVH
        (BVs, StaticVertices, RenderTriangles, RenderSceneObjects, LightObjects, TextureAtlas, Material2s, StaticTrisNum) = objectManager.ConstructScene();

        MaterialBuffer = ComputeHelper.CreateStructuredBuffer<Material2>(Material2s);
        shaderHelper.SetMaterialBuffer(MaterialBuffer);
        
        // Set BVH data
        BVBuffer = ComputeHelper.CreateStructuredBuffer<RenderBV>(BVs);
        shaderHelper.SetBVBuffer(BVBuffer);
 
        // Set SceneObjects & Tris data
        RenderSceneObjectBuffer = ComputeHelper.CreateStructuredBuffer<RenderSceneObject>(RenderSceneObjects);
        shaderHelper.SetRenderSceneObjectBuffer(RenderSceneObjectBuffer);
        RenderTriangleBuffer = ComputeHelper.CreateStructuredBuffer<RenderTriangle>(RenderTriangles, StaticTrisNum + mCubes.FluidTriMeshBufferACMax);
        shaderHelper.SetTriBuffer(RenderTriangleBuffer);
        StaticVertexBuffer = ComputeHelper.CreateStructuredBuffer<Vertex>(StaticVertices);
        shaderHelper.SetStaticVertexBuffer(StaticVertexBuffer);
        DynamicVertexBuffer = ComputeHelper.CreateStructuredBuffer<Vertex>(3 * mCubes.FluidTriMeshBufferACMax);
        shaderHelper.SetDynamicVertexBuffer(DynamicVertexBuffer);
        RunPreCalcShader();
 
        // Set LightObjects data
        LightObjectBuffer = ComputeHelper.CreateStructuredBuffer<LightObject>(LightObjects);
        shaderHelper.SetLightObjectBuffer(LightObjectBuffer);
 
        // ReStir
        CandidateBuffer = ComputeHelper.CreateStructuredBuffer<CandidateReservoir>(Resolution.x * Resolution.y);
        shaderHelper.SetCandidateBuffer(CandidateBuffer);
        CandidateReuseBuffer = ComputeHelper.CreateStructuredBuffer<CandidateReservoir>(Resolution.x * Resolution.y);
        shaderHelper.SetCandidateReuseBuffer(CandidateReuseBuffer);
        TemporalFrameBuffer = ComputeHelper.CreateStructuredBuffer<CandidateReservoir>(Resolution.x * Resolution.y);
        shaderHelper.SetTemporalFrameBuffer(TemporalFrameBuffer);
 
        HitInfoBuffer = ComputeHelper.CreateStructuredBuffer<HitInfo>(Resolution.x * Resolution.y);
        shaderHelper.SetHitInfoBuffer(HitInfoBuffer);
    }
 
    private void CreateTextures()
    {
        // Ray tracer result texture
        if (RTPassResultTexture == null)
        {
            RTPassResultTexture = TextureHelper.CreateTexture(Resolution, 4);
            RTPassResultTexture.Create();
            rtShader.SetTexture(4, "Result", RTPassResultTexture);
            ppShader.SetTexture(0, "Result", RTPassResultTexture);
            ppShader.SetTexture(1, "Result", RTPassResultTexture);
        }
 
        // Accumulated result texture
        if (AccumulatedResultTexture == null)
        {
            AccumulatedResultTexture = TextureHelper.CreateTexture(Resolution, 4);
            AccumulatedResultTexture.Create();
            ppShader.SetTexture(0, "AccumulatedResult", AccumulatedResultTexture);
            ppShader.SetTexture(1, "AccumulatedResult", AccumulatedResultTexture);
        }
 
        // Debug overlay texture
        if (DebugOverlayTexture == null)
        {
            DebugOverlayTexture = TextureHelper.CreateTexture(Resolution, 4);
            DebugOverlayTexture.Create();
            rtShader.SetTexture(0, "DebugOverlay", DebugOverlayTexture);
            rtShader.SetTexture(3, "DebugOverlay", DebugOverlayTexture);
        }
 
        // Ray hit point double buffer textures
        if (RayHitPointATexture == null)
        {
            RayHitPointATexture = TextureHelper.CreateTexture(Resolution, 3);
            RayHitPointATexture.Create();
            rtShader.SetTexture(0, "RayHitPointsA", RayHitPointATexture);
            rtShader.SetTexture(3, "RayHitPointsA", RayHitPointATexture);
        }
        if (RayHitPointBTexture == null)
        {
            RayHitPointBTexture = TextureHelper.CreateTexture(Resolution, 3);
            RayHitPointBTexture.Create();
            rtShader.SetTexture(0, "RayHitPointsB", RayHitPointBTexture);
            rtShader.SetTexture(3, "RayHitPointsB", RayHitPointBTexture);
        }
 
        // Depth buffer texture
        if (DepthBufferTexture == null)
        {
            DepthBufferTexture = new RenderTexture(Resolution.x, Resolution.y, 0, RenderTextureFormat.RFloat)
            {
                dimension = TextureDimension.Tex2D,
                enableRandomWrite = true,
                filterMode = FilterMode.Bilinear
            };
            DepthBufferTexture.Create();
            rtShader.SetTexture(0, "DepthBuffer", DepthBufferTexture);
            rtShader.SetTexture(1, "DepthBuffer", DepthBufferTexture);
            rtShader.SetTexture(2, "DepthBuffer", DepthBufferTexture);
            rtShader.SetTexture(3, "DepthBuffer", DepthBufferTexture);
        }
 
        // Normals buffer texture
        if (NormalsBufferTexture == null)
        {
            NormalsBufferTexture = TextureHelper.CreateTexture(Resolution, 3);
            NormalsBufferTexture.Create();
            rtShader.SetTexture(0, "NormalsBuffer", NormalsBufferTexture);
            rtShader.SetTexture(1, "NormalsBuffer", NormalsBufferTexture);
        }
    }
 
    private void RunPreCalcShader()
    {
        ComputeHelper.DispatchKernel(pcShader, "CalcTriNormals", RenderTriangles.Length, PreCalcThreadSize);
    }
 
    private void SpatialReuse()
    {
        bool reuseBufferCycle = false;
        for (int i = 0; i < SpatialReuseIterations; i++)
        {
            reuseBufferCycle = !reuseBufferCycle;
 
            int maxOffset = (int)Mathf.Pow(3, i) * SpatialReuseBlur;
            rtShader.SetInt("MaxOffset", maxOffset);
            rtShader.SetBool("ReuseBufferCycle", reuseBufferCycle);
 
            ComputeHelper.DispatchKernel(rtShader, "SpatialReusePass", Resolution, RayTracerThreadSize);
        }
 
        if (reuseBufferCycle == true) ComputeHelper.DispatchKernel(rtShader, "TransferToOriginal", Resolution, RayTracerThreadSize);
    }
 
    private void RunReSTIRShader()
    {
        ComputeHelper.DispatchKernel(rtShader, "InitialTrace", Resolution, RayTracerThreadSize);

        // Vector3[] normalsBuffer = TextureUnpacker.UnpackTextureTo1DArray(NormalsBufferTexture);

        if (TemporalReuseWeight > 0) ComputeHelper.DispatchKernel(rtShader, "TemporalReuse", Resolution, RayTracerThreadSize);

        if (SpatialReuseIterations > 0) SpatialReuse();
 
        ComputeHelper.DispatchKernel(rtShader, "TraceRays", Resolution, RayTracerThreadSize);
    }
 
    private void RunPostProcessingShader()
    {
        // Accumulate ray tracer output data between frames
        ComputeHelper.DispatchKernel(ppShader, "AccumulateFrames", Resolution, PostProcesserThreadSize);

        // Render the noise textures
        ComputeHelper.DispatchKernel(ppShader, "RenderNoiseTextures", Resolution, PostProcesserThreadSize);
    }

    public void RenderScene()
    {
        if (RenderThisFrame)
        {
            // Effectively resets the accumulated result texture
            if (!DoAccumulateFrames)
            {
                AccFrameCount = 0;
                ppShader.SetInt("AccFrameCount", AccFrameCount++);
            }

            // Execute the primary 
            for (int i = 0; i < RaysNum; i++)
            {
                // Execute a complete rendering pass
                RunReSTIRShader();
                RunPostProcessingShader();

                // Don't update for the last iteration
                if (i != RaysNum-1) UpdatePerFrame();
            }

            AsciiOverlay();
        }
    }

    private void AsciiOverlay()
    {
        if (RenderAsciiArt)
        {
            Texture2D tex;
            switch (renderTarget)
            {
                case RenderTargetSelect.RTPassResultTexture:
                    tex = asciiManager.RenderTextureToTexture2D(RTPassResultTexture);
                    break;
                case RenderTargetSelect.AccumulatedResultTexture:
                    tex = asciiManager.RenderTextureToTexture2D(AccumulatedResultTexture);
                    break;
                case RenderTargetSelect.DebugOverlayTexture:
                    tex = asciiManager.RenderTextureToTexture2D(DebugOverlayTexture);
                    break;
                case RenderTargetSelect.DepthBufferTexture:
                    tex = asciiManager.RenderTextureToTexture2D(DepthBufferTexture);
                    break;
                case RenderTargetSelect.NormalsBufferTexture:
                    tex = asciiManager.RenderTextureToTexture2D(NormalsBufferTexture);
                    break;
                case RenderTargetSelect.RayHitPointATexture:
                    tex = asciiManager.RenderTextureToTexture2D(RayHitPointATexture);
                    break;
                case RenderTargetSelect.RayHitPointBTexture:
                    tex = asciiManager.RenderTextureToTexture2D(RayHitPointBTexture);
                    break;
                case RenderTargetSelect.GridDensitiesTexture:
                    tex = asciiManager.RenderTextureToTexture2D(mCubes.GridDensitiesTexture);
                    break;
                default:
                    asciiManager.Asciitext.enabled = false;
                    return;
            }

            string asciiArt = asciiManager.ConvertTextureToASCII(tex);

            asciiManager.Asciitext.enabled = true;
            asciiManager.Asciitext.text = asciiArt;

            Destroy(tex);
        }
        else asciiManager.Asciitext.enabled = false;
    }
 
    private ComputeBuffer[] AllBuffers() => new ComputeBuffer[] { BVBuffer, RenderTriangleBuffer, StaticVertexBuffer, DynamicVertexBuffer, RenderSceneObjectBuffer, LightObjectBuffer, MaterialBuffer, CandidateBuffer, CandidateReuseBuffer, TemporalFrameBuffer, HitInfoBuffer };
 
    private void OnDestroy()
    {
        ComputeHelper.Release(AllBuffers());

        renderPipeline.colorImage.Dispose();
        renderPipeline.dst.Dispose();
    }
 
    // --- Test ---

    public void GetPixelsFromTexture()
    {
        // Create a new Texture2D with the same dimensions and format as the RenderTexture
        Texture2D tex = new Texture2D(RayHitPointATexture.width, RayHitPointATexture.height, TextureFormat.RGBA32, false);
 
        // Set the RenderTexture as the active render target
        RenderTexture.active = RayHitPointATexture;
 
        // Read the RenderTexture into the Texture2D
        tex.ReadPixels(new Rect(0, 0, RayHitPointATexture.width, RayHitPointATexture.height), 0, 0);
        tex.Apply();
 
        Color[] pixels = tex.GetPixels();
    }
}