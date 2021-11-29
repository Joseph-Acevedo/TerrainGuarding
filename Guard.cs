using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guard
{

    public float GUARD_HEIGHT = 2f;
    public float MAX_PHI = (Mathf.PI / 4f);             // Max Viewing angle measured as angle between verticle and edge of view

    byte[,] guardVis;
    Vector3 pos;
    Vector2 spawnCoords;
    GameObject guardModel;


    /* 
     * Initializes a guard above the vertex at (x,z). These coordespond to relative positions in the terrain,
     * not physical coordinates
     */
    public Guard(int x, int z, Terrain t) {
        guardVis = new byte[Terrain.T_SIZE, Terrain.T_SIZE];

        spawnCoords = new Vector2(x, z);
        pos = t.GetVertex(x, z);
        pos.y = GUARD_HEIGHT;

        GenerateVisibility(t);
    }

    public Guard(Vector2 pt, Terrain t) {
        guardVis = new byte[Terrain.T_SIZE, Terrain.T_SIZE];

        pos = t.GetVertex((int) pt.x, (int) pt.y);
        pos.y = GUARD_HEIGHT;

        GenerateVisibility(t);
    }

    void GenerateVisibility(Terrain t) {
        Ray currRay = new Ray( origin: pos, direction: Vector3.down );
        for (int x = 0; x < Terrain.T_SIZE; x++) {
            for (int z = 0; z < Terrain.T_SIZE; z++) {
                currRay.origin = t.GetVertex(x, z);
                currRay.direction = pos - currRay.origin;

                // <r, theta, phi>
                if (Maffs.CartesianToSpherical(currRay.direction).z > MAX_PHI) {
                    // Its out of the guards visibility
                    continue;
                }

                // Check if it has a clear line of sight to guard
                if (!t.DoesRayHitTerrain(currRay)) {
                    guardVis[x, z] = 1;
                }
            }
        }

    }

    public byte[,] GetVisibility() {
        return guardVis;
    }

    /* 
     * Returns a byte map of the intersection of this guard and another guard's visibility.
     * If there is no overlap in their visibility then null is returned, otherwise a byte map
     * is returned
     */ 
    public byte[,] IntersectVis(Guard g) {
        byte[,] vis2 = g.GetVisibility();
        byte[,] intersection = new byte[Terrain.T_SIZE, Terrain.T_SIZE];
        bool anyOverlap = false;

        for (int x = 0; x < Terrain.T_SIZE; x++) {
            for (int z = 0; z < Terrain.T_SIZE; z++) {
                if (guardVis[x, z] == 1 && vis2[x, z] == 1) {
                    intersection[x, z] = 1;
                    anyOverlap = true;
                }
            }
        }

        if (!anyOverlap)
            return null;
        else
            return intersection;
    }

    public byte[,] UnionVis(Guard g) {
        byte[,] vis2 = g.GetVisibility();
        byte[,] union = new byte[Terrain.T_SIZE, Terrain.T_SIZE];

        for (int x = 0; x < Terrain.T_SIZE; x++) {
            for (int z = 0; z < Terrain.T_SIZE; z++) {
                if (guardVis[x, z] == 1 || vis2[x, z] == 1) {
                    union[x, z] = 1;
                }
            }
        }

        return union;
    }

    public Vector3 GetPos() {
        return pos;
    }

    public Vector2 GetSpawnPos() {
        return this.spawnCoords;
    }

    public override string ToString() {
        return "G("+spawnCoords.x+","+spawnCoords.y+")";
    }
}
