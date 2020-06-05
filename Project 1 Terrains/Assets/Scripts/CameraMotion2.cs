using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMotion2 : MonoBehaviour {

    Dictionary<Vector2, string> terrainMap = new Dictionary<Vector2, string>();
    Dictionary<Vector2, int[,]> terrainToTreeMap = new Dictionary<Vector2, int[,]>();
    Dictionary<Vector2, int[,]> terrainToFlowerMap = new Dictionary<Vector2, int[,]>();
    int SIDE_LENGTH = 50;
    const int MAX_HEIGHT = 17;
    Camera MainCamera;
    float RENDER_DISTANCE = 0.01f; //how far away to render a new terrainName
    float CULL_DISTANCE = 0f; //how far away to remove an old terrain
    int terrainCount = 0;

    // sensitivity factors for translate and rotate
    float translateFactor = 0.3f;
    float rotateFactor = 5.0f;

    //plant instantiation
    string[] flowerNames = {"BlueFlower", "DarkBlueFlower", "YellowFlower", "BlueFlower"};

    void Start () {
        // cache the main camera
        print("START");
        MainCamera = Camera.main;
        CULL_DISTANCE = SIDE_LENGTH * 2;

        //initialize terrainMap
        // Vector3 camPos = MainCamera.transform.position;
        // Vector2 terrainKey = new Vector2(camPos.x, camPos.z);
        // terrainToTreeMap.Add(terrainKey, createTreeMap(terrainKey));
        // terrainToFlowerMap.Add(terrainKey, createFlowerMap(terrainKey));
        // terrainMap.Add(terrainKey, createTerrain(terrainKey));

        Vector3 camPos = MainCamera.transform.position;
        Vector2 terrainKey = new Vector2(0, 0);
        int CHUNK_LENGTH = 5;
        print(CHUNK_LENGTH);
        for (float x = camPos.x - (CHUNK_LENGTH * SIDE_LENGTH); x < camPos.x + (CHUNK_LENGTH * SIDE_LENGTH); x += SIDE_LENGTH) {
            for (float z = camPos.z - (CHUNK_LENGTH * SIDE_LENGTH); z < camPos.z + (CHUNK_LENGTH * SIDE_LENGTH); z += SIDE_LENGTH) {
                terrainKey = new Vector2(x, z);
                print("camPosx" + camPos.x);
                print("camPosz"+ camPos.z);
                print(Mathf.Abs(x));
                if (Mathf.Abs(camPos.x - Mathf.Abs(x)) <= 50 && Mathf.Abs(camPos.z - Mathf.Abs(z)) <= 50) {
                    terrainToTreeMap.Add(terrainKey, createTreeMap(terrainKey));
                    terrainToFlowerMap.Add(terrainKey, createFlowerMap(terrainKey));
                    terrainMap.Add(terrainKey, createTerrain(terrainKey));
                } else {
                    terrainToTreeMap.Add(terrainKey, createEmptyMap(terrainKey));
                    terrainToFlowerMap.Add(terrainKey, createEmptyMap(terrainKey));
                    terrainMap.Add(terrainKey, createTerrain(terrainKey));
                }
            }
        }


    }

    int[,] createEmptyMap(Vector2 terrainKey) {
        int[,] map = new int[SIDE_LENGTH + 1, SIDE_LENGTH + 1];
        print(map[0, 0]);
        return map;
    }

    // Move the camera, and maybe create a new plane
    void Update () {

        handleInput();

        //checkTerrains();

        changeCameraPosition();

    }

    void changeCameraPosition() {
        if (MainCamera.transform.position.y < heightFromNoise(MainCamera.transform.position.x, MainCamera.transform.position.z) + 10) {
            MainCamera.transform.position = new Vector3(MainCamera.transform.position.x, heightFromNoise(MainCamera.transform.position.x, MainCamera.transform.position.z) + 10, MainCamera.transform.position.z);
        }
    }

    string createTerrain(Vector2 terrainKey) {
        // create a new plane
        terrainCount++;
        GameObject terrain = new GameObject();
        terrain.name = "Terrain " + terrainKey.x + ", " + terrainKey.y;

        Mesh mesh = generateTerrainMesh(terrain, terrainKey);

        //Diffuse shading material
        terrain.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse"));

        // create a texture
        Texture2D texture = makeTexture(terrainKey, mesh.vertices);

        // attach the texture to the mesh
        Renderer renderer = terrain.GetComponent<Renderer>();
        renderer.material.mainTexture = texture;

        return terrain.name;
    }

    Mesh generateTerrainMesh(GameObject terrain, Vector2 terrainKey) {
        MeshFilter meshFilter = (MeshFilter)terrain.AddComponent(typeof(MeshFilter));
        terrain.AddComponent(typeof(MeshRenderer));

        Mesh mesh = new Mesh();

        int verticeIndex = 0;
        int verticesLength = (SIDE_LENGTH + 1) * (SIDE_LENGTH + 1);
        Vector3[] vertices = new Vector3[verticesLength];
        Vector2[] uv = new Vector2[verticesLength];
        Vector3[] normals = new Vector3[verticesLength];
        float height = 0f;

        for (int z = 0; z <= SIDE_LENGTH; z++) {
            for (int x = 0; x <= SIDE_LENGTH; x++) {
                height = heightFromNoise(x + (terrainKey.x - SIDE_LENGTH / 2f), z + (terrainKey.y - SIDE_LENGTH / 2f));
                vertices[verticeIndex] = new Vector3(x + (terrainKey.x - SIDE_LENGTH / 2f), height, z + (terrainKey.y - SIDE_LENGTH / 2f));
                uv[verticeIndex] = new Vector2((float)x / (SIDE_LENGTH), (float)z / (SIDE_LENGTH));
                verticeIndex++;
            }
        }

        int[] triangles = new int[SIDE_LENGTH * SIDE_LENGTH * 6];

        verticeIndex = 0;
        int triangleIndex = 0;
        for (int z = 0; z < SIDE_LENGTH; z++) {
            for (int x = 0; x < SIDE_LENGTH; x++) {
                triangles[triangleIndex] = triangles[triangleIndex + 3] = verticeIndex + 1 + SIDE_LENGTH;
                triangles[triangleIndex + 1] = verticeIndex + 2 + SIDE_LENGTH;
                triangles[triangleIndex + 2] = triangles[triangleIndex + 4] = verticeIndex + 1;
                triangles[triangleIndex + 5] = verticeIndex;
                verticeIndex++;
                triangleIndex += 6;
            }
            verticeIndex++;
        }

        // save the vertices and triangles in the mesh object
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;  // save the uv texture coordinates

        //normals and plant spawning
        int[,] flowerMap = new int[SIDE_LENGTH + 1, SIDE_LENGTH + 1];
        int[,] treeMap = new int[SIDE_LENGTH + 1, SIDE_LENGTH + 1];
        terrainToTreeMap.TryGetValue(terrainKey, out treeMap);
        terrainToFlowerMap.TryGetValue(terrainKey, out flowerMap);
        mesh.RecalculateNormals();
        verticeIndex = 0;
        for (int z = 0; z <= SIDE_LENGTH; z++) {
            for (int x = 0; x <= SIDE_LENGTH; x++) {
                //if on seam calc normal, if not use normal from mesh.RecalculateNormals()
                if (z == 0 || z == SIDE_LENGTH || x == 0 || x == SIDE_LENGTH) {
                    normals[verticeIndex] = calcNormal(vertices[verticeIndex]);
                } else {
                    normals[verticeIndex] = mesh.normals[verticeIndex];
                }

                //if on map check the terrain height and normal flatness to add plant
                if (flowerMap[x, z] == 1) {
                    checkAddFlower(vertices[verticeIndex], terrain, normals[verticeIndex]);
                }
                if (treeMap[x, z] == 1) {
                    checkAddTree(vertices[verticeIndex], terrain, normals[verticeIndex]);
                }
                verticeIndex++;
            }
        }
        mesh.normals = normals;

        terrain.GetComponent<MeshFilter>().mesh = mesh;

        return mesh;
    }

    Vector3 calcNormal(Vector3 vertex, bool printValues = false) {
        Vector3 topNeighbor = new Vector3(vertex.x, heightFromNoise(vertex.x, vertex.z + 1), vertex.z + 1);
        Vector3 bottomNeighbor = new Vector3(vertex.x, heightFromNoise(vertex.x, vertex.z - 1), vertex.z - 1);
        Vector3 leftNeighbor = new Vector3(vertex.x - 1, heightFromNoise(vertex.x - 1, vertex.z), vertex.z);
        Vector3 rightNeighbor = new Vector3(vertex.x + 1, heightFromNoise(vertex.x + 1, vertex.z), vertex.z);
        Vector3 v1 = rightNeighbor - vertex;
        Vector3 v2 = topNeighbor - vertex;
        Vector3 v3 = leftNeighbor - vertex;
        Vector3 v4 = bottomNeighbor - vertex;
        Vector3 n1 = Vector3.Cross(v2, v1).normalized;
        Vector3 n2 = Vector3.Cross(v1, v4).normalized;
        Vector3 n3 = Vector3.Cross(v3, v2).normalized;
        Vector3 n4 = Vector3.Cross(v4, v3).normalized;

        Vector3 surfaceNormal = Vector3.Normalize(n1 + n2 + n3 + n4);

        if (printValues) {
            print("vertex: " + vertex);
            print("top: " + topNeighbor);
            print("bottom: " + bottomNeighbor);
            print("right: " + rightNeighbor);
            print("left: " + leftNeighbor);
            print("surfaceNormal: " + surfaceNormal);
        }

        return surfaceNormal;
    }

    // Vector3 calcNormal2(Vector3 V) {
    //     Vector3 N = new Vector3(V.x, heightFromNoise(V.x, V.z + 1), V.z + 1);
    //     Vector3 S = new Vector3(V.x, heightFromNoise(V.x, V.z - 1), V.z - 1);
    //     Vector3 W = new Vector3(V.x - 1, heightFromNoise(V.x - 1, V.z), V.z);
    //     Vector3 E = new Vector3(V.x + 1, heightFromNoise(V.x + 1, V.z), V.z);
    //     Vector3 NE = new Vector3(V.x + 1, heightFromNoise(V.x + 1, V.z + 1), V.z + 1);
    //     Vector3 SW = new Vector3(V.x - 1, heightFromNoise(V.x - 1, V.z - 1), V.z - 1);
    //
    //     Vector3 surfaceNormal = new Vector3(0, 0, 0);
    //     surfaceNormal += Vector3.Cross(E - V, NE - E).normalized;
    //     surfaceNormal += Vector3.Cross(NE - E, V - NE).normalized;
    //     surfaceNormal += Vector3.Cross(NE - V, N - NE).normalized;
    //     surfaceNormal += Vector3.Cross(N - NE, V - N).normalized;
    //     surfaceNormal += Vector3.Cross(V - W, N - V).normalized;
    //     surfaceNormal += Vector3.Cross(N - V, W - N).normalized;
    //     surfaceNormal += Vector3.Cross(V - SW, W - V).normalized;
    //     surfaceNormal += Vector3.Cross(W - V, SW - W).normalized;
    //     surfaceNormal += Vector3.Cross(S - SW, V - S).normalized;
    //     surfaceNormal += Vector3.Cross(V - S, SW - V).normalized;
    //     surfaceNormal += Vector3.Cross(S - V, E - S).normalized;
    //     surfaceNormal += Vector3.Cross(E - S, V - E).normalized;
    //     return surfaceNormal;
    // }

    Texture2D makeTexture(Vector2 terrainKey, Vector3[] vertices) {
        Texture2D texture = new Texture2D (SIDE_LENGTH + 1, SIDE_LENGTH + 1);
        Color[] colors = new Color[(SIDE_LENGTH + 1) * (SIDE_LENGTH + 1)];
        Vector3 vertex = new Vector3();
        int index = 0;
        for (int z = 0; z < (SIDE_LENGTH + 1); z++) {
            for (int x = 0; x < (SIDE_LENGTH + 1); x++) {
                index = z * (SIDE_LENGTH + 1) + x;
                vertex = vertices[index];
                colors[index] = calculateColor(vertex.y, vertex.x, vertex.z);
            }
        }

        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colors);
        texture.Apply();
        return texture;
    }

    void checkAddFlower(Vector3 vertex, GameObject terrain, Vector3 normal) {
        float newMaxHeight = MAX_HEIGHT + 5;
        if (vertex.y < newMaxHeight * 0.6f && vertex.y >= newMaxHeight * 0.4f && flatEnough(normal)) {
            GameObject flower = (GameObject) Instantiate(Resources.Load(getRandomFlower()));
            flower.transform.position = new Vector3(Random.Range(vertex.x - 1f, vertex.x + 1f), vertex.y, Random.Range(vertex.z - 1f, vertex.z + 1f));
            flower.transform.localScale = flower.transform.localScale * Random.Range(0.25f, 0.35f);
            flower.transform.parent = terrain.transform;
        }
    }

    void checkAddTree(Vector3 vertex, GameObject terrain, Vector3 normal) {
        float newMaxHeight = MAX_HEIGHT + 5;
        if (vertex.y < newMaxHeight * 0.6f && vertex.y >= newMaxHeight * 0.4f && flatEnough(normal)) {
            GameObject tree = (GameObject) Instantiate(Resources.Load("Tree"));
            tree.transform.position = new Vector3(Random.Range(vertex.x - 0.5f, vertex.x + 0.5f), vertex.y, Random.Range(vertex.z - 0.5f, vertex.z + 0.5f));
            tree.transform.localScale = tree.transform.localScale * Random.Range(0.7f, 1f);
            tree.transform.parent = terrain.transform;
        }
    }

    float averageHeight(Vector3 vertex) {
        float height = vertex.y;
        float heightN = heightFromNoise(vertex.x, vertex.z + 1);
        float heightE = heightFromNoise(vertex.x + 1, vertex.z);
        float heightS = heightFromNoise(vertex.x, vertex.z - 1);
        float heightW = heightFromNoise(vertex.x - 1, vertex.z);
        return (height + heightN + heightE + heightS + heightW) / 5;
    }

    string getRandomFlower() {
        int index = Random.Range(0, 4);
        return flowerNames[index];
    }

    Color calculateColor(float height, float x, float z) {
        Color color = new Color(0, 0, 0, 1);
        float newMaxHeight = MAX_HEIGHT + 5;

        if (height >= newMaxHeight * 0.95f) {
            //Snow Color
            color = new Color(1, 1, 1, 1);
        } else if (height >= newMaxHeight * 0.6f) {
            //Rock Color
            color = new Color(Random.Range(0.25f, 0.35f), Random.Range(0.25f, 0.35f), Random.Range(0.25f, 0.35f), 1);
        } else if (height >= newMaxHeight * 0.4f) {
            //Grass Color
            color = new Color(Random.Range(0.40f, 0.42f), Random.Range(0.53f, 0.58f), Random.Range(0.12f, 0.16f), 1);
        } else if (height >= newMaxHeight * 0.35f) {
            //Sand Color
            color = new Color(Random.Range(0.78f, 0.82f), Random.Range(0.74f, 0.78f), Random.Range(0.66f, 0.7f), 1);
        } else {
            //Water Color
            color = new Color(Random.Range(0.5f, 0.54f), Random.Range(0.74f, 0.78f), Random.Range(0.85f, 0.9f), 1);
        }
        return color;
    }

    float heightFromNoise(float x, float z) {
        float pX = (x / 15f) + 500f;
        float pZ = (z / 15f) + 500f;

        float noiseHeight = MAX_HEIGHT * (Mathf.PerlinNoise(pX, pZ) +
                        (0.5f * Mathf.PerlinNoise(2f * pX, 2f * pZ)) +
                        (0.25f * Mathf.PerlinNoise(4f * pX, 4f * pZ)));
        return noiseHeight;
    }

    float Amplitude(int x, int z) {
        return MAX_HEIGHT;
    }

    void removeTerrain(Vector2 terrainKey) {
        terrainCount--;
        string terrainName = "";
        terrainMap.TryGetValue(terrainKey, out terrainName);
        GameObject terrainToGo = GameObject.Find(terrainName);
        Destroy(terrainToGo);
        terrainMap.Remove(terrainKey);
        terrainToTreeMap.Remove(terrainKey);
        terrainToFlowerMap.Remove(terrainKey);
    }

    void handleInput() {
        // get the horizontal and verticle controls (arrows, or WASD keys)
        float dx = Input.GetAxis ("Horizontal");
        float dz = Input.GetAxis ("Vertical");

        //get space and shift Input
        if (Input.GetKeyDown("space")) {
            MainCamera.transform.Translate(0, 1, 0);
        }
        if (Input.GetKeyDown("left shift")) {
            MainCamera.transform.Translate(0, -1, 0);
        }

        // translate forward or backwards
        MainCamera.transform.Translate (0, 0, dz * translateFactor);

        // rotate left or right
        MainCamera.transform.Rotate (0, dx * rotateFactor, 0);
    }

    void checkTerrains() {
        // grab the main camera position
        Vector3 camPos = MainCamera.transform.position;
        Vector2 modCamPos = new Vector2(camPos.x, camPos.z);

        // if the camera has moved far enough, create another plane
        Vector2 currentKey = closestKey(modCamPos);
        if (Vector2.Distance(currentKey, modCamPos) > RENDER_DISTANCE) {
            Vector2 diff = modCamPos - currentKey;
            Vector2 newKeyNE = new Vector2(currentKey.x + (SIDE_LENGTH * Mathf.Sign(diff.x)),
                                        currentKey.y + (SIDE_LENGTH * Mathf.Sign(diff.y)));
            Vector2 newKeyNW = new Vector2(currentKey.x + (SIDE_LENGTH * -Mathf.Sign(diff.x)),
                                        currentKey.y + (SIDE_LENGTH * Mathf.Sign(diff.y)));
            Vector2 newKeyE = new Vector2(currentKey.x + (SIDE_LENGTH * Mathf.Sign(diff.x)), currentKey.y);
            Vector2 newKeyW = new Vector2(currentKey.x + (SIDE_LENGTH * -Mathf.Sign(diff.x)), currentKey.y);
            Vector2 newKeyN = new Vector2(currentKey.x, currentKey.y + (SIDE_LENGTH * Mathf.Sign(diff.y)));

            Vector2[] newKeys = {newKeyNW, newKeyE, newKeyW, newKeyNE, newKeyN};

            for (int i = 0; i < newKeys.Length; i++) {
                Vector2 terrainKey = newKeys[i];
                if (!terrainMap.ContainsKey(terrainKey)) {
                    terrainToTreeMap.Add(terrainKey, createTreeMap(terrainKey));
                    terrainToFlowerMap.Add(terrainKey, createFlowerMap(terrainKey));
                    terrainMap.Add(terrainKey, createTerrain(terrainKey));
                }
            }
        }

        //remove terrains that are too far away
        if (terrainCount > 5) {
            List<Vector2> badTerrainKeys = new List<Vector2>();
            foreach (Vector2 key in terrainMap.Keys) {
                if (Vector2.Distance(key, modCamPos) > CULL_DISTANCE) {
                    badTerrainKeys.Add(key);
                }
            }

            foreach (Vector2 key in badTerrainKeys) {
                removeTerrain(key);
            }
        }
    }

    Vector2 closestKey(Vector2 camPos) {
        float minDist = Mathf.Infinity;
        Vector2 lowest = new Vector2(0, 0);
        float currentDist = 0.0f;
        foreach (Vector2 key in terrainMap.Keys) {
            currentDist = Vector2.Distance(key, camPos);
            if (currentDist < minDist) {
                lowest = key;
                minDist = currentDist;
            }
        }
        return lowest;
    }

    int[,] createTreeMap(Vector2 terrainKey) {
        int seed = (5 * (int)terrainKey.x) + (7 * (int)terrainKey.y);
        Random.InitState(seed);
        int[,] map = new int[SIDE_LENGTH + 1, SIDE_LENGTH + 1];
        int TOTAL_RUNS = 5;

        for (int z = 0; z <= SIDE_LENGTH; z++) {
            for (int x = 0; x <= SIDE_LENGTH; x++) {
                if (z == 0 || z == SIDE_LENGTH || x == 0 || x == SIDE_LENGTH) {
                    map[x, z] = 0;
                } else if (Random.value <= 0.4) {
                    map[x, z] = 1;
                } else {
                    map[x, z] = 0;
                }
            }
        }

        int mooreValue = 0;
        int value = 0;
        for (int runs = 0; runs < TOTAL_RUNS; runs++) {
            for (int z = 1; z < SIDE_LENGTH; z++) {
                for (int x = 1; x < SIDE_LENGTH; x++) {
                    mooreValue = mooreNeighborhood(map, x, z);
                    value = map[x, z];
                    if (value == 1 && mooreValue < 3) {
                        map[x, z] = 0;
                    } else if (value ==0 && mooreValue > 4) {
                        map[x, z] = 1;
                    }
                }
            }
        }

        return map;
    }

    int mooreNeighborhood(int[,] map, int x, int z) {
        int value = 0;
        value += map[x, z + 1];
        value += map[x, z - 1];
        value += map[x + 1, z - 1];
        value += map[x + 1, z + 1];
        value += map[x - 1, z - 1];
        value += map[x - 1, z + 1];
        value += map[x - 1, z];
        value += map[x + 1, z];
        return value;
    }

    int[,] createFlowerMap(Vector2 terrainKey) {
        int seed = (13 * (int)terrainKey.x) + (7 * (int)terrainKey.y);
        Random.InitState(seed);
        int[,] map = new int[SIDE_LENGTH + 1, SIDE_LENGTH + 1];
        int TOTAL_RUNS = 5;

        for (int z = 0; z <= SIDE_LENGTH; z++) {
            for (int x = 0; x <= SIDE_LENGTH; x++) {
                if (Random.value <= 0.4) {
                    map[x, z] = 1;
                } else {
                    map[x, z] = 0;
                }
            }
        }

        int mooreValue = 0;
        int value = 0;
        for (int runs = 0; runs < TOTAL_RUNS; runs++) {
            for (int z = 1; z < SIDE_LENGTH; z++) {
                for (int x = 1; x < SIDE_LENGTH; x++) {
                    mooreValue = mooreNeighborhood(map, x, z);
                    value = map[x, z];
                    if (value == 1 && mooreValue < 3) {
                        map[x, z] = 0;
                    } else if (value ==0 && mooreValue > 4) {
                        map[x, z] = 1;
                    }
                }
            }
        }

        return map;
    }

    bool flatEnough(Vector3 normal) {
        return true;
    }

}
