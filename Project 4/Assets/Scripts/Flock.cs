using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static FlockUI;

public class Flock : MonoBehaviour
{
    public float centering_weight = 30f;
    public float collision_avoidance_weight = 2f;
    public float velocity_matching_weight = 1f;
    public float wander_weight = 1.5f;

    public float min_velocity, max_velocity;

    public GameObject flock_obj;

    private List<Boid> boid_pool = new List<Boid>();
    private int enabled_count;

    private Vector3 bounds;
    private List<Plane> bound_planes;

    private float delta_t = 0.1f;

    private float boid_radius;

    private List<GameObject> trail_pool = new List<GameObject>();

    //=======================
    //      INITIALIZE
    //=======================

    public void initialize(Mesh _boid_mesh, Vector3 _bounds, float _boid_radius)
    {
        flock_obj = new GameObject();
        flock_obj.name = "Flock";

        bounds = _bounds;
        initBoundPlanes();

        boid_radius = _boid_radius;

        min_velocity = 1f;
        max_velocity = 3f;

        initBoidPool(_boid_mesh);

        //enable 100 boids to start with
        enableBoids(100);

        scatter();

        initTrailPool();
    }

    private void initBoundPlanes()
    {
        bound_planes = new List<Plane>()
        {
            new Plane(Vector3.right, Vector3.right * bounds.x),
            new Plane(Vector3.left, Vector3.left * bounds.x),
            new Plane(Vector3.up, Vector3.up * bounds.y),
            new Plane(Vector3.down, Vector3.zero),
            new Plane(Vector3.forward, Vector3.forward * bounds.z),
            new Plane(Vector3.back, Vector3.zero)
        };
    }

    private void initBoidPool(Mesh _boid_mesh)
    {
        int boid_pool_size = 150;
        Boid boid;
        GameObject boid_obj;
        MeshFilter mesh_filter;
        MeshRenderer mesh_renderer;

        for (int i = 0; i < boid_pool_size; i++)
        {
            boid = new Boid();
            boid_obj = boid.getObj();
            boid_obj.name = "boid #" + i;
            boid_obj.transform.parent = flock_obj.transform;

            mesh_filter = (MeshFilter) boid_obj.AddComponent(typeof(MeshFilter));
            mesh_renderer = (MeshRenderer) boid_obj.AddComponent(typeof(MeshRenderer));
            mesh_filter.mesh = _boid_mesh;
            mesh_renderer.material = (Material) Resources.Load("Materials/BirdGlass");
            boid.setEnabled(false);
            boid_pool.Add(boid);
        }
    }

    private void initTrailPool()
    {
        int trail_pool_size = 5000;
        GameObject trail_obj;

        for (int i = 0; i < trail_pool_size; i++)
        {
            trail_obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            trail_obj.name = "trail_obj #" + i;
            trail_obj.transform.parent = flock_obj.transform;
            trail_obj.transform.localScale *= 0.1f;
            trail_obj.GetComponent<Renderer>().material = (Material) Resources.Load("Materials/BirdGlass");
            trail_obj.SetActive(false);
            trail_pool.Add(trail_obj);
        }
    }

    private IEnumerator SpawnTrail(Vector3 _position)
    {
        yield return new WaitForSeconds(0.4f);
        GameObject trail_obj = null;
        for (int i = 0; i < trail_pool.Count; i++)
        {
            trail_obj = trail_pool[i];
            if (!trail_obj.activeSelf)
            {
                break;
            }
        }
        trail_obj.transform.position = _position;
        trail_obj.SetActive(true);
        yield return new WaitForSeconds(2f);
        trail_obj.SetActive(false);
    }

    //=================
    //     UPDATE
    //=================

