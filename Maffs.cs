using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Maffs
{

    public static Vector2[] NEIGHBORS_2D = {
        Vector2.up,
        Vector2.up   +  Vector2.left,
                        Vector2.left,
        Vector2.down +  Vector2.left,
        Vector2.down,
        Vector2.down +  Vector2.right,
                        Vector2.right,
        Vector2.up   +  Vector2.right
    };


    /* 
     * Solves the quadratic formula
     */
    public static float Quad(float a, float b, float c) {
        float d = b*b-4*a*c;

        // complex results
        if (d < 0)
            return -1f;

        float t1 = -b + Mathf.Sqrt(d) / (2 * a);
        float t2 = -b - Mathf.Sqrt(d) / (2 * a);
        return t1 >= 0 ? t1 : t2;
    }

    /*
     * Converts a point in spherical coordinates to a point in cartesian coordinates.
     * The given spherical point must be in the form <r, theta, phi> and the result will
     * be <x, y, z>. It is important to note that in this implementation the phi is measured
     * from the Y-axis rather than the Z-axis. This has the effect of swapping the Y and Z
     * results
     */
    public static Vector3 SphericalToCartesian(Vector3 pt) {
        // Assumes pt = <r, theta, phi>
        float x = pt.x * Mathf.Sin(pt.z) * Mathf.Cos(pt.y);
        float z = pt.x * Mathf.Sin(pt.z) * Mathf.Sin(pt.y);
        float y = pt.x * Mathf.Cos(pt.z);

        return new Vector3(x, y, z);
    }

    /*
     * Converts a point in cartesian coordinates to a point in spherical coordinates.
     * The returned spherical point will have phi measured from the Y-axis (this is
     * done by swapping the Z and Y axis in the calculations)
     */
    public static Vector3 CartesianToSpherical(Vector3 pt) {
        // assumes <x, y, z> where Y is the height
        float r = Mathf.Sqrt( (pt.x*pt.x) + (pt.y*pt.y) + (pt.z*pt.z) );
        float theta = Mathf.Atan2(pt.z, pt.x);
        float phi = Mathf.Atan2(Mathf.Sqrt( (pt.x*pt.x) + (pt.z*pt.z) ), pt.y);

        return new Vector3(r, theta, phi);
    }

    public static void AddToMatrix(int[,] A, byte[,] B) {
        if (A.GetLength(0) != B.GetLength(0) || A.GetLength(1) != B.GetLength(1))
            return;
        
        for (int i = 0; i < A.GetLength(0); i++) {
            for (int j = 0; j < A.GetLength(1); j++) {
                A[i, j] += B[i, j];
            }
        }
    }

    public static List<List<Vector2>> GetConnectedComponents(byte[,] region) {
        // initialized to 0: all nodes unvisited
        byte[,] visitMap = new byte[region.GetLength(0), region.GetLength(1)];
        List<List<Vector2>> components = new List<List<Vector2>>();

        for (int i = 0; i < region.GetLength(0); i++) {
            for (int j = 0; j < region.GetLength(1); j++) {
                // If unvisited:
                if (visitMap[i,j] == 0) {
                    visitMap[i,j] = 1;      // mark as visited, maybe dont???

                    // If part of visible region, BFS for the entire connected component
                    if (region[i,j] != 0) {
                        components.Add(BFSComponent(new Vector2(i, j), region, visitMap));
                    }
                }
            }
        }
        return components;
    }

    /**
     * Performs a BFS from the starting position to find all the points that are connected
     */
    private static List<Vector2> BFSComponent(Vector2 start, byte[,] region, byte[,] visited) {
        List<Vector2> component = new List<Vector2>();
        List<Vector2> toVisit = new List<Vector2>();

        toVisit.Add(start);
        while (toVisit.Count != 0) {
            Vector2 curr = toVisit[0];
            toVisit.RemoveAt(0);
            component.Add(curr);

            foreach (Vector2 nbr in NEIGHBORS_2D) {
                Vector2 currNbr = curr + nbr;

                // Check lower bounds
                if (currNbr.x < 0 || currNbr.y < 0) {
                    continue;
                }
                // Check upper bounds
                if (currNbr.x >= region.GetLength(0) || currNbr.y >= region.GetLength(1)) {
                    continue;
                }
                // If the node has already been visited, skip
                if (visited[(int) currNbr.x, (int) currNbr.y] != 0) {
                    continue;
                }

                // Mark the neighbor as visited
                visited[(int) currNbr.x, (int) currNbr.y] = 1;
                // If position is visible, add it to be visited in BFS
                if (region[(int) currNbr.x, (int) currNbr.y] != 0) {
                    toVisit.Add(currNbr);
                }
            }
        }
        return component;
    }
}
