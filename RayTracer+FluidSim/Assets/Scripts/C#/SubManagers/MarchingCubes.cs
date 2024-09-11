using UnityEngine;
using Unity.Mathematics;
using System;

// Import utils from RendererResources.cs
using RendererResources;
public class MarchingCubes : MonoBehaviour
{
    // Scene-related variables
    public float CellSize;
    public float DensityRadius;
    public float Threshold;
    public float DistanceMultiplier;
    public float DensityMultiplier;

    public float FluidTriMeshSLBufferSafety = 1;

    // World transform
    public float3 FluidDims;
    public float3 FluidPos;

    // Script references
    public Simulation sim;
    public new NewRenderer renderer;
    public MarchingCubesShaderHelper shaderHelper;
    public TextureManager textureManager;

    // Shader references
    public ComputeShader mcShader;
    public ComputeShader svoShader;
    public ComputeShader ssShader;
    // Run-time set variables
    [NonSerialized] public int4 NumCells;
    [NonSerialized] public int NumCellsAll;
    private const uint MaxTrisPerCell = 5;

    // Shader settings
    private const int mcShaderThreadSize = 8;
    private const int mcShaderThreadSize2 = 512;
    private const int ssShaderThreadSize = 512;
#region Run Time Set Variables
    [NonSerialized] public int NumPoints;
    [NonSerialized] public int NumPoints_NextPow2;
    [NonSerialized] public  int FluidMeshLength = 0;
    private int LastFluidMeshLength = 0;
    private int FluidTriMeshSLLength = 0;
#endregion

    // Buffers and textures
    public ComputeBuffer PointsBuffer;
    public ComputeBuffer SpatialLookupBuffer;
    public ComputeBuffer StartIndicesBuffer;
    public ComputeBuffer FluidTriMeshBufferAC;
    public ComputeBuffer FluidTriMeshSLBuffer;
    public ComputeBuffer FluidStartIndicesBuffer;
    // private ComputeBuffer AC_SurfaceCells;
    // private ComputeBuffer AC_FluidTriMesh;
    // private ComputeBuffer CB_A;
    [NonSerialized] public RenderTexture GridDensitiesTexture;
    [NonSerialized] public RenderTexture SurfaceCellsTexture;
    [NonSerialized] public RenderTexture SurfaceCellsLookupTexture;
    private int3 SurfaceCellsMM1Dims;
    private int SurfaceCellsMipmapDepth;
    private bool ProgramStarted = false;

    public void ScriptSetup()
    {
        UpdateSettings();
        InitBuffers();
        InitTextures();

        ProgramStarted = true;
    }

    private void OnValidate()
    {
        if (ProgramStarted) UpdateSettings();
    }

    private void UpdateSettings()
    {
        if (!sim.ProgramStarted) Debug.LogWarning("Marching cubes class initiated before Simulation class. Variables might not be set correctly");

        float3 simMaxBounds = new(sim.Width, sim.Height, sim.Depth);

        NumCells = new(Mathf.CeilToInt(simMaxBounds.x / CellSize),
                        Mathf.CeilToInt(simMaxBounds.y / CellSize),
                        Mathf.CeilToInt(simMaxBounds.z / CellSize), 0);
        NumCells.w = NumCells.x * NumCells.y;
        NumCellsAll = NumCells.x * NumCells.y * NumCells.z;

        FluidTriMeshSLLength = (int)(NumCellsAll * MaxTrisPerCell * FluidTriMeshSLBufferSafety);

        mcShader.SetFloat("CellSize", CellSize);
        mcShader.SetFloat("DensityRadius", DensityRadius);
        mcShader.SetFloat("Threshold", Threshold);
        mcShader.SetVector("NumCells", new Vector4(NumCells.x, NumCells.y, NumCells.z, NumCells.x * NumCells.y));
        mcShader.SetInt("NumCellsAll", NumCellsAll);

        mcShader.SetFloat("DensityMultiplier", DensityMultiplier);
        mcShader.SetFloat("DistanceMultiplier", DistanceMultiplier);

        mcShader.SetVector("FluidDims", new Vector3(FluidDims.x, FluidDims.y, FluidDims.z));
        mcShader.SetVector("FluidPos", new Vector3(FluidPos.x, FluidPos.y, FluidPos.z));

        renderer.DoUpdateSettings = true;
    }

