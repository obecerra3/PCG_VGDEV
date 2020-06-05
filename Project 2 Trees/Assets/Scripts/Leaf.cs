using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leaf : MonoBehaviour {

    public Mesh createMesh() {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[4];
        Vector2[] uv = new Vector2[4];
        int[] triangles = new int[6];

        float length = Random.Range(0.1f, 0.6f);
        float width = Random.Range(0.05f, 0.2f);

        vertices[0] = new Vector3(0, 0, 0);
        vertices[1] = new Vector3(width, 0, 0);
        vertices[2] = new Vector3(width * 0.5f, length, 0);
        vertices[3] = new Vector3(width * 0.5f, -1f * length, 0);

        uv[0] = new Vector2(0f, 0f);
        uv[1] = new Vector2(0.25f, 0.25f);
        uv[2] = new Vector2(0.75f, 0.75f);
        uv[3] = new Vector2(1f, 1f);


        triangles[0] = 2;
        triangles[1] = 1;
        triangles[2] = 0;
        triangles[3] = 1;
        triangles[4] = 3;
        triangles[5] = 0;

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }

    public Texture2D createTexture() {
        Color current_color = new Color(Random.Range(0.1f, 0.4f), Random.Range(0.6f, 0.9f), Random.Range(0.1f, 0.4f), 1f);
        Texture2D texture = new Texture2D (2, 2);
        Color[] colors = new Color[4];

        for (int c = 0; c < colors.Length; c++) {
            colors[c] = current_color;
        }

        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colors);
        texture.Apply();
        return texture;
    }

}
