using UnityEngine;
using Unity.Mathematics;
using System;

using SimResources;
using UnityEditor;
public class MarchingCubes : MonoBehaviour
{
    // Scene-related variables
    public float CellSize;
    public float Threshold;
    [NonSerialized] public int4 NumCells;
    [NonSerialized] public int NumCellsAll;

    // Script references
    public Simulation sim;

    // Shader references
    public ComputeShader mcShader;

    // Shader settings
    private const int mcShaderThreadSize = 8;
    private const int mcShaderThreadSize2 = 512;
#region Run Time Set Variables
    [NonSerialized] public int NumPoints;
    [NonSerialized] public int NumPoints_NextPow2;
    private int FluidTriMeshLength;
#endregion

    // Buffers and textures
    public ComputeBuffer PointsBuffer;
    public ComputeBuffer SpatialLookupBuffer;
    public ComputeBuffer StartIndicesBuffer;
    public ComputeBuffer FluidTriMeshBufferAC;
    // private ComputeBuffer AC_SurfaceCells;
    // private ComputeBuffer AC_FluidTriMesh;
    // private ComputeBuffer CB_A;
    private RenderTexture GridDensitiesTexture;
    private RenderTexture SurfaceCellsTexture;

    public void ScriptSetup()
    {
        UpdateSettings();
        InitBuffers();
        InitTextures();
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
        mcShader.SetFloat("Threshold", Threshold);
        mcShader.SetVector("NumCells", new Vector4(NumCells.x, NumCells.y, NumCells.z, NumCells.x * NumCells.y));
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

        ComputeHelper.CreateStructuredBuffer<MCTri>(ref FluidTriMeshBufferAC, FluidTriMeshLength);
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
        }
        if (SurfaceCellsTexture == null)
        {
            SurfaceCellsTexture = TextureHelper.CreateTexture(NumCells.xyz, 1);
            SurfaceCellsTexture.Create();
            mcShader.SetTexture(1, "SurfaceCells", SurfaceCellsTexture);
            mcShader.SetTexture(2, "SurfaceCells", SurfaceCellsTexture);
        }
    }

    public void RunMarchingCubes()
    {
        
    }

    public void RunMCShader()
    {
        // Calculate grid densities
        ComputeHelper.DispatchKernel(mcShader, "CalcGridDensities", NumCells.xyz, mcShaderThreadSize);

        // Find the mesh surface cells
        ComputeHelper.DispatchKernel(mcShader, "FindSurface", NumCells.xyz, mcShaderThreadSize);

        // Reset the counter for FluidTriMeshBufferAC before use
        FluidTriMeshBufferAC.SetCounterValue(0);

        // Generate the fluid mesh using marching cubes
        ComputeHelper.DispatchKernel(mcShader, "GenerateFluidMesh", FluidTriMeshLength, mcShaderThreadSize2);

        bool doFetchACBufferLength = true;
        int fluidTriMeshLength = 0;
        if (doFetchACBufferLength) fluidTriMeshLength = ComputeHelper.GetAppendBufferCount(FluidTriMeshBufferAC);
    }

    void OnDestroy()
    {
        ComputeHelper.Release(PointsBuffer, SpatialLookupBuffer, StartIndicesBuffer);
    }
}