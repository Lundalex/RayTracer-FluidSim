using UnityEngine;
using Unity.Mathematics;

// Import utils from SimResources.cs
using SimResources;
using System;

public class ProgramManager : MonoBehaviour
{
#region Inspector

    [Header("Fluid Object")]
    // public float RotationSpeed;
    // public float ParticleSpheresRadius;
    // public float3 Rot;

    [Header("References")]
    // Class instances
    public Simulation sim;
    public MarchingCubes mCubes;
    public NewRenderer newRenderer;
    public TextureManager textureManager;
    public ProgramManagerShaderHelper shaderHelper;
    // Compute shaders
    public ComputeShader dtShader;
    public ComputeShader ssShader;
    private bool ProgramStarted = false;
#endregion

#region Shader Settings
    private const int dtShaderThreadSize = 512; // /1024
    private const int ssShaderThreadSize = 512; // /1024
#endregion

#region Other
    // private bool ProgramStarted = false;
#endregion

    private void Awake()
    {
        // Manage the setup order of all major class instances
        sim.ScriptSetup();
        mCubes.ScriptSetup();
        newRenderer.ScriptSetup();
        textureManager.ScriptSetup();
        ScriptSetup();
    }

    private void ScriptSetup()
    {
        InitBuffers();
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

    void InitBuffers()
    {

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

    void Update()
    {
        // Run simulation
        sim.RunTimeSteps();

        // Tranfer particle position data to dedicated points buffer, and apply spatial sorting
        PreparePointsData();

        // Run marching cubes to generate a mesh from the points
        mCubes.RunMarchingCubes();

        // Update script before render pass(es)
        newRenderer.ScriptUpdate();

        // Render the scene to a texture
        newRenderer.RenderScene();
    }

    void PreparePointsData()
    {
        // Transfer particle position data
        ComputeHelper.DispatchKernel(dtShader, "TransferParticleData", sim.ParticlesNum, dtShaderThreadSize);

        // Sort points (for processing by MS shader)
        ComputeHelper.SpatialSort(ssShader, mCubes.NumPoints, ssShaderThreadSize);
    }
}