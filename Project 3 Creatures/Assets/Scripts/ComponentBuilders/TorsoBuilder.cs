using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorsoBuilder {

    //torso components
    GameObject torso_obj;
    CMesh torso_mesh;

    //torso settings
    public int cp_count = 10;
    public int length;
    public float box_length;
    public float box_height;
    public float box_width;
    public float top_middle_offset;
    public float top_offset;
    public float side_middle_offset;
    public float side_offset;
    public float length_delta;
    public int torso_type;

    //torso variables
    public List<Vector3> cps;

    //texture variables
    public List<Color> colors;

    public GameObject build() {
        torso_obj = new GameObject();
        torso_obj.name = "torso";

        initSettings();

        buildMesh();

        MeshFilter mesh_filter = (MeshFilter) torso_obj.AddComponent(typeof(MeshFilter));
        MeshRenderer mesh_renderer = (MeshRenderer) torso_obj.AddComponent(typeof(MeshRenderer));
        mesh_filter.mesh = torso_mesh.mesh;

        //texture setup
        torso_obj.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse"));
        Texture2D texture = Utils.makeTexture(torso_mesh.mesh.vertices, "torso", colors);
        Renderer renderer = torso_obj.GetComponent<Renderer>();
        renderer.material.mainTexture = texture;

        return torso_obj;
    }

    public void initSettings() {
        float top_offset_delta = 1.0f; //<1.0f should be round, >1.0f should create raised dip effect
        float side_offset_delta = 1.0f; //no decay, can change to lower or higher numbers depending on effect;

        length = (int) Utils.weightedRange(new float[] { 3, 5, 5, 7, 9, 4, 12, 14, 1 }); //Random.Range(3, 15);
        box_length = length / (float) cp_count;
        box_height = box_length; //can change later
        box_width = box_length;
        top_middle_offset = Random.Range(0f, 0.5f);
        top_offset = Random.Range(0f, top_middle_offset * top_offset_delta);
        side_middle_offset = Random.Range(0f, 0.5f);
        side_offset = Random.Range(0f, side_middle_offset * side_offset_delta);
        length_delta = Random.Range(0.5f, 0.7f);
        torso_type = Random.Range(0, 3);
        //reduce the offsets into concepts, e.g. bulge, plate like, rigged, etc...
        //bulge = Random.Range();

        //init colors
        colors = new List<Color>();
        colors.Add(Utils.getBaseColor());
        for (int i = 0; i < Random.Range(1, 5); i++) {
            colors.Add(Utils.getHighlightColor());
        }
    }

    public void buildMesh() {
        torso_mesh = new CMesh();

        uniformMesh();
    }

    public void uniformMesh() {
        cps = new List<Vector3>();
        Vector3 cp_pos = new Vector3(0, 0, 0);
        float cp_distance = box_length;
        float x_wiggle, y_wiggle, z_direction;

        float width_offset = (box_width / 2f) * 0.8f;
        float height_offset = box_height * 0.8f;
        float[] offsets = new float[2];

        List<Vector2> uvs = new List<Vector2>();

        int new_base, old_base;

        //hard_edges and hard_vertices
        List<Vector3> hard_vertices = new List<Vector3>();
        List<Edge> hard_edges = new List<Edge>();
        Vector3 v0, v1, v2, v3;
        float r;

        for (int i = 0; i < cp_count; i++) {
            //distance_along_torso
            r = i / (float) cp_count; //[0, 1]

            //build control points
            Utils.debugSphere(torso_obj.transform, cp_pos, Color.black, 0.1f);
            cps.Add(cp_pos);

            //determine cp_distance
            if (r < 0.3f) {
                cp_distance = cp_distance * 0.8f;
            } else if (r > 0.7f) {
                cp_distance = box_length * 0.1f;
            } else {
                cp_distance = box_length;//* length_delta;
            }

            //build geo_table
            torso_mesh.geo_table.Add(cp_pos + new Vector3(-width_offset, 0f, 0f)); //left top corner
            torso_mesh.geo_table.Add(cp_pos + new Vector3(width_offset, 0f, 0f)); //right top corner
            torso_mesh.geo_table.Add(cp_pos + new Vector3(width_offset, -height_offset, 0f)); //right bottom corner
            torso_mesh.geo_table.Add(cp_pos + new Vector3(-width_offset, -height_offset, 0f)); //left bottom corner

            //determine width_offset and height_offset
            //reduce this into different functions or different mathematical relationship
            offsets = getOffsets(r);
            width_offset = offsets[0];
            height_offset = offsets[1];

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
                // torso_mesh.addTriangle(old_base + 1, old_base, new_base);
                // torso_mesh.addTriangle(new_base + 1, old_base + 1, new_base);
                //counter
                torso_mesh.addTriangle(new_base, old_base, old_base + 1);
                torso_mesh.addTriangle(new_base, old_base + 1, new_base + 1);

                //bottom face counterclockwise
                // torso_mesh.addTriangle(new_base + 3, old_base + 3, old_base + 2);
                // torso_mesh.addTriangle(new_base + 3, old_base + 2, new_base + 2);
                //clockwise
                torso_mesh.addTriangle(old_base + 2, old_base + 3, new_base + 3);
                torso_mesh.addTriangle(new_base + 2, old_base + 2, new_base + 3);

                // //right face clockwise
                // torso_mesh.addTriangle(new_base + 2, old_base + 2, old_base + 1);
                // torso_mesh.addTriangle(new_base + 1, new_base + 2, old_base + 1);
                //counter
                torso_mesh.addTriangle(old_base + 1, old_base + 2, new_base + 2);
                torso_mesh.addTriangle(old_base + 1, new_base + 2, new_base + 1);

                //left face counterclockwise
                // torso_mesh.addTriangle(old_base, old_base + 3, new_base + 3);
                // torso_mesh.addTriangle(old_base, new_base + 3, new_base);
                //clockwise
                torso_mesh.addTriangle(new_base + 3, old_base + 3, old_base);
                torso_mesh.addTriangle(new_base, new_base + 3, old_base);


                if (i == cp_count - 1) {
                    // torso_mesh.addTriangle(i * 4, (i * 4) + 3, (i * 4) + 2);
                    // torso_mesh.addTriangle(i * 4, (i * 4) + 2, (i * 4) + 1);
                    torso_mesh.addTriangle((i * 4) + 2, (i * 4) + 3, (i * 4));
                    torso_mesh.addTriangle((i * 4) + 1, (i * 4) + 2, i * 4);
                }
            } else if (i == 0) {
                // torso_mesh.addTriangle(2, 3, 0);
                // torso_mesh.addTriangle(1, 2, 0);
                torso_mesh.addTriangle(0, 3, 2);
                torso_mesh.addTriangle(0, 2, 1);

                //add hard_hard edges
                v0 = torso_mesh.geo_table[0];
                v1 = torso_mesh.geo_table[1];
                v2 = torso_mesh.geo_table[2];
                v3 = torso_mesh.geo_table[3];
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
            x_wiggle = 0f;
            y_wiggle = 0f;
            z_direction = -1f;
            if (r > 0.6f) {
                y_wiggle = 0.2f;
            }
            cp_pos += new Vector3(x_wiggle, y_wiggle, z_direction).normalized * cp_distance;
        }

        for (int i = 0; i < torso_mesh.geo_table.Count; i++) {
            Utils.debugSphere(torso_obj.transform, torso_mesh.geo_table[i], Color.blue, 0.1f);
        }

        //set values into cmesh.mesh
        torso_mesh.mesh.vertices = torso_mesh.geo_table.ToArray();
        torso_mesh.mesh.uv = uvs.ToArray();
        torso_mesh.mesh.triangles = torso_mesh.triangle_table.ToArray();
        torso_mesh.mesh.RecalculateNormals();

        //loop subdivision
        LoopSubdivision.setParameters(torso_mesh);
        LoopSubdivision.subdivide(hard_edges, hard_vertices, 2);
    }

    public float[] getOffsets(float r) {
        if (torso_type == 0) {
            return uniformOffsets(r);
        } else if (torso_type == 1) {
            return variation1Offsets(r);
        } else {
            return variation2Offsets(r);
        }
    }

    public float[] variation1Offsets(float r) {
        float width_offset = 0f;
        float height_offset = 0f;
        if (r < 0.1f) {
            width_offset = (box_width / 2f) * 0.8f;
            height_offset = box_height * 1.5f;
        } else if (r < 0.2f) {
            width_offset = (box_width / 2f) * 2f;
            height_offset = box_height * 1.5f;
        } else if (r < 0.3) {
            width_offset = (box_width / 2f) * 2.5f;
            height_offset = box_height * 1.5f;
        } else if (r < 0.4) {
            width_offset = (box_width / 2f) * 3f;
            height_offset = box_height * 1.5f;
        } else if (r < 0.5) {
            //up to halfway
            width_offset = (box_width / 2f) * 3.5f;
            height_offset = box_height * 1.5f;
        } else if (r < 0.6) {
            //leaving halfway
            width_offset = (box_width / 2f) * 3f;
            height_offset = box_height * 1.5f;
        } else if (r < 0.7) {
            //towards front
            width_offset = (box_width / 2f) * 1.5f;
            height_offset = box_height * 1.25f;
        } else if (r < 0.8) {
            //towards front
            width_offset = (box_width / 2f) * 0.5f;
            height_offset = box_height * 1.0f;
        } else if (r < 0.9) {
            //towards front
            width_offset = (box_width / 2f) * 0.4f;
            height_offset = box_height * 0.6f;
        } else if (r < 1.0) {
            //towards front
            width_offset = (box_width / 2f) * 0.3f;
            height_offset = box_height * 0.5f;
        }

        return new float[] {width_offset, height_offset};
    }

    public float[] variation2Offsets(float r) {
        float width_offset = 0f;
        float height_offset = 0f;
        if (r < 0.1f) {
            width_offset = (box_width / 2f) * 0.8f;
            height_offset = box_height * 1.5f;
        } else if (r < 0.2f) {
            width_offset = (box_width / 2f) * 0.9f;
            height_offset = box_height * 1.5f;
        } else if (r < 0.3) {
            width_offset = (box_width / 2f) * 0.9f;
            height_offset = box_height * 1.5f;
        } else if (r < 0.4) {
            width_offset = (box_width / 2f) * 1.1f;
            height_offset = box_height * 1.5f;
        } else if (r < 0.5) {
            //up to halfway
            width_offset = (box_width / 2f) * 1.1f;
            height_offset = box_height * 1.5f;
        } else if (r < 0.6) {
            //leaving halfway
            width_offset = (box_width / 2f) * 1.1f;
            height_offset = box_height * 1.5f;
        } else if (r < 0.7) {
            //towards front
            width_offset = (box_width / 2f) * 1.1f;
            height_offset = box_height * 1f;
        } else if (r < 0.8) {
            //towards front
            width_offset = (box_width / 2f) * 1.0f;
            height_offset = box_height * 1.0f;
        } else if (r < 0.9) {
            //towards front
            width_offset = (box_width / 2f) * 0.7f;
            height_offset = box_height * 0.7f;
        } else if (r < 1.0) {
            //towards front
            width_offset = (box_width / 2f) * 0.6f;
            height_offset = box_height * 0.7f;
        }

        return new float[] {width_offset, height_offset};
    }

    public float[] uniformOffsets(float r) {
        float width_offset = 0f;
        float height_offset = 0f;
        if (r < 0.1f) {
            width_offset = (box_width / 2f) * 1.5f;
            height_offset = box_height * 1.5f;
        } else if (r < 0.2f) {
            width_offset = (box_width / 2f) * 1.7f;
            height_offset = box_height * 1.5f;
        } else if (r < 0.3) {
            width_offset = (box_width / 2f) * 1.7f;
            height_offset = box_height * 1.5f;
        } else if (r < 0.4) {
            width_offset = (box_width / 2f) * 2.25f;
            height_offset = box_height * 1.5f;
        } else if (r < 0.5) {
            //up to halfway
            width_offset = (box_width / 2f) * 2.5f;
            height_offset = box_height * 1.75f;
        } else if (r < 0.6) {
            //leaving halfway
            width_offset = (box_width / 2f) * 2.5f;
            height_offset = box_height * 1.5f;
        } else if (r < 0.7) {
            //towards front
            width_offset = (box_width / 2f) * 2.5f;
            height_offset = box_height * 1.25f;
        } else if (r < 0.8) {
            //towards front
            width_offset = (box_width / 2f) * 0.8f;
            height_offset = box_height * 1.0f;
        } else if (r < 0.9) {
            //towards front
            width_offset = (box_width / 2f) * 0.7f;
            height_offset = box_height * 0.7f;
        } else if (r < 1.0) {
            //towards front
            width_offset = (box_width / 2f) * 0.6f;
            height_offset = box_height * 0.7f;
        }

        return new float[] {width_offset, height_offset};
    }
}
