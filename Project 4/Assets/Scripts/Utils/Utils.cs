using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils {
    static bool debug_on = true;

    public static void debugSphere(Vector3 pos, Color color, float scale, Transform parent = null) {
        if (debug_on) {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.localScale *= scale;
            sphere.transform.position = pos;
            if (parent != null) {
                sphere.transform.parent = parent;
            }
            Renderer renderer = sphere.GetComponent<Renderer>();
            renderer.material.color = color;
        }
    }

    public static void drawSphere(Transform parent, Vector3 pos, Color color, float scale) {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.localScale *= scale;
        sphere.transform.position = pos;
        sphere.transform.parent = parent;
        Renderer renderer = sphere.GetComponent<Renderer>();
        renderer.material.color = color;
    }

    public static void printArray<T>(IList<T> values) {
        string value_string = "[";
        foreach (T value in values) {
            value_string += value + ", ";
        }
        value_string += "]";
        Debug.Log(value_string);
    }

    public static Texture2D makeTexture(Vector3[] vertices, string component_type, List<Color> _colors) {
        int texture_length = Mathf.FloorToInt(Mathf.Sqrt(vertices.Length));
        Texture2D texture = new Texture2D (texture_length, texture_length);
        Color[] colors = new Color[vertices.Length];
        Vector3 vertex = new Vector3();
        int index = 0;
        Color color;
        for (int z = 0; z < texture_length; z++) {
            for (int x = 0; x < texture_length; x++) {
                index = z * texture_length + x;
                vertex = vertices[index];
                color = Color.red;
                colors[index] = color;
            }
        }

        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colors);
        texture.Apply();
        return texture;
    }


}
