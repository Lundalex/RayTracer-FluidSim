using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;

// Import utils from Resources.cs
using RendererResources;
// Usage: Utils.(functionName)()

public class ObjectManager : MonoBehaviour
{
    public GameObject[] SceneObjects;
    public GameObject[] FluidObjects;
    public MaterialInput[] MaterialInputs;
    public bool DesignatedVertices;
    public int MaxAtlasDims;
    public int MaxDepthSceneBVH;
    public int SplitResolution; // ex. 10 -> Each BV split will test 10 increments for each component x,y,z (30 tests total)
    public string FileName;
    public BVUpdateMode BVUpdateModeSelect;
    public FileDataMode FileDataModeSelect;
    public ExtensionMode ExtensionModeSelect;
    public NewRenderer m;

    private Texture2D textureAtlas;
    private Material2[] materials;
    private SceneObjectData[] SceneObjectDatas;
    private LightObject[] LightObjects;
    private RenderBV[] LoadedBVs = new RenderBV[0];
    private int2[] LoadedComponentDatas = new int2[0];
    private List<MeshData> LoadedMeshes = new();
    private Vertex[] LoadedVertices = new Vertex[0];
    private Triangle[] LoadedTriangles = new Triangle[0];
    private RenderTriangle[] RenderTriangles;
    private RenderSceneObject[] RenderSceneObjects;
    private int[] LoadedMeshesLookup;
    private int LastSceneBVHLength = 0;
    private int LoadedVerticesNum;
    private bool resetMaterials = false;

    private void OnValidate()
    {
        if (m.ProgramStarted) { m.DoUpdateSettings = true; m.DoReloadData = true; resetMaterials = true; }
    }
    public (Vertex[], Triangle[]) LoadMesh(Mesh mesh, int baseVertexIndex)
    {
        bool containsUVs = mesh.uv.Length > 0;

        Vector3[] meshVertices = mesh.vertices;
        Vector2[] meshUVs = mesh.uv;
        int[] meshTriangles = mesh.triangles;

        // Load vertices
        Vertex[] vertices = new Vertex[mesh.vertices.Length];
        Parallel.For(0, vertices.Length, i =>
        {
            vertices[i] = new Vertex
            {
                pos = meshVertices[i],
                uv = containsUVs ? meshUVs[i] : -1
            };
        });

        // Load triangles
        int trianglesNum = mesh.triangles.Length / 3;
        Triangle[] triangles = new Triangle[trianglesNum];
        Parallel.For(0, trianglesNum, i =>
        {
            int baseIndex = 3 * i;
            Triangle triangle = new Triangle
            {
                vertex0Index = meshTriangles[baseIndex] + baseVertexIndex,
                vertex1Index = meshTriangles[baseIndex + 1] + baseVertexIndex,
                vertex2Index = meshTriangles[baseIndex + 2] + baseVertexIndex
            };
            triangle.Initialise(vertices, baseVertexIndex);

            triangles[i] = triangle;
        });

        // Create designated vertices for each triangle. Inefficient for memory usage, but may lead to improved memory coherancy.
        if (DesignatedVertices)
        {
            Vertex[] designatedVertices = new Vertex[3 * trianglesNum];
            Parallel.For(0, trianglesNum, i =>
            {
                int baseIndex = 3 * i;
                Triangle triangle = triangles[i];
                designatedVertices[baseIndex] = vertices[triangle.vertex0Index - baseVertexIndex];
                designatedVertices[baseIndex + 1] = vertices[triangle.vertex1Index - baseVertexIndex];
                designatedVertices[baseIndex + 2] = vertices[triangle.vertex2Index - baseVertexIndex];

                triangle.vertex0Index = baseVertexIndex + baseIndex;
                triangle.vertex1Index = baseVertexIndex + baseIndex + 1;
                triangle.vertex2Index = baseVertexIndex + baseIndex + 2;

                triangles[i] = triangle;
            });

            return (designatedVertices, triangles);
        }

        return (vertices, triangles);
    }

    private float3 GetMin<T>(T[] components) where T : IBVHComponent
    {
        float3 min = new float3(float.MaxValue, float.MaxValue, float.MaxValue);
        foreach (var component in components)
        {
            min = math.min(min, component.GetMin());
        }

        return min;
    }
    private float3 GetMin<T>(List<T> components) where T : IBVHComponent
    {
        float3 min = new float3(float.MaxValue, float.MaxValue, float.MaxValue);
        foreach (var component in components)
        {
            min = math.min(min, component.GetMin());
        }

        return min;
    }

