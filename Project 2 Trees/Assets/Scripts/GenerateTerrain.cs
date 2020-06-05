using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateTerrain : MonoBehaviour {

    const int SIDE_LENGTH = 250;
    const int MAX_HEIGHT = 17;

    void Start() {
    }

    void Update() {
    }

    public GameObject createTerrain(Vector2 terrainKey) {
        GameObject terrain = new GameObject();
        Mesh mesh = generateTerrainMesh(terrain, terrainKey);
        terrain.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse"));
        Texture2D texture = makeTexture(mesh.vertices);
        Renderer renderer = terrain.GetComponent<Renderer>();
        renderer.material.mainTexture = texture;
        terrain.name = "Terrain";
        return terrain;
    }

    Mesh generateTerrainMesh(GameObject terrain, Vector2 terrainKey) {
        MeshFilter meshFilter = (MeshFilter)terrain.AddComponent(typeof(MeshFilter));
        terrain.AddComponent(typeof(MeshRenderer));

        Mesh mesh = new Mesh();

        int verticeIndex = 0;
        int verticesLength = (SIDE_LENGTH + 1) * (SIDE_LENGTH + 1);
        Vector3[] vertices = new Vector3[verticesLength];
        Vector2[] uv = new Vector2[verticesLength];
        float height = 0f;

        for (int z = 0; z <= SIDE_LENGTH; z++) {
            for (int x = 0; x <= SIDE_LENGTH; x++) {
                height = heightFromNoise(x + (terrainKey.x - SIDE_LENGTH / 2f), z + (terrainKey.y - SIDE_LENGTH / 2f), terrainKey);
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
        mesh.RecalculateNormals();

        terrain.GetComponent<MeshFilter>().mesh = mesh;

        return mesh;
    }

    Texture2D makeTexture(Vector3[] vertices) {
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

    Color calculateColor(float height, float x, float z) {
        Color color = new Color(0, 0, 0, 1);
        float newMaxHeight = MAX_HEIGHT + 5;

        if (height >= newMaxHeight * 0.95f) {
            //Snow Color
            color = new Color(1, 1, 1, 1);
        } else if (height >= newMaxHeight * 0.6f) {
            //Rock Color
            color = new Color(Random.Range(0.25f, 0.35f), Random.Range(0.25f, 0.35f), Random.Range(0.25f, 0.35f), 1);
        } else {
            //Grass Color
            color = new Color(Random.Range(0.40f, 0.42f), Random.Range(0.53f, 0.58f), Random.Range(0.12f, 0.16f), 1);
        }
        return color;
    }

    float heightFromNoise(float x, float z, Vector2 terrainCenter) {
        float distance = Vector2.Distance(new Vector2(x, z), terrainCenter);
        float heightScale = (distance + 0.1f) / (SIDE_LENGTH / 1.8f);

        float pX = (x / 15f) + 500f;
        float pZ = (z / 15f) + 500f;

        float noiseHeight = MAX_HEIGHT * (Mathf.PerlinNoise(pX, pZ) +
                        (0.5f * Mathf.PerlinNoise(2f * pX, 2f * pZ)) +
                        (0.25f * Mathf.PerlinNoise(4f * pX, 4f * pZ)));

        return noiseHeight * heightScale;
    }

}
