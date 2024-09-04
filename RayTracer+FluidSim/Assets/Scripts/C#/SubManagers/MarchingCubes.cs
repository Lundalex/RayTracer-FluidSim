using UnityEngine;
using Unity.Mathematics;
using System;

// Import utils from SimResources.cs
using SimResources;
public class MarchingCubes : MonoBehaviour
{
    // Scene-related variables
    public float CellSize;
    public float DensityRadius;
    public float Threshold;
    public float DistanceMultiplier;
    public float DensityMultiplier;

    // Script references
    public Simulation sim;
    public new NewRenderer renderer;
    public TextureManager textureManager;

    // Shader references
    public ComputeShader mcShader;
    // Run-time set variables
    [NonSerialized] public int4 NumCells;
    [NonSerialized] public int NumCellsAll;

    // Shader settings
    private const int mcShaderThreadSize = 8;
    private const int mcShaderThreadSize2 = 512;
#region Run Time Set Variables
    [NonSerialized] public int NumPoints;
    [NonSerialized] public int NumPoints_NextPow2;
    private int FluidMeshMaxCapacity = 20000;
    private int FluidMeshLength = 0;
#endregion

    // Buffers and textures
    public ComputeBuffer PointsBuffer;
    public ComputeBuffer SpatialLookupBuffer;
    public ComputeBuffer StartIndicesBuffer;
    public ComputeBuffer FluidTriMeshBufferAC;
    // private ComputeBuffer AC_SurfaceCells;
    // private ComputeBuffer AC_FluidTriMesh;
    // private ComputeBuffer CB_A;
    [NonSerialized] public RenderTexture GridDensitiesTexture;
    [NonSerialized] public RenderTexture SurfaceCellsTexture;
    private bool ProgramStarted = false;

    public void ScriptSetup()
    {
        UpdateSettings();
        InitBuffers();
        InitTextures();

        ProgramStarted = true;
        Debug.Log(FluidMeshMaxCapacity);
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

        mcShader.SetFloat("CellSize", CellSize);
        mcShader.SetFloat("DensityRadius", DensityRadius);
        mcShader.SetFloat("Threshold", Threshold);
        mcShader.SetVector("NumCells", new Vector4(NumCells.x, NumCells.y, NumCells.z, NumCells.x * NumCells.y));

        mcShader.SetFloat("DensityMultiplier", DensityMultiplier);
        mcShader.SetFloat("DistanceMultiplier", DistanceMultiplier);
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

        ComputeHelper.CreateAppendBuffer<MCTri>(ref FluidTriMeshBufferAC, 20000);
        mcShader.SetBuffer(2, "FluidTriMeshAPPEND", FluidTriMeshBufferAC);
        mcShader.SetBuffer(4, "FluidTriMeshCONSUME", FluidTriMeshBufferAC);
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
            SurfaceCellsTexture = TextureHelper.CreateTexture(NumCells.xyz, 1);
            SurfaceCellsTexture.Create();
            mcShader.SetTexture(1, "SurfaceCells", SurfaceCellsTexture);
            mcShader.SetTexture(2, "SurfaceCells", SurfaceCellsTexture);
            renderer.ppShader.SetTexture(1, "TexB", SurfaceCellsTexture);
        }
    }

    public void RunMarchingCubes()
    {
        UpdatePerFrame();

        RunMCShader();
    }

    private void UpdatePerFrame()
    {
        renderer.ppShader.SetInt("FrameCount", renderer.FrameCount);
    }

    public void SetMCFluidSettings()
    {
        mcShader.SetInt("FluidMeshLength", FluidMeshLength);
        // mcShader.SetInt("ReservedTrisNum", );
        // mcShader.SetInt("ReservedVerticesNum", );
    }

    public void RunMCShader()
    {
        // Delete previous fluid mesh
        SetMCFluidSettings();
        // if (FluidMeshLength > 0) ComputeHelper.DispatchKernel(mcShader, "DeleteFluidMesh", FluidMeshLength, mcShaderThreadSize2);

        // Calculate grid densities
        ComputeHelper.DispatchKernel(mcShader, "CalcGridDensities", NumCells.xyz, mcShaderThreadSize);

        // Find the mesh surface cells
        ComputeHelper.DispatchKernel(mcShader, "FindSurface", NumCells.xyz, mcShaderThreadSize);

        // Reset the counter for FluidTriMeshBufferAC before use
        FluidTriMeshBufferAC.SetCounterValue(0);

        // Generate the fluid mesh using marching cubes
        ComputeHelper.DispatchKernel(mcShader, "GenerateFluidMesh", NumCells.xyz, mcShaderThreadSize);

        // Get new fluid mesh length
        if (true) FluidMeshLength = ComputeHelper.GetAppendBufferCount(FluidTriMeshBufferAC);

        // CALCULATE BVH ! ! ! ! ! !  ! ! ! ! ! !  ! ! !   !  ! !  !

        // Transfer fluid mesh to the render triangle buffer
        // ComputeHelper.DispatchKernel(mcShader, "TransferFluidMesh", FluidMeshLength, mcShaderThreadSize2);
        
        // DEBUG
        // Debug.Log(fluidTriMeshLength);
        // MCTri[] test = new MCTri[fluidTriMeshLength];
        // FluidTriMeshBufferAC.GetData(test);
        // float3[] test2 = new float3[sim.ParticlesNum];
        // int2[] test3 = new int2[NumPoints_NextPow2];
        // int[] test4 = new int[NumPoints_NextPow2];
        // PointsBuffer.GetData(test2);
        // SpatialLookupBuffer.GetData(test3);
        // StartIndicesBuffer.GetData(test4);
        // int a = 0;
    }

    void OnDestroy()
    {
        ComputeHelper.Release(PointsBuffer, SpatialLookupBuffer, StartIndicesBuffer, FluidTriMeshBufferAC);
    }
}