using UnityEngine;
using Unity.Mathematics;
using System;

using SimResources;
using UnityEditor;
public class MarchingCubes : MonoBehaviour
{
    // Scene-related variables
    public float CellSize;
    [NonSerialized] public int4 NumCells;
    [NonSerialized] public int NumCellsAll;
    public float ThresholdMS;

    // Script references
    public Simulation sim;

    // Shader references
    public ComputeShader mcShader;

    // Shader settings
    private const int msShaderThreadSize = 8;
    private const int msShaderThreadSize2 = 512;

    // Buffers and textures
    public ComputeBuffer PointsBuffer;
    // private ComputeBuffer AC_SurfaceCells;
    // private ComputeBuffer AC_FluidTriMesh;
    // private ComputeBuffer CB_A;
    private RenderTexture T_GridDensities;
    private RenderTexture T_SurfaceCells;

    public void ScriptSetup()
    {
        UpdateSettings();
        InitBuffers();
        InitializeTextures();
    }

    private void UpdateSettings()
    {
        if (!sim.ProgramStarted) Debug.LogWarning("Marching cubes class initiated before Simulation class. Variables might not be set correctly");

        float3 simMaxBounds = new float3(sim.Width, sim.Height, sim.Depth);

        NumCells = new(Mathf.CeilToInt(simMaxBounds.x / CellSize),
                        Mathf.CeilToInt(simMaxBounds.y / CellSize),
                        Mathf.CeilToInt(simMaxBounds.z / CellSize), 0);
        NumCells.w = NumCells.x * NumCells.y;
        NumCellsAll = NumCells.x * NumCells.y * NumCells.z;
    }

    private void InitBuffers()
    {
        if (!sim.ProgramStarted) Debug.LogWarning("Marching cubes class initiated before Simulation class. Variables might not be set correctly");
        ComputeHelper.CreateStructuredBuffer<float3>(ref PointsBuffer, sim.ParticlesNum);
    }

    private void InitializeTextures()
    {

    }

    public void RunMCShader()
    {



        // ComputeHelper.DispatchKernel(mcShader, "CalcGridDensities", NumCells.xyz, msShaderThreadSize);
        // AC_SurfaceCells.SetCounterValue(0);
        // ComputeHelper.DispatchKernel(mcShader, "FindSurface", NumCells.xyz, msShaderThreadSize);

        // int SC_len = ComputeHelper.GetAppendBufferCount(AC_SurfaceCells, CB_A);

        // AC_FluidTriMesh.SetCounterValue(0);
        // ComputeHelper.DispatchKernel(mcShader, "GenerateFluidMesh", Mathf.Max(SC_len, 1), msShaderThreadSize2);

        // int FTM_len = ComputeHelper.GetAppendBufferCount(AC_FluidTriMesh, CB_A);

        // UpdateTris(FTM_len);

        // ComputeHelper.DispatchKernel(mcShader, "DeleteFluidMesh", Mathf.Max(DynamicNumTris, 1), msShaderThreadSize2);
        // ComputeHelper.DispatchKernel(mcShader, "TransferFluidMesh", Mathf.Max(FTM_len, 1), msShaderThreadSize2);
    }

    void OnDestroy()
    {
        ComputeHelper.Release(PointsBuffer);
    }
}