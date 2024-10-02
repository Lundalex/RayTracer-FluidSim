using Unity.Mathematics;
[System.Serializable]
public struct RenderSceneObject
{
    public float4x4 worldToLocalMatrix;
    public float4x4 localToWorldMatrix;
    public float areaApprox;
    public int materialIndex;
    public int bvStartIndex;
};