#include "../Constants.hlsl"
#include "RendererDataTypes.hlsl"

float MSDensityKernel(float dst, float radius)
{
	if (dst < radius)
	{
        float dstR = dst / radius;
        return sqrt(1 - dstR);
	}
	return 0;
}

// Sort an array of size 8 using bubble sort
void SortDescendingBubbleSort_NodeDst8(inout NodeDst arr[8], int length)
{
    for (int i = 0; i < length; i++)
    {
        for (int j = 0; j < length - 1 - i; j++) // length - 1 - i to avoid comparing sorted elements
        {
            if (arr[j].dst < arr[j + 1].dst)
            {
                SwapNodeDst(arr[j], arr[j + 1]);
            }
        }
    }
}

// Sort an array of size 8 using insertion sort
void SortDescendingInsertionSort_NodeDst8(inout NodeDst arr[8], uint length)
{
    for (int i = 1; i < length; i++)
    {
        NodeDst keyVal = arr[i];
        int j = i - 1;
        while (j >= 0 && arr[j].dst < keyVal.dst)
        {
            arr[j + 1] = arr[j];
            j--;
        }
        arr[j + 1] = keyVal;
    }
}

void ReverseArrayUint4_50(inout uint4 arr[50], uint startIndex, uint endIndex)
{
    uint iterations = (uint)((endIndex - startIndex) * 0.5);
    for (uint i = 0; i < iterations; i++)
    {
        SwapUint4(arr[startIndex + i], arr[endIndex - i - 1]);
    }
}

void ApplyTransformF3x3(float3 rot, inout float3 a, inout float3 b, inout float3 c)
{
    float cosX = cos(rot.x);
    float sinX = sin(rot.x);
    float cosY = cos(rot.y);
    float sinY = sin(rot.y);
    float cosZ = cos(rot.z);
    float sinZ = sin(rot.z);

    // Combine rotation matrices into a single matrix
    float3x3 rotationMatrix = float3x3(
        cosY * cosZ,                             cosY * sinZ,                           -sinY,
        sinX * sinY * cosZ - cosX * sinZ,   sinX * sinY * sinZ + cosX * cosZ,  sinX * cosY,
        cosX * sinY * cosZ + sinX * sinZ,   cosX * sinY * sinZ - sinX * cosZ,  cosX * cosY
    );

    // Apply the combined rotation matrix to each vertex
    a = mul(rotationMatrix, a);
    b = mul(rotationMatrix, b);
    c = mul(rotationMatrix, c);
}

float sqr(float a)
{
	return a * a;
}

float avg(float a, float b) // float version
{
    return .5 * (a + b);
}
float2 avg(float2 a, float2 b) // float2 version
{
    return .5 * (a + b);
}
float3 avg(float3 a, float3 b) // float3 version
{
    return .5 * (a + b);
}

float dot2(float3 a) // float3 version
{
    return dot(a, a);
}
float dot2(float2 a) // float2 version
{
    return dot(a, a);
}

uint pow2(uint exponent)
{
    return 1 << exponent;
}

int3 ceilF3(float3 a)
{
    return int3(ceil(a.x), ceil(a.y), ceil(a.z));
}

int2 ceilF2(float2 a)
{
    return int2(ceil(a.x), ceil(a.y));
}

int3 floorF3(float3 a)
{
    return int3(floor(a.x), floor(a.y), floor(a.z));
}

int2 floorF2(float2 a)
{
    return int2(floor(a.x), floor(a.y));
}

uint NextRandom(inout uint state)
{
    state = state * 747796405 + 2891336453;
    uint result = ((state >> ((state >> 28) + 4)) ^ state) * 277803737;
    result = (result >> 22) ^ result;
    return result;
}

float randNormalized(inout uint state)
{
    return NextRandom(state) / 4294967295.0; // 2^32 - 1
}

int randIntSpan(int a, int b, inout uint state)
{
    float randNorm = randNormalized(state);
    int diff = b - a;
    int offset = (int)(diff * randNorm);
    int result = a + offset;
    return result;
}

bool weightedRand(float a, float b, inout uint state)
{
    float randNorm = randNormalized(state);
    
    float relRand = randNorm * b;
    return relRand < a;
}

float randValueNormalDistribution(inout uint state)
{
    float theta = 2 * PI * randNormalized(state);
    float rho = sqrt(-2 * log(randNormalized(state)));
    return rho * cos(theta);
}

float3 randPointOnUnitSphere(inout uint state)
{
    float x = randValueNormalDistribution(state);
    float y = randValueNormalDistribution(state);
    float z = randValueNormalDistribution(state);
    return normalize(float3(x, y, z));
}

float2 randPointInCircle(inout uint state)
{
    float angle = randNormalized(state) * 2 * PI;
    float2 pointOnCircle = float2(cos(angle), sin(angle));
    return pointOnCircle * sqrt(randNormalized(state));
}

float angleBetweenNormals(float3 a, float3 b)
{
    float dotProduct = dot(a, b);
    dotProduct = clamp(dotProduct, -1.0, 1.0);
    float angleRadians = acos(dotProduct);
    float angleDegrees = degrees(angleRadians);

    return angleDegrees;
}

void ensureMinValue(inout float value, float minValue = MIN_NON_ZERO)
{
    float valueSign = (value >= 0.0) ? 1.0 : -1.0;
    value = (abs(value) > minValue) ? value : (valueSign * minValue);
}