using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Terrain : MonoBehaviour
{
    // 1024 vertices
    public static int T_SIZE = 16;                  // The number of vertices per edge on the terrain
    public static int V_WIDE = 28;          // 28
    public static int V_DEEP = 2;       //2

    public float terrainScale = 1f;                 // The 'scale' of the terrain - how much detail in the region

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    MeshCollider meshCollider;

    // Notation: +z = up, +x = right
    Vector3[] terrain;
    bool isGridTerrain;

    List<Vector2> geogebra2DTerrain = new List<Vector2>()
    {
        new Vector2(-3.7f,0f),
        new Vector2(-3.7f,0.8f),
        new Vector2(-3.2f,0.8f),
        new Vector2(-3.2f,0f),
        new Vector2(-1.9f,0f),
        new Vector2(-1.9f,0.8f),
        new Vector2(-1.4f,0.8f),
        new Vector2(-1.4f,0f),
        new Vector2(-0.1f,0f),
        new Vector2(-0.1f,0.8f),
        new Vector2(0.4f,0.8f),
        new Vector2(0.4f,0f),
        new Vector2(1, 0),
        new Vector2(1.7f,0f),
        new Vector2(1.7f,0.8f),
        new Vector2(2.2f,0.8f),
        new Vector2(2.2f,0f),
        new Vector2(3, 0),
        new Vector2(3.5f,0f),
        new Vector2(3.5f,0.8f),
        new Vector2(4f,0.8f),
        new Vector2(4f,0f),
        new Vector2(5, 0),
        new Vector2(5.3f,0f),
        new Vector2(5.3f,0.8f),
        new Vector2(5.8f,0.8f),
        new Vector2(5.8f,0f),
        new Vector2(7, 0),
    };


    /* 
     * Awake is called when the script instance is being loaded
     */
    void Awake() {
        var t_start = Time.realtimeSinceStartup;
        this.gameObject.transform.localPosition = Vector3.zero;

        meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
        meshFilter = this.gameObject.AddComponent<MeshFilter>();
        meshCollider = this.gameObject.AddComponent<MeshCollider>();

        meshRenderer.material.SetColor("_Color", new Color(0f, 0f, 0f));
        meshRenderer.material.SetFloat("_Glossiness", 0.25f);
        //meshFilter.mesh = CreateShapes.GetShapeMesh(CreateShapes.TERRAIN, vertexPerEdge: T_SIZE, terrainScale: terrainScale);
        meshFilter.mesh = CreateShapes.Custom_Create2DTerrain(geogebra2DTerrain);
        isGridTerrain = false;

        meshCollider.sharedMesh = meshFilter.mesh;

        terrain = meshFilter.mesh.vertices;
        print("Time for generating terrain: " + (Time.realtimeSinceStartup - t_start).ToString("f6") + "ms");
    }

    public Vector3 GetVertex(int x, int z) {
        return terrain[z*V_WIDE + x];
    }

    /*
     * Casts a ray from a vertex on the terrain towards the guard. If the ray hits the terrain
     * then that point isn't visible. If it doesn't hit the terrain we should have a clear line
     * of sight to the guard
     */
    public bool DoesRayHitTerrain(Ray ray) {
        RaycastHit hit;
        return meshCollider.Raycast(ray, out hit, 200f);
    }

    public bool IsGrid() {
        return isGridTerrain;
    }
}
