using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class expandEffect : MonoBehaviour
{
    public float timer;
    private float time;
    public AnimationCurve alphaOverTime = AnimationCurve.Linear(0, 1, 1, 0);
    public AnimationCurve sizeOverTime = AnimationCurve.Linear(0, 0, 1, 1);
    public AnimationCurve widthOverTime = AnimationCurve.Constant(0, 1, 5);
    public float sizeCoef;
    private lineGraphic line;

    void Awake() {
        line = GetComponent<lineGraphic>();
    }

    void FixedUpdate() {
        time ++;

        if (time >= timer)
            Destroy(gameObject);

        transform.localScale = Vector2.one * sizeCoef * sizeOverTime.Evaluate(time / timer);
        line.color = new Color(line.color.r, line.color.g, line.color.b, alphaOverTime.Evaluate(time / timer));
        line.thickness = widthOverTime.Evaluate(time / timer);
    }
}
