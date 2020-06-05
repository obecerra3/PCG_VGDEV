using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour {

    Camera mainCamera;
    public GenerateTerrain generateTerrain;
    public GenerateTree generateTree;
    public int seed = 2;
    GameObject center_tree;
    GameObject right_tree;
    GameObject left_tree;
    GameObject terrain;
    public InputField inputField;

    void Start() {
        var submitEvent = new InputField.SubmitEvent();
        submitEvent.AddListener(submitSeed);
        inputField.onEndEdit = submitEvent;

        generateTerrain = GetComponent<GenerateTerrain>();
        generateTree = GetComponent<GenerateTree>();

        initializeCamera();

        terrain = generateTerrain.createTerrain(mainCamera.transform.position);
        renderTrees();
    }

    void Update() {

    }

    void initializeCamera() {
        mainCamera = Camera.main;
        mainCamera.transform.position += new Vector3(0, 6, 0);
    }

    void submitSeed(string text) {
        int newSeed;
        bool success = int.TryParse(text, out newSeed);
        if (success) {
            changeSeed(newSeed);
        }
    }

    void changeSeed(int newSeed) {
        seed = newSeed;
        destroyTrees();
        renderTrees();
    }

    void destroyTrees() {
        Destroy(center_tree);
        Destroy(left_tree);
        Destroy(right_tree);
    }

    void renderTrees() {
        center_tree = generateTree.createTree(seed, mainCamera.transform.position + new Vector3(0, -6.75f, 27));
        center_tree.name = "center_tree";
        left_tree = generateTree.createTree(seed + 1, mainCamera.transform.position + new Vector3(-10, -5.5f, 27));
        left_tree.name = "left_tree";
        right_tree = generateTree.createTree(seed + 2, mainCamera.transform.position + new Vector3(10, -6.25f, 27));
        right_tree.name = "right_tree";
    }
}
