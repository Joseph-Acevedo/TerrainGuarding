using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path
{
    Terrain terrain;
    List<Vector2> ptsTraversed;
    float sqrCost;                      // the total distance travelled, except each step is squared since
                                        // it is cheaper to compute sqrMagnitude (and gives abs)

    private Path(Terrain t, List<Vector2> pts, float sqrCost) {
        terrain = t;
        this.sqrCost = sqrCost;
        ptsTraversed = new List<Vector2>(pts);
    }

    public Path(Terrain t, Vector2 initialPoint) {
        terrain = t;
        sqrCost = 0f;
        ptsTraversed = new List<Vector2> {initialPoint};
    }

    public float AddVertex(Vector2 nextPt) {
        Vector2 last = ptsTraversed[ptsTraversed.Count - 1];
        Vector3 lastV = terrain.GetVertex((int) last.x, (int) last.y);
        Vector3 nextV = terrain.GetVertex((int) nextPt.x, (int) nextPt.y);
        ptsTraversed.Add(nextPt);

        sqrCost += (nextV - lastV).sqrMagnitude;
        return sqrCost;
    }

    public Vector2 GetCurrentPt() {
        return ptsTraversed[ptsTraversed.Count - 1];
    }

    public bool ReachedBottom() {
        return ptsTraversed[ptsTraversed.Count - 1].y == 0;
    }

    public float GetSqrCost() {
        return sqrCost;
    }

    public float CompareTo(Path p) {
        return this.sqrCost - p.GetSqrCost();
    }

    public Path Copy() {
        return new Path(terrain, ptsTraversed, sqrCost);
    }

    public List<Vector2> GetPathPoints() {
        return ptsTraversed;
    }

    
}
