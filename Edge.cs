using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge
{
    Vector2 p1, p2;
    Guard b1, b2;

    public Edge(Vector2 p1, Vector2 p2, Guard b1, Guard b2) {
        this.p1 = p1;
        this.p2 = p2;
        this.b1 = b1;
        this.b2 = b2;
    }

    public Vector2 P1() {
        return this.p1;
    }

    public Vector2 P2() {
        return this.p2;
    }

    public Guard B1() {
        return this.b1;
    }

    public Guard B2() {
        return this.b2;
    }
}
