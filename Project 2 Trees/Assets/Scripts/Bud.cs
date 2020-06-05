using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Bud {
    public bool initial;
    public bool apical;
    public bool axillary = false;
    public bool hasLeaf = false;
    public bool isAlive = true;
    public Vector3 position;
    public Vector3 tangent;
    public int age;
    public int dimension;

    public Bud(Vector3 new_position, Vector3 new_tangent, bool new_apical = false, bool new_initial = false) {
        position = new_position;
        tangent = new_tangent;
        apical = new_apical;
        initial = new_initial;
    }

    public bool isValid() {
        return isAlive && !axillary;
    }

}
