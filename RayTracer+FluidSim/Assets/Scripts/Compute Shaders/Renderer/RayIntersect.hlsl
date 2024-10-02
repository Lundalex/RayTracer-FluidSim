TriHitInfo RayStaticTriangleIntersect(Ray ray, Vertex2 v0, Vertex2 v1, Vertex2 v2)
{
    float3 edgeAB = v1.pos - v0.pos;
    float3 edgeAC = v2.pos - v0.pos;
    float3 normalVector = cross(edgeAB, edgeAC);
    float3 ao = ray.pos - v0.pos;
    float3 dao = cross(ao, ray.dir);
 
    float determinant = -dot(ray.dir, normalVector);
    float invDet = 1 / determinant;
 
    // Calculate dst to tri & barycentric coordinates of intersection point
    float dst = dot(ao, normalVector) * invDet;
    float u = dot(edgeAC, dao) * invDet;
    float v = -dot(edgeAB, dao) * invDet;
    float w = 1 - u - v;
 
    // Initialize tri hit info
    TriHitInfo triHitInfo;
    triHitInfo.didHit = determinant >= 1E-8 && dst >= 0 && u >= 0 && v >= 0 && w >= 0;
    triHitInfo.uv = triHitInfo.didHit ? v0.uv * w + v1.uv * u + v2.uv * v : float2(0, 0);
    triHitInfo.dst = dst;
    return triHitInfo;
}

TriHitInfo RayDynamicTriangleIntersect(Ray ray, Vertex3 v0, Vertex3 v1, Vertex3 v2)
{
    float3 edgeAB = v1.pos - v0.pos;
    float3 edgeAC = v2.pos - v0.pos;
    float3 normalVector = cross(edgeAB, edgeAC);
    float3 ao = ray.pos - v0.pos;
    float3 dao = cross(ao, ray.dir);
 
    float determinant = -dot(ray.dir, normalVector);
    float invDet = 1 / determinant;
 
    // Calculate dst to tri & barycentric coordinates of intersection point
    float dst = dot(ao, normalVector) * invDet;
    float u = dot(edgeAC, dao) * invDet;
    float v = -dot(edgeAB, dao) * invDet;
    float w = 1 - u - v;
 
    // Initialize tri hit info
    TriHitInfo triHitInfo;
    triHitInfo.didHit = determinant >= 1E-8 && dst >= 0 && u >= 0 && v >= 0 && w >= 0;
    triHitInfo.uv = float2(0, 0);
    triHitInfo.dst = dst;
    return triHitInfo;
}

float RayBVIntersect(Ray ray, BV bv)
{
    float3 tMin = (bv.min - ray.pos) * ray.invDir;
    float3 tMax = (bv.max - ray.pos) * ray.invDir;
    float3 t1 = min(tMin, tMax);
    float3 t2 = max(tMin, tMax);
    float tNear = max(max(t1.x, t1.y), t1.z);
    float tFar = min(min(t2.x, t2.y), t2.z);
 
    bool didHit = tFar >= tNear && tFar > 0;
    float dst = didHit ? tNear > 0 ? tNear : 0 : 1.#INF;
 
    return dst;
};

float RayBoxIntersect(Ray ray, float3 boxMin, float3 boxMax)
{
    float3 tMin = (boxMin - ray.pos) * ray.invDir;
    float3 tMax = (boxMax - ray.pos) * ray.invDir;
    float3 t1 = min(tMin, tMax);
    float3 t2 = max(tMin, tMax);
    float tNear = max(max(t1.x, t1.y), t1.z);
    float tFar = min(min(t2.x, t2.y), t2.z);
 
    bool didHit = tFar >= tNear && tFar > 0;
    float dst = didHit ? tNear > 0 ? tNear : 0 : 1.#INF;
 
    return dst;
};

// Ray must intersect with box for this optimisation to work
void MoveToBox(inout Ray ray, float3 boxMin, float3 boxMax)
{
    float3 t1 = (boxMin - ray.pos) * ray.invDir;
    float3 t2 = (boxMax - ray.pos) * ray.invDir;

    float tNear = max(max(min(t1.x, t2.x), min(t1.y, t2.y)), min(t1.z, t2.z));

    tNear = max(tNear, 0.0f);

    ray.pos += tNear * ray.dir;
}