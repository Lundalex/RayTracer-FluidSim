using UnityEngine;

// Import utils from SimResources.cs
using SimResources;
public class ProgramManagerShaderHelper : MonoBehaviour
{
    public ProgramManager manager;

// --- SHADER BUFFERS ---

    public void SetDTShaderBuffers(ComputeShader dtShader)
    {
        dtShader.SetBuffer(0, "PDataB", manager.sim.PDataBuffer);
        dtShader.SetBuffer(0, "PTypes", manager.sim.PTypesBuffer);
        dtShader.SetBuffer(0, "Points", manager.mCubes.PointsBuffer);
    }

    public void SetSSShaderBuffers (ComputeShader ssShader)
    {
        ssShader.SetBuffer(0, "Points", manager.mCubes.PointsBuffer);
        ssShader.SetBuffer(0, "SpatialLookup", manager.SpatialLookupBuffer);

        ssShader.SetBuffer(1, "SpatialLookup", manager.SpatialLookupBuffer);

        ssShader.SetBuffer(2, "SpatialLookup", manager.SpatialLookupBuffer);
        ssShader.SetBuffer(2, "StartIndices", manager.StartIndicesBuffer);
    }

// --- SHADER SETTINGS / VARIABLES ---

    public void SetSSSettings (ComputeShader ssShader)
    {
        // Num constants
        ssShader.SetVector("NumCells", new Vector4(manager.mCubes.NumCells.x, manager.mCubes.NumCells.y, manager.mCubes.NumCells.z, manager.mCubes.NumCells.w));
        ssShader.SetInt("NumCellsAll", manager.mCubes.NumCellsAll);
        ssShader.SetFloat("CellSize", manager.mCubes.CellSize);
        ssShader.SetInt("NumPoints", manager.NumPoints);
        ssShader.SetInt("NumPoints_NextPow2", Func.NextPow2(manager.NumPoints_NextPow2));
    }
}