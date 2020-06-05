using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdMeshBuilder
{
    public Mesh bird_mesh;
    public float box_length;
    public float box_width;
    public float box_height;
    public float bird_radius;

    public BirdMeshBuilder()
    {
        float box_constant = 0.15f;
        box_length = box_constant;
        box_width = box_constant;
        box_height = box_constant;

        CpState body_state = initBodyState();
        CpState wing_state = initWingState();

        CMesh body_mesh = buildMesh(body_state);
        CMesh wing_mesh = buildMesh(wing_state);

        bird_radius = wing_state.length * 0.5f;

        //combine the meshes into one
        CombineInstance[] combine = new CombineInstance[2];
        combine[0].mesh = body_mesh.mesh;
        combine[1].mesh = wing_mesh.mesh;
        bird_mesh = new Mesh();
        bird_mesh.CombineMeshes(combine, true, false, false);
    }

    public CpState initBodyState()
    {
        Vector3 init_pos = Vector3.zero;

        List<float> distances = new List<float>()
        {
            box_length * 0.5f, //beak
            box_length * 0.5f, //head
            box_length * 0.65f, //neck
            box_length * 1.2f, //torso
            box_length * 1.7f, //torso
            box_length * 1.5f, //torso
            box_length * 2.0f, //tail
            box_length * 0.5f, //tail
            box_length * 0.2f, // tail
            0.0f,
        };

        List<float> bend_points = new List<float>()
        {
            0.0f,
            0.1f,
            0.3f,
            0.5f,
            0.6f,
            1.0f,
        };

        List<Vector3> directions = new List<Vector3>()
        {
            new Vector3(0f, 0.3f, 0.8f),
            new Vector3(0f, 0.8f, 0.65f),
            new Vector3(0f, 0.1f, 0.8f),
            new Vector3(0f, -0.1f, 0.8f),
            new Vector3(0f, 0.07f, 0.8f),
            new Vector3(0f, -0.3f, 0.9f),
        };

        List<Vector2> size_offsets = new List<Vector2>()
        {
            new Vector2(box_width * 0.01f, box_height * 0.01f),//beak
            new Vector2(box_width * 0.1f, box_height * 0.11f), //beak
            new Vector2(box_width * 0.4f, box_height * 0.35f), //head
            new Vector2(box_width * 0.55f, box_height * 0.37f), //neck
            new Vector2(box_width * 0.45f, box_height * 0.68f), //torso
            new Vector2(box_width * 0.6f, box_height * 0.53f), //torso
            new Vector2(box_width * 0.2f, box_height * 0.2f), //torso
            new Vector2(box_width * 0.5f, box_height * 0.3f), //tail
            new Vector2(box_width * 0.48f, box_height * 0.2f), //tail
            new Vector2(box_width * 0.01f, box_height * 0.01f), //tail
        };

        List<float[]> orientations = new List<float[]>
        {
            new float[] {1, 1, 1, 1}, //beak
            new float[] {1, 1, 0, 2}, //beak
            new float[] {1, 1, 0, 2}, //head
            new float[] {1, 1, 0, 2}, //neck
            new float[] {1, 1, 0, 2}, //torso
            new float[] {1, 1, 0, 2}, //torso
            new float[] {1, 1, 0, 2}, //torso
            new float[] {1, 1, 0, 2}, //tail
            new float[] {1, 1, 0, 2}, //tail
            new float[] {1, 1, 1, 1}, //tail
        };

        return new CpState(10, init_pos, distances, bend_points, directions, size_offsets, orientations);
    }

    public CpState initWingState()
    {
        Vector3 init_pos = Vector3.zero;

        List<float> distances = new List<float>()
        {
            box_length,
            box_length,
            box_length,
            box_length,
            box_length,//repeat from here
            box_length,
            box_length,
            box_length,
            box_length,
            0.0f,
        };

        List<float> bend_points = new List<float>()
        {

        };

        List<Vector3> directions = new List<Vector3>()
        {
            new Vector3(-1f, 0f, 0f)
        };

        List<Vector2> size_offsets = new List<Vector2>()
        {
            new Vector2(box_width * 0.1f, box_height * 0.01f),
            new Vector2(box_width * 0.5f, box_height * 0.1f),
            new Vector2(box_width * 0.9f, box_height * 0.1f),
            new Vector2(box_width * 0.8f, box_height * 0.1f),
            new Vector2(box_width, box_height * 0.1f),//repeat from here
            new Vector2(box_width, box_height * 0.1f),
            new Vector2(box_width * 0.8f, box_height * 0.1f),
            new Vector2(box_width * 0.9f, box_height * 0.1f),
            new Vector2(box_width * 0.5f, box_height * 0.1f),
            new Vector2(box_width * 0.1f, box_height * 0.01f),
        };

        List<float[]> orientations = new List<float[]>
        {
            new float[] {0, 2, 0, 2},
            new float[] {0, 2, 0, 2},
            new float[] {0, 2, 0, 2},
            new float[] {0, 2, 0, 2},
            new float[] {0, 2, 0, 2},
            new float[] {0, 2, 0, 2},
            new float[] {0, 2, 0, 2},
            new float[] {0, 2, 0, 2},
            new float[] {0, 2, 0, 2},
            new float[] {0, 2, 0, 2},
        };

        CpState wing_state = new CpState(10, init_pos, distances, bend_points, directions, size_offsets, orientations);

        // init_pos = new Vector3(1.13f, 0.18f, 0.47f);
        init_pos = new Vector3(4.52f * box_length, 0.68f * box_length, 1.88f * box_length);

        wing_state.position = init_pos;

        return wing_state;
    }


    public CMesh buildMesh(CpState current_state)
    {
        CMesh current_mesh = new CMesh();

        float current_cp;

        int cp_count = current_state.cp_count;

        for (int i = 0; i < cp_count; i++)
        {
            current_cp = i / (float) cp_count; //[0, 1]

            //build control points
            // Utils.debugSphere(current_state.position, Color.green, 0.0125f);
            current_state.addCp();

            //build geo_table
            addVertices(current_cp, current_state, current_mesh);

            //build uv
            addUvs(current_mesh.uvs, current_cp);

            //build triangles
            addTriangles(i, current_mesh, cp_count);

            //update cp_position
            current_state.updatePosition(current_cp);
        }

        current_mesh.addToMesh();

        // loop subdivision
        LoopSubdivision.setParameters(current_mesh);
        LoopSubdivision.subdivide(1);

        return current_mesh;
    }

    //===============================================================
    //===============================================================
    //HELPERS
    //===============================================================
    //===============================================================

    public void addVertices(float current_cp, CpState cp_state, CMesh c_mesh)
    {
        Vector3 cp_pos = cp_state.position;
        Vector3 normal = cp_state.getNormal(current_cp);
        Vector3 binormal = cp_state.getBinormal(current_cp);

        Vector2 size_offset = cp_state.getSizeOffset(current_cp);
        float width_offset = size_offset.x;
        float height_offset = size_offset.y;

        float[] orientation = cp_state.getOrientation(current_cp);
        float width_type_left = orientation[0];
        float width_type_right = orientation[1];
        float height_type_top = orientation[2];
        float height_type_bottom = orientation[3];

        Vector3 v1 = cp_pos + (normal * -width_offset * width_type_left) + (binormal * height_offset * height_type_top); //top left corner
        Vector3 v2 = cp_pos + (normal * width_offset * width_type_right) + (binormal * height_offset * height_type_top); //top right corner
        Vector3 v3 = cp_pos + (normal * width_offset * width_type_right) + (binormal * -height_offset * height_type_bottom); //bottom right
        Vector3 v4 = cp_pos + (normal * -width_offset * width_type_left) + (binormal * -height_offset * height_type_bottom); //bottom left
        c_mesh.geo_table.Add(v1);
        c_mesh.geo_table.Add(v2);
        c_mesh.geo_table.Add(v3);
        c_mesh.geo_table.Add(v4);
    }

    public void addUvs(List<Vector2> uvs, float current_cp)
    {
        uvs.Add(new Vector2(0f, current_cp));
        uvs.Add(new Vector2(0.33f, current_cp));
        uvs.Add(new Vector2(0.66f, current_cp));
        uvs.Add(new Vector2(1f, current_cp));
    }

    public void addTriangles(int i, CMesh c_mesh, int cp_count)
    {
        //build triangle_table
        //other method could be to create int v0 through vX and just assign them like that, in this case max is v7
        // Vector3 v0, v1, v2, v3;

        if (i > 0)
        {
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

            //add hard edges
            // v0 = c_mesh.geo_table[old_base];
            // v1 = c_mesh.geo_table[new_base];
            // c_mesh.hard_edges.Add(new Edge(v0, v1));
            // v0 = c_mesh.geo_table[old_base + 1];
            // v1 = c_mesh.geo_table[new_base + 1];
            // c_mesh.hard_edges.Add(new Edge(v0, v1));
            // v0 = c_mesh.geo_table[old_base + 2];
            // v1 = c_mesh.geo_table[new_base + 2];
            // c_mesh.hard_edges.Add(new Edge(v0, v1));
            // v0 = c_mesh.geo_table[old_base + 3];
            // v1 = c_mesh.geo_table[new_base + 3];
            // c_mesh.hard_edges.Add(new Edge(v0, v1));

            if (i == cp_count - 1)
            {
                c_mesh.addTriangle(i * 4, (i * 4) + 3, (i * 4) + 2);
                c_mesh.addTriangle(i * 4, (i * 4) + 2, (i * 4) + 1);
                //add hard edges at the end of the limb
            }
        } else if (i == 0)
        {
            c_mesh.addTriangle(2, 3, 0);
            c_mesh.addTriangle(1, 2, 0);

            //add hard edges
            // v0 = c_mesh.geo_table[0];
            // v1 = c_mesh.geo_table[1];
            // v2 = c_mesh.geo_table[2];
            // v3 = c_mesh.geo_table[3];
            // c_mesh.hard_edges.Add(new Edge(v0, v1));
            // c_mesh.hard_edges.Add(new Edge(v1, v2));
            // c_mesh.hard_edges.Add(new Edge(v2, v3));
            // c_mesh.hard_edges.Add(new Edge(v3, v0));
        }

    }
}
