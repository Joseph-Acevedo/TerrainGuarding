using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engine : MonoBehaviour
{
    // Top/Bottom is on Z-Axis
    // East/West is on X-Axis
    public bool checkForPath = false;
    bool pathChecked = false;
    public bool generateGuards = false;
    bool guardsGend = false;

    byte UNGUARDED = 0;
    byte GUARDED = 1;
    byte CHECKED = 2;
    float VIS_OFFSET = 0.01f;             // The vertical offset for the vertices so that they're visible above the terrain
    Vector2[] NEIGHBORS = {
        Vector2.up,
        Vector2.up   +  Vector2.left,
                        Vector2.left,
        Vector2.down +  Vector2.left,
        Vector2.down,
        Vector2.down +  Vector2.right,
                        Vector2.right,
        Vector2.up   +  Vector2.right
    };




    Terrain terrain;
    GameObject[,] vertices;
    byte[,] visibility;
    List<GameObject> guardObjects;
    Path minPath;
    List<Guard> guards;


    // Start is called before the first frame update
    void Start()
    {
        
        this.gameObject.transform.position = Vector3.zero;
        terrain = this.gameObject.AddComponent<Terrain>();
        vertices = new GameObject[Terrain.T_SIZE, Terrain.T_SIZE];
        
        
        InstantiateVisualization();
        /*
        List<Vector2> guardPos = BFS_PickGuards(3);
        if (guardPos == null) {
            print("Returned no guards");
        }
        List<Guard> guards = new List<Guard>();
        foreach (Vector2 pos in guardPos) {
            guards.Add(new Guard((int) pos.x, (int) pos.y, this.terrain));
        }

        VisualizeGuards(guards);
        VisualizeVisibility(GenerateCombinedVisibility(guards));
        */
        RegionBasedOverlap();
    }

    void Update() {
        if (checkForPath && !pathChecked) {
            pathChecked = true;

            var t_start = Time.realtimeSinceStartup;
            minPath = FindMinPathAcrossTerrain(terrain);
            print("Time for min path finding: " + (Time.realtimeSinceStartup - t_start).ToString("f6") + "ms");
            VisualizePath(minPath);
        }

        if (pathChecked && generateGuards && !guardsGend) {
            guardsGend = true;
            var t_start = Time.realtimeSinceStartup;
            guards = GenerateGuards(minPath, terrain);
            print("Time for placing guards: " + (Time.realtimeSinceStartup - t_start).ToString("f6") + "ms");
            print("Guards used: " + guards.Count);
            VisualizeGuards(guards);
            VisualizeVisibility(GenerateCombinedVisibility(guards));
            VisualizePath(minPath);
        }
    }

    void InstantiateVisualization() {
        GameObject vertex = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        vertex.transform.localScale = 0.05f*Vector3.one;
        var renderer = vertex.GetComponent<Renderer>();
        //Call SetColor using the shader property name "_Color" and setting the color to red
        renderer.material.SetColor("_Color", Color.red);

        for (int x = 0; x < Terrain.T_SIZE; x++) {
            for (int z = 0; z < Terrain.T_SIZE; z++) {
                vertices[x, z] = Instantiate(vertex, terrain.GetVertex(x, z) + new Vector3(0, VIS_OFFSET, 0), Quaternion.identity);
            }
        }
    }

    void VisualizeGuards(List<Guard> guards) {
        guardObjects = new List<GameObject>();
        
        GameObject template = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        template.transform.localScale = 0.125f * Vector3.one;
        var cubeRenderer = template.GetComponent<Renderer>();
        //Call SetColor using the shader property name "_Color" and setting the color to red
        cubeRenderer.material.SetColor("_Color", Color.white);

        foreach (Guard g in guards) {
            guardObjects.Add(Instantiate(template, g.GetPos(), Quaternion.identity));
        }
    } 

    /*
     * Places a ball on the terrain for every vertex, coloring it green if it is marked
     * as visible in the given byte-map, and red if not.
     *
     * TODO: The original sphere needs to be moved or deleted afterwards to avoid having random
     *  points off the terrain
     */
    void VisualizeVisibility(byte[,] vis) {
        GameObject visibleVertex = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visibleVertex.transform.localScale = 0.05f*Vector3.one;
        var cubeRenderer = visibleVertex.GetComponent<Renderer>();
        //Call SetColor using the shader property name "_Color" and setting the color to red
        cubeRenderer.material.SetColor("_Color", Color.green);

        GameObject invisibleVertex = Instantiate(visibleVertex, Vector3.down, Quaternion.identity);
        cubeRenderer = invisibleVertex.GetComponent<Renderer>();
        //Call SetColor using the shader property name "_Color" and setting the color to red
        cubeRenderer.material.SetColor("_Color", Color.red);


        for (int x = 0; x < vis.GetLength(0); x++) {
            for (int z = 0; z < vis.GetLength(1); z++) {
                if (vis[x,z] == GUARDED) {
                    var vertex = vertices[x, z];
                    var renderer = vertex.GetComponent<Renderer>();
                    renderer.material.SetColor("_Color", Color.green);
                    //vertices[x, z] = Instantiate(visibleVertex, terrain.GetVertex(x, z) + new Vector3(0, VIS_OFFSET, 0), Quaternion.identity);
                }
            }
        }
    }

    /*
     * Given a set of points representing a path, this will change the color of each vertex that the
     * points correspond to to yellow to show the path
     *
     * TODO: The original sphere needs to be moved or deleted afterwards to avoid having random
     *  points off the terrain
     */
    void VisualizePath(Path path) {

        GameObject pathVertex = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pathVertex.transform.localScale = 0.1f*Vector3.one;
        var cubeRenderer = pathVertex.GetComponent<Renderer>();
        //Call SetColor using the shader property name "_Color" and setting the color to red
        cubeRenderer.material.SetColor("_Color", Color.yellow);

        foreach (Vector2 v in path.GetPathPoints()) {
            int x = (int) v.x;
            int z = (int) v.y;
            var renderer = vertices[x, z].GetComponent<Renderer>();// = Instantiate(pathVertex, terrain.GetVertex(x, z) + new Vector3(0, VIS_OFFSET, 0), Quaternion.identity);
            renderer.material.SetColor("_Color", Color.yellow);
            vertices[x, z].transform.localScale = 0.1f * Vector3.one;
            
        }
    }

    /*
     * Determines if there is a continuous path from the top to the bottom that is entirely visible
     * by the set of guards. Rather than passing in the set of guards this will take in a byte-map of
     * the visibility of all of the guards. This should be generated by passing your list of guards into
     * 'GenerateCombinedVisibility(Guard[] guards)'.
     *
     * If there exists a path, then this will return the minimum length path (determined by number of steps)
     * and if not, then it will return null
     */
    List<Vector2> CheckGuardPositions(List<Guard> guards) {
        byte[,] vis = GenerateCombinedVisibility(guards);
        List<List<Vector2>> paths = new List<List<Vector2>>();
        int zTop = Terrain.T_SIZE - 1;

        for (int x = 0; x < Terrain.T_SIZE; x++) {
            if (vis[x, zTop] == GUARDED) {
                // If a point on the top is visible, add it as a starting pt for a path
                paths.Add(new List<Vector2> { new Vector2(x, zTop) });
                vis[x, zTop] = CHECKED;
            }
        }

        while (paths.Count > 0) {
            paths.Sort( (a, b) => a.Count.CompareTo(b.Count) );

            List<Vector2> minPath = paths[0];
            Vector2 lastVertex = minPath[minPath.Count - 1];
            if (lastVertex.y == 0) {
                // We've reached the bottom. Since cost is Count then we don't need to
                // empty the rest of the queue
                return minPath;
            }

            foreach (Vector2 n in NEIGHBORS) {
                Vector2 next = n + lastVertex;
                if (IsValidPoint(next, vis, true)) {
                    // Add to list (create new list)
                    print("Adding point to path");
                    List<Vector2> newPath = new List<Vector2>(minPath);
                    newPath.Add(next);
                    paths.Add(newPath);
                    vis[(int) next.x, (int) next.y] = CHECKED;
                }
            }
            paths.Remove(minPath);
        }

        // We've searched all paths, no path found
        return null;
    }

    bool IsValidPoint(Vector2 pt, byte[,] vis, bool ensureGuarded) {
        if (pt.x < 0 || pt.y < 0)
            return false;
        if (pt.x >= Terrain.T_SIZE || pt.y >= Terrain.T_SIZE)
            return false;
        // Check if the point has already been seen, regardless of if it must be guarded
        if (vis[(int) pt.x, (int) pt.y] == CHECKED)
            return false;
        if (ensureGuarded && (vis[(int) pt.x, (int) pt.y] != GUARDED))
            return false;

        return true;
    }

    /*
     * Finds the minimum distance path from the top to the bottom of the terrain. This doesn't
     * take into account visibility of the vertices because we use this path to generate guard 
     * positions
     * 
     * TODO: CheckGuardPositions and this method do extremely similar things and should be
     *  merged into a single more general function, however I can't be bothered to refactor
     *  my code right now
     */
    Path FindMinPathAcrossTerrain(Terrain t) {
        // Used to keep track of which vertices we've checked
        byte[,] vTrack = new byte[Terrain.T_SIZE, Terrain.T_SIZE];

        List<Path> paths = new List<Path>();
        int zTop = Terrain.T_SIZE - 1;

        for (int x = 0; x < Terrain.T_SIZE; x++) {
            // Add all top points as possible starting positions
            paths.Add(new Path(t, new Vector2(x, zTop)));
            vTrack[x, zTop] = CHECKED;
        }

        while (paths.Count > 0) {
            paths.Sort( (a, b) => (int) a.CompareTo(b) );

            Path minPath = paths[0];
            if (minPath.ReachedBottom()) {
                // We've reached the bottom. Since cost is Count then we don't need to
                // empty the rest of the queue
                return minPath;
            }

            Vector2 currPt = minPath.GetCurrentPt();
            foreach (Vector2 n in NEIGHBORS) {
                Vector2 next = n + currPt;

                // We don't need to ensure that a vertex is guarded, jsut that its in bounds
                if (IsValidPoint(next, vTrack, false)) {
                    // Add to list (create new list)
                    Path newPath = minPath.Copy();
                    newPath.AddVertex(next);
                    paths.Add(newPath);
                    vTrack[(int) next.x, (int) next.y] = CHECKED;
                }
            }
            paths.Remove(minPath);
        }

        // We've searched all paths, no path found
        return null;
    }

    List<Guard> GenerateGuards(Path minPath, Terrain t) {
        List<Vector2> pathPts = minPath.GetPathPoints();

        List<Guard> guards = new List<Guard>();
        guards.Add(new Guard(pathPts[0], t));
        // guards.Add(new Guard(pathPts[pathPts.Count - 1], t));

        // [1..Count-1] because we've already used first and last pt
        int count = 0;
        Guard prev = guards[0];
        while (CheckGuardPositions(guards) == null) {
            // TODO: Could optimize this by saving previously generated guards
            for (int idx = pathPts.Count - 1; idx >= 0; idx--) {
                Guard tempGuard = new Guard(pathPts[idx], t);
                if (prev.IntersectVis(tempGuard) != null) {
                    guards.Add(tempGuard);
                    prev = tempGuard;
                    count++;
                    break;
                }
            }
        }

        return guards;
    }

    /* 
     * Combines the visibility of all the guards into a single map where a pixel 
     * value shows if a vertex is visible by at least one guard or not
     */
    byte[,] GenerateCombinedVisibility(List<Guard> guards) {
        byte[,] vis = new byte[Terrain.T_SIZE, Terrain.T_SIZE];

        if (guards == null || guards.Count == 0) {
            Debug.Log("No guards given");
        }

        int numGuardedPts = 0;
        for (int x = 0; x < Terrain.T_SIZE; x++) {
            for (int z = 0; z < Terrain.T_SIZE; z++) {
                foreach (Guard g in guards) {
                    // Only need one guard to see the vertex
                    if (g.GetVisibility()[x, z] != UNGUARDED) {
                        vis[x, z] = GUARDED;
                        numGuardedPts++;
                        break;
                    }
                }
            }
        }
        return vis;
    }

    List<Vector2> BFS_PickGuards(int spacesBtwn) {
        // tracks if this alg has visited that node
        byte[,] vis = new byte[Terrain.T_SIZE, Terrain.T_SIZE];
        List<List<Vector2>> paths = new List<List<Vector2>>();

        for (int x = 0; x < Terrain.T_SIZE; x += spacesBtwn) {
            // Add the candidate guards on the bottom to the list of path starts
            paths.Add(new List<Vector2> { new Vector2(x, 0) });
            vis[x, 0] = CHECKED;
        }

        List<Vector2> overallMin = null;
        while (paths.Count > 0) {
            paths.Sort( (a, b) => a.Count.CompareTo(b.Count) );

            List<Vector2> minPath = paths[0];
            Vector2 lastVertex = minPath[minPath.Count - 1];
            if (lastVertex.y == (Terrain.T_SIZE - 1)) {
                // We've reached the bottom. Since cost is Count then we don't need to
                // empty the rest of the queue
                if (overallMin == null || overallMin.Count > minPath.Count)
                    overallMin = minPath;
            }

            foreach (Vector2 n in NEIGHBORS) {
                Vector2 next = (n*spacesBtwn) + lastVertex;
                if (IsValidPoint(next, vis, false)) {
                    // Add to list (create new list)
                    List<Vector2> newPath = new List<Vector2>(minPath);
                    newPath.Add(next);
                    paths.Add(newPath);
                    vis[(int) next.x, (int) next.y] = CHECKED;
                }
            }
            paths.Remove(minPath);
        }

        // We've searched all paths, no path found
        return overallMin;
    }

    List<Guard> BFS_GenerateCandGuards(int spacesBtwn) {
        List<Guard> candidateGuards = new List<Guard>();
        for (int x = 0; x < Terrain.T_SIZE; x += spacesBtwn) {
            for (int z = 0; z < Terrain.T_SIZE; z += spacesBtwn) {
                candidateGuards.Add(new Guard(x, z, this.terrain));
            }
        }
        return candidateGuards;
    }

    void AddEdgesWithinComponent(Graph g, List<Vector2> component, Guard b1, Guard b2) {
        foreach (Vector2 v1 in component) {
            foreach (Vector2 v2 in component) {
                if (v1 == v2) {
                    continue;
                }

                g.AddEdge(v1, v2, b1, b2);
            }
        }
    }

    void RegionBasedOverlap() {
        Graph Gbar = new Graph(Terrain.T_SIZE, Terrain.T_SIZE);
        List<Guard> candidates = BFS_GenerateCandGuards(3);

        // TODO: To view all candidate guards
        // VisualizeGuards(candidates);

        List<ConnectedComponent> S = new List<ConnectedComponent>();
        for (int i = 0; i < candidates.Count; i++) {
            for (int j = 0; j < candidates.Count; j++) {
                if (i == j) {
                    continue;
                }

                Guard bi = candidates[i];
                Guard bj = candidates[j];

                // Compute Vis(g_i) U Vis(g_j)
                byte[,] bi_U_bj = bi.UnionVis(bj);

                // VisualizeVisibility(bi_U_bj);

                // Add all connected components
                var components = Maffs.GetConnectedComponents(bi_U_bj);
                foreach (List<Vector2> cmptPts in components) {
                    ConnectedComponent cmpt = new ConnectedComponent(cmptPts);
                    cmpt.SetGuards(bi, bj);
                    S.Add(cmpt);
                    AddEdgesWithinComponent(Gbar, cmptPts, bi, bj);
                }
            }
        }

        Debug.Log("Edges: " + Gbar.EdgesCount());
        List<Guard> minSet = Gbar.BFS();
        for (int i = 0; i < minSet.Count; ) {
            if (minSet[i] == null) {
                minSet.RemoveAt(i);
            } else {
                print(minSet[i].ToString() + "->");
                i++;
            }
        }

        VisualizeGuards(minSet);
        VisualizeVisibility(GenerateCombinedVisibility(minSet));
        
    }

}
