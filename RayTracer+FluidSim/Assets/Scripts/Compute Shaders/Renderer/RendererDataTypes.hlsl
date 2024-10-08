// --- Rendered object types ---

struct Triangle
{
    int vertex0Index;
    int vertex1Index;
    int vertex2Index;
    float3 localNormal; // TBR
    float area;
};
struct Vertex2
{
    float3 pos;
    float2 uv;
};
struct Vertex3
{
    float3 pos;
};
struct BV
{
    float3 min;
    float3 max;
    int indexA; // -childIndexA / componentsStart, a < 0 <= b
    int indexB; // -childIndexB / totComponents, a < 0 <= b
};
struct SceneObject
{
    float4x4 worldToLocalMatrix;
    float4x4 localToWorldMatrix;
    float areaApprox;
    int materialIndex;
    int bvStartIndex;
};
struct LightObject
{
    float4x4 localToWorldMatrix;
    float areaApprox;
    float brightness;
    int triStart;
    int totTris;
};
struct Material2
{
    float brightness;

    // Color map
    float3 col;
    int2 colTexLoc;
    int2 colTexDims;

    // Bump map
    float bump;
    int2 bumpTexLoc;
    int2 bumpTexDims;

    // Roughness
    float roughness;
    int2 roughnessTexLoc;
    int2 roughnessTexDims;

    // Metallicity
    float metallicity;
    int2 metallicityTexLoc;
    int2 metallicityTexDims;
    
    // Normals map
    int2 normalsTexLoc;
    int2 normalsTexDims;
};
 
// --- Ray tracer structs ---
 
struct Ray
{
    float3 pos;
    float3 dir;
    float3 invDir;
};
struct HitInfo
{
    float dst; // dst == 1.#INF => didHit = false
    float3 hitPoint;
    float2 uv;
    float3 normal;
    float3 incomingDir;
    int materialIndex;
};
struct TriHitInfo
{
    bool didHit;
    float dst;
    float2 uv;
    int triIndex;
};
struct TraceInfo
{
    float totTravelDst;
    float3 rayColor;
    float3 directLight;
    float3 indirectLight;
    float3 firstHitPoint;
};
struct Reservoir
{
    int chosenIndex;
    float chosenWeight;
    float totWeights;
};
struct CandidateReservoir
{
    float3 dir;
    float3 hitPoint;
    float3 normal;
    float brdf;
    float chosenWeight;
    float totWeights;
    float totCandidates; // "float" since temporal reuse averages the values from sequential frames
};
struct DebugData
{
    int triChecks;
    int bvVoxelChecks;
    bool errorhasOccured;
};
struct NodeDst
{
    float dst;
    uint3 pixelID;
};

// Basic Tri struct for marching cubes shader
struct MCTri
{
    float3 vertex0;
    float3 vertex1;
    float3 vertex2;
    int cellKey;
};
struct NearInfo
{
    float val;
    int materialIndex;
};
 
// --- Init functions ---

Vertex3 InitVertex3(float3 pos)
{
    Vertex3 vertex;
    vertex.pos = pos;

    return vertex;
}

BV InitBV()
{
    BV bv;
    bv.min = 0;
    bv.max = 0;
    bv.indexA = 0;
    bv.indexB = 0;

    return bv;
}

Triangle InitTriangle()
{
    Triangle tri;
    tri.vertex0Index = 0;
    tri.vertex1Index = 0;
    tri.vertex2Index = 0;
    tri.localNormal = 0;
    tri.area = 0;

    return tri;
}

Triangle InitTriangle(int vertex0Index, int vertex1Index, int vertex2Index, float3 localNormal, float area)
{
    Triangle tri;
    tri.vertex0Index = vertex0Index;
    tri.vertex1Index = vertex1Index;
    tri.vertex2Index = vertex2Index;
    tri.localNormal = localNormal;
    tri.area = area;

    return tri;
}
 
Reservoir InitReservoir(int firstElementIndex, float firstElementWeight)
{
    Reservoir reservoir;
    reservoir.chosenIndex = firstElementIndex;
    reservoir.chosenWeight = firstElementWeight;
    reservoir.totWeights = firstElementWeight;
 
    return reservoir;
}
Ray InitRay(float3 pos, float3 dir)
{
    Ray ray;
    ray.pos = pos;
    ray.dir = dir;
    ray.invDir = 1 / dir;
 
    return ray;
}
Ray InitRay()
{
    Ray ray;
    ray.pos = 0;
    ray.dir = 0;
    ray.invDir = 0;
 
    return ray;
}
HitInfo InitHitInfo()
{
    HitInfo hitInfo;
    hitInfo.dst = 1.#INF;
    hitInfo.hitPoint = -1;
    hitInfo.uv = 0;
    hitInfo.normal = 0;
    hitInfo.incomingDir = 0;
    hitInfo.materialIndex = 0;
 
    return hitInfo;
}
DebugData InitDebugData()
{
    DebugData debugData;
    debugData.triChecks = 0;
    debugData.bvVoxelChecks = 0;
    debugData.errorhasOccured = false;
 
    return debugData;
}
NodeDst InitNodeDst()
{
    NodeDst nodeDst;
    nodeDst.dst = 1.#INF;
    nodeDst.pixelID = 0;

    return nodeDst;
}
NodeDst InitNodeDst(float dst, uint3 pixelID)
{
    NodeDst nodeDst;
    nodeDst.dst = dst;
    nodeDst.pixelID = pixelID;

    return nodeDst;
}
void SwapNodeDst(inout NodeDst a, inout NodeDst b)
{
    NodeDst temp = a;
    a = b;
    b = temp;
}
Material2 InitMaterial()
{
    Material2 material;
    material.brightness = 0;

    // Color map
    material.col = 0;
    material.colTexLoc = 0;
    material.colTexDims = 0;

    // Bump map
    material.bump = 0;
    material.bumpTexLoc = 0;
    material.bumpTexDims = 0;

    // Roughness
    material.roughness = 0;
    material.roughnessTexLoc = 0;
    material.roughnessTexDims = 0;

    // Metallicity
    material.metallicity = 0;
    material.metallicityTexLoc = 0;
    material.metallicityTexDims = 0;

    // Normals map
    material.normalsTexLoc = 0;
    material.normalsTexDims = 0;
 
    return material;
}
CandidateReservoir InitCandidateReservoir(float3 dir, float3 hitPoint, float3 normal, float brdf, float chosenWeight, float totWeights, int totCandidates)
{
    CandidateReservoir candidateReservoir;
    candidateReservoir.dir = dir;
    candidateReservoir.hitPoint = hitPoint;
    candidateReservoir.normal = normal;
    candidateReservoir.brdf = brdf;
    candidateReservoir.chosenWeight = chosenWeight;
    candidateReservoir.totWeights = totWeights;
    candidateReservoir.totCandidates = totCandidates;
 
    return candidateReservoir;
}
CandidateReservoir InitCandidateReservoir()
{
    CandidateReservoir candidateReservoir;
    candidateReservoir.dir = 0;
    candidateReservoir.hitPoint = 0;
    candidateReservoir.normal = 0;
    candidateReservoir.brdf = 0;
    candidateReservoir.chosenWeight = 0;
    candidateReservoir.totWeights = 0;
    candidateReservoir.totCandidates = 0;
 
    return candidateReservoir;
}