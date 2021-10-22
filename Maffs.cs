using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Maffs
{
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
}
