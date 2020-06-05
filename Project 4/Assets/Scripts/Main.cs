using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    Flock flock;
    BirdMeshBuilder bird_mesh_builder;
    WorldBox world_box;

    void Start()
    {
        bird_mesh_builder = new BirdMeshBuilder();
        world_box = new WorldBox();
        flock = GetComponent<Flock>();
        flock.initialize(bird_mesh_builder.bird_mesh, world_box.bounds, bird_mesh_builder.bird_radius);
    }

    void Update()
    {
        if (flock != null)
        {
            flock.update();

        } else
        {
            print("Error: flock is null");
        }

    }
}
