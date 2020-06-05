using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimbBuilder {

    //child builder
    ClawBuilder claw_builder = new ClawBuilder();

    //limb components
    GameObject limb_obj;
    CMesh front_right_mesh;
    GameObject front_right_obj, front_left_obj, back_right_obj, back_left_obj;

    //limb settings
    public int cp_count = 10;
    public CpState front_right_state;

    public float length;

    public float box_length;
    public float box_height;
    public float box_width;

    //===============================================================
    //===============================================================
    //GAME_OBJECTS
    //===============================================================
    //===============================================================

    public GameObject build(TorsoBuilder torso_builder) {
        limb_obj = new GameObject();
        limb_obj.name = "limbs";

        initSettings(torso_builder);

        buildMesh();

        //==========
        //front right
        //==========
        front_right_obj = new GameObject();
        MeshFilter mesh_filter = (MeshFilter) front_right_obj.AddComponent(typeof(MeshFilter));
        MeshRenderer mesh_renderer = (MeshRenderer) front_right_obj.AddComponent(typeof(MeshRenderer));
        mesh_filter.mesh = front_right_mesh.mesh;
        front_right_obj.transform.parent = limb_obj.transform;
        front_right_obj.name = "front_right_limb";

        //texture setup
        front_right_obj.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse"));
        Texture2D texture = Utils.makeTexture(front_right_mesh.mesh.vertices, "limbs", torso_builder.colors);
        Renderer renderer = front_right_obj.GetComponent<Renderer>();
        renderer.material.mainTexture = texture;

        //assign child claw_builder
        claw_builder.build(this, front_right_obj.transform, torso_builder);

        //==========
        //front left
        //==========
        front_left_obj = Object.Instantiate(front_right_obj, limb_obj.transform);
        front_left_obj.transform.localScale = new Vector3(-1f, 1f, 1f);
        front_left_obj.name = "front_left_limb";

        //==========
        //back right
        //==========
        float back_inward_offset = Random.Range(0.1f, 0.15f);
        // Vector3 back_scale_offset = new Vector3(Random.Range(2f, 2f), Random.Range(1.2f, 1.3f), Random.Range(1.2f, 1.3f));
        Vector3 pos = front_right_obj.transform.position - new Vector3(-torso_builder.box_width * back_inward_offset, 0, torso_builder.box_length * -5f);
        back_right_obj = Object.Instantiate(front_right_obj, pos, Quaternion.identity, limb_obj.transform);
        // back_right_obj.transform.localScale = back_scale_offset;
        back_right_obj.name = "back_right_limb";

        //==========
        //back left
        //==========
        back_left_obj = Object.Instantiate(front_left_obj, front_right_obj.transform.position - new Vector3(torso_builder.box_width * back_inward_offset, 0, torso_builder.box_length * -5f), Quaternion.identity, limb_obj.transform);
        // back_left_obj.transform.localScale = Vector3.Scale(back_scale_offset, new Vector3(-1f, 1f, 1f));
        back_left_obj.name = "back_left_limb";

        return limb_obj;
    }

    //===============================================================
    //===============================================================
    //INIT
    //===============================================================
    //===============================================================

    public void initSettings(TorsoBuilder torso_builder) {
        cp_count = 10;

        length = torso_builder.length / 4f;

        box_length = length / (float) cp_count;
        box_height = box_length * 3f;
        box_width = box_length * 3f;

        initCpState(torso_builder);

    }

    //===============================================================
    //===============================================================
    //INIT HELPERS
    //===============================================================
    //===============================================================

    public void initCpState(TorsoBuilder torso_builder) {
        //==========
        //front right
        //==========
        float[] torso_offset = torso_builder.getOffsets(0.1f);
        Vector3 front_offset = new Vector3(-torso_offset[0] * 0.75f, -torso_offset[1] / 3f, 0f);
        Vector3 front_start = (torso_builder.cps[torso_builder.cps.Count - 3] + torso_builder.cps[torso_builder.cps.Count - 2]) * 0.5f;
        Vector3 init_pos = front_start + front_offset;
        int type = 0;//Random.Range(0, 2); //0
        // Debug.Log(type);
        List<float> distances;// = new List<float>();
        if (type == 0) {
            distances = new List<float>() {
                box_length * 1.5f,
                box_length,
                box_length,
                box_length,
                box_length,
                box_length,
                box_length,
                box_length * 0.5f,
                box_length * 0.5f
             };
        } else {
            //default
            distances = new List<float>() {
                box_length,
                box_length,
                box_length,
                box_length,
                box_length,
                box_length,
                box_length,
                box_length,
                box_length,
            };
        }
        List<float> bend_points = new List<float>() {0.4f};
        List<Vector3> directions = new List<Vector3>() {
            new Vector3(Random.Range(-0.9f, -0.25f), Random.Range(-0.4f, 0f), Random.Range(0.5f, 0.9f)).normalized,
            new Vector3(Random.Range(0f, 0f), Random.Range(-0.3f, -0.3f), Random.Range(-0.7f, -0.25f)).normalized
        };
        type = 0;
        List<Vector2> size_offsets;
        if (type == 0) {
            size_offsets = new List<Vector2>() {
                new Vector2(box_width * 0.25f, box_height * 0.25f),
                new Vector2(box_width * 0.25f, box_height * 0.25f),
                new Vector2(box_width * 0.25f, box_height * 0.25f),
                new Vector2(box_width * 0.25f, box_height * 0.25f),
                new Vector2(box_width * 0.25f, box_height * 0.25f),
                new Vector2(box_width * 0.3f, box_height * 0.4f),
                new Vector2(box_width * 0.25f, box_height * 0.3f),
                new Vector2(box_width * 0.2f, box_height * 0.25f),
                new Vector2(box_width * 0.2f, box_height * 0.2f),
                new Vector2(box_width * 0.2f, box_height * 0.2f)
            };
        } else {
            //default
            size_offsets = new List<Vector2>() {
                new Vector2(box_width * 0.5f, box_height * 0.5f),
                new Vector2(box_width * 0.5f, box_height * 0.5f),
                new Vector2(box_width * 0.5f, box_height * 0.5f),
                new Vector2(box_width * 0.5f, box_height * 0.5f),
                new Vector2(box_width * 0.5f, box_height * 0.5f),
                new Vector2(box_width * 0.5f, box_height * 0.5f),
                new Vector2(box_width * 0.5f, box_height * 0.5f),
                new Vector2(box_width * 0.5f, box_height * 0.5f),
                new Vector2(box_width * 0.5f, box_height * 0.5f),
                new Vector2(box_width * 0.5f, box_height * 0.5f)
            };
        }
        front_right_state = new CpState(init_pos, distances, bend_points, directions, size_offsets);
    }

    //===============================================================
    //===============================================================
    //MESH
    //===============================================================
    //===============================================================

    public void buildMesh() {
        front_right_mesh = new CMesh();

        float current_cp;

        for (int i = 0; i < cp_count; i++) {
            current_cp = i / (float) cp_count; //[0, 1]

            //build control points
            Utils.debugSphere(limb_obj.transform, front_right_state.position, Color.black, 0.1f);
            front_right_state.addCp();

            //build geo_table
            addVertices(current_cp, front_right_state, front_right_mesh);

            //build uv
            addUvs(front_right_mesh.uvs, current_cp);

            //build triangles
            addTriangles(i, front_right_mesh);

            //update cp_position
            front_right_state.updatePosition(current_cp);
        }

        //debug vertices
        for (int i = 0; i < front_right_mesh.geo_table.Count; i++) {
            Utils.debugSphere(limb_obj.transform, front_right_mesh.geo_table[i], Color.blue, 0.1f);
        }

        //set values into cmesh.mesh
        front_right_mesh.mesh.vertices = front_right_mesh.geo_table.ToArray();
        front_right_mesh.mesh.uv = front_right_mesh.uvs.ToArray();
        front_right_mesh.mesh.triangles = front_right_mesh.triangle_table.ToArray();
        front_right_mesh.mesh.RecalculateNormals();

        // loop subdivision
        LoopSubdivision.setParameters(front_right_mesh);
        LoopSubdivision.subdivide(2);
    }

    //===============================================================
    //===============================================================
    //HELPERS
    //===============================================================
    //===============================================================

    public void addVertices(float current_cp, CpState cp_state, CMesh c_mesh) {
        Vector3 cp_pos = cp_state.position;
        Vector3 current_normal = cp_state.getNormal(current_cp);
        Vector3 next_normal = cp_state.getNextNormal(current_cp);
        Vector3 current_binormal = cp_state.getBinormal(current_cp);
        Vector3 next_binormal = cp_state.getNextBinormal(current_cp);

        float interpolant = cp_state.getInterpolant(current_cp);
        Vector3 normal = Vector3.Lerp(current_normal, next_normal, interpolant);
        Vector3 binormal = Vector3.Lerp(current_binormal, next_binormal, interpolant);

        Vector2 size_offset = cp_state.getSizeOffset(current_cp);
        float width_offset = size_offset.x;
        float height_offset = size_offset.y;

        c_mesh.geo_table.Add(cp_pos + (normal * -width_offset) + (binormal * height_offset)); //left top corner
        c_mesh.geo_table.Add(cp_pos + (normal * width_offset) + (binormal * height_offset)); //right top corner
        c_mesh.geo_table.Add(cp_pos + (normal * width_offset) + (binormal * -height_offset)); //right bottom
        c_mesh.geo_table.Add(cp_pos + (normal * -width_offset) + (binormal * -height_offset)); //left bottom
    }

    public void addUvs(List<Vector2> uvs, float current_cp) {
        uvs.Add(new Vector2(0f, current_cp));
        uvs.Add(new Vector2(0.33f, current_cp));
        uvs.Add(new Vector2(0.66f, current_cp));
        uvs.Add(new Vector2(1f, current_cp));
    }

    public void addTriangles(int i, CMesh c_mesh) {
        //build triangle_table
        //other method could be to create int v0 through vX and just assign them like that, in this case max is v7
        Vector3 v0, v1, v2, v3;

        if (i > 0) {
            int new_base = i * 4;
            int old_base = (i - 1) * 4;
            //top face clockwise
            c_mesh.addTriangle(old_base + 1, old_base, new_base);
            c_mesh.addTriangle(new_base + 1, old_base + 1, new_base);

            //bottom face counterclockwise
            c_mesh.addTriangle(new_base + 3, old_base + 3, old_base + 2);
            c_mesh.addTriangle(new_base + 3, old_base + 2, new_base + 2);

            //right face clockwise
            c_mesh.addTriangle(new_base + 2, old_base + 2, old_base + 1);
            c_mesh.addTriangle(new_base + 1, new_base + 2, old_base + 1);

            //left face counterclockwise
            c_mesh.addTriangle(old_base, old_base + 3, new_base + 3);
            c_mesh.addTriangle(old_base, new_base + 3, new_base);

            if (i == cp_count - 1) {
                c_mesh.addTriangle(i * 4, (i * 4) + 3, (i * 4) + 2);
                c_mesh.addTriangle(i * 4, (i * 4) + 2, (i * 4) + 1);
                //add hard edges at the end of the limb
                // v0 = c_mesh.geo_table[i * 4];
                // v1 = c_mesh.geo_table[(i * 4) + 1];
                // v2 = c_mesh.geo_table[(i * 4) + 2];
                // v3 = c_mesh.geo_table[(i * 4) + 3];
                // c_mesh.hard_edges.Add(new Edge(v0, v1));
                // c_mesh.hard_edges.Add(new Edge(v1, v2));
                // c_mesh.hard_edges.Add(new Edge(v2, v3));
                // c_mesh.hard_edges.Add(new Edge(v3, v0));
            }
        } else if (i == 0) {
            c_mesh.addTriangle(2, 3, 0);
            c_mesh.addTriangle(1, 2, 0);

            //add hard edges
            v0 = c_mesh.geo_table[0];
            v1 = c_mesh.geo_table[1];
            v2 = c_mesh.geo_table[2];
            v3 = c_mesh.geo_table[3];
            c_mesh.hard_edges.Add(new Edge(v0, v1));
            c_mesh.hard_edges.Add(new Edge(v1, v2));
            c_mesh.hard_edges.Add(new Edge(v2, v3));
            c_mesh.hard_edges.Add(new Edge(v3, v0));
        }

    }
    // foreach (CMesh c_mesh in limb_meshes) {
    //     c_mesh.mesh.vertices = c_mesh.geo_table.ToArray();
    //     c_mesh.mesh.uv = c_mesh.uvs.ToArray();
    //     c_mesh.mesh.triangles = c_mesh.triangle_table.ToArray();
    //     c_mesh.mesh.RecalculateNormals();
    // }
}
