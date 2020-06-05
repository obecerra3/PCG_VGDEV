using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class LoopSubdivision {

    static CMesh current_mesh;
    static List<int> triangle_table;
    static List<Vector3> geo_table;
    static List<int> opposite_table;

    public static void setParameters(CMesh _mesh) {
        current_mesh = _mesh;
        triangle_table = current_mesh.triangle_table;
        geo_table = current_mesh.geo_table;
        opposite_table = current_mesh.opposite_table;
    }

    public static void subdivide(List<Edge> hard_edges, List<Vector3> hard_vertices, int times) {
        for (int i = 0; i < times; i++) {
            subdivideHelper(hard_edges, hard_vertices);
        }
    }

    public static void subdivide(int times) {
        for (int i = 0; i < times; i++) {
            subdivideHelper(current_mesh.hard_edges, current_mesh.hard_vertices);
        }
    }

    public static void subdivideHelper(List<Edge> hard_edges, List<Vector3> hard_vertices) {
        if (opposite_table.Count == 0) {
            buildOppositeTable();
        }

        CMesh new_mesh = new CMesh();

        int start_corner, current_corner, n;
        Vector3 vertex, neighbor_sum, new_vertex;
        float beta;

        // Debug.Log("assigning even vertices");
        //EVEN Vertices
        for (int vertex_index = 0; vertex_index < geo_table.Count; vertex_index++) {
            start_corner = triangle_table.FindIndex(v => v == vertex_index);
            current_corner = start_corner;
            vertex = geo_table[vertex_index];

            //test for hard_edge or hard_vertex
            if (isHardEdge(hard_edges, vertex)) {
                //hard edge found
                new_vertex = (0.75f * vertex) + (0.125f * geo_table[triangle_table[nextCorner(current_corner)]]) + (0.125f * geo_table[triangle_table[previousCorner(current_corner)]]);
                new_mesh.geo_table.Add(new_vertex);
            } else if (isHardVertex(hard_vertices, vertex)) {
                //hard vertex found
                new_mesh.geo_table.Add(vertex);
            } else {
                //normal subdivision
                neighbor_sum = new Vector3();
                n = 0;
                List<int> opposite_table2 = opposite_table;

                do {
                    neighbor_sum = neighbor_sum + geo_table[triangle_table[nextCorner(current_corner)]];
                    n++;
                    current_corner = swing(current_corner);
                } while (start_corner != current_corner && current_corner != -1);

                beta = 0.0f;
                if (n > 3) {
                    beta = 3.0f / (8.0f * n);
                } else if (n == 3) {
                    beta = 3.0f / 16.0f;
                }

                new_vertex = vertex * (1 - n * beta);
                new_vertex = new_vertex + (neighbor_sum * beta);
                new_mesh.geo_table.Add(new_vertex);
            }
        }

        int[] oppositeEdgeTable = new int[triangle_table.Count];

        HashSet<int> set1 = new HashSet<int>();

        Vector3 a, b, c, d, odd_vertex, vec1, vec2, vec3, vec4;

        // Debug.Log("assigning odd vertices and Opposite Edge Table");
        for (int corner = 0; corner < triangle_table.Count; corner++) {
            if (!set1.Contains(corner)) {
                //copy a,b,c,d
                a = geo_table[triangle_table[nextCorner(corner)]];
                b = geo_table[triangle_table[nextCorner(opposite_table[corner])]];
                c = geo_table[triangle_table[corner]];
                d = geo_table[triangle_table[opposite_table[corner]]];

                if (isHardEdge(hard_edges, new Edge(a, b))) {
                    //hard_edge found
                    odd_vertex = (0.5f * a) + (0.5f * b);
                    new_mesh.geo_table.Add(odd_vertex);

                    //add to hard_edges
                    hard_edges.Add(new Edge(a, odd_vertex));
                    hard_edges.Add(new Edge(odd_vertex, b));

                    //add to set1 and oppositeEdgeTable
                    set1.Add(corner);
                    set1.Add(opposite_table[corner]);
                    oppositeEdgeTable[corner] = new_mesh.geo_table.Count - 1;
                    oppositeEdgeTable[opposite_table[corner]] = new_mesh.geo_table.Count - 1;
                } else {
                    //normal odd subdivision
                    vec1 = a + b;
                    vec2 = c + d;
                    vec3 = vec1 * (3.0f/8.0f);
                    vec4 = vec2 * (1.0f/8.0f);
                    odd_vertex = vec3 + vec4;

                    set1.Add(corner);
                    set1.Add(opposite_table[corner]);
                    new_mesh.geo_table.Add(odd_vertex);
                    oppositeEdgeTable[corner] = new_mesh.geo_table.Count - 1;
                    oppositeEdgeTable[opposite_table[corner]] = new_mesh.geo_table.Count - 1;
                }
            }
        }

        // new_mesh.triangle_table = new List<int>();

        // Debug.Log("assigning odd triangles");
        int odd_vertex1, odd_vertex2, odd_vertex3;
        for (int face = 0; face < triangle_table.Count; face += 3) {
            odd_vertex1 = oppositeEdgeTable[previousCorner(face)];
            odd_vertex2 = oppositeEdgeTable[face];
            odd_vertex3 = oppositeEdgeTable[nextCorner(face)];

            //face1
            new_mesh.triangle_table.Add(triangle_table[face]);
            new_mesh.triangle_table.Add(odd_vertex1);
            new_mesh.triangle_table.Add(odd_vertex3);
            //face2
            new_mesh.triangle_table.Add(odd_vertex1);
            new_mesh.triangle_table.Add(triangle_table[nextCorner(face)]);
            new_mesh.triangle_table.Add(odd_vertex2);
            //face3
            new_mesh.triangle_table.Add(odd_vertex3);
            new_mesh.triangle_table.Add(odd_vertex2);
            new_mesh.triangle_table.Add(triangle_table[previousCorner(face)]);
            //face4
            new_mesh.triangle_table.Add(odd_vertex1);
            new_mesh.triangle_table.Add(odd_vertex2);
            new_mesh.triangle_table.Add(odd_vertex3);
        }

        current_mesh.clearTables();

        // Debug.Log("assigning new triangles");
        for (int i = 0; i < new_mesh.triangle_table.Count; i++) {
            triangle_table.Add(new_mesh.triangle_table[i]);
        }

        // Debug.Log("assigning new geo_table vertices");
        for (int i = 0; i < new_mesh.geo_table.Count; i++) {
            geo_table.Add(new_mesh.geo_table[i]);
        }

        new_mesh.clearTables();

        current_mesh.mesh.vertices = current_mesh.geo_table.ToArray();
        current_mesh.mesh.triangles = current_mesh.triangle_table.ToArray();
        current_mesh.mesh.RecalculateNormals();

    }

    public static bool isHardEdge(List<Edge> hard_edges, Vector3 vertex) {
        Edge edge;
        for (int i = 0; i < hard_edges.Count; i++) {
            edge = hard_edges[i];
            if (vertex == edge.v[0] || vertex == edge.v[1]) {
                return true;
            }
        }
        return false;
    }

    public static bool isHardEdge(List<Edge> hard_edges, Edge _edge) {
        Edge edge;
        for (int i = 0; i < hard_edges.Count; i++) {
            edge = hard_edges[i];
            if (edge.equalTo(_edge)) {
                return true;
            }
        }
        return false;
    }

    public static bool isHardVertex(List<Vector3> hard_vertices, Vector3 _vertex) {
        Vector3 vertex;
        for (int i = 0; i < hard_vertices.Count; i++) {
            vertex = hard_vertices[i];
            if (vertex == _vertex) {
                return true;
            }
        }
        return false;
    }
    public static void buildOppositeTable() {
        // Debug.Log("buildOppositeTable start");
        int[] opposite_temp = new int[triangle_table.Count];
        for (int i = 0; i < triangle_table.Count; i++) {
            opposite_temp[i] = -1;
        }

        for (int i = 0; i < triangle_table.Count; i++) {
            for (int j = 0; j < triangle_table.Count; j++) {
                if (triangle_table[nextCorner(i)] == triangle_table[previousCorner(j)] && triangle_table[previousCorner(i)] == triangle_table[nextCorner(j)]) {
                    opposite_temp[i] = j;
                    opposite_temp[j] = i;
                }
            }
        }

        for (int i = 0; i < triangle_table.Count; i++) {
            opposite_table.Add(opposite_temp[i]);
        }
    }

    public static int previousCorner(int corner) {
        return nextCorner(nextCorner(corner));
    }

    public static int nextCorner(int corner) {
        return 3 * triangleCorner(corner) + ((corner + 1) % 3);
    }

    public static int triangleCorner(int corner) {
        return corner / 3;
    }

    public static int swing(int corner) {
        int next = nextCorner(corner);
        int opposite = opposite_table[next];
        if (opposite == -1) {
            return -1;
        }
        int nextnext = nextCorner(opposite);
        return nextnext;
    }

}
