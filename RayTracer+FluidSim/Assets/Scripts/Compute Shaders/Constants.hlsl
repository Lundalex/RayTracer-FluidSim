// --- Simulation ---

static const int TN_PS = 256; // ParticleSim
static const int TN_RB = 32; // RigidbodySim
static const int TN_IPS = 512; // InterParticleSprings

static const float PI = 3.1415926;
static const float SmoothViscosityLaplacianFactor = 45 / PI;

// --- Renderer ---

static const int TN_RT = 8; // RayTracer
static const int TN_PC = 256; // PreCalc
static const int TN_PP = 8; // PostProcessing

static const uint MAX_DDA_ITERATIONS = 100;
static const uint MAX_VOXEL_TREE_ITERATIONS = 100;
static const uint FLUID_NODE_STACK_LENGTH = 50;
static const uint MAX_BVH_DEPTH = 32;

// --- Marching Cubes ---

static const int TN_MC = 8; // MarchingCubes
static const int TN_MC2 = 512; // MarchingCubes2

// --- Textures ---

static const int TN_NG = 8; // NoiseGenerator
static const int TN_TC = 8; // TextureComposition
static const int TN_TC2 = 8; // TextureComposition2

// --- Data Transfer ---

static const int TN_DT = 512;

// --- Spatial Sort ---

static const int TN_SS_DT = 512; // DataTransfer
static const int TN_SS_MC = 512; // MarchingCubes
static const int TN_SS_S = 512; // Simulation

// --- Fixed Constants ---

static const float MIN_NON_ZERO = 0.00001;
static const uint BVH_NODE_STACK_LENGTH = MAX_BVH_DEPTH + 1;

// --- Other Fixed Constants ---

// Gaussian blur lookup
// Weight distribution is symmetric through the center
// [y=0][x=0] (upper right) contains the center weight
// 2x2x2
static const float GBLookup1_totWeight = 28.0;
static const float2x2 GBLookup1_z1 = float2x2(
    4.0,  2.0,
    2.0,  1.0
);
static const float2x2 GBLookup1_z2 = float2x2(
    2.0,  1.0,
    1.0,  0.0
);

// 3x3x3
static const float GBLookup2_totWeight = 2150.0;
static const float3x3 GBLookup2_z1 = float3x3(
    97.0, 64.0, 41.0,
    64.0, 17.0, 11.0,
    41.0, 11.0,  7.0
);
static const float3x3 GBLookup2_z2 = float3x3(
    64.0, 41.0, 26.0,
    41.0, 11.0,  7.0,
    26.0,  7.0,  4.0
);
static const float3x3 GBLookup2_z3 = float3x3(
    41.0, 26.0, 16.0,
    26.0,  7.0,  4.0,
    16.0,  4.0,  1.0
);

// 4x4x4
static const float GBLookup3_totWeight = 1590.0;
static const float4x4 GBLookup3_z1 = float4x4(
    159.0, 97.0, 22.0, 2.0,
    97.0,  59.0, 13.0, 1.0,
    22.0,  13.0, 3.0,  0.0,
    2.0,   1.0,  0.0,  0.0
);
static const float4x4 GBLookup3_z2 = float4x4(
    97.0, 22.0, 2.0, 0.0,
    22.0, 13.0, 1.0, 0.0,
    2.0,   1.0, 0.0, 0.0,
    0.0,   0.0, 0.0, 0.0
);
static const float4x4 GBLookup3_z3 = float4x4(
    22.0, 2.0, 0.0, 0.0,
    2.0,  1.0, 0.0, 0.0,
    0.0,  0.0, 0.0, 0.0,
    0.0,  0.0, 0.0, 0.0
);
static const float4x4 GBLookup3_z4 = float4x4(
    2.0, 0.0, 0.0, 0.0,
    0.0, 0.0, 0.0, 0.0,
    0.0, 0.0, 0.0, 0.0,
    0.0, 0.0, 0.0, 0.0
);

float WeightFromGBLookup(uint x, uint y, uint z, uint radius)
{
    if (radius == 1)
    {
        if (z == 0) return GBLookup1_z1[y][x];
        if (z == 1) return GBLookup1_z2[y][x];
    }
    else if (radius == 2)
    {
        if (z == 0) return GBLookup2_z1[y][x];
        if (z == 1) return GBLookup2_z2[y][x];
        if (z == 2) return GBLookup2_z3[y][x];
    }
    else if (radius == 3)
    {
        if (z == 0) return GBLookup3_z1[y][x];
        if (z == 1) return GBLookup3_z2[y][x];
        if (z == 2) return GBLookup3_z3[y][x];
        if (z == 3) return GBLookup3_z4[y][x];
    }
    return -1.#INF; // Error indication
}

float TotWeightFromGBLookup(uint radius)
{
    if (radius == 1) { return GBLookup1_totWeight; }
    if (radius == 2) { return GBLookup2_totWeight; }
    return GBLookup3_totWeight; // radius == 3
}

static const float f8_INF[8] = {
    1.#INF, 1.#INF, 1.#INF, 1.#INF, 1.#INF, 1.#INF, 1.#INF, 1.#INF
};

void SwapInt2(inout int2 a, inout int2 b)
{
    int2 temp = a;
    a = b;
    b = temp;
}

void SwapFloat(inout float a, inout float b)
{
    float temp = a;
    a = b;
    b = temp;
}

void SwapUint3(inout uint3 a, inout uint3 b)
{
    uint3 temp = a;
    a = b;
    b = temp;
}

void SwapUint4(inout uint4 a, inout uint4 b)
{
    uint4 temp = a;
    a = b;
    b = temp;
}