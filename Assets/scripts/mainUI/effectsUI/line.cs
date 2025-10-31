using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class line : MonoBehaviour
{
    private LineRenderer Line;
    public float timer;
    public float val = .01f;
    private RectTransform r;

    void Start() {
        Line = GetComponent<LineRenderer>();
        disable();
    }

    public void SetPos(RectTransform r_) {
        Line.enabled = true;

        r = r_;
    }

    public void disable() {
        Line.enabled = false;
    }

    public void FixedUpdate() {
        timer += val;
        Line.endColor = new Color(Line.endColor.r, Line.endColor.g, Line.endColor.b, timer);
        Line.startColor = new Color(Line.startColor.r, Line.startColor.g, Line.startColor.b, timer);

        if (timer < .5f)
            timer = .5f;
        if (timer > 1)
            timer = 1;

        if (timer == .5f)
            val = .01f;
        if (timer == 1)
            val = -.01f;

        if (r == null)
            return;
            
        Vector3[] v = new Vector3[4];
        r.GetWorldCorners(v);
        Line.SetPositions(v);
    }
}