    private void InitBuffers()
    {
        if (!sim.ProgramStarted) Debug.LogWarning("Marching cubes class initiated before Simulation class. Variables might not be set correctly");
        ComputeHelper.CreateStructuredBuffer<float3>(ref PointsBuffer, sim.ParticlesNum);
        mcShader.SetBuffer(0, "Points", PointsBuffer);

        NumPoints = sim.ParticlesNum;
        NumPoints_NextPow2 = Func.NextPow2(NumPoints);

        ComputeHelper.CreateStructuredBuffer<int2>(ref SpatialLookupBuffer, Func.NextPow2(NumPoints_NextPow2));
        ComputeHelper.CreateStructuredBuffer<int>(ref StartIndicesBuffer, NumCellsAll);
        mcShader.SetBuffer(0, "SpatialLookup", SpatialLookupBuffer);
        mcShader.SetBuffer(0, "StartIndices", StartIndicesBuffer);

        ComputeHelper.CreateStructuredBuffer<int>(ref FluidStartIndicesBuffer, NumCellsAll);
        mcShader.SetBuffer(4, "FluidStartIndices", FluidStartIndicesBuffer);

        ComputeHelper.CreateAppendBuffer<MCTri>(ref FluidTriMeshBufferAC, 20000);
        mcShader.SetBuffer(2, "FluidTriMeshAPPEND", FluidTriMeshBufferAC);
        mcShader.SetBuffer(3, "FluidTriMeshCONSUME", FluidTriMeshBufferAC);

        ComputeHelper.CreateStructuredBuffer<MCTri>(ref FluidTriMeshSLBuffer, FluidTriMeshSLLength);
        mcShader.SetBuffer(3, "FluidTriMeshSL", FluidTriMeshSLBuffer);
        mcShader.SetBuffer(4, "FluidTriMeshSL", FluidTriMeshSLBuffer);
        mcShader.SetBuffer(5, "FluidTriMeshSL", FluidTriMeshSLBuffer);

        shaderHelper.SetSSShaderBuffers(ssShader);
    }

    private void InitTextures()
    {
        if (GridDensitiesTexture == null)
        {
            GridDensitiesTexture = TextureHelper.CreateTexture(NumCells.xyz, 1);
            GridDensitiesTexture.Create();
            mcShader.SetTexture(0, "GridDensities", GridDensitiesTexture);
            mcShader.SetTexture(1, "GridDensities", GridDensitiesTexture);
            mcShader.SetTexture(2, "GridDensities", GridDensitiesTexture);
            renderer.ppShader.SetTexture(1, "TexA", GridDensitiesTexture);
        }
        if (SurfaceCellsTexture == null)
        {
            (SurfaceCellsTexture, SurfaceCellsMM1Dims, SurfaceCellsMipmapDepth) = TextureHelper.CreateVoxelTexture(NumCells.xyz);
            SurfaceCellsTexture.Create();
            mcShader.SetTexture(1, "SurfaceCells", SurfaceCellsTexture);
            mcShader.SetTexture(2, "SurfaceCells", SurfaceCellsTexture);
            mcShader.SetTexture(4, "SurfaceCells", SurfaceCellsTexture);
            svoShader.SetTexture(0, "SurfaceCells", SurfaceCellsTexture);
            renderer.rtShader.SetTexture(0, "SurfaceCells", SurfaceCellsTexture);
            renderer.rtShader.SetTexture(4, "SurfaceCells", SurfaceCellsTexture);
            renderer.ppShader.SetTexture(1, "TexB", SurfaceCellsTexture);

            SurfaceCellsLookupTexture = TextureHelper.CreateIntTexture(NumCells.xyz, 2);
            mcShader.SetTexture(4, "SurfaceCellsLookup", SurfaceCellsLookupTexture);

            // Temp - test
            renderer.rtShader.SetTexture(0, "SurfaceCellsLookup", SurfaceCellsLookupTexture);
            renderer.rtShader.SetTexture(4, "SurfaceCellsLookup", SurfaceCellsLookupTexture);
            renderer.rtShader.SetInt("MipmapMaxDepth", SurfaceCellsMipmapDepth);

            svoShader.SetInt("MipmapMaxDepth", SurfaceCellsMipmapDepth);
            svoShader.SetVector("TextureMM1Dims", new Vector3(SurfaceCellsMM1Dims.x, SurfaceCellsMM1Dims.y, SurfaceCellsMM1Dims.z));
            renderer.rtShader.SetVector("TextureMM1Dims", new Vector3(SurfaceCellsMM1Dims.x, SurfaceCellsMM1Dims.y, SurfaceCellsMM1Dims.z));

            // Temporary
            textureManager.NoiseResolution = SurfaceCellsMM1Dims;
            textureManager.NoiseResolution.x = (int)(textureManager.NoiseResolution.x * 1.5);
            textureManager.SetPostProcessorShaderSettings();
        }
    }

