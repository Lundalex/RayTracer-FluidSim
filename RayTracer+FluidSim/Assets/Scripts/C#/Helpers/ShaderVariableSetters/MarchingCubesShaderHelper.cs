using System;
using RendererResources;
using UnityEngine;

public class MarchingCubesShaderHelper : MonoBehaviour
{
    public MarchingCubes m;

    public void SetPPSettings (ComputeShader ppShader)
    {
        
    }

    public void UpdatePPVariables (ComputeShader ppShader)
    {

    }

    public void SetSSShaderBuffers (ComputeShader ssShader)
    {
        ssShader.SetBuffer(0, "FluidTriMeshSL", m.FluidTriMeshSLBuffer);

        ssShader.SetBuffer(1, "FluidTriMeshSL", m.FluidTriMeshSLBuffer);
        ssShader.SetBuffer(1, "FluidStartIndices", m.FluidStartIndicesBuffer);
    }

    public void UpdateSSShaderVariables (ComputeShader ssShader)
    {
        ssShader.SetInt("NumCellsAll", m.NumCellsAll);
        ssShader.SetInt("SortLength", m.FluidMeshLength);
        ssShader.SetInt("SortLengthNextPow2", Func.NextPow2(m.FluidMeshLength));
    }
}