    public void update()
    {
        Boid boid;
        Vector3 new_velocity;
        Vector3 pos;

        for (int i = 0; i < enabled_count; i++)
        {
            boid = boid_pool[i];
            boid.setForce(forceAll(boid));

            if (trailEnabled())
            {
                StartCoroutine(SpawnTrail(boid.getObj().transform.position));
            }
        }

        for (int i = 0; i < enabled_count; i++)
        {
            boid = boid_pool[i];
            new_velocity = boundVelocity(boid.getVelocity() + delta_t * boid.getForce(), boid.getObj().transform.position);
            boid.getObj().transform.position += delta_t * ((boid.getVelocity() + new_velocity) * 0.5f);

            pos = boid.getObj().transform.position;
            //BOUND X POSITION
            if (pos.x >= bounds.x)
            {
                boid.getObj().transform.position = new Vector3(bounds.x - (pos.x - bounds.x), pos.y, pos.z);
            }
            if (pos.x <= -bounds.x)
            {
                boid.getObj().transform.position = new Vector3(-bounds.x - (pos.x - bounds.x), pos.y, pos.z);
            }

            //BOUND Y POSITION
            if (pos.y >= bounds.y)
            {
                boid.getObj().transform.position = new Vector3(pos.x, bounds.y - (pos.y - bounds.y), pos.z);
            }
            if (pos.y <= 0)
            {
                boid.getObj().transform.position = new Vector3(pos.x, -pos.y, pos.z);
            }

            //BOUND Z POSITION
            if (pos.z >= bounds.z)
            {
                boid.getObj().transform.position = new Vector3(pos.x, pos.y, bounds.z - (pos.z - bounds.z));
            }
            if (pos.y <= 0)
            {
                boid.getObj().transform.position = new Vector3(pos.x, pos.y, -pos.z);
            }

            boid.setVelocity(new_velocity);
            boid.getObj().transform.rotation = Quaternion.LookRotation(-new_velocity);
        }

        if (Input.GetKeyDown("space"))
        {
            scatter();
        }

        if (getBoidSize() != enabled_count)
        {
            enableBoids(getBoidSize());
        }
    }

    //================
    //    FORCES
    //================

    public Vector3 forceAll(Boid _boid)
    {
        // force_all = wfc ffc + wvm fvm + wcm fcm + ww fw
        return (centering_weight * centeringForce(_boid)) +
                (collision_avoidance_weight * collisionAvoidance(_boid)) +
                (velocity_matching_weight * velocityMatching(_boid)) +
                (wander_weight * wander(_boid));
    }

    public Vector3 centeringForce(Boid _boid)
    {
        /* - rFc, radius of flock centering
        // - find list of k neighbors within rFc
        // - unweighted centroid:
        //     - c = (1 / k) sum from i to k of Pi
        // - force of flock centering fFc = c - p
        //     - this is a vector pointing from p to the centroid of its neighboring boids
        // - distance between boids i,j
        //     - dij = sqrt( (pix - pjx)^2 + (piy - pjy)^2 +(piz - pjz)^2)
        // - weight
        //     - wij = 1 / (dij ^ 2 + Epsilon) inverse square with epsilon being some small value
        //     - wij = max(dmax - dij, 0) linear
        // - weighted flock centering
        //     - fFc = sum from i to k of Wi (pi - p) / sum from i to k of Wi
        //     - the division normalizes the weight*/
        if (!centeringEnabled())
        {
            return Vector3.zero;
        }

        float radius_FC = boid_radius * 20f;
        float distance, weight;
        GameObject other_boid_obj;
        Vector3 boid_pos = _boid.getObj().transform.position;
        float epsilon = 0.01f;
        Vector3 force_FC = new Vector3();
        float weight_sum = 0f;

        for (int i = 0; i < enabled_count; i++)
        {
            other_boid_obj = boid_pool[i].getObj();
            distance = Vector3.Distance(boid_pos, other_boid_obj.transform.position);
            if (distance < radius_FC)
            {
                weight = 1f / (Mathf.Pow(distance, 2f) + epsilon);
                force_FC += (other_boid_obj.transform.position - boid_pos) * weight;
                weight_sum += weight;
            }
        }

        force_FC = Vector3.Scale(force_FC, Vector3.one * (1f/ weight_sum));
        return force_FC;
    }

