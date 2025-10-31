using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class compTrail : MonoBehaviour
{
    public float time = 50, timer = 50;

    public void start() {
        GetComponent<component>().enabled = false;
    }

    void FixedUpdate() {
        foreach (Graphic g in GetComponentsInChildren<Graphic>())
            g.color = new Color(g.color.r, g.color.g, g.color.b, time / timer);

        time --;

        if (time <= 0)
            Destroy(gameObject);
    }
}
