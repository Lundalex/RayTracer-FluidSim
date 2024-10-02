using UnityEngine;

public class ProgramManager : MonoBehaviour
{
#region Inspector
    [Header("References")]
    public FluidManager fluidManager;
    public NewRenderer newRenderer;
    public TextureManager textureManager;
#endregion


    private void Awake()
    {
        // Manage the setup order of all major class instances
        fluidManager.ScriptSetup();
        newRenderer.ScriptSetup();
        textureManager.ScriptSetup();
    }

    void Update()
    {
        // Allow the fluid objects to update all internal components (SPH, DT, MC)
        fluidManager.UpdateFluid();

        // Update the renderer before render pass(es)
        newRenderer.ScriptUpdate();

        // Render the scene to a texture
        newRenderer.RenderScene();
    }
}