    public void RunMarchingCubes()
    {
        UpdatePerFrame();

        RunMCShaders();
    }

    private void UpdatePerFrame()
    {
        renderer.ppShader.SetInt("FrameCount", renderer.FrameCount);
    }

    public void SetMCFluidVariables()
    {
        mcShader.SetInt("StaticVerticesNum", renderer.StaticVerticesNum);
        mcShader.SetInt("StaticTrisNum", renderer.StaticTrisNum);

        // Temp - test
        renderer.rtShader.SetInt("StaticTrisNum", renderer.StaticTrisNum);
        renderer.rtShader.SetInt("StaticVerticesNum", renderer.StaticVerticesNum);
    }

    public void RunMCShaders()
    {
        // Reset static tris & vertices values in case those have been changed between frames
        SetMCFluidVariables();

        // Calculate grid densities
        ComputeHelper.DispatchKernel(mcShader, "CalcGridDensities", NumCells.xyz, mcShaderThreadSize);

        // Find the mesh surface cells
        ComputeHelper.DispatchKernel(mcShader, "FindSurface", NumCells.xyz, mcShaderThreadSize);

        // Reset the counter for FluidTriMeshBufferAC before use
        FluidTriMeshBufferAC.SetCounterValue(0);

        // Generate the fluid mesh using marching cubes
        ComputeHelper.DispatchKernel(mcShader, "GenerateFluidMesh", NumCells.xyz, mcShaderThreadSize);

        // Get new fluid mesh length
        // GetAppendBufferCount() IS VERY EXPENIVE. USE ASYNC! ! ! ! !
        FluidMeshLength = 5000; // ComputeHelper.GetAppendBufferCount(FluidTriMeshBufferAC);

        // Set fluid mesh length settings
        mcShader.SetInt("LastFluidVerticesNum", LastFluidMeshLength * 3);
        mcShader.SetInt("LastFluidTrisNum", LastFluidMeshLength);
        LastFluidMeshLength = FluidMeshLength;
        mcShader.SetInt("FluidVerticesNum", FluidMeshLength * 3);
        mcShader.SetInt("FluidTrisNum", FluidMeshLength);

        // Transfer fluid mesh to the sort buffer
        ComputeHelper.DispatchKernel(mcShader, "TransferToSpatialLookup", FluidMeshLength, mcShaderThreadSize2);

        // Set necessary variables, and then run the spatial sort shader
        shaderHelper.UpdateSSShaderVariables(ssShader);

        // Sort the mesh
        RunSSShader();

        // Create the spatial lookup texture
        ComputeHelper.DispatchKernel(mcShader, "ConstructSpatialLookupTexture", NumCells.xyz, mcShaderThreadSize);

        // Transfer sorted fluid mesh to the render triangle buffer
        ComputeHelper.DispatchKernel(mcShader, "TransferToRenderer", FluidMeshLength, mcShaderThreadSize2);

        // Construct the sparse voxel traversal tree mipmap texture for the fluid mesh
        ConstructSparseVoxelTree();

        // SurfaceCellsLookup and Triangles[StaticTrisNum < (indices)] can now be used by the renderer
    }

    private void RunSSShader() => ComputeHelper.SpatialSort(ssShader, FluidMeshLength, ssShaderThreadSize, false);

    private void ConstructSparseVoxelTree()
    {
        for (int mipmapPassDepth = 1; mipmapPassDepth <= SurfaceCellsMipmapDepth; mipmapPassDepth++)
        {
            svoShader.SetInt("MipmapPassDepth", mipmapPassDepth);

            ComputeHelper.DispatchKernel(svoShader, "ConstructSparseVoxelTree", SurfaceCellsMM1Dims / Func.Pow2(mipmapPassDepth), mcShaderThreadSize);
        }
    }

    void OnDestroy()
    {
        ComputeHelper.Release(PointsBuffer, SpatialLookupBuffer, StartIndicesBuffer, FluidTriMeshBufferAC, FluidTriMeshSLBuffer, FluidStartIndicesBuffer);
    }
}