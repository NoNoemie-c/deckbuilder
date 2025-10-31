using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

[ExecuteInEditMode]
public class squareClockTester : MonoBehaviour
{
    SpriteShapeController s;
    [Range(0, 1)] public float t;

    void Start() {
        s = GetComponent<SpriteShapeController>();
    }

    void Update() {
        s.squareClock(t, 2);
    }
}
