using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph
{
    List<Vector2> vertices;
    Dictionary<Vector2, List<Edge>> edges;
    int n_edges = 0;

    public Graph(int width, int height) {
        vertices = new List<Vector2>();
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                vertices.Add(new Vector2(i, j));
            }
        }
        edges = new Dictionary<Vector2, List<Edge>>();
    }

    public void SetVertices(List<Vector2> v) {
        this.vertices = v;
    }

    public void AddEdge(Vector2 p1, Vector2 p2, Guard b1, Guard b2) {
        Edge newEdge = new Edge(p1, p2, b1, b2);
        if (!this.vertices.Contains(p1) || !this.vertices.Contains(p2)) {
            Debug.Log("Endpoint not found: " + p1 + ", " + p2);
            Debug.Log(this.vertices.ToString());
        }

        //Debug.Log("("+p1.x+","+p1.y+") - ("+p2.x+","+p2.y+")");
        
        if (this.edges.ContainsKey(p1)) {
            this.edges[p1].Add( new Edge(p1, p2, b1, b2) );
        } else {
            this.edges.Add(p1, new List<Edge> {new Edge(p1, p2, b1, b2) } );
        }
        n_edges++;
    }

    public int EdgesCount() {
        return n_edges;
    }

    public List<Guard> BFS() {
        // create v_src and v_sink which are placeholders for top and bottom
        Vector2 vSource = new Vector2(-1, -1);
        Vector2 vSink = new Vector2(-2, -2);

        this.vertices.Add(vSource);
        this.vertices.Add(vSink);
        // Connect vSource and vSink to top and bottom, resp.
        for (int i = 0; i < Terrain.T_SIZE; i++) {
            this.AddEdge(vSource, vertices[i], null, null);
            this.AddEdge(vertices[Terrain.T_SIZE * (Terrain.T_SIZE - 1) + i], vSink, null, null);
        }

        int[] visited = new int[vertices.Count];

        // pts to perform the BFS on
        List<Vector2> ptQ = new List<Vector2> {vSource};

        // cooresponding list of paths
        List<List<Guard>> pathQ = new List<List<Guard>> { new List<Guard>() };

        while (ptQ.Count != 0) {
            Vector2 currPt = ptQ[0];
            List<Guard> currPath = pathQ[0];
            ptQ.RemoveAt(0);
            pathQ.RemoveAt(0);

            Debug.Log("BFS (" + currPt.x + ","+currPt.y+")");

            if (!edges.ContainsKey(currPt)) {
                continue;
            }

            List<Edge> connectedPts = edges[currPt];
            foreach (Edge nbrEdge in connectedPts) {
                Vector2 nbr = nbrEdge.P2();
                if (nbr == vSink) {
                    return currPath;
                }

                // If the nbr is visited, skip
                if (visited[(int) (nbr.y * Terrain.T_SIZE + nbr.x)] == 1) {
                    continue;
                }

                ptQ.Add(nbr);
                visited[(int) (nbr.y * Terrain.T_SIZE + nbr.x)] = 1;
                var newPath = new List<Guard>(currPath);
                newPath.Add(nbrEdge.B1());
                newPath.Add(nbrEdge.B2());
                pathQ.Add(newPath);
            }
        }
        Debug.Log("No path found");
        return null;
    }

}