    public Vector3 collisionAvoidance(Boid _boid)
    {
        // - rCa (radius of collision avoidance < rFc)
        // - fCa = sum from i to k of wi (p - pi) which is a force in a direction away from all neighbors.
        float radius_CA = boid_radius * 2f;
        float distance, weight;
        float epsilon = 0.01f;
        Vector3 force_CA = new Vector3();
        Vector3 boid_pos = _boid.getObj().transform.position;
        Plane bound_plane;

        //check for collisions with world box bounding planes
        for (int i = 0; i < bound_planes.Count; i++)
        {
            bound_plane = bound_planes[i];
            distance = bound_plane.GetDistanceToPoint(boid_pos);
            if (distance < radius_CA)
            {
                weight = 1f / (Mathf.Pow(distance, 2f) + epsilon);
                force_CA += (boid_pos - bound_plane.ClosestPointOnPlane(boid_pos)) * weight;
            }
        }

        if (!collisionAvoidanceEnabled())
        {
            return force_CA;
        }

        //collision avoidance with other boids
        GameObject other_boid_obj;
        radius_CA = boid_radius * 7f;
        for (int i = 0; i < enabled_count; i++)
        {
            other_boid_obj = boid_pool[i].getObj();
            distance = Vector3.Distance(boid_pos, other_boid_obj.transform.position);
            if (distance < radius_CA)
            {
                weight = 1f / (Mathf.Pow(distance, 2f) + epsilon);
                force_CA += (boid_pos - other_boid_obj.transform.position) * weight;
            }
        }

        return force_CA;
    }

    public Vector3 velocityMatching(Boid _boid)
    {
        // - rVm (radius of velocity matching) is approximately == to rFc
        // - fVm = sum from i to k of wi (vi - v)
        if (!velocityMatchingEnabled())
        {
            return Vector3.zero;
        }

        float radius_VM = boid_radius * 20f;
        float distance, weight;
        float epsilon = 0.01f;
        GameObject other_boid_obj;
        Vector3 other_boid_velocity;
        Vector3 force_VM = new Vector3();
        Vector3 boid_pos = _boid.getObj().transform.position;
        Vector3 boid_velocity = _boid.getVelocity();

        for (int i = 0; i < enabled_count; i++)
        {
            other_boid_obj = boid_pool[i].getObj();
            other_boid_velocity = boid_pool[i].getVelocity();
            distance = Vector3.Distance(boid_pos, other_boid_obj.transform.position);
            if (distance < radius_VM)
            {
                weight = 1f / (Mathf.Pow(distance, 2f) + epsilon);
                force_VM += (other_boid_velocity - boid_velocity) * weight;
            }
        }

        return force_VM;
    }

    public Vector3 wander(Boid _boid)
    {
        // - fw = (rx, ry) random value [-1, 1]
        if (!wanderEnabled())
        {
            return Vector3.zero;
        }
        return new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
    }


    //=================
    //     HELPERS
    //=================

    public void enableBoids(int _count)
    {
        enabled_count = _count;
        for (int i = 0; i < boid_pool.Count; i++)
        {
            if (i < enabled_count)
            {
                boid_pool[i].setEnabled(true);
            } else
            {
                boid_pool[i].setEnabled(false);
            }
        }
    }

    public void scatter()
    {
        Boid boid;
        float x = bounds.x;
        float y = bounds.y;
        float z = bounds.z;
        for (int i = 0; i < boid_pool.Count; i++)
        {
            boid = boid_pool[i];
            boid.getObj().transform.position = new Vector3(Random.Range(-x + 5, x - 5), Random.Range(5, y - 5), Random.Range(5, z - 5));
            //give random velocity too?///velocity = 0
            boid.setVelocity(Vector3.zero);
        }
    }

    public void toggleTrails(bool _enable)
    {

    }

    //bound the velocity by min, max values and keep boid within world_box
    public Vector3 boundVelocity(Vector3 _v, Vector3 _pos)
    {
        if (_v.magnitude > max_velocity)
        {
            _v = _v.normalized * max_velocity;
        } else if (_v.magnitude < min_velocity)
        {
            if (_v.magnitude == 0) {
                // _v = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                _v = Vector3.one;
            }
            _v = _v.normalized * min_velocity;

        }

        // float collision_radius = boid_radius * 5f;
        //
        // if (_pos.x >= bounds.x - collision_radius || _pos.x <= -bounds.x + collision_radius)
        // {
        //     _v = Vector3.Scale(_v, new Vector3(-1, 1, 1));
        // }
        //
        // if (_pos.y >= bounds.y - collision_radius || _pos.y <= 0 + collision_radius)
        // {
        //     _v = Vector3.Scale(_v, new Vector3(1, -1, 1));
        // }
        //
        // if (_pos.z >= bounds.z - collision_radius || _pos.z <= 0 + collision_radius)
        // {
        //     _v = Vector3.Scale(_v, new Vector3(1, 1, -1));
        // }

        return _v;
    }







}
