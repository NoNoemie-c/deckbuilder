using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class soundTester : MonoBehaviour
{
    public componentAnimation anim;

    void Update() {
        if (anim == null)
            return;

        StartCoroutine(anim.play(GetComponentInChildren<component>().transform, .5f));
        
        anim = null;
    }
}
