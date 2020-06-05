using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CpState {
    public List<Vector3> cps = new List<Vector3>();
    public Vector3 position;
    public List<float> distances;
    public List<float> bend_points;
    public List<Vector3> directions;
    public List<Vector3> normals;
    public List<Vector3> binormals;
    public List<Vector2> size_offsets; //(width, height)


    public CpState(Vector3 _position, List<float> _distances, List<float> _bend_points, List<Vector3> _directions, List<Vector2> _size_offsets) {
        position = _position;
        distances = _distances;
        bend_points = _bend_points;
        directions = _directions;
        size_offsets = _size_offsets;

        //init normals and binormals
        normals = new List<Vector3>();
        binormals = new List<Vector3>();
        Vector3 direction, normal, binormal;
        for (int i = 0; i < directions.Count; i++) {
            direction = directions[i];
            if (direction.Equals(Vector3.up) || direction.Equals(Vector3.down)) {
                normal = Vector3.Cross(direction, Vector3.right).normalized;
            } else {
                normal = Vector3.Cross(direction, Vector3.down).normalized;
            }
            binormal = Vector3.Cross(direction, normal).normalized;
            normals.Add(normal);
            binormals.Add(binormal);
        }
    }

    public void addCp() {
        cps.Add(position);
    }

    public float getDistance(float current_cp) {
        return distances[Mathf.RoundToInt(current_cp * distances.Count)];
        //determine cp_distance
        // if (current_cp < 0.3f || current_cp > 0.7f) {
        //     // cp_distance = cp_distance * 0.8f;
        // } else {
        //     cp_distance = front_box_length;
        // }
    }

    public Vector3 getDirection(float current_cp) {
        for (int i = 0; i < bend_points.Count; i++) {
            if (current_cp < bend_points[i]) {
                return directions[i];
            }
        }
        return directions[directions.Count - 1];
    }

    public Vector3 getNormal(float current_cp) {
        //already accounts for current_cp > normals.Count
        //
        for (int i = 0; i < bend_points.Count; i++) {
            if (current_cp < bend_points[i]) {
                return normals[i];
            }
        }
        return normals[normals.Count - 1];
    }

    public Vector3 getBinormal(float current_cp) {
        for (int i = 0; i < bend_points.Count; i++) {
            if (current_cp < bend_points[i]) {
                return binormals[i];
            }
        }
        return binormals[binormals.Count - 1];
    }

    public Vector2 getSizeOffset(float current_cp) {
        return size_offsets[Mathf.RoundToInt(current_cp * size_offsets.Count)];
    }

    public void updatePosition(float current_cp) {
        position += getDirection(current_cp) * getDistance(current_cp);
    }

    public Vector3 getNextNormal(float current_cp) {
        for (int i = 0; i < bend_points.Count; i++) {
            if (current_cp < bend_points[i]) {
                return normals[Mathf.Min(i + 1, normals.Count - 1)];
            }
        }
        return normals[normals.Count - 1];
    }

    public Vector3 getNextBinormal(float current_cp) {
        for (int i = 0; i < bend_points.Count; i++) {
            if (current_cp < bend_points[i]) {
                return binormals[Mathf.Min(i + 1, binormals.Count - 1)];
            }
        }
        return binormals[binormals.Count - 1];
    }

    public float getInterpolant(float current_cp) {
        return 0f;
        float next_bend = bend_points[bend_points.Count - 1];
        float past_bend = bend_points[Mathf.Max(bend_points.Count - 2, 0)];
        for (int i = 0; i < bend_points.Count; i++) {
            if (current_cp < bend_points[i]) {
                next_bend = bend_points[i];
                past_bend = bend_points[Mathf.Max(0, i - 1)];
                break;
            }
        }
        float next_interpolant = 1f - (next_bend - current_cp);
        float past_interpolant = 1f - (current_cp - past_bend);
        if (next_bend < current_cp) {
            return 0f;
        }
        return next_interpolant - (0.25f * past_interpolant);
    }
}
