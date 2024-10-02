static const uint3 Offsets_2x2x2[8] = {
    uint3(0,0,0),
    uint3(1,0,0),
    uint3(1,0,1),
    uint3(0,0,1),
    uint3(0,1,0),
    uint3(1,1,0),
    uint3(1,1,1),
    uint3(0,1,1)
};
static const uint InvOffsets_2x2x2[2][2][2] = {
    {   // x = 0
        {   // y = 0
            0, // z = 0 -> Index 0: (0, 0, 0)
            3  // z = 1 -> Index 3: (0, 0, 1)
        },
        {   // y = 1
            4, // z = 0 -> Index 4: (0, 1, 0)
            7  // z = 1 -> Index 7: (0, 1, 1)
        }
    },
    {   // x = 1
        {   // y = 0
            1, // z = 0 -> Index 1: (1, 0, 0)
            2  // z = 1 -> Index 2: (1, 0, 1)
        },
        {   // y = 1
            5, // z = 0 -> Index 5: (1, 1, 0)
            6  // z = 1 -> Index 6: (1, 1, 1)
        }
    }
};

// static const int3 _8BitToOffset_2x2x2[256][9] = {
//     { int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(1,0,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(1,0,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(0,0,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(0,0,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(0,0,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,1), int3(0,0,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,1), int3(0,0,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(1,0,1), int3(0,0,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(1,0,1), int3(0,0,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(0,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(0,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(0,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,1), int3(0,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,1), int3(0,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(1,0,1), int3(0,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(1,0,1), int3(0,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,1), int3(0,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(0,0,1), int3(0,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(0,0,1), int3(0,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(0,0,1), int3(0,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,1), int3(0,0,1), int3(0,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,1), int3(0,0,1), int3(0,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(1,0,1), int3(0,0,1), int3(0,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(1,0,1), int3(0,0,1), int3(0,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(1,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(1,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,1), int3(1,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,1), int3(1,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(1,0,1), int3(1,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(1,0,1), int3(1,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,1), int3(1,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(0,0,1), int3(1,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(0,0,1), int3(1,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(0,0,1), int3(1,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,1), int3(0,0,1), int3(1,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,1), int3(0,0,1), int3(1,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(1,0,1), int3(0,0,1), int3(1,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(1,0,1), int3(0,0,1), int3(1,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,1,0), int3(1,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(0,1,0), int3(1,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(0,1,0), int3(1,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(0,1,0), int3(1,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,1), int3(0,1,0), int3(1,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,1), int3(0,1,0), int3(1,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(1,0,1), int3(0,1,0), int3(1,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(1,0,1), int3(0,1,0), int3(1,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,1), int3(0,1,0), int3(1,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(0,0,1), int3(0,1,0), int3(1,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(0,0,1), int3(0,1,0), int3(1,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(0,0,1), int3(0,1,0), int3(1,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,1), int3(0,0,1), int3(0,1,0), int3(1,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,1), int3(0,0,1), int3(0,1,0), int3(1,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(1,0,1), int3(0,0,1), int3(0,1,0), int3(1,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(1,0,1), int3(0,0,1), int3(0,1,0), int3(1,1,0), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,1), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,1), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(1,0,1), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(1,0,1), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,1), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(0,0,1), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(0,0,1), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(0,0,1), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,1), int3(0,0,1), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,1), int3(0,0,1), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(1,0,1), int3(0,0,1), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(1,0,1), int3(0,0,1), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(0,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(0,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(0,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,1), int3(0,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,1), int3(0,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(1,0,1), int3(0,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(1,0,1), int3(0,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,1), int3(0,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(0,0,1), int3(0,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(0,0,1), int3(0,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(0,0,1), int3(0,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,1), int3(0,0,1), int3(0,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,1), int3(0,0,1), int3(0,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(1,0,1), int3(0,0,1), int3(0,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(1,0,1), int3(0,0,1), int3(0,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(1,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(1,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,1), int3(1,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,1), int3(1,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(1,0,1), int3(1,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(1,0,1), int3(1,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,1), int3(1,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(0,0,1), int3(1,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(0,0,1), int3(1,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(0,0,1), int3(1,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,1), int3(0,0,1), int3(1,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,1), int3(0,0,1), int3(1,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(1,0,1), int3(0,0,1), int3(1,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(1,0,1), int3(0,0,1), int3(1,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,1,0), int3(1,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(0,1,0), int3(1,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(0,1,0), int3(1,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(0,1,0), int3(1,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,1), int3(0,1,0), int3(1,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,1), int3(0,1,0), int3(1,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(1,0,1), int3(0,1,0), int3(1,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(1,0,1), int3(0,1,0), int3(1,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,1), int3(0,1,0), int3(1,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(0,0,1), int3(0,1,0), int3(1,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(0,0,1), int3(0,1,0), int3(1,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(0,0,1), int3(0,1,0), int3(1,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,1), int3(0,0,1), int3(0,1,0), int3(1,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,1), int3(0,0,1), int3(0,1,0), int3(1,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(1,0,1), int3(0,0,1), int3(0,1,0), int3(1,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(1,0,1), int3(0,0,1), int3(0,1,0), int3(1,1,0), int3(1,1,1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(1,0,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(1,0,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(0,0,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(0,0,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(0,0,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,1), int3(0,0,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,1), int3(0,0,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(1,0,1), int3(0,0,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(1,0,1), int3(0,0,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(0,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(0,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(0,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,1), int3(0,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,1), int3(0,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(1,0,1), int3(0,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(1,0,1), int3(0,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,1), int3(0,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(0,0,1), int3(0,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(0,0,1), int3(0,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(0,0,1), int3(0,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,1), int3(0,0,1), int3(0,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,1), int3(0,0,1), int3(0,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(1,0,1), int3(0,0,1), int3(0,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(1,0,1), int3(0,0,1), int3(0,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(1,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(1,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,1), int3(1,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,1), int3(1,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(1,0,1), int3(1,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(1,0,1), int3(1,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,1), int3(1,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(0,0,1), int3(1,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(0,0,1), int3(1,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(0,0,1), int3(1,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,1), int3(0,0,1), int3(1,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,1), int3(0,0,1), int3(1,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(1,0,1), int3(0,0,1), int3(1,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(1,0,1), int3(0,0,1), int3(1,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,1,0), int3(1,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(0,1,0), int3(1,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(0,1,0), int3(1,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(0,1,0), int3(1,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,1), int3(0,1,0), int3(1,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,1), int3(0,1,0), int3(1,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(1,0,1), int3(0,1,0), int3(1,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(1,0,1), int3(0,1,0), int3(1,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,1), int3(0,1,0), int3(1,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(0,0,1), int3(0,1,0), int3(1,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(0,0,1), int3(0,1,0), int3(1,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(0,0,1), int3(0,1,0), int3(1,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,1), int3(0,0,1), int3(0,1,0), int3(1,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,1), int3(0,0,1), int3(0,1,0), int3(1,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(1,0,1), int3(0,0,1), int3(0,1,0), int3(1,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(1,0,1), int3(0,0,1), int3(0,1,0), int3(1,1,0), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,1), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,1), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(1,0,1), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(1,0,1), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,1), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(0,0,1), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(0,0,1), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(0,0,1), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,1), int3(0,0,1), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,1), int3(0,0,1), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(1,0,1), int3(0,0,1), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(1,0,1), int3(0,0,1), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(0,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(0,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(0,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,1), int3(0,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,1), int3(0,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(1,0,1), int3(0,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(1,0,1), int3(0,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,1), int3(0,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(0,0,1), int3(0,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(0,0,1), int3(0,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(0,0,1), int3(0,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,1), int3(0,0,1), int3(0,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,1), int3(0,0,1), int3(0,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(1,0,1), int3(0,0,1), int3(0,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(1,0,1), int3(0,0,1), int3(0,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(1,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(1,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,1), int3(1,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,1), int3(1,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(1,0,1), int3(1,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(1,0,1), int3(1,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,1), int3(1,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(0,0,1), int3(1,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(0,0,1), int3(1,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(0,0,1), int3(1,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,1), int3(0,0,1), int3(1,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,1), int3(0,0,1), int3(1,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(1,0,1), int3(0,0,1), int3(1,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(1,0,1), int3(0,0,1), int3(1,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,1,0), int3(1,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(0,1,0), int3(1,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(0,1,0), int3(1,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(0,1,0), int3(1,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,1), int3(0,1,0), int3(1,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,1), int3(0,1,0), int3(1,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(1,0,1), int3(0,1,0), int3(1,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(1,0,1), int3(0,1,0), int3(1,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,1), int3(0,1,0), int3(1,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(0,0,1), int3(0,1,0), int3(1,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(0,0,1), int3(0,1,0), int3(1,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(0,0,1), int3(0,1,0), int3(1,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,1), int3(0,0,1), int3(0,1,0), int3(1,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,1), int3(0,0,1), int3(0,1,0), int3(1,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(1,0,0), int3(1,0,1), int3(0,0,1), int3(0,1,0), int3(1,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1), int3(-1,-1,-1) },
//     { int3(0,0,0), int3(1,0,0), int3(1,0,1), int3(0,0,1), int3(0,1,0), int3(1,1,0), int3(1,1,1), int3(0,1,1), int3(-1,-1,-1) },
// };

// Related code to _8BitToOffset_2x2x2:

// // Get child pixel offsets from lookup table
// int3 childPixelIDs[9] = _8BitToOffset_2x2x2[node8bitData];

// // Init nodeDst array
// NodeDst nodeDstArr[8] = InitNodeDstArr;

// int i = -1;
// uint nodeDstLength = 0;
// while(childPixelIDs[++i].x != -1)
// {
//     uint3 pixelID = basePixelID + childPixelIDs[i];
//     float dst = RayVoxelIntersect(pixelID, childMipmapLayer, localRay);

//     if (dst < hitInfo.dst)
//     {
//         nodeDstArr[nodeDstLength++] = InitNodeDst(dst, pixelID);
//     }
// }