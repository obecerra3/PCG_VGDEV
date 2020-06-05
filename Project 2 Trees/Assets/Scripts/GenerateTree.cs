using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateTree : MonoBehaviour {

    public GameObject createTree(int seed, Vector3 position) {
        GameObject tree_object = new GameObject();
        Leaf leaf = (Leaf) GetComponent<Leaf>();
        Mesh leaf_mesh = leaf.createMesh();
        Texture2D leaf_texture = leaf.createTexture();
        ProceduralTree tree = tree_object.AddComponent<ProceduralTree>();
        tree.setParameters(seed, tree_object, leaf_mesh, leaf_texture);
        tree.createBranches();
        tree.renderBranches();
        tree_object.transform.position = position;
        return tree_object;
    }


}
