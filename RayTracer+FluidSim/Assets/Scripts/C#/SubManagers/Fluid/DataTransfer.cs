using System;
using UnityEngine;

public class DataTransfer : MonoBehaviour
{
#region Inspector Set References
    public ComputeShader dtShader;
    public ComputeShader ssShader;
#endregion

#region Run Time Set References
    private Simulation sim;
    private MarchingCubes mCubes;
    private DataTransferShaderHelper shaderHelper;
#endregion

#region Other
    private bool ProgramStarted = false;
#endregion

#region Shader Settings
    private const int dtShaderThreadSize = 512; // /1024
    private const int ssShaderThreadSize = 512; // /1024
#endregion

    public void ScriptSetup()
    {
        SetReferences();

        SetBufferData();
        UpdateSettings();

        ProgramStarted = true;
    }

    private void OnValidate()
    {
        if (ProgramStarted)
        {
            UpdateSettings();
        }
    }

    private void SetReferences()
    {
        shaderHelper = this.gameObject.GetComponent<DataTransferShaderHelper>();
        shaderHelper.ScriptSetup();
        mCubes = this.gameObject.GetComponent<MarchingCubes>();
        sim = this.gameObject.GetComponent<Simulation>();
    }

    private void SetBufferData()
    {
        shaderHelper.SetDTShaderBuffers(dtShader);
        
        shaderHelper.SetSSShaderBuffers(ssShader);
        shaderHelper.SetSSSettings(ssShader);
    }

    private void UpdateSettings()
    {
        dtShader.SetInt("ParticlesNum", sim.ParticlesNum);
        dtShader.SetInt("ChunksNumAll", sim.ChunksNumAll);
        dtShader.SetInt("PTypesNum", sim.PTypes.Length);
        dtShader.SetVector("SimBoundraryDims", new Vector3(sim.Width, sim.Height, sim.Depth));

        dtShader.SetVector("Rot", new Vector3(0, 0, 0));
    }

    public void PreparePointsData()
    {
        // Transfer particle position data
        ComputeHelper.DispatchKernel(dtShader, "TransferParticleData", sim.ParticlesNum, dtShaderThreadSize);

        // Sort points (for processing by MS shader)
        ComputeHelper.SpatialSort(ssShader, mCubes.NumPoints, ssShaderThreadSize);
    }
}