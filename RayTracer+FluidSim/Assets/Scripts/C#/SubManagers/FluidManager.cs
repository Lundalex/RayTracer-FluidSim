using System;
using UnityEngine;

public class FluidManager : MonoBehaviour
{
    [Header("Material")]
    public int MaterialIndex = 0;

#region "Run Time Set Variables"
    [NonSerialized] public Simulation sim;
    [NonSerialized] public DataTransfer dataTransfer;
    [NonSerialized] public MarchingCubes mCubes;
#endregion

    public void ScriptSetup()
    {
        SetReferences();

        sim.ScriptSetup();
        mCubes.ScriptSetup();
        dataTransfer.ScriptSetup();
    }

    public void UpdateFluid()
    {
        // Run SPH simulation
        sim.RunTimeSteps();

        // Transfer particle position data to dedicated points buffer. Then, apply spatial sorting
        dataTransfer.PreparePointsData();

        // Run marching cubes to generate a mesh from the point buffer
        mCubes.RunMarchingCubes();
    }

    private void SetReferences()
    {
        sim = this.gameObject.GetComponent<Simulation>();
        dataTransfer = this.gameObject.GetComponent<DataTransfer>();
        mCubes = this.gameObject.GetComponent<MarchingCubes>();
    }
}
