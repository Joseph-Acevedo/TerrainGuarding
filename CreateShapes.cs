using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Creates mesh shapes
 */
public class CreateShapes
{
    private static int numShapes = 2;
    private static Mesh[] meshes = new Mesh[numShapes];

    private const float CUBE_VERTEX_PER_EDGE = 8f;

    public const int CUBE = 0;
    public const int SPHERE = 1;
    public const int TERRAIN = 2;
    

    public static Mesh GetShapeMesh(int shape, float vertexPerEdge = CUBE_VERTEX_PER_EDGE, float terrainScale = 1f) {
        
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        switch(shape) {
            case CUBE:
                CreateCube(ref vertices, ref triangles, vertexPerEdge);
                break;
            case SPHERE:
                CreateSphere(ref vertices, ref triangles, vertexPerEdge);
                break;
            case TERRAIN:
                CreateTerrain(ref vertices, ref triangles, vertexPerEdge, terrainScale);
                break;
            default:
                Debug.Log("Unknown shape");
                return null;
        }

        Mesh mesh = new Mesh();
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        //mesh.Optimize();
        mesh.RecalculateNormals();
        
        return mesh;
    }

    private static void CreateSphere(ref List<Vector3> outVertices, ref List<int> outTriangles, float vertexPerEdge) {
        
        List<Vector3> vertices = new List<Vector3>();
        CreateCube(ref vertices, ref outTriangles, vertexPerEdge);


        // ray cast cube into sphere, center = origin
        float maxSize = 0.0f;
        foreach (Vector3 v in vertices) {
            float a = Vector3.Dot(v.normalized, v.normalized);
            float b = 2 * Vector3.Dot(v.normalized, v.normalized);
            float c = Vector3.Dot(v.normalized, v.normalized) - 1f;

            float t = Maffs.Quad(a, b, c);
            Vector3 spherePoint = v.normalized + t*v.normalized;
            maxSize = Mathf.Max(maxSize, spherePoint.magnitude);
            outVertices.Add(spherePoint);
        }

        // The 0.5 is to scale it to 1.0f diameter
        outVertices = (from vertex in outVertices
                       select vertex * (-0.5f/maxSize)).ToList();
    }
    
    private static void CreateCube(ref List<Vector3> outVertices, ref List<int> outTriangles, float vertexPerEdge) {
        List<List<Vector3>> faces = new List<List<Vector3>>();

        // create the vertices on the front (<0,0,1> dir) face
        List<Vector3> frontFace = new List<Vector3>();
        for (int row = 0; row < vertexPerEdge; row++) {
            for (int col = 0; col < vertexPerEdge; col++) {
                float x = (col * 1f/(vertexPerEdge - 1)) - 0.5f;
                float y = (row * 1f/(vertexPerEdge - 1)) - 0.5f;
                frontFace.Add(new Vector3(x, y, 0.5f));
                // Debug.Log(string.Format("Adding ({0},{1})", x, y));
            }
        }

        // create the triangles for the front face
        List<int> faceTriangles = new List<int>();
        for (int idx = 0; idx < frontFace.Count; idx++) {
            int x = (int) (idx % vertexPerEdge);
            int y = Mathf.FloorToInt(idx / vertexPerEdge);

            // at the edge, no triangles to make
            if (x == vertexPerEdge - 1 || y == vertexPerEdge - 1) {
                continue;
            }
            
            //  T1: (x,y)   (x+1,y) (x,y+1)
            faceTriangles.Add(CoordToIndex(x  , y  , vertexPerEdge));
            faceTriangles.Add(CoordToIndex(x+1, y  , vertexPerEdge));
            faceTriangles.Add(CoordToIndex(x  , y+1, vertexPerEdge));

            //  T2: (x+1,y) (x,y+1) (x+1,y+1)
            faceTriangles.Add(CoordToIndex(x+1, y  , vertexPerEdge));
            faceTriangles.Add(CoordToIndex(x+1, y+1, vertexPerEdge));
            faceTriangles.Add(CoordToIndex(x  , y+1, vertexPerEdge));
   
        }

        Quaternion rotateUp = Quaternion.FromToRotation(Vector3.forward, Vector3.up);
        Quaternion rotateDown = Quaternion.FromToRotation(Vector3.forward, Vector3.down);
        Quaternion rotateLeft = Quaternion.FromToRotation(Vector3.forward, Vector3.left);
        Quaternion rotateRight = Quaternion.FromToRotation(Vector3.forward, Vector3.right);
        Quaternion rotateBack = Quaternion.FromToRotation(Vector3.forward, Vector3.back);

        Matrix4x4 Rx_Up = Matrix4x4.Rotate(rotateUp);
        Matrix4x4 Rx_Down = Matrix4x4.Rotate(rotateDown);
        Matrix4x4 Ry_Left = Matrix4x4.Rotate(rotateLeft);
        Matrix4x4 Ry_Right = Matrix4x4.Rotate(rotateRight);
        Matrix4x4 Rz_Back = Matrix4x4.Rotate(rotateBack);


        // copy front face for other 5 faces
        faces.Add(frontFace);                               // Front
        faces.Add(RotatePoints(frontFace, Rx_Up));          // Up
        faces.Add(RotatePoints(frontFace, Ry_Left));        // Left
        faces.Add(RotatePoints(frontFace, Rz_Back));        // Back
        faces.Add(RotatePoints(frontFace, Rx_Down));        // Down
        faces.Add(RotatePoints(frontFace, Ry_Right));       // Right

        List<Vector3> vertices = (from face in faces 
                                  from vertex in face
                                  select vertex).ToList();

        // Add the triangles to a single list, with them shifted for each face
        List<int> triangles = new List<int>();
        AddToShifted(faceTriangles, triangles, 0);
        AddToShifted(faceTriangles, triangles, 1*vertexPerEdge*vertexPerEdge);
        AddToShifted(faceTriangles, triangles, 2*vertexPerEdge*vertexPerEdge);
        AddToShifted(faceTriangles, triangles, 3*vertexPerEdge*vertexPerEdge);
        AddToShifted(faceTriangles, triangles, 4*vertexPerEdge*vertexPerEdge);
        AddToShifted(faceTriangles, triangles, 5*vertexPerEdge*vertexPerEdge);

        outVertices = vertices;
        outTriangles = triangles;

    }

