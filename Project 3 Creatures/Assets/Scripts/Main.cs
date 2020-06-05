using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour {

    Camera mainCamera;
    public TerrainBuilder terrain_builder = new TerrainBuilder();
    public CreatureBuilder creature_builder = new CreatureBuilder();
    public int seed = 2;
    GameObject[] creatures;
    GameObject terrain;
    public InputField inputField;
    Vector3[] positions;
    int creature_max = 12;

    void Start() {
        var submitEvent = new InputField.SubmitEvent();
        submitEvent.AddListener(submitSeed);
        inputField.onEndEdit = submitEvent;

        initializeCamera();

        // terrain = terrain_builder.createTerrain(mainCamera.transform.position);

        int zOffset = 27;
        positions = new Vector3[] {
            new Vector3(-8, 9f, zOffset),
            new Vector3(0, 9f, zOffset),
            new Vector3(8, 9f, zOffset),
            new Vector3(-8, 4f, zOffset),
            new Vector3(0, 4f, zOffset),
            new Vector3(8, 4f, zOffset),
            new Vector3(-8, -1, zOffset),
            new Vector3(0, -1, zOffset),
            new Vector3(8, -1, zOffset),
            new Vector3(-8, -6, zOffset),
            new Vector3(0, -6, zOffset),
            new Vector3(8, -6, zOffset),
        };

        creatures = new GameObject[creature_max];

        renderCreatures();
    }

    // void Vector3 getPosition(int creature_num, int creature_max) {
    //
    //
    // }

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
        destroyCreatures();
        renderCreatures();
    }

    void destroyCreatures() {
        for (int i = 0; i < creature_max; i++) {
            Destroy(creatures[i]);
        }
    }

    void renderCreatures() {
        for (int i = 0; i < creature_max; i++) {
            // spawn 1 centrally
            // if (i == 1) {
            //     creatures[i] = newCreature(seed + i, mainCamera.transform.position + positions[i]);
            //     creatures[i].name = "creature_" + i;
            // }
            // spawn creature_max;
            creatures[i] = newCreature(seed + i, mainCamera.transform.position + positions[i]);
            creatures[i].name = "creature_" + i;
        }
    }

    GameObject newCreature(int seed, Vector3 position) {
        GameObject creature_object = new GameObject();
        creature_builder.setParameters(seed, creature_object);
        creature_builder.build();
        creature_object.transform.position = position;
        return creature_object;
    }

}
