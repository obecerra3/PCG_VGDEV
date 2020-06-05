using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TailBuilder {

    //tail components
    GameObject tail_obj;
    CMesh tail_mesh;

    enum tail_types {
        uniform,
        uniform_top_ridge,
        uniform_side_ridge,
        uniform_both,
        spiral,
        variation1,
        variation2,
    };


    //tail settings
    public int type;
    public int cp_count;
    public float length;
    public float box_length;
    public float box_height;
    public float box_width;
    public float top_middle_offset;
    public float top_offset;
    public float side_middle_offset;
    public float side_offset;

    public GameObject build(TorsoBuilder torso_builder) {
        tail_obj = new GameObject();
        tail_obj.name = "tail";

        initSettings(torso_builder);

        buildMesh();

        MeshFilter mesh_filter = (MeshFilter) tail_obj.AddComponent(typeof(MeshFilter));
        MeshRenderer mesh_renderer = (MeshRenderer) tail_obj.AddComponent(typeof(MeshRenderer));
        mesh_filter.mesh = tail_mesh.mesh;

        tail_obj.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse"));
        Texture2D texture = Utils.makeTexture(tail_mesh.mesh.vertices, "tail", torso_builder.colors);
        Renderer renderer = tail_obj.GetComponent<Renderer>();
        renderer.material.mainTexture = texture;

        return tail_obj;
    }

    public void initSettings(TorsoBuilder torso_builder) {
        float top_offset_delta = 1.0f; //<1.0f should be round, >1.0f should create raised dip effect
        float side_offset_delta = 1.0f; //no decay, can change to lower or higher numbers depending on effect;
        type = Random.Range(0, 7);
        cp_count = Random.Range(8, 15);
        length = torso_builder.length * Random.Range(2f, 4f);//+ Random.Range(0, 7);
        box_length = length / (float) cp_count;
        box_height = torso_builder.box_height * 0.8f;
        box_width = torso_builder.box_width * 0.8f;
        top_middle_offset = Random.Range(0f, 0.5f);
        top_offset = Random.Range(0f, top_middle_offset * top_offset_delta);
        side_middle_offset = Random.Range(0f, 0.5f);
        side_offset = Random.Range(0f, side_middle_offset * side_offset_delta);
    }

    public void buildMesh() {
        tail_mesh = new CMesh();

        // int tail_type = Random.Range(0, 7);
        int tail_type = 0;

        switch (tail_type) {
            case (int) tail_types.uniform:
                uniformMesh();
                break;
        }
    }

    public void uniformMesh() {
        List<Vector3> cps = new List<Vector3>();
        Vector3 cp_pos = new Vector3(0, 0, -0.05f);
        float cp_distance = box_length;
        float x_wiggle, y_wiggle, z_direction;

        float width_offset = box_width / 2f;
        float height_offset = box_height;

        List<Vector2> uvs = new List<Vector2>();

        int new_base, old_base;

        //hard_edges and hard_vertices
        List<Vector3> hard_vertices = new List<Vector3>();
        List<Edge> hard_edges = new List<Edge>();
        Vector3 v0, v1, v2, v3;

        for (int i = 0; i < cp_count; i++) {
            //build control points
            Utils.debugSphere(tail_obj.transform, cp_pos, Color.black, 0.1f);
            cps.Add(cp_pos);

            //determine cp_distance
            cp_distance *= 0.8f;

            //build geo_table
            tail_mesh.geo_table.Add(cp_pos + new Vector3(-width_offset, 0f, 0f)); //left top corner
            tail_mesh.geo_table.Add(cp_pos + new Vector3(width_offset, 0f, 0f)); //right top corner
            tail_mesh.geo_table.Add(cp_pos + new Vector3(width_offset, -height_offset, 0f)); //right bottom corner
            tail_mesh.geo_table.Add(cp_pos + new Vector3(-width_offset, -height_offset, 0f)); //left bottom corner

            //determine width_offset and height_offset
            width_offset *= 0.8f;
            height_offset *= 0.8f;

            //build uv
            uvs.Add(new Vector2(0f, i / (float) cp_count));
            uvs.Add(new Vector2(0.33f, i / (float) cp_count));
            uvs.Add(new Vector2(0.66f, i / (float) cp_count));
            uvs.Add(new Vector2(1f, i / (float) cp_count));

            //build triangle_table
            //other method could be to create int v0 through vX and just assign them like that, in this case max is v7
            if (i > 0) {
                new_base = i * 4;
                old_base = (i - 1) * 4;
                //top face clockwise
                tail_mesh.addTriangle(old_base + 1, old_base, new_base);
                tail_mesh.addTriangle(new_base + 1, old_base + 1, new_base);
                //counterclockwise
                // tail_mesh.addTriangle(new_base, old_base, old_base + 1);
                // tail_mesh.addTriangle(new_base, old_base + 1, new_base + 1);

                //bottom face counterclockwise
                tail_mesh.addTriangle(new_base + 3, old_base + 3, old_base + 2);
                tail_mesh.addTriangle(new_base + 3, old_base + 2, new_base + 2);

                //right face clockwise
                tail_mesh.addTriangle(new_base + 2, old_base + 2, old_base + 1);
                tail_mesh.addTriangle(new_base + 1, new_base + 2, old_base + 1);
                //counterclockwise
                // tail_mesh.addTriangle(old_base + 1, old_base + 2, new_base + 2);
                // tail_mesh.addTriangle(old_base + 1, new_base + 2, new_base + 1);

                //left face counterclockwise
                tail_mesh.addTriangle(old_base, old_base + 3, new_base + 3);
                tail_mesh.addTriangle(old_base, new_base + 3, new_base);

                if (i == cp_count - 1) {
                    tail_mesh.addTriangle(i * 4, (i * 4) + 3, (i * 4) + 2);
                    tail_mesh.addTriangle(i * 4, (i * 4) + 2, (i * 4) + 1);
                }
            } else if (i == 0) {
                tail_mesh.addTriangle(2, 3, 0);
                tail_mesh.addTriangle(1, 2, 0);

                //add hard_hard edges
                v0 = tail_mesh.geo_table[0];
                v1 = tail_mesh.geo_table[1];
                v2 = tail_mesh.geo_table[2];
                v3 = tail_mesh.geo_table[3];
                hard_edges.Add(new Edge(v0, v1));
                hard_edges.Add(new Edge(v1, v2));
                hard_edges.Add(new Edge(v2, v3));
                hard_edges.Add(new Edge(v3, v0));
                // hard_vertices.Add(v0);
                // hard_vertices.Add(v1);
                // hard_vertices.Add(v2);
                // hard_vertices.Add(v3);
            }

            //update cp_pos
            x_wiggle = Random.Range(-0.5f, 0.5f); //Define these as constants?
            y_wiggle = Random.Range(-0.2f, 0.1f);
            z_direction = 1f;
            cp_pos += new Vector3(x_wiggle, y_wiggle, z_direction).normalized * cp_distance;
        }

        for (int i = 0; i < tail_mesh.geo_table.Count; i++) {
            Utils.debugSphere(tail_obj.transform, tail_mesh.geo_table[i], Color.blue, 0.1f);
        }

        //set values into cmesh.mesh
        tail_mesh.mesh.vertices = tail_mesh.geo_table.ToArray();
        tail_mesh.mesh.uv = uvs.ToArray();
        tail_mesh.mesh.triangles = tail_mesh.triangle_table.ToArray();
        tail_mesh.mesh.RecalculateNormals();

        //loop subdivision
        LoopSubdivision.setParameters(tail_mesh);
        LoopSubdivision.subdivide(hard_edges, hard_vertices, 2);

    }
}
