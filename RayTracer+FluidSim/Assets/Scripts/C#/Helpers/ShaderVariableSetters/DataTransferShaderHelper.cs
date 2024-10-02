using UnityEngine;

// Import utils from SimResources.cs
using SimResources;
public class DataTransferShaderHelper : MonoBehaviour
{
    private FluidManager fluidManager;

    public void ScriptSetup()
    {
        fluidManager = gameObject.GetComponent<FluidManager>();
    }

    public void SetDTShaderBuffers(ComputeShader dtShader)
    {
        dtShader.SetBuffer(0, "PDataB", fluidManager.sim.PDataBuffer);
        dtShader.SetBuffer(0, "PTypes", fluidManager.sim.PTypesBuffer);
        dtShader.SetBuffer(0, "Points", fluidManager.mCubes.PointsBuffer);
    }

    public void SetSSShaderBuffers (ComputeShader ssShader)
    {
        ssShader.SetBuffer(0, "Points", fluidManager.mCubes.PointsBuffer);
        ssShader.SetBuffer(0, "SpatialLookup", fluidManager.mCubes.SpatialLookupBuffer);

        ssShader.SetBuffer(1, "SpatialLookup", fluidManager.mCubes.SpatialLookupBuffer);

        ssShader.SetBuffer(2, "SpatialLookup", fluidManager.mCubes.SpatialLookupBuffer);
        ssShader.SetBuffer(2, "StartIndices", fluidManager.mCubes.StartIndicesBuffer);
    }

    public void SetSSSettings (ComputeShader ssShader)
    {
        // Num constants
        ssShader.SetVector("NumCells", new Vector4(fluidManager.mCubes.NumCells.x, fluidManager.mCubes.NumCells.y, fluidManager.mCubes.NumCells.z, fluidManager.mCubes.NumCells.w));
        ssShader.SetInt("NumCellsAll", fluidManager.mCubes.NumCellsAll);
        ssShader.SetFloat("CellSize", fluidManager.mCubes.sim.MaxInfluenceRadius);
        ssShader.SetInt("NumPoints", fluidManager.mCubes.NumPoints);
        ssShader.SetInt("NumPoints_NextPow2", Func.NextPow2(fluidManager.mCubes.NumPoints_NextPow2));
    }
}