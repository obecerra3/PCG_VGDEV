using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//Short for CornerMesh
public class CMesh {
    public Mesh mesh;
    public List<Vector3> geo_table;
    public List<int> triangle_table;
    public List<int> opposite_table;
    public List<Vector2> uvs;
    public List<Vector3> hard_vertices;
    public List<Edge> hard_edges;

    public CMesh() {
        mesh = new Mesh();
        geo_table = new List<Vector3>();
        triangle_table = new List<int>();
        opposite_table = new List<int>();
        uvs = new List<Vector2>();
        hard_vertices = new List<Vector3>();
        hard_edges = new List<Edge>();
    }

    public void clearTables() {
        geo_table.Clear();
        triangle_table.Clear();
        opposite_table.Clear();
        uvs.Clear();
    }

    public void addTriangle(int v1, int v2, int v3) {
        triangle_table.Add(v1);
        triangle_table.Add(v2);
        triangle_table.Add(v3);
    }

}
