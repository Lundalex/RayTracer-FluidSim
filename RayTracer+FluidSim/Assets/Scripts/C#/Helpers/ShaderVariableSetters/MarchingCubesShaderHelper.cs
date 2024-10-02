using System;
using RendererResources;
using UnityEngine;

public class MarchingCubesShaderHelper : MonoBehaviour
{
    private MarchingCubes mCubes;

    public void ScriptSetup()
    {
        mCubes = this.gameObject.GetComponent<MarchingCubes>();
    }

    public void SetPPSettings (ComputeShader ppShader)
    {
        
    }

    public void UpdatePPVariables (ComputeShader ppShader)
    {

    }

    public void SetSSShaderBuffers (ComputeShader ssShader)
    {
        ssShader.SetBuffer(0, "FluidTriMeshSL", mCubes.FluidTriMeshSLBuffer);

        ssShader.SetBuffer(1, "FluidTriMeshSL", mCubes.FluidTriMeshSLBuffer);
        ssShader.SetBuffer(1, "FluidStartIndices", mCubes.FluidStartIndicesBuffer);
    }

    public void UpdateSSShaderVariables (ComputeShader ssShader)
    {
        ssShader.SetInt("NumCellsAll", mCubes.NumCellsAll);
    }
}