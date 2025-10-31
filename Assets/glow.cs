using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class glow : MonoBehaviour
{
    public bool active;
    public float speed = .01f, pos;

    void FixedUpdate() {
        if (GetComponent<Image>().enabled) {
            pos += speed;
            if (active) {
                if ((pos - 1 > .001f && speed > 0) || (pos - .7f < .001f && speed < 0))
                    speed = -speed;
            } else
                pos = Mathf.Lerp(.5f, pos, .9f);

            transform.localScale = (pos, pos).v();
        }
    }
}
