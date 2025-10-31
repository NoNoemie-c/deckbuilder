using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[ExecuteAlways] [RequireComponent(typeof(Slider))]
public class numSlider : MonoBehaviour
{
    public Slider s;
    public TextMeshProUGUI num;

    public float multiplier;

    void Start() {
        s = GetComponent<Slider>();
        num = GetComponentsInChildren<TextMeshProUGUI>()[1];
        s.onValueChanged.AddListener(changeTxt);
        changeTxt(s.value);
    }

    void changeTxt(float f) {
        num.text = (f * multiplier).ToString();
    }
}
