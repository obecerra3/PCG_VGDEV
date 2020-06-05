using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClawBuilder {

        //claw components
        GameObject claws_obj;
        List<CMesh> claw_meshes;
        GameObject front_right_obj, front_left_obj, back_right_obj, back_left_obj;

        //claw settings
        public int cp_count = 5;
        public int claw_count = 5;
        List<CpState> claw_states;
        public float length;
        public int claw_type;
        public float box_length;
        public float box_height;
        public float box_width;
        public Vector3 init_pos;

        //===============================================================
        //===============================================================
        //GAME_OBJECTS
        //===============================================================
        //===============================================================

        public GameObject build(LimbBuilder limb_builder, Transform _parent_transform, TorsoBuilder torso_builder) {
            claws_obj = new GameObject();
            claws_obj.name = "claws";
            claws_obj.transform.parent = _parent_transform;

            initSettings(limb_builder);

            claw_meshes = new List<CMesh>();
            foreach (CpState claw_state in claw_states) {
                claw_meshes.Add(buildMesh(claw_state));
            }

            //==========
            //front right
            //==========
            front_right_obj = new GameObject();
            front_right_obj.name = "front_right_claws";
            front_right_obj.transform.parent = claws_obj.transform;
            GameObject claw_obj;

            for (int i = 0; i < claw_count; i++) {
                claw_obj = new GameObject();
                claw_obj.name = "claw of " + i;
                MeshFilter mesh_filter = (MeshFilter) claw_obj.AddComponent(typeof(MeshFilter));
                MeshRenderer mesh_renderer = (MeshRenderer) claw_obj.AddComponent(typeof(MeshRenderer));
                mesh_filter.mesh = claw_meshes[i].mesh;
                claw_obj.transform.parent = front_right_obj.transform;

                claw_obj.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse"));
                Texture2D texture = Utils.makeTexture(claw_meshes[i].mesh.vertices, "claws", torso_builder.colors);
                Renderer renderer = claw_obj.GetComponent<Renderer>();
                renderer.material.mainTexture = texture;
            }

            return claws_obj;
        }

        //===============================================================
        //===============================================================
        //INIT
        //===============================================================
        //===============================================================

        public void initSettings(LimbBuilder limb_builder) {
            length = limb_builder.length / 5f;

            box_length = length / (float) cp_count;
            box_height = box_length;
            box_width = box_length;

            claw_type = Random.Range(0, 4);

            List<Vector3> cps = limb_builder.front_right_state.cps;
            int end_index = cps.Count - 1;
            init_pos = (0.5f * cps[end_index]) + (0.5f * cps[end_index - 1]);

            if (claw_type == 0) {
                initGeckoState();
            } else if (claw_type == 1) {
                initIguanaState();
            } else if (claw_type == 2) {
                initKomodoState();
            } else {
                initDefaultState();
            }

        }

        //===============================================================
        //===============================================================
        //INIT HELPERS
        //===============================================================
        //===============================================================

        public void initGeckoState() {
            claw_states = new List<CpState>();

            List<Vector3> claw_directions = new List<Vector3>() {
                new Vector3(0f, 0f, -1f).normalized,
                new Vector3(-0.5f, 0f, -0.5f).normalized,
                new Vector3(0.5f, 0f, -0.5f).normalized,
                new Vector3(-1f, 0f, 0f).normalized,
                new Vector3(1f, 0f, 0f).normalized,
            };

            for (int i = 0; i < claw_count; i++) {

                List<float> distances = new List<float>() {
                    box_length,
                    box_length,
                    box_length,
                    box_length * 2f,
                };

                List<float> bend_points = new List<float>();

                List<Vector3> directions = new List<Vector3>() {
                    claw_directions[i],
                };

                List<Vector2> size_offsets = new List<Vector2>() {
                    new Vector2(box_width * 0.15f, box_height * 0.15f),
                    new Vector2(box_width * 0.25f, box_height * 0.25f),
                    new Vector2(box_width * 0.7f, box_height * 0.1f),
                    new Vector2(box_width * 0.7f, box_height * 0.1f),
                    new Vector2(box_width * 0.1f, box_height * 0.25f),
                };

                claw_states.Add(new CpState(init_pos, distances, bend_points, directions, size_offsets));
            }
        }

        public void initIguanaState() {
            claw_states = new List<CpState>();

            List<Vector3> claw_directions = new List<Vector3>() {
                new Vector3(0f, 0f, -1f).normalized,
                new Vector3(-0.2f, 0f, -0.5f).normalized,
                new Vector3(0.2f, 0f, -0.5f).normalized,
                new Vector3(-0.5f, 0f, -0.5f).normalized,
                new Vector3(0.4f, 0f, -0.5f).normalized,
            };

            List<float> base_length = new List<float>() {
                box_length * 2f,
                box_length * 1.75f,
                box_length * 1.85f,
                box_length * 1.2f,
                box_length,
            };

            for (int i = 0; i < claw_count; i++) {

                List<float> distances = new List<float>() {
                    base_length[i],
                    base_length[i],
                    base_length[i],
                    base_length[i],
                };

                List<float> bend_points = new List<float>() {
                    0.2f,
                    0.4f,
                };

                List<Vector3> directions = new List<Vector3>() {
                    claw_directions[i],
                    Vector3.Normalize(claw_directions[i] + new Vector3(Random.Range(-0.25f, 0.25f), 0f, 0f)),
                    Vector3.Normalize(claw_directions[i] + new Vector3(Random.Range(-0.25f, 0.25f), 0f, 0f)),
                };

                List<Vector2> size_offsets = new List<Vector2>() {
                    new Vector2(box_width * 0.3f, box_height * 0.3f),
                    new Vector2(box_width * 0.3f, box_height * 0.3f),
                    new Vector2(box_width * 0.2f, box_height * 0.2f),
                    new Vector2(box_width * 0.2f, box_height * 0.2f),
                    new Vector2(box_width * 0.05f, box_height * 0.05f),
                };

                claw_states.Add(new CpState(init_pos, distances, bend_points, directions, size_offsets));
            }
        }

        public void initKomodoState() {
            claw_states = new List<CpState>();

            List<Vector3> claw_directions = new List<Vector3>() {
                new Vector3(0f, 0f, -1f).normalized,
                new Vector3(-0.2f, 0f, -0.5f).normalized,
                new Vector3(0.2f, 0f, -0.5f).normalized,
                new Vector3(-1f, 0f, -0.5f).normalized,
                new Vector3(0.4f, 0f, -0.5f).normalized,
            };

            List<float> base_length = new List<float>() {
                box_length * 1f,
                box_length * 1f,
                box_length * 1f,
                box_length * 1f,
                box_length * 1f
            };

            for (int i = 0; i < claw_count; i++) {

                List<float> distances = new List<float>() {
                    base_length[i],
                    base_length[i],
                    base_length[i],
                    base_length[i],
                };

                List<float> bend_points = new List<float>() {
                    0.2f,
                    0.4f,
                };

                List<Vector3> directions = new List<Vector3>() {
                    claw_directions[i],
                    Vector3.Normalize(claw_directions[i] + new Vector3(Random.Range(-0.1f, 0.1f), 1f, 0f)),
                    Vector3.Normalize(claw_directions[i] + new Vector3(Random.Range(-0.1f, 0.1f), -1f, 0f)),
                };

                List<Vector2> size_offsets = new List<Vector2>() {
                    new Vector2(box_width * 0.3f, box_height * 0.3f),
                    new Vector2(box_width * 0.3f, box_height * 0.3f),
                    new Vector2(box_width * 0.2f, box_height * 0.2f),
                    new Vector2(box_width * 0.2f, box_height * 0.2f),
                    new Vector2(box_width * 0.05f, box_height * 0.05f),
                };

                claw_states.Add(new CpState(init_pos, distances, bend_points, directions, size_offsets));
            }
        }

        public void initDefaultState() {
            claw_states = new List<CpState>();

            List<Vector3> claw_directions = new List<Vector3>() {
                new Vector3(0f, 0f, -1f).normalized,
                new Vector3(-0.5f, 0f, -0.5f).normalized,
                new Vector3(0.5f, 0f, -0.5f).normalized,
                new Vector3(-1f, 0f, 0f).normalized,
                new Vector3(1f, 0f, 0f).normalized,
            };

            for (int i = 0; i < claw_count; i++) {

                List<float> distances = new List<float>() {
                    box_length,
                    box_length,
                    box_length,
                    box_length,
                };

                List<float> bend_points = new List<float>();

                List<Vector3> directions = new List<Vector3>() {
                    claw_directions[i],
                };

                List<Vector2> size_offsets = new List<Vector2>() {
                    new Vector2(box_width * 0.25f, box_height * 0.75f),
                    new Vector2(box_width * 0.4f, box_height * 0.25f),
                    new Vector2(box_width * 0.3f, box_height * 0.1f),
                    new Vector2(box_width * 0.4f, box_height * 0.1f),
                    new Vector2(box_width * 0.5f, box_height * 0.25f),
                };

                claw_states.Add(new CpState(init_pos, distances, bend_points, directions, size_offsets));
            }
        }

        //===============================================================
        //===============================================================
        //MESH
        //===============================================================
        //===============================================================

        public CMesh buildMesh(CpState claw_state) {
            CMesh claw_mesh = new CMesh();

            float current_cp;

            for (int i = 0; i < cp_count; i++) {
                current_cp = i / (float) cp_count; //[0, 1]

                //build control points
                Utils.debugSphere(claws_obj.transform, claw_state.position, Color.black, 0.1f);
                claw_state.addCp();

                //build geo_table
                addVertices(current_cp, claw_state, claw_mesh);

                //build uv
                addUvs(claw_mesh.uvs, current_cp);

                //build triangles
                addTriangles(i, claw_mesh);

                //update cp_position
                claw_state.updatePosition(current_cp);
            }

            //debug vertices
            for (int i = 0; i < claw_mesh.geo_table.Count; i++) {
                Utils.debugSphere(claws_obj.transform, claw_mesh.geo_table[i], Color.blue, 0.1f);
            }

            //set values into cmesh.mesh
            claw_mesh.mesh.vertices = claw_mesh.geo_table.ToArray();
            claw_mesh.mesh.uv = claw_mesh.uvs.ToArray();
            claw_mesh.mesh.triangles = claw_mesh.triangle_table.ToArray();
            claw_mesh.mesh.RecalculateNormals();

            // loop subdivision
            LoopSubdivision.setParameters(claw_mesh);
            LoopSubdivision.subdivide(2);
            return claw_mesh;
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
