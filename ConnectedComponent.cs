using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectedComponent
{
    List<Vector2> points;
    Guard b_i, b_j;

    public ConnectedComponent(List<Vector2> points) {
        this.points = points;
    }

    public void SetGuards(Guard g1, Guard g2) {
        this.b_i = g1;
        this.b_j = g2;
    }

    public Guard B_i() {
        return b_i;
    }

    public Guard B_j() {
        return b_j;
    }
}
