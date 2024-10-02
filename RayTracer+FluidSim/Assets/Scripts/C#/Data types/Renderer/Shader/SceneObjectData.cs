using Unity.Mathematics;
using RendererResources;
using UnityEngine;
[System.Serializable]
public struct SceneObjectData : IBVHComponent
{
    public float4x4 worldToLocalMatrix;
    public float4x4 localToWorldMatrix;
    public float areaApprox;
    public int materialIndex;
    public int bvStartIndex;
    public int maxDepthBVH;
    public float3 min;
    public float3 max;
    public readonly float3 GetMin() => min;
    public readonly float3 GetMax() => max;
    public readonly void CalcMax(float3 v0Pos = new float3(), float3 v1Pos = new float3(), float3 v2Pos = new float3())
    {
        Debug.Log("SceneObjectData method CalcMax() not allowed to be used!");
    }

    public readonly void CalcMin(float3 v0Pos = new float3(), float3 v1Pos = new float3(), float3 v2Pos = new float3())
    {
        Debug.Log("SceneObjectData method CalcMin() not allowed to be used!");
    }
    public readonly float3 GetMid() => Func.Avg(GetMin(), GetMax());
};