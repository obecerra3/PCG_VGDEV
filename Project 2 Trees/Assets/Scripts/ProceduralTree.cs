using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralTree : MonoBehaviour {
    public int tree_seed;

    public List<Branch> branches;

    public float plagiotropic;
    public float orthotropic;

    public float internode_radius;
    public float internode_length;

    public int internodes_per_growth;

    public string[] ramification_types = { "continuous", "rhythmic", "diffuse", "palm" };
    public string ramification_type;

    public string[] phyllotaxy_types = { "distic", "spiral" };
    public string phyllotaxy_type;

    public int max_order;
    public int min_buds_per_node;

    public float pause_prob;
    public float death_prob;
    public float ramification_prob;

    public float min_dimension_1;
    public float min_dimension;

    int growth_cycles;

    public float conical;

    public GameObject parent_object;

    public float wiggle;

    public Mesh leaf_mesh;
    public Texture2D leaf_texture;

    Color[] color_sets = {
        new Color(139f/255f, 69/255f, 19/255f, 1f), //saddle brown
        new Color(188/255f, 143/255f, 143/255f, 1f), //rosy brown
        new Color(222/255f, 184/255f, 135/255f, 1f), //burly wood
        new Color(205/255f, 133/255f, 63/255f, 1f), //peru
        new Color(210/255f, 105/255f, 30/255f, 1f), //chocolate
        new Color(160/255f, 82/255f, 45/255f, 1f), //siena
        new Color(0.36f, 0.29f, 0.23f, 1f), //dark brown
        new Color(0.36f*0.75f, 0.29f*0.75f, 0.23f*0.75f, 1f), //darker brown
        new Color(0.36f*0.5f, 0.29f*0.5f, 0.23f*0.5f, 1f) //darkest brown
    };
    public Color current_color;
    public int index;
    // public int[] current_colors;

    public void setParameters(int seed, GameObject new_parent_object, Mesh new_leaf_mesh, Texture2D new_leaf_texture) {
        tree_seed = seed;
        Random.InitState(tree_seed);

        plagiotropic = Random.Range(0.4f, 1.2f);
        if (Random.value > 0.5f) {
            orthotropic = Random.Range(-1f, -0.1f);
        } else {
            orthotropic =  Random.Range(0.1f, 1f);
        }

        internode_radius = Random.Range(0.05f, 0.15f);
        internode_length = Random.Range(0.5f, 1.5f);

        internodes_per_growth = Random.Range(1, 5);

        ramification_type = ramification_types[Random.Range(0, 3)];
        phyllotaxy_type = phyllotaxy_types[Random.Range(0, 1)];

        max_order = Random.Range(6, 9);
        min_buds_per_node = Random.Range(1, 2);

        pause_prob = Random.Range(0.1f, 0.2f);
        death_prob = Random.Range(0.05f, 0.2f);
        ramification_prob = Random.Range(0.2f, 0.3f);

        growth_cycles = Random.Range(3, 5);

        min_dimension_1 = Random.Range(1, 9);
        min_dimension = Random.Range(2, 5);

        conical = Random.Range(0.05f, 0.1f);

        //Instantiate first branch
        Branch initial_branch = new Branch(this, new Vector3(0, 0, 0), new Vector3(0.1f * plagiotropic, 1, 0.1f * plagiotropic).normalized, 1);
        initial_branch.buds[0].age = 1;
        branches = new List<Branch>();
        branches.Add(initial_branch);

        parent_object = new_parent_object;
        index = Random.Range(0, color_sets.Length);
        current_color = color_sets[index];

        wiggle = Random.Range(0.25f, 0.75f);
        leaf_mesh = new_leaf_mesh;
        leaf_texture = new_leaf_texture;
        // current_colors = new int[Random.Range(3, 7)];
        // for (int i = 0; i < current_colors.Length; i++) {
        //     current_colors[i] = Random.Range(0, color_sets.Length - 1);
        // }
    }

    public void createBranches() {
        //build branches data structure
        List<Branch> new_branches;
        growth_cycles = 6;
        for (int g = 0; g < growth_cycles; g++) {
            new_branches = new List<Branch>();
            for (int b = 0; b < branches.Count; b++) {
                branches[b].grow(new_branches);
            }
            if (new_branches.Count > 0) {
                branches.AddRange(new_branches);
            }
        }

        List<Branch> empty_branches = new List<Branch>();
        for (int b = 0; b < branches.Count; b++) {
            if (branches[b].buds.Count == 1) {
                empty_branches.Add(branches[b]);
            }
        }
        for (int b = 0; b < empty_branches.Count; b++) {
            branches.Remove(empty_branches[b]);
        }
    }

    public void renderBranches() {
        //render branches by creating new GameObjects with a mesh and texture
        Branch branch;
        GameObject branch_object;
        for (int b = 0; b < branches.Count; b++) {
            branch = branches[b];

            branch_object = new GameObject();

            MeshFilter mesh_filter = (MeshFilter) branch_object.AddComponent<MeshFilter>();
            MeshRenderer mesh_renderer = (MeshRenderer) branch_object.AddComponent<MeshRenderer>();
            mesh_renderer.material = new Material(Shader.Find("Diffuse"));
            Renderer renderer = branch_object.GetComponent<Renderer>();

            Mesh branch_mesh = createBranchMesh(branch);
            mesh_filter.mesh = branch_mesh;

            Texture2D branch_texture = createBranchTexture(branch, branch_mesh);
            renderer.material.mainTexture = branch_texture;

            branch_object.name = "Branch of Seed: " + tree_seed + ", Order: " + branch.order + ", Buds: " + branch.buds.Count;
            branch_object.transform.parent = parent_object.transform;
        }
    }

    public Mesh createBranchMesh(Branch branch) {
        //create curved mesh
        // debugDrawBuds(branch);

        Mesh branch_mesh = new Mesh();

        Bud current_bud;
        Bud next_bud;
        Vector3 point_on_internode;

        int column = 0;
        int row = 0;

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uv = new List<Vector2>();
        List<int> triangles = new List<int>();

        int vertex_index = 0;

        float theta = 0;
        float radius = 0;
        float y_step_size = 0.25f;
        int circle_step_size = 16;

        Vector3 apical_bud_pos = branch.buds[branch.buds.Count - 1].position;

        for (int b = 0; b < branch.buds.Count - 1; b++) {
            current_bud = branch.buds[b];
            next_bud = branch.buds[b + 1];
            if (next_bud.apical) {
                y_step_size *= 0.5f;
            }
            for (float t = 0; t < 1; t += y_step_size) {
                point_on_internode = degree1Bezier(t, current_bud.position, next_bud.position);
                // debugInternodeSphere(point_on_internode);
                column = 0;
                radius = radiusPerGrowth(Vector3.Distance(apical_bud_pos, point_on_internode));
                for (theta = 0; theta < 2f*Mathf.PI; theta += 2f*Mathf.PI / circle_step_size) {
                    vertices.Add(point_on_internode + getVertexOffset(theta, current_bud.tangent, radius, branch.order));
                    uv.Add(new Vector2(column / (circle_step_size + 1), row));
                    vertex_index++;
                    column++;
                }
                row++;
            }

            //leaf spawn
            if (branch.order >= 2 && !current_bud.axillary && !current_bud.apical && !current_bud.initial) {
                GameObject leaf_object = new GameObject();
                MeshFilter mesh_filter = (MeshFilter) leaf_object.AddComponent<MeshFilter>();
                MeshRenderer mesh_renderer = (MeshRenderer) leaf_object.AddComponent<MeshRenderer>();
                mesh_renderer.material = new Material(Shader.Find("Diffuse"));
                leaf_object.transform.position = current_bud.position + leafOffset(current_bud, radius);
                leaf_object.transform.parent = parent_object.transform;
                Renderer renderer = leaf_object.GetComponent<Renderer>();
                mesh_filter.mesh = leaf_mesh;
                renderer.material.mainTexture = leaf_texture;
            }
        }

        vertices.Add(apical_bud_pos);
        uv.Add(new Vector2(1.0f, 1.0f));

        //divide each value in row by row max - 1
        for (int i = 0; i < uv.Count; i++) {
            //draw vertices
            // debugInternodeSphere(vertices[i], Color.blue);
            uv[i] = new Vector2(uv[i].x, uv[i].y / (row - 1));
        }

        for (int v = 0; v < vertices.Count - 1; v++) {
            //for drawing the very tip of the branch
            if (v >= vertices.Count - circle_step_size - 1) {
                if ((v + 1) % circle_step_size == 0) {
                    triangles.Add(v);
                    triangles.Add(v - circle_step_size + 1);
                    triangles.Add(vertices.Count - 1);
                } else {
                    triangles.Add(v);
                    triangles.Add(v + 1);
                    triangles.Add(vertices.Count - 1);
                }
            } else if ((v + 1) % circle_step_size == 0) {
                //for drawing last vertice in ring of internode
                triangles.Add(v);
                triangles.Add(v - circle_step_size + 1);
                triangles.Add(v + circle_step_size);
                triangles.Add(v - circle_step_size + 1);
                triangles.Add(v + 1);
                triangles.Add(v + circle_step_size);
            } else {
                //for drawing ring of internode
                triangles.Add(v);
                triangles.Add(v + 1);
                triangles.Add(v + circle_step_size);
                triangles.Add(v + 1);
                triangles.Add(v + 1 + circle_step_size);
                triangles.Add(v + circle_step_size);
            }
        }

        branch_mesh.vertices = vertices.ToArray();
        branch_mesh.triangles = triangles.ToArray();
        branch_mesh.uv = uv.ToArray();
        branch_mesh.RecalculateNormals();

        return branch_mesh;
    }

    public Vector3 leafOffset(Bud bud, float radius) {
        float theta = Random.Range(0f, 2f*Mathf.PI);
        Vector3 tangent = bud.tangent;
        Vector3 normal = Vector3.Cross(tangent, Vector3.right).normalized;
        Vector3 binormal = Vector3.Cross(tangent, normal).normalized;
        Vector3 offset = ((normal * Mathf.Cos(theta)) + (binormal * Mathf.Sin(theta))) * radius;
        return offset;
    }

    public Vector3 getVertexOffset(float theta, Vector3 tangent, float radius, int order) {
        Vector3 normal;

        if (tangent.Equals(Vector3.up) || tangent.Equals(Vector3.down)) {
            normal = Vector3.Cross(tangent, Vector3.right).normalized;
        } else {
            normal = Vector3.Cross(tangent, Vector3.down).normalized;
        }

        Vector3 binormal = Vector3.Cross(tangent, normal).normalized;

        return ((normal * Mathf.Cos(theta)) + (binormal * Mathf.Sin(theta))) * radius;

    }

    public float radiusPerGrowth(float distance_to_apical) {
        float radius = internode_radius ;
        radius *= conical * distance_to_apical; //(0.1f * distance_to_apical);
        radius *= 2f;
        return radius;
    }

    public Vector3 degree1Bezier(float t, Vector3 point1, Vector3 point2) {
        return ((1 - t) * point1) + (t * point2);
    }

    public void debugDrawBuds(Branch branch) {
        GameObject sphere;
        Bud bud;
        for (int b = 0; b < branch.buds.Count; b++) {
            if (branch.order == 1) {
                bud = branch.buds[b];
                sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.localScale *= 0.5f;
                sphere.transform.position = bud.position;
                sphere.transform.parent = parent_object.transform;
                Renderer renderer = sphere.GetComponent<Renderer>();
                renderer.material.color = Color.green;
            }
        }
    }

    public void debugInternodeSphere(Vector3 position, Color new_color) {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.localScale *= 0.01f;
        sphere.transform.position = position;
        sphere.transform.parent = parent_object.transform;
        Renderer renderer = sphere.GetComponent<Renderer>();
        renderer.material.color = new_color;
    }

    public Texture2D createBranchTexture(Branch branch, Mesh branch_mesh) {
        //color the texture of each branch
        Vector3[] vertices = branch_mesh.vertices;
        Texture2D texture = new Texture2D (Mathf.FloorToInt(Mathf.Pow(vertices.Length, 0.5f)), Mathf.FloorToInt(Mathf.Pow(vertices.Length, 0.5f)));
        Color[] colors = new Color[vertices.Length];
        // int index;

        for (int c = 0; c < colors.Length; c++) {
            // index = Random.Range(0, current_colors.Length - 1);
            // colors[c] = color_sets[current_colors[index]];
            colors[c] = current_color;
        }

        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colors);
        texture.Apply();
        return texture;
    }


}
