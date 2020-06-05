using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldBox
{
    public Vector3 bounds;

    public CMesh box_mesh;

    public GameObject box_obj;

    public WorldBox()
    {
        bounds = new Vector3(45f, 60f, 70f);

        buildMesh();

        box_obj = new GameObject();
        box_obj.name = "WorldBox";

        MeshFilter mesh_filter = (MeshFilter) box_obj.AddComponent(typeof(MeshFilter));
        MeshRenderer mesh_renderer = (MeshRenderer) box_obj.AddComponent(typeof(MeshRenderer));
        mesh_filter.mesh = box_mesh.mesh;
        mesh_renderer.material = (Material) Resources.Load("Materials/Glass");
    }

    public void buildMesh()
    {
        float x = bounds.x;
        float y = bounds.y;
        float z = bounds.z;

        box_mesh = new CMesh();

        //build geo_table
        box_mesh.geo_table.Add(new Vector3(-x, y, 0));
        box_mesh.geo_table.Add(new Vector3(-x, y, z));
        box_mesh.geo_table.Add(new Vector3(-x, 0, 0));
        box_mesh.geo_table.Add(new Vector3(-x, 0, z));
        box_mesh.geo_table.Add(new Vector3(x, y, 0));
        box_mesh.geo_table.Add(new Vector3(x, y, z));
        box_mesh.geo_table.Add(new Vector3(x, 0, 0));
        box_mesh.geo_table.Add(new Vector3(x, 0, z));

        //build_uvs
        for (int i = 0; i < 8; i++)
        {
            box_mesh.uvs.Add(new Vector2(i / 8f, i / 8f));
        }

        //build triangle_table
        box_mesh.addTriangle(0, 2, 3);
        box_mesh.addTriangle(0, 3, 1);
        box_mesh.addTriangle(4, 7, 6);
        box_mesh.addTriangle(4, 5, 7);
        box_mesh.addTriangle(0, 4, 2);
        box_mesh.addTriangle(4, 6, 2);
        box_mesh.addTriangle(3, 5, 1);
        box_mesh.addTriangle(3, 7, 5);
        box_mesh.addTriangle(2, 3, 6);
        box_mesh.addTriangle(3, 7, 6);

        box_mesh.addToMesh();

    }


}
