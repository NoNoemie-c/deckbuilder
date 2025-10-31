using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class elecUI : MonoBehaviour
{
    public Vector2 startPos, endPos;
    private List<float> abcisses, heights;
    private lineGraphic line;
    private float timer;
    public float maxTime;
    public int min, max;
    public float coef, add;

    public void SetPos(Vector2 StartPos, Vector2 EndPos) {
        startPos = StartPos;
        endPos = EndPos;
    }
    
    void Start() {
        line = GetComponent<lineGraphic>();

        abcisses = new List<float>(){0};
        heights = new List<float>();
        for (int i = 1; i < Random.Range(min, max); i++) {
            abcisses.Add(Random.Range(0, 1f));
            heights.Add(0);
        }
        abcisses.RemoveAt(0);

        abcisses.Sort();

        timer = maxTime;
    }

    void FixedUpdate() {
        timer --;
        line.points = new List<Vector2>();
        line.points.Add(startPos);

        Vector2 current;
        for (int i = 0; i < abcisses.Count; i++) {
            current = new Vector2(Mathf.Lerp(startPos.x, endPos.x, abcisses[i]), Mathf.Lerp(startPos.y, endPos.y, abcisses[i]));
            current += Vector2.Perpendicular(endPos - startPos) * heights[i];
            heights[i] += heights[i] * Random.Range(-coef, coef * 2) + Random.Range(-add, add);
            heights[i] = Mathf.Clamp(heights[i], -.1f, .1f);
            line.points.Add(new Vector3(current.x, current.y, .5f));
        }

        line.points.Add(endPos);

        line.color = new Color(line.color.r, line.color.g, line.color.b, timer / maxTime);

        if (timer == 0)
            Destroy(gameObject);
    }
}
