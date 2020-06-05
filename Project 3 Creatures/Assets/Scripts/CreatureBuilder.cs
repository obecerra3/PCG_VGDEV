using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureBuilder {

    //input parameters
    int seed;
    GameObject parent_object;

    //component builder scripts
    TorsoBuilder torso_builder = new TorsoBuilder();
    TailBuilder tail_builder = new TailBuilder();
    LimbBuilder limb_builder = new LimbBuilder();
    HeadBuilder head_builder = new HeadBuilder();

    //component dictionary
    Dictionary<string, GameObject> components;

    //initialize and set parameters
    public void setParameters(int _seed, GameObject _parent_object)  {
        seed = _seed;
        parent_object = _parent_object;
        components = new Dictionary<string, GameObject>();
        Random.InitState(seed);
    }

    //TailConfig, TorsoConfig, LimbConfig, HandConfig, HeadConfig which init state.
    //Or pass the same mesh to all of the builders and let them do their thing just adding onto the mesh.
    //pass on a parameter called like connection vertices or shared vertices so that the triangle builder
    //in each ComponentBuilder knows where to start connecting from. For now this is definitely feasible
    //with the tail to torso to neck to head. With the limbs I may need to define an abstract concept:
        //plane/ rectangle where if your connection is interrupted, e.g. the plane sits between
        //one vertex and another you create new vertices on either end of the plane. Restrict the
        //sides of the plane horizontally allong the z axis to lie between the zpoints of two control points
        //on the torso.
    //Go from head to tail in loop creating control points and building mesh allong the way.
    //end with blocky creature that is connected and then do loop subdivision.


    public void build() {
        //initialize some random values to get a uniform creature and pass to component builder scripts as necessary
        components.Add("torso", torso_builder.build());
        components.Add("tail", tail_builder.build(torso_builder));
        components.Add("limbs", limb_builder.build(torso_builder));
        components.Add("head", head_builder.build(torso_builder));

        //set parent of components
        foreach (KeyValuePair<string, GameObject> pair in components) {
            pair.Value.transform.parent = parent_object.transform;
        }

        // parent_object.transform.localScale *= 15f;
    }

}