    private static void CreateTerrain(ref List<Vector3> outVertices, ref List<int> outTriangles, float vertexPerEdge, float terrainScale) {
        // create the vertices on the front (<0,1,0> dir) face
        float terrainSeed = UnityEngine.Random.Range(0f, 10000f);
        List<Vector3> terrain = new List<Vector3>();
        for (int row = 0; row < vertexPerEdge; row++) {
            for (int col = 0; col < vertexPerEdge; col++) {
                float x = (col * (terrainScale/vertexPerEdge));// - (terrainScale/2);
                float z = (row * (terrainScale/vertexPerEdge));// - (terrainScale/2);
                float y = Mathf.PerlinNoise(x + terrainSeed, z + terrainSeed);
                terrain.Add(new Vector3(x, y, z));
            }
        }

        // create the triangles for the front face
        List<int> terrainTriangles = new List<int>();
        for (int idx = 0; idx < terrain.Count; idx++) {
            int x = (int) (idx % vertexPerEdge);
            int z = Mathf.FloorToInt(idx / vertexPerEdge);

            // at the edge, no triangles to make
            if (x == vertexPerEdge - 1 || z == vertexPerEdge - 1) {
                continue;
            }
            
            //  T1: (x,y)   (x+1,y) (x,y+1)
            terrainTriangles.Add(CoordToIndex(x  , z  , vertexPerEdge));
            terrainTriangles.Add(CoordToIndex(x  , z+1, vertexPerEdge));
            terrainTriangles.Add(CoordToIndex(x+1, z  , vertexPerEdge));

            //  T2: (x+1,y) (x,y+1) (x+1,y+1)
            terrainTriangles.Add(CoordToIndex(x+1, z  , vertexPerEdge));
            terrainTriangles.Add(CoordToIndex(x  , z+1, vertexPerEdge));
            terrainTriangles.Add(CoordToIndex(x+1, z+1, vertexPerEdge));
        }

        outVertices = terrain;
        outTriangles = terrainTriangles;
    }

    public static int CoordToIndex(int x, int z, float vertexPerEdge) {
        return x + (int) (z*vertexPerEdge);
    }

    // adds all elements of input to destination while adding shiftAmt to every element
    private static void AddToShifted(List<int> input, List<int> destination, float shiftAmt) {
        foreach (int i in input) {
            destination.Add(i + (int) shiftAmt);
        }
    }

    private static List<Vector3> RotatePoints(List<Vector3> points, Matrix4x4 rotationMatrix) {
        List<Vector3> rotatedPoints = new List<Vector3>();
        foreach (Vector3 point in points) {
            rotatedPoints.Add(rotationMatrix.MultiplyVector(point));
        }
        return rotatedPoints;
    }

}