    private float3 GetMax<T>(T[] components) where T : IBVHComponent
    {
        float3 max = new float3(float.MinValue, float.MinValue, float.MinValue);
        foreach (var component in components)
        {
            max = math.max(max, component.GetMax());
        }

        return max;
    }
    private float3 GetMax<T>(List<T> components) where T : IBVHComponent
    {
        float3 max = new float3(float.MinValue, float.MinValue, float.MinValue);
        foreach (var component in components)
        {
            max = math.max(max, component.GetMax());
        }

        return max;
    }

    private float GetBoxArea(float3 vA, float3 vB)
    {
        float length = Mathf.Abs(vA.x - vB.x);
        float width = Mathf.Abs(vA.y - vB.y);
        float height = Mathf.Abs(vA.z - vB.z);

        float area = 2 * (length * width + width * height + height * length);
        
        return area;
    }

    float GetCost<T>(List<T> componentsChildA, List<T> componentsChildB) where T : IBVHComponent
    {
        float costA = GetBoxArea(GetMin(componentsChildA), GetMax(componentsChildA)) * componentsChildA.Count;
        float costB = GetBoxArea(GetMin(componentsChildB), GetMax(componentsChildB)) * componentsChildB.Count;
        float totCost = costA + costB;

        return totCost;
    }

    private void SwapPair<T>(ref T[] array, int indexA, int indexB) where T : IBVHComponent => (array[indexB], array[indexA]) = (array[indexA], array[indexB]);

    private int DivideIntoSubGroupsRef<T>(ref T[] components, int axis, float3 splitCoord, int componentStart, int totComponents) where T : IBVHComponent
    {
        int highestIndexA = componentStart - 1;
        int countA = 0;
        for (int componentIndex = componentStart; componentIndex < componentStart + totComponents; componentIndex++)
        {
            float3 pos = components[componentIndex].GetMid();
            bool isInGroupA = axis switch
            {
                0 => pos.x < splitCoord.x,
                1 => pos.y < splitCoord.y,
                2 => pos.z < splitCoord.z,
                _ => false
            };
            if (isInGroupA)
            {
                highestIndexA++;
                countA++;
                
                if (highestIndexA != componentIndex) SwapPair(ref components, highestIndexA, componentIndex);
            }
        }

        return countA;
    }

    private (List<T>, List<T>) DivideIntoSubGroupsCopy<T>(T[] components, int axis, float3 splitCoord, int componentStart, int totComponent) where T : IBVHComponent
    {
        List<T> componentsChildA = new List<T>();
        List<T> componentsChildB = new List<T>();

        for (int componentIndex = componentStart; componentIndex < componentStart + totComponent; componentIndex++)
        {
            float3 pos = components[componentIndex].GetMid();
            bool isInGroupA = axis switch
            {
                0 => pos.x < splitCoord.x,
                1 => pos.y < splitCoord.y,
                2 => pos.z < splitCoord.z,
                _ => false
            };
            if (isInGroupA) componentsChildA.Add(components[componentIndex]);
            else componentsChildB.Add(components[componentIndex]);
        }

        return (componentsChildA, componentsChildB);
    }

