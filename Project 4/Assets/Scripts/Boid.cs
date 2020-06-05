using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid
{
    private GameObject obj = new GameObject();
    private bool enabled;
    private Vector3 velocity;
    private Vector3 force;

    public Boid()
    {
        velocity = Vector3.zero;
        force = Vector3.zero;
    }

    public void setForce(Vector3 _force)
    {
        force = _force;
    }

    public Vector3 getForce()
    {
        return force;
    }

    public void setVelocity(Vector3 _velocity)
    {
        velocity = _velocity;
    }

    public Vector3 getVelocity()
    {
        return velocity;
    }

    public void setEnabled(bool _enabled)
    {
        enabled = _enabled;
        obj.SetActive(_enabled);
    }

    public bool getEnabled()
    {
        return enabled;
    }

    public void setObj(GameObject _obj)
    {
        obj = _obj;
    }

    public GameObject getObj()
    {
        return obj;
    }


}
