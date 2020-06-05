using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class FlockUI
{

    static private Toggle centering_toggle;
    static private Toggle velocity_matching_toggle;
    static private Toggle collision_avoidance_toggle;
    static private Toggle wander_toggle;
    static private Toggle trail_toggle;

    static private GameObject centering_obj;
    static private GameObject velocity_matching_obj;
    static private GameObject collision_avoidance_obj;
    static private GameObject wander_obj;
    static private GameObject trail_obj;

    static private Toggle[] toggle_array;
    static private GameObject[] toggle_obj_array;

    static private InputField boid_size_input;
    static private int boid_size;

    static FlockUI()
    {
        centering_obj = GameObject.Find("centering");
        centering_toggle = centering_obj.GetComponent<Toggle>();

        velocity_matching_obj = GameObject.Find("velocity_matching");
        velocity_matching_toggle = velocity_matching_obj.GetComponent<Toggle>();

        collision_avoidance_obj = GameObject.Find("collision_avoidance");
        collision_avoidance_toggle = collision_avoidance_obj.GetComponent<Toggle>();

        wander_obj = GameObject.Find("wander");
        wander_toggle = wander_obj.GetComponent<Toggle>();

        trail_obj = GameObject.Find("trail_toggle");
        trail_toggle = trail_obj.GetComponent<Toggle>();

        toggle_array = new Toggle[]
        {
            centering_toggle,
            velocity_matching_toggle,
            collision_avoidance_toggle,
            wander_toggle,
            trail_toggle,
        };

        toggle_obj_array = new GameObject[]
        {
            centering_obj,
            velocity_matching_obj,
            collision_avoidance_obj,
            wander_obj,
            trail_obj
        };

        positionToggles();

        boid_size = 100;
        boid_size_input = GameObject.Find("boid_size").GetComponent<InputField>();

        var submitEvent = new InputField.SubmitEvent();
        submitEvent.AddListener(submitSize);
        boid_size_input.onEndEdit = submitEvent;
    }

    static private void submitSize(string _text)
    {
        int new_size;
        bool success = int.TryParse(_text, out new_size);
        if (success) {
            boid_size = Mathf.Min(new_size, 150);
        }
    }

    static public int getBoidSize()
    {
        return boid_size;
    }

    static private void positionToggles() {
        RectTransform rect_transform;
        for (int i = 0; i < toggle_array.Length; i++) {
            rect_transform = toggle_obj_array[i].GetComponent<RectTransform>();
            rect_transform.position += (Vector3.down * rect_transform.rect.height * i) + (Vector3.up * 50);
        }
    }

    static public bool centeringEnabled()
    {
        return centering_toggle.isOn;
    }

    static public bool velocityMatchingEnabled()
    {
        return velocity_matching_toggle.isOn;
    }

    static public bool collisionAvoidanceEnabled()
    {
        return collision_avoidance_toggle.isOn;
    }

    static public bool wanderEnabled()
    {
        return wander_toggle.isOn;
    }

    static public bool trailEnabled()
    {
        return trail_toggle.isOn;
    }
}
