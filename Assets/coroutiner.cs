using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class coroutiner : MonoBehaviour
{
    public static coroutiner This;

    void Awake() => This = this;

    public static Coroutine start(IEnumerator co) => This.StartCoroutine(co);
    public static void end(IEnumerator co) => This.StopCoroutine(co);
}
