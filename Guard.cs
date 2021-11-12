using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guard
{

    public float GUARD_HEIGHT = 8f;
    public float MAX_PHI = (Mathf.PI / 4f);             // Max Viewing angle measured as angle between verticle and edge of view

    byte[,] guardVis;
    Vector3 pos;
    GameObject guardModel;
    List<LineRenderer> visLines;


    /* 
     * Initializes a guard above the vertex at (x,z). These coordespond to relative positions in the terrain,
     * not physical coordinates, unless the terrain is not a grid terrain
     */
    public Guard(int x, int z, Terrain t) {
        guardVis = new byte[Terrain.V_WIDE, Terrain.V_DEEP];

        if (t.IsGrid()) {
            pos = t.GetVertex(x, z);
            pos.y = GUARD_HEIGHT;
        }
        else {
            pos = new Vector3(x, GUARD_HEIGHT, z);
        }

        GenerateVisibility(t);
    }

    public Guard(int x, int z, Terrain t, float pitch) {
        guardVis = new byte[Terrain.V_WIDE, Terrain.V_DEEP];

        if (t.IsGrid()) {
            pos = t.GetVertex(x, z);
            pos.y = GUARD_HEIGHT;
        }
        else {
            pos = new Vector3(x, GUARD_HEIGHT, z);
        }

        GenerateVisibility(t);
    }

    public Guard(Vector2 pt, Terrain t) {
        guardVis = new byte[Terrain.V_WIDE, Terrain.V_DEEP];

        pos = t.IsGrid() ? t.GetVertex((int) pt.x, (int) pt.y) : new Vector3(pt.x, 0, pt.y);
        pos.y = GUARD_HEIGHT;

        GenerateVisibility(t);
    }

    void GenerateVisibility(Terrain t) {
        Ray currRay = new Ray( origin: pos, direction: Vector3.down );
        for (int x = 0; x < Terrain.V_WIDE; x++) {
            for (int z = 0; z < Terrain.V_DEEP; z++) {
                currRay.origin = pos; //t.GetVertex(x, z);
                currRay.direction = (t.GetVertex(x, z) - currRay.origin).normalized;

                // <r, theta, phi>
                if (Maffs.CartesianToSpherical(currRay.direction).z > 2*Mathf.PI - MAX_PHI) {
                    // Its out of the guards visibility
                    continue;
                }

                Debug.DrawRay(currRay.origin, currRay.direction*200, Color.white, 100);
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

    public bool IsPtVisible(int x, int z) {
        return guardVis[x, z] == 1;
    }

    /* 
     * Returns a byte map of the intersection of this guard and another guard's visibility.
     * If there is no overlap in their visibility then null is returned, otherwise a byte map
     * is returned
     */ 
    public byte[,] IntersectVis(Guard g) {
        byte[,] vis2 = g.GetVisibility();
        byte[,] intersection = new byte[Terrain.V_WIDE, Terrain.V_DEEP];
        bool anyOverlap = false;

        for (int x = 0; x < Terrain.V_WIDE; x++) {
            for (int z = 0; z < Terrain.V_DEEP; z++) {
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

    public Vector3 GetPos() {
        return pos;
    }
}
