using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Edge {
    public Vector3[] v = new Vector3[2];

    public Edge(Vector3 v1, Vector3 v2) {
        v[0] = v1;
        v[1] = v2;
    }

    public bool equalTo(Edge _edge) {
        return (_edge.v[0] == v[0] && _edge.v[1] == v[1]) || (_edge.v[0] == v[1] && _edge.v[1] == v[0]);
    }

}
