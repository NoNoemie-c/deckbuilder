using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Button), typeof(Image))]
public class hover : MonoBehaviour
{
    public UnityEvent Event;
    private Image i;

    void Start() {
        i = GetComponent<Image>();
    }

    void Update() {
        i.color = new Color(i.color.r, i.color.g, i.color.b, GetComponent<Button>().interactable ? 1 : .2f);
        if (GetComponent<Button>().interactable && Mouse.current.delta.ReadValue() != Vector2.zero && 
            GetComponent<RectTransform>().rect.Contains(transform.InverseTransformPoint(Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()))))
            Event.Invoke();
    }
}
