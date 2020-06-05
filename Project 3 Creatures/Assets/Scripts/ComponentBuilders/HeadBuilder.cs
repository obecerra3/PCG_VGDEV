using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadBuilder {
    //head components
    GameObject head_obj;
    CMesh head_mesh;
    CMesh mouth_mesh;

    //limb settings
    public int cp_count = 10;
    public CpState head_state;
    public CpState mouth_state;

    public Vector3 init_pos;
    public Vector3 mouth_init_pos;
    public float length;
    public float box_length;
    public float box_height;
    public float box_width;
    public int head_type;

    //===============================================================
    //===============================================================
    //GAME_OBJECTS
    //===============================================================
    //===============================================================

    public GameObject build(TorsoBuilder torso_builder) {
        head_obj = new GameObject();
        head_obj.name = "head";

        initSettings(torso_builder);

        buildMesh();

        MeshFilter mesh_filter = (MeshFilter) head_obj.AddComponent(typeof(MeshFilter));
        MeshRenderer mesh_renderer = (MeshRenderer) head_obj.AddComponent(typeof(MeshRenderer));
        mesh_filter.mesh = head_mesh.mesh;

        //texture setup
        head_obj.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse"));
        Texture2D texture = Utils.makeTexture(head_mesh.mesh.vertices, "head", torso_builder.colors);
        Renderer renderer = head_obj.GetComponent<Renderer>();
        renderer.material.mainTexture = texture;

        GameObject mouth_obj = new GameObject();
        mouth_obj.name = "mouth";
        mesh_filter = (MeshFilter) mouth_obj.AddComponent(typeof(MeshFilter));
        mesh_renderer = (MeshRenderer) mouth_obj.AddComponent(typeof(MeshRenderer));
        mesh_filter.mesh = mouth_mesh.mesh;
        mouth_obj.transform.parent = head_obj.transform;

        //texture setup
        mouth_obj.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse"));
        texture = Utils.makeTexture(mouth_mesh.mesh.vertices, "head", torso_builder.colors);
        renderer = mouth_obj.GetComponent<Renderer>();
        renderer.material.mainTexture = texture;

        float eye_scale = length * 0.08f;
        int eye_cp = 5;
        Vector2 size_offset = head_state.getSizeOffset(0.5f);
        Vector3 eye_pos = head_state.cps[eye_cp] + new Vector3(-size_offset.x * 0.5f, size_offset.y * 1.6f, 0f);
        Vector3 right_offset = new Vector3(size_offset.x, 0f, 0f);
        Utils.drawSphere(head_obj.transform, eye_pos, Color.white, eye_scale);
        Utils.drawSphere(head_obj.transform, eye_pos + right_offset, Color.white, eye_scale);


        return head_obj;
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
        box_width = box_length * 5f;

        init_pos = (0.5f * torso_builder.cps[torso_builder.cps.Count - 1]) + (0.5f * torso_builder.cps[torso_builder.cps.Count - 2]);
        init_pos -= new Vector3(0, torso_builder.box_height * 0.75f, 0);
        mouth_init_pos = init_pos + new Vector3(0f, torso_builder.box_height * 0.25f, 0f);

        head_type = Random.Range(0, 3);

        if (head_type == 0) {
            initDefaultState();
        } else if (head_type == 1) {
            initVar1State();
        } else if (head_type == 2) {
            initVar2State();
        }
    }

    //===============================================================
    //===============================================================
    //INIT HELPERS
    //===============================================================
    //===============================================================

    public void initDefaultState() {
        float base_length = box_length * Random.Range(0.5f, 0.7f);

        List<float> distances = new List<float>() {
            base_length,
            base_length,
            base_length,
            base_length,
            base_length,
            base_length,
            base_length,
            base_length,
            base_length,
        };

        List<float> bend_points = new List<float>();

        float y_offset = Random.Range(0f, 0.5f);

        List<Vector3> directions = new List<Vector3>() {
            new Vector3(0f, y_offset, -1f).normalized
        };

        List<Vector2> size_offsets = new List<Vector2>() {
            new Vector2(box_width * 0.5f, box_height * 0.5f),
            new Vector2(box_width * 0.75f, box_height * 0.75f),
            new Vector2(box_width * 0.7f, box_height * 0.8f),
            new Vector2(box_width * 0.65f, box_height * 0.75f),
            new Vector2(box_width * 0.6f, box_height * 0.6f),
            new Vector2(box_width * 0.55f, box_height * 0.55f),
            new Vector2(box_width * 0.5f, box_height * 0.5f),
            new Vector2(box_width * 0.45f, box_height * 0.45f),
            new Vector2(box_width * 0.4f, box_height * 0.4f),
            new Vector2(box_width * 0.3f, box_height * 0.3f)
        };

        head_state = new CpState(init_pos, distances, bend_points, directions, size_offsets);

        float mouth_base_length = base_length * 0.95f;

        distances = new List<float>() {
            mouth_base_length,
            mouth_base_length,
            mouth_base_length,
            mouth_base_length,
            mouth_base_length,
            mouth_base_length,
            mouth_base_length,
            mouth_base_length,
            mouth_base_length,
        };

        bend_points = new List<float>();

        directions = new List<Vector3>() {
            new Vector3(0f, y_offset - Random.Range(0.1f, 0.2f), -1f).normalized
        };

        float base_width = box_width * 0.75f;
        float base_height = box_height * 0.3f;
        size_offsets = new List<Vector2>() {
            new Vector2(base_width * 0.5f, base_height * 1.1f),
            new Vector2(base_width * 0.75f, base_height * 0.9f),
            new Vector2(base_width * 0.7f, base_height * 0.85f),
            new Vector2(base_width * 0.65f, base_height * 0.8f),
            new Vector2(base_width * 0.6f, base_height * 0.6f),
            new Vector2(base_width * 0.55f, base_height * 0.55f),
            new Vector2(base_width * 0.5f, base_height * 0.5f),
            new Vector2(base_width * 0.45f, base_height * 0.45f),
            new Vector2(base_width * 0.4f, base_height * 0.4f),
            new Vector2(base_width * 0.3f, base_height * 0.3f)
        };

        mouth_state = new CpState(mouth_init_pos, distances, bend_points, directions, size_offsets);
    }

    public void initVar1State() {
        float base_length = box_length * Random.Range(0.7f, 0.9f);

        List<float> distances = new List<float>() {
            base_length,
            base_length,
            base_length,
            base_length,
            base_length,
            base_length,
            base_length,
            base_length,
            base_length,
        };

        List<float> bend_points = new List<float>();

        float y_offset = Random.Range(0f, 0.5f);

        List<Vector3> directions = new List<Vector3>() {
            new Vector3(0f, y_offset, -1f).normalized
        };

        float base_width = box_width;
        float base_height = box_height;
        List<Vector2> size_offsets = new List<Vector2>() {
            new Vector2(base_width * 0.4f, base_height * 0.5f),
            new Vector2(base_width * 0.6f, base_height * 0.5f),
            new Vector2(base_width * 0.6f, base_height * 0.6f),
            new Vector2(base_width * 0.6f, base_height * 0.6f),
            new Vector2(base_width * 0.6f, base_height * 0.6f),
            new Vector2(base_width * 0.6f, base_height * 0.6f),
            new Vector2(base_width * 0.5f, base_height * 0.4f),
            new Vector2(base_width * 0.4f, base_height * 0.35f),
            new Vector2(base_width * 0.3f, base_height * 0.3f),
            new Vector2(base_width * 0.2f, base_height * 0.35f)
        };

        head_state = new CpState(init_pos, distances, bend_points, directions, size_offsets);

        float mouth_base_length = base_length * 0.8f;

        distances = new List<float>() {
            mouth_base_length,
            mouth_base_length,
            mouth_base_length,
            mouth_base_length,
            mouth_base_length,
            mouth_base_length,
            mouth_base_length,
            mouth_base_length,
            mouth_base_length,
        };

        bend_points = new List<float>();

        directions = new List<Vector3>() {
            new Vector3(0f, y_offset - Random.Range(0.1f, 0.2f), -1f).normalized
        };

        base_width = box_width * 0.75f;
        base_height = box_height * 0.3f;
        List<Vector2> mouth_size_offsets = new List<Vector2>() {
            new Vector2(base_width * 0.4f, base_height * 0.5f),
            new Vector2(base_width * 0.6f, base_height * 0.5f),
            new Vector2(base_width * 0.6f, base_height * 0.6f),
            new Vector2(base_width * 0.6f, base_height * 0.6f),
            new Vector2(base_width * 0.6f, base_height * 0.6f),
            new Vector2(base_width * 0.6f, base_height * 0.6f),
            new Vector2(base_width * 0.5f, base_height * 0.4f),
            new Vector2(base_width * 0.4f, base_height * 0.35f),
            new Vector2(base_width * 0.3f, base_height * 0.3f),
            new Vector2(base_width * 0.2f, base_height * 0.35f)
        };

        mouth_state = new CpState(mouth_init_pos, distances, bend_points, directions, mouth_size_offsets);
    }

    public void initVar2State() {
        float base_length = box_length * Random.Range(0.65f, 0.75f);

        List<float> distances = new List<float>() {
            base_length,
            base_length,
            base_length,
            base_length,
            base_length,
            base_length,
            base_length,
            base_length,
            base_length,
        };

        List<float> bend_points = new List<float>();

        float y_offset = Random.Range(0f, 0.5f);

        List<Vector3> directions = new List<Vector3>() {
            new Vector3(0f, y_offset, -1f).normalized
        };

        float base_width = box_width;
        float base_height = box_height;
        List<Vector2> size_offsets = new List<Vector2>() {
            new Vector2(base_width * 0.4f, base_height * 0.4f),
            new Vector2(base_width * 0.5f, base_height * 0.5f),
            new Vector2(base_width * 0.6f, base_height * 0.6f),
            new Vector2(base_width * 0.6f, base_height * 0.6f),
            new Vector2(base_width * 0.6f, base_height * 0.6f),
            new Vector2(base_width * 0.6f, base_height * 0.6f),
            new Vector2(base_width * 0.5f, base_height * 0.5f),
            new Vector2(base_width * 0.4f, base_height * 0.4f),
            new Vector2(base_width * 0.3f, base_height * 0.3f),
            new Vector2(base_width * 0.2f, base_height * 0.2f)
        };

        head_state = new CpState(init_pos, distances, bend_points, directions, size_offsets);

        float mouth_base_length = base_length * 0.8f;

        distances = new List<float>() {
            mouth_base_length,
            mouth_base_length,
            mouth_base_length,
            mouth_base_length,
            mouth_base_length,
            mouth_base_length,
            mouth_base_length,
            mouth_base_length,
            mouth_base_length,
        };

        bend_points = new List<float>();

        directions = new List<Vector3>() {
            new Vector3(0f, y_offset - Random.Range(0.1f, 0.2f), -1f).normalized
        };

        base_width = box_width * 0.75f;
        base_height = box_height * 0.3f;
        List<Vector2> mouth_size_offsets = new List<Vector2>() {
            new Vector2(base_width * 0.4f, base_height * 0.4f),
            new Vector2(base_width * 0.5f, base_height * 0.5f),
            new Vector2(base_width * 0.6f, base_height * 0.6f),
            new Vector2(base_width * 0.6f, base_height * 0.6f),
            new Vector2(base_width * 0.6f, base_height * 0.6f),
            new Vector2(base_width * 0.6f, base_height * 0.6f),
            new Vector2(base_width * 0.5f, base_height * 0.5f),
            new Vector2(base_width * 0.4f, base_height * 0.4f),
            new Vector2(base_width * 0.3f, base_height * 0.3f),
            new Vector2(base_width * 0.2f, base_height * 0.2f)
        };

        mouth_state = new CpState(mouth_init_pos, distances, bend_points, directions, mouth_size_offsets);
    }


    //===============================================================
    //===============================================================
    //MESH
    //===============================================================
    //===============================================================

    public void buildMesh() {
        head_mesh = new CMesh();
        mouth_mesh = new CMesh();

        float current_cp;

        for (int i = 0; i < cp_count; i++) {
            current_cp = i / (float) cp_count; //[0, 1]

            //build control points
            Utils.debugSphere(head_obj.transform, head_state.position, Color.black, 0.1f);
            head_state.addCp();
            mouth_state.addCp();

            //build geo_table
            addVertices(current_cp, head_state, head_mesh, true);
            addVertices(current_cp, mouth_state, mouth_mesh, false);

            //build uv
            addUvs(head_mesh.uvs, current_cp);
            addUvs(mouth_mesh.uvs, current_cp);

            //build triangles
            addTriangles(i, head_mesh);
            addTriangles(i, mouth_mesh);

            //update cp_position
            head_state.updatePosition(current_cp);
            mouth_state.updatePosition(current_cp);
        }

        //set values into cmesh.mesh
        head_mesh.mesh.vertices = head_mesh.geo_table.ToArray();
        head_mesh.mesh.uv = head_mesh.uvs.ToArray();
        head_mesh.mesh.triangles = head_mesh.triangle_table.ToArray();
        head_mesh.mesh.RecalculateNormals();

        mouth_mesh.mesh.vertices = mouth_mesh.geo_table.ToArray();
        mouth_mesh.mesh.uv = mouth_mesh.uvs.ToArray();
        mouth_mesh.mesh.triangles = mouth_mesh.triangle_table.ToArray();
        mouth_mesh.mesh.RecalculateNormals();

        // loop subdivision
        LoopSubdivision.setParameters(head_mesh);
        LoopSubdivision.subdivide(2);

        LoopSubdivision.setParameters(mouth_mesh);
        LoopSubdivision.subdivide(2);
    }

    //===============================================================
    //===============================================================
    //HELPERS
    //===============================================================
    //===============================================================

    public void addVertices(float current_cp, CpState cp_state, CMesh c_mesh, bool is_head) {
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

        if (is_head) {
            c_mesh.geo_table.Add(cp_pos + (normal * -width_offset) + (binormal * height_offset * 2f)); //left top corner
            c_mesh.geo_table.Add(cp_pos + (normal * width_offset) + (binormal * height_offset * 2f)); //right top corner
            c_mesh.geo_table.Add(cp_pos + (normal * width_offset)); //right bottom
            c_mesh.geo_table.Add(cp_pos + (normal * -width_offset)); //left bottom
        } else {
            c_mesh.geo_table.Add(cp_pos + (normal * -width_offset)); //left top corner
            c_mesh.geo_table.Add(cp_pos + (normal * width_offset)); //right top corner
            c_mesh.geo_table.Add(cp_pos + (normal * width_offset) - (binormal * height_offset * 2f)); //right bottom
            c_mesh.geo_table.Add(cp_pos + (normal * -width_offset) - (binormal * height_offset * 2f)); //left bottom
        }
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

}
