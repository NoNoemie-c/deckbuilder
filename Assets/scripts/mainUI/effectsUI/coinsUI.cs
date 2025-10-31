using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class coinsUI : MonoBehaviour
{
    [NonSerialized] public float timer;
    public float maxTime;
    public float waitTime;
    private TextMeshProUGUI text;

    void Start() {
        text = GetComponent<TextMeshProUGUI>();
        timer = maxTime;
    }

    void FixedUpdate() {
        timer --;
        /*if (Convert.ToInt32(text.text.Replace("<sprite=" + informationWindow.coinIconID + ">", " ").Replace("<sprite=" + informationWindow.crankIconID + ">", " ").Replace('x', ' ')) < 0) 
            transform.localScale *= 1.005f;
        else
            transform.localScale *= 1.01f;*/

        text.color = new Color(text.color.r, text.color.g, text.color.b, timer / (maxTime - waitTime));

        if (timer == 0)
            Destroy(gameObject);
    }
}