    private int RecursivelySplitBV<T>(ref List<BV> BVs, ref T[] components, int bvParentIndex, BV bvParent, int maxDepth, int depth = 0) where T : IBVHComponent
    {
        depth += 1;
        if (depth >= maxDepth) { BVs[bvParentIndex].SetLeaf(); return bvParentIndex; }
        
        (float3 splitCoord, int axis, float cost) leastCostSplit = (0, -1, float.MaxValue);

        // Find the best split point for the parent BV
        float3 diff = bvParent.max - bvParent.min;
        for (int split = 0; split < SplitResolution; split++)
        {
            float3 splitCoord = bvParent.min + diff * (split+0.5f) / SplitResolution;

            for (int axis = 0; axis < 3; axis++)
            {
                // Test splitting the parent bounding box
                List<T> componentsChildA;
                List<T> componentsChildB;
                (componentsChildA, componentsChildB) = DivideIntoSubGroupsCopy(components, axis, splitCoord, bvParent.componentStart, bvParent.totComponents);

                // Calculate cost (total surface area) of the resulting box split
                float cost = GetCost(componentsChildA, componentsChildB);

                // Compare the resulting cost with the currently lowest split cost
                if (cost < leastCostSplit.cost) { leastCostSplit = (splitCoord, axis, cost); }
            }
        }

        // End recursion if no valid split was found
        if (leastCostSplit.axis == -1) { BVs[bvParentIndex].SetLeaf(); return bvParentIndex; }

        // Otherwise, split the bounding box
        int countA = DivideIntoSubGroupsRef(ref components, leastCostSplit.axis, leastCostSplit.splitCoord, bvParent.componentStart, bvParent.totComponents);

        // Get components for either child
        List<T> componentsBestChildA = components.Skip(bvParent.componentStart).Take(countA).ToList();
        List<T> componentsBestChildB = components.Skip(bvParent.componentStart + countA).Take(bvParent.totComponents - countA).ToList();

        // Recursively split child A
        int furthestChildIndex = bvParentIndex;
        if (componentsBestChildA.Count != 0)
        {
            // Add childIndexA to parent BV
            int childIndexA = bvParentIndex + 1;
            BV parentBV = BVs[bvParentIndex];
            parentBV.childIndexA = childIndexA;
            BVs[bvParentIndex] = parentBV;

            // Call RecursivelySplitBV() for child A
            BVs.Add(new BV(GetMin(componentsBestChildA), GetMax(componentsBestChildA), bvParent.componentStart, componentsBestChildA.Count));
            DebugUtils.ChildIndexValidation(childIndexA, BVs.Count);
            furthestChildIndex = RecursivelySplitBV(ref BVs, ref components, childIndexA, BVs[childIndexA], maxDepth, depth);
        }

        // Recursively split child B
        if (componentsBestChildB.Count != 0)
        {
            // Add childIndexB to parent BV
            int childIndexB = furthestChildIndex + 1;
            BV parentBV = BVs[bvParentIndex];
            parentBV.childIndexB = childIndexB;
            BVs[bvParentIndex] = parentBV;

            // Call RecursivelySplitBV() for child B
            BVs.Add(new BV(GetMin(componentsBestChildB), GetMax(componentsBestChildB), bvParent.componentStart + componentsBestChildA.Count, componentsBestChildB.Count));
            DebugUtils.ChildIndexValidation(childIndexB, BVs.Count);
            furthestChildIndex = RecursivelySplitBV(ref BVs, ref components, childIndexB, BVs[childIndexB], maxDepth, depth);
        }

        // Return the currently furthest child index
        return furthestChildIndex;
    }

    private (int, int) ConstructBVH(ref RenderBV[] loadedBVs, ref Triangle[] loadedTriangles, ref Vertex[] loadedVertices, ref int2[] loadedComponentDatas, Vertex[] newVertices, Triangle[] newTriangles, int maxDepthBVH)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        float3 objectMin = GetMin(newTriangles);
        float3 objectMax = GetMax(newTriangles);
        List<BV> newBVs = new List<BV> { new BV(objectMin, objectMax, 0, newTriangles.Length, 1, 2) };

        // Construct the BVH
        RecursivelySplitBV(ref newBVs, ref newTriangles, 0, newBVs[0], maxDepthBVH);

        int loadedTrianglesLength = loadedTriangles.Length;
        int loadedBVsLength = loadedBVs.Length;
        Parallel.For(0, newBVs.Count, i =>
        {
            BV bv = newBVs[i];
            if (bv.childIndexA != -1) bv.childIndexA += loadedBVsLength;
            if (bv.childIndexB != -1) bv.childIndexB += loadedBVsLength;
            bv.componentStart += loadedTrianglesLength;
            newBVs[i] = bv;
        });

        // Convert to bounding volume struct variant for shader buffer transfer
        RenderBV[] newRenderBVs;
        int2[] newComponentDatas;
        (newRenderBVs, newComponentDatas) = BV.ClassToStruct(newBVs);

        if (DesignatedVertices)
        {
            int loadedVerticesLength = loadedVertices.Length;
            Vertex[] sortedNewVertices = new Vertex[newVertices.Length];

            for (int i = 0; i < newTriangles.Length; i++)
            {
                int baseIndex = 3 * i;
                Triangle triangle = newTriangles[i];

                sortedNewVertices[baseIndex] = newVertices[triangle.vertex0Index - loadedVerticesLength];
                sortedNewVertices[baseIndex + 1] = newVertices[triangle.vertex1Index - loadedVerticesLength];
                sortedNewVertices[baseIndex + 2] = newVertices[triangle.vertex2Index - loadedVerticesLength];

                triangle.vertex0Index = loadedVerticesLength + baseIndex;
                triangle.vertex1Index = loadedVerticesLength + baseIndex + 1;
                triangle.vertex2Index = loadedVerticesLength + baseIndex + 2;

                newTriangles[i] = triangle;
            }

            newVertices = sortedNewVertices;
        }

