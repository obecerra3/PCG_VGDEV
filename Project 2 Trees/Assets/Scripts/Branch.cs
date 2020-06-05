using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class Branch {
    public List<Bud> buds = new List<Bud>();
    public int order;
    public ProceduralTree tree;

    public Branch(ProceduralTree new_tree, Vector3 initial_position, Vector3 initial_tangent, int new_order) {
        Bud initial_bud = new Bud(initial_position, initial_tangent, true, true);
        initial_bud.dimension = 1;
        buds.Add(initial_bud);
        order = new_order;
        tree = new_tree;
    }

    public void grow(List<Branch> new_branches) {
        Bud bud;
        List<Bud> new_buds = new List<Bud>();
        float chance;
        for (int b = 0; b < buds.Count; b++) {
            bud = buds[b];
            chance = Random.value; //setting chance before this loop will make same thing happen to all valid buds
            if (bud.isValid()) {
                if (deathProb(bud)) {
                    bud.isAlive = false;
                } else if (!pauseProb(bud)) {
                    if (bud.apical) {
                        //create new buds along same axis
                        createNewBuds(bud, new_buds);
                    }
                    if (!bud.initial && ramProb(bud)) {
                        //create new branches
                        createNewAxillaryBud(bud, new_branches);
                    }
                }
            } else if (tree.ramification_type == "palm" && bud.apical && order == 1 && !bud.isAlive) {
                createNewAxillaryBud(bud, new_branches);
            }
        }
        if (new_buds.Count > 0) {
            buds.AddRange(new_buds);
        }
    }

    public bool deathProb(Bud bud) {
        //based off of age, dimension, and order
        float dimension_effect;
        dimension_effect = bud.dimension * 0.1f;
        if (Random.value > tree.death_prob + dimension_effect || bud.age < 30) {
            if (order > 1 && bud.dimension > 5 && tree.plagiotropic < 0.5f) {
                return true;
            }
            return false;
        }
        return true;
    }

    public bool pauseProb(Bud bud) {
        //based off of age, dimension, and order
        // if (Random.value > tree.pause_prob) {
        //     return false;
        // }
        return false;
    }

    public bool ramProb(Bud bud) {
        // chance > tree.ramification_prob * (1f/order)
        //based off of age, dimension, and order and tree.ramification_type
        switch (tree.ramification_type) {
            case "continuous":
                if (order == 1 && bud.dimension >= tree.min_dimension_1) {
                    return true;
                } else if (order != 1 && bud.dimension >= tree.min_dimension) {
                    return true;
                }
                return false;
            case "palm":
                return false;
            case "diffuse":
                if (order == 1 && bud.dimension >= tree.min_dimension_1) {
                    float chance = Random.value;
                    return chance > 0.1f;
                } else if (order != 1 && bud.dimension >= tree.min_dimension) {
                    float chance = Random.value;
                    return chance > 0.1f;
                }
                break;
            case "rhythmic":
                if (order == 1 && bud.dimension >= tree.min_dimension_1) {
                    if (bud.dimension % 2 == 0) {
                        return true;
                    }
                } else if (order != 1 && bud.dimension >= tree.min_dimension) {
                    if (bud.dimension % 2 == 0) {
                        return true;
                    }
                }
                return false;
        }
        return false;
    }


    public void createNewBuds(Bud bud, List<Bud> new_buds) {
        bud.apical = false;
        Bud previous_bud = bud;
        Bud new_bud;
        Vector3 new_direction, new_position, new_tangent, internode_offset;

        int new_buds_count = newBudsPerGrowth(bud);

        float x_offset, z_offset;
        for (int i = 0; i < new_buds_count; i++) {
            internode_offset = internodeOffsetPerGrowth(previous_bud);
            // new_direction = (previous_bud.tangent + internode_offset).normalized;
            if (order == 1) {
                x_offset = Random.Range(0.05f, 0.1f);
                z_offset = Random.Range(0.05f, 0.1f);
                new_direction = new Vector3(x_offset, 1f, z_offset).normalized;
            } else {
                new_direction = (previous_bud.tangent + internode_offset).normalized;
            }
            new_position = previous_bud.position + (internodeLengthPerGrowth(previous_bud) * new_direction);
            if (new_position.y <= 1) {
                new_position.y = 1;
            }
            new_tangent = new_direction;
            new_bud = new Bud(new_position, new_tangent);
            new_bud.age = previous_bud.age + 1;
            new_bud.dimension = previous_bud.dimension + 1;
            new_buds.Add(new_bud);
            previous_bud = new_bud;
        }

        //make last one apical
        new_buds[new_buds.Count - 1].apical = true;
    }

    public int newBudsPerGrowth(Bud bud) {
        return Mathf.CeilToInt(tree.internodes_per_growth * (1f / (order * 0.5f))); //
    }

    public float internodeLengthPerGrowth(Bud bud) {
        return tree.internode_length;//* (1f / (order * 0.15f));
    }

    public Vector3 internodeOffsetPerGrowth(Bud bud) {
        Vector3 tangent = bud.tangent;
        Vector3 normal = Vector3.Cross(tangent, Vector3.right).normalized;
        Vector3 binormal = Vector3.Cross(tangent, normal).normalized;
        Vector3 offset = Vector3.up * tree.orthotropic;
        offset += new Vector3(bud.tangent.x, 0f, bud.tangent.z) * tree.plagiotropic;
        offset += normal * Random.Range(-1f, 1f) * tree.wiggle;
        offset += binormal * Random.Range(-1f, 1f) * tree.wiggle;
        return offset;
    }

    public void createNewAxillaryBud(Bud bud, List<Branch> new_branches) {
        bud.axillary = true;

        float num_of_branches = (float) Random.Range(tree.min_buds_per_node, 5);

        Vector3 tangent = bud.tangent;
        Vector3 normal = Vector3.Cross(tangent, Vector3.right).normalized;
        Vector3 binormal = Vector3.Cross(tangent, normal).normalized;

        Vector3 initial_position = bud.position;
        Vector3 new_vertex;
        Vector3 initial_tangent;

        Branch new_branch;
        float initial_theta = Random.Range(0f, 2f*Mathf.PI);
        for (float theta = initial_theta; theta < 2f*Mathf.PI + initial_theta; theta += ((2f*Mathf.PI) / num_of_branches)) {
            new_vertex = ((normal * Mathf.Cos(theta)) + (binormal * Mathf.Sin(theta))) * radiusPerGrowth(bud.dimension);
            initial_tangent = new_vertex.normalized;
            // debugSphere(initial_tangent, Color.red, 0.01f, tree.parent_object);
            new_branch = new Branch(tree, initial_position + new_vertex, initial_tangent, order + 1);
            new_branch.buds[0].age = bud.age + 1;
            new_branches.Add(new_branch);
        }

    }

    public float radiusPerGrowth(int dimension) {
        float radius = tree.internode_radius * order;
        radius *=  0.1f * (1f / dimension);
        radius *= 2f; //tweak according to how much branches are sticking out of each other
        return radius;
    }

    public void debugSphere(Vector3 position, Color new_color, float scale_change, GameObject parent_object) {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.localScale *= scale_change;
        sphere.transform.position = position;
        sphere.transform.parent = parent_object.transform;
        Renderer renderer = sphere.GetComponent<Renderer>();
        renderer.material.color = new_color;
    }



}
