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
void SortAscendingSize8BubbleSort(inout float arr1[8], inout uint3 arr2[8])
{
    for (int i = 0; i < 8; i++)
    {
        for (int j = 0; j < 7 - i; j++) // 7-i ensures we don't compare sorted elements
        {
            if (arr1[j] > arr1[j + 1])
            {
                // Swap [j] and [j + 1] for arr1 and arr2
                SwapFloat(arr1[j], arr1[j + 1]);
                SwapUint3(arr2[j], arr2[j + 1]);
            }
        }
    }
}

// Sort an array of size 8 using insertion sort
void SortAscendingSize8InsertionSort(inout float distances[8], inout uint3 pixelIDs[8])
{
    for (int i = 1; i < 8; i++)
    {
        float keyDistance = distances[i];
        uint3 keyPixelID = pixelIDs[i];
        int j = i - 1;

        // Move elements greater than keyDistance to one position ahead
        while (j >= 0 && distances[j] > keyDistance)
        {
            distances[j + 1] = distances[j];
            pixelIDs[j + 1] = pixelIDs[j];
            j--;
        }
        distances[j + 1] = keyDistance;
        pixelIDs[j + 1] = keyPixelID;
    }
}

void ApplyTransformTriVertices(float3 rot, inout float3 a, inout float3 b, inout float3 c)
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