        // Add new bounding volumes & tris to existing arrays
        loadedTriangles = loadedTriangles.Concat(newTriangles).ToArray();
        loadedVertices = loadedVertices.Concat(newVertices).ToArray();
        loadedBVs = loadedBVs.Concat(newRenderBVs).ToArray();
        loadedComponentDatas = loadedComponentDatas.Concat(newComponentDatas).ToArray();

        DebugUtils.LogStopWatch("BVH construction", ref stopwatch);

        return (newBVs.Count, newTriangles.Length);
    }

    private float GetEmittance(GameObject sceneObject)
    {
        SceneObjectSettings sceneObjectSettings = sceneObject.GetComponentInChildren<SceneObjectSettings>();
        int materialIndex = sceneObjectSettings.MaterialIndex;
        float brightness = MaterialInputs[materialIndex].brightness;
        return brightness;
    }
    private float GetEmittance(SceneObjectData sceneObjectData)
    {
        int materialIndex = sceneObjectData.materialIndex;
        float brightness = MaterialInputs[materialIndex].brightness;
        return brightness;
    }

    private int GetEmittingObjectsNum(GameObject[] SceneObjects) => SceneObjects.Count(obj => GetEmittance(obj) != 0.0f);

    private (Texture2D, Material2[]) ConstructTextureAtlas()
    {
        List<Texture2D> textures = new List<Texture2D>();
        foreach (MaterialInput mat in MaterialInputs)
        {
            if (mat.colTex != null) textures.Add(mat.colTex);
            if (mat.bumpTex != null) textures.Add(mat.bumpTex);
            if (mat.roughnessTex != null) textures.Add(mat.roughnessTex);
            if (mat.normalsTex != null) textures.Add(mat.normalsTex);
        }

        Texture2D atlas = new Texture2D(MaxAtlasDims, MaxAtlasDims, TextureFormat.RGBA32, false);
        Rect[] rects = atlas.PackTextures(textures.ToArray(), 1, MaxAtlasDims);

        UnityEngine.Debug.Log("Texture atlas constructed with " + rects.Length + " textures. Width: " + atlas.width + ". Height: " + atlas.height);

        int2 GetTexLoc(int rectIndex) => new((int)(rects[rectIndex].x * atlas.width), (int)(rects[rectIndex].y * atlas.height));
        int2 GetTexDims(int rectIndex) => new((int)(rects[rectIndex].width * atlas.width), (int)(rects[rectIndex].height * atlas.height));

        int rectIndex = 0;
        Material2[] renderMaterials = new Material2[MaterialInputs.Length];
        for (int i = 0; i < MaterialInputs.Length; i++)
        {
            Material2 renderMat = new Material2();
            MaterialInput mat = MaterialInputs[i];

            // Brightness
            renderMat.brightness = mat.brightness;

            // Col
            if (mat.colTex != null)
            {
                renderMat.colTexLoc = GetTexLoc(rectIndex);
                renderMat.colTexDims = GetTexDims(rectIndex);
                renderMat.col = -1;
                rectIndex++;
            }
            else renderMat.col = mat.col;

            // Bump
            if (mat.bumpTex != null)
            {
                renderMat.bumpTexLoc = GetTexLoc(rectIndex);
                renderMat.bumpTexDims = GetTexDims(rectIndex);
                renderMat.bump = -1;
                rectIndex++;
            }
            else renderMat.bump = mat.bump;

            // Roughness
            if (mat.roughnessTex != null)
            {
                renderMat.roughnessTexLoc = GetTexLoc(rectIndex);
                renderMat.roughnessTexDims = GetTexDims(rectIndex);
                renderMat.roughness = -1;
                rectIndex++;
            }
            else renderMat.roughness = mat.roughness;

            // Metallicity
            if (mat.metallicityTex != null)
            {
                renderMat.metallicityTexLoc = GetTexLoc(rectIndex);
                renderMat.metallicityTexDims = GetTexDims(rectIndex);
                renderMat.metallicity = -1;
                rectIndex++;
            }
            else renderMat.metallicity = mat.metallicity;

            // Normals
            if (mat.normalsTex != null)
            {
                renderMat.normalsTexLoc = GetTexLoc(rectIndex);
                renderMat.normalsTexDims = GetTexDims(rectIndex);
                rectIndex++;
            }
            else renderMat.normalsTexLoc = new int2(-1, -1);

            renderMaterials[i] = renderMat;
        }

        return (atlas, renderMaterials);
    }

    private (float3, float3) GetFastBV(float3 localMin, float3 localMax, Matrix4x4 localToWorldMatrix)
    {
        // Calculate the 8 corners of the local BV
        float3[] corners = new float3[8];
        corners[0] = localMin;
        corners[1] = new float3(localMin.x, localMin.y, localMax.z);
        corners[2] = new float3(localMin.x, localMax.y, localMin.z);
        corners[3] = new float3(localMin.x, localMax.y, localMax.z);
        corners[4] = new float3(localMax.x, localMin.y, localMin.z);
        corners[5] = new float3(localMax.x, localMin.y, localMax.z);
        corners[6] = new float3(localMax.x, localMax.y, localMin.z);
        corners[7] = localMax;

        float3 worldMin = new float3(float.MaxValue, float.MaxValue, float.MaxValue);
        float3 worldMax = new float3(float.MinValue, float.MinValue, float.MinValue);

        // Transform each BV corner to world space, and compare them
        for (int i = 0; i < 8; i++)
        {
            float3 worldCorner = Func.Mul(localToWorldMatrix, corners[i]);
            worldMin = math.min(worldMin, worldCorner);
            worldMax = math.max(worldMax, worldCorner);
        }

        return (worldMin, worldMax);
    }
    private (float3, float3) GetAccurateBV(Triangle[] sceneObjectTriangles, Vertex[] vertices, Matrix4x4 localToWorldMatrix)
    {
        for (int i = 0; i < sceneObjectTriangles.Length; i++)
        {
            sceneObjectTriangles[i].CalcMinMaxTransformed(vertices, localToWorldMatrix);
        }

        float3 worldMin = GetMin(sceneObjectTriangles);
        float3 worldMax = GetMax(sceneObjectTriangles);

        return (worldMin, worldMax);
    }

    private MultiArrayContainer LoadFromFile(string fileName)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        MultiArrayContainer loadContainer = ExtensionModeSelect == ExtensionMode.Json ? FileLoader.LoadMultiArrayContainerFromJsonFiles(fileName) : FileLoader.LoadArraysFromBinFile(fileName);

        LoadedTriangles = loadContainer.loadedTriangles;
        RenderSceneObjects = loadContainer.renderSceneObjects;
        LightObjects = loadContainer.lightObjects;
        LoadedMeshesLookup = loadContainer.loadedMeshesLookup;
        LoadedComponentDatas = loadContainer.loadedComponentDatas;
        LoadedMeshes = loadContainer.loadedMeshes;
        LoadedBVs = loadContainer.loadedBVs;
        LoadedVertices = loadContainer.loadedVertices;

        m.rtShader.SetInt("MaxBVHDepth", loadContainer.GetIntByKey("maxBVHDepth"));
        m.rtShader.SetInt("EmittingObjectsNum", loadContainer.GetIntByKey("emittingObjectsNum"));
        m.rtShader.SetInt("SceneBVHStartIndex", loadContainer.GetIntByKey("sceneBVHStartIndex"));
        m.rtShader.SetFloat("TotArea", loadContainer.GetFloatByKey("totArea"));

        DebugUtils.LogStopWatch("Data fetching", ref stopwatch);
        FileDataModeSelect = FileDataMode.Disabled;

        return loadContainer;
    }

    private void SaveToFile(string fileName, int maxBVHDepth, int emittingObjectsNum, int sceneBVHStartIndex, float totArea)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        var saveContainer = new MultiArrayContainer
        {
            loadedTriangles = LoadedTriangles,
            renderSceneObjects = RenderSceneObjects,
            lightObjects = LightObjects,
            loadedMeshesLookup = LoadedMeshesLookup,
            loadedComponentDatas = LoadedComponentDatas,
            loadedMeshes = LoadedMeshes,
            loadedBVs = LoadedBVs,
            loadedVertices = LoadedVertices,
            integers = new KeyedInt[]
            {
                new() { Key = "maxBVHDepth", Value = maxBVHDepth },
                new() { Key = "emittingObjectsNum", Value = emittingObjectsNum },
                new() { Key = "sceneBVHStartIndex", Value = sceneBVHStartIndex }
            },
            floats = new KeyedFloat[]
            {
                new() { Key = "totArea", Value = totArea }
            },
            renderTriangles = RenderTriangles,
        };

        if (ExtensionModeSelect == ExtensionMode.Json) FileLoader.SaveMultiArrayContainerToJsonFiles(fileName, saveContainer);
        else FileLoader.SaveArraysToBinFile(fileName, saveContainer);

        DebugUtils.LogStopWatch("Data writing", ref stopwatch);
        Utils.RemoveFromEndOfArray(ref LoadedComponentDatas, LastSceneBVHLength);
        LastSceneBVHLength = 0;

        FileDataModeSelect = FileDataMode.Disabled;
    }

    private int LoadSceneObjects()
    {
        int[] BVHDepths = new int[SceneObjects.Length + 1];
        BVHDepths[SceneObjects.Length] = MaxDepthSceneBVH;

        for (int i = 0; i < SceneObjects.Length; i++)
        {
            // Retrieve relevant game object data
            GameObject sceneObject = SceneObjects[i];
            Transform transform = sceneObject.transform;
            SceneObjectSettings sceneObjectSettings = sceneObject.GetComponent<SceneObjectSettings>();
            Mesh mesh = sceneObject.GetComponent<MeshFilter>().mesh;

            SceneObjectData sceneObjectData = new();

            // Set transformation matrices
            sceneObjectData.worldToLocalMatrix = Utils.CreateWorldToLocalMatrix(transform.position, transform.rotation.eulerAngles, transform.localScale);
            Matrix4x4 worldToLocalMatrix = sceneObjectData.worldToLocalMatrix;
            sceneObjectData.localToWorldMatrix = worldToLocalMatrix.inverse;

            // Transfer script settings
            sceneObjectData.materialIndex = sceneObjectSettings.MaterialIndex;
            sceneObjectData.maxDepthBVH = sceneObjectSettings.MaxDepthBVH;
            BVHDepths[i] = sceneObjectData.maxDepthBVH;

            // Construct mesh BVH if the mesh has yet to be loaded
            string meshKey = Utils.GetMeshKey(mesh);
            int meshIndex = Utils.GetMeshIndex(LoadedMeshes, meshKey);
            if (meshIndex == -1)
            {
                Vertex[] meshVertices;
                Triangle[] meshTriangles;
                (meshVertices, meshTriangles) = LoadMesh(mesh, LoadedVerticesNum);
                LoadedVerticesNum += meshVertices.Length;

                LoadedMeshes.Add(new MeshData(meshTriangles, GetMin(meshTriangles), GetMax(meshTriangles), LoadedTriangles.Length, LoadedBVs.Length, meshKey));
                ConstructBVH(ref LoadedBVs, ref LoadedTriangles, ref LoadedVertices, ref LoadedComponentDatas, meshVertices, meshTriangles, sceneObjectData.maxDepthBVH);
                meshIndex = LoadedMeshes.Count - 1;
            }
            LoadedMeshesLookup[i] = meshIndex;

            // Set start index values
            sceneObjectData.bvStartIndex = LoadedMeshes[meshIndex].bvStartIndex;

            // Add scene object data to the array
            SceneObjectDatas[i] = sceneObjectData;
        }

        int maxBVHDepth = Func.MaxInt(BVHDepths);

        return maxBVHDepth;
    }

    private (int, float) ConstructSceneBVH()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        Utils.RemoveFromEndOfArray(ref LoadedBVs, LastSceneBVHLength);
        Utils.RemoveFromEndOfArray(ref LoadedComponentDatas, LastSceneBVHLength);

        // Calculate area related values to be used by RT shader
        float totArea = 0.0f;
        Parallel.For(0, SceneObjects.Length, i =>
        {
            Matrix4x4 localToWorldMatrix = SceneObjectDatas[i].localToWorldMatrix;
            Triangle[] sceneObjectTriangles = LoadedMeshes[LoadedMeshesLookup[i]].triangles;
            float3 localMin = LoadedMeshes[LoadedMeshesLookup[i]].localMin;
            float3 localMax = LoadedMeshes[LoadedMeshesLookup[i]].localMax;

            float3 worldMin;
            float3 worldMax;
            (worldMin, worldMax) = BVUpdateModeSelect == BVUpdateMode.Fast ? GetFastBV(localMin, localMax, localToWorldMatrix) : GetAccurateBV(sceneObjectTriangles, LoadedVertices, localToWorldMatrix);

            float boxArea = GetBoxArea(worldMin, worldMax);

            SceneObjectDatas[i].min = worldMin;
            SceneObjectDatas[i].max = worldMax;
            SceneObjectDatas[i].areaApprox = boxArea;
            
            totArea += boxArea;
        });
        m.rtShader.SetFloat("TotArea", totArea);DebugUtils.LogStopWatch("BVH construction for scene objects", ref stopwatch);

        float3 sceneMin = GetMin(SceneObjectDatas);
        float3 sceneMax = GetMax(SceneObjectDatas);
        List<BV> sceneBVs = new List<BV> { new BV(sceneMin, sceneMax, 0, SceneObjectDatas.Length, 1, 2) };

        // Construct scene BVH
        RecursivelySplitBV(ref sceneBVs, ref SceneObjectDatas, 0, sceneBVs[0], MaxDepthSceneBVH);

        int sceneBVHStartIndex = LoadedBVs.Length;
        m.rtShader.SetInt("SceneBVHStartIndex", sceneBVHStartIndex);
        Parallel.For(0, sceneBVs.Count, i =>
        {
            BV bv = sceneBVs[i];
            if (bv.childIndexA != -1) bv.childIndexA += sceneBVHStartIndex;
            if (bv.childIndexB != -1) bv.childIndexB += sceneBVHStartIndex;
            sceneBVs[i] = bv;
        });

        // Replace existing scene BVH with new data
        RenderBV[] newRenderBVs;
        int2[] newComponentDatas;
        (newRenderBVs, newComponentDatas) = BV.ClassToStruct(sceneBVs);
        LoadedBVs = LoadedBVs.Concat(newRenderBVs).ToArray();
        LoadedComponentDatas = LoadedComponentDatas.Concat(newComponentDatas).ToArray();

        return (sceneBVHStartIndex, totArea);
    }

    public (RenderBV[], Vertex[], RenderTriangle[], RenderSceneObject[], LightObject[], Texture2D, Material2[], int) ConstructScene()
    {
        // Pack material textures into atlas
        if (textureAtlas == null || resetMaterials) { (textureAtlas, materials) = ConstructTextureAtlas(); resetMaterials = false; }

        // Fetch data
        if (FileDataModeSelect == FileDataMode.LoadExistingFile)
        {
            MultiArrayContainer loadContainer = LoadFromFile(FileName);

            return (loadContainer.loadedBVs, loadContainer.loadedVertices, loadContainer.renderTriangles, loadContainer.renderSceneObjects, loadContainer.lightObjects, textureAtlas, materials, loadContainer.renderTriangles.Length);
        }

        // --- Scene object BVHs ---

        SceneObjectDatas ??= new SceneObjectData[SceneObjects.Length + FluidObjects.Length];
        LoadedMeshesLookup ??= new int[SceneObjects.Length];

        int maxBVHDepth = LoadSceneObjects();

        m.rtShader.SetInt("MaxBVHDepth", maxBVHDepth);

        // Load light emitting object data
        int emittingObjectsNum = GetEmittingObjectsNum(SceneObjects);
        m.rtShader.SetInt("EmittingObjectsNum", emittingObjectsNum);
        LightObjects = new LightObject[emittingObjectsNum];
        int lightObjectIndex = 0;
        for (int i = 0; i < SceneObjects.Length; i++)
        {
            SceneObjectData sceneObjectData = SceneObjectDatas[i];
            if (GetEmittance(sceneObjectData) != 0.0f)
            {
                LightObjects[lightObjectIndex++] = new LightObject
                {
                    localToWorldMatrix = sceneObjectData.localToWorldMatrix,
                    areaApprox = sceneObjectData.areaApprox,
                    brightness = MaterialInputs[sceneObjectData.materialIndex].brightness,
                    triStart = LoadedComponentDatas[sceneObjectData.bvStartIndex].x,
                    totTris = LoadedComponentDatas[sceneObjectData.bvStartIndex].y
                };
            }
        }

        // --- Add fluid objects ---
        // It may be possible to use fluids as light objects, with some changes.
        // If this is implemented, the "Load light emitting object data" should be moved to below this part of the code.

        int maxMipmapDepth = 0;
        int3 composedSurfaceCellsSize = 0;
        int3 composedSurfaceCellsLookupSize = 0;
        for (int i = 0; i < FluidObjects.Length; i++)
        {
            // Retrieve relevant fluid object data
            GameObject fluidObject = FluidObjects[i];
            Transform transform = fluidObject.transform;
            FluidManager fluidManager = fluidObject.GetComponent<FluidManager>();
            Simulation simulation = fluidObject.GetComponent<Simulation>();
            MarchingCubes mCubes = fluidObject.GetComponent<MarchingCubes>();

            SceneObjectData sceneObjectData = new();

            // Set transformation matrices
            sceneObjectData.worldToLocalMatrix = Utils.CreateWorldToLocalMatrix(transform.position, transform.rotation.eulerAngles, transform.localScale);
            Matrix4x4 worldToLocalMatrix = sceneObjectData.worldToLocalMatrix;
            sceneObjectData.localToWorldMatrix = worldToLocalMatrix.inverse;

            // Set material
            sceneObjectData.materialIndex = fluidManager.MaterialIndex;

            // Mipmap values
            (int3 mipmap0Resolution, int3 textureSize, int mipmapDepth) = TextureHelper.GetVoxelTextureSize(mCubes.NumCells.xyz);
            mCubes.SurfaceCellsMM0Dims = mipmap0Resolution;
            mCubes.SurfaceCellsMipmapDepth = maxMipmapDepth;
            mCubes.SurfaceCellsMipmapDepth = mipmapDepth;
            maxMipmapDepth = Mathf.Max(maxMipmapDepth, mipmapDepth);

            // Set the offsets for this fluids SVO textures
            sceneObjectData.bvStartIndex = composedSurfaceCellsSize.y;
            mCubes.SurfaceCellsOffset = composedSurfaceCellsSize.y;
            mCubes.SurfaceCellsLookupOffset = composedSurfaceCellsLookupSize.y;

            static void ExtendTexture(ref int3 composedSize, int3 otherSize) => composedSize = new(Mathf.Max(composedSize.x, otherSize.x), composedSize.y + otherSize.y, Mathf.Max(composedSize.z, otherSize.z));

            // Update composedVoxelTextureSize with this fluids SVO textureSize
            ExtendTexture(ref composedSurfaceCellsSize, textureSize);
            ExtendTexture(ref composedSurfaceCellsLookupSize, textureSize);
        }

        RenderTexture ComposedSurfaceCellsTexture = TextureHelper.CreateIntTexture(composedSurfaceCellsSize, 1);
        RenderTexture ComposedSurfaceCellsLookupTexture = TextureHelper.CreateIntTexture(composedSurfaceCellsLookupSize, 2);

        // --- Scene BVH ---

        int sceneBVHStartIndex;
        float totArea;
        (sceneBVHStartIndex, totArea) = ConstructSceneBVH();

        // Transfer data to shader friendly structs
        if (RenderTriangles == null)
        {
            RenderTriangles = new RenderTriangle[LoadedTriangles.Length];
            Parallel.For(0, RenderTriangles.Length, i =>
            {
                Triangle triangle = LoadedTriangles[i];
                RenderTriangles[i] = new RenderTriangle
                {
                    vertex0Index = triangle.vertex0Index,
                    vertex1Index = triangle.vertex1Index,
                    vertex2Index = triangle.vertex2Index,
                    area = triangle.area
                };
            });
        }
        if (SceneObjectDatas != null)
        {
            RenderSceneObjects = new RenderSceneObject[SceneObjectDatas.Length];
            Parallel.For(0, RenderSceneObjects.Length, i =>
            {
                SceneObjectData sceneObjectData = SceneObjectDatas[i];
                RenderSceneObjects[i] = new RenderSceneObject
                {
                    worldToLocalMatrix = sceneObjectData.worldToLocalMatrix,
                    localToWorldMatrix = sceneObjectData.localToWorldMatrix,
                    areaApprox = sceneObjectData.areaApprox,
                    materialIndex = sceneObjectData.materialIndex,
                    bvStartIndex = sceneObjectData.bvStartIndex
                };
            });
        }

        // Save Data
        if (FileDataModeSelect == FileDataMode.GenerateNewFile) SaveToFile(FileName, maxBVHDepth, emittingObjectsNum, sceneBVHStartIndex, totArea);

        return (LoadedBVs, LoadedVertices, RenderTriangles, RenderSceneObjects, LightObjects, textureAtlas, materials, RenderTriangles.Length);
    }
}